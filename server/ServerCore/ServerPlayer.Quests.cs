using System;
using System.Collections.Generic;
using Durango.Network;
using Messages;

namespace DurangoServer.Core;

// ============================================================================
// ServerPlayer.Quests — ระบบเควส (beta 1.0)
//
// โปรโตคอลของเกมง่ายมาก — QuestToDo มีแค่ {Id, Progress, GoalCount, Finished, EndAt, Reward}
// server เป็นคนนับทั้งหมด client แค่วาด "X/Y" ให้ดู
//
//   client → GetQuests {Category}          → server ตอบ Quests {Category, Todos[]}
//   client → GetQuestState {QuestIds[]}    → server ตอบ Quests เหมือนกัน
//   server → NotifyQuestProceed            ตอนความคืบหน้าขยับ (client เด้งตัวเลขบน HUD)
//   server → QuestStarted                  ตอนมีเควสใหม่โผล่
//   client → RequestQuestReward {QuestId}  → server ตอบ QuestRewardResults + ให้ของจริง
//
// **ตัวนับไม่ต้องเขียนใหม่เลย** — เกี่ยวกับจุดที่มีอยู่แล้วทั้งหมด:
// GainExpForGather/Kill/Butchery/Craft/Build · กินอาหาร · ขึ้นเลเวล
//
// ดูตารางนิยามเควสที่ QuestData.cs · เอกสาร docs/server/Quests.md
// ============================================================================

public partial class ServerPlayer
{
    /// <summary>ความคืบหน้าของแต่ละเควส (id → จำนวนที่ทำไปแล้ว)</summary>
    private readonly Dictionary<string, int> _questProgress = new Dictionary<string, int>(StringComparer.Ordinal);

    /// <summary>เควสที่ทำครบแล้ว (ยังไม่ได้กดรับรางวัลก็อยู่ในนี้)</summary>
    private readonly HashSet<string> _questDone = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>เควสที่กดรับรางวัลไปแล้ว — กันกดซ้ำ</summary>
    private readonly HashSet<string> _questRewarded = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>
    /// เควสที่เคยประกาศว่า "เควสใหม่" ไปแล้ว
    ///
    /// 🐛 เดิมใช้เงื่อนไข "เปิดอยู่ + ยังไม่เสร็จ + ความคืบหน้าเป็น 0" ⇒ เควสที่เปิดไว้แต่ผู้เล่น
    /// ยังไม่แตะ จะถูกประกาศ **ซ้ำทุกครั้งที่เควสอื่นสำเร็จ** (ยังไม่เห็นผลตอนสายเดียว
    /// แต่พอมีหลายสายพร้อมกันจะเด้งข้อความรัว) — จึงต้องจำเป็นรายตัว
    /// </summary>
    private readonly HashSet<string> _questAnnounced = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>กันเรียกซ้อน: การให้รางวัลทำให้ได้ exp → ขึ้นเลเวล → เช็คเควสเลเวล → อาจให้รางวัลอีก</summary>
    private bool _grantingQuestReward;

    // ───────────────────────── ตัวช่วย ─────────────────────────

    private static bool QuestsEnabled => ServerConfig.Current.Features.Quests;

    /// <summary>เปิดชุด "เควสประจำวัน" อยู่ไหม — ปิดแล้วเควสชุดนั้นหายจากทุกทาง</summary>
    private static bool ChecklistEnabled => ServerConfig.Current.Features.QuestChecklist;

    /// <summary>เควสนี้ถูกซ่อนเพราะปิดชุดตรวจอยู่ไหม</summary>
    private static bool IsHidden(QuestData.Quest q)
    {
        return !ChecklistEnabled && QuestData.IsChecklist(q.Id);
    }

    /// <summary>เควสนี้เปิดให้ทำแล้วหรือยัง (ทำเควสที่มันต้องการเสร็จหรือยัง)</summary>
    private bool IsQuestOpen(QuestData.Quest q)
    {
        return string.IsNullOrEmpty(q.Requires) || _questDone.Contains(q.Requires);
    }

    private int ProgressOf(string id)
    {
        _questProgress.TryGetValue(id, out int n);
        return n;
    }

