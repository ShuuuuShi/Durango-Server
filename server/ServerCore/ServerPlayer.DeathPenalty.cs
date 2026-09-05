using System;
using System.Collections.Generic;
using Durango.Utils;
using Messages;
using Shared.Building;
using Shared.Etc;

namespace DurangoServer.Core;

// ============================================================================
// [TodoList/07 · 3 ก.ย. 2026] ตายแล้วเสียอะไร — ตาม constants.json → death_penalty
//
//   default_item_drop_ratio 0.5 · prevent_item_drop_ratio_by_level = 1 − 0.0125×level
//   ⇒ หล่น = 0.5 × (1 − 0.0125×lv)  (Lv.1 49% · Lv.40 25% · Lv.60 12.5% · Lv.80 0)
//   death_point_remaining_duration 300 (จุดตาย + ของ อยู่ 5 นาที)
//   gauge_ratio_by_death_count {0.6, 0.4, 0.2, 0.1} · fatigue_recovery_ratio_by_death_count [0.3, 0.2, 0.1, 0]
//
// ของที่หล่นไปอยู่ใน "กล่องของตก" (artifact กล่องที่ผู้ตายเป็น founder ⇒ เปิดได้คนเดียวผ่าน CanUseBox เดิม)
// ที่จุดตาย · ไม่พบ entity ถุงของตกในข้อมูลที่แถมมา จึงใช้ blueprint กล่องของเกม (Death.BoxBlueprint)
// ปิดได้ทั้งชุดที่ config Death.Enabled=false (ฟื้นเต็ม ไม่หล่น เหมือนเดิม)
// ============================================================================

public partial class ServerPlayer
{
    private int _deathCount;
    private double _lastDeathAt;
    /// <summary>หมุดจุดตายหมดอายุเมื่อไร (แม้ไม่มีของหล่นก็หายตาม death_point_remaining_duration)</summary>
    private double _deathPointExpiresAt;
    private string _deathBoxId;
    private double _deathBoxExpiresAt;

    private static DeathConfig DeathCfg => ServerConfig.Current.Death;

    private static bool DeathPenaltyOn => DeathCfg != null && DeathCfg.Enabled;

    /// <summary>จำนวนครั้งที่ตายติด ๆ กัน (ลดเหลือ 0 เมื่อไม่ตายนานเกิน DeathCountDecaySeconds)</summary>
    private int EffectiveDeathCount(double now)
    {
        DeathConfig cfg = DeathCfg;
        if (cfg == null || cfg.DeathCountDecaySeconds > 0 && _lastDeathAt > 0 && now - _lastDeathAt > cfg.DeathCountDecaySeconds)
        {
            return 0;
        }
        return _deathCount;
    }

    private static float RatioAt(float[] table, int index, float fallback)
    {
        if (table == null || table.Length == 0) { return fallback; }
        return table[Math.Clamp(index, 0, table.Length - 1)];
    }

    /// <summary>เรียกจาก Die() หลังจำจุดตายแล้ว — นับครั้ง + ของหล่นลงกล่องที่จุดตาย</summary>
    private void ApplyDeathPenalty(double now)
    {
        if (!DeathPenaltyOn)
        {
            return;
        }
        DeathConfig cfg = DeathCfg;
        _deathCount = EffectiveDeathCount(now) + 1;
        _lastDeathAt = now;
        _deathPointExpiresAt = now + cfg.DeathPointSeconds;

        // กล่องเก่าจากการตายครั้งก่อนยังอยู่ → เก็บก่อน (ของในนั้นย้ายมากล่องใหม่ ไม่หายไปเฉย ๆ)
        List<Item> carried = null;
        if (!string.IsNullOrEmpty(_deathBoxId))
        {
            carried = _world.TakeAllFromBox(_deathBoxId);
            RemoveDeathBox(announce: true);
        }

        var drop = carried ?? new List<Item>();
        float dropRatio = 0f;
        if (cfg.ItemDrop)
        {
            dropRatio = Math.Clamp(cfg.DropRatio * (1f - cfg.PreventPerLevel * Level), 0f, 1f);
            var equipped = new HashSet<string>(_equippedItems.Values, StringComparer.Ordinal);
            lock (_inventory)
            {
                for (int i = _inventory.Count - 1; i >= 0; i--)
                {
                    Item it = _inventory[i];
                    if (equipped.Contains(it.Id)) { continue; }          // ของที่ใส่อยู่ไม่หล่น (สึก 5% แทน)
                    if (ItemLevelData.DumpLocked.Contains(it.Prototype ?? string.Empty)) { continue; }   // ของเควสต์/อีเวนต์/ตั๋ว (dump_locked) ไม่หล่น
                    if (_deathRng.NextDouble() >= dropRatio) { continue; }
                    drop.Add(it);
                    ForgetInventoryItem(it.Id);
                    _inventory.RemoveAt(i);
                }
            }
        }

        if (drop.Count > 0)
        {
            if (TryPlaceDeathBox(drop, now, out string boxId))
            {
                _deathBoxId = boxId;
                _deathBoxExpiresAt = now + cfg.DeathPointSeconds;
                Console.WriteLine("[death] {0} ตายครั้งที่ {1} · ของหล่น {2} ชิ้น (อัตรา {3:P0}) ลงกล่อง {4} ที่ tile {5},{6} · อยู่ {7} วิ",
                    Name, _deathCount, drop.Count, dropRatio, boxId, _deathTile.x, _deathTile.y, cfg.DeathPointSeconds);
            }
            else
            {
                // วางกล่องไม่ได้ (ตายกลางน้ำ/ในสิ่งปลูกสร้าง) — คืนของให้ ไม่ให้หายฟรี
                lock (_inventory) { _inventory.AddRange(drop); }
                Console.WriteLine("[death] {0} ตายครั้งที่ {1} · หาที่วางกล่องของตกไม่ได้ — คืนของ {2} ชิ้น", Name, _deathCount, drop.Count);
            }
            SendInventory();
        }
        else
        {
            Console.WriteLine("[death] {0} ตายครั้งที่ {1} · ไม่มีของหล่น (อัตรา {2:P0})", Name, _deathCount, dropRatio);
        }
        MarkDirty();
    }

