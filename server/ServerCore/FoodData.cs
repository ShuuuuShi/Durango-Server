using System;
using System.Collections.Generic;

namespace DurangoServer.Core;

/// <summary>
/// ข้อมูลอาหารของเกม — **สร้างอัตโนมัติ อย่าแก้ด้วยมือ**
/// (`python scripts/extract_food.py "../game/DurangoV2_Data/resources.strings.txt" ServerCore/FoodData.cs`)
///
/// มาจาก TextAsset `performance` หัวข้อ <c>food</c> — ของกินได้ 352 ชนิด
/// แทนที่การเดาจากชื่อ prototype แบบเดิม (มีคำว่า meat/fruit/egg = กินได้ +30 สตามินาเท่ากันหมด)
///
/// ค่าที่ขึ้นกับเลเวลของไอเทมเก็บเป็น (ค่าที่เลเวล 1, ค่าที่เพิ่มต่อเลเวล) เพราะสูตรในข้อมูลเกม
/// เป็นเส้นตรงทั้งหมด เช่น "18 + 0.50 * (level -1)"
///
/// ⚠️ ตัวเลขเป็น **สเกลของเกมต้นฉบับ** (ความล้า -150, สตามินา 18-40)
/// ส่วน server เราใช้หลอด 0-100 ⇒ ต้องคูณตัวคูณใน <c>data/config.json</c> หัวข้อ Food ก่อนใช้จริง
/// ดู ServerPlayer.Items.HandleUseItem
/// </summary>
public static class FoodData
{
    /// <summary>ข้อมูลอาหารช่วงเลเวลหนึ่ง (ของบางอย่างมีค่าต่างกันระหว่างเลเวล 1-14 กับ 15-70)</summary>
    public sealed class Entry
    {
        public readonly int MinLevel;
        public readonly int MaxLevel;
        /// <summary>สตามินาที่ได้ที่เลเวล 1 · และที่เพิ่มต่อเลเวล</summary>
        public readonly float EnergyBase;
        public readonly float EnergyPerLevel;
        /// <summary>เลือดที่ได้ (health = ฟื้นทันที)</summary>
        public readonly float HealthBase;
        public readonly float HealthPerLevel;
        /// <summary>เลือดสูงสุดที่เพิ่ม (life) — ในข้อมูลเกมส่วนใหญ่เป็น 0</summary>
        public readonly float LifeBase;
        public readonly float LifePerLevel;
        /// <summary>ความอิ่ม — ยังไม่มีหลอดนี้ใน server (เก็บไว้ให้ระบบความหิว)</summary>
        public readonly float Satiety;
        /// <summary>ความล้าที่เปลี่ยน — **ติดลบ = ลดความล้า**</summary>
        public readonly float Fatigue;
        /// <summary>ดับกระหายไหม (0/1)</summary>
        public readonly float Water;
        /// <summary>กินแล้วต้องรอกี่วินาทีถึงกินชิ้นถัดไปได้</summary>
        public readonly int DigestiveTime;
        /// <summary>ท่าที่ client เล่นตอนกิน (Eat / Drink)</summary>
        public readonly string EatMotion;
        /// <summary>บัฟที่ติดหลังกิน (ยังไม่ได้ใช้ — รอระบบ status effect)</summary>
        public readonly string EffectOn;
        public readonly float EffectSeconds;

        public Entry(int minLevel, int maxLevel, float energyBase, float energyPerLevel,
            float healthBase, float healthPerLevel, float lifeBase, float lifePerLevel,
            float satiety, float fatigue, float water, int digestiveTime,
            string eatMotion, string effectOn, float effectSeconds)
        {
            MinLevel = minLevel; MaxLevel = maxLevel;
            EnergyBase = energyBase; EnergyPerLevel = energyPerLevel;
            HealthBase = healthBase; HealthPerLevel = healthPerLevel;
            LifeBase = lifeBase; LifePerLevel = lifePerLevel;
            Satiety = satiety; Fatigue = fatigue; Water = water;
            DigestiveTime = digestiveTime;
            EatMotion = eatMotion; EffectOn = effectOn; EffectSeconds = effectSeconds;
        }

        /// <summary>สตามินาที่ไอเทมเลเวลนี้ให้</summary>
        public float EnergyAt(int level) => EnergyBase + EnergyPerLevel * (level - 1);

        /// <summary>เลือดที่ไอเทมเลเวลนี้ให้</summary>
        public float HealthAt(int level) => HealthBase + HealthPerLevel * (level - 1);

        public float LifeAt(int level) => LifeBase + LifePerLevel * (level - 1);
    }

    private static Entry E(int minLevel, int maxLevel, float energyBase, float energyPerLevel,
        float healthBase, float healthPerLevel, float lifeBase, float lifePerLevel,
        float satiety, float fatigue, float water, int digestiveTime,
        string eatMotion, string effectOn, float effectSeconds)
        => new Entry(minLevel, maxLevel, energyBase, energyPerLevel, healthBase, healthPerLevel,
            lifeBase, lifePerLevel, satiety, fatigue, water, digestiveTime, eatMotion, effectOn, effectSeconds);