    private QuestToDo MakeTodo(QuestData.Quest q)
    {
        bool done = _questDone.Contains(q.Id);
        return new QuestToDo
        {
            Id = q.Id,
            Progress = Math.Min(ProgressOf(q.Id), q.Count),
            GoalCount = q.Count,
            Finished = done && _questRewarded.Contains(q.Id),
            EndAt = 0.0,
            // แนบรางวัลไปด้วยเพื่อให้หน้าต่างเควสโชว์ว่าจะได้อะไร
            Reward = done && !_questRewarded.Contains(q.Id) ? MakeRewardInfo(q.Prize) : (RewardInfo?)null
        };
    }

    private static RewardInfo MakeRewardInfo(QuestData.Reward prize)
    {
        var items = new List<RewardItem>();
        for (int i = 0; i < prize.Items.Length; i++)
        {
            items.Add(new RewardItem
            {
                PrototypeId = prize.Items[i].Prototype,
                Count = prize.Items[i].Count,
                Level = 1,
                NameGettext = ItemNameData.NameOf(prize.Items[i].Prototype, prize.Items[i].Prototype),
                ColorR = "FFFFFF",
                ColorG = "FFFFFF",
                ColorB = "FFFFFF"
            });
        }
        return new RewardInfo
        {
            Exp = prize.Exp > 0 ? prize.Exp : (int?)null,
            SkillPoints = prize.SkillPoints > 0 ? prize.SkillPoints : (int?)null,
            UsableSkillPoints = null,
            Currency = null,
            Abilities = null,
            DerivedAbilities = null,
            UnlockedSkills = null,
            Titles = null,
            FriendshipPoint = null,
            Items = items.Count == 0 ? null : items.ToArray(),
            RandomItems = null
        };
    }

    /// <summary>เควสที่ควรอยู่ในรายการตอนนี้ = เปิดแล้ว (รวมที่ทำเสร็จแล้วด้วย เพื่อให้กดรับรางวัลได้)</summary>
    private List<QuestData.Quest> VisibleQuests()
    {
        var list = new List<QuestData.Quest>();
        for (int i = 0; i < QuestData.All.Length; i++)
        {
            QuestData.Quest q = QuestData.All[i];
            if (IsQuestOpen(q) && !IsHidden(q))
            {
                list.Add(q);
            }
        }
        return list;
    }

    // ───────────────────────── handler ─────────────────────────

    private void HandleGetQuests(GetQuests msg, PacketHeader header)
    {
        List<QuestToDo> todos = new List<QuestToDo>();
        if (QuestsEnabled)
        {
            List<QuestData.Quest> open = VisibleQuests();
            for (int i = 0; i < open.Count; i++)
            {
                if (msg.Category == null || open[i].Category == msg.Category)
                {
                    todos.Add(MakeTodo(open[i]));
                }
            }
        }
        Send(new Quests
        {
            Category = msg.Category ?? QuestData.MainCategory,
            Todos = todos.Count == 0 ? null : todos.ToArray()
        }, header.Seq);
    }

    private void HandleGetQuestState(GetQuestState msg, PacketHeader header)
    {
        var todos = new List<QuestToDo>();
        if (QuestsEnabled && msg.QuestIds != null)
        {
            for (int i = 0; i < msg.QuestIds.Length; i++)
            {
                if (QuestData.TryGet(msg.QuestIds[i], out QuestData.Quest q) && IsQuestOpen(q))
                {
                    todos.Add(MakeTodo(q));
                }
            }
        }
        Send(new Quests
        {
            Category = QuestData.MainCategory,
            Todos = todos.Count == 0 ? null : todos.ToArray()
        }, header.Seq);
    }

    /// <summary>คะแนนสะสมของหมวดเควส — เราไม่มีระบบนี้ ตอบว่างไว้
    /// (client เปิดแท็บเควสแล้วส่ง GetQuestScoreInfos ทุกครั้ง — ถ้าไม่ตอบ แผงล่างจะหมุนโหลดค้างตลอด)</summary>
    private void HandleGetQuestScoreInfos(GetQuestScoreInfos msg, PacketHeader header)
    {
        Send(new QuestScoreInfos
        {
            Category = msg.Category ?? QuestData.MainCategory,
            CurQuestScore = 0,
            QuestScoreRewards = Array.Empty<QuestScoreReward>()
        }, header.Seq);
    }

