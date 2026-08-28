using System.Collections.Generic;

namespace DurangoServer.Core;

/// <summary>
/// Beta 1.0 — **นิยามเควส**
///
/// ## ทำไมต้องเขียนตารางเอง
///
/// ข้อมูลเกมมีเควส **1,386 อัน** ในตาราง `quests_for_client` แต่มีแค่ 8 ฟิลด์
/// (`subject` · `description` · `icon` · `order` · `category` · `quest_type` · `display_on_hud` · `auto_finish`)
/// — **ไม่มีเงื่อนไข ไม่มีเป้าหมาย ไม่มีรางวัล ไม่มีจำนวนที่ต้องทำ สักฟิลด์เดียว**
/// ตรรกะทั้งหมดอยู่ฝั่ง server ของ NEXON ซึ่งไม่ได้ติดมากับ client (โรคเดียวกับ `exp_amount` ของสัตว์)
///
/// ⇒ **เราใช้ id ของจริง** (client จะได้หยิบชื่อ/คำอธิบาย/ไอคอนของแท้มาวาดให้)
///   แล้ว **เขียนเงื่อนไขกับรางวัลเอง** ที่นี่
///
/// ## เลือก id ยังไง
///
/// เลือกอันที่ **คำบรรยายเกาหลีของมันตรงกับสิ่งที่เราจะให้ทำจริง ๆ** และตัวเลขในคำบรรยาย
/// ตรงกับ <see cref="Quest.Count"/> ที่ตั้งไว้ — ไม่งั้นผู้เล่นจะเห็นคำอธิบายอย่างหนึ่งแต่ต้องทำอีกอย่าง
///
/// ตัวอย่าง: `anniversary_1st_03` คำบรรยายคือ "동물 1마리 사냥하기" (ล่าสัตว์ 1 ตัว) ⇒ Count = 1
///
/// ## ภาษา
///
/// `Thai` คือข้อความไทยของเราเอง ใช้ส่งทาง `Info` ตอนรับ/ทำสำเร็จ
/// **ทำงานได้ทันทีโดยไม่ต้องรอเรื่องฟอนต์/คำแปล** ส่วนชื่อในหน้าต่างเควสยังเป็นเกาหลี
/// จนกว่าจะเปิดแค็ตตาล็อกไทยได้ (ดู docs/client/TUNING.md §2.1)
/// </summary>
public static class QuestData
{
    /// <summary>สิ่งที่ต้องทำให้ครบ — ผูกกับตัวนับที่ server มีอยู่แล้วทุกตัว</summary>
    public enum Goal
    {
        /// <summary>เก็บของจากธรรมชาติกี่ครั้ง (ทุบแร่/ตัดไม้ก็นับ)</summary>
        Gather,
        /// <summary>เก็บของที่เป็น prototype ที่ระบุ กี่ชิ้น (Param = prototype)</summary>
        GatherItem,
        /// <summary>ล่าสัตว์กี่ตัว</summary>
        Hunt,
        /// <summary>แล่ซากได้ชิ้นส่วนกี่ชิ้น</summary>
        Butcher,
        /// <summary>คราฟต์กี่ครั้ง (Param = หมวดสูตร เช่น "tool" · ว่าง = หมวดไหนก็ได้)</summary>
        Craft,
        /// <summary>ทำอาหารกี่ครั้ง</summary>
        Cook,
        /// <summary>สร้างสิ่งปลูกสร้างกี่ครั้ง (Param = blueprint id · ว่าง = อะไรก็ได้)</summary>
        Build,
        /// <summary>ไปให้ถึงเลเวลที่ระบุ</summary>
        Level,
        /// <summary>กินอาหารกี่ครั้ง</summary>
        Eat,
        /// <summary>ลงเมล็ดกี่ครั้ง (Param = prototype ของเมล็ด · ว่าง = เมล็ดอะไรก็ได้)</summary>
        Plant,
        /// <summary>เก็บเกี่ยวได้กี่ชิ้น (Param = prototype ของผลผลิต · ว่าง = อะไรก็ได้)</summary>
        Harvest,