    /// <summary>prototype -> ข้อมูลอาหารแยกตามช่วงเลเวล</summary>
    public static readonly Dictionary<string, Entry[]> Map = new Dictionary<string, Entry[]>(StringComparer.Ordinal)
    {
        { "antidote_heatpoison", new[] { E(1,70,10f,0.5f,0f,0f,0f,0f,5f,0f,1f,3,"Drink",null,300f) } },
        { "aphids", new[] { E(1,70,50f,1.2f,0f,0f,0f,0f,5f,0f,0f,3,"Eat","eat_bizarre_food",300f) } },
        { "apple", new[] { E(1,60,200f,0f,150f,0f,0f,0f,10f,-200f,10f,5,"Eat",null,300f) } },
        { "bacchus", new[] { E(1,70,20f,0f,10f,0f,0f,0f,30f,-16.3f,0f,5,"Drink",null,300f) } },
        { "bacon", new[] { E(1,70,20f,1f,0f,0f,0f,0f,25f,0f,0f,10,"Eat",null,300f) } },
        { "barley", new[] { E(1,70,43f,1f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,300f) } },
        { "barley_seed", new[] { E(1,70,43f,1f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "bean", new[] { E(1,70,43f,1f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,300f) } },
        { "bean_seed", new[] { E(1,70,43f,1f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "beer", new[] { E(1,70,30f,0.25f,0f,0f,0f,0f,5f,-30f,0f,3,"Drink",null,600f) } },
        { "beer_craftingRewards", new[] { E(1,70,0f,0f,0f,0f,0f,0f,10f,-0.5f,0f,5,"Drink",null,300f) } },
        { "bellpepper", new[] { E(1,70,20f,0.5f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,300f) } },
        { "belly_steak", new[] { E(1,60,30f,1f,0f,0f,0f,0f,50f,0f,0f,5,"Eat","hot_food",900f) } },
        { "bluebull", new[] { E(1,70,30f,0f,10f,0f,0f,0f,30f,-20.4f,0f,5,"Drink",null,300f) } },
        { "boiled_meat", new[] { E(1,70,80f,1.76f,0f,0f,0f,0f,10f,0f,0f,3,"Eat","hot_food",1200f) } },
        { "bread_01", new[] { E(1,70,80f,2f,0f,0f,0f,0f,15f,0f,0f,3,"Eat",null,1200f) } },
        { "bread_02", new[] { E(1,70,80f,2.5f,0f,0f,0f,0f,15f,0f,0f,3,"Eat",null,1200f) } },
        { "bread_02_fish", new[] { E(1,70,95f,2.5f,0f,0f,0f,0f,15f,0f,0f,3,"Eat",null,1200f) } },
        { "bread_02_fruit", new[] { E(1,70,80f,2.5f,0f,0f,0f,0f,15f,0f,0f,3,"Eat",null,1200f) } },
        { "bread_02_meat", new[] { E(1,70,100f,2.5f,0f,0f,0f,0f,15f,0f,0f,3,"Eat",null,1200f) } },
        { "bread_02_vege", new[] { E(1,70,85f,2.5f,0f,0f,0f,0f,15f,0f,0f,3,"Eat",null,1200f) } },
        { "bread_03_pizza", new[] { E(1,70,60f,3f,0f,0f,0f,0f,30f,0f,0f,3,"Eat",null,1200f) } },
        { "bread_03_pizza_fish", new[] { E(1,70,70f,3f,0f,0f,0f,0f,30f,0f,0f,3,"Eat",null,1200f) } },
        { "bread_03_pizza_fruit", new[] { E(1,70,60f,3f,0f,0f,0f,0f,30f,0f,0f,3,"Eat",null,1200f) } },
        { "bread_03_pizza_meat", new[] { E(1,70,75f,3f,0f,0f,0f,0f,30f,0f,0f,3,"Eat",null,1200f) } },
        { "bread_03_pizza_vege", new[] { E(1,70,65f,3f,0f,0f,0f,0f,30f,0f,0f,3,"Eat",null,1200f) } },
        { "bread_compi_apology_cs", new[] { E(1,70,80f,2f,0f,0f,0f,0f,20f,0f,0f,10,"Eat",null,1200f) } },
        { "bread_compi_sad_cs", new[] { E(1,70,80f,2f,0f,0f,0f,0f,20f,0f,0f,10,"Eat",null,1200f) } },
        { "bread_compi_thank_cs", new[] { E(1,70,80f,2f,0f,0f,0f,0f,20f,-500f,0f,10,"Eat",null,1200f) } },
        { "bread_picnic", new[] { E(1,70,80f,2f,0f,0f,0f,0f,15f,0f,0f,3,"Eat",null,1200f) } },
        { "brisket_steak", new[] { E(1,60,30f,1f,0f,0f,0f,0f,50f,0f,0f,5,"Eat","hot_food",900f) } },
        { "broth", new[] { E(1,70,14f,0.5f,0f,0f,0f,0f,5f,0f,0f,3,"Drink","drink_water",600f) } },
        { "broth_bone", new[] { E(1,70,16f,0.5f,0f,0f,0f,0f,5f,0f,0f,3,"Drink","drink_water",600f) } },
        { "broth_fish", new[] { E(1,70,16f,0.5f,0f,0f,0f,0f,5f,0f,0f,3,"Drink","drink_water",600f) } },
        { "broth_meat", new[] { E(1,70,18f,0.5f,0f,0f,0f,0f,5f,0f,0f,3,"Drink","drink_water",600f) } },
        { "broth_vege", new[] { E(1,70,14f,0.5f,0f,0f,0f,0f,5f,0f,0f,3,"Drink","drink_water",600f) } },
        { "bulb", new[] { E(1,70,20f,0.5f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,300f) } },
        { "bulb_violettulip", new[] { E(1,70,20f,0.5f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,300f) } },
        { "burgerking_monsterburger_event", new[] { E(1,70,0f,0f,0f,0f,0f,0f,20f,-100f,0f,10,"Eat",null,1200f) } },
        { "butter", new[] { E(1,70,60f,1.7f,0f,0f,0f,0f,5f,0f,0f,3,"Eat",null,600f) } },
        { "cactus", new[] { E(1,70,20f,0.3f,0f,0f,0f,0f,15f,0f,0f,7,"Eat",null,300f) } },
        { "cake_01", new[] { E(1,70,90f,2.3f,0f,0f,0f,0f,20f,0f,0f,3,"Eat","energetic",1200f) } },
        { "cake_02", new[] { E(1,70,65f,3f,0f,0f,0f,0f,30f,0f,0f,3,"Eat","energetic",1200f) } },
        { "cake_carnation_01", new[] { E(1,70,65f,3f,0f,1.5f,0f,1.5f,15f,-50f,0f,5,"Eat","energetic",1800f) } },
        { "cake_carnation_02", new[] { E(1,70,65f,3f,0f,2f,0f,2f,15f,-70f,0f,5,"Eat","energetic",1800f) } },
        { "cake_carnation_03", new[] { E(1,70,40f,2f,0f,0f,0f,1.5f,15f,-20f,0f,5,"Eat","energetic",1800f) } },
        { "carnation_seed", new[] { E(1,70,33f,1f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "cattail_flower", new[] { E(1,70,20f,0.15f,10f,0.5f,0f,0f,10f,0f,0f,5,"Eat","life_up",300f) } },
        { "cheese_01", new[] { E(1,70,60f,1.7f,0f,0f,0f,0f,15f,0f,0f,3,"Eat",null,600f) } },
        { "cheese_02", new[] { E(1,70,80f,2f,0f,0f,0f,0f,15f,0f,0f,3,"Eat",null,1200f) } },
        { "cherryblossom_01_flower", new[] { E(1,70,20f,0.15f,15f,0.7f,0f,0f,5f,0f,0f,5,"Eat","life_up",300f) } },
        { "cherryblossom_01_root", new[] { E(1,70,20f,0.15f,10f,0.5f,0f,0f,15f,0f,0f,5,"Eat","life_up",300f) } },
        { "cherryblossom_01_seed", new[] { E(1,70,20f,0.5f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,300f) } },
        { "cherryblossom_02_flower", new[] { E(1,70,20f,0.15f,15f,0.7f,0f,0f,5f,0f,0f,5,"Eat","life_up",300f) } },
        { "cherryblossom_02_root", new[] { E(1,70,20f,0.15f,10f,0.5f,0f,0f,15f,0f,0f,5,"Eat","life_up",300f) } },
        { "cherryblossom_2019_latte", new[] { E(1,70,55f,2.5f,0f,0f,0f,0f,7f,0f,1f,5,"Drink","drink_water",300f) } },
        { "cherryblossom_2019_popcorn", new[] { E(1,70,20f,2.5f,15f,0.5f,0f,0f,6f,0f,0f,7,"Eat",null,299f) } },
        { "cherryblossom_2019_snack", new[] { E(1,70,20f,2.5f,0f,0f,0f,0f,5f,0f,0f,6,"Eat",null,298f) } },
        { "chicken_01", new[] { E(1,70,70f,2.2f,0f,0f,0f,0f,20f,0f,0f,3,"Eat",null,1200f) } },
        { "chicken_02", new[] { E(1,70,65f,3.2f,0f,0f,0f,0f,30f,0f,0f,3,"Eat",null,1200f) } },
        { "chicken_02_cream_sauce", new[] { E(1,70,65f,3.2f,0f,0f,0f,0f,30f,0f,0f,3,"Eat",null,1200f) } },
        { "chicken_02_fruit_sauce", new[] { E(1,70,65f,3.2f,0f,0f,0f,0f,30f,0f,0f,3,"Eat",null,1200f) } },
        { "chicken_02_oil_sauce", new[] { E(1,70,65f,3.2f,0f,0f,0f,0f,30f,0f,0f,3,"Eat",null,1200f) } },
        { "chicken_02_vege_sauce", new[] { E(1,70,65f,3.2f,0f,0f,0f,0f,30f,0f,0f,3,"Eat",null,1200f) } },
        { "chilipepper", new[] { E(1,70,20f,0.5f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,300f) } },
        { "chrismas_onehand_beer_mug", new[] { E(1,70,80f,2.5f,0f,0f,0f,0f,15f,0f,5f,5,"Drink",null,1800f) } },
        { "chrismas_onehand_lollipop", new[] { E(1,70,80f,2.5f,0f,0f,0f,0f,15f,0f,0f,5,"Eat",null,1800f) } },
        { "clam", new[] { E(1,14,40f,0.9f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f), E(15,70,40f,0.9f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "clam_product", new[] { E(1,14,40f,0.9f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f), E(15,70,40f,0.9f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "cocktail", new[] { E(1,70,50f,1f,0f,0f,0f,0f,5f,0f,0f,3,"Drink","drunk",300f) } },
        { "coconut", new[] { E(1,14,23f,0.75f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,300f), E(15,70,23f,0.75f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,300f) } },
        { "coffee", new[] { E(1,70,10f,0.3f,0f,0f,0f,0f,5f,0f,0f,3,"Eat",null,300f) } },
        { "coffee_drip", new[] { E(1,70,15f,0.7f,0f,0f,0f,0f,5f,0f,0f,3,"Drink","effect_coffee_drip",1200f) } },
        { "coffee_dutch", new[] { E(1,70,15f,0.7f,0f,0f,0f,0f,5f,0f,0f,3,"Drink","effect_coffee_dutch",1200f) } },
        { "coffee_lessbe", new[] { E(1,70,15f,0f,10f,0f,0f,0f,30f,-12.2f,0f,5,"Drink",null,300f) } },
        { "coffee_seed", new[] { E(1,70,15f,0.3f,0f,0f,0f,0f,5f,0f,0f,3,"Eat",null,300f) } },
        { "corn_infertility", new[] { E(1,14,30f,1f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f), E(15,70,30f,1f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "corn_salad", new[] { E(1,70,40f,2.5f,0f,0f,0f,0f,5f,0f,1f,3,"Eat",null,300f) } },
        { "corn_seed", new[] { E(1,70,33f,1f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "cosmos_pizza", new[] { E(1,70,60f,1.7f,0f,0f,0f,0f,15f,0f,0f,3,"Eat",null,1200f) } },
        { "cosmos_seed", new[] { E(1,70,20f,0.5f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,300f) } },
        { "crab_coconut", new[] { E(1,14,40f,0.9f,0f,0f,0f,0f,15f,0f,0f,7,"Eat",null,1200f), E(15,70,40f,0.9f,0f,0f,0f,0f,15f,0f,0f,7,"Eat",null,1200f) } },
        { "cream", new[] { E(1,70,80f,2.3f,0f,0f,0f,0f,5f,0f,0f,3,"Eat",null,600f) } },
        { "critical_item_plant_01", new[] { E(1,70,20f,0.15f,10f,0.5f,0f,0f,15f,0f,0f,5,"Eat","life_up",600f) } },
        { "crunky_event", new[] { E(1,70,0f,0f,110f,10f,111f,11f,5f,0f,0f,0,"Eat",null,1200f) } },
        { "desertagave_flesh", new[] { E(1,70,20f,0.5f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,300f) } },
        { "dough_bread", new[] { E(1,70,20f,0.5f,0f,0f,0f,0f,30f,0f,0f,3,"Eat",null,300f) } },
        { "dough_cake", new[] { E(1,70,20f,0.5f,0f,0f,0f,0f,30f,0f,0f,3,"Eat",null,300f) } },
        { "dough_noodle", new[] { E(1,70,20f,0.5f,0f,0f,0f,0f,30f,0f,0f,3,"Eat",null,300f) } },
        { "dough_pie", new[] { E(1,70,20f,0.5f,0f,0f,0f,0f,30f,0f,0f,3,"Eat",null,300f) } },
        { "dough_songpyeon", new[] { E(1,70,20f,0.5f,0f,0f,0f,0f,30f,0f,0f,3,"Eat",null,300f) } },
        { "dried_persimmon", new[] { E(1,70,90f,2.3f,0f,0f,0f,0f,20f,0f,0f,3,"Eat","energetic",1800f) } },
        { "dried_persimmon_craftingRewards", new[] { E(1,70,0f,0f,0f,0f,0f,0f,20f,0f,0f,5,"Eat",null,300f) } },
        { "egg", new[] { E(1,14,40f,0.9f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f), E(15,70,40f,0.9f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "event_bread_01", new[] { E(1,70,0f,0f,0f,0f,0f,0f,10f,-50f,0f,5,"Eat",null,1800f) } },
        { "event_compy_cookies", new[] { E(1,70,0f,0f,0f,0f,0f,0f,0f,-80f,0f,5,"Eat",null,1200f) } },
        { "event_compy_macchiato", new[] { E(1,70,200f,0f,0f,0f,0f,0f,0f,-80f,1f,5,"Drink","drink_water",1200f) } },
        { "event_compy_maximcoffee", new[] { E(1,70,80f,0f,150f,0f,0f,0f,0f,-80f,1f,5,"Drink","drink_water",1200f) } },
        { "event_compy_vitamin_B", new[] { E(1,70,200f,0f,0f,0f,0f,0f,0f,-80f,0f,5,"Eat","energetic",1200f) } },
        { "event_compy_vitamin_C", new[] { E(1,70,80f,0f,150f,0f,0f,0f,0f,-80f,0f,5,"Eat","energetic",1200f) } },
        { "event_compy_vitamin_D", new[] { E(1,70,0f,0f,0f,0f,0f,0f,0f,-80f,0f,5,"Eat","energetic",1200f) } },
        { "event_compy_vitamin_multiple", new[] { E(1,70,0f,0f,0f,0f,0f,0f,0f,-100f,0f,5,"Eat","energetic",1200f) } },
        { "event_fatigue_drug", new[] { E(1,70,0f,0f,0f,0f,0f,0f,3f,-200f,0f,3,"Drink","drink_water",300f) } },
        { "event_fatigue_drug_01", new[] { E(1,70,0f,0f,0f,0f,0f,0f,3f,-200f,0f,3,"Drink","drink_water",300f) } },
        { "event_ramen", new[] { E(1,70,35f,1.2f,0f,0f,0f,0f,10f,0f,0f,5,"Eat","life_up",1800f) } },
        { "event_tiramisu", new[] { E(1,70,65f,3f,0f,0f,0f,0f,10f,0f,0f,5,"Eat","energetic",1800f) } },
        { "event_tiramisu_01", new[] { E(1,70,65f,3f,0f,0f,0f,0f,10f,0f,0f,5,"Eat","energetic",1800f) } },
        { "exp_jelly_01", new[] { E(1,70,0f,0f,0f,0f,0f,0f,5f,0f,0f,5,"Eat",null,300f) } },
        { "exp_jelly_02", new[] { E(1,70,0f,0f,0f,0f,0f,0f,5f,0f,0f,5,"Eat",null,300f) } },
        { "fat", new[] { E(1,70,55f,1f,0f,0f,0f,0f,120f,0f,0f,10,"Eat","eat_bizarre_food",300f) } },
        { "fatigue_drug", new[] { E(1,70,0f,0f,0f,0f,0f,0f,5f,-300f,0f,5,"Drink","drink_water",1200f) } },
        { "fatigue_drug_01_store", new[] { E(1,70,0f,0f,0f,0f,0f,0f,5f,-300f,0f,5,"Drink","drink_water",300f) } },
        { "fatigue_drug_02_store", new[] { E(1,70,0f,0f,0f,0f,0f,0f,5f,-300f,0f,5,"Drink","drink_water",300f) } },
        { "fatigue_drug_compi_cs", new[] { E(1,70,0f,0f,0f,0f,0f,0f,5f,-1000f,0f,5,"Drink","drink_water",1200f) } },
        { "fatigue_drug_event", new[] { E(1,70,0f,0f,0f,0f,0f,0f,5f,-1000f,0f,5,"Drink","drink_water",1200f) } },
        { "fatigue_drug_store", new[] { E(1,70,0f,0f,0f,0f,0f,0f,5f,-1000f,0f,0,"Barehand_Drink_Fast","drink_water",1200f) } },
        { "fatigue_drug_store_01", new[] { E(1,70,0f,0f,0f,0f,0f,0f,5f,-1000f,0f,0,"Barehand_Drink_Fast","drink_water",1200f) } },
        { "feed_carni_01", new[] { E(1,70,75f,1f,0f,0f,0f,0f,20f,150f,0f,10,"Eat","eat_bizarre_food",300f) } },
        { "feed_carni_02", new[] { E(1,70,100f,1.5f,0f,0f,0f,0f,30f,130f,0f,10,"Eat","eat_bizarre_food",300f) } },
        { "feed_herb_01", new[] { E(1,70,75f,1f,0f,0f,0f,0f,20f,150f,0f,10,"Eat","eat_bizarre_food",300f) } },
        { "feed_herb_02", new[] { E(1,70,100f,1.5f,0f,0f,0f,0f,30f,130f,0f,10,"Eat","eat_bizarre_food",300f) } },
        { "fish", new[] { E(1,14,50f,1.3f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f), E(15,70,50f,1.3f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "fish_big", new[] { E(1,70,58f,1.3f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "fish_big_eel", new[] { E(1,70,58f,1.3f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "fish_big_salmon", new[] { E(1,70,58f,1.3f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "fish_tropical", new[] { E(1,14,53f,1.3f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f), E(15,70,53f,1.3f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "fish_tropical_big", new[] { E(1,70,60f,1.3f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "flower", new[] { E(1,70,20f,0.15f,15f,0.7f,0f,0f,5f,0f,0f,5,"Eat","life_up",300f) } },
        { "flower_violettulip", new[] { E(1,70,20f,0.5f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,300f) } },
        { "food_k", new[] { E(1,1,50f,1f,0f,0f,0f,0f,3f,0f,0f,3,"Eat",null,300f) } },
        { "fried_dinosaur_eggs", new[] { E(1,70,80f,1f,20f,0.8f,20f,0.8f,25f,0f,0f,10,"Eat",null,300f) } },
        { "fried_hen", new[] { E(1,70,0f,0f,0f,0f,0f,0f,20f,-100f,0f,10,"Eat",null,300f) } },
        { "fruit", new[] { E(1,14,20f,0.5f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,300f), E(15,70,20f,0.5f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,300f) } },
        { "fruit_berry", new[] { E(1,14,20f,0.5f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,300f), E(15,70,20f,0.5f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,300f) } },
        { "fruit_juicy", new[] { E(1,14,20f,0.75f,0f,0f,0f,0f,10f,0f,0f,3,"Eat","fruit_water",300f), E(15,70,20f,0.75f,0f,0f,0f,0f,10f,0f,0f,3,"Eat","fruit_water",300f) } },
        { "fruit_liquid", new[] { E(1,70,40f,0f,0f,0f,0f,0f,30f,0f,0f,5,"Drink",null,300f) } },
        { "fruit_nut", new[] { E(1,14,25f,1f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,300f), E(15,70,25f,1f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,300f) } },
        { "fruit_sandalwood", new[] { E(1,14,20f,0.5f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,300f), E(15,70,20f,0.5f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,300f) } },
        { "fruit_sandwich", new[] { E(1,70,50f,1.5f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "fruit_sandwich_pvp", new[] { E(1,70,50f,1.5f,0f,0f,0f,0f,10f,0f,0f,5,"Eat","fruit_sandwich_effects",600f) } },
        { "fruit_sandwich_weapon", new[] { E(1,70,50f,1.5f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "fruit_tropical", new[] { E(1,14,23f,0.75f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,300f), E(15,70,23f,0.75f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,300f) } },
        { "glazed_meat", new[] { E(1,70,40f,2.5f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,298f) } },
        { "grape", new[] { E(1,70,20f,0.5f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,300f) } },
        { "halloween_cake", new[] { E(1,70,75f,1.75f,0f,1.2f,0f,1.1f,25f,0f,1f,3,"Eat","energetic",300f) } },
        { "halloween_candy", new[] { E(1,70,10f,0f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,300f) } },
        { "halloween_cookie", new[] { E(1,70,30f,0.5f,0f,0f,0f,0f,5f,-15f,0f,3,"Eat","thirsty",300f) } },
        { "halloween_jelly", new[] { E(1,70,15f,0.5f,0f,0f,0f,0f,1f,0f,0f,2,"Eat",null,300f) } },
        { "halloween_juice", new[] { E(1,70,10f,0.5f,0f,0f,0f,0f,7f,-50f,1f,5,"Drink","drink_water",300f) } },
        { "halloween_pumpkin", new[] { E(1,70,18f,0.5f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,300f) } },
        { "halloween_pumpkin_seed", new[] { E(1,70,10f,0.5f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "halloween_sausage", new[] { E(1,70,60f,2.5f,0f,0f,0f,0f,13f,0f,0f,3,"Eat","life_up",300f) } },
        { "halloween_soup", new[] { E(1,70,70f,2.5f,0f,0f,0f,0f,20f,0f,0f,3,"Eat","hot_food",300f) } },
        { "halloween_spider_skewers", new[] { E(1,70,50f,1.2f,0f,1.5f,0f,1.3f,20f,0f,0f,5,"Eat","hot_food",300f) } },
        { "halloween_spidertree_seed", new[] { E(1,70,15f,0.5f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,300f) } },
        { "hamberger_01", new[] { E(1,70,65f,3.5f,0f,0f,0f,0f,30f,0f,0f,3,"Eat",null,1200f) } },
        { "hambuger", new[] { E(1,1,50f,1f,0f,0f,0f,0f,5f,0f,0f,3,"Eat","stamina_up",300f) } },
        { "hardtack_event", new[] { E(1,70,0f,0f,0f,0f,0f,0f,3f,0f,0f,3,"Eat",null,300f) } },
        { "hardtack_store", new[] { E(1,70,0f,0f,0f,0f,0f,0f,3f,0f,0f,3,"Eat",null,300f) } },
        { "hemostat", new[] { E(1,70,20f,0.15f,0f,0f,0f,0f,5f,0f,0f,5,"Eat",null,300f) } },
        { "honey", new[] { E(1,70,60f,1.5f,0f,0f,0f,0f,5f,0f,0f,3,"Eat",null,300f) } },
        { "hp_medicine_02_store", new[] { E(1,70,0f,0f,110f,10f,111f,11f,5f,0f,0f,0,"Barehand_Drink_Fast",null,300f) } },
        { "hp_medicine_event", new[] { E(1,70,0f,0f,0f,0f,0f,0f,5f,0f,0f,5,"Drink",null,300f) } },
        { "hp_medicine_store", new[] { E(1,70,0f,0f,0f,0f,0f,0f,5f,0f,0f,0,"Barehand_Drink_Fast",null,300f) } },
        { "ice", new[] { E(1,70,20f,0.5f,0f,0f,0f,0f,10f,0f,0f,3,"Eat","cold_food",300f) } },
        { "immune_bleeding_inner", new[] { E(1,70,10f,0.5f,0f,0f,0f,0f,5f,0f,1f,3,"Drink",null,1200f) } },
        { "immune_bleeding_neverstop", new[] { E(1,70,10f,0.5f,0f,0f,0f,0f,5f,0f,1f,3,"Drink",null,1200f) } },
        { "immune_bug", new[] { E(1,70,10f,0.5f,0f,0f,0f,0f,5f,0f,1f,3,"Drink",null,1200f) } },
        { "immune_collect", new[] { E(1,70,10f,0.5f,0f,0f,0f,0f,5f,0f,1f,3,"Drink",null,1200f) } },
        { "immune_gas", new[] { E(1,70,10f,0.5f,0f,0f,0f,0f,5f,0f,1f,3,"Drink",null,1200f) } },
        { "immune_lizard", new[] { E(1,70,10f,0.5f,0f,0f,0f,0f,5f,0f,1f,3,"Drink",null,1200f) } },
        { "immune_tetanus", new[] { E(1,70,10f,0.5f,0f,0f,0f,0f,5f,0f,1f,3,"Drink",null,1200f) } },
        { "jasmine_grilled_fish", new[] { E(1,70,40f,2f,0f,0f,0f,0f,20f,0f,1f,5,"Eat","energetic",1800f) } },
        { "jasmine_seed", new[] { E(1,70,10f,1f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "jasmine_tea", new[] { E(1,70,30f,1f,0f,0f,0f,0f,4f,0f,0f,2,"Drink","effect_jasmine",300f) } },
        { "juice_cactus", new[] { E(1,70,40f,1.5f,0f,0f,0f,0f,7f,0f,1f,5,"Drink","effect_cactus_juice",1200f) } },
        { "juice_fruit", new[] { E(1,70,50f,1.5f,0f,0f,0f,0f,7f,0f,1f,5,"Drink","drink_water",1200f) } },
        { "juice_vege", new[] { E(1,70,45f,1.5f,0f,0f,0f,0f,7f,0f,1f,5,"Drink","drink_water",1200f) } },
        { "kingkong_event", new[] { E(1,70,0f,0f,110f,10f,111f,11f,5f,0f,0f,0,"Eat",null,1200f) } },
        { "lake_spa_water", new[] { E(1,60,5f,0f,0f,0f,0f,0f,5f,0f,1f,3,"Drink","drink_water",300f) } },
        { "leaf_bud", new[] { E(1,70,20f,0.15f,10f,0.6f,0f,0f,10f,0f,0f,5,"Eat","life_up",300f) } },
        { "leaf_herb", new[] { E(1,70,20f,0.15f,10f,0.6f,0f,0f,10f,0f,0f,5,"Eat","life_up",300f) } },
        { "leather_boil", new[] { E(1,70,30f,0.3f,-20f,0f,0f,0f,15f,20f,0f,10,"Eat","eat_bizarre_food",300f) } },
        { "leather_raw_hard_boil", new[] { E(1,70,30f,0.5f,-30f,0f,0f,0f,25f,30f,0f,15,"Eat","eat_bizarre_food",300f) } },
        { "lotus_flower", new[] { E(1,70,20f,0.15f,15f,0.7f,0f,0f,10f,0f,0f,5,"Eat","life_up",300f) } },
        { "lotus_leaf", new[] { E(1,70,20f,0.15f,10f,0.4f,0f,0f,10f,0f,0f,5,"Eat","life_up",300f) } },
        { "lotus_root", new[] { E(1,70,20f,0.15f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "mango_seed", new[] { E(1,60,20f,1f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "marshmallow", new[] { E(1,70,55f,1.5f,0f,0f,0f,0f,15f,0f,0f,5,"Eat",null,300f) } },
        { "meat", new[] { E(1,70,63f,1.7f,0f,0f,0f,0f,20f,0f,0f,5,"Eat",null,300f) } },
        { "meat_belley", new[] { E(1,70,73f,1.7f,0f,0f,0f,0f,40f,0f,0f,7,"Eat",null,1200f) } },
        { "meat_breast", new[] { E(1,70,73f,1.7f,0f,0f,0f,0f,40f,0f,0f,7,"Eat",null,1200f) } },
        { "meat_lizard", new[] { E(1,70,63f,1.7f,0f,0f,0f,0f,20f,0f,0f,5,"Eat",null,300f) } },
        { "meat_serloin", new[] { E(1,70,73f,1.7f,0f,0f,0f,0f,40f,0f,0f,7,"Eat",null,1200f) } },
        { "meat_skewers", new[] { E(1,70,63f,1.7f,0f,0f,0f,0f,12f,0f,0f,5,"Eat",null,600f) } },
        { "meat_steak", new[] { E(1,60,20f,1f,0f,0f,0f,0f,50f,0f,0f,5,"Eat","hot_food",900f) } },
        { "meat_tenderloin", new[] { E(1,60,73f,1.7f,0f,0f,0f,0f,40f,0f,0f,7,"Eat",null,1200f) } },
        { "meat_tutorial", new[] { E(1,10,23f,0.75f,0f,0f,0f,0f,20f,0f,0f,5,"Eat",null,300f) } },
        { "meatball_01", new[] { E(1,70,110f,1.6f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,600f) } },
        { "medicine_albochil", new[] { E(1,70,10f,0f,10f,0.6f,0f,0f,30f,-10f,0f,5,"Eat",null,300f) } },
        { "medicine_modern_01", new[] { E(1,70,20f,0f,40f,0.8f,0f,0f,30f,-10f,0f,5,"Eat",null,300f) } },
        { "medicine_pillet_01", new[] { E(1,70,20f,0f,30f,0.8f,0f,0f,30f,0f,0f,5,"Eat",null,300f) } },
        { "medicine_poison_oil", new[] { E(1,70,20f,0.5f,0f,0f,0f,0f,20f,0f,0f,3,"Eat",null,300f) } },
        { "medicine_poison_oil_02", new[] { E(1,70,20f,0.5f,0f,0f,0f,0f,20f,0f,0f,3,"Eat",null,300f) } },
        { "medicine_tea_01", new[] { E(1,70,10f,0f,15f,0.6f,0f,0f,30f,0f,0f,5,"Drink",null,300f) } },
        { "milk", new[] { E(1,70,20f,1.2f,0f,0f,0f,0f,5f,0f,1f,3,"Drink",null,600f) } },
        { "mojito", new[] { E(1,70,30f,2f,0f,0f,0f,0f,5f,0f,0f,3,"Drink","drink_water",300f) } },
        { "moss", new[] { E(1,70,20f,0.15f,15f,0.7f,0f,0f,10f,0f,0f,5,"Eat","life_up",300f) } },
        { "mushroom", new[] { E(1,70,20f,0.15f,0f,0f,0f,0f,30f,0f,0f,5,"Eat",null,300f) } },
        { "nagitoz_event", new[] { E(1,70,0f,0f,110f,10f,111f,11f,5f,0f,0f,0,"Eat",null,300f) } },
        { "narcotic_drug", new[] { E(1,70,125f,1f,0f,0f,0f,0f,5f,0f,0f,3,"Drink","poisoning",300f) } },
        { "noodle_soup", new[] { E(1,70,70f,2.5f,0f,0f,0f,0f,13f,0f,0f,3,"Eat","hot_food",1200f) } },
        { "noodle_soup_broth_bone", new[] { E(1,70,70f,2.5f,0f,0f,0f,0f,13f,0f,0f,3,"Eat","hot_food",1200f) } },
        { "noodle_soup_broth_fish", new[] { E(1,70,70f,2.5f,0f,0f,0f,0f,13f,0f,0f,3,"Eat","hot_food",1200f) } },
        { "noodle_soup_broth_meat", new[] { E(1,70,70f,2.5f,0f,0f,0f,0f,13f,0f,0f,3,"Eat","hot_food",1200f) } },
        { "noodle_soup_broth_vege", new[] { E(1,70,70f,2.5f,0f,0f,0f,0f,13f,0f,0f,3,"Eat","hot_food",1200f) } },
        { "noodle_stir_01", new[] { E(1,70,70f,2.5f,0f,0f,0f,0f,15f,0f,0f,3,"Eat",null,1200f) } },
        { "noodle_stir_01_cream_sauce", new[] { E(1,70,70f,2.5f,0f,0f,0f,0f,15f,0f,0f,3,"Eat",null,1200f) } },
        { "noodle_stir_01_fruit_sauce", new[] { E(1,70,70f,2.5f,0f,0f,0f,0f,15f,0f,0f,3,"Eat",null,1200f) } },
        { "noodle_stir_01_oil_sauce", new[] { E(1,70,70f,2.5f,0f,0f,0f,0f,15f,0f,0f,3,"Eat",null,1200f) } },
        { "noodle_stir_01_vege_sauce", new[] { E(1,70,70f,2.5f,0f,0f,0f,0f,15f,0f,0f,3,"Eat",null,1200f) } },
        { "noodle_stir_02", new[] { E(1,70,70f,2.8f,0f,0f,0f,0f,20f,0f,0f,3,"Eat",null,1200f) } },
        { "noodle_stir_02_cream_sauce", new[] { E(1,70,70f,2.8f,0f,0f,0f,0f,20f,0f,0f,3,"Eat",null,1200f) } },
        { "noodle_stir_02_fruit_sauce", new[] { E(1,70,70f,2.8f,0f,0f,0f,0f,20f,0f,0f,3,"Eat",null,1200f) } },
        { "noodle_stir_02_oil_sauce", new[] { E(1,70,70f,2.8f,0f,0f,0f,0f,20f,0f,0f,3,"Eat",null,1200f) } },
        { "noodle_stir_02_vege_sauce", new[] { E(1,70,70f,2.8f,0f,0f,0f,0f,20f,0f,0f,3,"Eat",null,1200f) } },
        { "oil_grains", new[] { E(1,70,50f,1.5f,0f,0f,0f,0f,120f,0f,0f,10,"Drink","thirsty",300f) } },
        { "oil_oily_fruit", new[] { E(1,70,55f,1.7f,0f,0f,0f,0f,120f,0f,0f,10,"Drink","thirsty",300f) } },
        { "onion_infertility", new[] { E(1,70,38f,1f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "onion_seed", new[] { E(1,70,38f,1f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "orange_seed", new[] { E(1,60,20f,1f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "organ", new[] { E(1,70,60f,1f,0f,0f,0f,0f,60f,0f,0f,10,"Eat","eat_bizarre_food",300f) } },
        { "oxalis_seed_blue", new[] { E(1,60,20f,1f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "oxalis_seed_red", new[] { E(1,60,20f,1f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "oxalis_seed_yellow", new[] { E(1,60,20f,1f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "painkiller_root", new[] { E(1,70,20f,0f,30f,0.8f,0f,0f,30f,0f,0f,5,"Eat",null,300f) } },
        { "persimmon", new[] { E(1,70,20f,0.5f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,300f) } },
        { "persimmontree_seed", new[] { E(1,70,20f,0.5f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,300f) } },
        { "pinenut", new[] { E(1,14,25f,1f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,300f), E(15,70,25f,1f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,300f) } },
        { "pizza", new[] { E(1,70,100f,2f,0f,0f,0f,0f,5f,0f,0f,3,"Eat",null,300f) } },
        { "poison_sac", new[] { E(1,70,30f,0.5f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,300f) } },
        { "potato_infertility", new[] { E(1,70,33f,1f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "potato_seed", new[] { E(1,70,35f,1f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "powder_barley", new[] { E(1,70,20f,0.5f,0f,0f,0f,0f,60f,0f,0f,10,"Eat","thirsty",300f) } },
        { "powder_coffee", new[] { E(1,70,20f,0.5f,0f,0f,0f,0f,60f,0f,0f,3,"Eat",null,300f) } },
        { "powder_grain", new[] { E(1,70,20f,0.5f,0f,0f,0f,0f,60f,0f,0f,10,"Eat","thirsty",300f) } },
        { "puff_grain", new[] { E(1,70,80f,2f,0f,0f,0f,0f,15f,0f,0f,3,"Eat","thirsty",1200f) } },
        { "pumpkin", new[] { E(1,70,35f,1f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,300f) } },
        { "pumpkin_seed", new[] { E(1,70,38f,1f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "ramen", new[] { E(1,70,35f,1.2f,0f,0f,0f,0f,10f,0f,0f,6,"Eat",null,900f) } },
        { "ration_survival", new[] { E(1,70,50f,1f,0f,0f,0f,0f,3f,0f,0f,3,"Eat",null,300f) } },
        { "rice", new[] { E(1,70,43f,1f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,300f) } },
        { "rice_cake", new[] { E(1,70,60f,1.7f,0f,0f,0f,0f,15f,0f,0f,3,"Eat",null,1200f) } },
        { "rice_cake_anniversary_event", new[] { E(1,70,200f,0f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,1200f) } },
        { "rice_drink_event_newyear_2019", new[] { E(1,70,10f,0.25f,0f,0f,0f,0f,7f,-150f,1f,5,"Drink","drink_water",1800f) } },
        { "rice_seed", new[] { E(1,70,43f,1f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "rice_steamed", new[] { E(1,70,75f,1.7f,0f,0f,0f,0f,15f,0f,0f,3,"Eat",null,1200f) } },
        { "roasted_shrimp", new[] { E(1,70,110f,1.5f,0f,0f,0f,0f,15f,0f,0f,3,"Eat",null,1200f) } },
        { "root", new[] { E(1,70,20f,0.15f,10f,0.5f,0f,0f,15f,0f,0f,5,"Eat","life_up",300f) } },
        { "rose_steak", new[] { E(1,70,15f,2.5f,15f,0.8f,0f,0f,5f,0f,0f,6,"Eat","life_up",298f) } },
        { "s02_bacchus", new[] { E(1,70,20f,0f,10f,0f,0f,0f,30f,-75f,0f,5,"Drink",null,600f) } },
        { "s02_cattail_flower", new[] { E(1,70,133f,0f,46f,0f,46f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "s02_centipede", new[] { E(1,70,181f,0f,0f,0f,0f,0f,20f,0f,0f,3,"Eat",null,600f) } },
        { "s02_fish", new[] { E(1,70,182f,0f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,600f) } },
        { "s02_fish_big", new[] { E(1,70,236f,0f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,1200f) } },
        { "s02_liver", new[] { E(1,70,92f,0f,10f,0f,10f,0f,120f,-150f,2f,3,"Eat",null,600f) } },
        { "s02_lotus_flower", new[] { E(1,70,128f,0f,50f,0f,50f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "s02_lotus_leaf", new[] { E(1,70,128f,0f,42f,0f,42f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "s02_lotus_root", new[] { E(1,70,128f,0f,60f,0f,60f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "s02_meat", new[] { E(1,70,226f,0f,0f,0f,0f,0f,20f,0f,0f,5,"Eat",null,1200f) } },
        { "s02_meat_rotten", new[] { E(1,70,163f,0f,0f,0f,0f,0f,20f,0f,0f,5,"Eat",null,1200f) } },
        { "s02_moss", new[] { E(1,70,63f,0f,38f,0f,38f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "s02_slug", new[] { E(1,70,146f,0f,0f,0f,0f,0f,20f,0f,0f,3,"Eat",null,600f) } },
        { "s02_spider", new[] { E(1,70,146f,0f,0f,0f,0f,0f,20f,0f,0f,3,"Eat",null,600f) } },
        { "s02_worm_stew", new[] { E(1,70,0f,0f,-200f,0f,-200f,0f,20f,-100f,2f,3,"Drink",null,600f) } },
        { "salad_01", new[] { E(1,70,100f,1.3f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,1200f) } },
        { "salt", new[] { E(1,70,5f,0.5f,0f,0f,0f,0f,5f,0f,0f,3,"Eat","thirsty",300f) } },
        { "salt_water", new[] { E(1,70,3f,0f,0f,0f,0f,0f,5f,0f,0f,3,"Drink","thirsty",300f) } },
        { "samgyetang", new[] { E(1,70,70f,2.5f,10f,0.25f,10f,0.25f,13f,0f,0f,5,"Eat","energetic",1200f) } },
        { "sandwich_01", new[] { E(1,70,55f,2.5f,0f,0f,0f,0f,15f,0f,0f,3,"Eat",null,1200f) } },
        { "sashimi", new[] { E(1,70,110f,1.5f,0f,0f,0f,0f,15f,0f,0f,3,"Eat",null,600f) } },
        { "sauce_01", new[] { E(1,70,50f,0.75f,0f,0f,0f,0f,60f,0f,0f,5,"Drink","thirsty",600f) } },
        { "sauce_02", new[] { E(1,70,55f,0.75f,0f,0f,0f,0f,60f,0f,0f,5,"Drink","thirsty",600f) } },
        { "sauce_02_fruit", new[] { E(1,70,60f,0.75f,0f,0f,0f,0f,60f,0f,0f,5,"Drink","thirsty",600f) } },
        { "sauce_02_vege", new[] { E(1,70,55f,0.75f,0f,0f,0f,0f,60f,0f,0f,5,"Drink","thirsty",600f) } },
        { "sauce_03", new[] { E(1,70,70f,0.75f,0f,0f,0f,0f,60f,0f,0f,5,"Drink","thirsty",600f) } },
        { "sausage_01", new[] { E(1,70,60f,2.5f,0f,0f,0f,0f,15f,0f,0f,3,"Eat",null,1200f) } },
        { "sausage_02", new[] { E(1,70,70f,3f,0f,0f,0f,0f,15f,0f,0f,3,"Eat",null,1200f) } },
        { "shaved_ice", new[] { E(1,70,20f,0.5f,0f,0f,0f,0f,10f,0f,0f,3,"Eat","cold_food",1200f) } },
        { "shaved_ice_watermelon", new[] { E(1,70,65f,3f,0f,2f,0f,2f,10f,-70f,0f,3,"Eat","cold_food",1200f) } },
        { "shrimp", new[] { E(1,14,50f,1.3f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f), E(15,70,50f,1.3f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "shrimp_big", new[] { E(1,14,58f,1.3f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f), E(15,70,58f,1.3f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "sirloin_steak", new[] { E(1,60,30f,1f,0f,0f,0f,0f,50f,0f,0f,5,"Eat","hot_food",900f) } },
        { "skewer_meat_mart", new[] { E(1,70,63f,1.7f,0f,0f,0f,0f,12f,0f,0f,5,"Eat",null,1200f) } },
        { "snails", new[] { E(1,70,50f,1.2f,0f,0f,0f,0f,5f,0f,0f,3,"Eat","eat_bizarre_food",300f) } },
        { "songpyeon", new[] { E(1,70,65f,3.5f,0f,0f,0f,0f,30f,0f,0f,3,"Eat",null,1800f) } },
        { "spam", new[] { E(1,70,80f,2f,0f,0f,0f,0f,30f,0f,0f,5,"Eat",null,900f) } },
        { "spider", new[] { E(1,70,25f,0.7f,0f,0f,0f,0f,15f,0f,0f,5,"Eat",null,300f) } },
        { "stew_01", new[] { E(1,70,50f,2.5f,0f,0f,0f,0f,7f,0f,0f,3,"Drink","hot_food",1200f) } },
        { "stew_02", new[] { E(1,70,65f,3f,0f,0f,0f,0f,7f,0f,0f,3,"Drink","hot_food",1200f) } },
        { "stew_dumpling_ricecake_event_newyear_2019", new[] { E(1,70,50f,1.3f,10f,0.25f,10f,0.25f,18f,0f,0f,5,"Eat","energetic",1200f) } },
        { "stew_ricecake", new[] { E(1,70,30f,2.5f,10f,0.25f,0f,0f,6f,0f,0f,3,"Drink","hot_food",600f) } },
        { "stew_ricecake_event_newyear_2019", new[] { E(1,70,50f,1.3f,10f,0.25f,10f,0.25f,13f,0f,0f,5,"Eat","energetic",1200f) } },
        { "stone_plate_egg_01", new[] { E(1,60,0f,0f,0f,0f,0f,0f,10f,0f,0f,5,"Eat","thirsty",900f) } },
        { "stone_plate_egg_02", new[] { E(1,60,0f,0f,0f,0f,0f,0f,10f,0f,0f,5,"Eat","thirsty",300f) } },
        { "stone_plate_egg_03", new[] { E(1,60,0f,0f,0f,0f,0f,0f,10f,0f,0f,5,"Eat","thirsty",900f) } },
        { "stone_plate_egg_04", new[] { E(1,60,0f,0f,0f,0f,0f,0f,10f,0f,0f,5,"Eat","thirsty",300f) } },
        { "sugar", new[] { E(1,70,60f,1.5f,0f,0f,0f,0f,5f,0f,0f,3,"Eat",null,300f) } },
        { "sugarcane", new[] { E(1,70,23f,0.75f,0f,0f,0f,0f,15f,0f,0f,5,"Eat","fruit_water",300f) } },
        { "survival_food_the_firm", new[] { E(1,70,40f,1f,0f,0f,0f,0f,3f,0f,0f,3,"Eat",null,300f) } },
        { "sushi", new[] { E(1,70,110f,2.5f,0f,0f,0f,0f,20f,0f,0f,3,"Eat",null,1200f) } },
        { "taro_infertility", new[] { E(1,60,35f,1f,0f,0f,0f,0f,15f,-0.25f,0f,5,"Eat",null,300f) } },
        { "taro_milktea", new[] { E(1,60,0f,0f,0f,0f,0f,0f,20f,-0.33f,0f,5,"Drink",null,300f) } },
        { "taro_seed", new[] { E(1,60,20f,1f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "taro_stick", new[] { E(1,60,0f,0f,0f,0f,0f,0f,20f,0f,0f,3,"Eat",null,300f) } },
        { "tea", new[] { E(1,70,30f,0.25f,0f,0f,0f,0f,5f,0f,0f,3,"Drink",null,300f) } },
        { "tea_01_black", new[] { E(1,70,30f,0.25f,0f,0f,0f,0f,5f,0f,0f,3,"Drink","tea_effect_02",1200f) } },
        { "tea_01_green", new[] { E(1,70,30f,0.25f,0f,0f,0f,0f,5f,0f,0f,3,"Drink","tea_effect_01",1200f) } },
        { "tenderloin_steak", new[] { E(1,60,30f,1f,0f,0f,0f,0f,50f,0f,0f,5,"Eat","hot_food",900f) } },
        { "tendon", new[] { E(1,70,30f,1f,0f,0f,0f,0f,60f,0f,0f,10,"Eat","eat_bizarre_food",300f) } },
        { "termites", new[] { E(1,70,50f,1.2f,0f,0f,0f,0f,5f,0f,0f,3,"Eat","eat_bizarre_food",300f) } },
        { "test_chicken", new[] { E(1,70,125f,1f,205f,5f,0f,0f,120f,0f,0f,10,"Eat",null,300f) } },
        { "test_pre_immune_oil", new[] { E(1,70,20f,0.5f,0f,0f,0f,0f,20f,0f,0f,3,"Eat",null,300f) } },
        { "tokbokki", new[] { E(1,70,70f,2.5f,0f,0f,0f,0f,20f,0f,0f,3,"Eat",null,1200f) } },
        { "tokbokki_cream_sauce", new[] { E(1,70,70f,2.5f,0f,0f,0f,0f,20f,0f,0f,3,"Eat",null,1200f) } },
        { "tokbokki_fruit_sauce", new[] { E(1,70,70f,2.5f,0f,0f,0f,0f,20f,0f,0f,3,"Eat",null,1200f) } },
        { "tokbokki_oil_sauce", new[] { E(1,70,70f,2.5f,0f,0f,0f,0f,20f,0f,0f,3,"Eat",null,1200f) } },
        { "tokbokki_vege_sauce", new[] { E(1,70,70f,2.5f,0f,0f,0f,0f,20f,0f,0f,3,"Eat",null,1200f) } },
        { "tomato_infertility", new[] { E(1,70,33f,1f,0f,0f,0f,0f,10f,0f,0f,5,"Eat","fruit_water",300f) } },
        { "tomato_seed", new[] { E(1,70,35f,1f,0f,0f,0f,0f,10f,0f,0f,5,"Eat","fruit_water",300f) } },
        { "valentine_cacao_mass", new[] { E(1,70,10f,0.3f,0f,0f,0f,0f,5f,0f,0f,5,"Eat","thirsty",1200f) } },
        { "valentine_cacaobean", new[] { E(1,70,10f,0.2f,0f,0f,0f,0f,5f,0f,0f,3,"Eat",null,600f) } },
        { "valentine_cacaotree_seed", new[] { E(1,70,15f,0.2f,0f,0f,0f,0f,5f,0f,0f,3,"Eat",null,600f) } },
        { "valentine_chocolate_milk", new[] { E(1,70,50f,1.5f,0f,0f,0f,0f,5f,0f,1f,3,"Drink",null,600f) } },
        { "valentine_chocolate_weapon", new[] { E(1,70,25f,0.2f,15f,0.1f,0f,0f,15f,0f,0f,7,"Eat",null,1200f) } },
        { "valentine_chocolate_weapon_head", new[] { E(1,70,15f,0.2f,10f,0.1f,0f,0f,10f,0f,0f,5,"Eat",null,1200f) } },
        { "valentine_fruit_chocolate", new[] { E(1,70,15f,0.2f,20f,0.1f,0f,0f,3f,0f,0f,2,"Eat",null,300f) } },
        { "vinegar", new[] { E(1,70,10f,0.5f,0f,0f,0f,0f,5f,0f,0f,3,"Eat",null,300f) } },
        { "warp_ketupat", new[] { E(1,70,200f,0f,0f,0f,0f,0f,10f,0f,0f,5,"Eat","energetic",1800f) } },
        { "water", new[] { E(1,70,5f,0f,0f,0f,0f,0f,5f,0f,1f,3,"Drink","drink_water",300f) } },
        { "water_well", new[] { E(1,70,5f,0f,0f,0f,0f,0f,5f,0f,1f,3,"Drink","drink_water",300f) } },
        { "watermelon_01", new[] { E(1,70,20f,0.5f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,300f) } },
        { "watermelon_punch", new[] { E(1,70,30f,0.25f,0f,1.5f,0f,1.5f,10f,-50f,1f,3,"Eat","energetic",300f) } },
        { "wheat_infertility", new[] { E(1,70,43f,1f,0f,0f,0f,0f,15f,0f,0f,5,"Eat",null,300f) } },
        { "wheat_seed", new[] { E(1,70,43f,1f,0f,0f,0f,0f,15f,0f,0f,5,"Eat",null,300f) } },
        { "white_rose_salt", new[] { E(1,70,5f,0.5f,0f,0f,0f,0f,5f,0f,0f,3,"Eat","thirsty",300f) } },
        { "white_rose_seed", new[] { E(1,70,33f,1f,0f,0f,0f,0f,10f,0f,0f,5,"Eat",null,300f) } },
        { "wildberry", new[] { E(1,14,20f,0.5f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,300f), E(15,70,20f,0.5f,0f,0f,0f,0f,10f,0f,0f,3,"Eat",null,300f) } },
        { "wine_fruit", new[] { E(1,70,30f,0.25f,0f,0f,0f,0f,5f,0f,0f,3,"Drink",null,1200f) } },
        { "wine_spring", new[] { E(1,70,240f,0f,0f,0f,0f,0f,30f,0f,0f,5,"Drink",null,1200f) } },
        { "worm", new[] { E(1,70,50f,1.2f,0f,0f,0f,0f,5f,0f,0f,3,"Eat","eat_bizarre_food",300f) } },
        { "worm_silk", new[] { E(1,70,50f,1.2f,0f,0f,0f,0f,5f,0f,0f,3,"Eat","eat_bizarre_food",300f) } },
    };

    /// <summary>ของชิ้นนี้กินได้ไหม</summary>
    public static bool IsFood(string prototype)
    {
        return !string.IsNullOrEmpty(prototype) && Map.ContainsKey(prototype);
    }

    /// <summary>ข้อมูลอาหารของไอเทมเลเวลนี้ (ไม่เจอช่วงที่ตรง = ใช้ช่วงแรก)</summary>
    public static bool TryGet(string prototype, int level, out Entry entry)
    {
        entry = null;
        if (string.IsNullOrEmpty(prototype) || !Map.TryGetValue(prototype, out Entry[] entries) || entries.Length == 0)
        {
            return false;
        }
        for (int i = 0; i < entries.Length; i++)
        {
            if (level >= entries[i].MinLevel && level <= entries[i].MaxLevel)
            {
                entry = entries[i];
                return true;
            }
        }
        entry = entries[0];
        return true;
    }
}
