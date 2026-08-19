using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Durango.Network;
using Durango.Utils;
using Snappy;

namespace Durango.Offline;

public class Connection
{
	public delegate void MessageHandler<T>(T msg, PacketHeader header);

	public delegate void PacketHandler(PacketHeader header, byte[] payload = null, object msg = null);

	private const int BufferCapacity = 2097152;

	private readonly MessagePacking _messagePacker = new MessagePacking();

	private bool _willBeClosed;

	private bool _prevConnected;

	private bool _receiveCompleted;

	private bool _sendCompleted = true;

	private int _sendBufferSize;

	private readonly byte[] _sendBuffer1 = new byte[2097152];

	private readonly byte[] _sendBuffer2 = new byte[2097152];

	private int _sendBufferIndex;

	private readonly byte[] _receiveBuffer = new byte[2097152];

	private readonly byte[] _packingBuffer = new byte[2097152];

	private readonly byte[] _compressingBuffer = new byte[SnappyCodec.GetMaxCompressedLength(2097152)];

	private readonly byte[] _decompressingBuffer = new byte[2097152];

	private readonly byte[] _receivedBuffer = new byte[2097152];

	private readonly byte[] _remainingBuffer = new byte[2097152];

	private int _receivedSize;

	private Socket _sock;

	private SocketAsyncEventArgs _socketEventArg;

	private SocketAsyncEventArgs _socketReceiveEventArg;

	private uint _sequenceNumber = 1u;

	private readonly Dictionary<uint, PacketHandler> _packetHandlers = new Dictionary<uint, PacketHandler>();

	private readonly Queue<Packet> _packetQueue = new Queue<Packet>();

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
		_socketReceiveEventArg.SetBuffer(_receiveBuffer, 0, 2097152);
		_socketReceiveEventArg.Completed += SocketEventCompleted;
		_socketReceiveEventArg.UserToken = this;
	}

	public void Close()
	{
		try
		{
			if (_sock != null && _sock.Connected)
			{
				_sock.Shutdown(SocketShutdown.Both);
			}
		}
		catch (Exception)
		{
		}
		_socketEventArg = null;
		_socketReceiveEventArg = null;
		_receiveCompleted = false;
		_sendCompleted = true;
		_sendBufferSize = 0;
		_sendBufferIndex = 0;
		_sequenceNumber = 1u;
		_willBeClosed = false;
		lock (_packetQueue)
		{
			_packetQueue.Clear();
		}
		if (_sock != null)
		{
			_sock.Close();
			_prevConnected = false;
			_sock = null;
			if (this.ConnetionClosed != null)
			{
				this.ConnetionClosed();
			}
		}
	}

	public bool Connected()
	{
		return _sock != null && _sock.Connected;
	}

	public bool Send<T>(T msg, uint replyOf = 0u)
	{
		if (!Connected())
		{
			return false;
		}
		uint seq = _sequenceNumber++;
		try
		{
			int num = Packet.SerializeMsg(Times.UnixTimeNow(), seq, replyOf, msg, SendBuffer, _sendBufferSize, _packingBuffer, _compressingBuffer, _messagePacker);
			_sendBufferSize += num;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			Close();
			return false;
		}
		return true;
	}

	public bool Recv<T>(MessageHandler<T> handler)
	{
		return RegisterMessageHandlerToRegistry(_packetHandlers, handler);
	}

	private bool RegisterMessageHandlerToRegistry<T>(Dictionary<uint, PacketHandler> registry, MessageHandler<T> handler)
	{
		uint key = (uint)typeof(T).GetField("TypeCode").GetValue(null);
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
		_sendCompleted = false;
		if (_sock != null && _sock.Connected)
		{
			_socketEventArg.SetBuffer(SendBuffer, 0, _sendBufferSize);
			bool flag = _sock.SendAsync(_socketEventArg);
			_sendBufferIndex = (_sendBufferIndex + 1) % 2;
			_sendBufferSize = 0;
			if (!flag)
			{
				SendCompleted(_socketEventArg);
			}
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
		if (e.SocketError != 0 || e.BytesTransferred <= 0)
		{
			_willBeClosed = true;
		}
		_sendCompleted = true;
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
		if (e.SocketError != 0)
		{
		}
		_willBeClosed = true;
	}

	private void ReceiveProcess(SocketAsyncEventArgs e)
	{
		Buffer.BlockCopy(e.Buffer, e.Offset, _receivedBuffer, _receivedSize, e.BytesTransferred);
		_receivedSize += e.BytesTransferred;
		int num = 0;
		int num2 = _receivedSize;
		while (num2 > 0)
		{
			PacketHeader header = Packet.ReadPacketHeader(_receivedBuffer, num2, num);
			int num3 = header.Size + header.PayloadSize;
			if (header.Size == 0 || num2 < num3)
			{
				break;
			}
			byte[] array = new byte[header.PayloadSize];
			Buffer.BlockCopy(_receivedBuffer, num + header.Size, array, 0, header.PayloadSize);
			Packet item = default(Packet);
			item.Header = header;
			item.Payload = array;
			lock (_packetQueue)
			{
				_packetQueue.Enqueue(item);
			}
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
			lock (_packetQueue)
			{
				ProcessPacketQueue();
			}
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
		if (_sendCompleted && _sendBufferSize > 0)
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

	private void ProcessPacketQueue()
	{
		if (_packetQueue.Count != 0)
		{
			Packet packet = _packetQueue.Dequeue();
			try
			{
				_packetHandlers.TryGetValue(packet.Header.TypeCode, out var value);
				value?.Invoke(packet.Header, packet.Payload);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
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
