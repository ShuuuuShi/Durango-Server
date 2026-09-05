using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Durango.Network;
using Durango.Offline;
using Durango.Utils;
using Messages;
using Shared.Etc;

namespace DurangoTestClient;

/// <summary>
/// ตรวจบั๊ก "สร้างเสร็จแล้วยังขึ้นรูปโครงไม้" — สิ่งปลูกสร้างที่เสร็จแล้วต้องมีโมเดล (Display.Parts)
/// ถ้า Parts ว่าง client จะเรนเดอร์แค่นั่งร้าน ทั้งที่ state = Completed
/// blueprint แบบ "ช่องวัสดุ" (ไม่มี default_look) คือกลุ่มที่พัง — โมเดลต้องมาจาก
/// blueprints.json slots[].looks[default_look_tag].model_key
/// </summary>
public static class BuiltPartsCheck
{
    private static int _passed, _failed;
    private static readonly List<string> _infos = new();

    private static void Pump(Connection c, int ms)
    {
        for (int i = 0; i < ms / 10; i++) { c.Process(); Thread.Sleep(10); }
    }

    private static void Check(string name, bool ok, string detail = null)
    {
        if (ok) { _passed++; Console.WriteLine($"  [PASS] {name}{(detail == null ? "" : " — " + detail)}"); }
        else { _failed++; Console.WriteLine($"  [FAIL] {name}{(detail == null ? "" : " — " + detail)}"); }
    }

    public static int Run(string host, int gamePort, int gatewayPort)
    {
        _passed = _failed = 0;
        string modelInfo =
            "{\"hair\":\"hair_f_01\",\"body_color\":[\"484E36\",\"F0D9B7\",\"29130D\"]," +
            "\"head_color\":[\"FF0000\",\"FFFFFF\",\"0000FF\"],\"skin_color\":\"F0D9B7\"," +
            "\"hair_color\":\"471513\",\"lip_color\":\"E88295\",\"eye_color\":\"52353F\"," +
            "\"portrait\":3,\"portrait_bg\":2,\"portrait_bg_color\":\"C5A293\",\"beard\":null," +
            "\"voice_type\":1,\"body_size\":1.0}";
        string id = CreateCharacterCheck.CreatePlayer(host, gatewayPort,
            "parts-" + Guid.NewGuid().ToString("N")[..6], isMale: false, modelInfo);
        if (string.IsNullOrEmpty(id)) { Console.WriteLine("สร้างตัวละครไม่ได้"); return 2; }

        string token = SessionClient.FetchRaw(host, gatewayPort,
            "{\"appear_player\":{\"entity_id\":\"" + id + "\"}}");
        if (string.IsNullOrEmpty(token)) { Console.WriteLine("ขอ token ไม่ได้"); return 2; }

        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(host, gamePort);
        var c = new Connection(socket);
        var parts = new Dictionary<string, Dictionary<string, string>>();
        string lastArtifact = null;
        c.Recv<Welcome>((m, h) => { }); c.Recv<Clock>((m, h) => { }); c.Recv<OK>((m, h) => { });
        c.Recv<Abort>((m, h) => { }); c.Recv<Info>((m, h) => _infos.Add(m.Text));
        c.Recv<Inventory>((m, h) => { }); c.Recv<Skills>((m, h) => { });
        c.Recv<Statistics>((m, h) => { }); c.Recv<Survival>((m, h) => { });
        c.Recv<AppearPlayer>((m, h) => { }); c.Recv<AppearAnimal>((m, h) => { });
        c.Recv<AppearArtifact>((m, h) =>
        {
            lastArtifact = m.EntityId;
            parts[m.EntityId] = m.Display.Parts ?? new Dictionary<string, string>();
        });
        c.Recv<Move>((m, h) => { }); c.Recv<Teleported>((m, h) => { });
        c.Recv<DefoggedChunks>((m, h) => { }); c.Recv<Chunk>((m, h) => { });
        c.Recv<QuestCategories>((m, h) => { }); c.Recv<WalletUpdated>((m, h) => { });
        c.Recv<Recipes>((m, h) => { }); c.Recv<ArtifactBlueprints>((m, h) => { });
        c.Recv<Messages.Timer>((m, h) => { }); c.Recv<ArtifactMaterials>((m, h) => { });
        c.Recv<ArtifactBuilt>((m, h) => { }); c.Recv<ArtifactCompleted>((m, h) => { });
        c.StartReceive();
        c.Send(new GetClock { Time = Times.UnixTimeNow() }); Pump(c, 250);
        c.Send(new Auth { EntityId = id, SessionToken = token, ClientVersion = "5.2.1", DeviceModel = "built-parts-check" });
        Pump(c, 450);
        c.Send(default(Ready)); Pump(c, 1500);

        Console.WriteLine($"=== built parts check: {host}:{gamePort} ===");

        // ตัวแทนของ blueprint แบบ "ช่องวัสดุ" ที่เคยได้ Parts ว่าง (โครงไม้ค้าง)
        string[] cases =
        {
            "tent", "bed_01", "gate_small", "basket", "fence2",
            "fur_box_01", "closet_table_01", "weapon_table_01", "raft",
        };
        foreach (string bp in cases)
        {
            lastArtifact = null;
            _infos.Clear();
            c.Send(new Cheat { _Cheat = "place real " + bp });
            Pump(c, 900);
            string info = _infos.Count > 0 ? _infos[^1] : "(ไม่มีข้อความตอบ)";
            if (lastArtifact == null || !parts.TryGetValue(lastArtifact, out var p))
            {
                Check($"{bp}: วางแล้วได้ AppearArtifact", false, info);
                continue;
            }
            Check($"{bp}: สร้างเสร็จแล้วมีโมเดล (Parts ไม่ว่าง)", p.Count > 0,
                p.Count == 0 ? "Parts ว่าง ⇒ client โชว์นั่งร้าน · " + info
                             : string.Join(", ", p));
        }

        Console.WriteLine($"\nสรุป: ผ่าน {_passed} · ไม่ผ่าน {_failed}");
        c.Close();
        return _failed == 0 ? 0 : 1;
    }
}
