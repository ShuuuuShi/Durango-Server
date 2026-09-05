using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Durango.Network;
using Durango.Offline;
using Durango.Utils;
using Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Item;
using Shared.Region;
using Shared.Economy;
using Shared.Faction;
using Shared.Skill;
using Shared.Social;
using Shared.Building;
using Shared.Etc;

namespace DurangoServer.Core;

// ============================================================================
// DurangoServer — ไฟล์หลักของ server
// ประกอบด้วย: ServerWorld (โลก), ServerPlayer (ผู้เล่น + handler เกมเพลย์),
// GameServer (TCP 8191), Gateway (HTTP 8190 + UDP knock), RadiotowerServer (แชท 8192)
// โปรโตคอล: MsgPack + Snappy, header 24 ไบต์ (time/seq/replyOf/typeCode/size)
// ============================================================================

// ServerPlayer.Sync — ดูรายละเอียดที่ docs/server/ServerPlayer.Sync.md

public partial class ServerPlayer
{
    /// <summary>ความจุกระเป๋าผู้เล่น (กล่องเก็บของคือ BoxMaxSize ดู ServerPlayer.Storage.cs)</summary>
    private const int PlayerInventoryMaxSize = 50;

    /// <summary>
    /// กระเป๋าเต็มไหม — เจอตอนรัน FarmBot: Collect/Craft เดิมไม่เช็คความจุเลย
    /// ทำให้ของทะลุ MaxSize ที่ประกาศไว้ (bot เก็บจนได้ 52 ชิ้นทั้งที่ MaxSize = 50)
    /// </summary>
    private bool InventoryFull
    {
        get
        {
            lock (_inventory)
            {
                return _inventory.Count >= PlayerInventoryMaxSize;
            }
        }
    }

    /// <summary>
    /// ข้อความบอกผู้เล่นตรงๆ ว่ากระเป๋าเต็ม แทนที่จะโชว์ debug string ดิบ ๆ แบบ "File.Method:Line"
    /// (`Aborts.Reason()` เดิมไม่ได้ใส่ why เลยตรงจุดกระเป๋าเต็ม เลยไปโผล่เป็นชื่อ handler ในเกมจริง)
    /// ทุกไอเทมกินช่องละ 1 เสมอในระบบนี้ (`Item.Size` ตั้งเป็น 1 ทุกจุดที่สร้างไอเทม ไม่มีไอเทมกินหลายช่อง)
    /// ⇒ จำนวนที่ต้องทิ้งเพื่อเก็บของใหม่ 1 ชิ้น คือ 1 เสมอ ไม่ต้องคำนวณตามขนาดไอเทม
    /// </summary>
    private string InventoryFullMessage
    {
        get
        {
            lock (_inventory)
            {
                return $"กระเป๋าเต็ม ({_inventory.Count}/{PlayerInventoryMaxSize} ช่อง) — ทิ้งหรือฝากของเข้าโกดังอย่างน้อย 1 ชิ้นก่อนถึงจะเก็บเพิ่มได้";
            }
        }
    }


    /// <summary>
    /// ดึงผู้เล่นกลับไปยืนที่ pos
    ///
    /// ต้องส่ง 2 อย่าง:
    /// - `Teleported` — อันเดียวที่ **ตัวคนนั้นเอง** ยอมรับ (PlayerManager ข้าม `Move` ของ local player ทิ้ง)
    ///   มีแค่ Tile (ละเอียด 1 tile = 200 หน่วย) พอสำหรับการดึงกลับ
    /// - `Move` — ให้คนอื่นในแมพเห็นว่าเขาย้ายไปแล้ว
    /// </summary>
    public void SendTeleport(WorldPosition pos, Shared.Teleport.TeleportType type = Shared.Teleport.TeleportType.WarpBack)
    {
        Send(new Teleported
        {
            Tile = new Point2((int)(pos.x / 200f), (int)(pos.y / 200f)),
            Type = type
        });

        Move move = new Move
        {
            EntityId = EntityId,
            Movements = new[]
            {
                new Movement
                {
                    MotionName = "Barehand_Stand",
                    MotionOption = 34,
                    PlaybackRate = 1f,
                    RotSpeed = 540f,
                    Path = new[]
                    {
                        new Location
                        {
                            Position = pos,
                            Yaw = 0f,
                            Time = Times.UnixTimeNow(),
                            Floor = 0,
                            Height = 0f
                        }
                    }
                }
            }
        };
        Send(move);
    }

