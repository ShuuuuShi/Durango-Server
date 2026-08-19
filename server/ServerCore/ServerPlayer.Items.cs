using System;
using System.Collections.Generic;
using Durango.Network;
using Messages;

namespace DurangoServer.Core;

// Beta 1.0 — ของในกระเป๋าที่ยังไม่มีใครรับผิดชอบ: ทิ้งของ (DumpItems) และใช้ของ (UseItem)
//
// ทั้งสองอย่างนี้ client ยิงมาตั้งแต่แรกแต่ **ไม่มี handler ฝั่ง server เลย**
// ผลที่เจอตอนเทสบอทฟาร์ม 30 นาที: กระเป๋าเต็ม 50 ช่องแล้วจบเห่ — เก็บของต่อไม่ได้
// และไม่มีทางเอาของออกนอกจากมีกล่องอยู่ใกล้ ๆ (ซึ่งคนเพิ่งเข้าเกมยังไม่มี)
public partial class ServerPlayer
{
    /// <summary>ทิ้งได้สูงสุดกี่ชิ้นต่อ 1 packet (กันยิงลิสต์ยาว ๆ มาให้ไล่ลูป)</summary>
    private const int MaxDumpPerRequest = 50;

    /// <summary>
    /// 🐛 เดิมตัดสินว่า "กินได้ไหม" จาก **คำที่อยู่ในชื่อ prototype** (meat/fruit/egg/...)
    /// แล้วเติมสตามินา 30 เท่ากันหมดทุกอย่าง
    /// ⇒ ต้มน้ำซุปทั้งเย็นได้ผลเท่ากับกัดผลไม้ดิบ · ระบบทำอาหารจึงไม่มีความหมาย
    /// ⇒ และของที่ชื่อมีคำว่า seed/food แต่กินไม่ได้จริงก็กินได้
    ///
    /// ตอนนี้ใช้ข้อมูลโภชนาการจริงของเกม 352 ชนิด (<see cref="FoodData"/>)
    /// ⇒ อยู่ในตาราง = กินได้ · ไม่อยู่ = กินไม่ได้ · ได้เท่าไรก็ตามตารางคูณสเกลใน config
    /// </summary>
    private static bool IsEdible(Item item)
    {
        return FoodData.IsFood(item.Prototype);
    }

    /// <summary>กินชิ้นถัดไปได้เมื่อไร (digestivetime ของเกม) — 0 = กินได้เลย</summary>
    private double _canEatAt;

