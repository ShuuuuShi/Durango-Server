using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Durango.Network;
using Durango.Offline;
using Durango.Utils;
using Messages;

namespace DurangoTestClient;

/// <summary>
/// Regression: ปิด PartyAndClan แล้วทุก entry point ของปาร์ตี้ต้องถูกปฏิเสธอย่างสุภาพ
///
/// - GetParty (query) → ตอบ Party ว่าง (Id=null, Info=null) เพื่อให้ client เคลียร์ HUD
///   ทันที ไม่ค้างรอ — client เรียก GetParty ทุกครั้งที่ OnReady (PartySystem.OnReady)
/// - ทุก mutation (MakeParty/Invite/Join/Reject/Leave/Kick/Elect) → Info+Abort
///   และห้ามส่ง Party ที่มีข้อมูลจริงกลับมา (แปลว่าเกิด state ทั้งที่ feature ปิด)
///
/// รัน: dotnet run -- --party-gate-check [host] [port เกม] [port gateway]
/// </summary>
public static class PartyGateCheck
{
    private static int _passed;
    private static int _failed;
    private static readonly List<string> Infos = new List<string>();
    private static int _aborts;
    private static int _oks;
    private static int _partyReplies;        // Party ที่ตอบกลับมาแบบมีข้อมูลจริง (Info.HasValue)
    private static int _emptyPartyReplies;   // Party ว่าง (client เคลียร์ state ได้)
    private static Messages.Party? _lastParty;

    private static void Pump(Connection connection, int milliseconds)
    {
        for (int i = 0; i < milliseconds / 10; i++) { connection.Process(); Thread.Sleep(10); }
    }

    private static void Check(string name, bool ok, string detail = null)
    {
        if (ok) { _passed++; Console.WriteLine($"  [PASS] {name}{(detail == null ? "" : " — " + detail)}"); }
        else { _failed++; Console.WriteLine($"  [FAIL] {name}{(detail == null ? "" : " — " + detail)}"); }
    }

    private static void Reset()
    {
        lock (Infos) { Infos.Clear(); }
        _aborts = _oks = _partyReplies = _emptyPartyReplies = 0;
        _lastParty = null;
    }

    private static string InfoText()
    {
        lock (Infos) { return string.Join("\n", Infos); }
    }

    private static void AssertRejected(string action)
    {
        string all = InfoText();
        Check(action + " replies with disabled message and Abort",
            _aborts >= 1 && all.Contains("ปาร์ตี้ยังไม่เปิด"),
            $"info={all.Replace('\n', '|')} abort={_aborts}");
        Check(action + " does not create or change party state",
            _oks == 0 && _partyReplies == 0,
            $"ok={_oks} partyWithData={_partyReplies}");
    }

