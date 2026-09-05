using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Durango.Network;
using Durango.Offline;
using Durango.Utils;
using Messages;

namespace DurangoServer.Core;

// ============================================================================
// DurangoServer — ไฟล์หลักของ server
// ประกอบด้วย: ServerWorld (โลก), ServerPlayer (ผู้เล่น + handler เกมเพลย์),
// GameServer (TCP 8191), Gateway (HTTP 8190 + UDP knock), RadiotowerServer (แชท 8192)
// โปรโตคอล: MsgPack + Snappy, header 24 ไบต์ (time/seq/replyOf/typeCode/size)
// ============================================================================

// RadiotowerServer — ดูรายละเอียดที่ docs/server/RadiotowerServer.md

public class RadiotowerServer
{
    public const int DefaultPort = 8192;

    /// <summary>ยาวสุดของข้อความแชท (ตรงกับ ServerPlayer.MaxChatLength)</summary>
    private const int MaxChatLength = 200;

    /// <summary>เว้นระยะระหว่างข้อความของคนเดียวกัน (วินาที)</summary>
    private const double ChatCooldown = 0.5;

    /// <summary>พอร์ตที่เปิดฟังจริง</summary>
    public int Port { get; private set; } = DefaultPort;

    private readonly Listener _listener = new Listener();
    private readonly List<Client> _clients = new List<Client>();
    private readonly object _connLock = new object();

    /// <summary>
    /// M-5: ต้องมี GameServer เพื่อตรวจ session token ที่ client ส่งมากับ Tune
    /// null = โหมดเก่า (ไม่มี auth) — เหลือไว้ให้เทสเฉพาะในเครื่องเท่านั้น
    /// </summary>
    private readonly GameServer _gameServer;

    public RadiotowerServer(GameServer gameServer = null)
    {
        _gameServer = gameServer;
    }

    /// <summary>1 connection = 1 คน; ยังไม่ผ่าน Tune = พูดไม่ได้</summary>
    private sealed class Client
    {
        public Durango.Offline.Connection Conn;
        public string EntityId;
        public string Name;
        public bool Authed;
        public double LastChatAt;
        public double ConnectedAt;
    }

    /// <summary>เปิดฟังพอร์ตแชท คืน false ถ้า bind ไม่สำเร็จ (GP-15)</summary>
    public bool Start(int port)
    {
        if (!_listener.Start(port))
        {
            return false;
        }
        Port = port;
        _listener.ClientAccepted += ClientAccepted;
        Console.WriteLine($"[radiotower] listening on 0.0.0.0:{port}"
            + (_gameServer != null ? " (ตรวจ session token)" : " ⚠️ ไม่มี auth"));
        return true;
    }

    public void Close()
    {
        _listener.ClientAccepted -= ClientAccepted;
        _listener.Close();
        Client[] snapshot;
        lock (_connLock)
        {
            snapshot = _clients.ToArray();
            _clients.Clear();
        }
        for (int i = 0; i < snapshot.Length; i++)
        {
            try { snapshot[i].Conn.Close(); } catch (Exception e) { Console.WriteLine($"[radiotower] ปิด connection ไม่สำเร็จ: {e.Message}"); }
        }
    }

