using System.Collections.Generic;

namespace DurangoServer.Core;

// ข้อมูลธรรมชาติของเกม (generated จาก resources.assets TextAsset `natural` + `emotions` + `prototype_data`)
// - Map: natural type (11002-15001) → ไอเทมที่จะได้ (prototype/ชื่อ/ไอคอนจริงของเกม)
// - EmoticonIds/MotionIds: รายชื่ออิโมจิและท่าทางทั้งหมด (ตอบ GetAvailableEmotions)
public static class NaturalData
{
    public struct GenEntry
    {
        public string Prototype;
        public string Name;
        public string Icon;
    }

    public static readonly string[] EmoticonIds = new[]
    {
        "smile", "wink", "surprise", "love", "sad", "angry", "up", "down", "left", "right",
        "left_up", "right_up", "left_down", "right_down", "ok", "no", "sos", "danger",
        "peace", "go_away", "heart", "broken_heart", "dislike", "like"
    };

    public static readonly string[] MotionIds = new[]
    {
        "heart", "fashion", "sit3", "santa", "fright", "cheerup", "thumbup", "flatter", "dance_sway", "dance6",
        "sad", "school3", "ceremony_e", "anger", "school2", "welcome_b", "fear", "dance_pray", "school4", "ginyu4",
        "ginyu5", "ginyu2", "ginyu3", "no", "spring_picnic", "cheerup_d", "dino", "dance11", "dance10", "disgust",
        "dance12", "heart2", "cheerup2", "cheerup3", "power3", "thumbdown", "ginyu", "spray_money", "power2", "welcome",
        "fashion2", "power", "joy", "spoiled_child", "fashion3", "dance3", "ttaemiri", "witch", "dance_tribe", "dance7",
        "thumbup2", "dance9", "dance8", "sit2", "ceremony_d", "ceremony_c", "ceremony_b", "ceremony_a", "dance_flap",
        "ok", "dance2", "boring", "school1", "sit", "bow", "Alien", "despair", "afterschool", "clap", "dance",
        "headache", "dance5", "dance4"
    };

