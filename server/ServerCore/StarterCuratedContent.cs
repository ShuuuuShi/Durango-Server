using System.Collections.Generic;

namespace DurangoServer.Core;

// รายชื่อสูตรคราฟ/แบบก่อสร้างที่ปลดล็อกให้ผู้เล่นตั้งแต่แรก — คัดจาก RecipeData.AllRecipeIds/AllBlueprintIds
// (ลิสต์เต็ม 720 สูตร / 570 แบบก่อสร้าง) ให้เหลือเฉพาะของที่จำเป็นกับการเล่น 3 แนวหลัก:
// ล่าสัตว์ (อาวุธ+ชิ้นส่วนประกอบ), ปลูกผัก (จอบ/ปุ๋ย/ถนอมอาหาร), สร้างบ้าน (ผนัง/หลังคา/ประตู/เฟอร์นิเจอร์)
//
// เกณฑ์คัด (ดูรายละเอียดใน docs/server/Starter-Recipes-Report.md):
// สูตร (354/720): เอาทั้งหมวด weapon_and_tool, tool, material_process, modular_attach (โครงสร้างหลัก
//   ของ 3 แนวเล่น) + เอาเฉพาะ cook_fire/cook_water/cook_preserve/cook_ingredient จากหมวด cook (ถนอม/เตรียม
//   อาหารจากของล่า/ปลูกได้ ไม่เอาอาหารหรูหลายวัตถุดิบ) + เอาเฉพาะ clothes_novice/shoes/gloves/bag จากหมวด
//   clothing (เสื้อผ้าเอาตัวรอดพื้นฐาน ไม่เอาชุดสะสม/แต่งหน้า) — จากนั้น **จำกัด MinLevel ≤ 30** (เจ้าของสั่ง
//   "จำกัดแค่เลเวล 30" 25 ส.ค. 2026 — สูตรเทียร์สูง 35-55 ตัดออก 103 อัน จะได้ไม่เห็นของเกินตัวตั้งแต่ Lv.1)
//   ตัด: cook_special/cook_medicine/cook_yeast_fermentation/cook_oil/cook_bake, clothes_hunter/explorer/
//   settler/reform/accessary/part/hat (ชุดคอสตูมหลายเทียร์), หมวด season2/event/system ทั้งหมด
// แบบก่อสร้าง (455/570): เดิมคัดด้วยชื่อ (ตัด halloween/xmas/army/compi ฯลฯ) เหลือ 515 — 25 ส.ค. 2026
//   (รอบ 3) เจอว่ายังหลุด **60 อัน** ที่ชื่อไม่ส่อถึงอีเวนต์เลย (เช่น `camp_radio_station`,
//   `camp_warehouse`, `statue_03_a`, `s02_flag_bohnanza_01`) เพราะกรองด้วยชื่อไม่พอ — ไปแกะข้อมูล
//   category จริงจาก APK มือถือ (`5.2.1`, ดูหัวข้อ "รอบ 3" ใน Starter-Recipes-Report.md) แล้วคัดใหม่ด้วย
//   category จริง: ตัด `recipe_book` (ของอีเวนต์ xmas/halloween/volcano — ตรงกับ Category ฝั่งสูตรคราฟ
//   พอดี) + `constructing_season2` (ของ DLC ฤดูกาล 2) + ทุกอันที่คำอธิบายเกมบอกตรงๆ ว่า "ระบบสร้าง
//   ผู้เล่นสร้างเองไม่ได้" (39 อัน — พร็อพฉากแคมป์ NPC เช่น camp_radio_station/camp_warehouse ทุกเทียร์)
//   รวมตัดออกจริง 74 อัน (60 อันหลุดมาจากลิสต์เดิม + 14 อันที่กรองด้วยชื่อไว้แล้วอยู่ก่อน) เหลือ 455
//   ไม่มีข้อมูล MinLevel ให้คัดตามเลเวลเหมือนสูตร (ระบบเกมไม่ผูกเลเวลกับแบบก่อสร้างโดยตรง)
//
// MinLevel ที่เหลือ (≤30) ยังไล่ระดับตามเลเวลผู้เล่นอยู่เหมือนเดิม (Lv1 ยังมีของให้ทำเยอะสุด ไล่ไป
// จนถึง Lv30) — ของอีเวนต์/ฤดูกาล/ระบบถูกกันเป็นพิเศษอีกชั้นที่ตัว handler ฝั่ง server ด้วย (ดู
// RecipeData.IsEventRecipeCategory / IsEventBlueprint ที่ HandleCraft/HandleOccupyArtifactSite/
// HandlePlaceCapsulatedArtifact) ให้แอดมินเท่านั้นที่คราฟ/วางได้ แม้ client จะยัด packet มาตรง ๆ ก็ตาม
public static class StarterCuratedContent
{
    public static readonly string[] Recipes = new[]
    {
        "artifact_repair_kit_01", "artifact_repair_kit_02", "artifact_repair_kit_03", "assembled_axe_one_01", "assembled_axe_one_02", "assembled_axe_two_01",
        "assembled_axe_two_02", "assembled_bow_two_02", "assembled_hammer_one_01", "assembled_hammer_one_02", "assembled_hammer_two_01", "assembled_hammer_two_02",
        "assembled_lance_two_02", "assembled_sword_one_01", "assembled_sword_one_02", "assembled_sword_two_01", "assembled_sword_two_02", "axe_tool_bone_01",
        "bag_bamboo", "bag_cherryblossom", "bag_cross", "bag_fabric", "bag_leaf", "bag_small",
        "bag_water_horn", "bag_water_pouch", "blade_axe_bone_01", "blade_axe_bone_02", "blade_axe_stone_01", "blade_big_axe_bone_01",
        "blade_big_axe_bone_02", "blade_big_axe_stone_01", "blade_big_bone", "blade_big_hammer_bone_01", "blade_big_hammer_stone_01", "blade_big_metal",
        "blade_big_stone", "blade_big_sword_bone_01", "blade_big_sword_stone_01", "blade_big_sword_stone_02", "blade_bone", "blade_hammer_bone_01",
        "blade_hammer_stone_01", "blade_hoe_bone", "blade_lance_bone_01", "blade_lance_stone_01", "blade_metal", "blade_pickaxe_bone",
        "blade_saw_bone", "blade_saw_stone", "blade_shovel_bone", "blade_stone", "blade_sword_bone_01", "blade_sword_stone_01",
        "blade_sword_stone_02", "blade_tool_stone", "board", "board_02", "board_03", "board_bak",
        "board_metal", "boil", "boiled_meat", "bonedecoration_01_head_common", "bonedecoration_01_horn", "bonedecoration_01_ivory",
        "bow_wooden_assembled", "bowstick_bone_01", "bowstick_wooden_01", "bowstring_01", "bowstring_02", "breaking_solid_lava",
        "broth", "butter", "can_food_01", "can_food_02", "candle_01", "capture_tool_01",
        "capture_tool_02", "charcoal", "charcoal_t2", "cheese_01", "clothes_hanbok", "clothes_jasmine_dress",
        "clothes_jasmine_hat", "clothes_leaf_01", "clothes_leather_01", "clothes_repair_kit_01", "clothes_repair_kit_02", "clothes_repair_kit_03",
        "clothes_shield", "clothes_shield_02", "clothes_straw_01", "club_onehand_wooden_01", "combine_metal", "copper_alloy",
        "cream", "crossbody_01", "crossbody_02", "crutch", "cut_pillar", "cutting_board_01",
        "door_01_frameless", "door_01_steel", "door_01_wood", "door_01_woodplank", "door_02_bone", "door_02_leather",
        "door_02_steel", "door_02_wood", "door_03_bone", "door_03_wood", "door_03_wood_leather", "dough_bread",
        "dough_cake", "dough_noodle", "dough_pie", "dry", "dry_food_01", "dry_food_02",
        "dry_rubber", "extend_rope", "extend_sheet", "extend_stick", "fabric_waterproof", "feed_carni_01",
        "feed_carni_02", "feed_carni_03_function", "feed_herb_01", "feed_herb_02", "feed_herb_03_function", "fertilizer_01",
        "fertilizer_02", "fertilizer_boost", "fertilizer_boost_02_1", "fertilizer_boost_02_2", "fertilizer_liquid_01", "fools_poster_01",
        "fools_poster_01_01", "fools_poster_02", "fools_poster_02_01", "frame_01_square", "frame_01_vertical", "frame_01_wide",
        "freecuttingbrass_alloy", "gloves_fivefinger_01", "gloves_halffinger_01", "gloves_mitten_01", "gloves_oversleeve_01", "glue",
        "glue_set", "glue_set_t2", "grafting_tree_01", "grafting_tree_02", "grill_stone", "hammer_tool_bone_01",
        "handle", "handle_02", "handle_02_long", "handle_03", "handle_03_long", "handle_lance_01",
        "handle_lance_02", "harpoon_bone_01", "harpoon_wooden_01", "hat_leaf", "hat_shield_02", "hat_straw",
        "hinge", "hinge_metal", "hoe_bone_01", "hoe_wooden_00", "hoe_wooden_01", "iguana_scale_to_leather_scaled",
        "iguana_scale_to_leather_striped", "instrument_drum_big", "instrument_drum_small02", "instrument_guitar02", "instrument_guitar03", "instrument_guitar_01",
        "instrument_horn01", "instrument_piano", "instrument_piano_elec", "instrument_synth01", "instrument_wind_01", "iron_alloy",
        "jewel_craft_extraction", "jewel_craft_eyes", "jewel_grinding_01", "jewel_grinding_02", "jewel_polishing", "juice_01",
        "lance_twohand_bamboo", "leadedbronze_alloy", "make_fur", "meatball_01", "metal_connection", "metal_set",
        "metal_set_t2", "metal_stick", "metal_stick_t2", "modular_ground_basalt", "modular_ground_board_wood", "modular_ground_dirt",
        "modular_ground_granite", "modular_ground_leaf", "modular_ground_marble", "modular_ground_stem", "modular_ground_stone", "modular_ground_stone_02",
        "modular_ground_stone_03", "modular_ground_wood", "modular_ground_woodplank_01", "modular_roof_basalt", "modular_roof_board_metal", "modular_roof_board_wood",
        "modular_roof_granite", "modular_roof_leaf", "modular_roof_leather", "modular_roof_stem", "modular_roof_wood", "modular_roof_woodplank_01",
        "modular_wall_basalt", "modular_wall_board_wood", "modular_wall_dirt", "modular_wall_granite", "modular_wall_metal", "modular_wall_stem",
        "modular_wall_stone", "modular_wall_stone_02", "modular_wall_stone_03", "modular_wall_wood", "modular_wall_woodplank_01", "mortar_01",
        "nail", "nail_metal", "nail_metal_t2", "needle", "needle_metal", "noodle_soup",
        "oil_grains", "oil_oily_fruit", "pickaxe_bone_01", "pickaxe_wooden_01", "pillar_metal", "pillar_stone",
        "pot_01", "pot_02", "powder_coffee", "powder_grain", "preserved", "process",
        "process_02", "process_03", "process_04", "process_05", "re_accuracy_trans_to_critical", "recipe_arrow",
        "recipe_bone_hammer", "recipe_ddukmae", "recipe_glazed_meat", "recipe_gunpowder", "recipe_powder_barley", "recipe_urban_axe",
        "recipe_urban_bow", "recipe_urban_lance", "recipe_valentine_chocolate_weapon", "recipe_valentine_chocolate_weapon_head", "refine", "refine_02",
        "refine_03", "refine_03_t2", "refine_04", "refine_snowfield", "reform_nail", "reform_temper",
        "rice_cake", "rice_steamed", "roast_01", "roast_01_seasoning", "roast_02", "roast_02_seasoning",
        "roast_03", "roast_03_seasoning", "salt", "salt_food_01", "salt_food_02", "saltpeter_concentration_merge",
        "samgyetang", "sashimi", "sauce_01", "sauce_02", "sauce_03", "sausage_01",
        "sausage_02", "saw_bone_01", "saw_stone_01", "sheaf", "shoes_boots_01", "shoes_footwraps",
        "shoes_moccasin_01", "shoes_sandal_leather", "shoes_sandal_straw", "shoes_wood", "shovel_bone_01", "shovel_wooden_00",
        "shovel_wooden_01", "silkworm_breeding", "silkworm_cocoon", "skewer", "skin_wood", "sling_01",
        "smelt", "smoke_food_01", "smoke_food_02", "soup_01", "steak_set_meal", "steam",
        "stew_01", "stew_02", "stirup", "stirup_metal", "string_leather", "sugar",
        "sulphur_concentration_merge", "sword_tool_bone_01", "tan", "tan_02", "tan_02_t2", "tan_03",
        "tan_04", "tan_snowfield", "taro_milktea", "tool_repair_kit_01", "tool_repair_kit_02", "tool_repair_kit_03",
        "torch_01_wall_wood", "torch_02_wall_wood", "trim", "twist_rope", "twist_rope_02", "twist_rope_02_t2",
        "wax", "weapon_connection", "weapon_connection_t2", "window_01_bone", "window_01_steel", "window_01_wood",
        "window_01_woodplank", "window_02_bone", "window_02_leather", "window_02_steel", "window_02_wood", "window_02_wood_leather",

    };

