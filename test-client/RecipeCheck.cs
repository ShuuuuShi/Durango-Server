using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Durango.Network;
using Durango.Offline;
using Durango.Utils;
using DurangoServer.Core;
using Messages;

namespace DurangoTestClient;

/// <summary>
/// เทสเฉพาะกิจ: สร้างตัวละครใหม่จริง ๆ แล้วขอ GetRecipes ตรง ๆ ดูว่าเซิร์ฟส่ง Ids กลับมากี่อัน
/// (ไล่ปัญหา "แทบคราฟต์ว่างเปล่า" ที่รายงานจากเซิร์ฟบ้านผ่าน Tailscale)
/// </summary>
public static class RecipeCheck
{
    private static bool IsFoodRecipe(string recipeId)
    {
        return RecipeMeta.TryGet(recipeId, out RecipeMeta.Info info)
            && (info.Category == "cook" || info.Category == "cook_season2");
    }

    private static void Pump(Connection connection, int milliseconds)
    {
        for (int i = 0; i < milliseconds / 10; i++) { connection.Process(); Thread.Sleep(10); }
    }

    public static int Run(string host, int gamePort, int gatewayPort)
    {
        string entityId = "recipe-check-" + DateTime.UtcNow.Ticks;
        string modelInfo = "{\"hair\":\"Models/PC/Female/Hair/f_hair_long\",\"body_color\":[\"484E36\",\"F0D9B7\",\"29130D\"],\"head_color\":[\"FF0000\",\"FFFFFF\",\"0000FF\"],\"skin_color\":\"C8A07A\",\"hair_color\":\"471513\",\"lip_color\":\"E88295\",\"eye_color\":\"52353F\",\"portrait\":3,\"portrait_bg\":2,\"portrait_bg_color\":\"C5A293\",\"beard\":null,\"voice_type\":4,\"body_size\":1.2}";
        string createdId = CreateCharacterCheck.CreatePlayer(host, gatewayPort, "เทสสูตร", false, modelInfo);
        if (string.IsNullOrEmpty(createdId))
        {
            Console.WriteLine("[recipe-check] สร้างตัวละครไม่สำเร็จเลย — เช็ค gateway ก่อน");
            return 1;
        }
        entityId = createdId;
        string token = SessionClient.FetchRaw(host, gatewayPort, "{\"appear_player\":{\"entity_id\":\"" + entityId + "\"}}");
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("[recipe-check] ขอ session token ไม่สำเร็จ");
            return 1;
        }

        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(host, gamePort);
        var connection = new Connection(socket);
        string[] recipeIds = null;
        int aborts = 0;
        bool sawSkills = false;
        int skillCount = 0;
        SkillBundle[] knownSkills = Array.Empty<SkillBundle>();
        connection.Recv<Abort>((m, h) => aborts++);
        connection.Recv<Recipes>((m, h) => { recipeIds = m.Ids; });
        connection.Recv<Skills>((m, h) =>
        {
            sawSkills = true;
            knownSkills = m.SkillList ?? Array.Empty<SkillBundle>();
            skillCount = knownSkills.Length;
        });
        connection.Recv<Welcome>((m, h) => { });
        connection.Recv<AppearPlayer>((m, h) => { });
        connection.Recv<Clock>((m, h) => { }); connection.Recv<OK>((m, h) => { });
        connection.Recv<Inventory>((m, h) => { });
        connection.Recv<Statistics>((m, h) => { }); connection.Recv<Equipments>((m, h) => { });
        connection.Recv<Survival>((m, h) => { }); connection.Recv<Points>((m, h) => { });
        connection.Recv<AppearAnimal>((m, h) => { }); connection.Recv<AppearArtifact>((m, h) => { });
        connection.Recv<Move>((m, h) => { }); connection.Recv<DefoggedChunks>((m, h) => { });
        connection.Recv<QuestCategories>((m, h) => { }); connection.Recv<WalletUpdated>((m, h) => { });
        connection.Recv<ArtifactBlueprints>((m, h) => { });
        connection.Recv<Chunk>((m, h) => { });
        connection.StartReceive();

        connection.Send(new GetClock { Time = Times.UnixTimeNow() }); Pump(connection, 250);
        connection.Send(new Auth { EntityId = entityId, SessionToken = token, ClientVersion = "5.2.1", DeviceModel = "recipe-check" });
        Pump(connection, 500);
        connection.Send(default(Ready)); Pump(connection, 2000);
        Console.WriteLine("== ส่ง GetRecipes ==");
        connection.Send(default(GetRecipes));
        Pump(connection, 1500);
        connection.Close();

        Console.WriteLine($"aborts = {aborts}");
        Console.WriteLine($"Skills packet ได้ไหม = {sawSkills} (จำนวนสกิลที่รู้ = {skillCount})");
        if (recipeIds == null)
        {
            Console.WriteLine("[recipe-check] ไม่ได้รับ Recipes packet กลับมาเลย!");
            return 1;
        }
        Console.WriteLine($"Recipes.Ids.Length = {recipeIds.Length}");
        int show = Math.Min(recipeIds.Length, 20);
        for (int i = 0; i < show; i++) Console.WriteLine("  - " + recipeIds[i]);
        if (recipeIds.Length > show) Console.WriteLine($"  ... อีก {recipeIds.Length - show} รายการ");

        var skillRecipes = new HashSet<string>();
        var skillBlueprints = new HashSet<string>();
        foreach (SkillBundle skill in knownSkills)
        {
            if (skill.Levels == null) continue;
            foreach (KeyValuePair<string, int> level in skill.Levels)
            {
                RecipeUnlockData.Collect(skill.SkillId, level.Key, level.Value, skillRecipes, skillBlueprints);
            }
        }

        string[] foodRecipes = recipeIds.Where(IsFoodRecipe).ToArray();
        string[] foodWithoutSkill = foodRecipes.Where(id => !skillRecipes.Contains(id)).ToArray();
        Console.WriteLine($"สูตรอาหารที่ส่งมา = {foodRecipes.Length}");
        foreach (string id in foodRecipes) Console.WriteLine("  [อาหาร] " + id);
        Console.WriteLine($"สูตรอาหารที่ไม่มาจากสกิล = {foodWithoutSkill.Length}");
        foreach (string id in foodWithoutSkill.Take(20)) Console.WriteLine("  [ผิด] " + id);

        bool passed = recipeIds.Length > 0 && sawSkills && foodRecipes.Length > 0 && foodWithoutSkill.Length == 0;
        Console.WriteLine(passed
            ? "[ผ่าน] รายการอาหารมีเฉพาะสูตรที่สกิลของตัวละครปลดแล้ว"
            : "[ตก] พบสูตรอาหารที่ยังไม่ได้ปลดด้วยสกิล");
        return passed ? 0 : 1;
    }
}