    public static readonly Dictionary<int, GenEntry[]> Map = new Dictionary<int, GenEntry[]>
    {
        { 11002, new[] { new GenEntry { Prototype = "stem", Name = "ลำต้น", Icon = "icon_nat_fiber_reed" } } },
        { 11004, new[] { new GenEntry { Prototype = "fruit_berry", Name = "ผลเบอร์รี", Icon = "tag_material_fruit" }, new GenEntry { Prototype = "wood_bush", Name = "พุ่มไม้", Icon = "icon_nat_fiber_straw" } } },
        { 11009, new[] { new GenEntry { Prototype = "fruit_berry", Name = "ผลเบอร์รี", Icon = "tag_material_fruit" }, new GenEntry { Prototype = "wood_bush", Name = "พุ่มไม้", Icon = "icon_nat_fiber_straw" } } },
        { 11013, new[] { new GenEntry { Prototype = "fruit_berry", Name = "ผลเบอร์รี", Icon = "tag_material_fruit" }, new GenEntry { Prototype = "wood_bush", Name = "พุ่มไม้", Icon = "icon_nat_fiber_straw" } } },
        { 11018, new[] { new GenEntry { Prototype = "wildberry", Name = "ผลเบอร์รีป่า", Icon = "icon_nat_fruit_wildberry" }, new GenEntry { Prototype = "wood_bush", Name = "พุ่มไม้", Icon = "icon_nat_fiber_straw" } } },
        { 11021, new[] { new GenEntry { Prototype = "flax", Name = "ต้นแฟลกซ์", Icon = "icon_nat_fiber_flax" } } },
        { 11026, new[] { new GenEntry { Prototype = "wood_log", Name = "ท่อนไม้", Icon = "icon_nat_wood_log" } } },
        { 11031, new[] { new GenEntry { Prototype = "fruit_berry", Name = "ผลเบอร์รี", Icon = "tag_material_fruit" }, new GenEntry { Prototype = "wood_bush", Name = "พุ่มไม้", Icon = "icon_nat_fiber_straw" } } },
        { 11032, new[] { new GenEntry { Prototype = "stem", Name = "ลำต้น", Icon = "icon_nat_fiber_reed" } } },
        { 12000, new[] { new GenEntry { Prototype = "stone", Name = "หิน", Icon = "icon_nat_mine_stone" }, new GenEntry { Prototype = "stone_big", Name = "หินก้อนใหญ่", Icon = "icon_nat_mine_rock" } } },
        { 12003, new[] { new GenEntry { Prototype = "stone", Name = "หิน", Icon = "icon_nat_mine_stone" }, new GenEntry { Prototype = "stone_big", Name = "หินก้อนใหญ่", Icon = "icon_nat_mine_rock" } } },
        { 12118, new[] { new GenEntry { Prototype = "fish", Name = "ปลา", Icon = "icon_nat_fish" } } },
        { 12119, new[] { new GenEntry { Prototype = "fish", Name = "ปลา", Icon = "icon_nat_fish" } } },
        { 12120, new[] { new GenEntry { Prototype = "fish", Name = "ปลา", Icon = "icon_nat_fish" } } },
        { 12121, new[] { new GenEntry { Prototype = "clam_shell", Name = "เปลือกหอย", Icon = "icon_nat_clam" } } },
        { 12124, new[] { new GenEntry { Prototype = "stone", Name = "หิน", Icon = "icon_nat_mine_stone" }, new GenEntry { Prototype = "stone_big", Name = "หินก้อนใหญ่", Icon = "icon_nat_mine_rock" } } },
        { 13000, new[] { new GenEntry { Prototype = "stone", Name = "หิน", Icon = "icon_nat_mine_stone" }, new GenEntry { Prototype = "stone_big", Name = "หินก้อนใหญ่", Icon = "icon_nat_mine_rock" } } },
        { 13006, new[] { new GenEntry { Prototype = "clay", Name = "ดินเหนียว", Icon = "icon_nat_clay" } } },
        { 13014, new[] { new GenEntry { Prototype = "rubber", Name = "ยาง", Icon = "material_rubber" }, new GenEntry { Prototype = "metal_brass", Name = "ทองเหลือง", Icon = "icon_nat_mine_zinc" } } },
        { 13044, new[] { new GenEntry { Prototype = "stone", Name = "หิน", Icon = "icon_nat_mine_stone" }, new GenEntry { Prototype = "stone_big", Name = "หินก้อนใหญ่", Icon = "icon_nat_mine_rock" } } },
        { 13045, new[] { new GenEntry { Prototype = "stone", Name = "หิน", Icon = "icon_nat_mine_stone" }, new GenEntry { Prototype = "stone_big", Name = "หินก้อนใหญ่", Icon = "icon_nat_mine_rock" } } },
        { 13046, new[] { new GenEntry { Prototype = "stone", Name = "หิน", Icon = "icon_nat_mine_stone" }, new GenEntry { Prototype = "stone_big", Name = "หินก้อนใหญ่", Icon = "icon_nat_mine_rock" } } },
        { 13047, new[] { new GenEntry { Prototype = "stone", Name = "หิน", Icon = "icon_nat_mine_stone" }, new GenEntry { Prototype = "stone_big", Name = "หินก้อนใหญ่", Icon = "icon_nat_mine_rock" } } },
        { 14004, new[] { new GenEntry { Prototype = "wood_bough", Name = "กิ่งไม้", Icon = "icon_nat_wood_branch" }, new GenEntry { Prototype = "wood_log", Name = "ท่อนไม้", Icon = "icon_nat_wood_log" } } },
        { 14005, new[] { new GenEntry { Prototype = "wood_bough", Name = "กิ่งไม้", Icon = "icon_nat_wood_branch" }, new GenEntry { Prototype = "wood_log", Name = "ท่อนไม้", Icon = "icon_nat_wood_log" } } },
        { 14014, new[] { new GenEntry { Prototype = "wood_bough", Name = "กิ่งไม้", Icon = "icon_nat_wood_branch" }, new GenEntry { Prototype = "wood_log", Name = "ท่อนไม้", Icon = "icon_nat_wood_log" } } },
        { 14017, new[] { new GenEntry { Prototype = "wood_bough", Name = "กิ่งไม้", Icon = "icon_nat_wood_branch" }, new GenEntry { Prototype = "wood_log", Name = "ท่อนไม้", Icon = "icon_nat_wood_log" } } },
        { 14029, new[] { new GenEntry { Prototype = "wood_bough", Name = "กิ่งไม้", Icon = "icon_nat_wood_branch" }, new GenEntry { Prototype = "wood_log", Name = "ท่อนไม้", Icon = "icon_nat_wood_log" } } },
        { 15001, new[] { new GenEntry { Prototype = "stone", Name = "หิน", Icon = "icon_nat_mine_stone" }, new GenEntry { Prototype = "stone_big", Name = "หินก้อนใหญ่", Icon = "icon_nat_mine_rock" } } },
    };
}