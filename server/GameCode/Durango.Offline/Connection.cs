using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Durango.Network;
using Durango.Utils;
using Snappier;

namespace Durango.Offline;

public class Connection
{
	public delegate void MessageHandler<T>(T msg, PacketHeader header);

	public delegate void PacketHandler(PacketHeader header, byte[] payload = null, object msg = null);

	// H-3: เดิม 2 MB × 8 บัฟเฟอร์ = ~16 MB **ต่อ connection ตั้งแต่ตอน accept**
	// เปิด TCP ค้างไว้ 200 เส้นก็ทำ RAM หมดได้ทั้งที่ยังไม่ Auth เลยสักเส้น
	// packet ที่ใหญ่ที่สุดที่เราส่งจริงคือ chunk/รายการสิ่งปลูกสร้าง ซึ่งอยู่ระดับสิบ KB
	private const int BufferCapacity = 524288;

	private readonly MessagePacking _messagePacker = new MessagePacking();

	private bool _willBeClosed;

	private bool _prevConnected;

	private volatile bool _receiveCompleted;

	private volatile bool _sendCompleted = true;

	private int _sendBufferSize;
	private byte[] _sendingBuffer;
	private int _sendingOffset;
	private int _sendingSize;
	private readonly object _sendLock = new object();
	private bool _closed;

	private readonly byte[] _sendBuffer1 = new byte[BufferCapacity];

	private readonly byte[] _sendBuffer2 = new byte[BufferCapacity];

	private int _sendBufferIndex;

	private readonly byte[] _receiveBuffer = new byte[BufferCapacity];

	private readonly byte[] _packingBuffer = new byte[BufferCapacity];

	private readonly byte[] _compressingBuffer = new byte[Snappy.GetMaxCompressedLength(BufferCapacity)];

	private readonly byte[] _decompressingBuffer = new byte[BufferCapacity];

	private readonly byte[] _receivedBuffer = new byte[BufferCapacity];

	private readonly byte[] _remainingBuffer = new byte[BufferCapacity];

	private int _receivedSize;

	private Socket _sock;

	private SocketAsyncEventArgs _socketEventArg;

	private SocketAsyncEventArgs _socketReceiveEventArg;

	private uint _sequenceNumber = 1u;

	private readonly Dictionary<uint, PacketHandler> _packetHandlers = new Dictionary<uint, PacketHandler>();

	private readonly Queue<Packet> _packetQueue = new Queue<Packet>();
	private int _queuedPayloadBytes;
	private const int MaxQueuedPackets = 2048;
	private const int MaxQueuedPayloadBytes = 4 * 1024 * 1024;
	private long _totalReceivedPackets;

	/// <summary>M-6: จำนวน packet ที่รับมาทั้งหมดตั้งแต่เปิด connection (ใช้ทำ rate limit ฝั่ง server)</summary>
	public int TotalReceivedPackets => (int)Math.Min(int.MaxValue, Interlocked.Read(ref _totalReceivedPackets));

	private byte[] SendBuffer => (_sendBufferIndex != 0) ? _sendBuffer2 : _sendBuffer1;

	public event Action ConnetionClosed;

	public Connection(Socket socket)
	{
		_sock = socket;
		_sock.NoDelay = true;
		_socketEventArg = new SocketAsyncEventArgs();
		_socketReceiveEventArg = new SocketAsyncEventArgs();
		_socketEventArg.Completed += SocketEventCompleted;
		_socketEventArg.UserToken = this;
		_socketReceiveEventArg.SetBuffer(_receiveBuffer, 0, BufferCapacity);
		_socketReceiveEventArg.Completed += SocketEventCompleted;
		_socketReceiveEventArg.UserToken = this;
	}

	public void Close()
	{
		Socket socket;
		Action closed;
		lock (_sendLock)
		{
			if (_closed)
			{
				return;
			}
			_closed = true;
			socket = _sock;
			_sock = null;
			_sendBufferSize = 0;
			_sendCompleted = true;
			_willBeClosed = true;
		}
		try
		{
			if (socket != null)
			{
				try { socket.Shutdown(SocketShutdown.Both); } catch (Exception) { }
				socket.Close();
			}
		}
		finally
		{
			lock (_packetQueue)
			{
				_packetQueue.Clear();
				_queuedPayloadBytes = 0;
			}
			_receiveCompleted = false;
			_prevConnected = false;
			closed = ConnetionClosed;
			if (closed != null)
			{
				try { closed(); } catch (Exception exception) { Debug.LogException(exception); }
			}
		}
	}

