using System;
using System.Collections.Generic;
using Durango.Utils;
using Messages;
using Shared.Accelerator;

namespace DurangoServer.Core;

/// <summary>
/// รอยแยก/วาร์ปเรกเซเลอเรเตอร์ (blueprint "warp_accelerator") — กิจกรรม PvE ป้องกันคลื่นสัตว์
///
/// ประวัติ/บริบท (22 ส.ค. 2026): เดิมทีเข้าใจผิดว่ากลไกนี้ใช้ฟิลด์ <see cref="Messages.Crack"/>
/// (ระบบ "ลงทุน" หินนำทางที่ ArtifactInteractions.Invest() ฝั่ง client เรียกใช้) แต่ตรวจ
/// RecipeData.BlueprintComponents แล้วพบว่า "Crack" ผูกกับ blueprint "crack_01"/"aqua_crack_01"
/// (ก้อนหินธรรมชาติ) เท่านั้น — ส่วน "warp_accelerator" ผูกกับ component "WarpAccelerator" ต่างหาก
/// ซึ่งมีฟิลด์ state ของตัวเองอยู่แล้วคือ <see cref="Messages.WarpAccelerator"/> (ArtifactState.Warpaccelerator)
/// พร้อม Interaction โค้ดของตัวเอง (client/InteractionData/Interaction.cs):
///   Accelerate = 670 (เริ่มกิจกรรมใหม่ตอนรอยแยกว่าง) · ParticipateAcceleration = 672 (เข้าร่วมรอบที่กำลังไป)
///   · ReceiveAccelerationRewards = 671 (กดรับรางวัลตอนผ่านครบ)
///
/// **state machine** (ตาม enum Shared.Accelerator.AcceleratorStatus):
///   RiftInactivated (ว่าง/รอ cooldown)
///     → Accelerate → Waiting (รอผู้เล่นมาสมทบ WaitSeconds วิ)
///     → Processing (เกิดสัตว์รอบจุด — ต้องฆ่าให้หมดภายใน PhaseSeconds วิ)
///       → ฆ่าหมดทัน + ยังไม่ครบทุกคลื่น → Intermission (พัก WaitSeconds วิ) → Processing (คลื่นถัดไป) ...
///       → ฆ่าหมดทัน + ครบทุกคลื่นแล้ว → End (มีเวลา RewardWindowSeconds วิ ให้กด "รับรางวัล")
///       → หมดเวลาแต่สัตว์เหลือ → ล้มเหลว กลับไป RiftInactivated ทันที (ไม่ได้รางวัลเลย)
///     → End หมดเวลาไม่มีใครมาเคลม → รีเซ็ตกลับ RiftInactivated (รางวัลที่เหลือหายไปเฉย ๆ)
///
/// ⚠️ **ขอบเขต MVP ที่ตัดไปก่อน** (บันทึกไว้ให้ทำต่อภายหลัง):
///   1. ไม่มีระบบกระเป๋าเงิน/Currency ในเซิร์ฟเลย (grep ยืนยันแล้ว) ⇒ ค่าธรรมเนียมเข้าร่วมตั้ง 0 เสมอ
///      (ดู WarpAcceleratorConfig.JoinCostAmount) — client ยังคงเห็น popup ยืนยันปกติ แค่ราคาคือ 0
///   2. รางวัล "Warp Matter" ไม่ได้ผูกกับระบบกระเป๋าเงินจริง (Wallet/WalletUpdated) — ใช้ตัวนับเดี่ยว ๆ
///      ต่อผู้เล่นแทน (ดู ServerPlayer.WarpAccelerator.cs: WarpMatterBalance + WeeklyWarpMatterAcquired)
///      ซึ่งพอสำหรับแสดงผล/สะสมได้จริง แต่ยังใช้ซื้ออะไรไม่ได้เพราะร้านค้า/ระบบแลกของก็ยังไม่มีเช่นกัน
///   3. คลื่นสัตว์สุ่มชนิด/เลเวลจาก SpawnTable เดียวกับสัตว์ทั่วไปบนเกาะ (ไม่ได้ทำตารางเฉพาะกิจกรรมนี้
///      หรือระบบ "PotentialBiocoms" ที่กำหนดล่วงหน้าว่าจะเจอสัตว์อะไรก่อนกด Accelerate)
///   4. ผู้เล่นที่ออฟไลน์ระหว่างกิจกรรม (ตัด connection) จะพลาดการรับรางวัลไปเฉย ๆ ถ้า RewardWindow
///      หมดเวลาก่อนกลับมาเล่น — ไม่มีระบบเก็บรางวัลค้างแบบ mailbox
///   5. ไม่มี AcceleratorStatus.RiftActivated เป็น state แยก — ข้ามจาก RiftInactivated ไป Waiting ตรง ๆ
///      ตอนกด Accelerate (RiftActivated ดูจากชื่อน่าจะเป็นสถานะเปลี่ยนผ่านสั้น ๆ ตอน "รอยแยกเปิด" ซึ่ง
///      ไม่มีข้อมูลอ้างอิงว่าต่างจาก Waiting ยังไงจริง ๆ เลยรวบเป็นสถานะเดียว)
/// </summary>
public sealed class WarpAcceleratorManager
{
    private static WarpAcceleratorConfig Cfg => ServerConfig.Current.WarpAccelerator;