    /// <summary>วาร์ปตัวเองไปพิกัดโลก แล้วจำตำแหน่งไว้ให้เซฟ/AOI ตามทัน</summary>
    public void WarpTo(WorldPosition pos)
    {
        _lastPosition = pos;
        _hasPosition = true;
        SendTeleport(pos);
    }

    /// <summary>
    /// ชุดข้อมูลที่ต้องส่งให้ครบตอนเข้าเกม
    ///
    /// 🐛 **หลอดขึ้น 999/999 ตอนเพิ่งเข้าเกม แล้วค่อยถูกต้องหลังไปเก็บของ**
    /// เพราะ `Statistics` (ที่บอก LifeMax/StaminaMax/FatigueMax) ถูกส่งเฉพาะตอน client ขอ
    /// ⇒ ถ้า client ยังไม่ขอ HUD ก็วาดด้วยค่าเดิมของตัวเอง (ตัวละครบนเกาะออฟไลน์ = หลอดใหญ่มาก)
    /// แล้วค่าถูกต้องโผล่ตอนเก็บของครั้งแรกเพราะตอนนั้นมี SurvivalUpdated วิ่งไป
    ///
    /// ⇒ ส่ง Statistics ไปพร้อมชุดแรกเสมอ **และส่งซ้ำอีกครั้งหลังเกาะโหลดเสร็จ**
    /// (HUD ของ client สร้างทีหลัง — ชุดแรกอาจถึงก่อนที่จะมีใครรับ)
    /// </summary>
    public void SendSpawnBurst()
    {
        // Send identity before the large spawn burst so the client always receives its own character.
        Send(MakeAppearPlayer());
        SendSkills();
        SendStatistics();
        SendInventory();
        SendEquipments();
        Send(BuildPoints());
        SendSurvival();                       // เฟส C
        SendDefoggedChunks();
        SendQuestCategories();
        SendQuestsOnSpawn();
        Send(new WalletUpdated
        {
            EntityId = EntityId,
            Wallet = new Wallet
            {
                PaidBalances = null,
                UnpaidBalances = null,
                Vouchers = null
            }
        });

        // ส่งซ้ำหลังเกาะโหลดเสร็จ — กันเคสที่ HUD ยังไม่เกิดตอนชุดแรกมาถึง
        _deferred.Add((Times.UnixTimeNow() + ResyncDelaySeconds, delegate
        {
            SendStatistics();
            SendSurvival();
            SendSkills();
        }));
    }

    /// <summary>รอกี่วินาทีก่อนส่งค่าสถานะซ้ำรอบสอง (เกาะโหลดเสร็จราว ๆ นี้)</summary>
    private const double ResyncDelaySeconds = 5.0;

    private void SendInventory()
    {
        lock (_inventory)
        {
            Send(new Inventory
            {
                EntityId = EntityId,
                InventoryItems = new InventoryItems
                {
                    EntityId = EntityId,
                    Items = _inventory.Count == 0 ? null : _inventory.ToArray()
                },
                InventoryInfos = new InventoryInfos
                {
                    EntityId = EntityId,
                    MaxSize = PlayerInventoryMaxSize,
                    LockedItemIds = CurrentProtectedItems().ItemIds,
                    ItemOrder = CurrentInventoryOrder(),
                    ProtectedItems = CurrentProtectedItems()
                },
                Wallet = null
            });
        }
    }

    private void SendEquipments()
    {
        // เฟส C: เดิมส่ง Presets = null ซึ่งทำให้ client โยน NRE
        Send(RebuildEquipments());
    }

    private void SendDefoggedChunks()
    {
        int count = _world.Terrain.NumChunksX * _world.Terrain.NumChunksY;
        Point2[] chunks = new Point2[count];
        int idx = 0;
        for (int i = 0; i < _world.Terrain.NumChunksX; i++)
        {
            for (int j = 0; j < _world.Terrain.NumChunksY; j++)
            {
                chunks[idx++] = new Point2(i, j);
            }
        }
        Send(new DefoggedChunks { Chunks = chunks });
    }