        // ── ตัวนับที่เพิ่มมาเพื่อ "เควสประจำวัน" (ดู Checklist ข้างล่าง) ──
        /// <summary>รดน้ำต้นไม้กี่ครั้ง</summary>
        Water,
        /// <summary>ใส่ปุ๋ยกี่ครั้ง</summary>
        Fertilize,
        /// <summary>ตักน้ำกี่ครั้ง</summary>
        DrawWater,
        /// <summary>สวมใส่อุปกรณ์กี่ครั้ง (Param = ช่อง เช่น "main" · ว่าง = ช่องไหนก็ได้)</summary>
        Equip,
        /// <summary>ซ่อมของกี่ครั้ง</summary>
        Repair,
        /// <summary>เอาของเก็บเข้ากล่องกี่ชิ้น</summary>
        Store,
        /// <summary>เรียนสกิลกี่ครั้ง</summary>
        LearnSkill,
        /// <summary>ฟื้นจากการตายกี่ครั้ง</summary>
        Revive,
        /// <summary>ล่าสัตว์ด้วยอาวุธระยะไกลกี่ตัว</summary>
        HuntRanged,
        /// <summary>เดินไปให้ถึงจุดที่กำหนดบนแผนที่ (Param = ชื่อจุด เช่น "north_beach")</summary>
        Reach
    }

    /// <summary>รางวัลเมื่อกดรับ</summary>
    public sealed class Reward
    {
        public readonly int Exp;
        public readonly int SkillPoints;
        /// <summary>ไอเทมที่ได้: prototype → จำนวน</summary>
        public readonly (string Prototype, int Count)[] Items;

        public Reward(int exp, int skillPoints = 0, params (string, int)[] items)
        {
            Exp = exp;
            SkillPoints = skillPoints;
            Items = items ?? System.Array.Empty<(string, int)>();
        }
    }

    public sealed class Quest
    {
        /// <summary>id ของจริงจากข้อมูลเกม — client เอาไปหาชื่อ/คำอธิบาย/ไอคอน</summary>
        public readonly string Id;
        /// <summary>หมวดที่ server ประกาศเอง (ไม่ต้องตรงกับ category ในข้อมูลเกม)</summary>
        public readonly string Category;
        public readonly Goal Kind;
        public readonly string Param;
        public readonly int Count;
        /// <summary>ต้องทำเควสไหนให้เสร็จก่อนถึงจะโผล่ (ว่าง = เปิดตั้งแต่แรก)</summary>
        public readonly string Requires;
        public readonly Reward Prize;
        /// <summary>ข้อความไทยของเรา — ใช้ตอนแจ้งผ่าน Info</summary>
        public readonly string Thai;

        public Quest(string id, string category, Goal kind, string param, int count,
            string requires, Reward prize, string thai)
        {
            Id = id;
            Category = category;
            Kind = kind;
            Param = param;
            Count = count;
            Requires = requires;
            Prize = prize;
            Thai = thai;
        }
    }

    /// <summary>หมวด "สายหลัก" — client แสดงเด่นกว่าหมวดอื่น (ช่อง Epic ของ QuestCategories)</summary>
    public const string MainCategory = "sunset";

    /// <summary>
    /// หมวดของ "เควสประจำวัน" (เดิม: รายการตรวจเซิร์ฟ) — โผล่เป็น **แท็บแยก** ในหน้าต่างเควส
    ///
    /// 💡 ชื่อแท็บมาจาก <c>QuestCategory.Name</c> ที่ **server ส่งเอง** ⇒ ใส่ภาษาไทยได้เลย
    ///    (ต่างจากชื่อเควสรายอันที่ client หยิบจากตารางในตัวเกมเป็นเกาหลี)
    ///
    /// ใช้ key "daily" ให้ตรงกับหมวดเควสรายวันของเกม (client ไม่ได้ผูก key กับตาราง —
    /// แท็บ/ความคืบหน้าวิ่งตาม key ที่ server ส่ง)
    /// </summary>
    public const string ChecklistCategory = "daily";

