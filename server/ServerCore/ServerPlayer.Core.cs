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

// ServerPlayer.Core — ดูรายละเอียดที่ docs/server/ServerPlayer.Core.md

public partial class ServerPlayer
{
    public string EntityId { get; }
    public string Name { get; set; }
    public ushort EntityType { get; set; } = 1000;
    public int Level { get; set; } = 1;

    private readonly Durango.Offline.Connection _conn;
    private readonly ServerWorld _world;
    // งานที่ต้องส่งหลังเวลาผ่านไป (Timer → Collected, ArtifactBuilt ฯลฯ) — process ใน main loop เท่านั้น
    private readonly List<(double at, System.Action act)> _deferred = new List<(double, System.Action)>();
    // สถานะของธรรมชาติแต่ละจุด (key = entity id จาก Touch) — จำนวน generator ที่เหลือ
    // GP-03: _generatorState ย้ายไปอยู่ที่ ServerWorld แล้ว (เดิมอยู่ตรงนี้ = แยกต่อคน → เก็บของซ้ำได้)
    // กระเป๋าผู้เล่น (state อยู่ในหน่วยความจำ ยังไม่ persist)
    private readonly List<Item> _inventory = new List<Item>();
    // สกิล + แต้มสกิล (เริ่มจาก save ของเกาะตัวเองที่ส่งมาทาง /sessions)
    private Dictionary<Shared.Skill.Category, SkillCategory> _skills = new Dictionary<Shared.Skill.Category, SkillCategory>();
    // Beta 1.0: แต้มสกิลได้จากการขึ้นเลเวลเท่านั้น (ดู ServerPlayer.Progress)
    // เดิมแจก 777 แต้มตั้งแต่แรกเพราะสกิลยังไม่มีผลอะไร — ตอนนี้สกิลมีผลจริงแล้วจึงต้องเริ่มจากศูนย์
    // (ยังยอมรับค่าที่มากับ /sessions จากเกาะตัวเองอยู่ ดู LoadFromSessionData)
    private int _skillPoints = 0;
    private readonly List<SkillBundle> _knownSkills = new List<SkillBundle>();
    private PlayerDisplay _loadedDisplay;
    private bool _hasLoadedDisplay;
    /// <summary>GP-14: client ส่ง entity type ที่ใช้ได้มารอบนี้ไหม (ถ้าไม่ ให้ใช้ของที่เซฟไว้)</summary>
    private bool _entityTypeFromClient;
    /// <summary>GP-14: client ส่งเลเวลมารอบนี้ไหม — ถ้าส่ง ไฟล์เซฟของเกาะห้ามมาทับ</summary>
    private bool _levelFromClient;

    // GP-02: ตำแหน่งล่าสุดที่ผู้เล่นเดินไปถึง อัปเดตจาก packet Move ทุกครั้ง
    // ใช้ตอนสร้าง AppearPlayer ให้คนอื่น — ก่อนหน้านี้ server ไม่เก็บตำแหน่งใครเลย
    private WorldPosition _lastPosition;
    private float _lastYaw;
    private bool _hasPosition;
    private float _lastHeight;
    private int _lastFloor;

    /// <summary>ความสูงพื้นล่าสุดที่ client รายงาน (server ไม่มี heightmap เอง)</summary>
    public float CurrentHeight => _lastHeight;

    public int CurrentFloor => _lastFloor;

    /// <summary>ตำแหน่งปัจจุบัน ถ้ายังไม่เคยขยับเลยจะคืนจุดเกิด</summary>
    public WorldPosition CurrentPosition => _hasPosition ? _lastPosition : _world.GetEntryPosition();

    /// <summary>ทิศที่หันอยู่ (0 ถ้ายังไม่เคยขยับ)</summary>
    public float CurrentYaw => _hasPosition ? _lastYaw : 0f;

    public ServerPlayer(string entityId, string name, Durango.Offline.Connection conn, ServerWorld world, GameServer.PlayerData data = null)
    {
        EntityId = entityId;
        Name = name;
        _conn = conn;
        _world = world;
        if (data != null)
        {
            ApplyPlayerData(data);
        }
        else
        {
            LoadPlayerSave();
        }
        // GP-07: ทับด้วย state ที่เซฟไว้ (ของ/สกิล/ตำแหน่ง) ต้องมาหลัง ApplyPlayerData
        // เพราะข้อมูลจาก client ใช้แค่ ชื่อ/เลเวล/หน้าตา ส่วนของในกระเป๋าเป็นของฝั่ง server
        LoadPersistedState();
    }

    // ── GP-14: ข้อมูลตัวละครที่ client อ้างมา ต้องผ่านการตรวจก่อน ──────────
    // /sessions รับ JSON มาจากเกาะของ client ตรง ๆ ค่าที่อยู่ในนั้นจึงปลอมได้ทั้งหมด
    // เลเวลเป็นค่าที่มีผลกับเกม (โชว์ให้คนอื่นเห็น + ใช้เป็นฐานของระบบต่อ ๆ ไป)
    // จึงยึด "ค่าที่ server เซฟไว้" เป็นหลัก ดู LoadPersistedState ใน ServerPlayer.Persistence.cs

