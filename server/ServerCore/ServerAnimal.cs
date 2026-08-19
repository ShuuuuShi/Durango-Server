using System;
using Durango.Utils;
using Messages;

namespace DurangoServer.Core;

/// <summary>
/// เฟส C — สัตว์ 1 ตัวในโลก
/// รอบนี้ทำแค่ "โผล่ในโลก + เดินสุ่ม" ยังไม่มีการต่อสู้
/// ดูรายละเอียดที่ docs/server/Animals.md
/// </summary>
public sealed class ServerAnimal
{
    public string EntityId { get; }
    public ushort EntityType { get; }
    public int Level { get; }
    public float Scale { get; }
    public bool IsAlive { get; private set; } = true;

    // ── ตำแหน่งจริงระหว่างเดิน ───────────────────────────────────────
    //
    // 🐛 ของเดิม: `MakeMove()` ตั้ง Position = ปลายทางทันทีที่สั่งเดิน
    // server จึงคิดว่าสัตว์ "ถึงที่หมายแล้ว" ตั้งแต่วินาทีแรก ทั้งที่ client ยังเดินอยู่
    // ⇒ ถ้ามีคำสั่งใหม่ระหว่างทาง (โดนตี · เข้าระยะโจมตี · ตาย) จุดเริ่มของคำสั่งใหม่
    //   จะเป็น "ปลายทางเก่า" แล้ว client ที่เดินไปได้แค่ครึ่งทาง **กระโดดไปข้างหน้าทันที**
    //   = อาการ "ม่อนเดินวาร์ป ๆ" ที่เห็นในเกม (เห็นชัดสุดตอนตีสัตว์ที่กำลังเดินอยู่)
    //
    // ตอนนี้ server เก็บ "เส้นทางที่กำลังเดิน" แล้วคำนวณตำแหน่งจริงตามเวลา
    // ทุกคำสั่งใหม่จึงเริ่มจากจุดที่สัตว์อยู่จริง ๆ — ไม่มีการกระโดดอีก
    // (แถมระยะที่ใช้ตัดสินใจ เช่น "เข้าระยะกัดหรือยัง" ก็ถูกต้องขึ้นด้วย)
    private WorldPosition _from;
    private WorldPosition _to;
    private double _moveStartAt;
    private double _moveEndAt;

    /// <summary>ตำแหน่งจริง ณ ตอนนี้ (คำนวณจากเส้นทางที่กำลังเดิน)</summary>
    public WorldPosition Position => PositionAt(Times.UnixTimeNow());

    /// <summary>ตำแหน่งจริง ณ เวลาที่ระบุ — ใช้ตัวนี้ใน loop ที่มี now อยู่แล้ว จะได้ไม่เรียกนาฬิกาซ้ำ</summary>
    public WorldPosition PositionAt(double now)
    {
        if (_moveEndAt <= _moveStartAt || now >= _moveEndAt)
        {
            return _to;
        }
        if (now <= _moveStartAt)
        {
            return _from;
        }
        float t = (float)((now - _moveStartAt) / (_moveEndAt - _moveStartAt));
        return new WorldPosition(
            _from.x + (_to.x - _from.x) * t,
            _from.y + (_to.y - _from.y) * t);
    }

    /// <summary>ยังเดินไม่ถึงที่หมายใช่ไหม</summary>
    public bool IsMoving(double now) => now < _moveEndAt;

    public float Yaw { get; private set; }

    /// <summary>จุดที่เกิด — เดินไปไกลกว่า WanderRadius จากตรงนี้ไม่ได้</summary>
    public WorldPosition Home { get; }

    public float LifeMax { get; }
    private float _life;
    private double _lifeAt;

    /// <summary>ถึงเวลาเลือกจุดหมายใหม่เมื่อไร</summary>
    public double NextMoveAt { get; set; }

    /// <summary>เวลาที่เดินถึงที่หมาย — ถึงแล้วต้องสั่งคลิป "ยืน" ไม่งั้นขาเดินค้าง (0 = ไม่ต้องสั่ง)</summary>
    public double StandAt { get; set; }

    public ServerAnimal(string entityId, ushort entityType, int level, float scale, WorldPosition home, float lifeMax, double now)
    {
        EntityId = entityId;
        EntityType = entityType;
        Level = level;
        Scale = scale;
        Home = home;
        SetPosition(home, 0f);
        LifeMax = lifeMax;
        _life = lifeMax;
        _lifeAt = now;
        NextMoveAt = now;
    }

    public float Life => _life;

    /// <summary>ลดเลือด คืน true ถ้าตาย (เตรียมไว้ให้ระบบต่อสู้รอบหน้า)</summary>
    public bool ApplyDamage(float amount, double now)
    {
        _life = Math.Max(0f, _life - amount);
        _lifeAt = now;
        if (_life <= 0f)
        {
            IsAlive = false;
        }
        return !IsAlive;
    }