    /// <summary>
    /// เควส "ต่อแพหนีเกาะ" — จุดจบของสายสอนเล่น และเป็นหมุดที่ `cheat questskip` ใช้หยุด
    /// (ผูกด้วย id ไม่ใช่ตำแหน่งในอาเรย์ — สาย Story ยาวขึ้นได้เรื่อย ๆ)
    /// </summary>
    public const string RaftQuestId = "story_enter_safehouse";

    /// <summary>
    /// **สายสอนเล่น → ต่อแพหนีเกาะ**
    ///
    /// ไล่ตามวงจรหลักของเกมพอดี: เก็บของ → คราฟต์เครื่องมือ → เครื่องมือเปิดทางให้เก็บของที่ดีกว่า
    /// → ล่า → แล่ → ทำอาหาร → สร้าง → **ต่อแพ** (ตรงกับ docs/project/GOAL.md ที่บอกว่าเกมนี้วัดความคืบหน้า
    /// จาก "เมื่อวานยังตัดไม้ไม่ได้ วันนี้มีขวานเหล็กแล้ว")
    ///
    /// ปลายทางคือ `story_enter_safehouse` — เควสเนื้อเรื่องจริงของเกม
    /// คำบรรยายเกาหลีของมันคือ **"앙코라를 탈출해야 합니다. 뗏목을 만드세요."**
    /// (ต้องหนีออกจากอังโครา — สร้างแพ) ตรงกับที่จะให้ทำเป๊ะ
    ///
    /// แพที่ใช้คือ blueprint `tutorial_boat` (탈출용 뗏목 = "แพหนีภัย")
    /// ต้องใช้ **ท่อนซุง 4 + ก้าน 6** · ไม่ต้องใช้เครื่องมือ · เลเวล 1 ก็สร้างได้
    /// ทั้งสองอย่างเก็บได้บนเกาะเราอยู่แล้ว (`wood_log` มี tag `pillar_normal` · `stem` มี tag `stem`)
    /// </summary>
    public static readonly Quest[] Story =
    {
        new Quest("event_2018_fall_3_17_any_gathering_01", MainCategory, Goal.Gather, null, 10,
            null, new Reward(20, 0, ("stem", 3)),
            "เก็บของจากธรรมชาติ 10 ครั้ง — กดค้างที่ต้นไม้หรือพุ่มไม้รอบตัว"),

        new Quest("daily_weaponcrafting_b_01", MainCategory, Goal.Craft, "tool", 5,
            "event_2018_fall_3_17_any_gathering_01", new Reward(30, 1),
            "คราฟต์เครื่องมือ 5 ชิ้น — ไม่มีขวานก็ตัดไม้ไม่ได้ ไม่มีมีดก็แล่ซากไม่ได้"),

        new Quest("customize_estate_quest_05", MainCategory, Goal.GatherItem, "wood_log", 10,
            "daily_weaponcrafting_b_01", new Reward(40, 0, ("stem", 6)),
            "เก็บท่อนซุง 10 อัน — ต้องมีขวานถึงจะตัดไม้ใหญ่ได้ (แพต้องใช้ 4 อัน)"),

        new Quest("anniversary_1st_03", MainCategory, Goal.Hunt, null, 1,
            "customize_estate_quest_05", new Reward(40, 0),
            "ล่าสัตว์ให้ได้ 1 ตัว — แตะตัวสัตว์แล้วกดปุ่มโจมตี"),

        new Quest("permanent_butchery_meat_01", MainCategory, Goal.Butcher, null, 10,
            "anniversary_1st_03", new Reward(50, 1),
            "แล่ซากให้ได้ 10 ชิ้น — ต้องถือมีดและออกจากโหมดต่อสู้ก่อนถึงจะแตะซากได้"),

        new Quest("permanent_cooking_any_01", MainCategory, Goal.Cook, null, 5,
            "permanent_butchery_meat_01", new Reward(60, 0),
            "ทำอาหาร 5 ครั้ง — เนื้อดิบให้พลังแค่ 60% และทำให้ล้าเพิ่ม เอาไปย่างที่กองไฟก่อน"),

        new Quest("daily_constructing_a_01", MainCategory, Goal.Build, null, 2,
            "permanent_cooking_any_01", new Reward(60, 1),
            "สร้างสิ่งปลูกสร้าง 2 อย่าง — ลองวางกองไฟกับกล่องเก็บของดู"),

        new Quest("story_enter_safehouse", MainCategory, Goal.Build, "tutorial_boat", 1,
            "daily_constructing_a_01", new Reward(150, 2, ("stem", 10)),
            "ต่อแพหนีเกาะ! ใช้ท่อนซุง 4 + ก้าน 6 — นี่คือทางออกจากอังโครา"),

        // ── เนื้อเรื่องจริงของเกม ต่อจากต่อแพ (หมวด "sunset") ──
        // ใช้ id จริงจากตาราง quests_for_client — client หยิบชื่อ/คำอธิบาย/ไอคอนจากตารางให้เอง
        new Quest("story_custom_event_meet_k", MainCategory, Goal.Reach, "north_beach", 1,
            "story_enter_safehouse", new Reward(80, 1),
            "ขึ้นฝั่งแล้วเดินไปที่หาดทางเหนือ — เจอหญิงสาวปริศนา K ยืนรออยู่"),

        new Quest("story_enter_personal", MainCategory, Goal.Craft, null, 5,
            "story_custom_event_meet_k", new Reward(100, 1),
            "K ฝากให้คุ้นเคยกับดูแรงโก — คราฟต์สิ่งของ 5 ชิ้นจากโต๊ะงาน"),

        new Quest("story_enter_risky", MainCategory, Goal.Hunt, null, 3,
            "story_enter_personal", new Reward(120, 1),
            "ประกาศตัวเป็นผู้บุกเบิก! ออกไปล่าสัตว์ 3 ตัวตามทุ่งรอบเกาะ"),

        new Quest("story_level_16", MainCategory, Goal.Level, null, 16,
            "story_enter_risky", new Reward(200, 2),
            "ถึงเลเวล 16 — พิสูจน์ว่าอยู่รอดในดูแรงโกได้จริง (เควสจากเนื้อเรื่อง: ถึง Lv.16)")
    };

