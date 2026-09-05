using System;
using System.Collections.Generic;
using Messages;

namespace DurangoServer.Core;

/// <summary>
/// การ "แปรรูป" ของ — สูตรชนิด Modify (type 1) ในข้อมูลเกม เช่น ย่าง / ต้ม / นึ่ง / ทอด / ตากแห้ง / รมควัน
///
/// ต่างจากสูตรปกติ (type 0) ตรงที่ **ไม่ได้สร้างของใหม่จากศูนย์** แต่เอาของในช่อง <c>base</c>
/// มาเปลี่ยนสภาพ — เนื้อดิบ 1 ก้อนเข้าไป ได้เนื้อสุก 1 ก้อนออกมา
/// สูตรพวกนี้จึงไม่มี <c>prototype_id</c> ในข้อมูลเกม (73 สูตร · เป็นสูตรทำอาหาร 26)
///
/// ผลลัพธ์ = **ของเดิมที่เปลี่ยนสภาพ** — เนื้อย่างยังเป็น prototype <c>meat</c> เหมือนเดิม
/// แต่ **ตัด tag `raw_food` ออกแล้วเติม `taste_good`** ตามที่ tag `raw_food` ในข้อมูลเกมบอกไว้
/// (<c>effect_off: 'taste_good'</c>) ⇒ กินแล้วไม่โดนโทษของดิบอีกต่อไป
///
/// ⚠️ เคยลองอีกทางคือประกอบชื่อ "สูตร_tag" (skewer + meat = <c>skewer_meat</c>) แล้วพบว่า **ใช้ไม่ได้**
/// เพราะ <c>skewer_meat</c> ในข้อมูลเกมติด tag <c>blunt_onehand</c> อย่างเดียว (เป็นโมเดลของที่ถือ
/// ไม่ใช่ของกิน) — ย่างเนื้อแล้วได้ของที่กินไม่ได้ จึงเลิกใช้วิธีนั้น
///
/// ⚠️ สภาพ "สุกแล้ว" ต้องเซฟลงไฟล์ด้วย (<see cref="ItemSave.ProcessedBy"/>)
/// ไม่งั้นออกเกมแล้วเข้าใหม่ เนื้อย่างจะกลับไปเป็นเนื้อดิบ
/// </summary>
public static class ItemProcessing
{
    /// <summary>tag ที่บอกว่า "ดิบ" — ตัดออกเมื่อแปรรูปแล้ว</summary>
    public const string RawTag = "raw_food";

    /// <summary>tag ที่ติดให้แทน (ข้อมูลเกม: tag `raw_food` มี effect_off = 'taste_good')</summary>
    public const string CookedTag = "taste_good";

    /// <summary>ช่องที่เก็บ "ของตั้งต้น" ของสูตรแปรรูป — ข้อมูลเกมใช้ชื่อนี้ทั้ง 73 สูตร</summary>
    public const string BaseSlot = "base";

    /// <summary>tag ของของที่แปรรูปแล้ว — ตัด `raw_food` ออก เติม `taste_good`</summary>
    public static Tag[] ProcessedTags(string basePrototype)
    {
        Tag[] tags = ItemTagData.For(basePrototype) ?? Array.Empty<Tag>();
        var result = new List<Tag>(tags.Length + 1);
        bool hasCooked = false;
        for (int i = 0; i < tags.Length; i++)
        {
            if (tags[i].Id == RawTag)
            {
                continue;
            }
            if (tags[i].Id == CookedTag)
            {
                hasCooked = true;
            }
            result.Add(tags[i]);
        }
        if (!hasCooked)
        {
            result.Add(new Tag { Id = CookedTag, Level = 1 });
        }
        return result.ToArray();
    }

