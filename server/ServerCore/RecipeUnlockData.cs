using System.Collections.Generic;

namespace DurangoServer.Core;

/// <summary>
/// **สกิลไหนปลดล็อกสูตร/แปลนอะไร** — ค่าจริงจากข้อมูลเกม
///
/// สกัดอัตโนมัติด้วย scripts/extract_recipe_unlocks.py - **อย่าแก้ด้วยมือ**
///
/// ใช้ที่ `GetRecipes` / `GetArtifactBlueprints` เพื่อส่งเฉพาะของที่ปลดล็อกแล้ว
/// เดิมส่งครบทั้ง 720 สูตรให้ทุกคน ⇒ ไม่ได้เรียนสกิลอะไรเลยก็เห็นสูตรครบ
///
/// สรุปจำนวน: สูตรทั้งหมด 720 - ปลดล็อกด้วยสกิล 501 - **ได้ตั้งแต่แรก 219**
/// </summary>
public static class RecipeUnlockData
{
    public readonly struct Unlock
    {
        public readonly string[] Recipes;
        public readonly string[] Blueprints;

        public Unlock(string[] recipes, string[] blueprints)
        {
            Recipes = recipes;
            Blueprints = blueprints;
        }
    }

    /// <summary>สูตรที่ไม่มีสกิลไหนปลดล็อก = ทุกคนได้ตั้งแต่เริ่ม (ไม่งั้นผู้เล่นใหม่คราฟอะไรไม่ได้เลย)</summary>
    public static readonly string[] AlwaysRecipes = new[] { "antidote_heatpoison", "axe_process", "bag_bamboo", "bag_cherryblossom", "blade_axe_metal_02_t2", "blade_big_axe_metal_02_t2", "blade_big_hammer_metal_02_t2", "blade_big_hammer_metal_03", "blade_hammer_metal_02_t2", "blade_hammer_metal_03", "bleach_color_b", "bleach_color_g", "bleach_color_r", "board_bak", "board_metal", "boots_stabproof", "bowstick_bone_04", "candle_01", "charcoal_t2", "clothes_builder_02", "clothes_explorer_02_scout_t2", "clothes_explorer_04", "clothes_hanbok", "clothes_hunter_03_skill_t2", "clothes_hunter_04", "clothes_jasmine_dress", "clothes_jasmine_hat", "clothes_santa_balloon_2018", "clothes_santa_bottom_2018", "clothes_santa_fur_2018", "clothes_santa_top_2018", "clothes_settler_02_sociable_t2", "clothes_settler_pajamas", "clothes_skeleton_event", "corsage_carnation_01", "corsage_carnation_02", "crossstick_bone_02_t2", "crossstick_bone_04", "crutch", "door_01_steel", "door_02_leather", "door_02_steel", "dough_pie", "drug_faction_enthanasia", "drug_faction_narcotic", "dye_color_b", "dye_color_g", "dye_color_r", "exp_jelly", "fools_poster_01", "fools_poster_01_01", "fools_poster_02", "fools_poster_02_01", "frame_01_square", "frame_01_vertical", "frame_01_wide", "freecuttingbrass_alloy", "fruit_mix", "gloves_armorcrafting", "gloves_stabproof", "gloves_weaponcrafting", "glue_set_t2", "grafting_tree_01", "grafting_tree_02", "hammer_constructing", "hat_builder_02", "hat_explorer_02_scout_t2", "hat_explorer_04", "hat_fur_wolf", "hat_hunter_03_skill_t2", "hat_hunter_04", "hat_settler_02_sociable_t2", "hat_skeleton_event", "hat_watermelon_01", "hemostat", "iguana_scale_to_leather_scaled", "instrument_guitar_01", "instrument_wind_01", "iron_alloy", "jewel_grinding_02", "leadedbronze_alloy", "medicine_03", "medicine_poison_oil", "medicine_poison_oil_02", "metal_set_t2", "metal_stick_t2", "modular_ground_basalt", "modular_ground_granite", "modular_ground_marble", "modular_roof_basalt", "modular_roof_board_metal", "modular_roof_board_wood", "modular_roof_granite", "modular_wall_basalt", "modular_wall_granite", "modular_wall_metal", "nail_metal_t2", "necklace_brachiotooth", "necklace_electrum", "necklace_gold_jokbo", "painkiller_root", "preserved", "process_05", "ration_survival", "re_accuracy_trans_to_critical", "recipe_arrow", "recipe_bacon", "recipe_bar_ricecake_event_newyear_2019", "recipe_beer", "recipe_bone_hammer", "recipe_cacaobean_process", "recipe_cake_carnation_03", "recipe_cherryblossom_latte", "recipe_cherryblossom_popcorn", "recipe_cherryblossom_snack", "recipe_chocolate_milk", "recipe_cocktail", "recipe_corn_salad", "recipe_cosmos_pizza", "recipe_crayon_medicine_dye", "recipe_ddukmae", "recipe_decode_code_c19", "recipe_dough_songpyeon", "recipe_dried_persimmon", "recipe_dumpling_event_newyear_2019", "recipe_dumpling_skin_event_newyear_2019", "recipe_fried_dinosaur_eggs", "recipe_fruit_chocolate", "recipe_fruit_sandwich", "recipe_glazed_meat", "recipe_gunpowder", "recipe_halloween_cake", "recipe_halloween_candy", "recipe_halloween_cookie", "recipe_halloween_jelly", "recipe_halloween_juice", "recipe_halloween_sausage", "recipe_halloween_soup", "recipe_halloween_spider_skewers", "recipe_hat_pachy", "recipe_hat_pumpkin", "recipe_jasmine_grilled_fish", "recipe_jasmine_tea", "recipe_meat_skewers", "recipe_mojito", "recipe_powder_barley", "recipe_properly_rice_event_newyear_2019", "recipe_roasted_shrimp", "recipe_rose_steak", "recipe_songpyeon", "recipe_stew_dumpling_ricecake_event_newyear_2019", "recipe_stew_ricecake_event_newyear_2019", "recipe_undiluted_rice_drink_event_newyear_2019", "recipe_urban_axe", "recipe_urban_bow", "recipe_urban_lance", "recipe_valentine_chocolate_weapon", "recipe_valentine_chocolate_weapon_head", "recipe_white_rose_salt", "refine_03_t2", "refine_04", "refine_snowfield", "reform_armorcrafting", "reform_breathability_t2", "reform_clothover_t2", "reform_constructing", "reform_cooking_farming", "reform_gathering", "reform_lightening_t2", "reform_pocket_t2", "reform_process", "reform_suncover_t2", "reform_weaponcrafting", "reform_windbreak_t2", "restore_volc_blueprint", "ribbon_01", "s02_bag_plastic", "s02_clothes_rubber", "s02_container_petbottle", "s02_cut_plastic", "s02_doll", "s02_dry_rubber", "s02_gloves_02", "s02_gloves_t3", "s02_hat_rubber", "s02_seal", "s02_shoes_02", "s02_shoes_t3", "s02_special_awl", "s02_special_ladle", "s02_special_wristband", "s02_stick", "s02_supplies_axe_tool_02", "saltpeter_concentration_merge", "samgyetang", "shaved_ice_watermelon", "shoes_lavaproof", "silkworm_breeding", "silkworm_cocoon", "silkworm_thread", "sling_01", "steak_set_meal", "stirup", "stirup_metal", "sulphur_concentration_merge", "sword_armorcrafting", "tan_02_t2", "tan_04", "tan_snowfield", "taro_milktea", "thread_02_breathability_good", "thread_02_oily", "twist_rope_02_t2", "watermelon_punch", "wax", "weapon_connection_t2", "window_01_steel", "window_02_steel", "wine_spring" };