    /// <summary>
    /// **เควสประจำวัน — เอาเกณฑ์เทสเซิร์ฟมาใส่เป็นเควส**
    ///
    /// ## ทำไมทำแบบนี้
    ///
    /// เกณฑ์เปิด beta ข้อ 3 คือ "เล่นด้วยตัวเกมจริง 30 นาที แล้วดูว่าระบบไหนพัง"
    /// ปัญหาคือคนเทสต้องถือกระดาษเช็คลิสต์ไว้ข้าง ๆ แล้วไล่ทำเอง — ลืมข้อง่ายมาก
    ///
    /// เอารายการนั้นมาใส่เป็นเควสซะเลย ⇒ **หน้าต่างเควสในเกมกลายเป็นเช็คลิสต์**
    /// เดินเล่นไปเรื่อย ๆ ก็รู้เองว่าเหลือระบบไหนที่ยังไม่ได้ลอง และ **ตัวนับมาจาก server**
    /// ไม่ใช่ความจำของคนเทส — ข้อไหนขึ้นแปลว่า packet เดินทางครบวงจรจริง
    ///
    /// ## ปิดยังไงตอนเทสผ่านแล้ว
    ///
    /// `data/config.json` → `"Features": { "QuestChecklist": false }` — หายทั้งชุดทันที
    /// ไม่ต้อง build ใหม่ (hot-reload 5 วิ) และสายสอนเล่นไม่กระทบ
    ///
    /// ## ทำไมไม่เรียงเป็นสาย
    ///
    /// ทุกข้อ `Requires = null` ⇒ **เปิดพร้อมกันหมด** เพราะเป็นเช็คลิสต์ ไม่ใช่บทเรียน
    /// คนเทสอยากลองระบบไหนก่อนก็ได้
    ///
    /// ## ⚠️ เรื่อง id กับชื่อที่ขึ้นในเกม
    ///
    /// สายสอนเล่นเลือก id ที่ "คำบรรยายเกาหลีตรงกับสิ่งที่ให้ทำ" ได้ทุกข้อ
    /// แต่ชุดนี้ทำแบบนั้นไม่ได้ครบ — ข้อมูลเกม **ไม่มีเควสสำหรับ รดน้ำ/ใส่ปุ๋ย/เก็บของเข้ากล่อง/ฟื้นจากตาย** เลย
    /// จึงเลือก id ที่ *ใกล้เคียงที่สุด* แล้วให้ **ข้อความไทยเป็นคำสั่งจริง** (ส่งทาง Info ตอนเควสโผล่)
    ///
    /// ยอมได้เพราะชุดนี้เป็นของชั่วคราวสำหรับเทส ไม่ใช่เนื้อหาที่ผู้เล่นจริงจะเห็น
    /// (id ที่ไม่มีในข้อมูลเกมเลยใช้ไม่ได้ — client `null-check` แล้ว **ไม่วาดอะไรเลย** ไม่ได้พัง แต่ก็ไม่โผล่)
    /// </summary>
    public static readonly Quest[] Checklist =
    {
        // ── ระบบปลูกผัก (ทำใหม่ล่าสุด ยังไม่เคยเจอตัวเกมจริง) ──────────
        new Quest("permanent_farming_seed_01", ChecklistCategory, Goal.Plant, "corn_seed", 4,
            null, new Reward(20, 0),
            "[ตรวจ] ปลูกเมล็ดข้าวโพด 4 ครั้ง — สร้างแปลงผักก่อน แล้วแตะแปลงเลือก \"ปลูก\""),

        new Quest("event_1_farming_cherry_01", ChecklistCategory, Goal.Water, null, 2,
            null, new Reward(15, 0),
            "[ตรวจ] รดน้ำต้นไม้ 2 ครั้ง — ไม่รดน้ำต้นจะตายตอนโตครบ"),

        new Quest("event_1_farming_cherry_02", ChecklistCategory, Goal.Fertilize, null, 2,
            null, new Reward(15, 0),
            "[ตรวจ] ใส่ปุ๋ย 2 ครั้ง — ปุ๋ยเป็นตัวกำหนดว่าจะได้ผลผลิตกี่ชิ้น"),

        new Quest("event_1_gathering_cherry_bough_01", ChecklistCategory, Goal.Harvest, null, 3,
            null, new Reward(25, 0),
            "[ตรวจ] เก็บเกี่ยวผลผลิต 3 ชิ้น — แตะแปลงที่โตแล้วเลือก \"เก็บ\""),

        new Quest("event_newyear_2019_quest_04", ChecklistCategory, Goal.DrawWater, null, 1,
            null, new Reward(15, 0),
            "[ตรวจ] ตักน้ำ 1 ครั้ง — ต้องถือภาชนะและยืนใกล้แม่น้ำ/ทะเลสาบ"),

        // ── ระบบที่สายสอนเล่นไม่ได้แตะ ────────────────────────────────
        new Quest("urban_weapon_event_04", ChecklistCategory, Goal.Equip, null, 2,
            null, new Reward(20, 0),
            "[ตรวจ] สวมอุปกรณ์ 2 ชิ้น — ดูว่าตัวละครเปลี่ยนหน้าตาและค่าพลังขึ้นจริงไหม"),

        new Quest("urban_weapon_event_10", ChecklistCategory, Goal.HuntRanged, null, 1,
            null, new Reward(30, 0),
            "[ตรวจ] ล่าสัตว์ด้วยธนู 1 ตัว — เทสสายโจมตีระยะไกล (คนละระบบกับตีประชิด)"),

        new Quest("mainstory_chapter4_6", ChecklistCategory, Goal.Repair, null, 1,
            null, new Reward(20, 0),
            "[ตรวจ] ซ่อมของ 1 ครั้ง — ใช้เครื่องมือจนความทนทานลด แล้วซ่อมด้วยชุดซ่อม"),

        new Quest("estate_build_lv55_02", ChecklistCategory, Goal.Store, null, 3,
            null, new Reward(20, 0),
            "[ตรวจ] เอาของเก็บเข้ากล่อง 3 ชิ้น — วางกล่องแล้วแตะเปิด ลองหยิบออกด้วย"),

        new Quest("permanent_level_skill_gathering_10", ChecklistCategory, Goal.LearnSkill, null, 1,
            null, new Reward(20, 0),
            "[ตรวจ] เรียนสกิล 1 อัน — เปิดหน้าสกิล ใช้แต้มที่ได้จากการขึ้นเลเวล"),

        new Quest("mainstory_chapter1_5", ChecklistCategory, Goal.Revive, null, 1,
            null, new Reward(20, 0),
            "[ตรวจ] ตายแล้วฟื้น 1 ครั้ง — ดูว่าจอเด้งกลับจุดเกิดและของไม่หาย"),

        new Quest("urban_cook_event_06", ChecklistCategory, Goal.Eat, null, 1,
            null, new Reward(15, 0),
            "[ตรวจ] กินอาหาร 1 ครั้ง — ดูว่าสตามินาขึ้นและความล้าลดจริง")
    };

