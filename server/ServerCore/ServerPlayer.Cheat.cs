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

// ServerPlayer.Cheat — ดูรายละเอียดที่ docs/server/ServerPlayer.Cheat.md

public partial class ServerPlayer
{

    /// <summary>คนสั่งเป็น admin ไหม (H-2) — คำสั่งที่ยุ่งกับผู้เล่นคนอื่นต้องผ่านด่านนี้</summary>
    private bool IsAdmin => GameServer.IsAdmin(EntityId, Name);

    private void HandleCheat(Cheat msg, PacketHeader header)
    {
        string raw = (msg._Cheat ?? "").Trim();
        string cmd = raw.ToLower();

        // H-2: ปิดคำสั่งทดสอบเป็นค่าเริ่มต้น — เดิมใครก็เสกของ/ฟื้นเลือด/เรียกสัตว์/ลากตัวคนอื่นได้
        if (!GameServer.CheatsEnabled)
        {
            Console.WriteLine($"[cheat] ปฏิเสธ {Name} ({EntityId}): '{cmd}' — คำสั่งทดสอบถูกปิดอยู่");
            Send(new Info { Text = "คำสั่งทดสอบถูกปิดอยู่ (เปิดเซิร์ฟด้วย --enable-cheat ถึงจะใช้ได้)" }, header.Seq);
            return;
        }
        Console.WriteLine($"[cheat] {EntityId}: {cmd}");

        // รีโมทคุมตัวละครคนอื่น: control <ชื่อ|entityId> <คำสั่ง> [args]
        // (ไม่เข้า switch เพราะต้องเก็บตัวพิมพ์ใหญ่-เล็กของชื่อไว้)
        if (cmd.StartsWith("control ", StringComparison.Ordinal))
        {
            HandleControl(raw, header);
            return;
        }

        // spawn [ชนิด]                     — เกิดตรงที่ยืนอยู่ (ชนิด 2000-2999, ไม่ใส่ = สุ่ม)
        // spawn <tileX> <tileY> [ความสูง]  — เกิดที่พิกัดที่ระบุ
        if (cmd.StartsWith("spawn ", StringComparison.Ordinal))
        {
            string[] sp = cmd.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (sp.Length == 2 && ushort.TryParse(sp[1], out ushort wantType) && wantType >= 2000)
            {
                ServerAnimal one = _world.Animals.SpawnAt(CurrentPosition, wantType, CurrentHeight);
                string known = AnimalData.TryGet(wantType, out AnimalData.AnimalInfo ai) ? ai.ModelPath : "(ไม่รู้จักชนิดนี้)";
                Send(new Info { Text = $"เกิดสัตว์ type {one.EntityType} lv{one.Level} ข้างตัว — โมเดล {known}" }, header.Seq);
                return;
            }
            if (sp.Length >= 3 && int.TryParse(sp[1], out int sx) && int.TryParse(sp[2], out int sy))
            {
                float height = CurrentHeight;
                if (sp.Length >= 4 && float.TryParse(sp[3], out float h))
                {
                    height = h;
                }
                ServerAnimal at = _world.Animals.SpawnAt(new WorldPosition(sx * 200f + 100f, sy * 200f + 100f), 0, height);
                Send(new Info { Text = $"เกิดสัตว์ type {at.EntityType} lv{at.Level} ที่ tile {sx},{sy} ความสูง {height:F0}" }, header.Seq);
                return;
            }
        }

        // give <prototype> [จำนวน] — เสกไอเทมอะไรก็ได้ที่มีอยู่ในเกม (ชื่อ/ไอคอน/tag มาจากข้อมูลจริง)
        // มีไว้เทสสูตรคราฟต์/ทำอาหาร: `give meat 3` แล้วเอาไปย่างได้เลย ไม่ต้องออกไปล่าจริง
        if (cmd.StartsWith("give ", StringComparison.Ordinal))
        {
            string[] g = raw.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (g.Length >= 2)
            {
                string proto = g[1];
                int count = 1;
                if (g.Length >= 3 && int.TryParse(g[2], out int n))
                {
                    count = Math.Clamp(n, 1, 20);
                }
                if (!ItemNameData.Map.ContainsKey(proto))
                {
                    // ไม่ใช่ชื่อ prototype — ลองเป็นชุดของสำเร็จรูปแทน (axe · bonfire · cook ฯลฯ)
                    // จะได้ใช้คำสั่งเดียวกันทั้งจากคอนโซลและจากกล่องเครื่องมือ (control <ชื่อ> give ...)
                    Send(new Info { Text = ControlGive(proto) }, header.Seq);
                    return;
                }
                for (int i = 0; i < count; i++)
                {
                    Item made = MakeGatheredItem(new Generator
                    {
                        Id = proto,
                        Name = ItemNameData.NameOf(proto, proto),
                        Icon = ItemNameData.IconOf(proto, string.Empty)
                    });
                    lock (_inventory)
                    {
                        _inventory.Add(made);
                    }
                }
                MarkDirty();
                SendInventory();
                Send(new Info { Text = $"ได้ {ItemNameData.NameOf(proto, proto)} x{count} (prototype={proto})" }, header.Seq);
                return;
            }
        }

        // exp <จำนวน> — ยัด exp ให้เลย ไว้เทสว่าขึ้นเลเวลแล้วค่าสถานะ/หลอดโตจริงไหม
        if (cmd.StartsWith("exp ", StringComparison.Ordinal))
        {
            if (int.TryParse(cmd.Substring(4).Trim(), out int amount) && amount > 0)
            {
                GainExp(Math.Clamp(amount, 1, 1000000), "cheat");
                Send(new Info { Text = $"ได้ exp {amount} — ตอนนี้เลเวล {Level} (exp รวม {TotalExp})" }, header.Seq);
            }
            else
            {
                Send(new Info { Text = "ใช้: cheat exp <จำนวน>" }, header.Seq);
            }
            return;
        }

        // travel <รหัสเกาะ> — เดินทางข้ามเกาะ (Beta 1.1)
        if (cmd.StartsWith("travel ", StringComparison.Ordinal))
        {
            string want = cmd.Substring("travel ".Length).Trim();
            Send(new Info { Text = TravelTo(want) }, header.Seq);
            return;
        }

        switch (cmd)
        {
            case "islands":
                Send(new Info { Text = DescribeIslands() }, header.Seq);
                break;
            case "tp spawn":
                SendTeleport(_world.GetEntryPosition());
                break;
            case "info":
                Send(new Info { Text = "DurangoServer v0.1 - players: " + _world.Count }, header.Seq);
                break;
            case "who":
            {
                // ใครออนไลน์อยู่บ้าง — เครื่องมือข้างนอกใช้หาชื่อตัวละครไปสั่ง control ต่อ
                ServerPlayer[] online = _world.SnapshotPlayers();
                if (online.Length == 0)
                {
                    Send(new Info { Text = "ไม่มีใครออนไลน์" }, header.Seq);
                    break;
                }
                var sb = new StringBuilder();
                sb.Append("ออนไลน์ ").Append(online.Length).Append(" คน:");
                for (int i = 0; i < online.Length; i++)
                {
                    WorldPosition p = online[i].CurrentPosition;
                    sb.Append("\n  ").Append(online[i].Name)
                      .Append(" | ").Append(online[i].EntityId)
                      .Append(" | tile ").Append((int)(p.x / 200f)).Append(',').Append((int)(p.y / 200f))
                      .Append(" | lv").Append(online[i].Level)
                      .Append(online[i].Dead ? " ☠" : "");
                }
                Send(new Info { Text = sb.ToString() }, header.Seq);
                break;
            }
            case "stats":
                SendStatistics();
                break;
            case "abilities":
                // ดูค่าสถานะ 8 ตัว + เลือด/สตามินาสูงสุด + พลังอาวุธที่ถืออยู่ (ไว้เทียบก่อน/หลังใส่ของ)
                Send(new Info { Text = DescribeAbilities() }, header.Seq);
                break;
            case "skills":
                // ดูว่าสกิลที่เรียนไปมีผลเท่าไรแล้ว (ไว้เทียบก่อน/หลังเรียน)
                Send(new Info
                {
                    Text = $"เลเวล {Level} · exp {TotalExp} (อีก {LevelData.ToNextLevel(TotalExp)} ขึ้นเลเวล) · แต้มสกิล {_skillPoints}\n"
                           + DescribeSkillBonuses()
                }, header.Seq);
                break;
            case "add bonfire":
            case "add_bonfire":
                lock (_inventory)
                {
                    _inventory.Add(MakeCapsuleItem("capsulated_bonfire", "กองไฟ", "furniture_workbench_bonfire"));
                }
                SendInventory();
                Send(new Info { Text = "ได้รับกองไฟ x1", }, header.Seq);
                break;
            // เฟส C — ของสำหรับทดสอบระบบสวมใส่
            case "add axe":
            case "add_axe":
                GiveEquipTestItem("axe_onehand_stone_01", "ขวานหิน", "weapon_axe_onehand_stone", header.Seq);
                break;
            case "add stone":
            case "add_stone":
                // หิน 1 ก้อน — วัตถุดิบของสูตร blade_stone (มีด) ไว้เทสสายเครื่องมือ
                GiveEquipTestItem("stone", "หิน", "icon_nat_stone", header.Seq);
                break;
            case "add knife":
            case "add_knife":
                // มีดหิน — ของจริงคราฟต์เองได้จากหิน (สูตร blade_stone) นี่เป็นทางลัดตอนเทส
                GiveEquipTestItem("blade_stone", "ใบมีดหิน", "icon_nat_blade_stone", header.Seq);
                break;
            case "add pickaxe":
            case "add_pickaxe":
                GiveEquipTestItem("pickaxe_wooden_01", "อีเต้อไม้", "weapon_pickaxe_wooden", header.Seq);
                break;
            case "add clothes":
            case "add_clothes":
                GiveEquipTestItem("clothes_builder_01", "ชุดช่าง", "clothes_builder_01", header.Seq);
                break;
            // เฟส C — กล่องเก็บของ
            case "add box":
            case "add_box":
                lock (_inventory)
                {
                    _inventory.Add(MakeCapsuleItem("capsulated_fur_box_03_leaf", "กล่องใบไม้", "furniture_box"));
                }
                MarkDirty();
                SendInventory();
                Send(new Info { Text = "ได้กล่องใบไม้ x1 — วางลงพื้นแล้วเปิดใส่ของได้" }, header.Seq);
                break;

            // เฟส C — ทดสอบค่าสถานะ
            case "survival":
                Send(new Info
                {
                    Text = $"เลือด {CurrentLife:F0}/{LifeMax:F0} · สตามินา {_stamina.ValueAt(Times.UnixTimeNow()):F0}/{StaminaMax:F0} · ความล้า {_fatigue.ValueAt(Times.UnixTimeNow()):F0}/{FatigueMax:F0}"
                }, header.Seq);
                break;
            case "rest":
                RestoreSurvival(clearFatigue: true);
                Send(new Info { Text = "พักผ่อนแล้ว — เลือด/สตามินาเต็ม ความล้าเป็น 0" }, header.Seq);
                break;
            case "tired":
                SetGaugeValue("stamina", 0f);
                Send(new Info { Text = "ตั้งสตามินาเป็น 0 (ลองเก็บของทันทีดูว่าโดนปฏิเสธไหม — ฟื้น 4/วิ)" }, header.Seq);
                break;
            case "hurt":
                bool dead = ApplyDamage(30f);
                if (dead)
                {
                    Die();          // เฟส C รอบ 2: บอกทุกคนว่าล้มแล้ว
                }
                Send(new Info { Text = $"โดน 30 ดาเมจ เหลือเลือด {CurrentLife:F0}{(dead ? " — ตายแล้ว" : "")}" }, header.Seq);
                break;
            case "spawn":
            case "spawn animal":
            {
                // เรียกสัตว์มาเกิดตรงที่ยืนอยู่ — สัตว์ปกติกระจายในรัศมี 30 tile ซึ่งมักอยู่นอกจอ
                ServerAnimal born = _world.Animals.SpawnAt(CurrentPosition);
                Send(new Info { Text = $"เรียกสัตว์ type {born.EntityType} lv{born.Level} มาเกิดข้างตัวแล้ว" }, header.Seq);
                break;
            }
            case "die":
                SetGaugeValue("life", 0f);
                Die();
                Send(new Info { Text = "ตายแล้ว — ส่ง Revive เพื่อฟื้น" }, header.Seq);
                break;
            case "kill animal":
            case "kill_animal":
            {
                // ฆ่าสัตว์ตัวที่ใกล้ที่สุดทันที — ไว้เทสการแล่เนื้อโดยไม่ต้องยืนตีเป็นนาที
                ServerAnimal[] all = _world.Animals.Snapshot();
                ServerAnimal nearest = null;
                float best = float.MaxValue;
                WorldPosition me = CurrentPosition;
                for (int i = 0; i < all.Length; i++)
                {
                    if (!all[i].IsAlive)
                    {
                        continue;
                    }
                    float ddx = all[i].Position.x - me.x;
                    float ddy = all[i].Position.y - me.y;
                    float d2 = ddx * ddx + ddy * ddy;
                    if (d2 < best)
                    {
                        best = d2;
                        nearest = all[i];
                    }
                }
                if (nearest == null)
                {
                    Send(new Info { Text = "ไม่มีสัตว์เป็น ๆ ในโลกเลย" }, header.Seq);
                    break;
                }
                _world.Animals.Damage(nearest.EntityId, nearest.LifeMax * 2f, EntityId);
                Send(new Info
                {
                    Text = $"ฆ่า {nearest.EntityId} (type {nearest.EntityType} lv{nearest.Level}) ห่าง {MathF.Sqrt(best) / 200f:F1} tile — แตะซากเพื่อแล่ได้เลย"
                }, header.Seq);
                break;
            }
            // ดูความทนทานของเครื่องมือที่ถืออยู่ — ไว้เทสว่าหลอดลดจริงไหมโดยไม่ต้องเปิด UI
            case "tools":
            {
                var lines = new List<string>();
                lock (_inventory)
                {
                    for (int i = 0; i < _inventory.Count; i++)
                    {
                        Item it = _inventory[i];
                        if (!ToolDurability.HasDurability(it))
                        {
                            continue;
                        }
                        float max = ToolDurability.MaxOf(it);
                        lines.Add($"{it.Name} ({it.Prototype}) วัสดุระดับ {ToolDurability.TierOf(it.Prototype)} — {ToolDurability.RemainingOf(it):F0}/{max:F0}");
                    }
                }
                ToolConfig tc = ServerConfig.Current.Tools;
                string head = tc.Enabled
                    ? $"ระบบความทนทาน: เปิด (ฐาน {tc.DurabilityBase:F0} + {tc.DurabilityPerTier:F0}/ระดับ · ใช้ครั้งละ {tc.WearPerUse:F0})"
                    : "ระบบความทนทาน: ปิดอยู่ (Tools.Enabled = false)";
                Send(new Info
                {
                    Text = lines.Count == 0
                        ? head + "\nไม่มีเครื่องมือในกระเป๋า"
                        : head + "\n" + string.Join("\n", lines)
                }, header.Seq);
                break;
            }

            // เททิ้งทั้งกระเป๋า — มีไว้ให้ชุดทดสอบเรียกตอนเริ่ม
            // ไม่งั้นบอทชื่อเดิม (เช่น gp-check-1) สะสมของทุกรอบจนกระเป๋าเต็ม
            // แล้วข้อที่ต้อง "เก็บของได้จริง" จะตกทั้งที่โค้ดถูก (เคยหลงแก้ผิดจุดมาแล้ว)
            case "clearbag":
            case "clear bag":
            {
                int before;
                lock (_inventory)
                {
                    before = _inventory.Count;
                    _inventory.Clear();
                }
                MarkDirty();
                SendInventory();
                Send(new Info { Text = $"เททิ้งของในกระเป๋า {before} ชิ้น" }, header.Seq);
                break;
            }
            // ล้าเต็มหลอด — ใช้เทสว่าเลือดไหลลงจนตายจริงไหม
            case "burnout":
                SetGaugeValue("fatigue", ServerConfig.Current.Survival.FatigueMax);
                Send(new Info { Text = $"ตั้งความล้าเป็น {ServerConfig.Current.Survival.FatigueMax:F0} (เต็มหลอด) — เลือดจะเริ่มไหลลง" }, header.Seq);
                break;
            case "exhaust":
                SetGaugeValue("fatigue", 90f);
                Send(new Info { Text = "ตั้งความล้า 90 (เกิน danger 85 → ค่าใช้จ่ายสตามินา x2)" }, header.Seq);
                break;
            default:
                Send(new Info { Text = "unknown cheat: " + cmd }, header.Seq);
                break;
        }
    }

