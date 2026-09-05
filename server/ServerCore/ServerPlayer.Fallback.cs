using System;
using System.Collections.Generic;
using System.Reflection;
using Durango.Network;
using Messages;

namespace DurangoServer.Core;

// ============================================================================
// ServerPlayer.Fallback — "รับให้ได้ทุกแพ็กเก็ต" (4 ก.ย. 2026, เจ้าของสั่ง)
//
// ตัวเกมส่ง message หาเซิร์ฟ ~280 ชนิด แต่เซิร์ฟลงทะเบียนรับได้ ~250 ⇒ ที่เหลือถูกทิ้งเงียบ ๆ
// ผลคือปุ่มบางปุ่มกดแล้วไม่เกิดอะไร หรือ client ค้างรอคำตอบที่ไม่มีวันมา
//
// ไฟล์นี้ทำ 2 ชั้น:
//   1) handler เบา ๆ สำหรับ message ที่ตอบง่ายและไม่มีผลข้างเคียง (ตอบ OK / ตอบลิสต์ว่าง / no-op)
//      ลงทะเบียน **เฉพาะที่ยังไม่มี handler จริง** (RecvIfMissing) — ของจริงที่เพิ่มทีหลังจะทับตัวนี้เอง
//   2) ตัวจับท้ายสุด (Connection.OnUnhandled): message ที่ไม่มีใครรับเลย ⇒ ตอบ Abort พร้อมข้อความไทย
//      ตาม Seq ที่ client ส่งมา (client เปิดกล่องข้อความแล้วเลิกรอ) + นับสถิติไว้ดูที่ /admin/status
//
// ดูรายการที่ยังไม่มีของจริง: docs/server/protocol-gap.json · แผน: docs/Plan/BACKEND-MMO-PROPOSAL.md
// ============================================================================

public partial class ServerPlayer
{
    /// <summary>TypeCode → ชื่อ message (จาก assembly Messages) ไว้พิมพ์ log/ข้อความให้คนอ่านรู้เรื่อง</summary>
    private static readonly Dictionary<uint, string> MessageNames = BuildMessageNames();

    private static Dictionary<uint, string> BuildMessageNames()
    {
        var map = new Dictionary<uint, string>();
        try
        {
            foreach (Type t in typeof(Abort).Assembly.GetTypes())
            {
                if (t.Namespace != "Messages") { continue; }
                FieldInfo f = t.GetField("TypeCode", BindingFlags.Static | BindingFlags.Public);
                if (f == null || !f.IsLiteral) { continue; }
                uint code = (uint)f.GetValue(null);
                map[code] = t.Name;
            }
        }
        catch (ReflectionTypeLoadException e)
        {
            Console.WriteLine("[fallback] อ่านชื่อ message ได้ไม่ครบ: " + e.Message);
        }
        return map;
    }

    /// <summary>ชื่อ message จาก TypeCode (ไม่รู้จัก = ตัวเลข)</summary>
    public static string MessageNameOf(uint typeCode)
    {
        return MessageNames.TryGetValue(typeCode, out string n) ? n : ("type " + typeCode);
    }

    /// <summary>ลงทะเบียน handler ถ้ายังไม่มีของจริง — ไม่ทับของที่ไฟล์อื่นลงไว้แล้ว</summary>
    private void RecvIfMissing<T>(Durango.Offline.Connection.MessageHandler<T> handler)
    {
        FieldInfo f = typeof(T).GetField("TypeCode", BindingFlags.Static | BindingFlags.Public);
        if (f == null || !f.IsLiteral) { return; }
        if (_conn.HasHandler((uint)f.GetValue(null))) { return; }
        _conn.Recv(handler);
    }

    /// <summary>ตอบ OK เฉย ๆ — ใช้กับคำสั่ง "ตั้งค่า/ติ๊ก" ที่เซิร์ฟยังไม่เก็บค่า แต่ client แค่รอ ack</summary>
    private void AckOnly<T>(string note) where T : struct
    {
        RecvIfMissing<T>(delegate(T msg, PacketHeader header)
        {
            Send(default(OK), header.Seq);
        });
    }

    /// <summary>รับแล้วเงียบ — message แจ้งเหตุจาก client ที่ไม่ต้องการคำตอบ</summary>
    private void Ignore<T>() where T : struct
    {
        RecvIfMissing<T>(delegate(T msg, PacketHeader header) { });
    }