    /// <summary>สายสอนเล่น + รายการตรวจ (รายการตรวจถูกกรองออกตอนส่งถ้าปิดใน config)</summary>
    public static readonly Quest[] All = Combine(Story, Checklist);

    private static Quest[] Combine(Quest[] a, Quest[] b)
    {
        var all = new Quest[a.Length + b.Length];
        a.CopyTo(all, 0);
        b.CopyTo(all, a.Length);
        return all;
    }

    private static readonly HashSet<string> _checklistIds = BuildChecklistIds();

    private static HashSet<string> BuildChecklistIds()
    {
        var set = new HashSet<string>(System.StringComparer.Ordinal);
        for (int i = 0; i < Checklist.Length; i++)
        {
            set.Add(Checklist[i].Id);
        }
        return set;
    }

    /// <summary>เควสนี้เป็นข้อในรายการตรวจไหม (ปิดได้ด้วย Features.QuestChecklist)</summary>
    public static bool IsChecklist(string id)
    {
        return !string.IsNullOrEmpty(id) && _checklistIds.Contains(id);
    }

    /// <summary>สร้างครั้งเดียวตอนโหลดคลาส — ไม่ทำ lazy init เพราะจะไม่ปลอดภัยถ้ามีหลายเธรดเรียกพร้อมกัน</summary>
    public static readonly Dictionary<string, Quest> ById = BuildIndex();

