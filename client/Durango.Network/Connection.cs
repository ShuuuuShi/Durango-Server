using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Durango.Development;
using Durango.Utils;
using JetBrains.Annotations;
using Messages;
using MsgPack;
using Snappy;
using UnityEngine;

namespace Durango.Network;

public class Connection
{
	public delegate void MessageHandler<T>(T msg, PacketHeader header);

	public delegate void PacketHandler(PacketHeader header, byte[] payload, int payloadOffset, object msg = null);

	private class Relayed
	{
		public double Time;

		public Action Exec;
	}

	private class SyncedTime
	{
		private double _syncedTime;

		private bool _synced;

		private bool _syncDirty;

		public double Time
		{
			get
			{
				if (!_synced)
				{
					return Times.UnixTimeNow();
				}
				double num = (double)GetTime() - LastGameTimeAtSynced;
				return _syncedTime + num;
			}
			set
			{
				_syncDirty = false;
				_synced = true;
				_syncedTime = value;
				LastGameTimeAtSynced = GetTime();
			}
		}

		public bool Synced => _synced && !_syncDirty;

		public bool NeverSyncedYet => !_synced;

		public double LastGameTimeAtSynced { get; private set; }

		public void Reset()
		{
			_syncedTime = 0.0;
			_synced = false;
			_syncDirty = false;
			LastGameTimeAtSynced = 0.0;
		}

		public void SetSyncDirty()
		{
			_syncDirty = true;
		}
	}

	private class SyncedClock
	{
		public double Latency;

		public readonly SyncedTime PredictedServerTime = new SyncedTime();
	}

	private const int BufferCapacity = 262144;

	private const float ServerDelayTime = 0.1f;

	private const float KeepalivePeriod = 30f;

	private const int MaxSyncPackets = 5;

	private readonly bool _timeSynchronize;

	private readonly MessagePacking _messagePacker = new MessagePacking();

	private bool _willBeClosed;

	private bool _prevConnected;

	private bool _receiveReady;

	private bool _sendReady;

	private bool _connectEventReceived;

	private int _sendBufferSize;

	private readonly byte[] _socketSendBuffer1 = new byte[262144];

	private readonly byte[] _socketSendBuffer2 = new byte[262144];

	private int _sendBufferIndex;

	private readonly byte[] _socketReceiveBuffer = new byte[262144];

	private readonly byte[] _packingBuffer = new byte[262144];

	private readonly byte[] _compressingBuffer = new byte[SnappyCodec.GetMaxCompressedLength(262144)];

	private readonly byte[] _decompressingBuffer = new byte[2097152];

	private byte[] _receivedBuffer = new byte[262144];

	private byte[] _payloadBuffer = new byte[262144];

	private int _payloadBufferOffset;

	private byte[] _remainingBuffer = new byte[262144];

	private int _receivedSize;

	private Socket _sock;

	private SocketAsyncEventArgs _socketSendEventArg;

	private SocketAsyncEventArgs _socketReceiveEventArg;

	private SocketAsyncEventArgs _socketConnectEventArg;

	private uint _sequenceNumber = 1u;

	private readonly Queue<Packet> _packetQueue = new Queue<Packet>();

	private readonly Queue<Relayed> _relayedQueue = new Queue<Relayed>();

	private readonly HashSet<uint> _continuousReplies = new HashSet<uint>();

	private float _keepaliveSendAt;

	private readonly List<SyncedClock> _syncClockPackets = new List<SyncedClock>();

	private bool _waitClockResponse;

	private double _latestClockSendAt;

	private SyncedTime _predictedServerTime = new SyncedTime();

	private readonly SyncedTime _latestReceivedTime = new SyncedTime();

	private float _bufferTime;

	private float _bufferPing;

	private readonly Dictionary<uint, PacketHandler> _packetHandlers = new Dictionary<uint, PacketHandler>();

	private readonly Dictionary<uint, ReplyMessageHandler> _replyPacketHandlers = new Dictionary<uint, ReplyMessageHandler>();

	private List<IConnectionHook> _hookList;

	private IPAddress[] _hostAddresses;

	private int _hostAddressCursor;

	private int _port;

	public float Ping { get; private set; }

	private byte[] SendBuffer => (_sendBufferIndex != 0) ? _socketSendBuffer2 : _socketSendBuffer1;

	public event Action ConnectionSucceed;

	public event Action ConnectionClosed;