    /// <summary>กดรับรางวัล — ต้องทำครบแล้วและยังไม่เคยรับ</summary>
    private void HandleRequestQuestReward(RequestQuestReward msg, PacketHeader header)
    {
        if (!QuestsEnabled)
        {
            Send(new Info { Text = "ระบบเควสยังไม่เปิดในรอบนี้" }, header.Seq);
            Send(default(Abort), header.Seq);
            return;
        }
        if (!QuestData.TryGet(msg.QuestId, out QuestData.Quest q))
        {
            Console.WriteLine("[quest] ปฏิเสธ {0}: ไม่มีเควส '{1}' ในตาราง", Name, msg.QuestId);
            Send(default(Abort), header.Seq);
            return;
        }
        if (IsHidden(q))
        {
            Console.WriteLine("[quest] ปฏิเสธ {0}: {1} เป็นเควสชุดตรวจที่ปิดอยู่", Name, q.Id);
            Send(default(Abort), header.Seq);
            return;
        }
        if (!_questDone.Contains(q.Id))
        {
            Console.WriteLine("[quest] ปฏิเสธ {0}: {1} ยังทำไม่ครบ ({2}/{3})", Name, q.Id, ProgressOf(q.Id), q.Count);
            Send(default(Abort), header.Seq);
            return;
        }
        if (_questRewarded.Contains(q.Id))
        {
            Console.WriteLine("[quest] ปฏิเสธ {0}: {1} รับรางวัลไปแล้ว", Name, q.Id);
            Send(default(Abort), header.Seq);
            return;
        }
        if (_grantingQuestReward)
        {
            // กันเรียกซ้อน: การให้ exp ทำให้ขึ้นเลเวล → เช็คเควสเลเวล → อาจวิ่งกลับมาที่นี่
            Console.WriteLine("[quest] ปฏิเสธ {0}: กำลังให้รางวัลอยู่ (กันเรียกซ้อน)", Name);
            Send(default(Abort), header.Seq);
            return;
        }

        // มาร์คก่อนให้ของ — ถ้าให้ของแล้วพังกลางทาง อย่างน้อยต้องไม่ให้ซ้ำได้อีก
        // (เสียรางวัลดีกว่าให้ซ้ำไม่จำกัด ซึ่งกลายเป็นช่องปั๊มของ)
        _questRewarded.Add(q.Id);
        MarkDirty();
        _grantingQuestReward = true;
        try
        {
            GrantQuestReward(q);
        }
        catch (Exception e)
        {
            Console.WriteLine("[quest] ⚠️ ให้รางวัล {0} ของ {1} ไม่สำเร็จ: {2}", q.Id, Name, e.Message);
            Send(new Info { Text = "รับรางวัลไม่สำเร็จ — แจ้งผู้ดูแลเซิร์ฟได้เลย" });
        }
        finally
        {
            _grantingQuestReward = false;
        }

        Send(new QuestRewardResults
        {
            Category = q.Category,
            QuestId = q.Id,
            Reward = MakeRewardInfo(q.Prize),
            QuestScoreInfos = default
        }, header.Seq);
        Send(default(OK), header.Seq);

        // เควสถัดไปในสายเพิ่งถูกปลดล็อกจากการ "ทำเสร็จ" ไม่ใช่ "รับรางวัล"
        // แต่ส่งรายการใหม่ตรงนี้ด้วยเพื่อให้หน้าต่างที่เปิดค้างอยู่เห็นสถานะล่าสุด
        SendQuestList();
    }