    private void SendQuestCategories()
    {
        // 💡 `Name` ของหมวดมาจาก **server** ไม่ใช่ตารางในตัวเกม ⇒ ใส่ภาษาไทยได้เลยตั้งแต่วันนี้
        //    (ต่างจากชื่อ/คำอธิบายของเควสรายอัน ที่ client หยิบจากตาราง `quests_for_client` เป็นเกาหลี
        //     จนกว่าจะเปิดแค็ตตาล็อกไทยได้ — ดู docs/client/TUNING.md §2.1)
        //
        // Epic = หมวดเนื้อเรื่อง (sunset) — ตรงกับของแท้ (Offline/Player.cs ส่ง Epic = "sunset")
        //         คลิกแล้ว client เปิดหน้า Story (chapters ของข้อมูลเกม — มี 8 บทของสาย K อยู่แล้ว)
        // Categories = แท็บย่อย — ของเราเพิ่ม "เควสประจำวัน" (Features.QuestChecklist) เปลี่ยนจาก
        // "รายการตรวจเซิร์ฟ" ตามที่ผู้เล่นใช้เป็นช่องทางเทสเซิร์ฟช่วยเหลือ
        // **ห้ามใส่ sunset ลง Categories ด้วย** — client กรองหมวดที่ == EpicCategory ออกจากแท็บ
        // แล้วตอน Epic เปิดหน้า Story มันจะสร้าง tab "เนื้อเรื่อง" ซ้ำกันในหน้าเควส
        QuestCategory[] tabs = ServerConfig.Current.Features.QuestChecklist
            ? new[]
            {
                new QuestCategory
                {
                    Category = QuestData.ChecklistCategory,
                    Name = "เควสประจำวัน",
                    Faction = null,
                    Season = null,
                    UnreceivedCount = CountUnclaimedQuests(QuestData.ChecklistCategory)
                }
            }
            : null;
        Send(new QuestCategories
        {
            Categories = tabs,
            Epic = new QuestCategory
            {
                Category = QuestData.MainCategory,
                Name = "เนื้อเรื่อง",
                Faction = null,
                Season = null,
                UnreceivedCount = CountUnclaimedQuests(QuestData.MainCategory)
            }
        });
    }

    public AppearPlayer MakeAppearPlayer()
    {
        // GP-02: ใช้ตำแหน่งล่าสุดที่ผู้เล่นเดินไปถึงจริง ๆ (อัปเดตจาก packet Move)
        // ถ้ายังไม่เคยขยับเลยจะ fallback เป็นจุดเกิดให้เอง — ดู ServerPlayer.Core.cs
        WorldPosition pos = CurrentPosition;
        return new AppearPlayer
        {
            EntityId = EntityId,
            EntityType = EntityType,
            IsAlive = !Dead,
            Name = Name,
            Freq = 0,
            Level = Level,
            Title = new Title
            {
                EntityId = EntityId,
                TitleId = _selectedTitleId,
                _Title = ""
            },
            Member = new Member
            {
                EntityId = EntityId,
                ClanId = "",
                ClanName = "",
                RoleId = 0,
                ApplyingClanId = null
            },
            Display = MakeDisplay(),
            Move = new Move
            {
                EntityId = EntityId,
                Movements = new[]
                {
                    new Movement
                    {
                        MotionName = "Barehand_Stand",
                        MotionOption = 34,
                        PlaybackRate = 1f,
                        RotSpeed = 540f,
                        Path = new[]
                        {
                            new Location
                            {
                                Position = pos,
                                Yaw = CurrentYaw,
                                Time = Times.UnixTimeNow(),
                                Floor = 0,
                                Height = 0f
                            }
                        }
                    }
                }
            },
            // เฟส C: ส่งเลือดจริง คนอื่นจะได้เห็นหลอดเลือดถูกต้อง
            Survival = new Survival
            {
                EntityId = EntityId,
                Life = BuildLifeGauge()
            },
            Musician = null,
            RescueRequested = false
        };
    }

    // เฟส C: หน้าตา = พื้นฐาน + อุปกรณ์ที่ใส่อยู่ (ดู ServerPlayer.Equipment.cs)
    private PlayerDisplay MakeDisplay()
    {
        return CurrentDisplay;
    }

}