    private readonly ServerWorld _world;
    private readonly object _lock = new object();
    private readonly Dictionary<string, Session> _sessions = new Dictionary<string, Session>();
    private readonly Random _rng = new Random();

    public WarpAcceleratorManager(ServerWorld world)
    {
        _world = world;
    }

    /// <summary>สถานะรันไทม์ของรอยแยกจุดหนึ่ง — ไม่เซฟลงดิสก์เลย (เหมือนสัตว์ใน AnimalSpawner)</summary>
    private sealed class Session
    {
        public WarpAccelerator State;

        /// <summary>RiftInactivated ต้องรอถึงเวลานี้ก่อนถึงจะ Accelerate รอบใหม่ได้</summary>
        public double CooldownUntil;

        /// <summary>id สัตว์ที่เกิดในคลื่นปัจจุบัน — ใช้เช็คว่าตายหมดหรือยัง</summary>
        public readonly List<string> WaveAnimalIds = new List<string>();

        /// <summary>Warp Matter ที่สะสมจากคลื่นที่ผ่านมาแล้วในรอบนี้ — โอนเข้าผู้เล่นจริงตอนเคลม (End เท่านั้น)</summary>
        public int PendingWarpMatter;

        /// <summary>คนที่เคลมรางวัลรอบนี้ไปแล้ว — กันกดรับซ้ำ</summary>
        public readonly HashSet<string> Claimed = new HashSet<string>(StringComparer.Ordinal);

        /// <summary>ค่า RemainAnimals ล่าสุดที่ broadcast ไปแล้ว — กัน broadcast รัวทุก tick ตอนเลขไม่เปลี่ยน</summary>
        public int LastBroadcastRemain = -1;
    }

    // ---------------------------------------------------------------- อ่านสถานะ (ใช้ตอนคำนวณเมนู Touch)

    public AcceleratorStatus GetStatus(string entityId)
    {
        lock (_lock)
        {
            return _sessions.TryGetValue(entityId ?? string.Empty, out Session s)
                ? s.State.Status
                : AcceleratorStatus.RiftInactivated;
        }
    }

    public bool IsParticipant(string entityId, string playerId)
    {
        lock (_lock)
        {
            if (_sessions.TryGetValue(entityId ?? string.Empty, out Session s) && s.State.Participants != null)
            {
                return Array.IndexOf(s.State.Participants, playerId) != -1;
            }
        }
        return false;
    }

    public bool HasUnclaimedReward(string entityId, string playerId)
    {
        lock (_lock)
        {
            if (_sessions.TryGetValue(entityId ?? string.Empty, out Session s)
                && s.State.Status == AcceleratorStatus.End
                && s.State.Participants != null
                && Array.IndexOf(s.State.Participants, playerId) != -1)
            {
                return !s.Claimed.Contains(playerId);
            }
        }
        return false;
    }

    // ---------------------------------------------------------------- Accelerate / Participate / รับรางวัล