    private void GrantQuestReward(QuestData.Quest q)
    {
        if (q.Prize.Exp > 0)
        {
            GainExp(q.Prize.Exp, "เควส");
        }
        if (q.Prize.SkillPoints > 0)
        {
            _skillPoints += q.Prize.SkillPoints;
            SendSkills();
        }
        for (int i = 0; i < q.Prize.Items.Length; i++)
        {
            (string proto, int count) = q.Prize.Items[i];
            for (int n = 0; n < count; n++)
            {
                lock (_inventory)
                {
                    if (_inventory.Count >= PlayerInventoryMaxSize)
                    {
                        Send(new Info { Text = "กระเป๋าเต็ม — ของรางวัลบางส่วนตกหล่น" });
                        break;
                    }
                    _inventory.Add(MakeGatheredItem(new Generator
                    {
                        Id = proto,
                        Name = ItemNameData.NameOf(proto, proto),
                        Icon = ItemNameData.IconOf(proto, string.Empty)
                    }));
                }
            }
        }
        if (q.Prize.Items.Length > 0)
        {
            SendInventory();
        }
        Console.WriteLine("[quest] 🎁 {0} รับรางวัล {1} (exp {2} · แต้มสกิล {3} · ของ {4} ชนิด)",
            Name, q.Id, q.Prize.Exp, q.Prize.SkillPoints, q.Prize.Items.Length);
    }

    // ───────────────────────── ตัวนับ ─────────────────────────

    /// <summary>
    /// จุดที่เควส "ไปถึง" (Goal.Reach) — หาแบบอัตโนมัติจากแผนที่ ไม่มีตารางตายตัว
    /// Param = ชื่อจุด เช่น "north_beach"
    /// </summary>
    private WorldPosition ReachSpot(string name)
    {
        // หาดเหนือสุดของเกาะ: scan จากบนสุด (y=0) ไล่ลงมา หา tile ที่เป็นหาดแถว ๆ แนวจุดเกิด
        if (name == "north_beach")
        {
            TerrainStore t = _world.Terrain;
            int entryX = t.EntryPoint.x;
            int xMin = Math.Max(0, entryX - 40);
            int xMax = Math.Min(t.Width - 1, entryX + 40);
            for (int y = 0; y < t.Height; y++)
            {
                for (int x = xMin; x <= xMax; x++)
                {
                    Shared.Region.Biome b = t.BiomeAt(x, y);
                    if (b == Shared.Region.Biome.SandBeach || b == Shared.Region.Biome.PebbleBeach)
                    {
                        return new WorldPosition(x * 200f + 100f, y * 200f + 100f);
                    }
                }
            }
        }
        return _world.GetEntryPosition();
    }

    /// <summary>
    /// เช็คเควส "ไปถึงจุด" — เรียกทุกครั้งที่ผู้เล่นเดินสำเร็จ (RememberPosition)
    /// ถ้ายืนใกล้จุดเป้า (รัศมี 300 หน่วย ≈ 1.5 tile) ก็ถือว่าถึง
    /// </summary>
    private void CheckReachQuests()
    {
        if (!QuestsEnabled)
        {
            return;
        }
        for (int i = 0; i < QuestData.All.Length; i++)
        {
            QuestData.Quest q = QuestData.All[i];
            if (q.Kind != QuestData.Goal.Reach || _questDone.Contains(q.Id) || !IsQuestOpen(q) || IsHidden(q))
            {
                continue;
            }
            WorldPosition spot = ReachSpot(q.Param);
            float dx = CurrentPosition.x - spot.x;
            float dy = CurrentPosition.y - spot.y;
            if (dx * dx + dy * dy <= 300f * 300f)
            {
                QuestProgress(QuestData.Goal.Reach, q.Param);
            }
        }
    }