    public Gauge LifeGauge()
    {
        return new Gauge(LifeMax, 0f, new[] { new GaugeNode { Time = _lifeAt, Value = _life } });
    }

    /// <summary>วางตัวไว้ที่จุดนี้แบบหยุดนิ่ง (เกิดใหม่ · หยุดเดิน · เล่นท่าอยู่กับที่)</summary>
    public void SetPosition(WorldPosition pos, float yaw)
    {
        _from = pos;
        _to = pos;
        _moveStartAt = 0.0;
        _moveEndAt = 0.0;
        Yaw = yaw;
    }

    /// <summary>เริ่มเดินจากจุดที่อยู่จริงไปปลายทาง (server จะคิดตำแหน่งระหว่างทางให้เอง)</summary>
    private void BeginMove(WorldPosition from, WorldPosition dest, double startAt, double endAt, float yaw)
    {
        _from = from;
        _to = dest;
        _moveStartAt = startAt;
        _moveEndAt = endAt;
        Yaw = yaw;
    }

    /// <summary>
    /// ความสูงพื้นตรงที่มันยืน — server ไม่มี heightmap ของแมพ จึงใช้ค่าที่ client รายงานมาล่าสุด
    /// (Height = 0 ทำให้ตัวสัตว์จมอยู่ใต้พื้น เห็นแต่เงา — เจอตอนเทสกับเกมจริง)
    /// </summary>
    public float Height { get; set; }

    public AppearAnimal MakeAppear()
    {
        double now = Times.UnixTimeNow();
        return new AppearAnimal
        {
            EntityId = EntityId,
            EntityType = EntityType,
            IsAlive = IsAlive,
            Level = Level,
            Display = new AnimalDisplay
            {
                EntityId = EntityId,
                BaseScale = Scale,
                CollectibleDisplay = null
            },
            Move = new Move
            {
                EntityId = EntityId,
                Movements = new[]
                {
                    new Movement
                    {
                        // ต้องส่งชื่อคลิป ไม่งั้นสัตว์โผล่มาแล้วยืนแข็ง (ดู AnimalMotionData)
                        MotionName = AnimalMotionData.Stand(EntityType),
                        MotionOption = 0,
                        PlaybackRate = 1f,
                        RotSpeed = 0f,
                        Path = new[]
                        {
                            new Location
                            {
                                Position = Position,
                                Yaw = Yaw,
                                Time = now,
                                Floor = 0,
                                Height = Height
                            }
                        }
                    }
                }
            },
            Survival = new Survival
            {
                EntityId = EntityId,
                Life = LifeGauge()
            },
            Role = null,
            EnemyId = null
        };
    }

    /// <summary>รอบนี้ให้เล่นคลิป "วิ่ง" แทน "เดิน" ไหม (ใช้ตอนไล่/หนี)</summary>
    private bool _running;

    /// <summary>
    /// packet ที่ให้ "ยืนอยู่กับที่แล้วเล่นคลิปหนึ่ง" (ท่าโจมตี/ยืนเฉย) พร้อมหันหน้าไปทางที่กำหนด
    ///
    /// ใช้กลไกเดียวกับการเดิน เพราะ client เล่นอนิเมชันจาก `Movement.MotionName` ของ packet Move
    /// (`AnimalBehavior.HandleMoveMsg` → `PlayAnimationMovement`) — path 2 จุดที่ตำแหน่งเดียวกัน
    /// จึงเป็น "อยู่กับที่" แต่ยังสั่งอนิเมชันกับมุมหันได้
    /// </summary>
    /// <param name="loop">ท่ายืนต้องวนลูป ส่วนท่าโจมตี/ตายเล่นรอบเดียว</param>
    /// <param name="playbackRate">
    /// ความเร็วการเล่นคลิป — 0 = หยุดค้างเฟรม
    /// (client ตั้ง <c>CurAnimState.speed = playbackRate</c> ตรง ๆ)
    /// </param>
    /// <param name="clipOffset">เริ่มเล่นคลิปที่วินาทีที่เท่าไร (ใช้คู่กับ playbackRate 0 เพื่อค้างท่าสุดท้าย)</param>
    public Move MakeMotion(string motionName, float yaw, double now, double seconds = 0.6, bool loop = false,
        float playbackRate = 1f, double clipOffset = 0.0)
    {
        // ท่าอยู่กับที่ = หยุดเดินตรงจุดที่อยู่จริง ณ ตอนนี้
        // (เดิมหยุดที่ "ปลายทางของคำสั่งเดินก่อนหน้า" ⇒ ตัวกระโดดไปข้างหน้าตอนเริ่มตี/ตาย)
        SetPosition(PositionAt(now), yaw);
        // MotionOption เป็น flag (ดู Durango.Network/MotionOption):
        //   1 LOOPING · 4 SNAP_ANGLE_BEGIN (หันทันทีตอนเริ่ม) · 8 IN_PLACE_MOTION (ไม่ให้ root motion ลากตัวไป)
        byte option = (byte)(loop ? 1 | 4 : 8 | 4);
        // client คิดจุดเริ่มของคลิปจาก "เวลาที่ผ่านไปนับจาก Path[0].Time"
        // (AnimalBehavior.PlayAnimationMovement → CheckBufferedTimePassed) — ย้อนเวลาให้ = เริ่มกลางคลิป
        double startAt = now - clipOffset;
        return new Move
        {
            EntityId = EntityId,
            Movements = new[]
            {
                new Movement
                {
                    MotionName = motionName,
                    MotionOption = option,
                    PlaybackRate = playbackRate,
                    RotSpeed = 540f,
                    Path = new[]
                    {
                        new Location { Position = Position, Yaw = yaw, Time = startAt, Floor = 0, Height = Height },
                        new Location { Position = Position, Yaw = yaw, Time = startAt + seconds, Floor = 0, Height = Height }
                    }
                }
            }
        };
    }

