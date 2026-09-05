using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Durango.Network;
using Durango.Offline;
using Durango.Utils;
using Messages;

namespace DurangoTestClient;

/// <summary>
/// เทสพอร์ตแชท radiotower หลังเปิดใช้งานจริง (--radiotower / --radiotower-port)
///
/// เดิมพอร์ตนี้ไม่มี auth เลย (M-5) — ใครต่อเข้ามาก็ Tune แล้วพูดแทนใครก็ได้
/// และไม่เติมชื่อคนพูด (GP-05) แชทจึงขึ้นเป็นข้อความลอย ๆ
///
/// เทส:
///  1. Tune ด้วย session token จริง → ได้ Conversations กลับมา
///  2. Tune ด้วย token มั่ว → ถูกปฏิเสธ (ไม่ได้ Conversations) และถูกตัดการเชื่อมต่อ
///  3. ยังไม่ Tune แล้วพูดเลย → ข้อความไม่ถูกส่งต่อให้ใคร
///  4. พูดแล้วคนอื่นในห้องได้ยิน และข้อความมี Speaker.Name เป็นชื่อจริงจาก session
///  5. ปลอม EntityId ในข้อความ → server เขียนทับด้วย id จริงของ session
///
/// รัน: dotnet run -- --radiotower-check [host] [port แชท] [port gateway]
/// </summary>
public static class RadiotowerCheck
{
    private static int _passed;
    private static int _failed;

    private static void Check(string name, bool ok, string detail = null)
    {
        if (ok) { _passed++; Console.WriteLine($"  [ผ่าน] {name}"); }
        else { _failed++; Console.WriteLine($"  [ตก ] {name}{(detail == null ? "" : " — " + detail)}"); }
    }

    private sealed class Peer
    {
        public string Id;
        public string Name;
        public Socket Sock;
        public Connection Conn;
        public int Conversations;
        public int Aborts;
        public readonly List<Message_> Heard = new List<Message_>();

        public void Reset() { Conversations = 0; Aborts = 0; Heard.Clear(); }
    }

    private static void PumpAll(List<Peer> ps, int ms)
    {
        for (int i = 0; i < ms / 10; i++)
        {
            for (int p = 0; p < ps.Count; p++)
            {
                try { ps[p].Conn.Process(); } catch (Exception) { }
            }
            Thread.Sleep(10);
        }
    }

    private static Peer Open(string host, int chatPort, string id, string name)
    {
        var p = new Peer { Id = id, Name = name };
        p.Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        p.Sock.Connect(host, chatPort);
        p.Conn = new Connection(p.Sock);
        p.Conn.Recv<Conversations>((m, h) => p.Conversations++);
        p.Conn.Recv<Abort>((m, h) => p.Aborts++);
        p.Conn.Recv<SayInExclusiveChannel>((m, h) => p.Heard.Add(m.Message));
        p.Conn.Recv<SayInConversation>((m, h) => p.Heard.Add(m.Message));
        p.Conn.StartReceive();
        return p;
    }