    private static readonly Random _deathRng = new Random();

    /// <summary>วางกล่องของตกที่จุดตาย (หรือช่องว่างใกล้ ๆ ในรัศมี 3 tile)</summary>
    private bool TryPlaceDeathBox(List<Item> items, double now, out string boxId)
    {
        boxId = null;
        DeathConfig cfg = DeathCfg;
        string blueprint = string.IsNullOrEmpty(cfg.BoxBlueprint) ? "fur_box_03_leaf" : cfg.BoxBlueprint;
        if (!RecipeData.BlueprintType.TryGetValue(blueprint, out ushort entityType))
        {
            Console.WriteLine("[death] blueprint กล่องของตก \"{0}\" ไม่มีในตาราง — ปิดของหล่น", blueprint);
            return false;
        }
        Point2 size = RecipeData.BlueprintSize.TryGetValue(blueprint, out var bp) && bp.x > 0 ? new Point2(bp.x, bp.y) : new Point2(1, 1);
        Point2? spot = null;
        for (int ring = 0; ring <= 3 && spot == null; ring++)
        {
            for (int dx = -ring; dx <= ring && spot == null; dx++)
            {
                for (int dy = -ring; dy <= ring; dy++)
                {
                    if (Math.Max(Math.Abs(dx), Math.Abs(dy)) != ring) { continue; }
                    var tile = new Point2(_deathTile.x + dx, _deathTile.y + dy);
                    if (tile.x < 0 || tile.y < 0 || tile.x >= _world.Terrain.Width || tile.y >= _world.Terrain.Height) { continue; }
                    if (!_world.Terrain.IsLand(tile.x, tile.y)) { continue; }
                    if (_world.Terrain.IsCliff(tile.x, tile.y)) { continue; }
                    if (_world.HasArtifactOverlapping(tile, size)) { continue; }
                    spot = tile;
                    break;
                }
            }
        }
        if (spot == null)
        {
            return false;
        }
        // ฝัง "หมดอายุเมื่อ" ไว้ใน id ให้ ServerWorld.SweepDeathBoxes เก็บได้แม้เจ้าของออฟไลน์/เซิร์ฟล้ม (world save ไม่มี owner)
        long expiresAt = (long)(now + DeathCfg.DeathPointSeconds);
        string id = $"deathbox_{expiresAt}_" + Guid.NewGuid().ToString("N").Substring(0, 8);
        // floor ต้องเป็น 0 ไม่ใช่ null — POI ของโลก (ServerWorld) ก็ใส่ 0 · ใส่ null แล้ว client ไม่วาดกล่อง (เทส 3 ก.ย.)
        AppearArtifact box = ArtifactFactory.Make(EntityId, id, entityType, spot.Value, size,
            Rotation.None, 0, 1, blueprint, BuildingState.Completed);
        _world.AddArtifact(box, blueprint);
        if (!_world.TryPutInBox(id, items, int.MaxValue))
        {
            _world.RemoveArtifact(id);
            return false;
        }
        _world.AnnounceArtifact(box);
        boxId = id;
        return true;
    }

    private void RemoveDeathBox(bool announce)
    {
        if (string.IsNullOrEmpty(_deathBoxId))
        {
            return;
        }
        string id = _deathBoxId;
        _deathBoxId = null;
        _deathBoxExpiresAt = 0;
        _world.RemoveArtifact(id);
        if (announce)
        {
            _world.AnnounceGone(id);
        }
        MarkDirty();
    }