    /// <summary>แปลนที่ได้ตั้งแต่แรกด้วยเหตุผลเดียวกัน</summary>
    public static readonly string[] AlwaysBlueprints = new[] { "2018_summer_pool_01_w", "2018_summer_pool_02_w", "2018_summer_sunbed_01", "2018_summer_tube_02", "2018_summer_tube_04", "NPC_agent_male", "allround", "aqua_crack_01", "artifact_chinaberry", "artifact_feathertree", "artifact_oak", "artifact_sandalwood", "artifact_wildberry", "barrel_01", "barrel_01_army", "barrel_01_army_fire", "barrel_02", "barrel_oak_01", "bath_02_wood", "beanbag_01_fabric", "beanbag_01_leather", "beanbag_01_straw", "beanbag_02", "beanbag_compi_blue_mini", "beanbag_compi_green", "beanbag_compi_green_01", "beanbag_compi_green_mini", "beanbag_compi_pink_mini", "beanbag_compi_yellow_mini", "bed_01_airmat", "bed_01_steel", "bed_02_halloween", "bed_02_steel_hospital", "bed_03", "bed_03_halloween", "bed_04", "bench_02", "benchpress_01", "blackboard_message_01", "blackboard_message_02", "bm_log_cabin", "bm_log_cabin_chair_01", "bm_log_cabin_fireplace", "bm_volcanic_cabin", "bm_volcanic_cabin_fireplace", "board_message_01_02", "bonfire_01", "bottom_animation_01", "bottom_message_01", "bottom_message_02", "bottom_message_03", "box_01_army", "box_02_army", "box_04", "box_army", "box_weapon_01", "bridge_stone_01", "bridge_test", "cabin_wood_chair", "cabin_wood_dresser", "cabin_wood_storage", "cabin_wood_table", "cabinet_01", "cabinet_02", "cabinet_warp_01", "cabinet_warp_02", "cage_02_6", "cake_tower_carnation_01", "cake_tower_carnation_02", "camp_bike", "camp_board", "camp_board_be_building", "camp_board_neglected", "camp_board_seol", "camp_board_volcanic", "camp_boxingbag", "camp_communication_station_01_sunset", "camp_dryingrack_01", "camp_dryingrack_02", "camp_fur_table_01", "camp_furnace_01", "camp_hot_air_balloon", "camp_kiln_01", "camp_kiln_02", "camp_kitchen_01", "camp_loom_01", "camp_loom_02", "camp_punch_machine", "camp_radio_station", "camp_radio_station_02", "camp_radio_station_be_building", "camp_radio_station_be_building_sunset", "camp_radio_station_neglected", "camp_radio_station_seol", "camp_radio_station_volcanic", "camp_rest_01_volcanic", "camp_rest_02", "camp_rest_02_b", "camp_square_fire", "camp_training_dummy", "camp_training_set", "camp_training_target", "camp_warehouse", "camp_warehouse_01_c", "camp_warehouse_02", "camp_warehouse_be_building", "camp_warehouse_be_building_sunset", "camp_warehouse_neglected", "camp_warehouse_seol", "camp_warehouse_volcacnic", "camp_warphole", "camping_grill", "candlestick_large_01", "candlestick_large_02", "candlestick_small_01", "cargo_warphole_in", "carnation_fence", "carpet_cotton_01", "carpet_red_01", "cat_tower_01", "cat_tower_02", "catapult", "chair_01_army", "chair_01_marble_01", "chair_01_recliner", "chair_03", "chair_medieval_01", "chair_medieval_02", "chair_medieval_03", "chair_stool_medieval_01", "chair_warp_01", "chair_warp_02", "chair_warp_h_01", "chair_warp_h_02", "chair_warp_h_03", "chandelier_01", "cherryblossom_bed", "cherryblossom_bedtable", "cherryblossom_rest", "cherryblossom_rest_02", "cherryblossom_rest_03", "cherryblossom_rest_04", "cherryblossom_rest_05", "cherryblossom_rest_06", "cherryblossom_vase", "christmas_snowman", "christmas_tree_01", "christmas_tree_01_web", "christmas_tree_02", "christmas_tree_02_web", "clan_advanced_lab", "clan_adventure_lab", "clan_battle_lab", "clan_board", "clan_collect_lab", "clan_craft_lab", "clan_hall_01", "clan_thurible", "clan_warehouse", "clan_warphole_tuner", "classroom_chair", "classroom_desk", "classroom_deskset", "classroom_deskset_store", "classroom_lecturetable", "classroom_lecturetable_store", "classroom_locker_01", "classroom_locker_02", "classroom_locker_store", "closet_01", "closet_03", "clotheshorse_warp_01", "clotheshorse_warp_02", "coffee_dutch_01", "coffin_dirt_01", "compi_pool", "copymachine_01", "crack_01", "crack_activate_flag_01", "curtain_01_steel_hospital", "curtain_01_w_halloween", "curtain_02_w_halloween", "curtain_blind_01_wood", "deadfall_trap", "defensive_t2", "dock", "dryingrack_02_t2", "dumpster_01", "dye_03", "estate_clan_flag", "estate_private_flag_01", "faction_food_bowl_large", "faction_food_bowl_mid", "faction_food_bowl_small", "fan_01", "farm_tile_03", "farm_tile_04", "fence3_bone", "fence3_leather", "fence3_stone", "fence3_wood", "fence_green_wall_01", "fence_green_wall_02", "fertilizer_maker_03", "fireplace_warp_01", "flag_01_korea", "flower_street_light_01", "flower_street_light_02", "flowerbed_03_deco", "flowerpot_01", "fur_box_02_leaf", "fur_box_02_red", "fur_box_03_leaf", "fur_box_medicinebox_01", "fur_box_medicinebox_02", "fur_table_01_marble_01", "garden_bench_01", "garden_bin_01", "garden_fence_01", "garden_light_01", "gate3_bone", "gate3_leather", "gate3_stone", "gate3_wood", "globe_01", "goalnet_basketball_01", "goalnet_soccer_01", "halloween_board_01", "halloween_candle", "halloween_communication_station_01", "halloween_lantern", "halloween_rest", "halloween_rest_01", "halloween_warehouse_01", "heavy_tech_01", "heavy_tech_02", "heavy_tech_03", "heavy_tech_04", "hurdle_01", "icebox_01", "icebox_02", "icebox_warp_01", "icebox_warp_02", "icepot_04", "kiln_04", "kitchen_03", "kitchen_04", "kitchen_table_05", "kotatsu_warp_01", "kotatsu_warp_02", "ladder_warp_01", "ladder_warp_02", "light_01_firefly", "light_tech_01", "light_tech_02", "light_tech_03", "light_tech_04", "lighthouse_01", "lighthouse_epic", "living_tech_01", "living_tech_02", "living_tech_03", "living_tech_04", "mannequin_female_01", "mannequin_female_02", "mannequin_female_03", "mannequin_female_04", "mannequin_female_05", "mannequin_female_06", "mannequin_male_01", "mannequin_male_02", "mannequin_male_03", "mannequin_male_04", "mannequin_male_05", "mannequin_male_06", "mat_cotton_01", "mat_leather_03", "mat_warp_01", "mat_warp_playground_01", "mat_warp_playground_02", "medicine_table_01_t2", "medicinebox_01_army", "medicinebox_02_army", "mini_stage_01", "modular_01", "modular_02", "modular_bm", "modular_classroom", "modular_storm_shelter", "modular_test", "movil_01", "multiuse_art_asset_1x1_rest", "multiuse_art_asset_1x2_rest", "multiuse_art_asset_2x1_rest", "multiuse_art_asset_2x2_rest", "multiuse_art_asset_3x3_rest", "multiuse_art_asset_4x4_clan_hall", "multiuse_art_asset_4x4_rest", "neutral_warphole", "oil_lantern_01", "operating_office_01", "package", "parasol_01", "parasol_01_table", "picnic_01_table", "picnic_02_table", "picnic_rest", "pot_flower", "pot_rose", "pot_sunflower", "prime_sunbed_01", "prime_sunbed_02", "prime_sunbed_03", "raft", "raft_deck", "rattan_sofa_01", "rattan_sofa_02", "rattan_sofa_03", "rattan_table_01", "refrigerator_01", "refrigerator_02", "refrigerator_03", "rice_drink_well_event_newyear_2019", "road_01_leaf", "road_01_shell", "road_cherryblossom", "road_flower", "road_leaf", "road_spring", "rocking_horse_01", "rocking_horse_02", "rocking_horse_03", "rocking_horse_04", "roof_modular_01", "roof_modular_02", "rope_velvet_01", "rope_velvet_02", "rope_velvet_03", "rose_fence", "rug_andrew", "s02_box", "s02_car", "s02_flag_bohnanza_01", "s02_flag_bohnanza_02", "s02_flag_chlorophyl_01", "s02_flag_chlorophyl_02", "s02_flag_committee_01", "s02_flag_firm_01", "s02_flag_pioneer_01", "s02_flag_pioneer_02", "s02_flag_society_01", "s02_fur_table", "s02_lizard_trap", "s02_rubber_icebox", "s02_shelter_03", "s02_thurible", "s02_washbowl", "sauna_01_rock_volcanic", "sauna_02_rock_volcanic", "secured_box_02", "seol_camp_rest_02", "shelf_01_wood", "shelf_02", "slide_01", "small_flowerbed_01_deco", "small_flowerbed_02_deco", "sofa_warp_01", "sofa_warp_02", "spa_pool", "spring_horse_01", "spring_horse_02", "sprinkler_04", "sprinkler_04_liquid", "sprinkler_05", "sprinkler_05_liquid", "statue_01_explorer", "statue_01_hunter", "statue_01_settler", "statue_02_a", "statue_02_b", "statue_02_c", "statue_02_d", "statue_03_a", "statue_kompi_epic", "summerclub_balloon_01", "summerclub_balloon_02", "summerclub_balloon_03", "summerclub_balloon_04", "summerclub_bed_01", "summerclub_bed_02", "summerclub_fireplate", "summerclub_mirrorball", "summerclub_personal_pool", "summerclub_pool_01", "summerclub_pool_02", "summerclub_pool_03", "summerclub_stage", "summerclub_table_01", "summerclub_table_02", "sunbed_01", "table_01_w_a", "table_01_w_b", "table_02_w_a", "table_02_w_b", "table_04", "table_medieval_01", "test_defensive", "toilet_warp_01", "trampoline_01", "trampoline_02", "trap_basket", "trap_pit", "treadmill_01", "tube_01", "tube_02", "tube_03", "tube_04", "tube_05", "tube_unicon", "tutorial_boat", "tutorial_bonfire", "vase_large_01", "vase_small_01", "vlautingbox_01", "warehouse_01", "warp_accelerator", "warp_sailo", "wastebasket_01", "wastebasket_02", "well_03", "well_04", "wheelchair_01", "whiteboard_message_01", "whiteboard_message_02", "worktable_05", "worktable_warp_01", "worktable_warp_02", "worktable_warp_03", "worktable_warp_04", "worktable_warp_05", "xmas_board_01", "xmas_communication_station01", "xmas_giftbox_01_2018", "xmas_giftbox_02_2018", "xmas_snowman_01", "xmas_snowman_02", "xmas_snowman_03", "xmas_snowman_04", "xmas_snowman_05", "xmas_snowman_06", "xmas_snowman_07", "xmas_snowman_08", "xmas_snowman_09", "xmas_snowman_10", "xmas_tree_2018_large", "xmas_tree_2018_small", "xmas_warehouse_01" };

