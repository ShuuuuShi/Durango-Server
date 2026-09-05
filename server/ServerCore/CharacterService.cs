using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Durango.Offline;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Messages;

namespace DurangoServer.Core;

/// <summary>
/// Character/account boundary used by the title screen and by the game session flow.
/// It intentionally lives in the server process for now; Gateway only owns routing.
/// </summary>
public sealed class CharacterService
{
    private const int PlayerSlotCount = 3;
    private const int MaxPlayerSlotCount = 7;
    private readonly GameServer _gameServer;

    public CharacterService(GameServer gameServer)
    {
        _gameServer = gameServer;
    }

    /// <summary>
    /// [4 ก.ย. 2026] หน้าเลือกตัวละคร — ถ้าเปิด DurangoID จะตอบจาก "ไอดีที่ผูก IP นี้ไว้" แทนการเดาจาก IP ล้วน
    ///
    /// เกมมือถือส่ง account_id (NPSN) มาเป็นค่าว่างเสมอ (Platform_Android ไม่ override) ⇒ ระบบเดิมเหลือแค่ IP
    /// ซึ่งเน็ตมือถือสลับ IP ทีตัวละครหายที · ไอดีที่ผูกไว้จึงเป็นตัวตนที่แน่นอนกว่า
    /// หลายไอดีผูก IP เดียวกันได้ (บ้านเดียวกัน) — คืนตัวละครของทุกไอดี แล้วให้ผู้เล่นเลือกเองในเกม
    /// </summary>
    public WebServer.Response ListAccounts(HttpListenerRequest request, string ownerKey)
    {
        string remoteIp = request?.RemoteEndPoint?.Address?.ToString() ?? "?";
        List<AccountStore.Account> accounts = ResolveAccounts(remoteIp, ownerKey);
        JArray players = new JArray();
        foreach (AccountStore.Account account in accounts)
        {
            if (players.Count >= PlayerSlotCount)
            {
                break;
            }
            PlayerSave save = SaveStore.Load<PlayerSave>(SaveStore.PlayerPath(account.EntityId));
            if (save == null)
            {
                continue;
            }
            players.Add(new JObject
            {
                ["player_entity_id"] = account.EntityId,
                ["player_name"] = string.IsNullOrEmpty(save.Name) ? account.Name : save.Name,
                ["player_level"] = save.Level,
                ["disconnected_at"] = account.LastSeenAt,
                ["deletes_at"] = save.DeletesAt
            });
        }

        return new WebServer.JsonResponse(new JObject
        {
            ["players"] = players,
            ["max_player_slot_count"] = MaxPlayerSlotCount,
            ["player_slot_count"] = PlayerSlotCount
        }.ToString());
    }

    /// <summary>หาตัวละครที่ควรโชว์ให้เครื่องนี้ — ผ่าน DurangoID ก่อน แล้วค่อยตกไปที่ระบบ IP เดิม</summary>
    private static List<AccountStore.Account> ResolveAccounts(string remoteIp, string ownerKey)
    {
        PlayerIdConfig ids = ServerConfig.Current.PlayerIds;
        if (ids == null || !ids.Enabled)
        {
            return AccountStore.FindByOwner(remoteIp, ownerKey);
        }

        List<string> entityIds = PlayerIdStore.EntitiesForIp(remoteIp);
        if (entityIds.Count > 0)
        {
            var result = new List<AccountStore.Account>(entityIds.Count);
            foreach (string entityId in entityIds)
            {
                AccountStore.Account acc = AccountStore.FindByEntityId(entityId)
                    ?? new AccountStore.Account { EntityId = entityId };
                result.Add(acc);
            }
            result.Sort((a, b) => b.LastSeenAt.CompareTo(a.LastSeenAt));
            return result;
        }

        // ยังไม่ผูกไอดีกับ IP นี้ — บังคับสมัคร = ไม่โชว์อะไรเลย (ผู้เล่นต้องไปกดผูกที่หน้า /id ก่อน)
        return ids.Required ? new List<AccountStore.Account>() : AccountStore.FindByOwner(remoteIp, ownerKey);
    }