    /// <summary>
    /// ทิ้งของจากกระเป๋า (หรือจากกล่อง ถ้า client ระบุ SourceProp มา)
    /// client เรียกตอนกด "ทิ้ง" ในกระเป๋า — ไม่มีอันนี้ กระเป๋าเต็มแล้วตันถาวร
    /// </summary>
    private void HandleDumpItems(DumpItems msg, PacketHeader header)
    {
        if (Dead)
        {
            Send(default(Abort), header.Seq);
            return;
        }
        if (msg.ItemIds == null || msg.ItemIds.Length == 0 || msg.ItemIds.Length > MaxDumpPerRequest)
        {
            Send(default(Abort), header.Seq);
            return;
        }
        for (int i = 0; i < msg.ItemIds.Length; i++)
        {
            if (IsItemLocked(msg.ItemIds[i]))
            {
                Send(default(Abort), header.Seq);
                return;
            }
        }

        // ทิ้งของในกล่อง: ต้องเป็นกล่องของตัวเองและอยู่ในระยะเอื้อม (M-4 เหมือน TakeOutItem)
        if (msg.SourceProp.HasValue && !string.IsNullOrEmpty(msg.SourceProp.Value.EntityId))
        {
            string boxId = msg.SourceProp.Value.EntityId;
            if (!CanUseBox(boxId, header))
            {
                return;
            }
            // TakeFromBox เอาออกจากกล่องให้แล้ว — ของที่ได้มาไม่ต้องเอาเข้ากระเป๋า คือทิ้งไปเลย
            List<Item> dumped = _world.TakeFromBox(boxId, msg.ItemIds, msg.ItemIds.Length);
            if (dumped.Count == 0)
            {
                Send(default(Abort), header.Seq);
                return;
            }
            var dumpedIds = new string[dumped.Count];
            for (int i = 0; i < dumped.Count; i++) dumpedIds[i] = dumped[i].Id;
            Console.WriteLine("[item] {0} ทิ้งของ {1} ชิ้นจากกล่อง {2}", Name, dumped.Count, boxId);
            _world.Broadcast(new InventoryUpdated { EntityId = boxId, RemovedItemIds = dumpedIds });
            MarkDirty();
            Send(default(OK), header.Seq);
            return;
        }

        // ทิ้งของในกระเป๋าตัวเอง — id ที่ไม่มีจริงถูกข้ามเงียบ ๆ (ไม่ทำอะไรหาย)
        var removed = new List<string>();
        lock (_inventory)
        {
            for (int i = 0; i < msg.ItemIds.Length; i++)
            {
                int idx = _inventory.FindIndex(x => x.Id == msg.ItemIds[i]);
                if (idx >= 0)
                {
                    removed.Add(_inventory[idx].Id);
                    ForgetInventoryItem(_inventory[idx].Id);
                    _inventory.RemoveAt(idx);
                }
            }
        }
        if (removed.Count == 0)
        {
            Send(default(Abort), header.Seq);
            return;
        }

        Console.WriteLine("[item] {0} ทิ้งของ {1} ชิ้น", Name, removed.Count);
        MarkDirty();
        Send(new InventoryUpdated { EntityId = EntityId, RemovedItemIds = removed.ToArray() });
        Send(default(OK), header.Seq);
        SendInventory();
    }

    /// <summary>
    /// ใช้ของ (beta = กินอาหาร) — ได้สตามินาคืนและความล้าลด แล้วไอเทมหายไป 1 ชิ้น
    /// client รับได้ทั้ง StartTimer และ OK เป็นคำตอบว่าสำเร็จ (ดู InventorySystem.UseItem)
    /// </summary>
    private void HandleUseItem(UseItem msg, PacketHeader header)
    {
        if (Dead)
        {
            Send(default(Abort), header.Seq);
            return;
        }
        if (string.IsNullOrEmpty(msg.ItemId))
        {
            Send(default(Abort), header.Seq);
            return;
        }
        if (IsItemLocked(msg.ItemId))
        {
            Send(default(Abort), header.Seq);
            return;
        }

        double now = Durango.Utils.Times.UnixTimeNow();
        if (now < _canEatAt)
        {
            // ในเกมจริงอาหารมี "เวลาย่อย" — ไม่มีตัวนี้ก็รัวกินทั้งกระเป๋ารวดเดียวจนสตามินาเต็มตลอด
            Console.WriteLine("[item] ปฏิเสธ {0}: ยังอิ่มอยู่ (อีก {1:F0} วิ)", Name, _canEatAt - now);
            Send(new Info { Text = "เพิ่งกินไป รออีกสักครู่" }, header.Seq);
            Send(default(Abort), header.Seq);
            return;
        }

        Item item;
        lock (_inventory)
        {
            int idx = _inventory.FindIndex(x => x.Id == msg.ItemId);
            if (idx < 0)
            {
                Console.WriteLine("[item] ปฏิเสธ {0}: ไม่มีไอเทม {1} ในกระเป๋า", Name, msg.ItemId);
                Send(default(Abort), header.Seq);
                return;
            }
            item = _inventory[idx];
            if (!IsEdible(item))
            {
                Console.WriteLine("[item] ปฏิเสธ {0}: {1} ({2}) กินไม่ได้", Name, item.Name, item.Prototype);
                Send(default(Abort), header.Seq);
                return;
            }
            ForgetInventoryItem(item.Id);
            _inventory.RemoveAt(idx);
        }

        ApplyFoodEffect(item, out float stamina, out float fatigueRelief, out float life, out string motion, out int digestSeconds);
        GainProficiency(Shared.Skill.Category.Survival);   // ดูแลตัวเองเป็น = ชำนาญการเอาชีวิตรอด
        _canEatAt = now + digestSeconds;
        MarkDirty();

        Send(new ItemUsed { Motion = motion, Time = 1.5f, Msg = null });
        Send(new InventoryUpdated { EntityId = EntityId, RemovedItemIds = new[] { item.Id } });
        Send(default(OK), header.Seq);
        SendInventory();
    }