    /// <summary>ของชิ้นนี้ยังดิบอยู่ไหม — ดูจาก tag ที่ติดมากับไอเทมจริง ไม่ใช่ตาราง prototype</summary>
    public static bool IsRaw(in Item item)
    {
        Tag[] tags = item.Tags;
        if (tags == null)
        {
            return ItemTagData.LevelOf(item.Prototype, RawTag) > 0;
        }
        for (int i = 0; i < tags.Length; i++)
        {
            if (tags[i].Id == RawTag)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>ไอคอนของที่แปรรูปแล้ว — ใช้ไอคอนของสูตร (ย่าง/ต้ม/ตาก) จะได้เห็นว่าไม่ใช่ของดิบ</summary>
    public static string ProcessedIcon(string recipeId, string basePrototype)
    {
        if (RecipeData.RecipeInfo.TryGetValue(recipeId ?? string.Empty, out var info) && !string.IsNullOrEmpty(info.icon))
        {
            return info.icon;
        }
        return ItemNameData.IconOf(basePrototype, string.Empty);
    }

    // ── สูตรแปรรูปที่ "เปลี่ยนรูปทรง" ────────────────────────────────────────
    //
    // 🐛 [4 ก.ย. 2026] บั๊กที่เจอตอนไล่เคส FLOKi: คราฟต์ธนูไม่ได้ทั้งเซิร์ฟ
    //    `bow_wooden_assembled` ต้องใช้ของ tag `string_long` 2 ชิ้น
    //    ทั้งเกมมีของติด string_long แค่ 5 ชนิด ที่หาได้จริงมีตัวเดียวคือ `rope_long`
    //    ซึ่งได้จากสูตร `extend_rope` ("줄 잇기" — ต่อเชือกให้ยาว)
    //    แต่ BuildProcessedOutput เดิมเขียนไว้สำหรับ "ทำอาหาร" อย่างเดียว
    //    (ตัด raw_food เติม taste_good) ⇒ ต่อเชือกแล้วยังได้ `rope` tag เดิม
    //    ⇒ string_long หาไม่ได้เลย ⇒ ธนู/สูตรที่ใช้เชือกยาว คราฟต์ไม่ได้ทั้งเซิร์ฟ
    //
    // สูตรพวกนี้ในข้อมูลเกม **ไม่ได้ระบุ prototype ผลลัพธ์** (type=1 ทั้ง 73 อันไม่มี prototype_id)
    // เกมใช้วิธีเปลี่ยน tag รูปทรง (tags.json group `shape`) เราจึงทำแบบเดียวกัน
    // แล้ว "หา prototype ที่ tag ตรงกับผลลัพธ์" จากตารางจริง — ไม่ได้ hardcode ชื่อของ
    //   rope {burnable,rope,string_normal} → {burnable,rope,string_long} = rope_long ✔

    /// <summary>สูตร → (tag รูปทรงเดิมที่ต้องถอด, tag รูปทรงใหม่)</summary>
    private static readonly Dictionary<string, (string[] Remove, string Add)> ShapeChanges =
        new Dictionary<string, (string[], string)>(StringComparer.Ordinal)
        {
            // ชื่อ/คำอธิบายในข้อมูลเกมบอกตรง ๆ ว่าทำให้ "ยาวขึ้น/กว้างขึ้น"
            // และช่องวัตถุดิบรับเฉพาะทรงสั้น/ปกติ (ไม่รับทรงเป้าหมาย) จึงไม่กำกวม
            { "extend_rope",      (new[] { "string_short", "string_normal" }, "string_long") },
            { "extend_stick",     (new[] { "stick_short",  "stick_normal"  }, "stick_long")  },
            { "s02_extend_stick", (new[] { "stick_short",  "stick_normal"  }, "stick_long")  },
            { "extend_sheet",     (new[] { "sheet_narrow", "sheet_normal"  }, "sheet_wide")  },
        };

    /// <summary>สูตรนี้เปลี่ยนรูปทรงไหม</summary>
    public static bool IsShapeChange(string recipeId)
        => !string.IsNullOrEmpty(recipeId) && ShapeChanges.ContainsKey(recipeId);

    /// <summary>tag ของของหลังเปลี่ยนรูปทรง (null = สูตรนี้ไม่ใช่สูตรเปลี่ยนรูปทรง)</summary>
    public static Tag[] ShapeChangedTags(string recipeId, string basePrototype)
    {
        if (!ShapeChanges.TryGetValue(recipeId ?? string.Empty, out var change)) { return null; }
        Tag[] tags = ItemTagData.For(basePrototype) ?? Array.Empty<Tag>();
        var result = new List<Tag>(tags.Length + 1);
        bool hasNew = false;
        for (int i = 0; i < tags.Length; i++)
        {
            if (Array.IndexOf(change.Remove, tags[i].Id) >= 0) { continue; }
            if (tags[i].Id == change.Add) { hasNew = true; }
            result.Add(tags[i]);
        }
        if (!hasNew) { result.Add(new Tag { Id = change.Add, Level = 1 }); }
        return result.ToArray();
    }

    /// <summary>
    /// หา prototype จริงที่มี tag ตรงกับผลลัพธ์เป๊ะ ๆ (เช่น rope → rope_long)
    /// ไม่เจอ = null ⇒ ผู้เรียกใช้ prototype เดิมแล้วแก้แค่ tag
    /// </summary>
    public static string ResolveShapeChangedPrototype(string recipeId, string basePrototype)
    {
        Tag[] want = ShapeChangedTags(recipeId, basePrototype);
        if (want == null || want.Length == 0) { return null; }
        var wantSet = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < want.Length; i++) { wantSet.Add(want[i].Id); }
        foreach (KeyValuePair<string, Tag[]> kv in ItemTagData.Map)
        {
            if (kv.Key == basePrototype || kv.Value == null || kv.Value.Length != wantSet.Count) { continue; }
            bool same = true;
            for (int i = 0; i < kv.Value.Length && same; i++)
            {
                same = wantSet.Contains(kv.Value[i].Id);
            }
            if (same) { return kv.Key; }
        }
        return null;
    }

    /// <summary>ชื่อของที่แปรรูปแล้ว — "ชื่อสูตร ชื่อของ" เช่น "꼬치구이 고기"</summary>
    public static string ProcessedName(string recipeId, string basePrototype, string baseName)
    {
        string recipeName = recipeId;
        if (RecipeData.RecipeInfo.TryGetValue(recipeId ?? string.Empty, out var info) && !string.IsNullOrEmpty(info.name))
        {
            recipeName = info.name.Trim();
        }
        string itemName = ItemNameData.NameOf(basePrototype, baseName);
        return string.IsNullOrEmpty(itemName) ? recipeName : $"{recipeName} {itemName}";
    }
}