    public static int Run(string host, int gamePort, int gatewayPort)
    {
        _passed = _failed = 0;
        string id = "party-gate-" + Guid.NewGuid().ToString("N")[..8];
        string token = SessionClient.Fetch(host, gatewayPort, id, id);
        if (string.IsNullOrEmpty(token)) { Console.WriteLine("[FAIL] ขอ session ไม่ได้"); return 2; }

        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(host, gamePort);
        var connection = new Connection(socket);
        connection.Recv<Welcome>((m, h) => { }); connection.Recv<Clock>((m, h) => { });
        connection.Recv<Info>((m, h) => { lock (Infos) { Infos.Add(m.Text ?? ""); } });
        connection.Recv<Abort>((m, h) => _aborts++); connection.Recv<OK>((m, h) => _oks++);
        connection.Recv<Messages.Party>((m, h) =>
        {
            _lastParty = m;
            if (m.Info.HasValue && !string.IsNullOrEmpty(m.Id)) _partyReplies++;
            else _emptyPartyReplies++;
        });
        connection.Recv<Inventory>((m, h) => { }); connection.Recv<Skills>((m, h) => { });
        connection.Recv<Statistics>((m, h) => { }); connection.Recv<Equipments>((m, h) => { });
        connection.Recv<Survival>((m, h) => { }); connection.Recv<Points>((m, h) => { });
        connection.Recv<AppearPlayer>((m, h) => { }); connection.Recv<AppearAnimal>((m, h) => { });
        connection.Recv<AppearArtifact>((m, h) => { }); connection.Recv<DefoggedChunks>((m, h) => { });
        connection.Recv<QuestCategories>((m, h) => { }); connection.Recv<WalletUpdated>((m, h) => { });
        connection.Recv<Recipes>((m, h) => { }); connection.Recv<ArtifactBlueprints>((m, h) => { });
        connection.Recv<Chunk>((m, h) => { }); connection.Recv<Move>((m, h) => { });
        connection.StartReceive();
        connection.Send(new GetClock { Time = Times.UnixTimeNow() }); Pump(connection, 250);
        connection.Send(new Auth { EntityId = id, SessionToken = token, ClientVersion = "5.2.1", DeviceModel = "party-gate-check" }); Pump(connection, 500);
        connection.Send(default(Ready)); Pump(connection, 1600);

        Console.WriteLine("=== party gate check: feature ปิด (PartyAndClan=false) ===");

        // query ต้องตอบว่าง ไม่ใช่ค้างเฉย — client ทุกตัวเรียก GetParty ตอนพร้อมเล่นเสมอ
        Reset(); connection.Send(default(GetParty)); Pump(connection, 400);
        Check("GetParty answers with empty party (client clears HUD)",
            _emptyPartyReplies >= 1 && _lastParty.HasValue && string.IsNullOrEmpty(_lastParty.Value.Id) && !_lastParty.Value.Info.HasValue,
            $"emptyReplies={_emptyPartyReplies} withData={_partyReplies}");

        // mutation ทุกตัวต้องถูกปฏิเสธก่อนแตะ state
        Reset(); connection.Send(default(MakeParty)); Pump(connection, 400); AssertRejected("MakeParty");
        Reset(); connection.Send(new InviteIntoParty { InviteeEntityId = "someone-else" }); Pump(connection, 400); AssertRejected("InviteIntoParty");
        Reset(); connection.Send(default(JoinIntoParty)); Pump(connection, 400); AssertRejected("JoinIntoParty");
        Reset(); connection.Send(new RejectPartyInvitation { InviteeEntityId = null }); Pump(connection, 400); AssertRejected("RejectPartyInvitation(self)");
        Reset(); connection.Send(new RejectPartyInvitation { InviteeEntityId = "someone-else" }); Pump(connection, 400); AssertRejected("RejectPartyInvitation(leader cancel)");
        Reset(); connection.Send(default(LeaveParty)); Pump(connection, 400); AssertRejected("LeaveParty");
        Reset(); connection.Send(new KickPartyMember { MemberEntityId = "someone-else" }); Pump(connection, 400); AssertRejected("KickPartyMember");
        Reset(); connection.Send(new ElectPartyLeader { MemberEntityId = "someone-else" }); Pump(connection, 400); AssertRejected("ElectPartyLeader");

        // replay packet เดิมซ้ำหลายรอบก็ต้องถูกปฏิเสธทุกครั้ง ไม่ใช่ครั้งเดียวแล้วผ่าน
        Reset();
        for (int i = 0; i < 5; i++) connection.Send(default(MakeParty));
        Pump(connection, 800);
        Check("repeated MakeParty rejected every time", _aborts >= 5 && _partyReplies == 0,
            $"abort={_aborts} partyWithData={_partyReplies}");

        Check("still connected after all rejections", connection.Connected());

        connection.Close();
        Console.WriteLine();
        Console.WriteLine($"=== สรุป: ผ่าน {_passed} / ตก {_failed} ===");
        return _failed == 0 ? 0 : 1;
    }
}