    /// <summary>เริ่มรอบใหม่ที่รอยแยกนี้ (ต้องว่าง/พ้น cooldown) — คืน false + เหตุผลถ้าทำไม่ได้</summary>
    public bool TryAccelerate(string entityId, string playerId, out string reason)
    {
        lock (_lock)
        {
            double now = Durango.Utils.Times.UnixTimeNow();
            _sessions.TryGetValue(entityId ?? string.Empty, out Session s);
            if (s != null && s.State.Status != AcceleratorStatus.RiftInactivated)
            {
                reason = "รอยแยกนี้กำลังมีกิจกรรมอยู่แล้ว — กด \"เข้าร่วม\" แทน";
                return false;
            }
            if (s != null && now < s.CooldownUntil)
            {
                reason = "รอยแยกนี้เพิ่งใช้ไป ต้องรอสักครู่ก่อนเปิดใหม่";
                return false;
            }
            if (s == null)
            {
                s = new Session();
                _sessions[entityId] = s;
            }
            s.WaveAnimalIds.Clear();
            s.PendingWarpMatter = 0;
            s.Claimed.Clear();
            s.LastBroadcastRemain = -1;
            s.State = new WarpAccelerator
            {
                Status = AcceleratorStatus.Waiting,
                StatusSince = now,
                StatusUntil = now + Cfg.WaitSeconds,
                CurrentPhase = 1,
                CurrentWave = 1,
                CurrentMaxWave = Math.Max(1, Cfg.MaxPhase),
                RemainAnimals = null,
                Participants = new[] { playerId }
            };
            _world.SetArtifactWarpAccelerator(entityId, s.State);
            Console.WriteLine("[warp-accel] {0} เริ่มโดย {1} — รอ {2:F0} วิก่อนคลื่นแรก", entityId, playerId, Cfg.WaitSeconds);
            reason = null;
            return true;
        }
    }

    /// <summary>เข้าร่วมรอบที่กำลังดำเนินอยู่ (Waiting/Processing/Intermission)</summary>
    public bool TryParticipate(string entityId, string playerId, out string reason)
    {
        lock (_lock)
        {
            if (!_sessions.TryGetValue(entityId ?? string.Empty, out Session s)
                || s.State.Status == AcceleratorStatus.RiftInactivated
                || s.State.Status == AcceleratorStatus.End)
            {
                reason = "ยังไม่มีกิจกรรมกำลังดำเนินอยู่ที่นี่ — กด \"เร่งวาร์ป\" เพื่อเริ่มก่อน";
                return false;
            }
            string[] existing = s.State.Participants ?? Array.Empty<string>();
            if (Array.IndexOf(existing, playerId) != -1)
            {
                reason = "เข้าร่วมอยู่แล้ว";
                return false;
            }
            var list = new List<string>(existing) { playerId };
            WarpAccelerator state = s.State;
            state.Participants = list.ToArray();
            s.State = state;
            _world.SetArtifactWarpAccelerator(entityId, s.State);
            Console.WriteLine("[warp-accel] {0} — {1} เข้าร่วม (รวม {2} คน)", entityId, playerId, list.Count);
            reason = null;
            return true;
        }
    }

    /// <summary>กดรับรางวัล — ต้องอยู่ในสถานะ End และเป็นผู้เข้าร่วมที่ยังไม่เคลม</summary>
    public bool TryReceiveRewards(string entityId, string playerId, out int granted, out string reason)
    {
        granted = 0;
        lock (_lock)
        {
            if (!_sessions.TryGetValue(entityId ?? string.Empty, out Session s) || s.State.Status != AcceleratorStatus.End)
            {
                reason = "ยังไม่ผ่านกิจกรรมนี้ หรือกิจกรรมนี้จบไปแล้ว";
                return false;
            }
            if (s.State.Participants == null || Array.IndexOf(s.State.Participants, playerId) == -1)
            {
                reason = "ไม่ได้เข้าร่วมกิจกรรมนี้";
                return false;
            }
            if (!s.Claimed.Add(playerId))
            {
                reason = "รับรางวัลรอบนี้ไปแล้ว";
                return false;
            }
            granted = s.PendingWarpMatter;
            reason = null;
            // ทุกคนที่เข้าร่วมเคลมครบแล้ว — เคลียร์รอบนี้ทันที ไม่ต้องรอ RewardWindow หมดเวลา
            if (s.Claimed.Count >= s.State.Participants.Length)
            {
                ResetToIdle(entityId, s, Durango.Utils.Times.UnixTimeNow());
            }
            return true;
        }
    }

    // ---------------------------------------------------------------- tick หลัก — เรียกจาก ServerWorld.ProcessPlayers()

