using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Holoville.HOTween;
using Messages;
using MsgPack;
using Snappy;
using UnityEngine;

namespace K1Network;

public class Connection
{
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
					return KUtility.GetTimestamp();
				}
				double num = (double)Time.realtimeSinceStartup - LastGameTimeAtSynced;
				return _syncedTime + num;
			}
			set
			{
				_syncDirty = false;
				_synced = true;
				_syncedTime = value;
				LastGameTimeAtSynced = Time.realtimeSinceStartup;
			}
		}

		public bool Synced => _synced && !_syncDirty;

		public bool NeverSyncedYet => !_synced;

		public double LastGameTimeAtSynced { get; private set; }

		public void Reset()
		{
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

	public delegate void FailedResponseDelegate(MessagePackObjectDictionary data, object param);

	public delegate void MessageHandler<T>(T msg, PacketHeader header);

	public delegate void MessagePackObjectDictionaryHandler(MessagePackObjectDictionary dict);

	public delegate void PacketHandler(PacketHeader header, byte[] payload = null, object msg = null);

	public delegate void RelayHandler<T>(T msg, float timePassed);

	public delegate void ResponseDelegate(MessagePackObjectDictionary data, object param);

	private const int BufferCapacity = 262144;

	private const float SeverDelayTimeNormal = 1f;

	private const float SeverDelayTimeFastResponse = 0.5f;

	private const float KeepalivePeriod = 30f;

	private const int MaxSyncPackets = 5;

	private const float BufferTimeMultipleValue = 0.05f;

	private readonly bool _timeSynchronize;

	private readonly MessagePacking _messagePacker = new MessagePacking();

	private readonly byte[] _packetHeaderBuffer = new byte[16];

	private readonly byte[] _utf7Buffer = new byte[16];

	private bool _isFastResponseMode;

	private bool _willBeClosed;

	private bool _prevConnected;

	private bool _receiveCompleted;

	private bool _sendCompleted = true;

	private int _sendBufferSize;

	private readonly byte[] _socketSendBuffer1 = new byte[262144];

	private readonly byte[] _socketSendBuffer2 = new byte[262144];

	private int _sendBufferIndex;

	private readonly byte[] _socketReceiveBuffer = new byte[262144];

	private readonly byte[] _packingBuffer = new byte[262144];

	private readonly byte[] _compressingBuffer = new byte[SnappyCodec.GetMaxCompressedLength(262144)];

	private readonly byte[] _decompressingBuffer = new byte[1351680];

	private byte[] _receivedBuffer = new byte[262144];

	private byte[] _remainingBuffer = new byte[262144];

	private int _receivedSize;

	private Socket _sock;

	private SocketAsyncEventArgs _socketEventArg;

	private SocketAsyncEventArgs _socketReceiveEventArg;

	private ulong _sequenceNumber = 1uL;

	private ConnectionHook _hook;

	private readonly Dictionary<uint, PacketHandler> _packetHandlers = new Dictionary<uint, PacketHandler>();

	private readonly Dictionary<ulong, Dictionary<uint, PacketHandler>> _replyPacketHandlers = new Dictionary<ulong, Dictionary<uint, PacketHandler>>();

	private readonly Dictionary<int, MessageHandler<Notify>> _notificationHandlers = new Dictionary<int, MessageHandler<Notify>>();

	private readonly Dictionary<string, MessagePackObjectDictionaryHandler> _dynamicRelayHandlers = new Dictionary<string, MessagePackObjectDictionaryHandler>();

	private readonly Queue<Packet> _packetQueue = new Queue<Packet>();

	private readonly Queue<Relayed> _relayedQueue = new Queue<Relayed>();

	private readonly HashSet<ulong> _continuousReplies = new HashSet<ulong>();

	private float _keepaliveSendAt;

	private readonly List<SyncedClock> _syncClockPackets = new List<SyncedClock>();

	private bool _waitClockResponse;

	private double _latestClockSendAt;

	private SyncedTime _predictedServerTime = new SyncedTime();

	private readonly SyncedTime _latestReceivedTime = new SyncedTime();

	public float BufferTime { get; private set; }

	public float Latency { get; private set; }

	public bool IsFastResponseMode
	{
		get
		{
			return _isFastResponseMode;
		}
		set
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Expected O, but got Unknown
			_isFastResponseMode = value;
			float num = ((!value) ? 1f : 0.5f);
			TweenParms val = new TweenParms();
			val.Prop("SeverDelayTime", (object)num);
			val.Ease((EaseType)4);
			HOTween.To((object)this, 1f, val);
		}
	}

	public float SeverDelayTime { get; set; }

	private byte[] SendBuffer => (_sendBufferIndex != 0) ? _socketSendBuffer2 : _socketSendBuffer1;

	public event Action<Packet> PacketReceived;

	public event Action ConnetionSucceed;

	public event Action ConnetionClosed;

	public Connection(bool timeSynchronize = false)
	{
		SeverDelayTime = 1f;
		_timeSynchronize = timeSynchronize;
		_messagePacker.RegisterHandler<Response>(null);
	}

	private float MCeil(float num, float mul)
	{
		return Mathf.Ceil(num / mul) * mul;
	}

	private void UpdateBufferTime(float latency)
	{
		float num = MCeil(latency, 0.05f);
		if (num < BufferTime)
		{
			float num2 = BufferTime - 0.075f;
			if (latency < num2)
			{
				BufferTime = Mathf.Max(0f, num) * 2f;
			}
		}
		else
		{
			BufferTime = Mathf.Max(0f, num) * 2f;
		}
	}

	public void MaybeSendKeepalive()
	{
		if (Connected() && Time.time - _keepaliveSendAt > 30f)
		{
			Send(default(Keepalive));
			_keepaliveSendAt = Time.time;
		}
	}

	public bool ConnectAsync(string url, int port)
	{
		_willBeClosed = false;
		_prevConnected = false;
		InitializeSocket();
		_socketReceiveEventArg.RemoteEndPoint = new IPEndPoint(IPAddress.Parse(url), port);
		return _sock.ConnectAsync(_socketReceiveEventArg);
	}

	private void InitializeSocket()
	{
		if (Connected())
		{
			Debug.LogError((object)"InitializeSocket() in Connected Socket");
			Close();
		}
		_sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		_sock.NoDelay = true;
		_socketEventArg = new SocketAsyncEventArgs();
		_socketReceiveEventArg = new SocketAsyncEventArgs();
		_socketEventArg.Completed += SocketEventCompleted;
		_socketEventArg.UserToken = this;
		_socketReceiveEventArg.SetBuffer(_socketReceiveBuffer, 0, 262144);
		_socketReceiveEventArg.Completed += SocketEventCompleted;
		_socketReceiveEventArg.UserToken = this;
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
		_isFastResponseMode = false;
		_willBeClosed = false;
		_prevConnected = false;
		_receiveCompleted = false;
		_sendCompleted = true;
		_sendBufferSize = 0;
		_sendBufferIndex = 0;
		_receivedSize = 0;
		if (_sock != null)
		{
			_sock.Close();
			_sock = null;
		}
		else
		{
			callClosedHandler = false;
		}
		_socketEventArg = null;
		_socketReceiveEventArg = null;
		_sequenceNumber = 1uL;
		_hook = null;
		lock (_packetQueue)
		{
			_packetQueue.Clear();
		}
		_relayedQueue.Clear();
		_continuousReplies.Clear();
		_keepaliveSendAt = 0f;
		_syncClockPackets.Clear();
		_waitClockResponse = false;
		_latestClockSendAt = 0.0;
		_predictedServerTime.Reset();
		_latestReceivedTime.Reset();
		if (this.ConnetionClosed != null && callClosedHandler)
		{
			this.ConnetionClosed();
		}
	}

	public bool Connected()
	{
		return _sock != null && _sock.Connected;
	}

	public bool IsAttemptingToConnect()
	{
		return _sock != null;
	}

	public bool Request(int method, MessagePackObjectDictionary data = null, ResponseDelegate onSuccess = null, object param = null)
	{
		return true;
	}

	public MessageHandler<Notify> GetNotificationHandler(int method)
	{
		_notificationHandlers.TryGetValue(method, out var value);
		return value;
	}

	public void SetHook(ConnectionHook hook)
	{
		_hook = hook;
	}

	public ulong CurrentSeq()
	{
		return _sequenceNumber;
	}

	public ReplyMessageHandlerRegistrar Send<T>(T msg, bool noReply = false, ulong replyOf = 0)
	{
		ReplyMessageHandlerRegistrar empty = ReplyMessageHandlerRegistrar.Empty;
		if ((Object)(object)_hook != (Object)null && _hook.HookSendingMessage(msg))
		{
			return empty;
		}
		if (!Connected())
		{
			return empty;
		}
		ulong seq = _sequenceNumber++;
		try
		{
			int num = Packet.SerializeMsg(GetPredictedServerTime(), seq, replyOf, msg, SendBuffer, _sendBufferSize, _packingBuffer, _compressingBuffer, _utf7Buffer, _messagePacker);
			_sendBufferSize += num;
			if (Debug.isDebugBuild)
			{
				PacketWatcher.Instance().RecordSendPacket(msg, num, seq);
			}
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
			Close();
			return empty;
		}
		return (!noReply) ? new ReplyMessageHandlerRegistrar(this, seq) : empty;
	}

	public bool On<T>(MessageHandler<T> handler)
	{
		return RegisterMessageHandlerToRegistry(_packetHandlers, handler);
	}

	public bool RegisterReplyMessageHandler<T>(ulong seq, MessageHandler<T> handler)
	{
		if (!_replyPacketHandlers.ContainsKey(seq))
		{
			_replyPacketHandlers[seq] = new Dictionary<uint, PacketHandler>();
		}
		return RegisterMessageHandlerToRegistry(_replyPacketHandlers[seq], handler);
	}

	public bool RegisterDynamicRelayHandler(string method, MessagePackObjectDictionaryHandler handler)
	{
		bool flag = _dynamicRelayHandlers.ContainsKey(method);
		if (flag)
		{
			_dynamicRelayHandlers[method] = handler;
		}
		else
		{
			_dynamicRelayHandlers.Add(method, handler);
		}
		_messagePacker.RegisterHandler<Relay>(null);
		return flag;
	}

	public bool RegisterRelayHandler<T>(RelayHandler<T> handler)
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

	public bool Legacy_RegisterNotificationHandler(int method, MessageHandler<Notify> messageHandler)
	{
		bool flag = _notificationHandlers.ContainsKey(method);
		if (flag)
		{
			_notificationHandlers[method] = messageHandler;
		}
		else
		{
			_notificationHandlers.Add(method, messageHandler);
		}
		_messagePacker.RegisterHandler<Notify>(null);
		return flag;
	}

	public double GetBufferedServerTime()
	{
		return GetPredictedServerTime() - (double)SeverDelayTime;
	}

	public double GetBufferedServerTime_Enhanced()
	{
		return GetBufferedServerTime() - (double)BufferTime;
	}

	public double GetPredictedServerTime()
	{
		return (!_predictedServerTime.NeverSyncedYet) ? _predictedServerTime.Time : _latestReceivedTime.Time;
	}

	public float CheckBufferedTimePassed(double baseTime)
	{
		double bufferedServerTime = GetBufferedServerTime();
		if (bufferedServerTime > baseTime)
		{
			return (float)(bufferedServerTime - baseTime);
		}
		return 0f;
	}

	public float CheckBufferedTimePassed_Enhanced(double baseTime)
	{
		double num = GetBufferedServerTime() - (double)BufferTime;
		if (num > baseTime)
		{
			return (float)(num - baseTime);
		}
		return 0f;
	}

	public bool IsTimeSynchronized()
	{
		return _predictedServerTime.Synced;
	}

	public void Handle(uint type, object msg, PacketHeader header)
	{
		if (_packetHandlers.TryGetValue(type, out var value))
		{
			value(header, null, msg);
		}
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

	private T MakeMsg<T>(byte[] payload, int payloadSize)
	{
		return Packet.DeserializeMsg<T>(payload, payloadSize, _decompressingBuffer, _messagePacker);
	}

	private void SocketEventCompleted(object sender, SocketAsyncEventArgs e)
	{
		Connection connection = (Connection)e.UserToken;
		switch (e.LastOperation)
		{
		case SocketAsyncOperation.Connect:
			if (e.SocketError == SocketError.Success)
			{
				_prevConnected = true;
				if (this.ConnetionSucceed != null)
				{
					this.ConnetionSucceed();
				}
			}
			else
			{
				_willBeClosed = true;
			}
			break;
		case SocketAsyncOperation.Receive:
			connection.ReceiveCompleted(e);
			break;
		case SocketAsyncOperation.Send:
			connection.SendCompleted(e);
			break;
		case SocketAsyncOperation.Disconnect:
		case SocketAsyncOperation.ReceiveFrom:
		case SocketAsyncOperation.ReceiveMessageFrom:
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
			catch (Exception ex)
			{
				Debug.LogException(ex);
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
			PacketHeader header = Packet.ReadPacketHeader(_receivedBuffer, num3, num2, _packetHeaderBuffer);
			int num4 = header.Size + header.PayloadSize;
			if (header.Size == 0 || num3 < num4)
			{
				break;
			}
			byte[] array = new byte[header.PayloadSize];
			Buffer.BlockCopy(_receivedBuffer, num2 + header.Size, array, 0, header.PayloadSize);
			Packet packet = new Packet();
			packet.Header = header;
			packet.Payload = array;
			lock (_packetQueue)
			{
				if (_sock == null)
				{
					return;
				}
				_packetQueue.Enqueue(packet);
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
		Buffer.BlockCopy(targetBuffer, 0, array, 0, array.Length);
	}

	private bool RegisterMessageHandlerToRegistry<T>(Dictionary<uint, PacketHandler> registry, MessageHandler<T> handler)
	{
		FieldInfo field = typeof(T).GetField("TypeCode");
		if ((object)field == null)
		{
			return false;
		}
		uint key = (uint)field.GetValue(null);
		bool flag = registry.ContainsKey(key);
		if (flag)
		{
			registry.Remove(key);
		}
		registry.Add(key, delegate(PacketHeader header, byte[] payload, object msg)
		{
			T msg2 = ((payload == null) ? ((T)msg) : MakeMsg<T>(payload, header.PayloadSize));
			handler(msg2, header);
		});
		_messagePacker.RegisterHandler<T>(null);
		return flag;
	}

	private void ClockMessageHandler(Clock pass, PacketHeader header)
	{
		_waitClockResponse = false;
		double timestamp = KUtility.GetTimestamp();
		double clientTime = pass.ClientTime;
		double num = (timestamp - clientTime) / 2.0;
		SyncedClock syncedClock = new SyncedClock();
		syncedClock.Latency = num;
		syncedClock.PredictedServerTime.Time = pass.ServerTime + num;
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
		return !IsTimeSynchronized() || (double)Time.realtimeSinceStartup - _predictedServerTime.LastGameTimeAtSynced >= 600.0;
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
				Time = KUtility.GetTimestamp()
			}).On<Clock>(ClockMessageHandler);
		}
	}

	private void LatencyClockMessageHandler(Clock pass, PacketHeader header)
	{
		double timestamp = KUtility.GetTimestamp();
		double clientTime = pass.ClientTime;
		Latency = (float)(timestamp - clientTime) / 2f;
		UpdateBufferTime(Latency);
		double num = pass.ServerTime + (double)Latency;
		double num2 = Math.Abs(num - _predictedServerTime.Time);
		if (num2 > 0.30000001192092896)
		{
			ForceSyncClock();
		}
	}

	private void SendGetClockForCheckLatency()
	{
		Send(new GetClock
		{
			Time = KUtility.GetTimestamp()
		}).On<Clock>(LatencyClockMessageHandler);
	}

	public void Process()
	{
		if (_timeSynchronize && Connected())
		{
			if (NeedTimeSynchronization())
			{
				SendGetClock();
			}
			if (IsTimeSynchronized() && (double)Time.realtimeSinceStartup - _latestClockSendAt > 5.0)
			{
				SendGetClockForCheckLatency();
				_latestClockSendAt = Time.realtimeSinceStartup;
			}
		}
		if (_packetQueue.Count > 0)
		{
			lock (_packetQueue)
			{
				ProcessPacketQueue();
			}
		}
		double bufferedServerTime = GetBufferedServerTime();
		while (_relayedQueue.Count != 0 && !(_relayedQueue.Peek().Time > bufferedServerTime))
		{
			Relayed relayed = _relayedQueue.Dequeue();
			relayed.Exec();
		}
		CheckSocketClosed();
		if (_receiveCompleted)
		{
			try
			{
				StartReceive();
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
				Close();
			}
		}
		if (_sendCompleted && _sendBufferSize > 0)
		{
			try
			{
				StartSend();
			}
			catch (Exception ex2)
			{
				Debug.LogException(ex2);
				Close();
			}
		}
	}

	private void ProcessPacketQueue()
	{
		while (_packetQueue.Count != 0 && _sock != null)
		{
			Packet packet = _packetQueue.Dequeue();
			if (_latestReceivedTime.NeverSyncedYet)
			{
				_latestReceivedTime.Time = packet.Header.Time;
			}
			if (Debug.isDebugBuild)
			{
				PacketWatcher.Instance().RecordReceivePacket(packet.Header.TypeCode, packet.Payload, packet.Header.PayloadSize, packet.Header.ReplyOf, _messagePacker);
			}
			if (packet.Header.TypeCode == 0)
			{
				if (_continuousReplies.Contains(packet.Header.ReplyOf))
				{
					_continuousReplies.Remove(packet.Header.ReplyOf);
					_replyPacketHandlers.Remove(packet.Header.ReplyOf);
				}
				else
				{
					_continuousReplies.Add(packet.Header.ReplyOf);
				}
			}
			else if (packet.Header.TypeCode == 503)
			{
				HandleNotifyMsg(packet.Header, packet.Payload);
			}
			else if (packet.Header.TypeCode == 502)
			{
				_relayedQueue.Enqueue(new Relayed
				{
					Time = packet.Header.Time,
					Exec = delegate
					{
						HandleRelayMsg(packet.Header, packet.Payload);
					}
				});
			}
			else
			{
				try
				{
					PacketHandler value = null;
					if (_replyPacketHandlers.TryGetValue(packet.Header.ReplyOf, out var value2))
					{
						value2.TryGetValue(packet.Header.TypeCode, out value);
					}
					if (value == null)
					{
						_packetHandlers.TryGetValue(packet.Header.TypeCode, out value);
					}
					value?.Invoke(packet.Header, packet.Payload);
					if (packet.Header.ReplyOf != 0L && !_continuousReplies.Contains(packet.Header.ReplyOf))
					{
						_replyPacketHandlers.Remove(packet.Header.ReplyOf);
					}
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}
			if (this.PacketReceived != null)
			{
				this.PacketReceived(packet);
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

	private void HandleNotifyMsg(PacketHeader header, byte[] payload)
	{
		Notify msg = MakeMsg<Notify>(payload, header.PayloadSize);
		if (_notificationHandlers.TryGetValue(msg.Method, out var value))
		{
			value(msg, header);
		}
	}

	private void HandleRelayMsg(PacketHeader header, byte[] payload)
	{
		Relay relay = MakeMsg<Relay>(payload, header.PayloadSize);
		if (_dynamicRelayHandlers.TryGetValue(relay.Method, out var value))
		{
			value(relay.Data);
		}
	}
}