	public Connection(bool timeSynchronize = false)
	{
		_timeSynchronize = timeSynchronize;
	}

	private static float MCeil(float num, float mul)
	{
		return Mathf.Ceil(num / mul) * mul;
	}

	private void UpdateBufferTime(float ping)
	{
		float num = Mathf.Abs(_bufferPing - ping);
		if (!(num < 0.05f))
		{
			float b = MCeil(ping, 0.05f);
			_bufferTime = Mathf.Max(0f, b);
			_bufferPing = ping;
		}
	}

	private static float GetTime()
	{
		return Time.realtimeSinceStartup;
	}

	public void MaybeSendKeepalive()
	{
		float time = GetTime();
		if (Connected() && time - _keepaliveSendAt > 30f)
		{
			Send(default(Keepalive));
			_keepaliveSendAt = time;
		}
	}

	public void ConnectAsync(string host, int port)
	{
		try
		{
			_hostAddresses = Dns.GetHostEntry(host).AddressList;
		}
		catch (Exception)
		{
			if (!string.IsNullOrEmpty(host) && IPAddress.TryParse(host, out var address))
			{
				_hostAddresses = new IPAddress[1] { address };
			}
		}
		_hostAddressCursor = 0;
		_port = port;
		TryConnectAsync();
	}

	private void TryConnectAsync()
	{
		Close(callClosedHandler: false);
		if (_hostAddressCursor >= KUtility.GetSize(_hostAddresses))
		{
			return;
		}
		IPAddress addr = _hostAddresses[_hostAddressCursor];
		_hostAddressCursor++;
		InitializeSocket(addr, _port);
		try
		{
			if (!_sock.ConnectAsync(_socketConnectEventArg))
			{
				ConnectCompleted(_socketConnectEventArg);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			_connectEventReceived = true;
		}
	}

	private void InitializeSocket(IPAddress addr, int port)
	{
		_sock = new Socket(addr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
		_sock.NoDelay = true;
		_socketSendEventArg = new SocketAsyncEventArgs();
		_socketReceiveEventArg = new SocketAsyncEventArgs();
		_socketConnectEventArg = new SocketAsyncEventArgs();
		_socketSendEventArg.Completed += SocketEventCompleted;
		_socketSendEventArg.UserToken = this;
		_socketReceiveEventArg.SetBuffer(_socketReceiveBuffer, 0, 262144);
		_socketReceiveEventArg.Completed += SocketEventCompleted;
		_socketReceiveEventArg.UserToken = this;
		_socketConnectEventArg.Completed += SocketEventCompleted;
		_socketConnectEventArg.UserToken = this;
		_socketConnectEventArg.RemoteEndPoint = new IPEndPoint(addr, port);
	}

	public void Close(bool callClosedHandler = true)
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
		if (_sock != null)
		{
			_sock.Close();
			_sock = null;
		}
		else
		{
			callClosedHandler = false;
		}
		_willBeClosed = false;
		_prevConnected = false;
		_receiveReady = false;
		_sendReady = false;
		_connectEventReceived = false;
		_sendBufferSize = 0;
		_sendBufferIndex = 0;
		_receivedSize = 0;
		if (_socketSendEventArg != null)
		{
			_socketSendEventArg.Dispose();
			_socketSendEventArg = null;
		}
		if (_socketReceiveEventArg != null)
		{
			_socketReceiveEventArg.Dispose();
			_socketReceiveEventArg = null;
		}
		if (_socketConnectEventArg != null)
		{
			_socketConnectEventArg.Dispose();
			_socketConnectEventArg = null;
		}
		_sequenceNumber = 1u;
		lock (_packetQueue)
		{
			_packetQueue.Clear();
			_payloadBufferOffset = 0;
		}
		_relayedQueue.Clear();
		_continuousReplies.Clear();
		_keepaliveSendAt = 0f;
		_syncClockPackets.Clear();
		_waitClockResponse = false;
		_latestClockSendAt = 0.0;
		_predictedServerTime.Reset();
		_latestReceivedTime.Reset();
		if (this.ConnectionClosed != null && callClosedHandler)
		{
			this.ConnectionClosed();
		}
	}

	public bool Connected()
	{
		return _sock != null && _sock.Connected;
	}

	public bool IsAttemptingToConnect()
	{
		return _sock != null && !_sock.Connected;
	}

	public void AddHook([NotNull] IConnectionHook hook)
	{
		if (_hookList == null)
		{
			_hookList = new List<IConnectionHook>();
		}
		_hookList.Add(hook);
	}

	public void RemoveHook([NotNull] IConnectionHook hook)
	{
		if (_hookList != null)
		{
			_hookList.Remove(hook);
			if (_hookList.Count == 0)
			{
				_hookList = null;
			}
		}
	}

	public ReplyMessageHandlerRegistrar Send<T>(T msg, bool noReply = false, uint replyOf = 0u)
	{
		ReplyMessageHandlerRegistrar empty = ReplyMessageHandlerRegistrar.Empty;
		if (_hookList != null)
		{
			for (int num = _hookList.Count - 1; num >= 0; num--)
			{
				IConnectionHook connectionHook = _hookList[num];
				if (connectionHook.HookSendingMessage(this, _sequenceNumber, msg, noReply, replyOf))
				{
					uint num2 = _sequenceNumber++;
					if (Debug.isDebugBuild)
					{
						int bufferSize = Packet.SerializeMsg(GetPredictedServerTime(), num2, replyOf, msg, SendBuffer, _sendBufferSize, _decompressingBuffer, _compressingBuffer, _messagePacker);
						PacketWatcher.Instance().RecordSendPacket(SendBuffer, _sendBufferSize, bufferSize, num2);
					}
					return (!noReply) ? new ReplyMessageHandlerRegistrar(this, num2) : empty;
				}
			}
		}
		if (!Connected())
		{
			return empty;
		}
		uint num3 = _sequenceNumber++;
		try
		{
			int num4 = Packet.SerializeMsg(GetPredictedServerTime(), num3, replyOf, msg, SendBuffer, _sendBufferSize, _packingBuffer, _compressingBuffer, _messagePacker);
			if (Debug.isDebugBuild)
			{
				PacketWatcher.Instance().RecordSendPacket(SendBuffer, _sendBufferSize, num4, num3);
			}
			_sendBufferSize += num4;
		}
		catch (Exception)
		{
			Close();
			return empty;
		}
		return (!noReply) ? new ReplyMessageHandlerRegistrar(this, num3) : empty;
	}

	public bool On<T>(MessageHandler<T> handler)
	{
		return RegisterMessageHandlerToRegistry(_packetHandlers, handler);
	}

	public bool RegisterReplyMessageHandler<T>(uint seq, MessageHandler<T> handler)
	{
		ReplyMessageHandler value = _replyPacketHandlers.Get(seq);
		if (value.Dictionary == null)
		{
			value.Dictionary = new Dictionary<uint, PacketHandler>();
		}
		_replyPacketHandlers[seq] = value;
		return RegisterMessageHandlerToRegistry(value.Dictionary, handler);
	}

	public void RegisterReplyMessageHandler(uint seq, Action<Packet> handler, bool allowReplied)
	{
		ReplyMessageHandler value = _replyPacketHandlers.Get(seq);
		value.Handler = handler;
		value.AllowReplied = allowReplied;
		_replyPacketHandlers[seq] = value;
	}

	public void RegisterReplySequenceHandler(uint seq, Action<bool> handler)
	{
		ReplyMessageHandler value = _replyPacketHandlers.Get(seq);
		value.Sequence = handler;
		_replyPacketHandlers[seq] = value;
	}

	public bool RegisterRelayHandler<T>(Action<T, float> handler)
	{
		return On(delegate(T msg, PacketHeader header)
		{
			_relayedQueue.Enqueue(new Relayed
			{
				Time = header.Time,
				Exec = delegate
				{
					handler(msg, CheckBufferedTimePassed(header.Time));
				}
			});
		});
	}

	public double GetBufferedServerTime()
	{
		return GetPredictedServerTime() - 0.10000000149011612 - (double)_bufferTime;
	}

	public double GetPredictedServerTime()
	{
		return (!_predictedServerTime.NeverSyncedYet) ? _predictedServerTime.Time : _latestReceivedTime.Time;
	}

	public float CheckBufferedTimePassed(double baseTime)
	{
		double bufferedServerTime = GetBufferedServerTime();
		return Mathf.Max(0f, (float)(bufferedServerTime - baseTime));
	}

	public bool IsTimeSynchronized()
	{
		return _predictedServerTime.Synced;
	}

	public void Handle(uint type, object msg, PacketHeader header)
	{
		if (_packetHandlers.TryGetValue(type, out var value))
		{
			value(header, null, 0, msg);
		}
	}

	public void PushPacket<T>(T msg, uint replyOf = 0u)
	{
		PacketHeader packetHeader = default(PacketHeader);
		packetHeader.Time = GetBufferedServerTime();
		packetHeader.Size = 24;
		packetHeader.Seq = uint.MaxValue;
		packetHeader.ReplyOf = replyOf;
		PacketHeader header = packetHeader;
		using (MemoryStream memoryStream = new MemoryStream(_packingBuffer))
		{
			using Packer packer = Packer.Create(memoryStream);
			if (!_messagePacker.Pack(msg, packer, out var typeCode))
			{
				return;
			}
			header.TypeCode = typeCode;
			header.PayloadSize = SnappyCodec.Compress(_packingBuffer, 0, (int)memoryStream.Position, _compressingBuffer, 0);
		}
		lock (_packetQueue)
		{
			Buffer.BlockCopy(_compressingBuffer, 0, _payloadBuffer, _payloadBufferOffset, header.PayloadSize);
			Packet item = default(Packet);
			item.Header = header;
			item.Payload = _payloadBuffer;
			item.PayloadOffset = _payloadBufferOffset;
			_packetQueue.Enqueue(item);
			_payloadBufferOffset += header.PayloadSize;
		}
	}

	private void StartSend()
	{
		_sendReady = false;
		if (_sock != null && _sock.Connected && _socketSendEventArg != null)
		{
			_socketSendEventArg.SetBuffer(SendBuffer, 0, _sendBufferSize);
			bool flag = _sock.SendAsync(_socketSendEventArg);
			_sendBufferIndex = (_sendBufferIndex + 1) % 2;
			_sendBufferSize = 0;
			if (!flag)
			{
				SendCompleted(_socketSendEventArg);
			}
		}
	}

	private void StartReceive()
	{
		_receiveReady = false;
		if (_sock != null && _sock.Connected && _socketReceiveEventArg != null && !_sock.ReceiveAsync(_socketReceiveEventArg))
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
		case SocketAsyncOperation.Connect:
			connection.ConnectCompleted(e);
			break;
		case SocketAsyncOperation.Receive:
			connection.ReceiveCompleted(e);
			break;
		case SocketAsyncOperation.Send:
			connection.SendCompleted(e);
			break;
		}
	}