    public void Process(double now)
    {
        List<KeyValuePair<string, Session>> snapshot;
        lock (_lock)
        {
            snapshot = new List<KeyValuePair<string, Session>>(_sessions);
        }
        for (int i = 0; i < snapshot.Count; i++)
        {
            ProcessOne(snapshot[i].Key, snapshot[i].Value, now);
        }
    }

    private void ProcessOne(string entityId, Session s, double now)
    {
        lock (_lock)
        {
            // session อาจถูกรีเซ็ต/แทนที่ไปแล้วระหว่างรอ lock (เช่นมีคนกด ReceiveRewards จนเคลมครบพอดี)
            if (!_sessions.TryGetValue(entityId, out Session cur) || !ReferenceEquals(cur, s))
            {
                return;
            }
            switch (s.State.Status)
            {
                case AcceleratorStatus.Waiting:
                    if (now >= s.State.StatusUntil.GetValueOrDefault())
                    {
                        StartWave(entityId, s, now);
                    }
                    break;
                case AcceleratorStatus.Processing:
                    TickWave(entityId, s, now);
                    break;
                case AcceleratorStatus.Intermission:
                    if (now >= s.State.StatusUntil.GetValueOrDefault())
                    {
                        WarpAccelerator state = s.State;
                        state.CurrentWave = state.CurrentWave.GetValueOrDefault(1) + 1;
                        state.CurrentPhase = state.CurrentWave.Value;
                        s.State = state;
                        StartWave(entityId, s, now);
                    }
                    break;
                case AcceleratorStatus.End:
                    if (now >= s.State.StatusUntil.GetValueOrDefault())
                    {
                        Console.WriteLine("[warp-accel] {0} หมดเวลารับรางวัล — เคลมไปแล้ว {1}/{2} คน", entityId, s.Claimed.Count, s.State.Participants?.Length ?? 0);
                        ResetToIdle(entityId, s, now);
                    }
                    break;
                // RiftInactivated / RiftActivated: ไม่มีอะไรต้องทำต่อ tick (RiftActivated ไม่ได้ใช้เป็น
                // สถานะค้าง — ดูหมายเหตุขอบเขต MVP ข้อ 5 ด้านบน)
            }
        }
    }

    private void StartWave(string entityId, Session s, double now)
    {
        if (!_world.TryGetArtifact(entityId, out AppearArtifact a))
        {
            // artifact หายไปแล้ว (โดนรื้อระหว่างกิจกรรม?) — เลิก session ทิ้งเงียบ ๆ ไม่ต้อง broadcast อะไร
            _sessions.Remove(entityId);
            return;
        }
        int sx = a.Size.x <= 0 ? 1 : a.Size.x;
        int sy = a.Size.y <= 0 ? 1 : a.Size.y;
        WorldPosition center = new WorldPosition((a.Tile.x + sx / 2f) * 200f, (a.Tile.y + sy / 2f) * 200f);

        int wave = s.State.CurrentWave.GetValueOrDefault(1);
        int count = Math.Max(1, Cfg.AnimalsBase + Cfg.AnimalsStep * (wave - 1));
        float radius = Math.Max(1f, Cfg.SpawnRadiusTiles) * 200f;

        s.WaveAnimalIds.Clear();
        for (int i = 0; i < count; i++)
        {
            double ang = _rng.NextDouble() * Math.PI * 2.0;
            double r = Math.Sqrt(_rng.NextDouble()) * radius;
            WorldPosition pos = new WorldPosition(
                center.x + (float)(Math.Cos(ang) * r),
                center.y + (float)(Math.Sin(ang) * r));
            ServerAnimal animal = _world.Animals.SpawnAt(pos);
            s.WaveAnimalIds.Add(animal.EntityId);
        }

        WarpAccelerator state = s.State;
        state.Status = AcceleratorStatus.Processing;
        state.StatusSince = now;
        state.StatusUntil = now + Cfg.PhaseSeconds;
        state.CurrentWave = wave;
        state.CurrentPhase = wave;
        state.RemainAnimals = count;
        s.State = state;
        s.LastBroadcastRemain = count;
        _world.SetArtifactWarpAccelerator(entityId, s.State);

        Console.WriteLine("[warp-accel] {0} คลื่น {1}/{2} เริ่ม — สัตว์ {3} ตัว มีเวลา {4:F0} วิ",
            entityId, wave, state.CurrentMaxWave, count, Cfg.PhaseSeconds);
    }