    /// <summary>
    /// รีโมทคุมตัวละครของผู้เล่นอีกคน — <c>control &lt;ชื่อ|entityId&gt; &lt;คำสั่ง&gt; [args]</c>
    /// ใช้ขับตัวละครที่ล็อกอินอยู่ในตัวเกมจริงด้วย packet ล้วน (ดู ServerPlayer.RemoteControl.cs)
    /// </summary>
    private void HandleControl(string raw, PacketHeader header)
    {
        // H-2: control ยุ่งกับตัวละครของคนอื่นได้ (ลากไปไหนก็ได้ · พูดแทน · บังคับตีสัตว์)
        // จึงต้องเป็น admin เท่านั้น — ไม่ได้ตั้ง --admin ไว้ = ใช้ไม่ได้เลย
        if (!IsAdmin)
        {
            Console.WriteLine($"[control] ปฏิเสธ {Name} ({EntityId}): ไม่ใช่ admin");
            Send(new Info { Text = "คำสั่ง control ใช้ได้เฉพาะ admin (ตั้งด้วย --admin <ชื่อ|entityId> ตอนเปิดเซิร์ฟ)" }, header.Seq);
            return;
        }
        string[] a = raw.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (a.Length < 3)
        {
            Send(new Info { Text = "ใช้: control <ชื่อ|id> <tp|walk|stop|gather|attack|craft|eat|place|bag|prof|spawn|kill|heal|give|travel|say|status> [args]" }, header.Seq);
            return;
        }
        ServerPlayer target = _world.FindPlayerByNameOrId(a[1]);
        if (target == null)
        {
            Send(new Info { Text = $"ไม่เจอผู้เล่น '{a[1]}' ที่ออนไลน์อยู่" }, header.Seq);
            return;
        }
        string verb = a[2].ToLower();
        string reply;
        switch (verb)
        {
            case "tp":
            case "walk":
            {
                if (a.Length < 5 || !int.TryParse(a[3], out int tx) || !int.TryParse(a[4], out int ty))
                {
                    reply = $"ใช้: control {a[1]} {verb} <tileX> <tileY>";
                    break;
                }
                if (verb == "tp")
                {
                    target.ControlTeleport(tx, ty);
                    reply = $"ย้าย {target.Name} ไป tile {tx},{ty}";
                }
                else
                {
                    target.ControlWalk(tx, ty);
                    reply = $"สั่ง {target.Name} เดินไป tile {tx},{ty}";
                }
                break;
            }
            case "stop":
                target.ControlStop();
                reply = $"{target.Name} หยุดเดิน";
                break;
            case "gather":
                reply = target.ControlGather();
                break;
            case "attack":
                reply = target.ControlAttackNearest();
                break;
            // ── เครื่องมือตอนเล่นเอง: สั่งจากข้างนอกให้เกิดอะไรขึ้นตรงหน้าตัวละครในเกม ──
            case "spawn":
            {
                ushort want = 0;
                if (a.Length >= 4)
                {
                    ushort.TryParse(a[3], out want);
                }
                reply = target.ControlSpawn(want);
                break;
            }
            case "kill":
                reply = target.ControlKillNearest();
                break;
            case "heal":
                reply = target.ControlHeal();
                break;
            case "abilities":
            case "stats":
                reply = target.DescribeAbilities();
                break;
            case "travel":
                reply = a.Length >= 4 ? target.TravelTo(a[3]) : "ใช้: control <ชื่อ> travel <รหัสเกาะ>";
                break;
            case "give":
                reply = target.ControlGive(a.Length >= 4 ? a[3].ToLower() : "");
                break;
            case "say":
            {
                string text = raw.Substring(raw.IndexOf(" say ", StringComparison.OrdinalIgnoreCase) + 5);
                target.ControlSay(text);
                reply = $"{target.Name} พูดว่า: {text}";
                break;
            }
            case "status":
                reply = target.ControlStatus();
                break;
            // ── สั่งให้ตัวละคร "เล่นเกม" จากข้างนอก (ServerPlayer.RemoteDrive) ──
            case "go":
            {
                // เดินแบบนับจากที่ยืนอยู่ — สคริปต์เทสใช้อันนี้ ไม่ใช่ walk ที่เป็นพิกัดตายตัว
                if (a.Length < 5 || !int.TryParse(a[3], out int gx) || !int.TryParse(a[4], out int gy))
                {
                    reply = $"ใช้: control {a[1]} go <dx> <dy>";
                    break;
                }
                reply = target.ControlGoRelative(gx, gy);
                break;
            }
            case "craft":
                reply = target.ControlCraft(a.Length >= 4 ? a[3] : null);
                break;
            case "eat":
                reply = target.ControlEat(a.Length >= 4 ? a[3] : null);
                break;
            case "place":
                reply = target.ControlPlace(a.Length >= 4 ? a[3] : null);
                break;
            case "bag":
                reply = target.ControlBag();
                break;
            case "prof":
            case "proficiency":
                reply = target.ControlProficiency();
                break;
            default:
                reply = $"ไม่รู้จักคำสั่ง '{verb}' (tp/walk/stop/gather/attack/craft/eat/place/bag/prof/give/heal/kill/spawn/say/status)";
                break;
        }
        Console.WriteLine("[control] {0} สั่ง {1}: {2}", Name, target.Name, reply);
        Send(new Info { Text = reply }, header.Seq);
    }

    /// <summary>เฟส C — สร้างไอเทมที่ใส่ได้จริงสำหรับทดสอบ (prototype ต้องมีใน EquipData)</summary>
    private void GiveEquipTestItem(string prototype, string name, string icon, uint replyOf)
    {
        Item item = MakeGatheredItem(new Generator
        {
            Id = prototype,
            Name = name,
            Icon = icon
        });
        lock (_inventory)
        {
            _inventory.Add(item);
        }
        MarkDirty();
        SendInventory();
        bool known = EquipData.Weapons.ContainsKey(prototype) || EquipData.Armors.ContainsKey(prototype);
        Send(new Info { Text = $"ได้ {name} x1 (prototype={prototype}, รู้จักโมเดล: {(known ? "ใช่" : "ไม่")})" }, replyOf);
    }
}