    /// <summary>
    /// เรียกท้ายสุดของ RegisterHandlers() — ต้องอยู่หลังทุก Register* เพื่อให้ RecvIfMissing เห็นของจริงครบ
    /// </summary>
    private void RegisterFallbackHandlers()
    {
        // ── 1) ตอบง่าย ไม่มีผลข้างเคียง ────────────────────────────────────────
        // keepalive ของเกมเดิม: client ยิงเป็นระยะแล้วรอตัวเดิมตอบกลับ (เดิมไม่ตอบ ⇒ client นับว่าเงียบ)
        RecvIfMissing<Keepalive>(delegate(Keepalive msg, PacketHeader header)
        {
            Send(default(Keepalive), header.Seq);
        });

        // ประวัติแชทล่าสุด — แชทวิ่งบน radiotower ไม่ได้เก็บ log ฝั่งเกม ⇒ ลิสต์ว่าง (client ไม่ค้าง)
        RecvIfMissing<GetLatestChatLog>(delegate(GetLatestChatLog msg, PacketHeader header)
        {
            Send(new ChatLogs { Logs = Array.Empty<Message_>() }, header.Seq);
        });

        // ดื่มน้ำ/ล้างตัวจากแหล่งน้ำ — ใช้บัพ drink_water ที่มีอยู่แล้ว (ลดความล้าหมวดร้อน 180 วิ)
        RecvIfMissing<DrinkWater>(delegate(DrinkWater msg, PacketHeader header)
        {
            ApplyDrinkWater();
            Send(default(OK), header.Seq);
        });
        AckOnly<WashBody>("ล้างตัว — ยังไม่มีค่าความสกปรกฝั่งเซิร์ฟ");

        // ตั้งค่า/ติ๊กถูกใจ — เซิร์ฟยังไม่เก็บ แต่ client แค่รอ ack
        AckOnly<SetSocialOptions>("ตัวเลือกโซเชียล");
        AckOnly<SetTimelineOption>("ตัวเลือกไทม์ไลน์");
        AckOnly<SetRecipeLike>("ถูกใจสูตร");
        AckOnly<SetBlueprintLike>("ถูกใจพิมพ์เขียว");
        AckOnly<ToggleConversationNotification>("แจ้งเตือนบทสนทนา");
        AckOnly<ToggleStatusEffect>("เปิด/ปิดไอคอนสถานะ");
        AckOnly<GiveUpDistribution>("ยกเลิกแจกของ");

        // แคลนยังไม่เปิด (เลื่อนหลัง S8) — client ถามตอนเข้าเกมทุกครั้ง ตอบ ack ให้เลิกรอ
        AckOnly<GetClanNotificationEnabled>("แคลน");
        AckOnly<ToggleClanNotification>("แคลน");
        AckOnly<ResubscribeClanChannel>("แคลน");

        // แจ้งเหตุจาก client — ไม่ต้องตอบ
        Ignore<PlayerDrawLine>();
        Ignore<ParticleEffect>();
        Ignore<EngagementAgreementChanged>();
        Ignore<Weather>();

        // ── 2) ตัวจับท้ายสุด: อะไรที่ไม่มีใครรับ ตอบ Abort ตาม Seq ให้ client เลิกรอ ──────
        _conn.OnUnhandled = delegate(PacketHeader header, byte[] payload)
        {
            string name = MessageNameOf(header.TypeCode);
            bool first;
            lock (_unhandledSeen)
            {
                first = _unhandledSeen.Add(header.TypeCode);
            }
            if (first)
            {
                Console.WriteLine("[fallback] {0} ส่ง {1} ({2}) ที่เซิร์ฟยังไม่รองรับ — ตอบ Abort", Name, name, header.TypeCode);
            }
            if (header.Seq != 0)
            {
                Send(new Abort { Text = "ระบบนี้ยังไม่เปิดในรุ่นนี้ (" + name + ")" }, header.Seq);
            }
        };
    }

    /// <summary>ชนิด message ที่คนนี้เคยส่งมาแล้วเซิร์ฟไม่รองรับ — log ครั้งแรกครั้งเดียวต่อคน</summary>
    private readonly HashSet<uint> _unhandledSeen = new HashSet<uint>();
}