    private void TickWave(string entityId, Session s, double now)
    {
        int remain = 0;
        for (int i = 0; i < s.WaveAnimalIds.Count; i++)
        {
            if (_world.Animals.TryGet(s.WaveAnimalIds[i], out ServerAnimal a) && a.IsAlive)
            {
                remain++;
            }
        }
        if (remain != s.LastBroadcastRemain)
        {
            WarpAccelerator state = s.State;
            state.RemainAnimals = remain;
            s.State = state;
            s.LastBroadcastRemain = remain;
            _world.SetArtifactWarpAccelerator(entityId, s.State);
        }
        if (remain <= 0)
        {
            WaveCleared(entityId, s, now);
            return;
        }
        if (now >= s.State.StatusUntil.GetValueOrDefault())
        {
            WaveFailed(entityId, s, now);
        }
    }

    private void WaveCleared(string entityId, Session s, double now)
    {
        s.PendingWarpMatter += Cfg.WarpMatterPerPhase;
        int maxWave = s.State.CurrentMaxWave ?? Cfg.MaxPhase;
        int wave = s.State.CurrentWave ?? 1;
        WarpAccelerator state = s.State;
        if (wave >= maxWave)
        {
            state.Status = AcceleratorStatus.End;
            state.StatusSince = now;
            state.StatusUntil = now + Cfg.RewardWindowSeconds;
            state.RemainAnimals = 0;
            s.State = state;
            s.WaveAnimalIds.Clear();
            _world.SetArtifactWarpAccelerator(entityId, s.State);
            NotifyParticipants(s, $"ผ่านกิจกรรมวาร์ปเรกเซเลอเรเตอร์ครบทุกคลื่นแล้ว! กด \"รับรางวัล\" เพื่อรับ Warp Matter {s.PendingWarpMatter} หน่วย");
            Console.WriteLine("[warp-accel] {0} ผ่านครบ {1} คลื่น — รอเคลมรวม {2} Warp Matter", entityId, maxWave, s.PendingWarpMatter);
        }
        else
        {
            state.Status = AcceleratorStatus.Intermission;
            state.StatusSince = now;
            state.StatusUntil = now + Cfg.WaitSeconds;
            state.RemainAnimals = 0;
            s.State = state;
            s.WaveAnimalIds.Clear();
            _world.SetArtifactWarpAccelerator(entityId, s.State);
            Console.WriteLine("[warp-accel] {0} ผ่านคลื่น {1}/{2} — พัก {3:F0} วิ", entityId, wave, maxWave, Cfg.WaitSeconds);
        }
    }

    private void WaveFailed(string entityId, Session s, double now)
    {
        Console.WriteLine("[warp-accel] {0} คลื่น {1} ล้มเหลว (หมดเวลา {2:F0} วิ ยังมีสัตว์เหลืออยู่) — ไม่ได้รางวัลสะสม {3} หน่วยที่ค้างไว้",
            entityId, s.State.CurrentWave, Cfg.PhaseSeconds, s.PendingWarpMatter);
        NotifyParticipants(s, "กิจกรรมวาร์ปเรกเซเลอเรเตอร์ล้มเหลว — ฆ่าสัตว์ไม่ทันเวลาที่กำหนด");
        ResetToIdle(entityId, s, now);
    }

    private void ResetToIdle(string entityId, Session s, double now)
    {
        s.State = new WarpAccelerator
        {
            Status = AcceleratorStatus.RiftInactivated,
            StatusSince = null,
            StatusUntil = null,
            CurrentPhase = 0,
            CurrentWave = null,
            CurrentMaxWave = null,
            RemainAnimals = null,
            Participants = null
        };
        s.CooldownUntil = now + Cfg.CooldownSeconds;
        s.WaveAnimalIds.Clear();
        s.PendingWarpMatter = 0;
        s.Claimed.Clear();
        s.LastBroadcastRemain = -1;
        _world.SetArtifactWarpAccelerator(entityId, s.State);
    }

    private void NotifyParticipants(Session s, string text)
    {
        if (s.State.Participants == null)
        {
            return;
        }
        for (int i = 0; i < s.State.Participants.Length; i++)
        {
            ServerPlayer p = _world.FindPlayer(s.State.Participants[i]);
            p?.Send(new Info { Text = text });
        }
    }
}