	public bool Connected()
	{
		Socket socket = _sock;
		return !_closed && socket != null && socket.Connected;
	}

	public bool Send<T>(T msg, uint replyOf = 0u)
	{
		lock (_sendLock)
		{
			if (_closed || _sock == null || !_sock.Connected)
			{
				return false;
			}
			uint seq = _sequenceNumber++;
			try
			{
				int num = Packet.SerializeMsg(Times.UnixTimeNow(), seq, replyOf, msg, SendBuffer, _sendBufferSize, _packingBuffer, _compressingBuffer, _messagePacker);
				// [3 ก.ย. 2026] num <= 0 = แพ็กไม่สำเร็จ (message ไม่ได้ลงทะเบียน TypeCode — บั๊กฝั่งเซิร์ฟ
				//   ไม่ใช่ปัญหาของ connection) ⇒ **ข้ามข้อความนี้ไปเฉย ๆ ห้ามปิด connection**
				//   เดิมโยน exception → _willBeClosed=true ⇒ คนที่อยู่ใกล้จุด broadcast หลุดยกแผง
				//   (เจอจริง: natural regrowth ส่ง AppearEntityOnTile ที่ไม่มี TypeCode 1769 ครั้ง)
				if (num <= 0)
				{
					Debug.LogError("[conn] ข้ามข้อความที่แพ็กไม่ได้ (ไม่ปิด connection): " + typeof(T).Name);
					return false;
				}
				if (num > BufferCapacity - _sendBufferSize)
				{
					// บัฟเฟอร์ส่งเต็มจริง ๆ = ปัญหาของ connection นี้ ปิดได้
					throw new InvalidDataException($"Outbound packet exceeds buffer capacity: {num} bytes");
				}
				_sendBufferSize += num;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				_willBeClosed = true;
				return false;
			}
			return true;
		}
	}

	public bool Recv<T>(MessageHandler<T> handler)
	{
		return RegisterMessageHandlerToRegistry(_packetHandlers, handler);
	}