    public static int Run(string host, int chatPort, int gatewayPort)
    {
        _passed = _failed = 0;
        Console.WriteLine($"=== radiotower check: {host}:{chatPort} (gateway {gatewayPort}) ===");

        string modelInfo =
            "{\"hair\":\"hair_f_01\",\"body_color\":[\"484E36\",\"F0D9B7\",\"29130D\"]," +
            "\"head_color\":[\"FF0000\",\"FFFFFF\",\"0000FF\"],\"skin_color\":\"F0D9B7\"," +
            "\"hair_color\":\"471513\",\"lip_color\":\"E88295\",\"eye_color\":\"52353F\"," +
            "\"portrait\":3,\"portrait_bg\":2,\"portrait_bg_color\":\"C5A293\",\"beard\":null," +
            "\"voice_type\":1,\"body_size\":1.0}";
        string suffix = Guid.NewGuid().ToString("N")[..6];
        string nameA = "chat-a-" + suffix;
        string nameB = "chat-b-" + suffix;

        string idA = CreateCharacterCheck.CreatePlayer(host, gatewayPort, nameA, isMale: false, modelInfo);
        string idB = CreateCharacterCheck.CreatePlayer(host, gatewayPort, nameB, isMale: false, modelInfo);
        if (string.IsNullOrEmpty(idA) || string.IsNullOrEmpty(idB))
        {
            Console.WriteLine("สร้างตัวละครสำหรับเทสแชทไม่ได้");
            return 2;
        }
        string tokenA = SessionClient.FetchRaw(host, gatewayPort, "{\"appear_player\":{\"entity_id\":\"" + idA + "\"}}");
        string tokenB = SessionClient.FetchRaw(host, gatewayPort, "{\"appear_player\":{\"entity_id\":\"" + idB + "\"}}");
        if (string.IsNullOrEmpty(tokenA) || string.IsNullOrEmpty(tokenB))
        {
            Console.WriteLine("ขอ session token ไม่ได้");
            return 2;
        }

        Peer a, b, fake, silent;
        try
        {
            a = Open(host, chatPort, idA, nameA);
            b = Open(host, chatPort, idB, nameB);
            fake = Open(host, chatPort, idA, nameA);
            silent = Open(host, chatPort, idB, nameB);
        }
        catch (Exception e)
        {
            Console.WriteLine($"ต่อพอร์ตแชท {host}:{chatPort} ไม่ได้: {e.Message}");
            Console.WriteLine("เปิดเซิร์ฟด้วย --radiotower-port <พอร์ต> หรือยัง");
            return 2;
        }
        var all = new List<Peer> { a, b, fake, silent };

        // 1) Tune ด้วย token จริง
        a.Conn.Send(new Tune { EntityId = idA, SessionToken = tokenA, SyncedAt = 0.0 });
        b.Conn.Send(new Tune { EntityId = idB, SessionToken = tokenB, SyncedAt = 0.0 });
        PumpAll(all, 900);
        Check("Tune ด้วย session token จริง → ได้ Conversations", a.Conversations >= 1 && b.Conversations >= 1,
            $"a={a.Conversations} b={b.Conversations}");

        // 2) Tune ด้วย token มั่ว
        fake.Conn.Send(new Tune { EntityId = idA, SessionToken = "token-มั่ว-ไม่มีจริง", SyncedAt = 0.0 });
        PumpAll(all, 900);
        Check("Tune ด้วย token มั่ว → ไม่ได้ Conversations", fake.Conversations == 0, $"conv={fake.Conversations}");
        Check("Tune ด้วย token มั่ว → ถูกบอกเหตุผลแล้วตัดการเชื่อมต่อ",
            fake.Aborts >= 1 || !fake.Conn.Connected(), $"abort={fake.Aborts} connected={fake.Conn.Connected()}");

        // 3) ยังไม่ Tune แล้วพูดเลย — ห้ามถึงใคร
        a.Reset(); b.Reset();
        silent.Conn.Send(new SayInExclusiveChannel
        {
            Message = new Message_ { EntityId = idB, Body = new RadioTalk { Text = "ข้อความจากคนที่ยังไม่ Tune" }, Time = Times.UnixTimeNow() },
            ChannelType = Shared.Chat.ChannelType.Region
        });
        PumpAll(all, 900);
        Check("ยังไม่ Tune แล้วพูด → ไม่มีใครได้ยิน", a.Heard.Count == 0 && b.Heard.Count == 0,
            $"a={a.Heard.Count} b={b.Heard.Count}");

        // 4) พูดจริง — คนอื่นได้ยิน และมีชื่อคนพูด
        a.Reset(); b.Reset();
        a.Conn.Send(new SayInExclusiveChannel
        {
            Message = new Message_ { EntityId = idA, Body = new RadioTalk { Text = "สวัสดีจากพอร์ตแชท" }, Time = Times.UnixTimeNow() },
            ChannelType = Shared.Chat.ChannelType.Region
        });
        PumpAll(all, 900);
        Check("พูดแล้วคนอื่นในห้องได้ยิน", b.Heard.Count >= 1, $"b={b.Heard.Count}");
        Check("ผู้พูดได้ยินข้อความตัวเองด้วย (client ไม่ได้เติม log เอง)", a.Heard.Count >= 1, $"a={a.Heard.Count}");
        if (b.Heard.Count > 0)
        {
            Message_ m = b.Heard[^1];
            Check("ข้อความมีชื่อคนพูด (GP-05)",
                m.Speaker.HasValue && m.Speaker.Value.Name == nameA,
                $"speaker={(m.Speaker.HasValue ? m.Speaker.Value.Name : "(ไม่มี)")} ควรเป็น {nameA}");
            Check("ข้อความถึงครบ ไม่โดนตัด", (m.Body is RadioTalk rt) && rt.Text == "สวัสดีจากพอร์ตแชท", $"body={m.Body}");
        }

        // 5) ปลอม EntityId ในข้อความ → ต้องถูกเขียนทับด้วยของจริง
        a.Reset(); b.Reset();
        Thread.Sleep(600);   // เลี่ยง cooldown กันสแปม
        a.Conn.Send(new SayInExclusiveChannel
        {
            Message = new Message_ { EntityId = idB, Body = new RadioTalk { Text = "ปลอมเป็นคนอื่น" }, Time = Times.UnixTimeNow() },
            ChannelType = Shared.Chat.ChannelType.Region
        });
        PumpAll(all, 900);
        if (b.Heard.Count > 0)
        {
            Message_ m = b.Heard[^1];
            Check("ปลอม EntityId ไม่ได้ — server เขียนทับด้วย id ของ session",
                m.EntityId == idA && m.Speaker.HasValue && m.Speaker.Value.Name == nameA,
                $"entityId={m.EntityId} speaker={(m.Speaker.HasValue ? m.Speaker.Value.Name : "-")}");
        }
        else
        {
            Check("ปลอม EntityId ไม่ได้ — server เขียนทับด้วย id ของ session", false, "ไม่ได้ยินข้อความเลย");
        }

        // 6) แชทส่วนตัว (SayInConversation) เดินทางเหมือนกัน
        a.Reset(); b.Reset();
        Thread.Sleep(600);
        a.Conn.Send(new SayInConversation
        {
            Message = new Message_ { EntityId = idA, Body = new RadioTalk { Text = "แชทส่วนตัว" }, Time = Times.UnixTimeNow() },
            ConversationId = "conv-1"
        });
        PumpAll(all, 900);
        Check("SayInConversation ส่งถึงและมีชื่อคนพูด",
            b.Heard.Count >= 1 && b.Heard[^1].Speaker.HasValue && b.Heard[^1].Speaker.Value.Name == nameA,
            $"heard={b.Heard.Count}");

        // 7) ข้อความยาวเกินเพดานต้องถูกตัด (เดิมอ่าน Body as string ⇒ เพดานไม่เคยทำงาน)
        a.Reset(); b.Reset();
        Thread.Sleep(600);
        a.Conn.Send(new SayInExclusiveChannel
        {
            Message = new Message_ { EntityId = idA, Body = new RadioTalk { Text = new string('ก', 500) }, Time = Times.UnixTimeNow() },
            ChannelType = Shared.Chat.ChannelType.Region
        });
        PumpAll(all, 900);
        Check("ข้อความยาว 500 ตัวถูกตัดเหลือ 200",
            b.Heard.Count >= 1 && b.Heard[^1].Body is RadioTalk longTalk && longTalk.Text.Length == 200,
            $"len={(b.Heard.Count > 0 && b.Heard[^1].Body is RadioTalk t2 ? t2.Text.Length : -1)}");

        // 8) ข้อความว่างต้องไม่ถูกส่งต่อ
        a.Reset(); b.Reset();
        Thread.Sleep(600);
        a.Conn.Send(new SayInExclusiveChannel
        {
            Message = new Message_ { EntityId = idA, Body = new RadioTalk { Text = "   " }, Time = Times.UnixTimeNow() },
            ChannelType = Shared.Chat.ChannelType.Region
        });
        PumpAll(all, 900);
        Check("ข้อความว่างไม่ถูกส่งต่อ", b.Heard.Count == 0, $"heard={b.Heard.Count}");

        Console.WriteLine($"สรุป: ผ่าน {_passed} · ตก {_failed}");
        for (int i = 0; i < all.Count; i++) { try { all[i].Conn.Close(); } catch (Exception) { } }
        return _failed == 0 ? 0 : 1;
    }
}