    /// <summary>
    /// เพดานงานที่รอเวลาอยู่ต่อผู้เล่น (H-6) — ทุก handler ที่ใช้ _deferred ต้องเช็คก่อน
    /// ไม่งั้นยิง packet รัว ๆ ก็ทำให้คิวโตไม่จำกัดแล้ว main loop ค้าง
    /// </summary>
    public const int MaxPendingActions = 32;

    /// <summary>เพดานเลเวลของเกม — ค่าที่เกินนี้คือ client โกหก</summary>
    public const int MaxPlayerLevel = 60;

    private const ushort MinPlayerEntityType = 1000;
    private const ushort MaxPlayerEntityType = 1999;

    /// <summary>ตัดเลเวลให้อยู่ในช่วงที่เป็นไปได้ พร้อมบอกว่าโดนตัดเพราะอะไร</summary>
    private int ClampLevel(int level, string source)
    {
        if (level < 1)
        {
            return 1;
        }
        if (level > MaxPlayerLevel)
        {
            Console.WriteLine($"[player] {Name}: {source} อ้างเลเวล {level} เกินเพดาน {MaxPlayerLevel} — ตัดลงมา");
            return MaxPlayerLevel;
        }
        return level;
    }

    /// <summary>entity type ของผู้เล่นอยู่ช่วง 1000-1999 (2000+ เป็นสัตว์, 10000+ เป็นของธรรมชาติ)</summary>
    private static bool IsPlayerEntityType(ushort type)
    {
        return type >= MinPlayerEntityType && type <= MaxPlayerEntityType;
    }

