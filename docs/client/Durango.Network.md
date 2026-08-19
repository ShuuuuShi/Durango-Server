# namespace `Durango.Network`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

9 ไฟล์

## `Durango.Network/Connection.cs`

987 บรรทัด

**class `Connection`** — บรรทัด 17–986

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 19 | `public delegate void MessageHandler<T>(T msg, PacketHeader header);` | public |
| 21 | `public delegate void PacketHandler(PacketHeader header, byte[] payload, int payloadOffset, object msg = null);` | public |
| 95 | `private readonly MessagePacking _messagePacker = new MessagePacking();` |  |
| 119 | `private readonly byte[] _compressingBuffer = new byte[SnappyCodec.GetMaxCompressedLength(262144)];` |  |
| 143 | `private readonly Queue<Packet> _packetQueue = new Queue<Packet>();` |  |
| 145 | `private readonly Queue<Relayed> _relayedQueue = new Queue<Relayed>();` |  |
| 147 | `private readonly HashSet<uint> _continuousReplies = new HashSet<uint>();` |  |
| 151 | `private readonly List<SyncedClock> _syncClockPackets = new List<SyncedClock>();` |  |
| 157 | `private SyncedTime _predictedServerTime = new SyncedTime();` |  |
| 159 | `private readonly SyncedTime _latestReceivedTime = new SyncedTime();` |  |
| 165 | `private readonly Dictionary<uint, PacketHandler> _packetHandlers = new Dictionary<uint, PacketHandler>();` |  |
| 167 | `private readonly Dictionary<uint, ReplyMessageHandler> _replyPacketHandlers = new Dictionary<uint, ReplyMessageHandler>();` |  |
| 177 | `public float Ping { get; private set; }` | public |
| 179 | `private byte[] SendBuffer => (_sendBufferIndex != 0) ? _socketSendBuffer2 : _socketSendBuffer1;` |  |
| 185 | `public Connection(bool timeSynchronize = false)` | public |
| 190 | `private static float MCeil(float num, float mul)` |  |
| 195 | `private void UpdateBufferTime(float ping)` |  |
| 206 | `private static float GetTime()` |  |
| 211 | `public void MaybeSendKeepalive()` | public |
| 221 | `public void ConnectAsync(string host, int port)` | public |
| 239 | `private void TryConnectAsync()` |  |
| 263 | `private void InitializeSocket(IPAddress addr, int port)` |  |
| 280 | `public void Close(bool callClosedHandler = true)` | public |
| 344 | `public bool Connected()` | public |
| 349 | `public bool IsAttemptingToConnect()` | public |
| 354 | `public void AddHook([NotNull] IConnectionHook hook)` | public |
| 363 | `public void RemoveHook([NotNull] IConnectionHook hook)` | public |
| 375 | `public ReplyMessageHandlerRegistrar Send<T>(T msg, bool noReply = false, uint replyOf = 0u)` | public |
| 417 | `public bool On<T>(MessageHandler<T> handler)` | public |
| 422 | `public bool RegisterReplyMessageHandler<T>(uint seq, MessageHandler<T> handler)` | public |
| 433 | `public void RegisterReplyMessageHandler(uint seq, Action<Packet> handler, bool allowReplied)` | public |
| 441 | `public void RegisterReplySequenceHandler(uint seq, Action<bool> handler)` | public |
| 448 | `public bool RegisterRelayHandler<T>(Action<T, float> handler)` | public |
| 463 | `public double GetBufferedServerTime()` | public |
| 468 | `public double GetPredictedServerTime()` | public |
| 473 | `public float CheckBufferedTimePassed(double baseTime)` | public |
| 479 | `public bool IsTimeSynchronized()` | public |
| 484 | `public void Handle(uint type, object msg, PacketHeader header)` | public |
| 492 | `public void PushPacket<T>(T msg, uint replyOf = 0u)` | public |
| 522 | `private void StartSend()` |  |
| 538 | `private void StartReceive()` |  |
| 547 | `private T MakeMsg<T>(byte[] payload, int payloadOffset, int payloadSize)` |  |
| 552 | `private static void SocketEventCompleted(object sender, SocketAsyncEventArgs e)` |  |
| 569 | `private void ConnectCompleted(SocketAsyncEventArgs e)` |  |
| 580 | `private void SendCompleted(SocketAsyncEventArgs e)` |  |
| 592 | `private void ReceiveCompleted(SocketAsyncEventArgs e)` |  |
| 614 | `private void ReceiveProcess(SocketAsyncEventArgs e)` |  |
| 664 | `private static void ExtendBuffer(ref byte[] targetBuffer, int extendSize)` |  |
| 671 | `private static bool TryGetTypeCode(Type type, out uint typeCode)` |  |
| 683 | `private bool RegisterMessageHandlerToRegistry<T>(IDictionary<uint, PacketHandler> registry, MessageHandler<T> handler)` |  |
| 703 | `private void ClockMessageHandler(Clock pass, PacketHeader header)` |  |
| 721 | `private bool NeedTimeSynchronization()` |  |
| 726 | `public void ForceSyncClock()` | public |
| 731 | `private void SendGetClock()` |  |
| 743 | `private void PingClockMessageHandler(Clock pass, PacketHeader header)` |  |
| 758 | `private void SendGetClockForCheckPing()` |  |
| 766 | `public void Process()` | public |
| 783 | `private void ProcessClock()` |  |
| 800 | `private void ProcessPacketQueue()` |  |
| 824 | `private void ProcessPacket(Packet packet)` |  |
| 863 | `private void HandleMsg(Packet packet)` |  |
| 906 | `private void ProcessRelayedQueue()` |  |
| 923 | `private void ProcessReceiveSend()` |  |
| 949 | `private void CheckConnectionSucceed()` |  |
| 969 | `public static bool TryParse(string address, out string host, out int port)` | public |

   **class `Relayed`** — บรรทัด 23–28

   **class `SyncedTime`** — บรรทัด 30–76

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 62 | `public double LastGameTimeAtSynced { get; private set; }` | public |
   | 64 | `public void Reset()` | public |
   | 72 | `public void SetSyncDirty()` | public |

   **class `SyncedClock`** — บรรทัด 78–83

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 82 | `public readonly SyncedTime PredictedServerTime = new SyncedTime();` | public |