    public static readonly string[] Blueprints = new[]
    {
        "allround", "aqua_crack_01", "artifact_chinaberry", "artifact_feathertree", "artifact_oak", "artifact_sandalwood",
        "artifact_wildberry", "barrel_01", "barrel_02", "barrel_oak_01", "basket", "bath_02_wood",
        "bathtub_01", "beanbag_01_fabric", "beanbag_01_leather", "beanbag_01_straw", "beanbag_02", "bed_01",
        "bed_01_airmat", "bed_01_steel", "bed_02", "bed_02_steel_hospital", "bed_03", "bed_04",
        "bench_02", "benchpress_01", "blackboard_message_01", "blackboard_message_02", "bm_log_cabin", "bm_log_cabin_chair_01",
        "bm_log_cabin_fireplace", "bm_volcanic_cabin", "bm_volcanic_cabin_fireplace", "board_animation_01", "board_animation_02", "board_message_01",
        "board_message_01_02", "board_message_02", "bonfire", "bonfire_01", "bottom_animation_01", "bottom_message_01",
        "bottom_message_02", "bottom_message_03", "box_04", "box_weapon", "box_weapon_01", "box_weapon_02",
        "brazier_01", "bridge_stone_01", "bridge_test", "cabin_wood_chair", "cabin_wood_dresser", "cabin_wood_storage",
        "cabin_wood_table", "cabinet_01", "cabinet_02", "cabinet_warp_01", "cabinet_warp_02", "cage_01_2",
        "cage_01_4", "cage_01_6", "cage_02_6", "cage_domestication_2", "cage_domestication_4", "cake_tower_carnation_01",
        "cake_tower_carnation_02", "camp_communication_station_01_sunset", "camp_rest_01_volcanic", "camp_rest_02", "camp_rest_02_b", "camp_warehouse_01_c",
        "camp_warehouse_02", "camping_grill", "candlestick_large_01", "candlestick_large_02", "candlestick_small_01", "cargo_warphole_in",
        "cargo_warphole_out", "cargo_warphole_out2", "cargo_warphole_out3", "cargo_warphole_out4", "carnation_fence", "carpet_cotton_01",
        "carpet_red_01", "cat_tower_01", "cat_tower_02", "catapult", "chair_01_marble_01", "chair_01_recliner",
        "chair_01_wood_01", "chair_01_wood_02", "chair_01_wood_03", "chair_01_wood_04", "chair_02_wood_01", "chair_03",
        "chair_bark_01", "chair_bark_02", "chair_medieval_01", "chair_medieval_02", "chair_medieval_03", "chair_stool_medieval_01",
        "chair_warp_01", "chair_warp_02", "chair_warp_h_01", "chair_warp_h_02", "chair_warp_h_03", "chandelier_01",
        "cherryblossom_bed", "cherryblossom_bedtable", "cherryblossom_rest", "cherryblossom_rest_02", "cherryblossom_rest_03", "cherryblossom_rest_04",
        "cherryblossom_rest_05", "cherryblossom_rest_06", "cherryblossom_vase", "clan_advanced_lab", "clan_adventure_lab", "clan_battle_lab",
        "clan_board", "clan_collect_lab", "clan_craft_lab", "clan_hall_01", "clan_thurible", "clan_warehouse",
        "clan_warphole_tuner", "classroom_chair", "classroom_desk", "classroom_deskset", "classroom_deskset_store", "classroom_lecturetable",
        "classroom_lecturetable_store", "classroom_locker_01", "classroom_locker_02", "classroom_locker_store", "closet", "closet_01",
        "closet_02", "closet_03", "closet_table_01", "clotheshorse_warp_01", "clotheshorse_warp_02", "coffee_dutch_01",
        "coffin_dirt_01", "copymachine_01", "crack_01", "crack_activate_flag_01", "curtain_01_bone", "curtain_01_steel_hospital",
        "curtain_blind_01_wood", "deadfall_trap", "defensive_t2", "direction_sign_01", "dock", "dryingrack_01",
        "dryingrack_02", "dryingrack_02_t2", "dumpster_01", "dye_01", "dye_02", "dye_03",
        "dye_rack_01", "estate_clan_flag", "estate_private_flag_01", "faction_food_bowl_large", "faction_food_bowl_mid", "faction_food_bowl_small",
        "fan_01", "farm_tile_01", "farm_tile_02", "farm_tile_03", "farm_tile_04", "fence1",
        "fence2", "fence3", "fence3_bone", "fence3_leather", "fence3_stone", "fence3_wood",
        "fence_green_wall_01", "fence_green_wall_02", "fence_wood", "fertilizer_maker_01", "fertilizer_maker_02", "fertilizer_maker_03",
        "fireplace_warp_01", "fishtrap", "flag_01_korea", "flower_street_light_01", "flower_street_light_02", "flowerbed_03_deco",
        "flowerbed_tile_01_bone_01", "flowerbed_tile_01_stone_01", "flowerbed_tile_01_wood_01", "flowerbed_tile_02_bone_01", "flowerbed_tile_02_stone_01", "flowerbed_tile_02_wood_01",
        "flowerpot_01", "fur_box_01", "fur_box_02", "fur_box_02_leaf", "fur_box_02_red", "fur_box_03",
        "fur_box_03_leaf", "fur_box_medicinebox_01", "fur_box_medicinebox_02", "fur_table", "fur_table_01", "fur_table_01_marble_01",
        "fur_table_02", "fur_table_03", "fur_table_jewel", "furnace_01", "garden_bench_01", "garden_bin_01",
        "garden_fence_01", "garden_light_01", "gate1", "gate2", "gate3", "gate3_bone",
        "gate3_leather", "gate3_stone", "gate3_wood", "gate_small", "globe_01", "goalnet_basketball_01",
        "goalnet_soccer_01", "heavy_tech_01", "heavy_tech_02", "heavy_tech_03", "heavy_tech_04", "hurdle_01",
        "icebox_01", "icebox_02", "icebox_warp_01", "icebox_warp_02", "icepot_01", "icepot_02",
        "icepot_03", "icepot_04", "kiln_01", "kiln_02", "kiln_04", "kitchen_01",
        "kitchen_02", "kitchen_03", "kitchen_04", "kitchen_table_05", "kotatsu_warp_01", "kotatsu_warp_02",
        "ladder_warp_01", "ladder_warp_02", "leg_trap", "light_01_firefly", "light_tech_01", "light_tech_02",
        "light_tech_03", "light_tech_04", "lighthouse_01", "living_tech_01", "living_tech_02", "living_tech_03",
        "living_tech_04", "loom_01", "loom_02", "mannequin_female_01", "mannequin_female_02", "mannequin_female_03",
        "mannequin_female_04", "mannequin_female_05", "mannequin_female_06", "mannequin_male_01", "mannequin_male_02", "mannequin_male_03",
        "mannequin_male_04", "mannequin_male_05", "mannequin_male_06", "mat_cotton_01", "mat_dirt_01", "mat_leather_01",
        "mat_leather_02", "mat_leather_03", "mat_warp_01", "mat_warp_playground_01", "mat_warp_playground_02", "medicine_table_01",
        "medicine_table_01_t2", "mini_stage_01", "modular", "modular_01", "modular_02", "modular_bm",
        "modular_classroom", "modular_storm_shelter", "modular_test", "modular_upstair", "movil_01", "multiuse_art_asset_1x1_rest",
        "multiuse_art_asset_1x2_rest", "multiuse_art_asset_2x1_rest", "multiuse_art_asset_2x2_rest", "multiuse_art_asset_3x3_rest", "multiuse_art_asset_4x4_clan_hall", "multiuse_art_asset_4x4_rest",
        "oil_lantern_01", "operating_office_01", "package", "parasol_01", "parasol_01_table", "parasol_fabric_01",
        "parasol_leather_01", "parasol_stem_01", "parasol_wood_01", "picnic_01_table", "picnic_02_table", "picnic_rest",
        "pot_flower", "pot_rose", "pot_sunflower", "prime_sunbed_01", "prime_sunbed_02", "prime_sunbed_03",
        "raft", "raft_deck", "rattan_sofa_01", "rattan_sofa_02", "rattan_sofa_03", "rattan_table_01",
        "refrigerator_01", "refrigerator_02", "refrigerator_03", "road1", "road2", "road3",
        "road4", "road5_basalt", "road5_granite", "road_01_leaf", "road_01_shell", "road_cherryblossom",
        "road_flower", "road_leaf", "road_spring", "rocking_horse_01", "rocking_horse_02", "rocking_horse_03",
        "rocking_horse_04", "roof_modular", "roof_modular_01", "roof_modular_02", "rope_velvet_01", "rope_velvet_02",
        "rope_velvet_03", "rose_fence", "rug_andrew", "s02_rubber_icebox", "sauna_01_rock_volcanic", "sauna_02_rock_volcanic",
        "secured_box", "secured_box_01", "secured_box_02", "seol_camp_rest_02", "shelf_01_wood", "shelf_02",
        "slide_01", "small_flowerbed_01_deco", "small_flowerbed_02_deco", "sofa_warp_01", "sofa_warp_02", "spa_pool",
        "spring_horse_01", "spring_horse_02", "sprinkler_01", "sprinkler_02", "sprinkler_03", "sprinkler_03_liquid",
        "sprinkler_04", "sprinkler_04_liquid", "sprinkler_05", "sprinkler_05_liquid", "statue_01_explorer", "statue_01_hunter",
        "statue_01_settler", "statue_02_a", "statue_02_b", "statue_02_c", "statue_02_d", "summerclub_balloon_01",
        "summerclub_balloon_02", "summerclub_balloon_03", "summerclub_balloon_04", "summerclub_bed_01", "summerclub_bed_02", "summerclub_fireplate",
        "summerclub_mirrorball", "summerclub_personal_pool", "summerclub_pool_01", "summerclub_pool_02", "summerclub_pool_03", "summerclub_stage",
        "summerclub_table_01", "summerclub_table_02", "sunbed_01", "table_01_w_a", "table_01_w_b", "table_02_w_a",
        "table_02_w_b", "table_03_w_a", "table_04", "table_medieval_01", "temptent", "tent",
        "test_defensive", "toilet_warp_01", "torch_stand_01", "torch_stand_02", "trampoline_01", "trampoline_02",
        "trap_basket", "trap_pit", "treadmill_01", "tube_01", "tube_02", "tube_03",
        "tube_04", "tube_05", "tube_unicon", "tutorial_boat", "tutorial_bonfire", "vase_large_01",
        "vase_small_01", "vlautingbox_01", "warehouse_01", "warp_accelerator", "warp_sailo", "warphole_personal",
        "wastebasket_01", "wastebasket_02", "weapon_table_01", "well_01", "well_02", "well_03",
        "well_04", "well_05", "wheelchair_01", "whiteboard_message_01", "whiteboard_message_02", "worktable_05",
        "worktable_warp_01", "worktable_warp_02", "worktable_warp_03", "worktable_warp_04", "worktable_warp_05",

    };
}