    private static Dictionary<string, Quest> BuildIndex()
    {
        var map = new Dictionary<string, Quest>(System.StringComparer.Ordinal);
        for (int i = 0; i < All.Length; i++)
        {
            map[All[i].Id] = All[i];
        }
        return map;
    }

    public static bool TryGet(string id, out Quest quest)
    {
        return ById.TryGetValue(id ?? string.Empty, out quest);
    }

    /// <summary>
    /// **ด่านตรวจตารางเควสตอนเปิดเซิร์ฟ** — คืนรายการปัญหา (ว่าง = ตารางใช้ได้)
    ///
    /// ทำไมต้องมี: ตารางเควสเป็นข้อมูลที่คนเขียนด้วยมือ พิมพ์ผิดนิดเดียวแล้ว
    /// **เควสจะเงียบหายไปเฉย ๆ โดยไม่มี error** — เช่น
    ///   · `Requires` พิมพ์ id ผิด ⇒ เควสนั้นไม่มีวันเปิด ผู้เล่นค้างอยู่ครึ่งสาย
    ///   · id ซ้ำกัน ⇒ ความคืบหน้าเดินสองเด้งต่อการกระทำครั้งเดียว
    ///   · `Count` เป็น 0 ⇒ เควสจบตั้งแต่ทำครั้งแรก
    ///   · `Requires` วนกลับมาหาตัวเอง ⇒ ทั้งวงไม่มีวันเปิด
    /// ทั้งหมดนี้จับได้ตอนเปิดเซิร์ฟดีกว่าไปเจอตอนผู้เล่นเล่นค้าง
    /// </summary>
    public static List<string> Validate()
    {
        var problems = new List<string>();
        var seen = new HashSet<string>(System.StringComparer.Ordinal);

        for (int i = 0; i < All.Length; i++)
        {
            Quest q = All[i];
            if (string.IsNullOrWhiteSpace(q.Id))
            {
                problems.Add($"เควสลำดับ {i + 1}: ไม่มี id");
                continue;
            }
            if (!seen.Add(q.Id))
            {
                problems.Add($"{q.Id}: id ซ้ำในตาราง (ความคืบหน้าจะเดินสองเด้งต่อการกระทำครั้งเดียว)");
            }
            if (q.Count < 1)
            {
                problems.Add($"{q.Id}: Count = {q.Count} (ต้องอย่างน้อย 1 ไม่งั้นจบตั้งแต่ทำครั้งแรก)");
            }
            if (q.Prize == null)
            {
                problems.Add($"{q.Id}: ไม่มีรางวัล (Prize เป็น null)");
            }
            if (string.IsNullOrWhiteSpace(q.Thai))
            {
                problems.Add($"{q.Id}: ไม่มีข้อความไทย — ผู้เล่นจะไม่รู้ว่าต้องทำอะไรจนกว่าจะเปิดแค็ตตาล็อกไทย");
            }
            if (string.IsNullOrWhiteSpace(q.Category))
            {
                problems.Add($"{q.Id}: ไม่ได้ระบุหมวด");
            }
        }

        // Requires ต้องชี้ไปเควสที่มีอยู่จริง
        for (int i = 0; i < All.Length; i++)
        {
            Quest q = All[i];
            if (!string.IsNullOrEmpty(q.Requires) && !ById.ContainsKey(q.Requires))
            {
                problems.Add($"{q.Id}: Requires ชี้ไป '{q.Requires}' ซึ่งไม่มีในตาราง ⇒ เควสนี้ไม่มีวันเปิด");
            }
        }

        // หาวงวน — ไล่ตาม Requires ขึ้นไปเรื่อย ๆ ต้องจบที่เควสที่ไม่ต้องการอะไร
        for (int i = 0; i < All.Length; i++)
        {
            var path = new HashSet<string>(System.StringComparer.Ordinal);
            Quest cur = All[i];
            int guard = 0;
            while (cur != null && !string.IsNullOrEmpty(cur.Requires))
            {
                if (!path.Add(cur.Id) || ++guard > All.Length + 1)
                {
                    problems.Add($"{All[i].Id}: สาย Requires วนกลับมาหาตัวเอง ⇒ ทั้งวงไม่มีวันเปิด");
                    break;
                }
                if (!ById.TryGetValue(cur.Requires, out cur))
                {
                    break;      // รายงานไปแล้วข้างบน
                }
            }
        }

        return problems;
    }

    /// <summary>ตรวจตารางแล้วพิมพ์ผลตอนเปิดเซิร์ฟ — คืน false ถ้ามีปัญหา</summary>
    public static bool ValidateAndReport()
    {
        List<string> problems = Validate();
        if (problems.Count == 0)
        {
            System.Console.WriteLine("[quest] ตารางเควช {0} อัน — ตรวจแล้วไม่มีปัญหา", All.Length);
            return true;
        }
        System.Console.WriteLine("[quest] ⚠️ ตารางเควสมีปัญหา {0} ข้อ:", problems.Count);
        for (int i = 0; i < problems.Count; i++)
        {
            System.Console.WriteLine("[quest]   · {0}", problems[i]);
        }
        return false;
    }
}