    /// <summary>
    /// คีย์ <c>"skillId|subId"</c> → รายการต่อเลเวล (index 0 = เลเวล 1)
    /// เรียนสกิลถึงเลเวล N = ได้ของจาก index 0 ถึง N-1 ทั้งหมด
    /// </summary>
    public static readonly Dictionary<string, Unlock[]> BySkill = new Dictionary<string, Unlock[]>
    {
        { "acc_necklace|__base__", new[]
          {
            new Unlock(new[] { "necklace_flower" }, new string[0]),
            new Unlock(new[] { "necklace_feather" }, new string[0]),
            new Unlock(new[] { "necklace_pearl" }, new string[0]),
            new Unlock(new[] { "necklace_tag" }, new string[0]),
            new Unlock(new[] { "necklace_jem" }, new string[0])
          } },
        { "addon_bone|__base__", new[]
          {
            new Unlock(new[] { "bonedecoration_01_horn" }, new string[0]),
            new Unlock(new[] { "bonedecoration_01_head_common" }, new string[0]),
            new Unlock(new[] { "bonedecoration_01_ivory" }, new string[0])
          } },
        { "axe_onehand|__base__", new[]
          {
            new Unlock(new[] { "assembled_axe_one_01" }, new string[0]),
            new Unlock(new[] { "blade_bone" }, new string[0]),
            new Unlock(new[] { "assembled_axe_one_02", "blade_axe_stone_01" }, new string[0]),
            new Unlock(new[] { "blade_axe_bone_01" }, new string[0]),
            new Unlock(new[] { "blade_axe_bone_02" }, new string[0]),
            new Unlock(new[] { "assembled_axe_one_03", "blade_axe_metal_01" }, new string[0]),
            new Unlock(new[] { "blade_axe_bone_03" }, new string[0]),
            new Unlock(new[] { "blade_axe_metal_02" }, new string[0]),
            new Unlock(new[] { "assembled_axe_one_04", "blade_axe_metal_03" }, new string[0])
          } },
        { "axe_tool|__base__", new[]
          {
            new Unlock(new[] { "axe_tool_bone_01", "blade_tool_stone" }, new string[0]),
            new Unlock(new[] { "axe_tool_metal_01", "blade_tool_bone" }, new string[0])
          } },
        { "axe_twohand|__base__", new[]
          {
            new Unlock(new[] { "assembled_axe_two_01", "blade_big_stone" }, new string[0]),
            new Unlock(new[] { "blade_big_bone" }, new string[0]),
            new Unlock(new[] { "assembled_axe_two_02", "blade_big_axe_stone_01" }, new string[0]),
            new Unlock(new[] { "blade_big_axe_bone_01" }, new string[0]),
            new Unlock(new[] { "blade_big_axe_bone_02" }, new string[0]),
            new Unlock(new[] { "assembled_axe_two_03", "blade_big_axe_metal_01" }, new string[0]),
            new Unlock(new[] { "blade_big_axe_bone_03" }, new string[0]),
            new Unlock(new[] { "blade_big_axe_metal_02" }, new string[0]),
            new Unlock(new[] { "assembled_axe_two_04", "blade_big_axe_metal_03" }, new string[0])
          } },
        { "bag|__base__", new[]
          {
            new Unlock(new[] { "bag_leaf" }, new string[0]),
            new Unlock(new[] { "bag_fabric" }, new string[0]),
            new Unlock(new[] { "bag_small" }, new string[0]),
            new Unlock(new[] { "bag_cross" }, new string[0]),
            new Unlock(new[] { "bag_back" }, new string[0])
          } },
        { "basicwork|__base__", new[]
          {
            new Unlock(new[] { "cut_pillar", "extend_rope", "extend_stick" }, new string[0]),
            new Unlock(new[] { "skin_wood" }, new string[0])
          } },
        { "bath|__base__", new[]
          {
            new Unlock(new string[0], new[] { "bathtub_01" })
          } },
        { "blacksmith|__base__", new[]
          {
            new Unlock(new[] { "blade_big_metal", "blade_metal", "metal_stick", "nail_metal", "needle_metal" }, new string[0]),
            new Unlock(new[] { "metal_connection" }, new string[0]),
            new Unlock(new[] { "metal_set" }, new string[0])
          } },
        { "blade_stone|__base__", new[]
          {
            new Unlock(new[] { "blade_stone" }, new string[0]),
            new Unlock(new[] { "blade_tool_stone", "sword_tool_bone_01" }, new string[0]),
            new Unlock(new[] { "blade_tool_bone", "sword_tool_metal_01" }, new string[0])
          } },
        { "board|__base__", new[]
          {
            new Unlock(new string[0], new[] { "board_message_01" }),
            new Unlock(new string[0], new[] { "board_message_02" }),
            new Unlock(new string[0], new[] { "board_animation_01" }),
            new Unlock(new string[0], new[] { "board_animation_02" })
          } },
        { "bonfire|__base__", new[]
          {
            new Unlock(new string[0], new[] { "bonfire" }),
            new Unlock(new string[0], new[] { "brazier_01" })
          } },
        { "bow|__base__", new[]
          {
            new Unlock(new[] { "assembled_bow_two_02", "bowstick_wooden_01" }, new string[0]),
            new Unlock(new[] { "bowstick_bone_01" }, new string[0]),
            new Unlock(new[] { "assembled_bow_two_03", "bowstick_wooden_02", "bowstring_01" }, new string[0]),
            new Unlock(new[] { "bowstick_bone_02" }, new string[0]),
            new Unlock(new[] { "bowstick_bone_03" }, new string[0]),
            new Unlock(new[] { "assembled_bow_two_04", "bowstick_metal_01", "bowstring_02" }, new string[0])
          } },
        { "bow_assembled|__base__", new[]
          {
            new Unlock(new[] { "bow_wooden_assembled" }, new string[0])
          } },
        { "box_secure|__base__", new[]
          {
            new Unlock(new string[0], new[] { "secured_box_01" }),
            new Unlock(new string[0], new[] { "secured_box" })
          } },
        { "breaking_solid_lava|__base__", new[]
          {
            new Unlock(new[] { "breaking_solid_lava" }, new string[0])
          } },
        { "cage|__base__", new[]
          {
            new Unlock(new string[0], new[] { "cage_01_2" }),
            new Unlock(new string[0], new[] { "cage_01_4" }),
            new Unlock(new string[0], new[] { "cage_01_6" })
          } },
        { "cage_domestication|__base__", new[]
          {
            new Unlock(new string[0], new[] { "cage_domestication_2" }),
            new Unlock(new string[0], new[] { "cage_domestication_4" })
          } },
        { "capture_capturable|__base__", new[]
          {
            new Unlock(new[] { "capture_tool_01" }, new string[0]),
            new Unlock(new[] { "capture_tool_02" }, new string[0]),
            new Unlock(new[] { "capture_tool_03" }, new string[0])
          } },
        { "clothes_explorer|__base__", new[]
          {
            new Unlock(new[] { "clothes_linen_01" }, new string[0]),
            new Unlock(new[] { "clothes_explorer_01", "hat_explorer_01" }, new string[0]),
            new Unlock(new[] { "clothes_explorer_02", "hat_explorer_02" }, new string[0]),
            new Unlock(new[] { "clothes_explorer_02_scout" }, new string[0]),
            new Unlock(new[] { "clothes_explorer_03_miner", "hat_explorer_03_miner" }, new string[0])
          } },
        { "clothes_explorer|search", new[]
          {
            new Unlock(new[] { "clothes_explorer_02_squad", "hat_explorer_02_squad" }, new string[0]),
            new Unlock(new[] { "clothes_explorer_03_collector", "hat_explorer_03_collector" }, new string[0]),
            new Unlock(new[] { "clothes_sneak_01", "hat_sneak_01" }, new string[0])
          } },
        { "clothes_hunter|__base__", new[]
          {
            new Unlock(new[] { "clothes_winter_01", "hat_winter_01" }, new string[0]),
            new Unlock(new[] { "clothes_hunter_01" }, new string[0]),
            new Unlock(new[] { "clothes_hunter_02" }, new string[0]),
            new Unlock(new[] { "clothes_hunter_03_skill", "hat_hunter_03_skill" }, new string[0]),
            new Unlock(new[] { "clothes_assault_01", "hat_assault_01" }, new string[0])
          } },
        { "clothes_hunter|ranged", new[]
          {
            new Unlock(new[] { "clothes_hunter_02_hunter", "hat_hunter_02_hunter" }, new string[0]),
            new Unlock(new[] { "clothes_sniper_01", "hat_sniper_01" }, new string[0])
          } },
        { "clothes_hunter|towhand", new[]
          {
            new Unlock(new[] { "clothes_hunter_02_strong", "hat_hunter_02_strong" }, new string[0]),
            new Unlock(new[] { "clothes_hunter_03_hunt", "hat_hunter_03_hunt" }, new string[0])
          } },
        { "clothes_log|__base__", new[]
          {
            new Unlock(new[] { "clothes_shield" }, new string[0]),
            new Unlock(new[] { "clothes_shield_02", "hat_shield_02" }, new string[0])
          } },
        { "clothes_novice|__base__", new[]
          {
            new Unlock(new[] { "clothes_leaf_01", "hat_leaf" }, new string[0]),
            new Unlock(new[] { "clothes_straw_01", "hat_straw" }, new string[0]),
            new Unlock(new[] { "clothes_leather_01" }, new string[0])
          } },
        { "clothes_settler|__base__", new[]
          {
            new Unlock(new[] { "clothes_mobility" }, new string[0]),
            new Unlock(new[] { "clothes_settler_01_apron" }, new string[0]),
            new Unlock(new[] { "clothes_settler_01", "hat_settler_01" }, new string[0]),
            new Unlock(new[] { "clothes_settler_02", "hat_settler_02" }, new string[0]),
            new Unlock(new[] { "clothes_settler_02_sociable", "hat_settler_02_sociable" }, new string[0]),
            new Unlock(new[] { "clothes_settler_03_worker", "hat_settler_03_worker" }, new string[0]),
            new Unlock(new[] { "clothes_designer_01" }, new string[0])
          } },
        { "clothes_settler|build", new[]
          {
            new Unlock(new[] { "clothes_builder_01", "hat_builder_01" }, new string[0])
          } },
        { "clothes_settler|cook", new[]
          {
            new Unlock(new[] { "clothes_settler_02_maker" }, new string[0]),
            new Unlock(new[] { "clothes_settler_03_farmer", "hat_settler_03_farmer" }, new string[0]),
            new Unlock(new[] { "clothes_chef_01", "hat_chef_01" }, new string[0])
          } },
        { "clothes_settler|dress", new[]
          {
            new Unlock(new[] { "clothes_settler_flower" }, new string[0])
          } },
        { "clothes_warp|__base__", new[]
          {
            new Unlock(new[] { "clothes_warp_plastic" }, new string[0]),
            new Unlock(new[] { "clothes_warp_tire" }, new string[0])
          } },
        { "club_woolen|__base__", new[]
          {
            new Unlock(new[] { "club_onehand_wooden_01" }, new string[0])
          } },
        { "coal|__base__", new[]
          {
            new Unlock(new[] { "charcoal" }, new string[0])
          } },
        { "combine_food|__base__", new[]
          {
            new Unlock(new[] { "salad_01" }, new string[0]),
            new Unlock(new[] { "sandwich_01" }, new string[0]),
            new Unlock(new[] { "hamberger_01" }, new string[0])
          } },
        { "con_board|__base__", new[]
          {
            new Unlock(new[] { "board" }, new string[0]),
            new Unlock(new[] { "board_02", "board_03" }, new string[0])
          } },
        { "cook_bake|__base__", new[]
          {
            new Unlock(new[] { "bread_01" }, new string[0]),
            new Unlock(new[] { "bread_02" }, new string[0]),
            new Unlock(new[] { "cake_01" }, new string[0]),
            new Unlock(new[] { "bread_03_pizza" }, new string[0]),
            new Unlock(new[] { "cake_02" }, new string[0])
          } },
        { "cook_dye|__base__", new[]
          {
            new Unlock(new[] { "medicine_dye_01" }, new string[0]),
            new Unlock(new[] { "mix_medicine_dye_2color" }, new string[0]),
            new Unlock(new[] { "decolorizer_01" }, new string[0]),
            new Unlock(new[] { "medicine_dye_02" }, new string[0]),
            new Unlock(new[] { "mix_medicine_dye_3color" }, new string[0]),
            new Unlock(new[] { "decolorizer_02" }, new string[0])
          } },
        { "cook_fire|__base__", new[]
          {
            new Unlock(new[] { "skewer" }, new string[0]),
            new Unlock(new[] { "roast_01" }, new string[0]),
            new Unlock(new[] { "roast_01_seasoning" }, new string[0]),
            new Unlock(new[] { "roast_02" }, new string[0]),
            new Unlock(new[] { "roast_02_seasoning" }, new string[0]),
            new Unlock(new[] { "roast_03" }, new string[0]),
            new Unlock(new[] { "roast_03_seasoning" }, new string[0]),
            new Unlock(new[] { "steak_meat" }, new string[0])
          } },
        { "cook_fry|__base__", new[]
          {
            new Unlock(new[] { "fry" }, new string[0]),
            new Unlock(new[] { "chicken_01" }, new string[0]),
            new Unlock(new[] { "chicken_02" }, new string[0])
          } },
        { "cook_med|__base__", new[]
          {
            new Unlock(new[] { "herb_tea_01" }, new string[0]),
            new Unlock(new[] { "medicine_01" }, new string[0]),
            new Unlock(new[] { "medicine_02" }, new string[0])
          } },
        { "cook_med|tea", new[]
          {
            new Unlock(new[] { "tea_01" }, new string[0]),
            new Unlock(new[] { "coffee_drip", "coffee_dutch" }, new string[0])
          } },
        { "cook_med_animal|__base__", new[]
          {
            new Unlock(new[] { "medicine_animal_01" }, new string[0])
          } },
        { "cook_med_immune|__base__", new[]
          {
            new Unlock(new[] { "medicine_immune_plant_01" }, new string[0]),
            new Unlock(new[] { "medicine_immune_bug_01" }, new string[0]),
            new Unlock(new[] { "medicine_immune_poisonsac_01" }, new string[0])
          } },
        { "cook_preserve|__base__", new[]
          {
            new Unlock(new[] { "smoke_food_01" }, new string[0]),
            new Unlock(new[] { "dry_food_01" }, new string[0]),
            new Unlock(new[] { "smoke_food_02" }, new string[0]),
            new Unlock(new[] { "salt_food_01" }, new string[0]),
            new Unlock(new[] { "dry_food_02" }, new string[0]),
            new Unlock(new[] { "sausage_01" }, new string[0]),
            new Unlock(new[] { "salt_food_02" }, new string[0]),
            new Unlock(new[] { "can_food_01" }, new string[0]),
            new Unlock(new[] { "sausage_02" }, new string[0]),
            new Unlock(new[] { "can_food_02" }, new string[0])
          } },
        { "cook_stir|__base__", new[]
          {
            new Unlock(new[] { "fry_stir" }, new string[0]),
            new Unlock(new[] { "puff_grain" }, new string[0]),
            new Unlock(new[] { "noodle_stir_01" }, new string[0]),
            new Unlock(new[] { "noodle_stir_02" }, new string[0])
          } },
        { "cook_water|__base__", new[]
          {
            new Unlock(new[] { "boil" }, new string[0]),
            new Unlock(new[] { "soup_01" }, new string[0]),
            new Unlock(new[] { "steam" }, new string[0]),
            new Unlock(new[] { "boiled_meat" }, new string[0]),
            new Unlock(new[] { "stew_01" }, new string[0]),
            new Unlock(new[] { "rice_steamed" }, new string[0]),
            new Unlock(new[] { "broth" }, new string[0]),
            new Unlock(new[] { "rice_cake" }, new string[0]),
            new Unlock(new[] { "stew_02" }, new string[0]),
            new Unlock(new[] { "noodle_soup" }, new string[0])
          } },
        { "crossbow|__base__", new[]
          {
            new Unlock(new[] { "assembled_crossbow_two_03", "bowstring_01", "crossbody_01", "crossstick_wooden_01" }, new string[0]),
            new Unlock(new[] { "crossstick_bone_01" }, new string[0]),
            new Unlock(new[] { "crossstick_bone_02" }, new string[0]),
            new Unlock(new[] { "assembled_crossbow_two_04", "bowstring_02", "crossbody_02", "crossstick_metal_01" }, new string[0])
          } },
        { "door|__base__", new[]
          {
            new Unlock(new[] { "door_01_wood", "door_02_wood" }, new string[0]),
            new Unlock(new[] { "door_01_frameless", "door_02_bone", "door_03_wood" }, new string[0]),
            new Unlock(new[] { "door_01_woodplank", "door_03_bone", "door_03_wood_leather" }, new string[0])
          } },
        { "dough|__base__", new[]
          {
            new Unlock(new[] { "dough_bread" }, new string[0]),
            new Unlock(new[] { "dough_cake" }, new string[0]),
            new Unlock(new[] { "dough_noodle" }, new string[0])
          } },
        { "dry|__base__", new[]
          {
            new Unlock(new[] { "dry" }, new string[0]),
            new Unlock(new[] { "dry_rubber" }, new string[0])
          } },
        { "dryingrack|__base__", new[]
          {
            new Unlock(new string[0], new[] { "dryingrack_01" }),
            new Unlock(new string[0], new[] { "dryingrack_02" })
          } },
        { "dye|__base__", new[]
          {
            new Unlock(new string[0], new[] { "dye_01" }),
            new Unlock(new string[0], new[] { "dye_02" })
          } },
        { "dye_rack|__base__", new[]
          {
            new Unlock(new string[0], new[] { "dye_rack_01" })
          } },
        { "extend_sheet|__base__", new[]
          {
            new Unlock(new[] { "extend_sheet" }, new string[0])
          } },
        { "fabric_waterproof|__base__", new[]
          {
            new Unlock(new[] { "fabric_waterproof" }, new string[0])
          } },
        { "farm|__base__", new[]
          {
            new Unlock(new string[0], new[] { "farm_tile_01" }),
            new Unlock(new string[0], new[] { "farm_tile_02" })
          } },
        { "feed_pet|__base__", new[]
          {
            new Unlock(new[] { "feed_herb_01" }, new string[0]),
            new Unlock(new[] { "feed_carni_01" }, new string[0]),
            new Unlock(new[] { "feed_herb_02" }, new string[0]),
            new Unlock(new[] { "feed_carni_02" }, new string[0]),
            new Unlock(new[] { "feed_carni_03_function", "feed_herb_03_function" }, new string[0])
          } },
        { "fence|__base__", new[]
          {
            new Unlock(new string[0], new[] { "fence_wood" }),
            new Unlock(new string[0], new[] { "fence1" }),
            new Unlock(new string[0], new[] { "fence2" }),
            new Unlock(new string[0], new[] { "fence3" })
          } },
        { "fermentation|__base__", new[]
          {
            new Unlock(new[] { "vinegar" }, new string[0]),
            new Unlock(new[] { "tea_leaf_fermentation" }, new string[0]),
            new Unlock(new[] { "wine_fruit" }, new string[0]),
            new Unlock(new[] { "cheese_02" }, new string[0])
          } },
        { "fertilizer|__base__", new[]
          {
            new Unlock(new[] { "fertilizer_01" }, new[] { "fertilizer_maker_01" }),
            new Unlock(new[] { "fertilizer_02" }, new[] { "fertilizer_maker_02" }),
            new Unlock(new[] { "fertilizer_liquid_01" }, new string[0])
          } },
        { "fertilizer|process", new[]
          {
            new Unlock(new[] { "fertilizer_boost" }, new string[0]),
            new Unlock(new[] { "fertilizer_boost_02_1", "fertilizer_boost_02_2" }, new string[0])
          } },
        { "firetool|__base__", new[]
          {
            new Unlock(new string[0], new[] { "furnace_01" }),
            new Unlock(new string[0], new[] { "kitchen_01" }),
            new Unlock(new string[0], new[] { "kitchen_02" })
          } },
        { "flowerbed|__base__", new[]
          {
            new Unlock(new string[0], new[] { "flowerbed_tile_01_wood_01", "flowerbed_tile_02_wood_01" }),
            new Unlock(new string[0], new[] { "flowerbed_tile_01_stone_01", "flowerbed_tile_02_stone_01" }),
            new Unlock(new string[0], new[] { "flowerbed_tile_01_bone_01", "flowerbed_tile_02_bone_01" })
          } },
        { "furniture_box|__base__", new[]
          {
            new Unlock(new string[0], new[] { "basket" }),
            new Unlock(new string[0], new[] { "fur_box_01" }),
            new Unlock(new string[0], new[] { "fur_box_02" }),
            new Unlock(new string[0], new[] { "fur_box_03" })
          } },
        { "furniture_box|amor", new[]
          {
            new Unlock(new string[0], new[] { "closet" }),
            new Unlock(new string[0], new[] { "closet_02" })
          } },
        { "furniture_box|eatable", new[]
          {
            new Unlock(new string[0], new[] { "icepot_01" }),
            new Unlock(new string[0], new[] { "icepot_02" }),
            new Unlock(new string[0], new[] { "icepot_03" })
          } },
        { "furniture_box|weapon", new[]
          {
            new Unlock(new string[0], new[] { "box_weapon" }),
            new Unlock(new string[0], new[] { "box_weapon_02" })
          } },
        { "furniture_living|__base__", new[]
          {
            new Unlock(new string[0], new[] { "mat_leather_01" }),
            new Unlock(new string[0], new[] { "mat_dirt_01", "mat_leather_02" })
          } },
        { "furniture_living|curtain", new[]
          {
            new Unlock(new string[0], new[] { "curtain_01_bone" })
          } },
        { "furniture_room|__base__", new[]
          {
            new Unlock(new string[0], new[] { "chair_01_wood_01", "chair_01_wood_02", "chair_01_wood_03", "chair_01_wood_04" }),
            new Unlock(new string[0], new[] { "chair_bark_01", "chair_bark_02" }),
            new Unlock(new string[0], new[] { "chair_02_wood_01" })
          } },
        { "furniture_room|table", new[]
          {
            new Unlock(new string[0], new[] { "table_03_w_a" })
          } },
        { "furniture_shelter|__base__", new[]
          {
            new Unlock(new string[0], new[] { "bed_01" }),
            new Unlock(new string[0], new[] { "bed_02" })
          } },
        { "gate|__base__", new[]
          {
            new Unlock(new string[0], new[] { "gate_small" }),
            new Unlock(new string[0], new[] { "gate1" }),
            new Unlock(new string[0], new[] { "gate2" }),
            new Unlock(new string[0], new[] { "gate3" })
          } },
        { "gloves|__base__", new[]
          {
            new Unlock(new[] { "gloves_oversleeve_01" }, new string[0]),
            new Unlock(new[] { "gloves_halffinger_01" }, new string[0]),
            new Unlock(new[] { "gloves_mitten_01" }, new string[0]),
            new Unlock(new[] { "gloves_knit" }, new string[0])
          } },
        { "gloves|defense", new[]
          {
            new Unlock(new[] { "gloves_fivefinger_01" }, new string[0]),
            new Unlock(new[] { "gloves_bone_01" }, new string[0])
          } },
        { "gloves_shoes_volcanic|__base__", new[]
          {
            new Unlock(new[] { "gloves_volcanic_heat", "shoes_volcanic_heat" }, new string[0])
          } },
        { "glue|__base__", new[]
          {
            new Unlock(new[] { "glue" }, new string[0]),
            new Unlock(new[] { "glue_set" }, new string[0])
          } },
        { "hammer_onehand|__base__", new[]
          {
            new Unlock(new[] { "assembled_hammer_one_01" }, new string[0]),
            new Unlock(new[] { "assembled_hammer_one_02", "blade_hammer_stone_01" }, new string[0]),
            new Unlock(new[] { "blade_hammer_bone_01" }, new string[0]),
            new Unlock(new[] { "assembled_hammer_one_03", "blade_hammer_metal_01" }, new string[0]),
            new Unlock(new[] { "blade_hammer_bone_02" }, new string[0]),
            new Unlock(new[] { "blade_hammer_metal_02" }, new string[0]),
            new Unlock(new[] { "assembled_hammer_one_04", "blade_hammer_bone_03" }, new string[0])
          } },
        { "hammer_tool|__base__", new[]
          {
            new Unlock(new[] { "hammer_tool_bone_01" }, new string[0]),
            new Unlock(new[] { "hammer_tool_metal_01" }, new string[0])
          } },
        { "hammer_twohand|__base__", new[]
          {
            new Unlock(new[] { "assembled_hammer_two_01" }, new string[0]),
            new Unlock(new[] { "assembled_hammer_two_02", "blade_big_hammer_stone_01" }, new string[0]),
            new Unlock(new[] { "blade_big_hammer_bone_01" }, new string[0]),
            new Unlock(new[] { "assembled_hammer_two_03", "blade_big_hammer_metal_01" }, new string[0]),
            new Unlock(new[] { "blade_big_hammer_bone_02" }, new string[0]),
            new Unlock(new[] { "blade_big_hammer_metal_02" }, new string[0]),
            new Unlock(new[] { "assembled_hammer_two_04", "blade_big_hammer_bone_03" }, new string[0])
          } },
        { "handle|__base__", new[]
          {
            new Unlock(new[] { "handle" }, new string[0]),
            new Unlock(new[] { "handle_02", "handle_02_long" }, new string[0]),
            new Unlock(new[] { "handle_03", "handle_03_long" }, new string[0])
          } },
        { "harpoon|__base__", new[]
          {
            new Unlock(new[] { "harpoon_wooden_01" }, new string[0]),
            new Unlock(new[] { "harpoon_bone_01" }, new string[0]),
            new Unlock(new[] { "harpoon_metal_01" }, new string[0])
          } },
        { "hat|__base__", new[]
          {
            new Unlock(new[] { "hat_straw_02" }, new string[0]),
            new Unlock(new[] { "hat_buket" }, new string[0]),
            new Unlock(new[] { "hat_knit" }, new string[0])
          } },
        { "hinge|__base__", new[]
          {
            new Unlock(new[] { "hinge" }, new string[0]),
            new Unlock(new[] { "hinge_metal" }, new string[0])
          } },
        { "hoe|__base__", new[]
          {
            new Unlock(new[] { "hoe_wooden_01" }, new string[0]),
            new Unlock(new[] { "blade_hoe_bone", "hoe_bone_01" }, new string[0]),
            new Unlock(new[] { "blade_hoe_metal", "hoe_metal_01" }, new string[0])
          } },
        { "ice_food|__base__", new[]
          {
            new Unlock(new[] { "ice_drink" }, new string[0]),
            new Unlock(new[] { "shaved_ice" }, new string[0])
          } },
        { "jewel_crafted|__base__", new[]
          {
            new Unlock(new[] { "jewel_polishing" }, new string[0]),
            new Unlock(new[] { "jewel_grinding_01" }, new string[0]),
            new Unlock(new[] { "jewel_craft_extraction", "jewel_craft_eyes" }, new string[0])
          } },
        { "klin|__base__", new[]
          {
            new Unlock(new string[0], new[] { "kiln_01" }),
            new Unlock(new string[0], new[] { "kiln_02" })
          } },
        { "lance|__base__", new[]
          {
            new Unlock(new[] { "assembled_lance_two_02", "blade_lance_stone_01" }, new string[0]),
            new Unlock(new[] { "blade_lance_bone_01" }, new string[0]),
            new Unlock(new[] { "assembled_lance_two_03", "blade_lance_metal_01", "handle_lance_01" }, new string[0]),
            new Unlock(new[] { "blade_lance_bone_02" }, new string[0]),
            new Unlock(new[] { "blade_lance_bone_03" }, new string[0]),
            new Unlock(new[] { "assembled_lance_two_04", "blade_lance_metal_02", "handle_lance_02" }, new string[0])
          } },
        { "lance_bamboo|__base__", new[]
          {
            new Unlock(new[] { "lance_twohand_bamboo" }, new string[0])
          } },
        { "local_food|__base__", new[]
          {
            new Unlock(new[] { "sushi" }, new string[0]),
            new Unlock(new[] { "tokbokki" }, new string[0])
          } },
        { "loom|__base__", new[]
          {
            new Unlock(new string[0], new[] { "loom_01" }),
            new Unlock(new string[0], new[] { "loom_02" })
          } },
        { "make_fur|__base__", new[]
          {
            new Unlock(new[] { "make_fur" }, new string[0])
          } },
        { "mend_ingredient|__base__", new[]
          {
            new Unlock(new[] { "meatball_01" }, new string[0]),
            new Unlock(new[] { "salt", "sugar" }, new string[0]),
            new Unlock(new[] { "powder_grain" }, new string[0]),
            new Unlock(new[] { "juice_01" }, new string[0]),
            new Unlock(new[] { "tea_leaf_process" }, new string[0]),
            new Unlock(new[] { "sashimi" }, new string[0]),
            new Unlock(new[] { "powder_coffee" }, new string[0])
          } },
        { "mend_milk|__base__", new[]
          {
            new Unlock(new[] { "butter" }, new string[0]),
            new Unlock(new[] { "cheese_01" }, new string[0]),
            new Unlock(new[] { "cream" }, new string[0])
          } },
        { "metal_bucket|__base__", new[]
          {
            new Unlock(new[] { "bag_metal_bucket" }, new string[0])
          } },
        { "modular|__base__", new[]
          {
            new Unlock(new string[0], new[] { "roof_modular" })
          } },
        { "modular|house", new[]
          {
            new Unlock(new string[0], new[] { "modular" })
          } },
        { "modular_ground|__base__", new[]
          {
            new Unlock(new[] { "modular_ground_leaf", "modular_ground_stem" }, new string[0]),
            new Unlock(new[] { "modular_ground_dirt", "modular_ground_wood" }, new string[0]),
            new Unlock(new[] { "modular_ground_board_wood", "modular_ground_stone" }, new string[0]),
            new Unlock(new[] { "modular_ground_stone_02", "modular_ground_stone_03", "modular_ground_woodplank_01" }, new string[0])
          } },
        { "modular_roof|__base__", new[]
          {
            new Unlock(new[] { "modular_roof_leaf", "modular_roof_stem" }, new string[0]),
            new Unlock(new[] { "modular_roof_leather", "modular_roof_wood" }, new string[0]),
            new Unlock(new[] { "modular_roof_woodplank_01" }, new string[0])
          } },
        { "modular_stair|__base__", new[]
          {
            new Unlock(new string[0], new[] { "modular_upstair" })
          } },
        { "modular_wall|__base__", new[]
          {
            new Unlock(new[] { "modular_wall_stem" }, new string[0]),
            new Unlock(new[] { "modular_wall_dirt", "modular_wall_wood" }, new string[0]),
            new Unlock(new[] { "modular_wall_board_wood", "modular_wall_stone" }, new string[0]),
            new Unlock(new[] { "modular_wall_stone_02", "modular_wall_stone_03", "modular_wall_woodplank_01" }, new string[0])
          } },
        { "nail_normal|__base__", new[]
          {
            new Unlock(new[] { "nail" }, new string[0])
          } },
        { "parasol|__base__", new[]
          {
            new Unlock(new string[0], new[] { "parasol_fabric_01", "parasol_leather_01", "parasol_stem_01", "parasol_wood_01" })
          } },
        { "pickaxe|__base__", new[]
          {
            new Unlock(new[] { "pickaxe_wooden_01" }, new string[0]),
            new Unlock(new[] { "blade_pickaxe_bone", "pickaxe_bone_01" }, new string[0]),
            new Unlock(new[] { "blade_pickaxe_metal", "pickaxe_metal_01" }, new string[0])
          } },
        { "pillarwork|__base__", new[]
          {
            new Unlock(new[] { "pillar_stone" }, new string[0]),
            new Unlock(new[] { "pillar_metal" }, new string[0])
          } },
        { "process|__base__", new[]
          {
            new Unlock(new[] { "process" }, new string[0]),
            new Unlock(new[] { "process_02" }, new string[0]),
            new Unlock(new[] { "process_03" }, new string[0]),
            new Unlock(new[] { "process_04" }, new string[0])
          } },
        { "refine|__base__", new[]
          {
            new Unlock(new[] { "refine" }, new string[0]),
            new Unlock(new[] { "refine_02" }, new string[0]),
            new Unlock(new[] { "refine_03" }, new string[0])
          } },
        { "reform_breathability|__base__", new[]
          {
            new Unlock(new[] { "reform_breathability" }, new string[0])
          } },
        { "reform_clothover|__base__", new[]
          {
            new Unlock(new[] { "reform_clothover" }, new string[0])
          } },
        { "reform_lightening|__base__", new[]
          {
            new Unlock(new[] { "reform_lightening" }, new string[0])
          } },
        { "reform_nail|__base__", new[]
          {
            new Unlock(new[] { "reform_nail" }, new string[0])
          } },
        { "reform_pocket|__base__", new[]
          {
            new Unlock(new[] { "reform_pocket" }, new string[0])
          } },
        { "reform_protect_heat|__base__", new[]
          {
            new Unlock(new[] { "reform_protect_heat" }, new string[0])
          } },
        { "reform_scales|__base__", new[]
          {
            new Unlock(new[] { "reform_scales" }, new string[0])
          } },
        { "reform_suncover|__base__", new[]
          {
            new Unlock(new[] { "reform_suncover" }, new string[0])
          } },
        { "reform_temper|__base__", new[]
          {
            new Unlock(new[] { "reform_temper" }, new string[0])
          } },
        { "reform_windbreak|__base__", new[]
          {
            new Unlock(new[] { "reform_windbreak" }, new string[0])
          } },
        { "remodel|__base__", new[]
          {
            new Unlock(new[] { "repair_modern_clothes_01" }, new string[0]),
            new Unlock(new[] { "remodel_modern_clothes_01" }, new string[0])
          } },
        { "repair_kit_building|__base__", new[]
          {
            new Unlock(new[] { "artifact_repair_kit_01" }, new string[0]),
            new Unlock(new[] { "artifact_repair_kit_02" }, new string[0]),
            new Unlock(new[] { "artifact_repair_kit_03" }, new string[0])
          } },
        { "repair_kit_clothes|__base__", new[]
          {
            new Unlock(new[] { "clothes_repair_kit_01" }, new string[0]),
            new Unlock(new[] { "clothes_repair_kit_02" }, new string[0]),
            new Unlock(new[] { "clothes_repair_kit_03" }, new string[0])
          } },
        { "repair_kit_tools|__base__", new[]
          {
            new Unlock(new[] { "tool_repair_kit_01" }, new string[0]),
            new Unlock(new[] { "tool_repair_kit_02" }, new string[0]),
            new Unlock(new[] { "tool_repair_kit_03" }, new string[0])
          } },
        { "road|__base__", new[]
          {
            new Unlock(new string[0], new[] { "direction_sign_01" }),
            new Unlock(new string[0], new[] { "road1", "road2" }),
            new Unlock(new string[0], new[] { "road3", "road4" }),
            new Unlock(new string[0], new[] { "road5_basalt", "road5_granite" })
          } },
        { "rope|__base__", new[]
          {
            new Unlock(new[] { "twist_rope" }, new string[0]),
            new Unlock(new[] { "twist_rope_02" }, new string[0])
          } },
        { "s02_armorcrafting_accessory|__base__", new[]
          {
            new Unlock(new[] { "s02_gloves", "s02_shoes" }, new string[0]),
            new Unlock(new[] { "s02_gloves_t2" }, new string[0]),
            new Unlock(new[] { "s02_shoes_t2" }, new string[0])
          } },
        { "s02_armorcrafting_clothes|__base__", new[]
          {
            new Unlock(new[] { "s02_clothes_rope" }, new string[0]),
            new Unlock(new[] { "s02_hat_rope" }, new string[0]),
            new Unlock(new[] { "s02_clothes_plastic" }, new string[0]),
            new Unlock(new[] { "s02_hat_plastic" }, new string[0])
          } },
        { "s02_constructing|__base__", new[]
          {
            new Unlock(new string[0], new[] { "s02_bonfire" })
          } },
        { "s02_constructing_shelter|__base__", new[]
          {
            new Unlock(new string[0], new[] { "s02_shelter_01" }),
            new Unlock(new[] { "s02_board" }, new[] { "s02_bed", "s02_shelter_02" })
          } },
        { "s02_food|__base__", new[]
          {
            new Unlock(new[] { "s02_skewer" }, new string[0]),
            new Unlock(new[] { "s02_worm_stew" }, new string[0]),
            new Unlock(new[] { "s02_cooking" }, new string[0])
          } },
        { "s02_material_process|__base__", new[]
          {
            new Unlock(new[] { "s02_charcoal", "s02_cut_pillar", "s02_extend_stick", "s02_handle", "s02_string_leather" }, new string[0])
          } },
        { "s02_supplies|__base__", new[]
          {
            new Unlock(new[] { "s02_supplies_axe_tool_01", "s02_supplies_pickaxe_01", "s02_supplies_sword_tool_01" }, new string[0])
          } },
        { "s02_tool|__base__", new[]
          {
            new Unlock(new[] { "s02_harpoon_wooden_01" }, new string[0]),
            new Unlock(new[] { "s02_onehand_hammer" }, new string[0]),
            new Unlock(new[] { "s02_bow" }, new string[0])
          } },
        { "sauce|__base__", new[]
          {
            new Unlock(new[] { "sauce_01" }, new string[0]),
            new Unlock(new[] { "sauce_02" }, new string[0]),
            new Unlock(new[] { "sauce_03" }, new string[0])
          } },
        { "saw|__base__", new[]
          {
            new Unlock(new[] { "blade_saw_stone", "saw_stone_01" }, new string[0]),
            new Unlock(new[] { "blade_saw_bone", "saw_bone_01" }, new string[0]),
            new Unlock(new[] { "blade_saw_metal", "saw_metal_01" }, new string[0])
          } },
        { "scale_weaving_striped|__base__", new[]
          {
            new Unlock(new[] { "iguana_scale_to_leather_striped" }, new string[0])
          } },
        { "sewing|__base__", new[]
          {
            new Unlock(new[] { "needle", "thread" }, new string[0]),
            new Unlock(new[] { "thread_02" }, new string[0])
          } },
        { "sheaf|__base__", new[]
          {
            new Unlock(new[] { "sheaf" }, new string[0])
          } },
        { "shelter|__base__", new[]
          {
            new Unlock(new string[0], new[] { "temptent" }),
            new Unlock(new string[0], new[] { "tent" })
          } },
        { "shoes|__base__", new[]
          {
            new Unlock(new[] { "shoes_footwraps" }, new string[0]),
            new Unlock(new[] { "shoes_sandal_straw" }, new string[0]),
            new Unlock(new[] { "shoes_sandal_leather" }, new string[0]),
            new Unlock(new[] { "shoes_moccasin_01" }, new string[0]),
            new Unlock(new[] { "shoes_waterproof" }, new string[0])
          } },
        { "shoes|defense", new[]
          {
            new Unlock(new[] { "shoes_wood" }, new string[0]),
            new Unlock(new[] { "shoes_boots_01" }, new string[0]),
            new Unlock(new[] { "shoes_bone_01" }, new string[0])
          } },
        { "shovel|__base__", new[]
          {
            new Unlock(new[] { "shovel_wooden_01" }, new string[0]),
            new Unlock(new[] { "blade_shovel_bone", "shovel_bone_01" }, new string[0]),
            new Unlock(new[] { "blade_shovel_metal", "shovel_metal_01" }, new string[0])
          } },
        { "smelt|__base__", new[]
          {
            new Unlock(new[] { "combine_metal", "smelt" }, new string[0]),
            new Unlock(new[] { "copper_alloy" }, new string[0])
          } },
        { "sprinkler|__base__", new[]
          {
            new Unlock(new string[0], new[] { "sprinkler_01", "sprinkler_02" }),
            new Unlock(new string[0], new[] { "sprinkler_03" }),
            new Unlock(new string[0], new[] { "sprinkler_03_liquid" })
          } },
        { "sqeeze_oil|__base__", new[]
          {
            new Unlock(new[] { "oil_grains" }, new string[0]),
            new Unlock(new[] { "oil_oily_fruit" }, new string[0])
          } },
        { "string_leather|__base__", new[]
          {
            new Unlock(new[] { "string_leather" }, new string[0])
          } },
        { "sub_compi|__base__", new[]
          {
            new Unlock(new[] { "sub_compi" }, new string[0])
          } },
        { "sword_onehand|__base__", new[]
          {
            new Unlock(new[] { "assembled_sword_one_01" }, new string[0]),
            new Unlock(new[] { "blade_bone" }, new string[0]),
            new Unlock(new[] { "assembled_sword_one_02", "blade_sword_stone_01" }, new string[0]),
            new Unlock(new[] { "blade_sword_bone_01" }, new string[0]),
            new Unlock(new[] { "blade_sword_stone_02" }, new string[0]),
            new Unlock(new[] { "assembled_sword_one_03", "blade_sword_metal_01" }, new string[0]),
            new Unlock(new[] { "blade_sword_bone_02" }, new string[0]),
            new Unlock(new[] { "blade_sword_metal_02" }, new string[0]),
            new Unlock(new[] { "assembled_sword_one_04", "blade_sword_bone_03" }, new string[0])
          } },
        { "sword_twohand|__base__", new[]
          {
            new Unlock(new[] { "assembled_sword_two_01", "blade_big_stone" }, new string[0]),
            new Unlock(new[] { "blade_big_bone" }, new string[0]),
            new Unlock(new[] { "assembled_sword_two_02", "blade_big_sword_stone_01" }, new string[0]),
            new Unlock(new[] { "blade_big_sword_bone_01" }, new string[0]),
            new Unlock(new[] { "blade_big_sword_stone_02" }, new string[0]),
            new Unlock(new[] { "assembled_sword_two_03", "blade_big_sword_metal_01" }, new string[0]),
            new Unlock(new[] { "blade_big_sword_bone_02" }, new string[0]),
            new Unlock(new[] { "blade_big_sword_metal_02" }, new string[0]),
            new Unlock(new[] { "assembled_sword_two_04", "blade_big_sword_bone_03" }, new string[0])
          } },
        { "tan|__base__", new[]
          {
            new Unlock(new[] { "tan" }, new string[0]),
            new Unlock(new[] { "tan_02" }, new string[0]),
            new Unlock(new[] { "tan_03" }, new string[0])
          } },
        { "tool_cooking_cutting_board|__base__", new[]
          {
            new Unlock(new[] { "cutting_board_01" }, new string[0]),
            new Unlock(new[] { "cutting_board_02" }, new string[0])
          } },
        { "tool_cooking_grill_pan|__base__", new[]
          {
            new Unlock(new[] { "grill_stone" }, new string[0]),
            new Unlock(new[] { "frypan_01" }, new string[0]),
            new Unlock(new[] { "grill_metal" }, new string[0])
          } },
        { "tool_cooking_mortar|__base__", new[]
          {
            new Unlock(new[] { "mortar_01" }, new string[0]),
            new Unlock(new[] { "mortar_02" }, new string[0]),
            new Unlock(new[] { "mortar_03" }, new string[0])
          } },
        { "tool_cooking_pot|__base__", new[]
          {
            new Unlock(new[] { "pot_01" }, new string[0]),
            new Unlock(new[] { "pot_02" }, new string[0]),
            new Unlock(new[] { "pot_03" }, new string[0])
          } },
        { "tool_instrument_drum|__base__", new[]
          {
            new Unlock(new[] { "instrument_drum_small02" }, new string[0]),
            new Unlock(new[] { "instrument_drum_big" }, new string[0])
          } },
        { "tool_instrument_guitar|__base__", new[]
          {
            new Unlock(new[] { "instrument_guitar03" }, new string[0]),
            new Unlock(new[] { "instrument_guitar02" }, new string[0])
          } },
        { "tool_instrument_horn|__base__", new[]
          {
            new Unlock(new[] { "instrument_horn01" }, new string[0])
          } },
        { "tool_instrument_melody|__base__", new[]
          {
            new Unlock(new[] { "instrument_synth01" }, new string[0]),
            new Unlock(new[] { "instrument_piano_elec" }, new string[0]),
            new Unlock(new[] { "instrument_piano" }, new string[0])
          } },
        { "tool_simplicity|__base__", new[]
          {
            new Unlock(new[] { "hoe_wooden_00", "shovel_wooden_00" }, new string[0])
          } },
        { "torch|__base__", new[]
          {
            new Unlock(new[] { "torch_01_wall_wood" }, new[] { "torch_stand_01" }),
            new Unlock(new[] { "torch_02_wall_wood" }, new[] { "torch_stand_02" })
          } },
        { "trap|__base__", new[]
          {
            new Unlock(new string[0], new[] { "fishtrap" })
          } },
        { "trap_epic|__base__", new[]
          {
            new Unlock(new string[0], new[] { "leg_trap" })
          } },
        { "trim|__base__", new[]
          {
            new Unlock(new[] { "trim" }, new string[0])
          } },
        { "warp|__base__", new[]
          {
            new Unlock(new string[0], new[] { "cargo_warphole_out" }),
            new Unlock(new string[0], new[] { "cargo_warphole_out2" }),
            new Unlock(new string[0], new[] { "cargo_warphole_out3" }),
            new Unlock(new string[0], new[] { "cargo_warphole_out4" })
          } },
        { "warphole_personal|__base__", new[]
          {
            new Unlock(new string[0], new[] { "warphole_personal" })
          } },
        { "water_bottle|__base__", new[]
          {
            new Unlock(new[] { "bag_water_pouch" }, new string[0]),
            new Unlock(new[] { "bag_water_horn" }, new string[0]),
            new Unlock(new[] { "bag_water_fabric" }, new string[0]),
            new Unlock(new[] { "bag_water_sack" }, new string[0])
          } },
        { "weapon_connection|__base__", new[]
          {
            new Unlock(new[] { "weapon_connection" }, new string[0])
          } },
        { "weaving|__base__", new[]
          {
            new Unlock(new[] { "fabric" }, new string[0]),
            new Unlock(new[] { "fabric_02_01" }, new string[0]),
            new Unlock(new[] { "fabric_02_02" }, new string[0]),
            new Unlock(new[] { "fabric_02_03" }, new string[0])
          } },
        { "well|__base__", new[]
          {
            new Unlock(new string[0], new[] { "well_01" }),
            new Unlock(new string[0], new[] { "well_02" }),
            new Unlock(new string[0], new[] { "well_05" })
          } },
        { "window|__base__", new[]
          {
            new Unlock(new[] { "window_01_wood", "window_02_wood" }, new string[0]),
            new Unlock(new[] { "window_01_bone", "window_02_bone" }, new string[0]),
            new Unlock(new[] { "window_01_woodplank", "window_02_leather", "window_02_wood_leather" }, new string[0])
          } },
        { "worktable|__base__", new[]
          {
            new Unlock(new string[0], new[] { "fur_table" }),
            new Unlock(new string[0], new[] { "fur_table_01" }),
            new Unlock(new string[0], new[] { "fur_table_02" }),
            new Unlock(new string[0], new[] { "fur_table_03" })
          } },
        { "worktable|amor", new[]
          {
            new Unlock(new string[0], new[] { "closet_table_01" })
          } },
        { "worktable|jewel", new[]
          {
            new Unlock(new string[0], new[] { "fur_table_jewel" })
          } },
        { "worktable|medicine", new[]
          {
            new Unlock(new string[0], new[] { "medicine_table_01" })
          } },
        { "worktable|weapon", new[]
          {
            new Unlock(new string[0], new[] { "weapon_table_01" })
          } },
        { "yeast_fungui|__base__", new[]
          {
            new Unlock(new[] { "yeast_fungi_01" }, new string[0]),
            new Unlock(new[] { "yeast_fungi_02" }, new string[0])
          } },
    };