    /// <summary>
    /// กินของ 1 ชิ้นแล้วเกิดอะไร — ค่าทั้งหมดมาจากข้อมูลจริงของเกมคูณสเกลใน <c>data/config.json</c>
    ///
    /// ของดิบ (tag `raw_food` — เนื้อ/ปลา) ให้พลังน้อยกว่าและทำให้ล้าเพิ่ม
    /// **นี่คือเหตุผลที่ต้องเอาเนื้อไปย่างก่อนกิน** ไม่ใช่กินดิบตรงจุดที่ล่าได้เลย
    /// </summary>
    private void ApplyFoodEffect(Item item, out float stamina, out float fatigueRelief, out float life,
        out string motion, out int digestSeconds)
    {
        FoodConfig cfg = ServerConfig.Current.Food;
        stamina = 0f;
        fatigueRelief = 0f;
        life = 0f;
        motion = "Barehand_Eat";
        digestSeconds = 0;

        int level = item.Level > 0 ? item.Level : 1;
        if (!FoodData.TryGet(item.Prototype, level, out FoodData.Entry food))
        {
            return;
        }

        // ดู tag ที่ติดมากับ **ไอเทมชิ้นนั้น** ไม่ใช่ตาราง prototype — เนื้อที่ย่างแล้วเป็น prototype
        // เดียวกับเนื้อดิบ ต่างกันที่ tag (ดู ItemProcessing) ถ้าดูจาก prototype จะย่างเท่าไรก็ยังดิบอยู่ดี
        bool raw = ItemProcessing.IsRaw(item);
        stamina = food.EnergyAt(level) * cfg.EnergyScale;
        if (raw)
        {
            stamina *= cfg.RawFoodEnergyScale;
        }
        // ข้อมูลเกมเก็บความล้าเป็นเลขติดลบ (กินแล้วล้าลด) — ค่าบวกคือของที่ทำให้ล้าเพิ่ม
        fatigueRelief = Math.Max(0f, -food.Fatigue * cfg.FatigueScale);
        life = Math.Max(0f, food.HealthAt(level) * cfg.HealthScale);
        digestSeconds = (int)Math.Round(food.DigestiveTime * cfg.DigestScale);

        // ท่ากินที่ client เล่น: ของเหลวใช้ท่าดื่ม
        motion = food.EatMotion == "Drink" ? "Barehand_Drink" : "Barehand_Eat";

        RestoreStamina(stamina, fatigueRelief);
        RestoreSatiety(food.Satiety);
        ApplyFoodStatusEffect(food, level);
        if (life > 0f)
        {
            RestoreLife(life);
        }
        if (raw && cfg.RawFoodFatigue > 0f)
        {
            AddFatigue(cfg.RawFoodFatigue);
        }

        Console.WriteLine("[item] {0} กิน {1}{2} (+{3:F0} สตามินา{4}{5})",
            Name, item.Name ?? item.Prototype, raw ? " [ดิบ]" : string.Empty, stamina,
            fatigueRelief > 0f ? $" · ล้า −{fatigueRelief:F0}" : string.Empty,
            life > 0f ? $" · เลือด +{life:F0}" : string.Empty);
    }
}
