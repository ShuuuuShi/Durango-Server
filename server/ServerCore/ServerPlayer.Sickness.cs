using System;
using Durango.Utils;
using Messages;

namespace DurangoServer.Core;

// ============================================================================
// ServerPlayer.Sickness — ระบบป่วย (4 ก.ย. 2026 · เจ้าของเซิร์ฟสั่งเพิ่ม)
//
// "ถ้าป่วย งานคราฟใช้เวลานาน ของและเหนื่อยเพิ่มขึ้น เดินช้าขึ้น"
//   · คราฟต์นานขึ้น    → CraftDurationScale() คูณเพิ่ม
//   · เปลืองแรงขึ้น    → StaminaCostScale() คูณเพิ่ม (ค่า "ของ" ที่เสียต่อครั้ง)
//   · ล้าไวขึ้น        → BaseFatigueVelocity บวกเพิ่ม
//   · เดินช้าลง        → ส่ง SetBaseMoveSpeed ให้ client
//
// สาเหตุที่ป่วย (ปิด/ปรับได้ใน config → Sickness):
//   1. เปียกอยู่ในเขตหนาว (SnowField/Tundra) ติดต่อกัน 90 วิ — ต่อยอดจากระบบเปียก (บั๊ก #7)
//   2. กินของดิบจนปวดท้องซ้ำ 3 ครั้ง
//   3. `cheat sick` / `cheat cure` สำหรับทดสอบ
//
// สถานะที่ผู้เล่นเห็นใช้ id `poison_heat` เพราะมีชื่อ+ไอคอนใน client อยู่แล้ว
// ============================================================================

public partial class ServerPlayer
{
    /// <summary>id ที่ client มีชื่อ/ไอคอนอยู่แล้ว (ItemNameData) — ห้ามเปลี่ยนเป็น id ที่ไม่มีในตาราง</summary>
    private const string SickStatusEffectId = "poison_heat";

    private static SicknessConfig SickCfg => ServerConfig.Current.Sickness ?? SicknessConfig.Defaults();

    /// <summary>ป่วยอยู่ไหม</summary>
    public bool IsSick => SickCfg.Enabled && HasStatusEffect(SickStatusEffectId);

    /// <summary>เปียกในเขตหนาวมาแล้วกี่วินาที (สะสมจนถึงเกณฑ์แล้วป่วย)</summary>
    private double _coldWetSeconds;

    /// <summary>ปวดท้องจากของดิบมากี่ครั้งแล้ว</summary>
    private int _stomachacheStacks;

    /// <summary>ความเร็วเดินที่ส่งให้ client ล่าสุด — กันส่งซ้ำทุก tick</summary>
    private float _sentMoveSpeed;

    /// <summary>ทำให้ป่วย — คืน false ถ้าป่วยอยู่แล้ว/ระบบปิด</summary>
    public bool MakeSick(string reason = null)
    {
        SicknessConfig cfg = SickCfg;
        if (!cfg.Enabled || Dead) { return false; }
        bool wasSick = IsSick;
        ApplyTimedStatusEffect(SickStatusEffectId, cfg.DurationSeconds);
        _coldWetSeconds = 0;
        _stomachacheStacks = 0;
        SendStatusEffects();
        RefreshFatigueFromStatusEffects();
        PushMoveSpeed();
        if (!wasSick)
        {
            Send(new Info
            {
                Text = string.IsNullOrEmpty(reason)
                    ? "ป่วยแล้ว — ทำงานช้าลง เปลืองแรง เหนื่อยไว และเดินช้าลง"
                    : $"ป่วยแล้ว ({reason}) — ทำงานช้าลง เปลืองแรง เหนื่อยไว และเดินช้าลง"
            });
            Console.WriteLine("[sick] {0} ป่วย ({1}) นาน {2:F0} วิ", Name, reason ?? "-", cfg.DurationSeconds);
        }
        return !wasSick;
    }

    /// <summary>หายป่วย</summary>
    public bool CureSickness()
    {
        if (!ClearStatusEffect(SickStatusEffectId)) { return false; }
        _coldWetSeconds = 0;
        SendStatusEffects();
        RefreshFatigueFromStatusEffects();
        PushMoveSpeed();
        Send(new Info { Text = "หายป่วยแล้ว" });
        Console.WriteLine("[sick] {0} หายป่วย", Name);
        return true;
    }

    /// <summary>บอกความเร็วเดินให้ client (ป่วย = ช้าลง) — ส่งเฉพาะตอนค่าเปลี่ยนจริง</summary>
    private void PushMoveSpeed()
    {
        SicknessConfig cfg = SickCfg;
        float speed = cfg.BaseMoveSpeed > 0f ? cfg.BaseMoveSpeed : 500f;
        if (IsSick)
        {
            speed *= Math.Clamp(cfg.MoveSpeedScale, 0.1f, 1f);
        }
        if (Math.Abs(speed - _sentMoveSpeed) < 0.5f) { return; }
        _sentMoveSpeed = speed;
        Send(new SetBaseMoveSpeed { EntityId = EntityId, NormalSpeed = (int)MathF.Round(speed), BattleSpeed = (int)MathF.Round(speed) });
        Console.WriteLine("[sick] {0} ความเร็วเดิน → {1:F0}", Name, speed);
    }

    /// <summary>ปวดท้องจากของดิบ 1 ครั้ง — ครบเกณฑ์แล้วป่วย</summary>
    public void NoteStomachache()
    {
        SicknessConfig cfg = SickCfg;
        if (!cfg.Enabled || cfg.StomachacheStacksToSick <= 0) { return; }
        _stomachacheStacks++;
        if (_stomachacheStacks >= cfg.StomachacheStacksToSick)
        {
            MakeSick("กินของดิบซ้ำ ๆ");
        }
    }

    /// <summary>
    /// สะสมเวลา "เปียก + หนาว" แล้วป่วย — เรียกจาก ProcessWetness ทุกรอบตรวจ
    /// </summary>
    private void ProcessSickness(double now, double elapsed, bool wet, bool coldPlace)
    {
        SicknessConfig cfg = SickCfg;
        if (!cfg.Enabled) { return; }
        // สถานะป่วยหมดอายุเองแล้วต้องคืนความเร็วเดิน (PruneStatusEffects ตัดออกไปแล้ว)
        PushMoveSpeed();
        if (cfg.ColdWetSeconds <= 0f || IsSick) { return; }
        if (wet && coldPlace)
        {
            _coldWetSeconds += elapsed;
            if (_coldWetSeconds >= cfg.ColdWetSeconds)
            {
                MakeSick("ตัวเปียกอยู่ในที่หนาวนานเกินไป");
            }
        }
        else if (_coldWetSeconds > 0)
        {
            _coldWetSeconds = Math.Max(0, _coldWetSeconds - elapsed);
        }
    }
}