    public WebServer.Response GetInfo(string entityId)
    {
        PlayerSave save;
        try
        {
            save = SaveStore.Load<PlayerSave>(SaveStore.PlayerPath(entityId));
        }
        catch (Exception e)
        {
            Console.WriteLine("[character] player info load failed: " + e.Message);
            return new WebServer.JsonResponse("{}");
        }
        if (save == null)
        {
            return new WebServer.JsonResponse("{}");
        }

        JToken display = BuildCurrentDisplay(save, entityId);

        JObject result = new JObject
        {
            ["entity_id"] = save.EntityId ?? entityId,
            ["freq"] = 0,
            ["name"] = save.Name ?? string.Empty,
            ["level"] = save.Level,
            ["clan"] = new JObject
            {
                ["clan_id"] = save.ClanId ?? string.Empty,
                ["clan_name"] = save.ClanName ?? string.Empty
            },
            ["region"] = null,
            ["returning_region"] = null,
            ["display"] = display,
            ["personal_region_id"] = save.LastIsland ?? string.Empty,
            ["pioneer_grade"] = 0,
            ["deletes_at"] = save.DeletesAt
        };
        return new WebServer.JsonResponse(result.ToString(Formatting.None));
    }

    public WebServer.Response Delete(string entityId, HttpListenerRequest request)
    {
        string remoteIp = request?.RemoteEndPoint?.Address?.ToString() ?? "?";
        string ownerKey = request?.QueryString?["account_id"];
        List<AccountStore.Account> requesterAccounts = string.IsNullOrEmpty(ownerKey)
            ? AccountStore.FindByIp(remoteIp)
            : AccountStore.FindByOwner(remoteIp, ownerKey);
        bool ownedByRequester = requesterAccounts.Exists(account => account.EntityId == entityId);
        if (!ownedByRequester)
        {
            return new WebServer.JsonResponse("{}", System.Net.HttpStatusCode.Forbidden);
        }

        string playerPath = SaveStore.PlayerPath(entityId);
        PlayerSave save = SaveStore.Load<PlayerSave>(playerPath);
        if (save == null)
        {
            return new WebServer.JsonResponse("{}", System.Net.HttpStatusCode.NotFound);
        }

        try
        {
            File.Delete(playerPath);
            AccountStore.Release(entityId);
            Console.WriteLine($"[character] deleted immediately {entityId}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"[character] delete failed {entityId}: {e.Message}");
            return new WebServer.JsonResponse("{}", System.Net.HttpStatusCode.InternalServerError);
        }

        return new WebServer.JsonResponse(new JObject
        {
            ["deleted"] = true
        }.ToString(Formatting.None));
    }

    public WebServer.Response CancelDeletion(string entityId)
    {
        PlayerSave save = SaveStore.Load<PlayerSave>(SaveStore.PlayerPath(entityId));
        if (save == null)
        {
            return new WebServer.JsonResponse("{}", System.Net.HttpStatusCode.NotFound);
        }

        save.DeletesAt = null;
        SaveStore.Save(SaveStore.PlayerPath(entityId), save);
        Console.WriteLine($"[character] deletion cancelled {entityId}");
        return new WebServer.JsonResponse("{}");
    }

    public WebServer.Response Create(Dictionary<string, string> postData, HttpListenerRequest request = null)
    {
        // [4 ก.ย. 2026] บังคับสมัครไอดีก่อนเล่น (PlayerIds.Required) — ไม่มีไอดีที่ผูก IP นี้ = สร้างไม่ได้
        // ตัวเกมโชว์ได้แค่ "สร้างตัวละครไม่สำเร็จ" เพราะยังไม่เข้าโลก (popup ต้องใช้ช่องแชท System)
        // จึงต้องบอกทางไปสมัครผ่านช่องทางอื่น (หน้าดาวน์โหลด/ประกาศ) ดู docs/server/PlayerIds.md
        string createIpEarly = AccountStore.NormalizeIp(request?.RemoteEndPoint?.Address?.ToString() ?? "?");
        PlayerIdConfig idCfg = ServerConfig.Current.PlayerIds;
        string boundId = (idCfg != null && idCfg.Enabled) ? PlayerIdStore.LatestBoundId(createIpEarly) : null;
        if (idCfg != null && idCfg.Enabled && idCfg.Required && string.IsNullOrEmpty(boundId))
        {
            Console.WriteLine($"[id] ปฏิเสธการสร้างตัวละครจาก {createIpEarly} — ยังไม่ได้ผูกไอดี");
            return new WebServer.JsonResponse(
                new JObject { ["error"] = "ต้องสมัครและผูกไอดีที่หน้า /id ก่อนสร้างตัวละคร" }.ToString(),
                HttpStatusCode.Forbidden);
        }

        string entityId = Guid.NewGuid().ToString();
        string name = (postData.Get("name") ?? string.Empty).Trim();
        bool isMale = !string.Equals(postData.Get("gender"), "female", StringComparison.OrdinalIgnoreCase);
        ushort entityType = (ushort)(isMale ? 1000 : 1001);
        string displayJson = BuildCreatedDisplayJson(entityId, isMale, postData.Get("model_info"));

        GameServer.PlayerData data = new GameServer.PlayerData
        {
            EntityId = entityId,
            Name = name,
            Level = 1,
            EntityType = entityType,
            DisplayJson = displayJson
        };
        if (!string.IsNullOrEmpty(name))
        {
            _gameServer.RegisterName(entityId, name);
        }
        _gameServer.RegisterPlayerData(data);

        PlayerSave save = new PlayerSave
        {
            EntityId = entityId,
            Name = name,
            Level = 1,
            EntityType = entityType,
            DisplayJson = displayJson
        };

        int job = ParseStarterJob(postData.Get("job"));
        save.Job = job;
        ItemSave starterOutfit = BuildStarterOutfit(job, displayJson);
        if (starterOutfit != null)
        {
            save.Inventory.Add(starterOutfit);
            save.InventoryOrder.Add(starterOutfit.Id);
            save.EquippedItems["body"] = starterOutfit.Id;
            save.EquipmentPresets["1"] = new Dictionary<string, string>
            {
                ["body"] = starterOutfit.Id
            };
        }
        if (ServerConfig.Current.Features.Jobs)
        {
            ApplyJobSkillBoost(save, job);
        }
        SaveStore.Save(SaveStore.PlayerPath(entityId), save);

        // 🐛 [แก้เอง 30 ส.ค. 2026] ตัวละครที่เพิ่งสร้าง **ไม่เคยถูกผูกบัญชีเลย**
        //
        // `AccountStore.TryClaim` ถูกเรียกที่เดียวคือ `/sessions` และเรียกเฉพาะตอน `hasSelectedCharacter`
        // = true (id ที่ "มีเซฟอยู่แล้ว") — แต่ตอนสร้างตัวละครใหม่ id ยังไม่มีเซฟ ⇒ ข้ามไปเสมอ
        // ⇒ ไม่มีไฟล์ใน saves/accounts/ ⇒ `/accounts` (ListAccounts → FindByOwner/FindByIp) คืนลิสต์ว่าง
        // ⇒ client เห็นว่า "ไม่มีตัวละคร" แล้วบังคับสร้างใหม่ทุกครั้งที่เข้าเกม **วนไม่รู้จบ**
        // (เจอจริงบน VPS 30 ส.ค.: ADMIN → แอดมิน → อีกตัว สร้างใหม่ 3 รอบติด ทั้งที่เซฟเก่ายังอยู่ครบ)
        //
        // แก้ให้จองบัญชีตั้งแต่ตอนสร้าง — จุดที่รู้แน่นอนว่าใครเป็นเจ้าของ id นี้
        string createIp = request?.RemoteEndPoint?.Address?.ToString() ?? "?";
        if (!AccountStore.TryClaim(entityId, name, createIp, postData.Get("account_id"), out string claimDenied))
        {
            Console.WriteLine($"[account] ผูกบัญชีให้ตัวละครใหม่ {entityId} ไม่สำเร็จ: {claimDenied}");
        }

        // ตัวละครใหม่เป็นของไอดีที่ผูก IP นี้ไว้ล่าสุด (คนที่เพิ่งกด "ผูกเครื่องนี้" ที่หน้า /id)
        if (!string.IsNullOrEmpty(boundId))
        {
            PlayerIdStore.AttachEntity(boundId, entityId);
        }

        Console.WriteLine($"[character] created {entityId} name={(string.IsNullOrEmpty(name) ? "(empty)" : name)} " +
            $"gender={(isMale ? "male" : "female")} display={(string.IsNullOrEmpty(displayJson) ? "no" : "yes")} " +
            $"job={postData.Get("job")} region={postData.Get("region")}");
        return new WebServer.JsonResponse(new JObject { ["entity_id"] = entityId }.ToString());
    }

    private static JToken BuildCurrentDisplay(PlayerSave save, string entityId)
    {
        if (string.IsNullOrWhiteSpace(save.DisplayJson))
        {
            return null;
        }
        try
        {
            PlayerDisplay display = JsonConvert.DeserializeObject<PlayerDisplay>(save.DisplayJson);
            display.EntityId = save.EntityId ?? entityId;

            Dictionary<string, string> equipped = save.EquippedItems;
            string presetKey = save.CurrentEquipSlotType.ToString();
            if (save.EquipmentPresets != null
                && save.EquipmentPresets.TryGetValue(presetKey, out Dictionary<string, string> preset))
            {
                equipped = preset;
            }

            if (equipped != null && save.Inventory != null)
            {
                foreach (KeyValuePair<string, string> pair in equipped)
                {
                    ItemSave item = save.Inventory.Find(candidate => candidate != null && candidate.Id == pair.Value);
                    if (item == null)
                    {
                        continue;
                    }
                    // [4 ก.ย. 2026] เดิม fallback ขาวตรง ๆ ⇒ ชุดตัวละครขาว · แปลงชื่อ palette/ตัด '#' ก่อน
                    var pc = GameData.ItemColorOrWhite(item.Prototype);
                    string[] colors =
                    {
                        GameData.ResolveColor(item.ColorR, item.Prototype) ?? pc.R,
                        GameData.ResolveColor(item.ColorG, item.Prototype) ?? pc.G,
                        GameData.ResolveColor(item.ColorB, item.Prototype) ?? pc.B
                    };
                    if (EquipData.TryGetArmor(item.Prototype, out EquipData.ArmorInfo armor))
                    {
                        bool isMale = save.EntityType != 1001;
                        string model = isMale ? armor.MaleModel : armor.FemaleModel;
                        if (armor.Slot == "body" && !string.IsNullOrEmpty(model))
                        {
                            display.Body = model;
                            display.BodyColor = colors;
                        }
                        else if (armor.Slot == "head" && !string.IsNullOrEmpty(model))
                        {
                            display.Head = model;
                            display.HeadColor = colors;
                        }
                    }
                }
            }
            return JToken.FromObject(display);
        }
        catch (Exception e)
        {
            Console.WriteLine("[character] player display parse failed: " + e.Message);
            return null;
        }
    }

    private static int ParseStarterJob(string rawJob)
    {
        if (!int.TryParse(rawJob, out int job))
        {
            job = 0;
        }
        return Math.Clamp(job, 0, 7);
    }

    /// <summary>
    /// ตอนสร้างตัว (เมื่อ Features.Jobs) — ดันความชำนาญหมวดอาชีพเป็น 20
    /// และปลดโหนดสกิลตาม jobs.json ไม่ backfill ตัวเก่า
    /// </summary>
    private static void ApplyJobSkillBoost(PlayerSave save, int job)
    {
        if (JobCatalog.TryGet(job, out JobCatalog.Definition def))
        {
            foreach (KeyValuePair<int, int> pair in def.CategoryLevels)
            {
                ApplyCategoryBoost(save, (Shared.Skill.Category)pair.Key, pair.Value);
            }
            foreach (JobCatalog.Grant grant in def.GivenSkills)
            {
                GrantJobSkill(save, grant);
            }
            Console.WriteLine($"[character] job skill boost job={job} จาก jobs.json หมวด {def.CategoryLevels.Count} โหนด {def.GivenSkills.Count}");
            return;
        }

        if (!TryJobBoostCategory((Shared.Player.Job)job, out Shared.Skill.Category category))
        {
            return;
        }
        ApplyCategoryBoost(save, category, 20);
        Console.WriteLine($"[character] job skill boost job={job} category={category} (ตารางสำรอง)");
    }

    private static void ApplyCategoryBoost(PlayerSave save, Shared.Skill.Category category, int boostLevel)
    {
        if (category == Shared.Skill.Category.Invalid || boostLevel <= 1)
        {
            return;
        }
        int exp = SkillCategoryData.TotalExpToReach(category, boostLevel);
        if (exp <= 0)
        {
            return;
        }
        save.CategoryExp[((int)category).ToString()] = exp;
        if (category != Shared.Skill.Category.Survival)
        {
            string key = $"{(int)category}:{boostLevel}";
            if (!save.CompletedCategoryResearch.Contains(key))
            {
                save.CompletedCategoryResearch.Add(key);
            }
        }
    }

    private static void GrantJobSkill(PlayerSave save, JobCatalog.Grant grant)
    {
        if (string.IsNullOrEmpty(grant.SkillId))
        {
            return;
        }
        string sub = string.IsNullOrEmpty(grant.SubId) ? "__base__" : grant.SubId;
        int cat = SkillData.SkillCategory.TryGetValue(grant.SkillId, out int found) ? found : 0;
        SkillBundleSave existing = save.KnownSkills.Find(s => s != null && s.SkillId == grant.SkillId);
        if (existing == null)
        {
            existing = new SkillBundleSave
            {
                SkillId = grant.SkillId,
                Category = cat,
                Levels = new Dictionary<string, int>()
            };
            save.KnownSkills.Add(existing);
        }
        existing.Levels ??= new Dictionary<string, int>();
        existing.Levels[sub] = Math.Max(1, grant.Level);
    }

    private static bool TryJobBoostCategory(Shared.Player.Job job, out Shared.Skill.Category category)
    {
        switch (job)
        {
            case Shared.Player.Job.Engineer: category = Shared.Skill.Category.Weaponcrafting; return true;
            case Shared.Player.Job.Office: category = Shared.Skill.Category.Constructing; return true;
            case Shared.Player.Job.Student: category = Shared.Skill.Category.Gathering; return true;
            case Shared.Player.Job.Farmer: category = Shared.Skill.Category.Farming; return true;
            case Shared.Player.Job.Waiter: category = Shared.Skill.Category.Armorcrafting; return true;
            case Shared.Player.Job.Soldier: category = Shared.Skill.Category.MeleeCombat; return true;
            case Shared.Player.Job.Homemaker: category = Shared.Skill.Category.Cooking; return true;
            case Shared.Player.Job.Jobless: category = Shared.Skill.Category.Defense; return true;
            default: category = Shared.Skill.Category.Invalid; return false;
        }
    }

    private static ItemSave BuildStarterOutfit(int job, string displayJson)
    {
        string[] outfits =
        {
            "clothes_engineer", "clothes_officeworker", "clothes_student", "clothes_farmer",
            "clothes_waiter", "clothes_soldier", "clothes_homeworker", "clothes_jobless"
        };
        job = Math.Clamp(job, 0, outfits.Length - 1);
        string prototype = outfits[job];
        if (!EquipData.TryGetArmor(prototype, out _))
        {
            return null;
        }

        string name = prototype;
        string icon = null;
        if (ItemNameData.Map.TryGetValue(prototype, out (string Name, string Icon) metadata))
        {
            name = metadata.Name;
            icon = metadata.Icon;
        }

        string[] colors = { "FFFFFF", "FFFFFF", "FFFFFF" };
        try
        {
            JObject display = JObject.Parse(displayJson ?? "{}");
            string[] selected = (display["BodyColor"] ?? display["body_color"])?.ToObject<string[]>();
            if (selected != null && selected.Length >= 3)
            {
                colors = new[] { selected[0], selected[1], selected[2] };
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("[character] starter outfit color parse failed: " + e.Message);
        }

        return new ItemSave
        {
            Id = Guid.NewGuid().ToString(),
            Prototype = prototype,
            Name = name,
            Description = name,
            Icon = icon,
            Level = 1,
            Size = 1,
            ColorR = colors[0],
            ColorG = colors[1],
            ColorB = colors[2]
        };
    }

    private static string BuildCreatedDisplayJson(string entityId, bool isMale, string modelInfoJson)
    {
        PlayerDisplay display = default;
        display.EntityId = entityId;
        display.DefaultBody = isMale
            ? "Models/PC/Male/Body/m_body_nothing.FBX"
            : "Models/PC/Female/Body/f_body_nothing.FBX";
        display.DefaultInner = isMale
            ? "Models/PC/Male/Inner/m_inner_basic.FBX"
            : "Models/PC/Female/Inner/f_inner_basic.FBX";
        display.Body = display.DefaultBody;
        display.BodySize = 1f;

        if (!string.IsNullOrWhiteSpace(modelInfoJson))
        {
            try
            {
                JObject model = JObject.Parse(modelInfoJson);
                display.Hair = model.Value<string>("hair");
                display.Beard = model.Value<string>("beard");
                display.SkinColor = model.Value<string>("skin_color");
                display.HairColor = model.Value<string>("hair_color");
                display.EyeColor = model.Value<string>("eye_color");
                display.LipColor = model.Value<string>("lip_color");
                display.PortraitBgColor = model.Value<string>("portrait_bg_color");
                display.BodyColor = model["body_color"]?.ToObject<string[]>();
                display.HeadColor = model["head_color"]?.ToObject<string[]>();
                display.Portrait = model.Value<int?>("portrait") ?? 0;
                display.PortraitBg = model.Value<int?>("portrait_bg") ?? 0;
                display.VoiceType = model.Value<int?>("voice_type") ?? 0;
                float size = model.Value<float?>("body_size") ?? 0f;
                display.BodySize = size > 0f ? size : 1f;
            }
            catch (Exception e)
            {
                Console.WriteLine("[character] model_info parse failed: " + e.Message);
            }
        }
        return JsonConvert.SerializeObject(display);
    }
}