---

## `Durango.Network/Connections.cs`

42 บรรทัด

**class `Connections`** — บรรทัด 3–41

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 33 | `static Connections()` |  |

---

## `Durango.Network/EntityType.cs`

14 บรรทัด

**enum `EntityType`** — บรรทัด 3

---

## `Durango.Network/IConnectionHook.cs`

7 บรรทัด

**interface `IConnectionHook`** — บรรทัด 3–6

---

## `Durango.Network/MotionOption.cs`

18 บรรทัด

**enum `MotionOption`** — บรรทัด 6

---

## `Durango.Network/Packet.cs`

101 บรรทัด

**struct `Packet`** — บรรทัด 8–100

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 18 | `public static T DeserializeMsg<T>(byte[] payload, int payloadOffset, int payloadSize, byte[] decompressingBuffer, MessagePacking packer)` | public |
| 27 | `public static int SerializeMsg<T>(double time, uint seq, uint replyOf, T msg, byte[] dstBuffer, int dstOffset, byte[] packingBuffer, byte[] compressingBuffer, MessagePacking messagePacker)` | public |
| 42 | `public static int WritePacketHeader(double time, uint seq, uint replyOf, uint type, int payloadSize, byte[] buffer, int offset)` | public |
| 52 | `public static PacketHeader ReadPacketHeader(byte[] bytes, int remainBytes, int offset)` | public |
| 72 | `private static void WriteUlong(ulong value, byte[] bytes, int offset)` |  |
| 80 | `private static void WriteUint(uint value, byte[] bytes, int offset)` |  |
| 88 | `public static bool IsSuccess(Packet packet)` | public |

---

## `Durango.Network/PacketHeader.cs`

17 บรรทัด

**struct `PacketHeader`** — บรรทัด 3–16

---

## `Durango.Network/ReplyMessageHandler.cs`

16 บรรทัด

**struct `ReplyMessageHandler`** — บรรทัด 6–15

---

## `Durango.Network/ReplyMessageHandlerRegistrar.cs`

77 บรรทัด

**struct `ReplyMessageHandlerRegistrar`** — บรรทัด 5–76

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public ReplyMessageHandlerRegistrar(Connection connection, uint seq)` | public |
| 19 | `public ReplyMessageHandlerRegistrar On<T>(Connection.MessageHandler<T> messageHandler)` | public |
| 28 | `public ReplyMessageHandlerRegistrar All(Action<Packet> handler)` | public |
| 37 | `public ReplyMessageHandlerRegistrar Rest(Action<Packet> handler)` | public |
| 46 | `public ReplyMessageHandlerRegistrar OnSequence(Action<bool> handler)` | public |
| 55 | `public ReplyMessageHandlerRegistrar Ignore<T>()` | public |
| 62 | `public bool IsEmpty()` | public |
| 67 | `public static bool operator true(ReplyMessageHandlerRegistrar r)` | public |
| 72 | `public static bool operator false(ReplyMessageHandlerRegistrar r)` | public |

---
