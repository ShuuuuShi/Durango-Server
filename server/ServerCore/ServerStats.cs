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
        RecordSample();
    }

    // ── ประวัติย้อนหลัง ────────────────────────────────────────────────────
    // [4 ก.ย. 2026] หน้า admin เห็นแต่ "ค่าตอนนี้" ⇒ ไม่มีทางรู้ว่าเมื่อคืนคนพีคกี่คน
    // หรือ tps ตกตอนไหน (พอเปิดดูอีกที เหตุการณ์ผ่านไปแล้ว)
    // เก็บใน memory อย่างเดียว ตั้งใจ — หายตอนรีสตาร์ตไม่เป็นไร ไม่คุ้มที่จะเขียนดิสก์ทุกนาที
    // 720 จุด × ทุก 60 วิ = ย้อนหลังได้ 12 ชั่วโมง กินไม่กี่ KB

    public readonly struct Sample
    {
        public Sample(double atUnix, double tps, int players, long ramMb)
        {
            At = atUnix; Tps = tps; Players = players; RamMb = ramMb;
        }
        public double At { get; }
        public double Tps { get; }
        public int Players { get; }
        public long RamMb { get; }
    }

    private const int HistoryCapacity = 720;
    private const double HistoryIntervalSeconds = 60.0;
    private static readonly Sample[] _history = new Sample[HistoryCapacity];
    private static readonly object _historyLock = new object();
    private static int _historyCount;
    private static int _historyNext;
    private static DateTime _lastSampleUtc = DateTime.MinValue;

    private static void RecordSample()
    {
        DateTime now = DateTime.UtcNow;
        if ((now - _lastSampleUtc).TotalSeconds < HistoryIntervalSeconds)
        {
            return;
        }
        _lastSampleUtc = now;
        var s = new Sample(
            (now - DateTime.UnixEpoch).TotalSeconds,
            Math.Round(Tps, 1), OnlinePlayers, RamMb);
        lock (_historyLock)
        {
            _history[_historyNext] = s;
            _historyNext = (_historyNext + 1) % HistoryCapacity;
            if (_historyCount < HistoryCapacity) { _historyCount++; }
        }
    }

    /// <summary>ประวัติเรียงจากเก่าไปใหม่ (คัดลอกออกมาแล้ว ผู้เรียกถือต่อได้เลย)</summary>
    public static Sample[] History()
    {
        lock (_historyLock)
        {
            var outp = new Sample[_historyCount];
            int start = (_historyCount == HistoryCapacity) ? _historyNext : 0;
            for (int i = 0; i < _historyCount; i++)
            {
                outp[i] = _history[(start + i) % HistoryCapacity];
            }
            return outp;
        }
    }
}
