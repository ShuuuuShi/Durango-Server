using System;
using System.Linq;
using Durango.Utils;
using Messages;
using Shared.Etc;

namespace DurangoServer.Core;

// ============================================================================
// ServerPlayer.Wetness — สถานะ "เปียก" / "สกปรก" / "ล้างตัวแล้ว"
//
// 🐛 [4 ก.ย. 2026] บั๊ก #7 ที่ผู้เล่นแจ้ง: "ลงน้ำ ค่าสถานะเปียกไม่ขึ้น · โดนฝนก็ไม่ขึ้น ·
//    ตักโคลนค่าสกปรกก็ไม่ขึ้น (เลยไม่รู้ว่าล้างตัวในน้ำได้ไหม)" — เซิร์ฟ **ไม่เคยมีระบบนี้เลย**
//    ทั้งที่ข้อมูลเกม (survival/status_effects.json) มี wet/dirty/clean ครบ และ client มีไอคอนอยู่แล้ว
//
// ค่าทั้งหมดมาจากข้อมูลเกมจริง ไม่ได้ตั้งเอง:
//   wet   — duration 120 วิ · ร้อน −0.2 ล้า · หนาว +0.3 ล้า (ดับ burn)
//   dirty — ไม่มีวันหมดเอง · +0.2 ล้า (lv1) / +0.5 (lv2) · ถูกดับด้วย clean
//   clean — duration 360 วิ · −0.2 ล้า · ดับ dirty
//
// กลไก: ตรวจทุก ~2 วิใน Process() — อยู่ในน้ำ = เปียก + ล้างตัวสะอาด · ฝนตก = เปียก
// ผลกับความล้าเข้าทาง RefreshFatigueFromStatusEffects() ที่อ่าน status_effects.json อยู่แล้ว
// ============================================================================

public partial class ServerPlayer
{
    private const string WetStatusEffectId = "wet";
    private const string DirtyStatusEffectId = "dirty";
    private const string CleanStatusEffectId = "clean";

    /// <summary>ข้อมูลเกม: wet 120 วิ · clean 360 วิ (survival/status_effects.json)</summary>
    private const double WetSeconds = 120.0;
    private const double CleanSeconds = 360.0;

    /// <summary>ตรวจสภาพรอบตัวถี่แค่ไหน — ไม่ต้องทุก tick</summary>
    private const double WetCheckInterval = 2.0;
    private double _nextWetCheck;

    /// <summary>ไบโอมที่ถือว่า "ลงน้ำ"</summary>
    private static bool IsWaterBiome(Shared.Region.Biome b)
        => b == Shared.Region.Biome.River || b == Shared.Region.Biome.Lake
           || b == Shared.Region.Biome.ColdOcean || b == Shared.Region.Biome.WarmOcean;

    /// <summary>ฝนที่ทำให้เปียก (ชื่อสภาพอากาศตามที่ ServerWeather ใช้)</summary>
    private static bool IsRainy(string weather)
        => weather == "rainy" || weather == "heavy_rainy" || weather == "storm";

    /// <summary>
    /// เปิด/ต่ออายุ status effect ที่มีเวลาจำกัด — ต่ออายุถ้ามีอยู่แล้ว ไม่ซ้อนอันใหม่
    /// คืน true เมื่อ "เพิ่งติดใหม่" (ใช้ตัดสินว่าต้องบอกผู้เล่นไหม)
    /// </summary>
    private bool ApplyTimedStatusEffect(string id, double seconds, int level = 1)
    {
        PruneStatusEffects();
        double now = Times.UnixTimeNow();
        StatusEffectSave found = _statusEffects.FirstOrDefault(x => x.Id == id || x.EffectId == id);
        bool isNew = found == null || !found.Enabled;
        if (found == null)
        {
            _statusEffects.Add(new StatusEffectSave
            {
                Id = id,
                EffectId = id,
                Level = Math.Max(1, level),
                Since = now,
                Until = seconds > 0 ? now + seconds : 0,
                Enabled = true
            });
        }
        else
        {
            found.Enabled = true;
            found.Level = Math.Max(1, level);
            if (isNew) { found.Since = now; }
            found.Until = seconds > 0 ? now + seconds : 0;
        }
        MarkDirty();
        return isNew;
    }