    /// <summary>ทุก Process(): กล่องของตกหมดเวลา → ลบกล่อง + หมุดจุดตาย</summary>
    private void ProcessDeathBox(double now)
    {
        // หมุดจุดตายหมดอายุแม้ไม่มีกล่อง (ตายแบบไม่มีของหล่น)
        if (string.IsNullOrEmpty(_deathBoxId) && _hasDeathPoint && DeathPenaltyOn && _deathPointExpiresAt > 0 && now >= _deathPointExpiresAt)
        {
            _hasDeathPoint = false;
            _deathPointExpiresAt = 0;
            MarkDirty();
            Send(BuildPoints());
            return;
        }
        if (string.IsNullOrEmpty(_deathBoxId) || now < _deathBoxExpiresAt)
        {
            return;
        }
        int left = _world.GetBoxItems(_deathBoxId).Length;
        Console.WriteLine("[death] กล่องของตกของ {0} หมดเวลา — ของที่เหลือ {1} ชิ้นหายไป", Name, left);
        RemoveDeathBox(announce: true);
        if (_hasDeathPoint)
        {
            _hasDeathPoint = false;
            Send(BuildPoints());
        }
    }

    /// <summary>เรียกหลังหยิบของจากกล่อง — กล่องของตกว่างแล้วก็เก็บทิ้ง + เอาหมุดออก</summary>
    private void AfterTakeFromBox(string boxId)
    {
        if (string.IsNullOrEmpty(_deathBoxId) || boxId != _deathBoxId)
        {
            return;
        }
        if (_world.GetBoxItems(boxId).Length > 0)
        {
            return;
        }
        Console.WriteLine("[death] {0} เก็บของตกคืนครบแล้ว — เก็บกล่อง", Name);
        RemoveDeathBox(announce: true);
        if (_hasDeathPoint)
        {
            _hasDeathPoint = false;
            Send(BuildPoints());
        }
    }

    /// <summary>
    /// เกจตอนฟื้น: ตายครั้งแรก 60% · ซ้ำ 40/20/10% · ความล้าลดแค่ 30/20/10/0% (ไม่ล้าง)
    /// ปิด Death.Enabled = ฟื้นเต็ม + ล้างความล้า (พฤติกรรมเดิม)
    /// </summary>
    private void RestoreOnRevive()
    {
        if (!DeathPenaltyOn)
        {
            RestoreSurvival(clearFatigue: true);
            return;
        }
        DeathConfig cfg = DeathCfg;
        double now = Times.UnixTimeNow();
        int idx = Math.Max(0, EffectiveDeathCount(now) - 1);
        float gauge = Math.Clamp(RatioAt(cfg.GaugeRatios, idx, 0.6f), 0.05f, 1f);
        float fatigueRecover = Math.Clamp(RatioAt(cfg.FatigueRecoveryRatios, idx, 0.3f), 0f, 1f);
        RestoreSurvival(clearFatigue: false);
        SetGaugeValue("life", LifeMax * gauge);
        SetGaugeValue("stamina", StaminaMax * gauge);
        float fatigueNow = _fatigue != null ? _fatigue.Value : 0f;
        SetGaugeValue("fatigue", Math.Max(0f, fatigueNow - FatigueMax * fatigueRecover));
        Console.WriteLine("[death] {0} ฟื้น: เลือด/สตามินา {1:P0} · ความล้าลด {2:P0} (ตายติดกัน {3} ครั้ง)", Name, gauge, fatigueRecover, idx + 1);
    }

    private void ApplyDeathPenaltySave(PlayerSave save)
    {
        _deathCount = Math.Max(0, save.DeathCount);
        _lastDeathAt = save.LastDeathAt;
        _deathBoxId = string.IsNullOrEmpty(save.DeathBoxId) ? null : save.DeathBoxId;
        _deathBoxExpiresAt = save.DeathBoxExpiresAt;
        _deathPointExpiresAt = save.DeathPointExpiresAt;
        // กล่องที่บันทึกไว้ต้องยังมีอยู่ในโลก (เซฟโลกกับเซฟผู้เล่นคนละไฟล์) ไม่งั้นทิ้งการอ้างอิง
        if (_deathBoxId != null && !_world.TryGetArtifact(_deathBoxId, out _))
        {
            _deathBoxId = null;
            _deathBoxExpiresAt = 0;
        }
    }

    private void FillDeathPenaltySave(PlayerSave save)
    {
        save.DeathCount = _deathCount;
        save.LastDeathAt = _lastDeathAt;
        save.DeathBoxId = _deathBoxId;
        save.DeathBoxExpiresAt = _deathBoxExpiresAt;
        save.DeathPointExpiresAt = _deathPointExpiresAt;
    }
}
