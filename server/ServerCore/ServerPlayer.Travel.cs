using System;
using System.Collections.Generic;
using System.Text;
using Messages;

namespace DurangoServer.Core;

/// <summary>
/// Beta 1.1 — เดินทางข้ามเกาะ
///
/// **1 เกาะ = 1 server** ตัวเกมจึงต้องตัดการเชื่อมต่อแล้วต่อใหม่ไปอีกเกาะ ลำดับคือ:
/// ```
/// ผู้เล่นสั่งเดินทาง → server ตรวจเลเวล/ปลายทาง
///                   → เซฟตัวละครทันที (ของ/เลเวล/สกิล ต้องไปด้วยครบ)
///                   → ส่ง Info "##goto &lt;host:port&gt;" (client ที่ patch แล้วอ่านบรรทัดนี้)
///                   → ส่ง Emigrated → client ปิด connection แล้วกลับหน้าไตเติ้ล
/// ```
/// `Emigrated` เป็น packet ของเกมเองอยู่แล้ว (`GameManager.EmigratedReceived` → `Connections.Frontend.Close()`)
/// ส่วนการ **เลือกปลายทาง** ต้องพึ่ง patch ฝั่ง client เพราะ `Server.ConnectTo()` ฮาร์ดโค้ด gateway 8190
/// และไม่มี packet ไหนของเกมที่ส่ง "ที่อยู่เซิร์ฟใหม่" มาให้ — ดู docs/server/Islands.md
///
/// ตัวละครใช้ไฟล์เซฟร่วมกันทุกเกาะ (`saves/players/`) ของกับเลเวลจึงตามไปเอง
/// ส่วนบ้าน/กล่องอยู่กับเกาะ (`saves/worlds/&lt;id&gt;.json`) — ของในกล่องไม่ตามไปด้วย
/// </summary>
public partial class ServerPlayer
{
    /// <summary>ข้อความนำหน้าที่ client (ที่ patch แล้ว) ใช้จับว่าเป็นคำสั่งย้ายเซิร์ฟ</summary>
    public const string GotoPrefix = "##goto ";

    /// <summary>รายชื่อเกาะทั้งหมด + บอกว่าไปได้ไหม</summary>
    public string DescribeIslands()
    {
        if (IslandRegistry.All.Count == 0)
        {
            return "เซิร์ฟนี้เปิดแบบเกาะเดียว (ไม่ได้ใส่ --island)";
        }
        var sb = new StringBuilder();
        sb.Append("เกาะทั้งหมด (คุณเลเวล ").Append(Level).Append("):");
        for (int i = 0; i < IslandRegistry.All.Count; i++)
        {
            IslandInfo isle = IslandRegistry.All[i];
            bool here = IslandRegistry.Current != null && isle.Id == IslandRegistry.Current.Id;
            bool canGo = Level >= isle.RequiredLevel;
            sb.Append("\n  ").Append(isle.Id).Append(" | ").Append(isle.Name)
              .Append(" | สัตว์ lv").Append(isle.MinLevel).Append('-').Append(isle.MaxLevel)
              .Append(" | ต้องเลเวล ").Append(isle.RequiredLevel).Append('+')
              .Append(here ? "  ← อยู่ที่นี่" : (canGo ? "  (ไปได้)" : "  (เลเวลไม่ถึง)"));
        }
        return sb.ToString();
    }

    /// <summary>
    /// สั่งเดินทางไปเกาะอื่น คืนข้อความอธิบายผลเสมอ (ไม่ throw)
    /// ตรวจ 4 ข้อ: มีเกาะนั้นจริง · ไม่ใช่เกาะเดิม · เลเวลถึง · ไม่ได้ตายอยู่
    /// </summary>
    public string TravelTo(string islandId)
    {
        if (!ServerConfig.Current.Features.IslandTravel)
        {
            return "การเดินทางข้ามเกาะยังปิดอยู่ในรอบนี้ (เปิดที่ Features.IslandTravel ใน config.json)";
        }
        if (IslandRegistry.Current == null)
        {
            return "เซิร์ฟนี้เปิดแบบเกาะเดียว เดินทางไม่ได้ (ต้องเปิดด้วย --island)";
        }
        IslandInfo dest = IslandRegistry.Find(islandId);
        if (dest == null)
        {
            return $"ไม่มีเกาะ '{islandId}' — มีอยู่: {string.Join(", ", IslandRegistry.Ids())}";
        }
        if (dest.Id == IslandRegistry.Current.Id)
        {
            return $"อยู่ที่ {dest.Name} อยู่แล้ว";
        }
        if (Dead)
        {
            return "ตายอยู่ เดินทางไม่ได้ — ฟื้นก่อน";
        }
        if (Level < dest.RequiredLevel)
        {
            return $"{dest.Name} ต้องเลเวล {dest.RequiredLevel} ขึ้นไป (ตอนนี้ {Level})";
        }

        // เซฟก่อนตัดสาย ไม่งั้นของที่เก็บมาหลัง autosave ครั้งล่าสุดหายทั้งหมด
        // และ LastIsland ต้องเป็น "เกาะที่กำลังจะออก" เพื่อให้ปลายทางรู้ว่าเป็นคนมาใหม่
        Save();

        Console.WriteLine("[island] {0} เดินทาง {1} → {2} ({3})", Name, IslandRegistry.Current.Id, dest.Id, dest.Address);
        Send(new Info { Text = GotoPrefix + dest.Address });
        Send(new Info { Text = $"กำลังเดินทางไป {dest.Name}..." });
        Send(new Emigrated { Type = Shared.Teleport.TeleportType.Unknown });
        return $"ส่ง {Name} ไป {dest.Name} ({dest.Address}) แล้ว";
    }
}