    private void ApplyPlayerData(GameServer.PlayerData data)
    {
        // 🐛 เดิมเชื่อเลเวลที่ client ส่งมาทาง /sessions เสมอ
        //
        // ตัวเกมส่งเลเวลของ "ตัวละครบนเกาะของตัวเอง" (offline island) มาด้วยทุกครั้งที่ล็อกอิน
        // ⇒ ลบไฟล์เซฟทิ้งเพื่อรีเซ็ตแล้วก็ไม่เป็นผล เพราะ client อ้างเลเวลเดิมกลับมาใหม่
        //   (เจอจริง: รีเซ็ตทุกคนเป็นเลเวล 1 แล้ว แต่ตัวละครในเกมยังขึ้นเลเวล 7)
        // ⇒ และเป็นช่องโกงตรง ๆ ด้วย เพราะค่านั้นแก้ได้ที่เครื่องผู้เล่น
        //
        // beta 1.0.0: **exp ที่ server เก็บเป็นตัวจริง เลเวลเป็นผลลัพธ์เสมอ**
        // เลเวลข้ามเกาะไม่หายเพราะ exp อยู่ในไฟล์เซฟที่ใช้ร่วมกันทุกเกาะ ไม่ได้พึ่งค่าจาก client
        // (ยังเปิดพฤติกรรมเดิมได้ด้วย --trust-client-profile ตอนย้ายข้อมูลจากที่อื่น)
        if (data.Level > 0 && GameServer.TrustClientProfile)
        {
            Level = ClampLevel(data.Level, "client");
            _levelFromClient = true;
        }
        else if (data.Level > 1)
        {
            Console.WriteLine("[player] {0}: client อ้างเลเวล {1} — ไม่รับ (เลเวลคิดจาก exp ที่ server เก็บเท่านั้น)",
                data.Name ?? Name, data.Level);
        }
        if (IsPlayerEntityType(data.EntityType))
        {
            EntityType = data.EntityType;
            _entityTypeFromClient = true;
        }
        else if (data.EntityType > 0)
        {
            // ปล่อยผ่านแล้วคนอื่นจะเห็นเราเป็นสัตว์/ต้นไม้ (หรือ client เจ้าอื่นหาโมเดลไม่เจอแล้วพัง)
            Console.WriteLine($"[player] {Name}: client อ้าง entity type {data.EntityType} ซึ่งไม่ใช่ของผู้เล่น — ใช้ {EntityType} แทน");
        }
        if (!string.IsNullOrEmpty(data.Name))
        {
            Name = data.Name;
        }
        if (!string.IsNullOrEmpty(data.DisplayJson))
        {
            try
            {
                JToken display = JToken.Parse(data.DisplayJson);
                if (display.Type == JTokenType.Object)
                {
                    _loadedDisplay = display.ToObject<PlayerDisplay>();
                    _loadedDisplay.EntityId = EntityId;
                    _hasLoadedDisplay = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[player] display parse failed: " + e.Message);
            }
        }
        if (!_hasLoadedDisplay)
        {
            LoadPlayerSave();
        }
        // 🐛 **สกิลค้างที่ Max ทั้งหมด** — สาเหตุเดียวกับเรื่องเลเวล
        //
        // /sessions รับ SkillsJson / SkillPoints / KnownSkillsJson มาจาก **เกาะของ client เอง**
        // ซึ่งเป็นตัวละครที่เล่นมานาน (เลเวล 60 สกิลเต็มทุกหมวด) แล้วเซิร์ฟรับมาทั้งก้อน
        // ⇒ เข้าเซิร์ฟใหม่ก็เห็นสกิลเต็มตั้งแต่วินาทีแรก · รีเซ็ตเซฟก็ไม่หาย · และปลอมได้ที่เครื่องผู้เล่น
        //
        // beta 1.0.0: **สกิลเป็นของ server ล้วน** — เรียนผ่าน LearnSkill เท่านั้น
        // เก็บใน saves/players/<id>.json (ข้ามเกาะไปด้วยกันอยู่แล้ว จึงไม่ต้องพึ่งค่าจาก client)
        if (GameServer.TrustClientProfile)
        {
            if (!string.IsNullOrEmpty(data.SkillsJson))
            {
                try
                {
                    _skills = JToken.Parse(data.SkillsJson).ToObject<Dictionary<Shared.Skill.Category, SkillCategory>>() ?? new Dictionary<Shared.Skill.Category, SkillCategory>();
                }
                catch (Exception e)
                {
                    Console.WriteLine("[player] skills parse failed: " + e.Message);
                }
            }
            if (data.SkillPoints > 0)
            {
                _skillPoints = data.SkillPoints;
            }
            if (!string.IsNullOrEmpty(data.KnownSkillsJson))
            {
                try
                {
                    SkillBundle[] known = JToken.Parse(data.KnownSkillsJson).ToObject<SkillBundle[]>();
                    if (known != null)
                    {
                        _knownSkills.AddRange(known);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("[player] known skills parse failed: " + e.Message);
                }
            }
        }
        else if (!string.IsNullOrEmpty(data.SkillsJson) || data.SkillPoints > 0)
        {
            Console.WriteLine("[player] {0}: client ส่งสกิล/แต้มสกิลมา — ไม่รับ (สกิลเป็นของ server ล้วน)",
                data.Name ?? Name);
        }
    }

    private void LoadPlayerSave()
    {
        string path = GameServer.PlayerSavePath;
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            return;
        }
        try
        {
            JObject save = JObject.Parse(File.ReadAllText(path));
            JToken display = save["appear_player"]?["Display"];
            if (display != null && display.Type == JTokenType.Object)
            {
                _loadedDisplay = display.ToObject<PlayerDisplay>();
                _loadedDisplay.EntityId = EntityId;
                _hasLoadedDisplay = true;
            }
            // GP-14: ไฟล์เซฟของเกาะเป็นแค่ fallback ตอน /sessions ไม่ได้ส่งข้อมูลผู้เล่นมา
            // ห้ามทับค่าที่ client ส่งมารอบนี้ (เจอตอนเทส: ผู้เล่นอ้าง Lv.5 แต่โผล่มาเป็น Lv.60 ของเจ้าของเครื่อง)
            // beta 1.0.0: เลเวลมาจาก exp ที่ server เก็บเท่านั้น (ดูเหตุผลที่ ApplyPlayerData)
            // ไฟล์เซฟเกาะของ client ก็เป็นข้อมูลฝั่งผู้เล่นเหมือนกัน จึงเชื่อไม่ได้เท่ากัน
            int level = save["appear_player"]?.Value<int>("Level") ?? 0;
            if (level > 0 && !_levelFromClient && GameServer.TrustClientProfile)
            {
                Level = ClampLevel(level, "ไฟล์เซฟเกาะ");
            }
            // เฟส C: อ่านเพศจากไฟล์เดียวกับที่เอา Display มา ไม่งั้นอาจได้ display หญิง
            // แต่ EntityType ชาย → เลือกโมเดลเกราะผิดเพศ
            ushort entityType = save["appear_player"]?.Value<ushort>("EntityType") ?? 0;
            if (IsPlayerEntityType(entityType) && !_entityTypeFromClient)     // GP-14
            {
                EntityType = entityType;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("[player] save load failed: " + e.Message);
        }
    }

    public void Process()
    {
        double now = Times.UnixTimeNow();
        for (int i = _deferred.Count - 1; i >= 0; i--)
        {
            if (_deferred[i].at <= now)
            {
                System.Action act = _deferred[i].act;
                _deferred.RemoveAt(i);
                try
                {
                    act();
                }
                catch (Exception e)
                {
                    Console.WriteLine("[player] deferred action failed: " + e.Message);
                }
            }
        }
        ProcessSkillResearch(now);
    }

    public void Send<T>(T msg) where T : struct
    {
        try
        {
            _conn.Send(msg);
        }
        catch (Exception e)
        {
            Console.WriteLine("[player] send failed: " + e.Message);
        }
    }

    public void Send<T>(T msg, uint replyOf) where T : struct
    {
        try
        {
            _conn.Send(msg, replyOf);
        }
        catch (Exception e)
        {
            Console.WriteLine("[player] send failed: " + e.Message);
        }
    }

    public void RegisterHandlers()
    {
        _conn.Recv<Move>(HandleMove);
        // ⚠️ ห้ามลงทะเบียน Messages.Say — struct นี้ไม่มี const TypeCode
        // จะถูกลงทะเบียนใต้ key 0 ซึ่งฝั่ง client ใช้ TypeCode 0 = "packet ตอบกลับ (reply)"
        // ผลคือ reply ใด ๆ ที่วิ่งเข้ามาจะถูกพยายาม deserialize เป็น Say (เจอตอนเทสกับเกมจริง)
        _conn.Recv<SetChunk>(HandleSetChunk);
        _conn.Recv<Cheat>(HandleCheat);
        _conn.Recv<GetStatistics>(delegate(GetStatistics msg, PacketHeader header) { SendStatistics(); });
        RegisterGroup2Handlers();
        // เฟส C: GetInventory มี Target = ดูของในกล่อง, ไม่มี = กระเป๋าตัวเอง
        _conn.Recv<GetInventory>(HandleGetInventory);
        _conn.Recv<PutInItem>(HandlePutInItem);
        _conn.Recv<TakeOutItem>(HandleTakeOutItem);
        // beta 1.0: ทิ้งของ / กินของ — เดิมไม่มี handler เลย กระเป๋าเต็มแล้วตันถาวร
        _conn.Recv<DumpItems>(HandleDumpItems);
        _conn.Recv<UseItem>(HandleUseItem);
        _conn.Recv<RepairItem>(HandleRepairItem);
        _conn.Recv<InventoryOrder>(HandleInventoryOrder);
        _conn.Recv<LockOrUnlockItems>(HandleLockOrUnlockItems);
        _conn.Recv<GetSkills>(delegate(GetSkills msg, PacketHeader header) { SendSkills(); });
        // สกิล: เรียน/ลืม (ตอบ OK แล้วส่ง Skills ใหม่พร้อมแต้มที่อัปเดต)
        _conn.Recv<LearnSkill>(HandleLearnSkill);
        _conn.Recv<UntrainSkill>(HandleUntrainSkill);
        _conn.Recv<ResearchSkillCategory>(HandleResearchSkillCategory);
        _conn.Recv<CancelSkillCategoryResearch>(HandleCancelSkillCategoryResearch);
        _conn.Recv<SkipSkillCategoryResearch>(HandleSkipSkillCategoryResearch);
        // คราฟ: ส่งรายการสูตรทั้งหมด (720 สูตรจาก data จริงของเกม)
        // 🐛 เดิมส่ง `AllRecipeIds` ทั้ง 720 อันให้ทุกคน ⇒ **ไม่ได้เรียนสกิลอะไรเลยก็เห็นสูตรครบ**
        //
        // ฝั่ง client (Crafting/Category.SetAvailableList) ตั้ง `item.Available` จากรายการ id ที่เราส่งตรง ๆ
        // ⇒ ส่งเท่าไรก็เห็นเท่านั้น ตัวกรองอยู่ที่ server ล้วน
        //
        // ในเกมจริง **สกิลเป็นตัวปลดล็อกสูตร** (client/Durango.Logic.Skill/Reward.cs มีฟิลด์ RecipeIds)
        // ตารางจริงอยู่ใน TextAsset `skills` + `rewards` — สกัดมาไว้ที่ RecipeUnlockData แล้ว
        _conn.Recv<GetRecipes>(delegate(GetRecipes msg, PacketHeader header)
        {
            Send(new Recipes
            {
                Ids = UnlockedRecipes(),
                NewRecipeIds = null,
                LikedRecipeIds = null
            }, header.Seq);
        });
        _conn.Recv<GetArtifactBlueprints>(delegate(GetArtifactBlueprints msg, PacketHeader header)
        {
            Send(new ArtifactBlueprints
            {
                Ids = UnlockedBlueprints(),
                NewBlueprintIds = null,
                LikedBlueprintIds = null
            }, header.Seq);
        });
        _conn.Recv<Craft>(HandleCraft);
        // เฟส C — สวมใส่อุปกรณ์
        _conn.Recv<Equip>(HandleEquip);
        _conn.Recv<GetEquipments>(HandleGetEquipments);
        _conn.Recv<ChangeEquipSlotType>(HandleChangeEquipSlotType);
        _conn.Recv<AttachAccessory>(HandleAttachAccessory);
        _conn.Recv<ResetAccessory>(HandleResetAccessory);
        _conn.Recv<PlaceCapsulatedArtifact>(HandlePlaceCapsulatedArtifact);
        _conn.Recv<RestOn>(delegate(RestOn msg, PacketHeader header)
        {
            // ตามรายการ beta: ความล้าฟื้นได้ด้วย "กองไฟ เต็นท์ หลับนอน" เท่านั้น
            // (นอกนั้นความล้ามีแต่ขึ้น — นี่คือสิ่งที่ทำให้ต้องกลับบ้าน)
            Send(new Info { Text = TryStartResting(msg.EntityId) }, header.Seq);
            Send(default(OK), header.Seq);
        });
        _conn.Recv<GetQuests>(HandleGetQuests);
        _conn.Recv<GetQuestState>(HandleGetQuestState);
        _conn.Recv<RequestQuestReward>(HandleRequestQuestReward);
        _conn.Recv<GetQuestScoreInfos>(HandleGetQuestScoreInfos);
        // โหมดสอน: ตอนผู้เล่นต่อแพแล้วกด "ออกเรือ" → client ส่ง DepartTutorial มา
        // (ดู client/TutorialIslandSystem.cs:82) — ถ้าไม่ตอบ client จะรอ DepartTutorialReady ค้าง
        // flow: DepartTutorial → ส่ง DepartTutorialReady → client ส่ง DepartTutorialFor
        //      → ส่ง Emigrated → client ปิด connection กลับหน้า title เพื่อเข้าเซิร์ฟใหม่
        _conn.Recv<DepartTutorial>(HandleDepartTutorial);
        _conn.Recv<DepartTutorialFor>(HandleDepartTutorialFor);
        // POI — ระบบค้นหาหลุม warp/rift + วาร์ปข้ามเกาะ (ดู ServerPlayer.POI.cs)
        _conn.Recv<SearchPOIs>(HandleSearchPOIs);
        _conn.Recv<GetPOICount>(HandleGetPOICount);
        _conn.Recv<GetExploredPOIs>(HandleGetExploredPOIs);
        _conn.Recv<GetLastSearchedTime>(HandleGetLastSearchedTime);
        _conn.Recv<ExplorePOI>(HandleExplorePOI);
        _conn.Recv<Warp>(HandleWarp);
        _conn.Recv<WarpBack>(HandleWarpBack);
        _conn.Recv<WarpToPort>(HandleWarpToPort);
        // [แก้เอง] 23 ส.ค. 2026 — เจ้าของรายงาน "ไม่มีเมนูกดวาป" ที่หลุมวาร์ป ต้นเหตุคือ 2 คำสั่งนี้ไม่มี
        // handler เลยมาตั้งแต่แรก (WorldMapGroup.cs ฝั่ง client ส่ง GetWarpCosts ก่อนแสดงปุ่ม/ป้ายราคาบนแผนที่
        // เสมอตอนเปิดโหมด "วาป" — ไม่ตอบกลับ = ป้ายราคา/สถานะกดได้ไม่ขึ้นเลย ดูเหมือนไม่มีเมนู) ดู HandleGetWarpCosts
        _conn.Recv<GetWarpCosts>(HandleGetWarpCosts);
        _conn.Recv<GetWarpBackCost>(HandleGetWarpBackCost);
        // [แก้เอง] คู่กับเมนู "วาป" ที่เพิ่งเพิ่มใน HandleTouch (component "Warphole" — ServerPlayer.Gathering.cs)
        // client กดเมนูนี้แล้วส่งอันนี้มาก่อนเสมอ ก่อนจะเปิดแผนที่โหมดวาป
        _conn.Recv<IsWarpholeAvailable>(HandleIsWarpholeAvailable);
        _conn.Recv<GetAvailableEmotions>(delegate(GetAvailableEmotions msg, PacketHeader header)
        {
            // ปิดอยู่ = ตอบรายการว่าง (ห้ามไม่ตอบเลย client จะรอค้าง)
            bool on = ServerConfig.Current.Features.Emotes;
            Send(new AvailableEmotions
            {
                Motions = on ? NaturalData.MotionIds : Array.Empty<string>(),
                Emoticons = on ? NaturalData.EmoticonIds : Array.Empty<string>()
            }, header.Seq);
        });
        // เฟส C รอบ 2 — ต่อสู้ / ตาย / ฟื้น (ดู ServerPlayer.Combat.cs)
        _conn.Recv<GetActions>(HandleGetActions);
        _conn.Recv<UseBattleAction>(HandleUseBattleAction);
        _conn.Recv<Revive>(HandleRevive);
        _conn.Recv<GetReviveImmediatelyInfo>(HandleGetReviveImmediatelyInfo);
        _conn.Recv<ReviveImmediately>(HandleReviveImmediately);
        _conn.Recv<RemoveDeathPoint>(HandleRemoveDeathPoint);
        _conn.Recv<SelectBattleTarget>(delegate(SelectBattleTarget msg, PacketHeader header)
        {
            // client บอกว่ากำลังเล็งใคร — server ไม่ต้องทำอะไร แต่ต้องไม่ปล่อยให้ไม่มี handler
        });
        _conn.Recv<EnterBattle>(delegate(EnterBattle msg, PacketHeader header)
        {
            Send(default(OK), header.Seq);
            Send(new BattleBegun
            {
                EntityId = EntityId,
                EventAt = Times.UnixTimeNow(),
                EnemyId = msg.EntityId,
                StartDamaged = false
            });
        });
        _conn.Recv<ExitBattle>(delegate(ExitBattle msg, PacketHeader header)
        {
            // client ออกจากโหมดต่อสู้ก็ต่อเมื่อได้ BattleEnded — ตอบแค่ OK ทำให้ค้างอยู่ในโหมดต่อสู้ตลอด
            Send(default(OK), header.Seq);
            EndBattle();
        });
        _conn.Recv<Touch>(HandleTouch);
        _conn.Recv<Collect>(HandleCollect);
        _conn.Recv<GetCollectible>(HandleGetCollectible);
        _conn.Recv<PlayEmoticon>(delegate(PlayEmoticon msg, PacketHeader header)
        {
            if (!ServerConfig.Current.Features.Emotes)
            {
                return;                 // ปิดอยู่ตามขอบเขต beta (Features.Emotes)
            }
            // M-1: บังคับ EntityId เป็นของจริง ไม่งั้นสั่งให้ตัวละครคนอื่นเล่นท่าทางได้
            msg.EntityId = EntityId;
            Console.WriteLine("[emote] {0} -> {1}", EntityId, msg.EmoticonId);
            _world.BroadcastToViewers(EntityId, msg);
        });
        _conn.Recv<SayInExclusiveChannel>(delegate(SayInExclusiveChannel msg, PacketHeader header)
        {
            if (!AcceptChat(ref msg.Message))
            {
                return;
            }
            Console.WriteLine("[chat] {0}: {1}", Name, msg.Message.Body);
            msg.Message = StampSpeaker(msg.Message);
            // broadcast กลับหาคนส่งด้วยถูกแล้ว — client ไม่ได้เพิ่มข้อความตัวเองลง log ตอนส่ง
            _world.Broadcast(msg);
        });
        _conn.Recv<SayInConversation>(delegate(SayInConversation msg, PacketHeader header)
        {
            if (!AcceptChat(ref msg.Message))
            {
                return;
            }
            Console.WriteLine("[chat-conv] {0}: {1}", Name, msg.Message.Body);
            msg.Message = StampSpeaker(msg.Message);
            _world.Broadcast(msg);
        });
        _conn.Recv<DisappearEntityOnTile>(delegate(DisappearEntityOnTile msg, PacketHeader header)
        {
            // GP-09: เดิมลบตาม tile ที่ client บอกดื้อ ๆ — ส่ง packet รัวก็ถางป่าทั้งแมพจากมุมไหนก็ได้
            // ตอนนี้ต้องเป็นจุดที่เคยแตะ (server รู้ tile เอง) และต้องอยู่ในระยะเอื้อม
            if (!_world.TryGetNaturalTile(msg.EntityId, out Point2 tile))
            {
                Console.WriteLine("[natural-remove] ปฏิเสธ {0}: ยังไม่ได้แตะ {1}", Name, msg.EntityId);
                return;
            }
            if (!IsWithinReach(tile))
            {
                Console.WriteLine("[natural-remove] ปฏิเสธ {0}: tile {1},{2} ไกลเกินเอื้อม", Name, tile.x, tile.y);
                return;
            }
            Console.WriteLine("[natural-remove] {0} tile={1},{2}", EntityId, tile.x, tile.y);
            if (_world.Terrain.RemoveNatural(tile.x, tile.y))
            {
                _world.ForgetNaturalTile(msg.EntityId);
                _world.MarkDirty();   // GP-07
                _world.BroadcastNear(new WorldPosition(tile.x * 200f + 100f, tile.y * 200f + 100f), new DisappearEntityOnTile { EntityId = msg.EntityId, Tile = tile });
            }
        });
        RegisterFarmingHandlers();      // PlantSeed / WaterPlant / FertilizePlant / UprootPlant / DrawWater
        RegisterWarpAcceleratorHandlers();   // GetWarpAcceleratorCost / Accelerate / ParticipateAcceleration / ReceiveAcceleratorRewards
        _conn.Recv<OccupyArtifactSite>(HandleOccupyArtifactSite);
        _conn.Recv<PutMaterialsIntoArtifact>(delegate(PutMaterialsIntoArtifact msg, PacketHeader header)
        {
            Send(default(OK), header.Seq);
        });
        _conn.Recv<BuildArtifact>(HandleBuildArtifact);
        _conn.Recv<GetArtifact>(HandleGetArtifact);
        _conn.Recv<DestructArtifact>(HandleDestructArtifact);
        _conn.Recv<EstimateBuild>(delegate(EstimateBuild msg, PacketHeader header)
        {
            Send(new BuildEstimation
            {
                Level = 1,
                Durability = 1f,
                Tags = new Dictionary<string, int>(),
                UnrevealedRareTagCount = 0,
                ArtifactPreview = default
            }, header.Seq);
        });
    }

    /// <summary>
    /// GP-05: เติมชื่อคนพูดลงในข้อความก่อน broadcast
    /// client เช็ค <c>if (msg.Message.Speaker.HasValue)</c> ก่อนตั้งชื่อในกล่องแชท
    /// ถ้าไม่เติม แชทจะขึ้นแต่ไม่มีชื่อ และ EntityId ก็ถูกบังคับเป็นของจริงกัน client ปลอมเป็นคนอื่น
    /// </summary>
    private Message_ StampSpeaker(Message_ message)
    {
        message.EntityId = EntityId;
        message.Speaker = new RadioId
        {
            Name = Name,
            Freq = 0
        };
        return message;
    }

    private void HandleMove(Move msg, PacketHeader header)
    {
        // M-1: ห้าม broadcast packet ดิบ — ของเดิมส่งต่อ EntityId ที่ client ใส่มา
        // ⇒ ส่ง Move{EntityId="<id ของคนอื่น>"} แล้วทุกคนจะเห็นเขาวิ่งไปมาตามที่เราสั่ง
        // (ปลอมเป็นสัตว์ก็ได้ ทำให้ client คนอื่น desync)
        msg.EntityId = EntityId;
        // M-2: ตรวจความเร็วก่อน ถ้าเร็วเกินมนุษย์ = ไม่ยอมรับตำแหน่ง และดึง client กลับ
        if (!RememberPosition(msg))
        {
            return;
        }
        _world.BroadcastToViewers(EntityId, msg);
    }


    // M-2: ผู้เล่นวิ่งเร็วสุด 500 หน่วย/วินาที (client/PlayerController.DefaultMoveSpeed)
    // ถนนคูณ 1.2 ⇒ 600 · เผื่อ latency/ความคลาดเคลื่อนเป็น 900
    private const float MaxMoveSpeed = 900f;

    /// <summary>ระยะที่ยอมให้เกินได้เสมอ (แพ็กเก็ตมาถี่ ๆ ระยะสั้น ๆ ไม่ควรโดนจับ)</summary>
    private const float MoveSlack = 300f;

    /// <summary>
    /// เพดานเวลาที่เอามาคูณความเร็ว — ถ้าไม่จำกัด คนที่ยืนนิ่ง 1 นาทีจะ "สะสมโควตา"
    /// แล้ววาร์ปข้ามแมพได้ในแพ็กเก็ตเดียว
    /// </summary>
    private const double MaxMoveWindow = 2.0;

    /// <summary>ความยาวข้อความแชทสูงสุด — ข้อความเดียวถูก broadcast ให้ทุกคน คนเดียวจึงกินแบนด์วิดท์คูณจำนวนคน</summary>
    private const int MaxChatLength = 200;

    /// <summary>เว้นระยะขั้นต่ำระหว่างข้อความ (วินาที)</summary>
    private const double ChatCooldown = 0.7;

    private double _lastChatAt;

    /// <summary>
    /// กรองแชทก่อน broadcast — ตัดข้อความยาวเกินและกันสแปม
    /// (M-6 จำกัด packet รวมไว้ 120/วิ แล้ว แต่แชท 1 ข้อความ = ส่งออก N ข้อความตามจำนวนคนในโลก)
    /// คืน false ถ้าไม่ควรส่งต่อ — เงียบ ๆ ไม่ตอบ Abort เพราะ client ไม่ได้รอคำตอบของแชทอยู่แล้ว
    /// </summary>
    private bool AcceptChat(ref Message_ message)
    {
        if (!ServerConfig.Current.Features.Chat) return false;
        // Body เป็น object เพราะ protocol เดิมใส่ได้ทั้งข้อความและ payload อย่างอื่น (เช่นอีโมติคอน)
        string body = message.Body as string;
        if (body == null)
        {
            // ไม่ใช่ข้อความล้วน — ปล่อยผ่าน แต่ยังนับ cooldown
            double t = Times.UnixTimeNow();
            if (t - _lastChatAt < ChatCooldown) return false;
            _lastChatAt = t;
            return true;
        }
        if (string.IsNullOrWhiteSpace(body))
        {
            return false;
        }
        double now = Times.UnixTimeNow();
        if (now - _lastChatAt < ChatCooldown)
        {
            return false;
        }
        _lastChatAt = now;
        if (body.Length > MaxChatLength)
        {
            message.Body = body.Substring(0, MaxChatLength);
        }
        return true;
    }

    private double _lastMoveAt;
    private double _lastSpeedWarnAt;

    /// <summary>เก็บจุดปลายทางของ movement ล่าสุดไว้เป็นตำแหน่งปัจจุบันของผู้เล่น (GP-02)
    /// คืน false ถ้าไม่ยอมรับ (เร็วเกินจริง — M-2)</summary>
    private bool RememberPosition(Move msg)
    {
        Movement[] movements = msg.Movements;
        if (movements == null || movements.Length == 0)
        {
            return true;
        }
        Movement last = movements[movements.Length - 1];
        Location[] path = last.Path;
        if (path == null || path.Length == 0)
        {
            return true;
        }
        Location dest = path[path.Length - 1];

        double nowSec = Times.UnixTimeNow();
        float moveDx = dest.Position.x - _lastPosition.x;
        float moveDy = dest.Position.y - _lastPosition.y;
        float moveDistance = _hasPosition ? MathF.Sqrt(moveDx * moveDx + moveDy * moveDy) : 0f;
        if (_hasPosition && _lastMoveAt > 0.0)
        {
            double dt = Math.Min(Math.Max(nowSec - _lastMoveAt, 0.05), MaxMoveWindow);
            float dx = moveDx;
            float dy = moveDy;
            float dist = moveDistance;
            float allowed = (float)(MaxMoveSpeed * dt) + MoveSlack;
            if (dist > allowed)
            {
                // เกินนิดหน่อย (เน็ตกระตุกแล้ว client ส่ง Move ที่ค้างมาทีเดียว) = ไม่รับ move นี้เฉย ๆ
                // เกินเยอะ (วาร์ปจริง) = สั่ง Teleported ให้ client เด้งกลับ
                //
                // ทำไมไม่เด้งกลับทุกครั้ง: client กิน `Teleported` ด้วยการ **ขึ้นจอโหลด** (PlayerController.Teleport)
                // ถ้าเด้งทุกครั้งที่คำนวณคลาดนิดเดียว คนเล่นปกติจะเจอจอโหลดกะพริบไปตลอดทาง
                // ส่วน `Move` ที่ยิงกลับไปตรง ๆ client ไม่สนใจอยู่แล้วเพราะ PlayerManager ข้าม Move ของตัวเอง
                bool blatant = dist > allowed * 3f;
                if (blatant)
                {
                    SendTeleport(_lastPosition);
                }
                if (nowSec - _lastSpeedWarnAt > 3.0)
                {
                    _lastSpeedWarnAt = nowSec;
                    Console.WriteLine("[move] ปฏิเสธ {0}: ขยับ {1:F0} หน่วยใน {2:F2} วิ (เพดาน {3:F0}){4}",
                        Name, dist, dt, allowed, blatant ? " — ดึงกลับที่เดิม" : "");
                }
                return false;
            }
        }
        _lastMoveAt = nowSec;
        _lastPosition = dest.Position;
        _lastYaw = dest.Yaw;
        // ความสูงพื้นที่ client รายงานมา — server ไม่มี heightmap ของแมพ
        // เอาไว้ใช้ตอนสร้าง entity อื่นแถวนั้น (สัตว์ที่ Height=0 จะจมอยู่ใต้พื้น)
        _lastHeight = dest.Height;
        _lastFloor = dest.Floor;
        _world.NoteGroundHeight(dest.Height);
        _hasPosition = true;
        // Move packets ที่เกิดจากการ snap เข้า attachment/แก้ jitter ไม่ควรทำให้การพักหลุด
        // หยุดพักเฉพาะเมื่อผู้เล่นขยับจริงเกินระยะ epsilon
        if (moveDistance > 10f)
        {
            StopResting();        // ลุกเดินจริงแล้วเลิกพัก ความล้ากลับไปไต่ขึ้นตามเวลา
        }
        CheckReachQuests();       // เควส "เดินไปถึงจุด" (เช่น ไปหาดเหนือเจอ K)
        MarkDirty();              // GP-07
        return true;
    }

    /// <summary>ตั้งตำแหน่งฝั่ง server ตรง ๆ (ใช้ตอนวาร์ป/ฟื้นจากตาย)</summary>
    private void RememberPosition(WorldPosition pos, float yaw)
    {
        _lastPosition = pos;
        _lastYaw = yaw;
        _hasPosition = true;
        _lastMoveAt = Times.UnixTimeNow();   // M-2: เริ่มนับความเร็วใหม่จากจุดนี้
        MarkDirty();
    }

    /// <summary>กรอบ chunk ที่ส่ง garden ให้คนนี้ไปแล้วรอบก่อน (−1 = ยังไม่เคยส่ง)</summary>
    private int _sentChunkCx = -1;
    private int _sentChunkCy = -1;
    private int _sentChunkRange = -1;

    /// <summary>
    /// client ขยับไปอยู่ chunk ใหม่ → ส่งข้อมูลต้นไม้/หิน (garden) ของกรอบรอบตัวให้
    ///
    /// 🐛 **ต้นตออาการ "วิ่งไปแล้วแมพรีเฟรชเป็นระยะ"**
    /// ฝั่ง client (ChunkPool.LoadChunk) ถ้าได้ข้อมูลของ chunk ที่โหลดไว้แล้ว
    /// มัน **Reset ทิ้งแล้วสร้างใหม่ทั้งก้อน** ไม่ได้เช็คว่าข้อมูลเหมือนเดิมไหม
    /// ⇒ เดิมส่งทั้งกรอบทุกครั้งที่ข้ามขอบ chunk = **สร้างพื้น/ต้นไม้/หญ้าใหม่ทั้งจอ**
    ///
    /// แก้โดยส่งเฉพาะ chunk ที่ "เพิ่งเข้ามาในกรอบ" (อยู่ในกรอบใหม่ แต่ไม่อยู่ในกรอบเก่า)
    /// chunk ที่ยังอยู่ในกรอบเดิม client มีอยู่แล้วและไม่ต้องแตะ
    ///
    /// กรอบต้องกว้างเท่า `_visibleRange` ของ client (Durango.Terrain/TerrainBase.InitChunkPool)
    /// แคบกว่า = วงนอกไม่มีต้นไม้ แล้วต้องส่งตามทีหลัง = กลับไปรีเฟรชเหมือนเดิม
    /// </summary>
    private void HandleSetChunk(SetChunk msg, PacketHeader header)
    {
        int cx = Math.Clamp(msg.Chunk.x, 0, _world.Terrain.NumChunksX - 1);
        int cy = Math.Clamp(msg.Chunk.y, 0, _world.Terrain.NumChunksY - 1);
        int range = ServerConfig.Current.World.ChunkSendRange;
        bool hadPrevious = _sentChunkRange == range && _sentChunkCx >= 0;

        int sent = 0;
        for (int i = cx - range; i <= cx + range; i++)
        {
            for (int j = cy - range; j <= cy + range; j++)
            {
                if (i < 0 || i >= _world.Terrain.NumChunksX || j < 0 || j >= _world.Terrain.NumChunksY)
                {
                    continue;
                }
                // เคยอยู่ในกรอบเดิมแล้ว = client ยังถืออยู่ ไม่ต้องส่งซ้ำ
                if (hadPrevious
                    && Math.Abs(i - _sentChunkCx) <= range
                    && Math.Abs(j - _sentChunkCy) <= range)
                {
                    continue;
                }
                Send(new Chunk
                {
                    _Chunk = new Point2(i, j),
                    Garden = _world.Terrain.GetChunkGarden(i, j) ?? new byte[0]
                });
                sent++;
            }
        }
        _sentChunkCx = cx;
        _sentChunkCy = cy;
        _sentChunkRange = range;
        if (sent > 0)
        {
            Console.WriteLine("[chunk] {0} ย้ายไป chunk {1},{2} — ส่ง garden ใหม่ {3} ก้อน", Name, cx, cy, sent);
        }
    }
}