    /// <summary>ของที่สกิลนี้ให้เมื่อเรียนถึงเลเวลที่กำหนด (สะสมตั้งแต่เลเวล 1)</summary>
    public static void Collect(string skillId, string subId, int level, HashSet<string> recipes, HashSet<string> blueprints)
    {
        if (!BySkill.TryGetValue(skillId + "|" + subId, out Unlock[] levels))
        {
            return;
        }
        int upto = level < levels.Length ? level : levels.Length;
        for (int i = 0; i < upto; i++)
        {
            for (int j = 0; j < levels[i].Recipes.Length; j++)
            {
                recipes.Add(levels[i].Recipes[j]);
            }
            for (int j = 0; j < levels[i].Blueprints.Length; j++)
            {
                blueprints.Add(levels[i].Blueprints[j]);
            }
        }
    }

    // [แก้เอง] 25 ส.ค. 2026 — แปลนทั้งหมดที่ "ปลดล็อกด้วยการเรียนสกิล" (รวมจาก BySkill ทุกหมวด)
    // ใช้ตอนซ่อนแท็บเมนูสร้าง: ของสกิลเป็น progression จริง (เตา/โต๊ะ/เตียง/กับดัก) ห้ามซ่อน
    // ต่อให้หมวดของมันอยู่ในลิสต์ที่สั่งซ่อน — ดู RecipeData.FreeBlueprintsInCategories
    private static HashSet<string> _skillGatedBlueprints;
    public static HashSet<string> SkillGatedBlueprints
    {
        get
        {
            if (_skillGatedBlueprints == null)
            {
                var set = new HashSet<string>();
                foreach (Unlock[] levels in BySkill.Values)
                {
                    if (levels == null) continue;
                    foreach (Unlock u in levels)
                    {
                        if (u.Blueprints == null) continue;
                        foreach (string bp in u.Blueprints) set.Add(bp);
                    }
                }
                _skillGatedBlueprints = set;
            }
            return _skillGatedBlueprints;
        }
    }
}