    private void ClientAccepted(Socket socket)
    {
        Durango.Offline.Connection connection = new Durango.Offline.Connection(socket);
        Client client = new Client
        {
            Conn = connection,
            ConnectedAt = Times.UnixTimeNow()
        };
        lock (_connLock)
        {
            _clients.Add(client);
        }
        connection.Recv<Tune>(delegate(Tune tune, PacketHeader header)
        {
            if (!Authorize(client, tune))
            {
                Console.WriteLine($"[radiotower] ปฏิเสธ Tune ของ {tune.EntityId ?? "(ไม่ระบุ)"}: token ไม่ถูกต้อง");
                try { connection.Send(Aborts.Reason("session token ไม่ถูกต้อง — ต่อแชทไม่ได้"), header.Seq); } catch (Exception) { }
                try { connection.Close(); } catch (Exception) { }
                return;
            }
            Console.WriteLine($"[radiotower] {client.Name} ({client.EntityId}) เข้าห้องแชทแล้ว");
            connection.Send(new Conversations { _Conversations = null }, header.Seq);
        });
        connection.Recv<SayInExclusiveChannel>(delegate(SayInExclusiveChannel msg, PacketHeader header)
        {
            if (!AcceptChat(client, ref msg.Message)) return;
            msg.Message = StampSpeaker(client, msg.Message);
            Console.WriteLine("[chat] {0}: {1}", client.Name, ChatBody.ReadText(msg.Message.Body) ?? "(ไม่ใช่ข้อความ)");
            Broadcast(msg);
        });
        connection.Recv<SayInConversation>(delegate(SayInConversation msg, PacketHeader header)
        {
            if (!AcceptChat(client, ref msg.Message)) return;
            msg.Message = StampSpeaker(client, msg.Message);
            Console.WriteLine("[chat-conv] {0}: {1}", client.Name, ChatBody.ReadText(msg.Message.Body) ?? "(ไม่ใช่ข้อความ)");
            Broadcast(msg);
        });
        // [4 ก.ย. 2026] พอร์ตแชทก็ต้อง "รับให้ได้ทุกแพ็กเก็ต" เหมือนพอร์ตเกม — client ยิง Keepalive /
        // GetLatestChatLog / คำถามแคลน มาทางนี้ (เจอจริงตอนเทสในเครื่อง: UnhandledCounts type 254/25/4027/24)
        connection.Recv<Keepalive>(delegate(Keepalive msg, PacketHeader header)
        {
            connection.Send(default(Keepalive), header.Seq);
        });
        connection.Recv<GetLatestChatLog>(delegate(GetLatestChatLog msg, PacketHeader header)
        {
            connection.Send(new ChatLogs { Logs = Array.Empty<Message_>() }, header.Seq);
        });
        connection.Recv<GetClanNotificationEnabled>(delegate(GetClanNotificationEnabled msg, PacketHeader header) { connection.Send(default(OK), header.Seq); });
        connection.Recv<ToggleClanNotification>(delegate(ToggleClanNotification msg, PacketHeader header) { connection.Send(default(OK), header.Seq); });
        connection.Recv<ResubscribeClanChannel>(delegate(ResubscribeClanChannel msg, PacketHeader header) { connection.Send(default(OK), header.Seq); });
        connection.Recv<ToggleConversationNotification>(delegate(ToggleConversationNotification msg, PacketHeader header) { connection.Send(default(OK), header.Seq); });
        connection.OnUnhandled = delegate(PacketHeader header, byte[] payload)
        {
            string name = ServerPlayer.MessageNameOf(header.TypeCode);
            Console.WriteLine($"[radiotower] {client.Name ?? "?"} ส่ง {name} ({header.TypeCode}) ที่ยังไม่รองรับ — ตอบ Abort");
            if (header.Seq != 0)
            {
                try { connection.Send(new Abort { Text = "ระบบนี้ยังไม่เปิดในรุ่นนี้ (" + name + ")" }, header.Seq); } catch (Exception) { }
            }
        };
        connection.StartReceive();
        Console.WriteLine($"[radiotower] client connected from {socket.RemoteEndPoint}");
    }

    /// <summary>M-5: Tune ต้องยื่น token ที่ /sessions ออกให้ เหมือน Auth ของพอร์ตเกม</summary>
    private bool Authorize(Client client, Tune tune)
    {
        if (_gameServer == null)
        {
            // ไม่มี GameServer ให้ตรวจ (เทสในเครื่อง) — ยอมรับตามที่อ้างมา
            client.EntityId = string.IsNullOrEmpty(tune.EntityId) ? "unknown" : tune.EntityId;
            client.Name = client.EntityId;
            client.Authed = true;
            return true;
        }
        if (!_gameServer.TryAuthorizeChat(tune.SessionToken, tune.EntityId, out string entityId, out string name))
        {
            client.Authed = false;
            return false;
        }
        client.EntityId = entityId;
        client.Name = string.IsNullOrEmpty(name) ? entityId : name;
        client.Authed = true;
        return true;
    }