	private void ConnectCompleted(SocketAsyncEventArgs e)
	{
		_connectEventReceived = true;
		if (e.SocketError == SocketError.Success)
		{
			_prevConnected = true;
			_receiveReady = true;
			_sendReady = true;
		}
	}

	private void SendCompleted(SocketAsyncEventArgs e)
	{
		if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
		{
			_sendReady = true;
		}
		else
		{
			_willBeClosed = true;
		}
	}

	private void ReceiveCompleted(SocketAsyncEventArgs e)
	{
		if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
		{
			try
			{
				ReceiveProcess(e);
				_receiveReady = true;
				return;
			}
			catch (Exception)
			{
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
		int num = _receivedBuffer.Length - _receivedSize - e.BytesTransferred;
		if (num < 0)
		{
			ExtendBuffer(ref _receivedBuffer, -num);
		}
		Buffer.BlockCopy(e.Buffer, e.Offset, _receivedBuffer, _receivedSize, e.BytesTransferred);
		_receivedSize += e.BytesTransferred;
		int num2 = 0;
		int num3 = _receivedSize;
		while (num3 > 0)
		{
			PacketHeader header = Packet.ReadPacketHeader(_receivedBuffer, num3, num2);
			int num4 = header.Size + header.PayloadSize;
			if (header.Size == 0 || num3 < num4)
			{
				break;
			}
			lock (_packetQueue)
			{
				if (_payloadBufferOffset + header.PayloadSize >= _payloadBuffer.Length)
				{
					_payloadBuffer = new byte[_payloadBufferOffset + header.PayloadSize + 262144];
					_payloadBufferOffset = 0;
				}
				Buffer.BlockCopy(_receivedBuffer, num2 + header.Size, _payloadBuffer, _payloadBufferOffset, header.PayloadSize);
				Packet item = default(Packet);
				item.Header = header;
				item.Payload = _payloadBuffer;
				item.PayloadOffset = _payloadBufferOffset;
				_packetQueue.Enqueue(item);
				_payloadBufferOffset += header.PayloadSize;
			}
			num2 += num4;
			num3 -= num4;
		}
		if (num3 > 0 && num3 != _receivedSize)
		{
			int num5 = _remainingBuffer.Length - num3;
			if (num5 < 0)
			{
				ExtendBuffer(ref _remainingBuffer, -num5);
			}
			Buffer.BlockCopy(_receivedBuffer, num2, _remainingBuffer, 0, num3);
			Buffer.BlockCopy(_remainingBuffer, 0, _receivedBuffer, 0, num3);
		}
		_receivedSize = num3;
	}

	private static void ExtendBuffer(ref byte[] targetBuffer, int extendSize)
	{
		byte[] array = targetBuffer;
		targetBuffer = new byte[array.Length + extendSize];
		Buffer.BlockCopy(array, 0, targetBuffer, 0, array.Length);
	}

	private static bool TryGetTypeCode(Type type, out uint typeCode)
	{
		FieldInfo field = type.GetField("TypeCode");
		if (field == null)
		{
			typeCode = 0u;
			return false;
		}
		typeCode = (uint)field.GetValue(null);
		return true;
	}

	private bool RegisterMessageHandlerToRegistry<T>(IDictionary<uint, PacketHandler> registry, MessageHandler<T> handler)
	{
		if (!TryGetTypeCode(typeof(T), out var typeCode))
		{
			return false;
		}
		bool flag = registry.ContainsKey(typeCode);
		if (flag)
		{
			registry.Remove(typeCode);
		}
		registry.Add(typeCode, delegate(PacketHeader header, byte[] payload, int offset, object msg)
		{
			T msg2 = ((payload == null) ? ((T)msg) : MakeMsg<T>(payload, offset, header.PayloadSize));
			handler(msg2, header);
		});
		_messagePacker.RegisterHandler<T>(null);
		return flag;
	}

	private void ClockMessageHandler(Clock pass, PacketHeader header)
	{
		_waitClockResponse = false;
		double num = Times.UnixTimeNow();
		double clientTime = pass.ClientTime;
		double num2 = (num - clientTime) / 2.0;
		SyncedClock syncedClock = new SyncedClock();
		syncedClock.Latency = num2;
		syncedClock.PredictedServerTime.Time = pass.ServerTime + num2;
		_syncClockPackets.Add(syncedClock);
		if (_syncClockPackets.Count >= 5)
		{
			_syncClockPackets.Sort((SyncedClock x, SyncedClock y) => x.Latency.CompareTo(y.Latency));
			_predictedServerTime = _syncClockPackets[0].PredictedServerTime;
			_syncClockPackets.Clear();
		}
	}

	private bool NeedTimeSynchronization()
	{
		return !IsTimeSynchronized() || (double)GetTime() - _predictedServerTime.LastGameTimeAtSynced >= 600.0;
	}

	public void ForceSyncClock()
	{
		_predictedServerTime.SetSyncDirty();
	}

	private void SendGetClock()
	{
		if (_syncClockPackets.Count < 5 && !_waitClockResponse)
		{
			_waitClockResponse = true;
			Send(new GetClock
			{
				Time = Times.UnixTimeNow()
			}).On<Clock>(ClockMessageHandler);
		}
	}

	private void PingClockMessageHandler(Clock pass, PacketHeader header)
	{
		double num = Times.UnixTimeNow();
		double clientTime = pass.ClientTime;
		Ping = (float)(num - clientTime);
		UpdateBufferTime(Ping);
		float num2 = Ping / 2f;
		double num3 = pass.ServerTime + (double)num2;
		double num4 = Math.Abs(num3 - _predictedServerTime.Time);
		if (num4 > 0.30000001192092896)
		{
			ForceSyncClock();
		}
	}

	private void SendGetClockForCheckPing()
	{
		Send(new GetClock
		{
			Time = Times.UnixTimeNow()
		}).On<Clock>(PingClockMessageHandler);
	}

	public void Process()
	{
		CheckConnectionSucceed();
		ProcessClock();
		ProcessPacketQueue();
		ProcessRelayedQueue();
		if (_willBeClosed)
		{
			Close();
		}
		ProcessReceiveSend();
		if (_prevConnected && _sock != null && !_sock.Connected)
		{
			_willBeClosed = true;
		}
	}

	private void ProcessClock()
	{
		if (_timeSynchronize && Connected())
		{
			if (NeedTimeSynchronization())
			{
				SendGetClock();
			}
			float time = GetTime();
			if (IsTimeSynchronized() && (double)time - _latestClockSendAt > 5.0)
			{
				SendGetClockForCheckPing();
				_latestClockSendAt = time;
			}
		}
	}

	private void ProcessPacketQueue()
	{
		if (_packetQueue.Count <= 0)
		{
			return;
		}
		lock (_packetQueue)
		{
			while (_packetQueue.Count != 0)
			{
				Packet packet = _packetQueue.Dequeue();
				try
				{
					ProcessPacket(packet);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
			_payloadBufferOffset = 0;
		}
	}

	private void ProcessPacket(Packet packet)
	{
		if (_latestReceivedTime.NeverSyncedYet)
		{
			_latestReceivedTime.Time = packet.Header.Time;
		}
		if (Debug.isDebugBuild)
		{
			PacketWatcher.Instance().RecordReceivePacket(packet.Header, packet.Payload, packet.PayloadOffset);
		}
		if (packet.Header.TypeCode == 0)
		{
			if (_continuousReplies.Contains(packet.Header.ReplyOf))
			{
				_continuousReplies.Remove(packet.Header.ReplyOf);
				if (_replyPacketHandlers.TryGetValue(packet.Header.ReplyOf, out var value))
				{
					_replyPacketHandlers.Remove(packet.Header.ReplyOf);
					if (value.Sequence != null)
					{
						value.Sequence(obj: false);
					}
				}
			}
			else
			{
				_continuousReplies.Add(packet.Header.ReplyOf);
				if (_replyPacketHandlers.TryGetValue(packet.Header.ReplyOf, out var value2) && value2.Sequence != null)
				{
					value2.Sequence(obj: true);
				}
			}
		}
		else
		{
			HandleMsg(packet);
		}
	}

	private void HandleMsg(Packet packet)
	{
		uint replyOf = packet.Header.ReplyOf;
		uint typeCode = packet.Header.TypeCode;
		ReplyMessageHandler replyMessageHandler = _replyPacketHandlers.Get(replyOf);
		PacketHandler packetHandler = null;
		bool flag = false;
		if (replyMessageHandler.Dictionary != null)
		{
			packetHandler = replyMessageHandler.Dictionary.Get(typeCode);
			if (packetHandler != null)
			{
				flag = true;
			}
		}
		if (packetHandler == null)
		{
			packetHandler = _packetHandlers.Get(typeCode);
		}
		bool flag2 = true;
		if (packetHandler != null)
		{
			packetHandler(packet.Header, packet.Payload, packet.PayloadOffset);
			flag2 = false;
		}
		if (replyMessageHandler.Handler != null && (replyMessageHandler.AllowReplied || !flag))
		{
			replyMessageHandler.Handler(packet);
			flag2 = false;
		}
		if (flag && typeCode == 1022)
		{
			_packetHandlers.Get(typeCode)?.Invoke(packet.Header, packet.Payload, packet.PayloadOffset);
		}
		if (flag2)
		{
		}
		if (replyOf != 0 && !_continuousReplies.Contains(replyOf))
		{
			_replyPacketHandlers.Remove(replyOf);
		}
	}

	private void ProcessRelayedQueue()
	{
		double bufferedServerTime = GetBufferedServerTime();
		while (_relayedQueue.Count != 0 && !(_relayedQueue.Peek().Time > bufferedServerTime))
		{
			try
			{
				Relayed relayed = _relayedQueue.Dequeue();
				relayed.Exec();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	private void ProcessReceiveSend()
	{
		if (_receiveReady)
		{
			try
			{
				StartReceive();
			}
			catch (Exception)
			{
				Close();
			}
		}
		if (_sendReady && _sendBufferSize > 0)
		{
			try
			{
				StartSend();
			}
			catch (Exception)
			{
				Close();
			}
		}
	}

	private void CheckConnectionSucceed()
	{
		if (!_connectEventReceived)
		{
			return;
		}
		_connectEventReceived = false;
		if (Connected())
		{
			if (this.ConnectionSucceed != null)
			{
				this.ConnectionSucceed();
			}
		}
		else
		{
			TryConnectAsync();
		}
	}

	public static bool TryParse(string address, out string host, out int port)
	{
		host = string.Empty;
		port = 0;
		if (string.IsNullOrEmpty(address))
		{
			return false;
		}
		int num = address.LastIndexOf(':');
		if (num == -1)
		{
			return false;
		}
		host = address.Substring(0, num);
		string s = address.Substring(host.Length + 1, address.Length - host.Length - 1);
		return int.TryParse(s, out port);
	}
}