    /// <summary>ปิด status effect (คืน true ถ้าเคยติดอยู่จริง)</summary>
    private bool ClearStatusEffect(string id)
    {
        StatusEffectSave found = _statusEffects.FirstOrDefault(x => (x.Id == id || x.EffectId == id) && x.Enabled);
        if (found == null) { return false; }
        found.Enabled = false;
        MarkDirty();
        return true;
    }

    private bool HasStatusEffect(string id)
    {
        double now = Times.UnixTimeNow();
        return _statusEffects.Any(x => (x.Id == id || x.EffectId == id) && x.Enabled
                                       && (x.Until <= 0 || x.Until > now));
    }

    /// <summary>เปื้อนโคลน/ดิน — เรียกจากตอนขุด/ตักโคลน (ไม่มีวันหมดเอง ต้องไปล้าง)</summary>
    public void MakeDirty(int level = 1)
    {
        if (Dead) { return; }
        // ล้างตัวอยู่ = หายสะอาดทันทีเมื่อเปื้อนใหม่ (ข้อมูลเกม: dirty ถูกดับด้วย clean และกลับกัน)
        ClearStatusEffect(CleanStatusEffectId);
        if (ApplyTimedStatusEffect(DirtyStatusEffectId, 0, level))
        {
            Send(new Info { Text = "ตัวเปื้อนโคลนแล้ว — ลงน้ำเพื่อล้างตัว" });
        }
        SendStatusEffects();
        RefreshFatigueFromStatusEffects();
    }

    /// <summary>ตรวจน้ำ/ฝนแล้วอัปเดตสถานะเปียก-สะอาด (เรียกจาก Process ทุก ~2 วิ)</summary>
    private void ProcessWetness(double now)
    {
        if (now < _nextWetCheck || Dead || !SceneReady) { return; }
        _nextWetCheck = now + WetCheckInterval;
        if (_world?.Terrain == null) { return; }

        int tx = (int)(CurrentPosition.x / 200f);
        int ty = (int)(CurrentPosition.y / 200f);
        Shared.Region.Biome biome = _world.Terrain.BiomeAt(tx, ty);
        bool inWater = IsWaterBiome(biome);
        bool rain = IsRainy(_world.Weather?.Current);
        bool changed = false;

        if (inWater || rain)
        {
            // อยู่ในน้ำ/กลางฝน = เปียกและต่ออายุไปเรื่อย ๆ พอขึ้นฝั่งค่อยนับถอยหลัง 120 วิ
            if (ApplyTimedStatusEffect(WetStatusEffectId, WetSeconds))
            {
                Send(new Info { Text = inWater ? "ตัวเปียกน้ำแล้ว" : "โดนฝนจนตัวเปียก" });
            }
            changed = true;
        }

        // ลงน้ำ = ล้างตัว (ข้อมูลเกม: clean ดับ dirty)
        if (inWater && HasStatusEffect(DirtyStatusEffectId))
        {
            ClearStatusEffect(DirtyStatusEffectId);
            ApplyTimedStatusEffect(CleanStatusEffectId, CleanSeconds);
            Send(new Info { Text = "ล้างตัวในน้ำแล้ว — หายสกปรก" });
            changed = true;
        }

        // ระบบป่วย — เปียกอยู่ในที่หนาวนาน ๆ แล้วป่วย
        bool coldPlace = biome == Shared.Region.Biome.SnowField || biome == Shared.Region.Biome.Tundra
                         || biome == Shared.Region.Biome.ColdOcean;
        ProcessSickness(now, WetCheckInterval, HasStatusEffect(WetStatusEffectId) || inWater || rain, coldPlace);

        // หมดอายุเองก็ต้องบอก client ไม่งั้นไอคอนค้าง
        int before = _statusEffects.Count(x => x.Enabled);
        PruneStatusEffects();
        if (changed || _statusEffects.Count(x => x.Enabled) != before)
        {
            SendStatusEffects();
            RefreshFatigueFromStatusEffects();
        }
    }
}