    /// <summary>มุมหันไปยังจุดหนึ่ง (องศา 0-360 ตามสูตรเดียวกับ client: atan2(dx, dz))</summary>
    public static float YawTo(WorldPosition from, WorldPosition to)
    {
        float dx = to.x - from.x;
        float dy = to.y - from.y;          // world y = client z
        float yaw = MathF.Atan2(dx, dy) * (180f / MathF.PI);
        return yaw < 0f ? yaw + 360f : yaw;
    }

    /// <summary>สร้าง packet เดินจากตำแหน่งปัจจุบันไปยังจุดหมาย</summary>
    /// <param name="travelSeconds">เวลาที่ใช้เดินจริง — ผู้เรียกต้องรอให้ถึงก่อนสั่งเดินใหม่</param>
    public Move MakeMove(WorldPosition dest, float speed, double now, out double travelSeconds, bool running = false)
    {
        _running = running;
        // จุดเริ่มต้องเป็น "ที่ที่มันอยู่จริงตอนนี้" ไม่ใช่ปลายทางของคำสั่งก่อนหน้า
        // ไม่งั้น client กระโดดไปข้างหน้าทันทีที่ได้ packet (ดูคอมเมนต์บนหัวไฟล์)
        WorldPosition start = PositionAt(now);
        float dx = dest.x - start.x;
        float dy = dest.y - start.y;
        float dist = MathF.Sqrt(dx * dx + dy * dy);
        double duration = speed <= 0f ? 1.0 : dist / speed;
        travelSeconds = duration;
        // มุมต้องเป็น 0-360 ตามที่ client คิด (Maths.CalcYaw = atan2(dir.x, dir.z) แล้วบวก 360 ถ้าติดลบ)
        // ค่าติดลบทำให้ตัวหันผิดด้าน
        float yaw = YawTo(start, dest);

        Move move = new Move
        {
            EntityId = EntityId,
            Movements = new[]
            {
                new Movement
                {
                    MotionName = _running ? AnimalMotionData.Run(EntityType) : AnimalMotionData.Walk(EntityType),
                    MotionOption = 5,
                    PlaybackRate = 1f,
                    // หันตัวให้ทันก่อนถึงที่หมาย — 100 องศา/วิ ช้าไปจนดูเหมือนหันผิดทางตอนเปลี่ยนทิศเร็ว ๆ
                    RotSpeed = 540f,
                    Path = new[]
                    {
                        // ⚠️ จุดแรกต้องใช้ "ทิศปลายทาง" ด้วย
                        // client lerp มุมจาก Path[0].Yaw ไป Path[1].Yaw ตลอดช่วงเวลาเดิน
                        // (PathMovable: Mathf.DeltaAngle(prev.Yaw, next.Yaw) แล้วค่อย ๆ หมุน)
                        // ถ้าใส่ทิศเดิมไว้จุดแรก ตัวจะหมุนตัวไปเรื่อย ๆ ระหว่างเดิน = เดินหันข้างทั้งเส้นทาง
                        new Location { Position = start, Yaw = yaw, Time = now, Floor = 0, Height = Height },
                        new Location { Position = dest, Yaw = yaw, Time = now + duration, Floor = 0, Height = Height }
                    }
                }
            }
        };
        BeginMove(start, dest, now, now + duration, yaw);
        return move;
    }
}
