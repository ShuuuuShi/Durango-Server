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

    public WebServer.Response ListAccounts(HttpListenerRequest request, string ownerKey)
    {
        string remoteIp = request?.RemoteEndPoint?.Address?.ToString() ?? "?";
        List<AccountStore.Account> accounts = AccountStore.FindByOwner(remoteIp, ownerKey);
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

    public WebServer.Response Create(Dictionary<string, string> postData)
    {
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

        ItemSave starterOutfit = BuildStarterOutfit(postData.Get("job"), displayJson);
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
        SaveStore.Save(SaveStore.PlayerPath(entityId), save);

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
                    string[] colors =
                    {
                        string.IsNullOrEmpty(item.ColorR) ? "FFFFFF" : item.ColorR,
                        string.IsNullOrEmpty(item.ColorG) ? "FFFFFF" : item.ColorG,
                        string.IsNullOrEmpty(item.ColorB) ? "FFFFFF" : item.ColorB
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

    private static ItemSave BuildStarterOutfit(string rawJob, string displayJson)
    {
        string[] outfits =
        {
            "clothes_engineer", "clothes_officeworker", "clothes_student", "clothes_farmer",
            "clothes_waiter", "clothes_soldier", "clothes_homeworker", "clothes_jobless"
        };
        if (!int.TryParse(rawJob, out int job))
        {
            job = 0;
        }
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
