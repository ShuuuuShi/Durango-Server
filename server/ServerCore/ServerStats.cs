using System;

namespace DurangoServer.Core;

// ============================================================================
// ServerStats — สถานะรวมของเซิร์ฟ (tps/RAM/uptime) เก็บไว้ใน memory
//
// ทำไมต้องมี: เดิมค่าพวกนี้แค่ Console.WriteLine ทิ้งไปทุก 30 วิ (`[loop] ... tps ...`
// ใน Program.cs) ไม่มีที่เก็บให้ใครอ่านได้นอกจาก scroll console เอง
// ตอนนี้ Program.cs main loop เรียก Update() เป็นระยะ (ดู StatsUpdateIntervalSeconds)
// แล้ว admin web panel (Gateway /admin/status) มาอ่านค่าที่เก็บไว้ตรงนี้ผ่าน HTTP แทนที่จะ
// parse log — ตัวเลขจึงเป็นของจริงจากเซิร์ฟที่รันอยู่ ไม่ใช่ค่าจากไฟล์เซฟที่อาจ stale
// ============================================================================

public static class ServerStats
{
    private static readonly DateTime _startedAtUtc = DateTime.UtcNow;

    /// <summary>เวลาที่ process นี้เริ่มทำงาน (UTC)</summary>
    public static DateTime StartedAtUtc => _startedAtUtc;

    /// <summary>รันมาแล้วกี่วินาที</summary>
    public static double UptimeSeconds => (DateTime.UtcNow - _startedAtUtc).TotalSeconds;

    /// <summary>tps เฉลี่ยตั้งแต่ครั้งก่อนหน้าที่ Update ถูกเรียก</summary>
    public static double Tps { get; private set; }

    public static int OnlinePlayers { get; private set; }

    public static int AliveAnimals { get; private set; }

    public static int CorpseAnimals { get; private set; }

    /// <summary>RAM ที่ .NET GC ถืออยู่ (MB) — ดูจาก GC.GetTotalMemory(false)</summary>
    public static long RamMb { get; private set; }

    /// <summary>เวลาที่ Update() ถูกเรียกครั้งล่าสุด (UTC) — panel ใช้เช็คว่าเซิร์ฟยังตอบสนองอยู่ไหม</summary>
    public static DateTime LastUpdatedUtc { get; private set; } = DateTime.UtcNow;

    public static void Update(double tps, int onlinePlayers, int aliveAnimals, int corpseAnimals)
    {
        Tps = tps;
        OnlinePlayers = onlinePlayers;
        AliveAnimals = aliveAnimals;
        CorpseAnimals = corpseAnimals;
        RamMb = GC.GetTotalMemory(false) / 1048576;
        LastUpdatedUtc = DateTime.UtcNow;
    }
}