	// ลงทะเบียน handler ตาม TypeCode ของ message
	// บาง message (เช่น Say) ไม่มี field TypeCode → ลง key 0 พร้อม warning (แทนที่จะ crash)
	private bool RegisterMessageHandlerToRegistry<T>(Dictionary<uint, PacketHandler> registry, MessageHandler<T> handler)
	{
		System.Reflection.FieldInfo fieldInfo = typeof(T).GetField("TypeCode", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
		if (fieldInfo == null || !fieldInfo.IsLiteral)
		{
			Console.WriteLine("[connection] FATAL: {0} has no TypeCode — handler NOT registered (key 0 ทับกันได้)", typeof(T).FullName);
			return false;
		}
		uint key = (uint)fieldInfo.GetValue(null);
		bool flag = registry.ContainsKey(key);
		if (flag)
		{
			registry.Remove(key);
		}
		registry.Add(key, delegate(PacketHeader header, byte[] payload, object msg)
		{
			T msg2 = ((payload == null) ? ((T)msg) : MakeMsg<T>(payload, 0, header.PayloadSize));
			handler(msg2, header);
		});
		_messagePacker.RegisterHandler<T>(null);
		return flag;
	}

	private void StartSend()
	{
		Socket socket;
		SocketAsyncEventArgs args;
		lock (_sendLock)
		{
			if (_closed || !_sendCompleted)
			{
				return;
			}
			socket = _sock;
			args = _socketEventArg;
			if (socket == null || args == null || !socket.Connected)
			{
				_willBeClosed = true;
				return;
			}
			if (_sendingSize == 0)
			{
				if (_sendBufferSize <= 0)
				{
					return;
				}
				_sendingBuffer = SendBuffer;
				_sendingOffset = 0;
				_sendingSize = _sendBufferSize;
				_sendBufferIndex = (_sendBufferIndex + 1) % 2;
				_sendBufferSize = 0;
			}
			_sendCompleted = false;
			args.SetBuffer(_sendingBuffer, _sendingOffset, _sendingSize);
		}
		try
		{
			if (!socket.SendAsync(args))
			{
				SendCompleted(args);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			_willBeClosed = true;
			_sendCompleted = true;
		}
	}

	public void StartReceive()
	{
		_receiveCompleted = false;
		if (_sock != null && _sock.Connected && !_sock.ReceiveAsync(_socketReceiveEventArg))
		{
			ReceiveCompleted(_socketReceiveEventArg);
		}
	}

	private T MakeMsg<T>(byte[] payload, int payloadOffset, int payloadSize)
	{
		return Packet.DeserializeMsg<T>(payload, payloadOffset, payloadSize, _decompressingBuffer, _messagePacker);
	}

	private static void SocketEventCompleted(object sender, SocketAsyncEventArgs e)
	{
		Connection connection = (Connection)e.UserToken;
		switch (e.LastOperation)
		{
		case SocketAsyncOperation.Receive:
			connection.ReceiveCompleted(e);
			break;
		case SocketAsyncOperation.Send:
			connection.SendCompleted(e);
			break;
		}
	}

	private void SendCompleted(SocketAsyncEventArgs e)
	{
		lock (_sendLock)
		{
			if (e.SocketError != SocketError.Success || e.BytesTransferred <= 0)
			{
				_willBeClosed = true;
				_sendingSize = 0;
				_sendingOffset = 0;
				_sendCompleted = true;
				return;
			}
			if (e.BytesTransferred < _sendingSize)
			{
				_sendingOffset += e.BytesTransferred;
				_sendingSize -= e.BytesTransferred;
				_sendCompleted = true;
				return;
			}
			_sendingBuffer = null;
			_sendingOffset = 0;
			_sendingSize = 0;
			_sendCompleted = true;
		}
	}

	private void ReceiveCompleted(SocketAsyncEventArgs e)
	{
		if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
		{
			try
			{
				ReceiveProcess(e);
				_receiveCompleted = true;
				return;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				_willBeClosed = true;
				return;
			}
		}
		if (e.SocketError != SocketError.Success)
		{
		}
		_willBeClosed = true;
	}

	private void ReceiveProcess(SocketAsyncEventArgs e)
	{
		if (e.BytesTransferred <= 0 || e.BytesTransferred > BufferCapacity - _receivedSize)
		{
			_willBeClosed = true;
			return;
		}
		Buffer.BlockCopy(e.Buffer, e.Offset, _receivedBuffer, _receivedSize, e.BytesTransferred);
		_receivedSize += e.BytesTransferred;
		int num = 0;
		int num2 = _receivedSize;
		while (num2 > 0)
		{
			if (!Packet.TryReadPacketHeader(_receivedBuffer, num2, num, out PacketHeader header, out bool incomplete))
			{
				if (!incomplete)
				{
					_willBeClosed = true;
				}
				break;
			}
			int num3 = Packet.HeaderSize + header.PayloadSize;
			if (num3 > num2)
			{
				break;
			}
			lock (_packetQueue)
			{
				if (_packetQueue.Count >= MaxQueuedPackets || _queuedPayloadBytes > MaxQueuedPayloadBytes - header.PayloadSize)
				{
					_willBeClosed = true;
					break;
				}
				byte[] array = new byte[header.PayloadSize];
				Buffer.BlockCopy(_receivedBuffer, num + header.Size, array, 0, header.PayloadSize);
				_packetQueue.Enqueue(new Packet { Header = header, Payload = array });
				_queuedPayloadBytes += array.Length;
			}
			Interlocked.Increment(ref _totalReceivedPackets);
			num += num3;
			num2 -= num3;
		}
		if (num2 > 0 && num2 != _receivedSize)
		{
			Buffer.BlockCopy(_receivedBuffer, num, _remainingBuffer, 0, num2);
			Buffer.BlockCopy(_remainingBuffer, 0, _receivedBuffer, 0, num2);
		}
		_receivedSize = num2;
	}

	public void Process()
	{
		if (_packetQueue.Count > 0)
		{
			ProcessPacketQueue();
		}
		CheckSocketClosed();
		if (_receiveCompleted)
		{
			try
			{
				StartReceive();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				Close();
			}
		}
		if (_sendCompleted && (_sendBufferSize > 0 || _sendingSize > 0))
		{
			try
			{
				StartSend();
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
				Close();
			}
		}
	}

	// GP-01: เดิมโค้ดนี้ดึง packet ออกมาแค่ "1 ตัวต่อการเรียก 1 ครั้ง" ทั้งที่ฝั่ง client
	// (client/Durango.Network/Connection.cs) ระบายทั้งคิวด้วย while — โค้ดสองตัวนี้เป็นแฝดกัน
	// ตัวนี้หาย while ไป จึงเป็นบั๊กไม่ใช่การออกแบบ
	// ผลเดิม: main loop ~64 รอบ/วินาที = เพดาน ~64 packet/วินาที/ผู้เล่น → ดีเลย์สะสมไล่ไม่ทัน
	private const int MaxPacketsPerTick = 512;

	/// <summary>
	/// นับ message ที่ client ส่งมาแต่เซิร์ฟไม่มี handler — แชร์ทุก connection
	/// ใช้ตอบคำถาม "เซิร์ฟเราทำงานครบตามที่ตัวเกมต้องการไหม" โดยดูจากของจริงที่เกมส่ง
	/// ไม่ใช่เดาจากโค้ด — ดูสรุปได้ที่ endpoint /admin/status
	/// </summary>
	public static readonly Dictionary<uint, int> UnhandledCounts = new Dictionary<uint, int>();

	/// <summary>
	/// [4 ก.ย. 2026] ตัวรับ "แพ็กเก็ตที่ไม่มี handler" — ServerPlayer ใช้ตอบ Abort กลับไปตาม Seq
	/// เพื่อไม่ให้ client ค้างรอคำตอบ (เดิมแค่ log แล้วทิ้ง ⇒ ปุ่มในเกมกดแล้วเงียบ/ค้าง)
	/// </summary>
	public Action<PacketHeader, byte[]> OnUnhandled;

	/// <summary>มี handler สำหรับ TypeCode นี้แล้วหรือยัง — ใช้กัน fallback ทับของจริง</summary>
	public bool HasHandler(uint typeCode)
	{
		return _packetHandlers.ContainsKey(typeCode);
	}

	private void ProcessPacketQueue()
	{
		int processed = 0;
		while (processed < MaxPacketsPerTick)
		{
			Packet packet;
			// dequeue ในล็อก แต่เรียก handler นอกล็อก — handler ที่ทำงานนานจะได้ไม่บล็อก
			// thread pool ที่กำลัง enqueue packet ใหม่เข้ามา
			lock (_packetQueue)
			{
				if (_packetQueue.Count == 0)
				{
					break;
				}
				packet = _packetQueue.Dequeue();
				_queuedPayloadBytes -= packet.Payload?.Length ?? 0;
			}
			processed++;
			try
			{
				if (!_packetHandlers.TryGetValue(packet.Header.TypeCode, out var value) || value == null)
				{
					// [แก้เอง] 31 ส.ค. 2026 — เดิมพิมพ์ทุกครั้งที่เจอ ⇒ ข้อความที่ client ส่งถี่ ๆ
					// (Depart 587 ครั้ง/ชม.) ท่วม log จนหาปัญหาจริงไม่เจอ
					// ตอนนี้พิมพ์ครั้งแรกครั้งเดียวต่อชนิด พร้อมนับรวมไว้ให้เรียกดูทีหลังได้
					lock (UnhandledCounts)
					{
						UnhandledCounts.TryGetValue(packet.Header.TypeCode, out int seen);
						UnhandledCounts[packet.Header.TypeCode] = seen + 1;
						if (seen == 0)
						{
							Console.WriteLine("[conn] ไม่มี handler สำหรับ type={0} (bytes={1}) — จะไม่เตือนซ้ำอีก",
								packet.Header.TypeCode, packet.Payload?.Length ?? 0);
						}
					}
					OnUnhandled?.Invoke(packet.Header, packet.Payload);
				}
				else
				{
					value.Invoke(packet.Header, packet.Payload);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		if (processed >= MaxPacketsPerTick)
		{
			// ชนเพดานแล้ว — ที่เหลือค้างไว้รอบหน้า ไม่ตัดทิ้งเงียบ ๆ
			int remain;
			lock (_packetQueue)
			{
				remain = _packetQueue.Count;
			}
			Debug.LogWarning($"[conn] ชนเพดาน {MaxPacketsPerTick} packet/tick, ค้างอีก {remain} ตัว");
		}
	}

	private void CheckSocketClosed()
	{
		if (_willBeClosed || (_prevConnected && _sock != null && !_sock.Connected))
		{
			Close();
		}
	}
}