    /// <summary>
    /// เรียกจากจุดที่ผู้เล่น "ทำอะไรสำเร็จจริง ๆ" — เดียวกับที่เรียก GainExpFor* / GainProficiency
    /// (ห้ามเรียกตอนกดสั่ง ไม่งั้นยิง packet รัว ๆ ที่ถูกปฏิเสธก็เดินเควสได้)
    /// </summary>
    public void QuestProgress(QuestData.Goal kind, string param = null, int amount = 1)
    {
        if (!QuestsEnabled || amount <= 0)
        {
            return;
        }
        bool unlockedSomething = false;
        for (int i = 0; i < QuestData.All.Length; i++)
        {
            QuestData.Quest q = QuestData.All[i];
            if (q.Kind != kind || _questDone.Contains(q.Id) || !IsQuestOpen(q) || IsHidden(q))
            {
                continue;
            }
            // 🐛 เดิมเช็คแค่ "ถ้าเควสเจาะจง ต้องตรงกัน" ⇒ เควสแบบ**ไม่เจาะจง**ถูกนับสองรอบ
            //    เพราะผู้เรียกยิงสองครั้งเสมอ (ทั่วไป + เจาะจง) เช่น GainExpForBuild:
            //        QuestProgress(Build);  QuestProgress(Build, blueprintId);
            //    เควส "สร้าง 2 อย่าง" จึงจบตั้งแต่สร้างชิ้นแรก
            //
            //    ตอนนี้จับคู่ให้ตรงชนิด: เรียกแบบทั่วไป → เข้าเฉพาะเควสทั่วไป
            //                            เรียกแบบเจาะจง → เข้าเฉพาะเควสที่เจาะจงและตรงกัน
            bool callSpecific = !string.IsNullOrEmpty(param);
            bool questSpecific = !string.IsNullOrEmpty(q.Param);
            if (callSpecific != questSpecific)
            {
                continue;
            }
            if (questSpecific && !string.Equals(q.Param, param, StringComparison.Ordinal))
            {
                continue;
            }

            // ตัดที่เป้าหมาย — ไม่งั้นเลขในไฟล์เซฟโตไปเรื่อย ๆ ทั้งที่ไม่มีความหมาย
            int now = Math.Min(ProgressOf(q.Id) + amount, q.Count);
            _questProgress[q.Id] = now;
            MarkDirty();

            bool finished = now >= q.Count;
            if (finished)
            {
                _questDone.Add(q.Id);
                unlockedSomething = true;
                Console.WriteLine("[quest] ✅ {0} ทำเควส {1} สำเร็จ ({2}/{3})", Name, q.Id, Math.Min(now, q.Count), q.Count);
                Send(new Info { Text = $"[เควสสำเร็จ] {q.Thai}\nเปิดหน้าเควสเพื่อรับรางวัล" });
            }
            else
            {
                Console.WriteLine("[quest] {0}: {1} {2}/{3}", Name, q.Id, now, q.Count);
            }

            Send(new NotifyQuestProceed
            {
                QuestId = q.Id,
                Progress = Math.Min(now, q.Count),
                GoalCount = q.Count,
                Finished = finished
            });
            PluginManager.Instance?.FireEvent(finished ? "quest.completed" : "quest.progressed", this, false, true);
        }

        if (unlockedSomething)
        {
            AnnounceNewQuests();
        }
    }

    /// <summary>เควสที่เพิ่งเปิดจากการทำอันก่อนหน้าสำเร็จ — บอก client ให้เด้ง "เควสใหม่"</summary>
    private void AnnounceNewQuests()
    {
        var fresh = new List<QuestToDo>();
        var freshChecklist = new List<QuestToDo>();
        int checklistFresh = 0;
        for (int i = 0; i < QuestData.All.Length; i++)
        {
            QuestData.Quest q = QuestData.All[i];
            if (!IsQuestOpen(q) || IsHidden(q) || _questDone.Contains(q.Id))
            {
                continue;
            }
            if (!_questAnnounced.Add(q.Id))
            {
                continue;       // ประกาศไปแล้ว ไม่ประกาศซ้ำ
            }
            MarkDirty();
            if (QuestData.IsChecklist(q.Id))
            {
                freshChecklist.Add(MakeTodo(q));
                // ชุดตรวจเปิดพร้อมกันทีเดียว 12 ข้อ — ถ้าประกาศทีละอันจะท่วมกล่องข้อความ
                // สรุปเป็นบรรทัดเดียวข้างล่างแทน
                checklistFresh++;
                continue;
            }
            fresh.Add(MakeTodo(q));
            Send(new Info { Text = $"[เควสใหม่] {q.Thai}" });
        }
        if (checklistFresh > 0)
        {
            Send(new Info
            {
                Text = $"[เควสประจำวัน] เพิ่ม {checklistFresh} ข้อในหน้าเควส — พิมพ์ `cheat checklist` ดูรายการเต็ม"
            });
        }
        if (fresh.Count > 0)
        {
            Send(new QuestStarted { Category = QuestData.MainCategory, Quests = fresh.ToArray() });
        }
        if (freshChecklist.Count > 0)
        {
            Send(new QuestStarted { Category = QuestData.ChecklistCategory, Quests = freshChecklist.ToArray() });
        }
        SendQuestList();
    }