    /// <summary>
    /// กรองแชทก่อน broadcast — ต้อง Tune ผ่านก่อน, ตัดข้อความยาวเกิน, กันสแปม
    /// (โครงเดียวกับ ServerPlayer.AcceptChat — เงียบ ๆ ไม่ตอบกลับ เพราะ client ไม่ได้รอคำตอบอยู่แล้ว)
    /// </summary>
    private bool AcceptChat(Client client, ref Message_ message)
    {
        if (!client.Authed)
        {
            Console.WriteLine("[radiotower] ทิ้งข้อความจาก connection ที่ยังไม่ผ่าน Tune");
            return false;
        }
        if (!ServerConfig.Current.Features.Chat) return false;

        double now = Times.UnixTimeNow();
        // Body เป็น object เพราะ protocol เดิมใส่ได้ทั้งข้อความและ payload อย่างอื่น
        // ตัวเกมส่ง RadioTalk มา ไม่ใช่ string — อ่านผ่าน ChatBody (ดูหมายเหตุในไฟล์นั้น)
        string body = ChatBody.ReadText(message.Body);
        if (body == null)
        {
            if (now - client.LastChatAt < ChatCooldown) return false;
            client.LastChatAt = now;
            return true;
        }
        if (string.IsNullOrWhiteSpace(body)) return false;
        if (now - client.LastChatAt < ChatCooldown) return false;
        client.LastChatAt = now;
        if (body.Length > MaxChatLength)
        {
            message.Body = ChatBody.WriteText(message.Body, body.Substring(0, MaxChatLength));
        }
        return true;
    }

    /// <summary>GP-05: เดิมไม่เติมชื่อคนพูด แชทจึงขึ้นเป็นข้อความลอย ๆ ไม่รู้ว่าใครพูด</summary>
    private static Message_ StampSpeaker(Client client, Message_ message)
    {
        message.EntityId = client.EntityId;
        message.Speaker = new RadioId
        {
            Name = client.Name,
            Freq = 0
        };
        if (message.Time <= 0.0)
        {
            message.Time = Times.UnixTimeNow();
        }
        return message;
    }

    /// <summary>ส่งต่อให้เฉพาะคนที่ Tune ผ่านแล้ว</summary>
    private void Broadcast<T>(T msg) where T : struct
    {
        lock (_connLock)
        {
            for (int i = _clients.Count - 1; i >= 0; i--)
            {
                if (!_clients[i].Authed) continue;
                try
                {
                    _clients[i].Conn.Send(msg);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[radiotower] ส่งให้ {_clients[i].Name} ไม่สำเร็จ: {e.Message}");
                }
            }
        }
    }

    public void Process()
    {
        _listener.Process();
        double now = Times.UnixTimeNow();
        lock (_connLock)
        {
            for (int i = _clients.Count - 1; i >= 0; i--)
            {
                Client client = _clients[i];
                client.Conn.Process();
                if (!client.Conn.Connected())
                {
                    _clients.RemoveAt(i);
                    continue;
                }
                // ต่อเข้ามาแล้วไม่ Tune ภายใน 30 วิ = ไม่ใช่ client เกม ปิดทิ้งไม่ให้ค้างกินที่
                if (!client.Authed && now - client.ConnectedAt > 30.0)
                {
                    Console.WriteLine("[radiotower] ปิด connection ที่ไม่ Tune ภายใน 30 วิ");
                    try { client.Conn.Close(); } catch (Exception) { }
                    _clients.RemoveAt(i);
                }
            }
        }
    }
}
