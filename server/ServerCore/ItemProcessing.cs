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