    /// <summary>
    /// `cheat checklist` — พิมพ์ "เควสประจำวัน" พร้อมสถานะแต่ละข้อ
    ///
    /// มีไว้เพราะหน้าต่างเควสในเกมโชว์ชื่อเป็น**ภาษาเกาหลี** (มาจากข้อมูลเกม) ซึ่งอ่านแล้วไม่รู้ว่าต้องทำอะไร
    /// อันนี้พิมพ์คำสั่งภาษาไทยของเราออกมาให้ครบ พร้อมตัวเลขความคืบหน้าที่ server นับจริง
    /// </summary>
    public string DescribeChecklist()
    {
        if (!ChecklistEnabled)
        {
            return "เควสประจำวันปิดอยู่ (Features.QuestChecklist = false)";
        }
        var sb = new System.Text.StringBuilder();
        int done = 0;
        for (int i = 0; i < QuestData.Checklist.Length; i++)
        {
            QuestData.Quest q = QuestData.Checklist[i];
            int now = Math.Min(ProgressOf(q.Id), q.Count);
            bool ok = _questDone.Contains(q.Id);
            if (ok)
            {
                done++;
            }
            sb.AppendFormat("{0} {1}/{2}  {3}\n", ok ? "[/]" : "[ ]", now, q.Count, q.Thai);
        }
        sb.AppendFormat("— ผ่านแล้ว {0}/{1} ข้อ", done, QuestData.Checklist.Length);
        return sb.ToString();
    }

    /// <summary>
    /// ส่งรายการเควส — **แยกเป็นคนละ packet ต่อหมวด**
    ///
    /// 🐛 เดิมยัดทุกเควสลงหมวดเดียว (sunset) ⇒ พอเพิ่มแท็บ "รายการตรวจเซิร์ฟ"
    ///    แท็บนั้นจะว่างเปล่า เพราะ client เก็บรายการแยกตามหมวดที่ packet บอกมา
    /// </summary>
    private void SendQuestList()
    {
        if (!QuestsEnabled)
        {
            return;
        }
        SendQuestListFor(QuestData.MainCategory);
        if (ChecklistEnabled)
        {
            SendQuestListFor(QuestData.ChecklistCategory);
        }
    }

    private void SendQuestListFor(string category)
    {
        List<QuestData.Quest> open = VisibleQuests();
        var todos = new List<QuestToDo>();
        for (int i = 0; i < open.Count; i++)
        {
            if (open[i].Category == category)
            {
                todos.Add(MakeTodo(open[i]));
            }
        }
        Send(new Quests
        {
            Category = category,
            Todos = todos.Count == 0 ? null : todos.ToArray()
        });
    }

    /// <summary>ตอนเข้าเกม — ส่งรายการเควสและเช็คเควสแบบ "ถึงเลเวล" ที่อาจครบไปแล้ว</summary>
    public void SendQuestsOnSpawn()
    {
        if (!QuestsEnabled)
        {
            return;
        }
        CheckLevelQuests();
        SendQuestList();
    }

    /// <summary>
    /// เควสประเภท "ถึงเลเวล N" ต่างจากอันอื่นตรงที่ **วัดจากค่าปัจจุบัน ไม่ใช่การนับสะสม**
    /// จึงต้องเช็คตอนขึ้นเลเวลและตอนเข้าเกม (เผื่อเลเวลถึงตั้งแต่ก่อนเปิดระบบเควส)
    /// </summary>
    public void CheckLevelQuests()
    {
        if (!QuestsEnabled)
        {
            return;
        }
        bool finishedAny = false;
        for (int i = 0; i < QuestData.All.Length; i++)
        {
            QuestData.Quest q = QuestData.All[i];
            if (q.Kind != QuestData.Goal.Level || _questDone.Contains(q.Id) || !IsQuestOpen(q))
            {
                continue;
            }
            _questProgress[q.Id] = Math.Min(Level, q.Count);
            if (Level >= q.Count)
            {
                _questDone.Add(q.Id);
                finishedAny = true;
                MarkDirty();
                Send(new Info { Text = $"[เควสสำเร็จ] {q.Thai}" });
                Send(new NotifyQuestProceed
                {
                    QuestId = q.Id,
                    Progress = q.Count,
                    GoalCount = q.Count,
                    Finished = true
                });
            }
        }
        if (finishedAny)
        {
            // 🐛 เดิมไม่ได้เรียก ⇒ เควสแบบ "ถึงเลเวล N" สำเร็จแล้ว **ขั้นถัดไปในสายไม่โผล่**
            //    จนกว่าจะมีเควสอื่นสำเร็จมาปลุกให้
            AnnounceNewQuests();
        }
    }

    // ───────────────────────── เซฟ/โหลด ─────────────────────────

    private void FillQuestSave(PlayerSave save)
    {
        save.QuestProgress = new Dictionary<string, int>(_questProgress);
        save.QuestDone = new List<string>(_questDone);
        save.QuestRewarded = new List<string>(_questRewarded);
        save.QuestAnnounced = new List<string>(_questAnnounced);
    }

    /// <summary>
    /// โหลดจากไฟล์เซฟ — **ทิ้ง id ที่ไม่มีในตารางแล้ว**
    ///
    /// สำคัญตอนแก้ตารางเควส: ถ้าลบ/เปลี่ยนชื่อเควส เซฟเก่าจะมี id ค้างอยู่ตลอดไป
    /// และถ้าวันหลังเอา id เดิมมาใช้กับเควสคนละอัน ผู้เล่นเก่าจะได้เควสนั้น "เสร็จแล้ว" ฟรี ๆ
    /// </summary>
    private void ApplyQuestSave(PlayerSave save)
    {
        _questProgress.Clear();
        _questDone.Clear();
        _questRewarded.Clear();
        _questAnnounced.Clear();
        int dropped = 0;

        if (save.QuestProgress != null)
        {
            foreach (KeyValuePair<string, int> pair in save.QuestProgress)
            {
                if (string.IsNullOrEmpty(pair.Key) || pair.Value <= 0)
                {
                    continue;
                }
                if (!QuestData.TryGet(pair.Key, out QuestData.Quest q))
                {
                    dropped++;
                    continue;
                }
                _questProgress[pair.Key] = Math.Min(pair.Value, q.Count);
            }
        }
        dropped += LoadQuestIds(save.QuestDone, _questDone);
        dropped += LoadQuestIds(save.QuestRewarded, _questRewarded);
        dropped += LoadQuestIds(save.QuestAnnounced, _questAnnounced);

        // เซฟเก่าจากตอนที่ยังไม่มี _questAnnounced — ถือว่าเควสที่เปิดอยู่แล้วเคยประกาศไปแล้ว
        // ไม่งั้นพอ login ครั้งแรกหลังอัปเดตจะเด้ง "เควสใหม่" ของทุกอันที่ค้างอยู่พร้อมกัน
        if (save.QuestAnnounced == null || save.QuestAnnounced.Count == 0)
        {
            for (int i = 0; i < QuestData.All.Length; i++)
            {
                if (IsQuestOpen(QuestData.All[i]))
                {
                    _questAnnounced.Add(QuestData.All[i].Id);
                }
            }
        }

        if (dropped > 0)
        {
            Console.WriteLine("[quest] {0}: ทิ้ง id ที่ไม่มีในตารางแล้ว {1} รายการจากไฟล์เซฟ", Name, dropped);
            MarkDirty();
        }
    }

    /// <summary>ยัด id จากเซฟลงเซ็ต ข้ามอันที่ไม่มีในตารางแล้ว — คืนจำนวนที่ทิ้ง</summary>
    private static int LoadQuestIds(List<string> from, HashSet<string> into)
    {
        int dropped = 0;
        if (from == null)
        {
            return 0;
        }
        for (int i = 0; i < from.Count; i++)
        {
            string id = from[i];
            if (string.IsNullOrEmpty(id))
            {
                continue;
            }
            if (QuestData.ById.ContainsKey(id))
            {
                into.Add(id);
            }
            else
            {
                dropped++;
            }
        }
        return dropped;
    }

    /// <summary>จำนวนเควสที่ทำครบแล้วแต่ยังไม่ได้กดรับรางวัล — client เอาไปขึ้นตัวเลขแดงบนไอคอน</summary>
    public int CountUnclaimedQuests(string category = null)
    {
        if (!QuestsEnabled)
        {
            return 0;
        }
        int n = 0;
        foreach (string id in _questDone)
        {
            if (_questRewarded.Contains(id))
            {
                continue;
            }
            if (category != null)
            {
                // นับเฉพาะหมวดที่ถาม — เลขนี้ไปขึ้นเป็นจุดแดงบนแท็บ ถ้านับรวมทุกหมวดจะแดงผิดแท็บ
                if (!QuestData.TryGet(id, out QuestData.Quest q) || q.Category != category)
                {
                    continue;
                }
            }
            n++;
        }
        return n;
    }

    /// <summary>
    /// ตัวช่วยเทสเท่านั้น (`cheat questskip`) — ทำทุกขั้นให้เสร็จ **ยกเว้นขั้นสุดท้าย**
    /// เพื่อกระโดดไปเทสปลายสาย (ต่อแพ) โดยไม่ต้องไล่ทำครบทุกขั้นจริง ๆ
    /// ไม่ให้รางวัล — แค่ปลดล็อกสายให้เดินต่อได้
    /// </summary>
    public string SkipQuestsForTest()
    {
        if (!QuestsEnabled)
        {
            return "ระบบเควสปิดอยู่";
        }
        // 🐛 รอบแรกวนทั้ง QuestData.All — พอเพิ่ม "รายการตรวจเซิร์ฟ" เข้ามาต่อท้าย
        //    ตัวสุดท้ายของ All กลายเป็นข้อในชุดตรวจ ⇒ เควสต่อแพถูกมาร์คว่าเสร็จไปด้วย
        //    แก้เป็นวนเฉพาะ Story แล้วเว้นตัวสุดท้าย
        //
        // 🐛 รอบสอง (อันนี้): พอต่อเนื้อเรื่องจริงของเกมต่อจากแพ สาย Story ก็ยาวขึ้น
        //    "ตัวสุดท้ายของ Story" เลยไม่ใช่เควสต่อแพอีกต่อไป ⇒ **แพโดนมาร์คว่าเสร็จอีกครั้ง**
        //    ผลคือผู้เล่นที่ใช้คำสั่งนี้ต่อแพจริงแล้วไม่มีข้อความฉลองขึ้น (จังหวะสำคัญของสายสอนเล่น)
        //
        // ⇒ ผูกกับ "เควสต่อแพ" ตรง ๆ แทนการนับตำแหน่ง: หยุดก่อนถึงมันเสมอ
        //    สายจะยาวขึ้นอีกกี่ขั้นก็ไม่กระทบ
        int stopAt = QuestData.Story.Length - 1;
        for (int i = 0; i < QuestData.Story.Length; i++)
        {
            if (QuestData.Story[i].Id == QuestData.RaftQuestId)
            {
                stopAt = i;
                break;
            }
        }
        int n = 0;
        for (int i = 0; i < stopAt; i++)
        {
            QuestData.Quest q = QuestData.Story[i];
            if (_questDone.Add(q.Id))
            {
                _questProgress[q.Id] = q.Count;
                n++;
            }
        }
        MarkDirty();
        SendQuestList();
        return $"ข้ามเควสไป {n} ขั้น — เหลือขั้นต่อไป: {QuestData.Story[stopAt].Thai}";
    }

    /// <summary>สรุปสถานะเควสไว้ตอบคำสั่ง `cheat quests`</summary>
    public string DescribeQuests()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append(Name).Append(" — เควสสายหลัก:");
        for (int i = 0; i < QuestData.All.Length; i++)
        {
            QuestData.Quest q = QuestData.All[i];
            string state = _questRewarded.Contains(q.Id) ? "รับรางวัลแล้ว"
                : _questDone.Contains(q.Id) ? "**รอรับรางวัล**"
                : !IsQuestOpen(q) ? "ยังไม่เปิด"
                : $"{Math.Min(ProgressOf(q.Id), q.Count)}/{q.Count}";
            sb.Append("\n  ").Append(i + 1).Append(". ").Append(q.Thai).Append("  [").Append(state).Append(']');
        }
        return sb.ToString();
    }
}
