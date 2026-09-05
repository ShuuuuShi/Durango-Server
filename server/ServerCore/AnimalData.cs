using System.Collections.Generic;

namespace DurangoServer.Core;

/// <summary>
/// เฟส C — ข้อมูลสัตว์ (entity type 2000–2999)
///
/// สกัดอัตโนมัติจาก game/DurangoV2_Data/resources.strings.txt ด้วย scripts/extract_animals.py
/// **อย่าแก้ด้วยมือ** ให้รันสคริปต์ใหม่แทน
///
/// เก็บเฉพาะที่ server ต้องใช้ตอน spawn — ค่าพลังชีวิต/ดาเมจจริงในเกมเป็น "สูตรข้อความ"
/// (เช่น "(0 + combat_level * 5) * unstable_factor") ที่ต้องมี NCalc มาคำนวณ
/// ตอนนี้ยังไม่ได้ทำ จึงใช้ค่าคงที่จาก AnimalSpawner แทน
/// </summary>
public static class AnimalData
{
    public readonly struct AnimalInfo
    {
        public readonly ushort EntityType;
        public readonly string Name;
        public readonly string ModelPath;
        public readonly float Scale;
        public readonly string AiFactorId;
        public readonly bool Tamable;

        /// <summary>
        /// ขนาดตัวจริงจากข้อมูลเกม (size_level 1–7)
        /// 1 = กิ้งก่า/คอมป์โซ · 2 = แร็ปเตอร์/โปรโตเซราท็อปส์ · 4 = สเตโก/ทริเซรา · 7 = ตัวใหญ่สุด
        /// **ไม่ใช่ Scale** — Scale เป็นตัวคูณของ prefab แต่ละโมเดล เทียบข้ามชนิดไม่ได้
        /// (แร็ปเตอร์ Scale 2.2 แต่ตัวเล็กกว่าบราคิโอที่ Scale 1.27)
        /// server ใช้ค่านี้กำหนดว่าตัวใหญ่ต้องเกิดลึกเข้าไปในเกาะแค่ไหน
        /// </summary>
        public readonly int SizeLevel;

        /// <summary>ความยากจากข้อมูลเกม (0.3 = กิ้งก่า … 10 = ทริเซราท็อปส์)</summary>
        public readonly float Difficulty;

        public AnimalInfo(int entityType, string name, string modelPath, float scale, string aiFactorId, bool tamable,
            int sizeLevel, float difficulty)
        {
            EntityType = (ushort)entityType;
            Name = name;
            ModelPath = modelPath;
            Scale = scale;
            AiFactorId = aiFactorId;
            Tamable = tamable;
            SizeLevel = sizeLevel;
            Difficulty = difficulty;
        }
    }

    /// <summary>สัตว์ 213 ชนิด</summary>
    public static readonly Dictionary<ushort, AnimalInfo> All = new Dictionary<ushort, AnimalInfo>
    {
        { 2000, new AnimalInfo(2000, "스테고사우루스", "Stegosaurus/StegosaurusPrefab", 1.5f, "stegosaurus_ai", false, 4, 5.0f) },
        { 2001, new AnimalInfo(2001, "랩터", "Raptor/RaptorPrefab", 2.2f, "raptor_ai", true, 2, 1.0f) },
        { 2002, new AnimalInfo(2002, "오비랍토르", "Raptor/OviraptorPrefab", 1.8f, "oviraptor_ai", true, 2, 1.0f) },
        { 2003, new AnimalInfo(2003, "트리케라톱스", "Tricera/TriceratopsPrefab", 1.0f, "triceratops_ai", true, 4, 10.0f) },
        { 2004, new AnimalInfo(2004, "브라키오사우루스", "Brachio/BrachioPrefab", 1.27f, "brachio_ai", false, 6, 10.0f) },
        { 2005, new AnimalInfo(2005, "티라노사우루스", "Trex/TRexPrefab", 1.0f, "trex_ai", false, 7, 39.0f) },
        { 2006, new AnimalInfo(2006, "페나코두스", "Phenacodus/PhenacodusPrefab", 1.2f, "phenacodus_ai", false, 1, 1.0f) },
        { 2007, new AnimalInfo(2007, "스밀로돈", "Sabertooth/SabertoothPrefab", 1.8f, "sabertooth_ai", true, 4, 3.0f) },
        { 2008, new AnimalInfo(2008, "매머드", "Mammoth/MammothPrefab", 1.0f, "mammoth_ai", false, 5, 10.0f) },
        { 2009, new AnimalInfo(2009, "파라사우롤로푸스", "Parasaurolophus/ParasaurolophusPrefab", 0.87f, "parasaurolophus_ai", true, 4, 3.0f) },
        { 2010, new AnimalInfo(2010, "안킬로사우루스", "Ankylosaurus/AnkylosaurusPrefab", 1.2f, "ankylosaurus_ai", true, 4, 10.0f) },
        { 2011, new AnimalInfo(2011, "유오플로케팔루스", "Ankylosaurus/EuoplocephalusPrefab", 1.2f, "euoplocephalus_ai", true, 4, 10.0f) },
        { 2012, new AnimalInfo(2012, "마크라우케니아", "Macrauchenia/MacraucheniaPrefab", 0.82f, "macrauchenia_ai", true, 2, 1.0f) },
        { 2013, new AnimalInfo(2013, "수컷 메갈로케로스", "Macrauchenia/MegalocerosPrefab", 1.24f, "megaloceros_ai", true, 3, 3.0f) },
        { 2014, new AnimalInfo(2014, "오르니토미무스", "Ornithomimus/OrnithomimusPrefab", 1.0f, "ornithomimus_ai", true, 3, 3.0f) },
        { 2015, new AnimalInfo(2015, "콤프소그나투스", "Compso/CompsognathusPrefab", 0.25f, "compsognathus_ai", false, 1, 0.3f) },
        { 2016, new AnimalInfo(2016, "코엘로피시스", "Ornithomimus/CoelophysisPrefab", 0.55f, "coelophysis_ai", true, 2, 1.0f) },
        { 2017, new AnimalInfo(2017, "프로토케라톱스", "Tricera/ProtoceratopsPrefab", 0.45f, "protoceratops_ai", true, 2, 1.0f) },
        { 2018, new AnimalInfo(2018, "카스모사우루스", "Tricera/ChasmosaurusPrefab", 0.82f, "chasmosaurus_ai", false, 4, 15.0f) },
        { 2019, new AnimalInfo(2019, "스티라코사우루스", "Tricera/StyracosaurusPrefab", 0.88f, "styracosaurus_ai", true, 4, 5.0f) },
        { 2020, new AnimalInfo(2020, "다이어울프", "Direwolf/DirewolfPrefab", 1.0f, "direwolf_ai", true, 2, 3.0f) },
        { 2021, new AnimalInfo(2021, "알로사우루스", "Allosaurus/AllosaurusPrefab", 1.0f, "allosaurus_ai", false, 5, 15.0f) },
        { 2022, new AnimalInfo(2022, "파키케팔로사우루스", "Pachycephalo/PachycephalosaurusPrefab", 0.67f, "pachycephalosaurus_ai", false, 4, 5.0f) },
        { 2023, new AnimalInfo(2023, "유타랍토르", "Raptor/UtahraptorPrefab", 2.6f, "utahraptor_ai", true, 2, 1.0f) },
        { 2024, new AnimalInfo(2024, "딜로포사우루스", "Raptor/DilophosaurusPrefab", 2.4f, "dilophosaurus_ai", true, 2, 3.0f) },
        { 2025, new AnimalInfo(2025, "갈리미무스", "Ornithomimus/GallimimusPrefab", 0.85f, "gallimimus_ai", true, 3, 3.0f) },
        { 2026, new AnimalInfo(2026, "투오지앙고사우루스", "Stegosaurus/TuojiangosaurusPrefab", 1.72f, "tuojiangosaurus_ai", false, 4, 5.0f) },
        { 2027, new AnimalInfo(2027, "제브라케라톱스", "Tricera/ZebraceratopsPrefab", 1.0f, "protoceratops_ai", true, 2, 1.0f) },
        { 2028, new AnimalInfo(2028, "사우롤로푸스", "Parasaurolophus/SaurolophusPrefab", 1.0f, "parasaurolophus_ai", false, 4, 3.0f) },
        { 2029, new AnimalInfo(2029, "데이노니쿠스", "Raptor/DeinonychusPrefab", 1.0f, "raptor_ai", true, 2, 1.0f) },
        { 2030, new AnimalInfo(2030, "코리토사우루스", "Parasaurolophus/CorythosaurusPrefab", 1.0f, "parasaurolophus_ai", false, 4, 3.0f) },
        { 2031, new AnimalInfo(2031, "에드몬토사우루스", "Parasaurolophus/EdmontosaurusPrefab", 1.0f, "parasaurolophus_ai", false, 4, 3.0f) },
        { 2032, new AnimalInfo(2032, "스컹코두스", "Phenacodus/SkunkodusPrefab", 1.0f, "phenacodus_ai", true, 1, 1.0f) },
        { 2033, new AnimalInfo(2033, "도도피시스", "Compso/DodophysisPrefab", 1.0f, "compsognathus_ai", false, 1, 1.0f) },
        { 2034, new AnimalInfo(2034, "디메트로돈", "Dimetrodon/DimetrodonPrefab", 1.0f, "dimetrodon_ai", false, 3, 1.0f) },
        { 2035, new AnimalInfo(2035, "도도피시스", "Compso/Dodo_snowfieldPrefab", 1.0f, "compsognathus_ai", false, 1, 1.0f) },
        { 2036, new AnimalInfo(2036, "줄무늬 콤프소그나투스", "Compso/Compsognathus_AncoraPrefab", 0.25f, "compsognathus_ai", false, 1, 0.3f) },
        { 2037, new AnimalInfo(2037, "엘리펀툴루스", "Phenacodus/Phenacodus_savanaPrefab", 1.0f, "phenacodus_ai", false, 1, 0.3f) },
        { 2038, new AnimalInfo(2038, "거대 쥐", "Phenacodus/Phenacodus_ratPrefab", 1.0f, "phenacodus_ai", false, 1, 1.0f) },
        { 2039, new AnimalInfo(2039, "아케니쿠스", "Raptor/Deinonychus_savanaPrefab", 1.0f, "deinonychus_savana_ai", false, 2, 1.0f) },
        { 2040, new AnimalInfo(2040, "연분홍 프로토케라톱스", "Tricera/Protoceratops_TBprefab", 0.45f, "protoceratops_ai", true, 2, 1.0f) },
        { 2041, new AnimalInfo(2041, "센트로사우루스", "Tricera/CentrosaurusPrefab", 0.45f, "protoceratops_ai", true, 2, 5.0f) },
        { 2042, new AnimalInfo(2042, "도마뱀", "Lizard/Lizard_savanaPrefab", 0.15f, "lizard_ai", false, 1, 0.3f) },
        { 2043, new AnimalInfo(2043, "모래 도마뱀", "Lizard/Lizard_sandPrefab", 0.15f, "lizard_ai", false, 1, 0.3f) },
        { 2044, new AnimalInfo(2044, "붉은점도마뱀", "Dimetrodon/Lizard_poisonousPrefab", 1.0f, "dimetrodon_ai", false, 3, 1.0f) },
        { 2045, new AnimalInfo(2045, "코엘로피시스", "Ornithomimus/Coelophysis_swampPrefab", 0.55f, "coelophysis_ai", false, 2, 1.0f) },
        { 2046, new AnimalInfo(2046, "하드로사우루스", "Parasaurolophus/HadrosaurusPrefab", 1.0f, "parasaurolophus_ai", false, 4, 3.0f) },
        { 2048, new AnimalInfo(2048, "이구아노돈", "Parasaurolophus/IguanodonPrefab", 1.0f, "parasaurolophus_ai", false, 4, 3.0f) },
        { 2049, new AnimalInfo(2049, "한복 페나코두스", "Phenacodus/Phenacodus_hanbokPrefab", 1.2f, "phenacodus_ai", true, 1, 1.0f) },
        { 2050, new AnimalInfo(2050, "마녀 페나코두스", "Phenacodus/Phenacodus_witchPrefab", 1.2f, "phenacodus_ai", true, 1, 1.0f) },
        { 2051, new AnimalInfo(2051, "겁쟁이 랩터", "Raptor/Raptor_cowardPrefab", 1.8f, "raptor_ai", false, 2, 1.0f) },
        { 2052, new AnimalInfo(2052, "호박마녀 페나코두스", "Phenacodus/Phenacodus_pumpkinwitchPrefab", 1.2f, "phenacodus_ai", true, 1, 1.0f) },
        { 2053, new AnimalInfo(2053, "루돌프 메갈로케로스", "Macrauchenia/Megaloceros_xmas_elitePrefab", 1.5f, "megaloceros_ai", true, 3, 15.0f) },
        { 2054, new AnimalInfo(2054, "땅거북안킬로", "Ankylosaurus/Tortoise_AnkylosaurusPrefab", 1.2f, "ankylosaurus_ai", true, 4, 10.0f) },
        { 2055, new AnimalInfo(2055, "다이어울프", "Direwolf/DirewolfPrefab", 1.0f, "direwolf_ai", true, 2, 3.0f) },
        { 2056, new AnimalInfo(2056, "마크라우케니아", "Macrauchenia/MacraucheniaPrefab", 0.82f, "macrauchenia_ai", true, 2, 1.0f) },
        { 2057, new AnimalInfo(2057, "수컷 메갈로케로스", "Macrauchenia/MegalocerosPrefab", 1.24f, "megaloceros_ai", true, 3, 3.0f) },
        { 2058, new AnimalInfo(2058, "코엘로피시스", "Ornithomimus/CoelophysisPrefab", 0.55f, "coelophysis_ai", false, 2, 1.0f) },
        { 2059, new AnimalInfo(2059, "파키케팔로사우루스", "Pachycephalo/PachycephalosaurusPrefab", 0.67f, "pachycephalosaurus_ai", false, 4, 5.0f) },
        { 2060, new AnimalInfo(2060, "스컹코두스", "Phenacodus/SkunkodusPrefab", 1.0f, "phenacodus_ai", true, 1, 1.0f) },
        { 2061, new AnimalInfo(2061, "스테고사우루스", "Stegosaurus/StegosaurusPrefab", 1.5f, "stegosaurus_ai", false, 4, 5.0f) },
        { 2062, new AnimalInfo(2062, "트리케라톱스", "Tricera/TriceratopsPrefab", 1.0f, "triceratops_ai", false, 4, 10.0f) },
        { 2063, new AnimalInfo(2063, "암컷 메갈로케로스", "Macrauchenia/MegalocerosFPrefab", 1.0f, "megaloceros_female_ai", false, 2, 1.0f) },
        { 2064, new AnimalInfo(2064, "어린 메갈로케로스", "Macrauchenia/Megaloceros_smallPrefab", 0.65f, "megaloceros_baby_ai", false, 1, 0.3f) },
        { 2065, new AnimalInfo(2065, "어린 트리케라톱스", "Tricera/Triceratops_smallPrefab", 0.55f, "triceratops_baby_ai", false, 2, 5.0f) },
        { 2066, new AnimalInfo(2066, "어린 브라키오사우루스", "Brachio/Brachio_smallPrefab", 0.25f, "brachio_baby_ai", false, 2, 5.0f) },
        { 2067, new AnimalInfo(2067, "어린 안킬로사우루스", "Ankylosaurus/Ankylosaurus_smallPrefab", 0.45f, "ankylosaurus_baby_ai", false, 1, 5.0f) },
        { 2068, new AnimalInfo(2068, "어린 유오플로케팔루스", "Ankylosaurus/Euoplocephalus_smallPrefab", 0.4f, "euoplocephalus_baby_ai", false, 1, 5.0f) },
        { 2069, new AnimalInfo(2069, "다이어울프", "Direwolf/Direwolf_blackPrefab", 1.0f, "direwolf_ai", false, 2, 3.0f) },
        { 2070, new AnimalInfo(2070, "흰털다이어울프", "Direwolf/Direwolf_snowPrefab", 1.0f, "direwolf_ai", false, 2, 3.0f) },
        { 2071, new AnimalInfo(2071, "어린 다이어울프", "Direwolf/Direwolf_smallPrefab", 0.5f, "direwolf_baby_ai", false, 1, 1.0f) },
        { 2072, new AnimalInfo(2072, "어린 카스모사우루스", "Tricera/Chasmosaurus_smallPrefab", 0.45f, "chasmosaurus_baby_ai", false, 2, 15.0f) },
        { 2073, new AnimalInfo(2073, "어린 스티라코사우루스", "Tricera/Styracosaurus_smallPrefab", 0.48f, "styracosaurus_baby_ai", false, 2, 3.0f) },
        { 2074, new AnimalInfo(2074, "어린 투오지앙고사우루스", "Stegosaurus/Tuojiangosaurus_smallPrefab", 0.85f, "tuojiangosaurus_baby_ai", false, 2, 3.0f) },
        { 2075, new AnimalInfo(2075, "어린 스테고사우루스", "Stegosaurus/Stegosaurus_smallPrefab", 0.8f, "stegosaurus_baby_ai", false, 2, 3.0f) },
        { 2076, new AnimalInfo(2076, "어린 갈리미무스", "Ornithomimus/Gallimimus_smallPrefab", 0.45f, "gallimimus_baby_ai", false, 1, 1.0f) },
        { 2077, new AnimalInfo(2077, "알비랍토르", "Raptor/Oviraptor_snowyPrefab", 1.8f, "oviraptor_snowy_ai", true, 2, 1.0f) },
        { 2078, new AnimalInfo(2078, "픽타랍토르", "Raptor/Oviraptor_tropicalPrefab", 1.8f, "oviraptor_snowy_ai", true, 2, 1.0f) },
        { 2079, new AnimalInfo(2079, "수컷 흰배메갈로케로스", "Macrauchenia/Megaloceros_snowyPrefab", 1.24f, "megaloceros_ai", true, 3, 3.0f) },
        { 2080, new AnimalInfo(2080, "암컷 흰배메갈로케로스", "Macrauchenia/Megaloceros_snowyFPrefab", 1.0f, "megaloceros_female_ai", false, 2, 1.0f) },
        { 2081, new AnimalInfo(2081, "어린 흰배메갈로케로스", "Macrauchenia/Megaloceros_snowy_smallPrefab", 0.65f, "megaloceros_baby_ai", false, 1, 0.3f) },
        { 2082, new AnimalInfo(2082, "케라토사우루스", "Allosaurus/CeratosaurusPrefab", 0.8f, "trex_ai", false, 5, 15.0f) },
        { 2083, new AnimalInfo(2083, "켄트로사우루스", "Stegosaurus/KentrosaurusPrefab", 1.2f, "stegosaurus_ai", false, 4, 5.0f) },
        { 2084, new AnimalInfo(2084, "어린 켄트로사우루스", "Stegosaurus/Kentrosaurus_smallPrefab", 0.7f, "kentrosaurus_baby_ai", false, 2, 3.0f) },
        { 2085, new AnimalInfo(2085, "어린 오르니토미무스", "Ornithomimus/Ornithomimus_smallPrefab", 0.5f, "gallimimus_baby_ai", false, 2, 1.0f) },
        { 2086, new AnimalInfo(2086, "타르보사우루스", "Trex/TarbosaurusPrefab", 0.87f, "trex_ai", false, 5, 15.0f) },
        { 2087, new AnimalInfo(2087, "아랫뿔코끼리", "Mammoth/DeinotheriumPrefab", 1.3f, "mammoth_ai", false, 5, 10.0f) },
        { 2088, new AnimalInfo(2088, "스밀로돈", "Sabertooth/Sabertooth_goldenPrefab", 1.8f, "sabertooth_ai", true, 4, 3.0f) },
        { 2089, new AnimalInfo(2089, "타르보사우루스", "Trex/Tarbosaurus_snowyPrefab", 0.87f, "trex_ai", true, 5, 15.0f) },
        { 2090, new AnimalInfo(2090, "설원 코엘로피시스", "Ornithomimus/Coelophysis_snowfieldPrefab", 0.55f, "coelophysis_ai", true, 2, 1.0f) },
        { 2091, new AnimalInfo(2091, "흰털다이어울프", "Direwolf/Direwolf_snowPrefab", 1.0f, "direwolf_ai", false, 2, 3.0f) },
        { 2092, new AnimalInfo(2092, "수컷 흰배메갈로케로스", "Macrauchenia/Megaloceros_snowyPrefab", 1.24f, "megaloceros_ai", true, 3, 3.0f) },
        { 2093, new AnimalInfo(2093, "파보미무스", "Ornithomimus/Gallimimus_birdyPrefab", 0.85f, "gallimimus_ai", true, 2, 3.0f) },
        { 2094, new AnimalInfo(2094, "보누사우루스", "Tricera/Styracosaurus_deerPrefab", 0.88f, "bonusaurus_ai", false, 2, 5.0f) },
        { 2095, new AnimalInfo(2095, "우두머리 페나코두스", "Phenacodus/PhenacodusPrefab", 2.0f, "phenacodus_ai", false, 2, 15.0f) },
        { 2096, new AnimalInfo(2096, "센트로사우루스", "Tricera/CentrosaurusPrefab", 0.45f, "protoceratops_ai", true, 2, 5.0f) },
        { 2097, new AnimalInfo(2097, "우두머리 오르니토미무스", "Ornithomimus/OrnithomimusPrefab", 1.25f, "ornithomimus_ai", false, 3, 15.0f) },
        { 2098, new AnimalInfo(2098, "우두머리 트리케라톱스", "Tricera/Triceratops_elitePrefab", 1.15f, "triceratops_ai", false, 4, 15.0f) },
        { 2099, new AnimalInfo(2099, "우두머리 프로토케라톱스", "Tricera/ProtoceratopsPrefab", 0.6f, "protoceratops_ai", false, 2, 15.0f) },
        { 2100, new AnimalInfo(2100, "우두머리 유타랍토르", "Raptor/UtahraptorPrefab", 3.0f, "utahraptor_ai", false, 2, 15.0f) },
        { 2101, new AnimalInfo(2101, "우두머리 파라사우롤로푸스", "Parasaurolophus/ParasaurolophusPrefab", 1.0f, "parasaurolophus_ai", false, 4, 15.0f) },
        { 2102, new AnimalInfo(2102, "우두머리 데이노테리움", "Mammoth/DeinotheriumPrefab", 1.5f, "mammoth_ai", false, 5, 15.0f) },
        { 2103, new AnimalInfo(2103, "우두머리 메갈로케로스", "Macrauchenia/Megaloceros_elitePrefab", 1.5f, "megaloceros_ai", false, 3, 15.0f) },
        { 2104, new AnimalInfo(2104, "우두머리 다이어울프", "Direwolf/DirewolfPrefab", 1.25f, "direwolf_ai", false, 2, 15.0f) },
        { 2105, new AnimalInfo(2105, "우두머리 스티라코사우루스", "Tricera/StyracosaurusPrefab", 1.1f, "styracosaurus_ai", false, 4, 15.0f) },
        { 2106, new AnimalInfo(2106, "우두머리 보누사우루스", "Tricera/Styracosaurus_deerPrefab", 1.1f, "bonusaurus_ai", false, 2, 15.0f) },
        { 2107, new AnimalInfo(2107, "우두머리 카스모사우루스", "Tricera/ChasmosaurusPrefab", 1.05f, "chasmosaurus_ai", false, 4, 15.0f) },
        { 2108, new AnimalInfo(2108, "우두머리 콤프소그나투스", "Compso/CompsognathusPrefab", 0.4f, "compsognathus_ai", false, 1, 15.0f) },
        { 2109, new AnimalInfo(2109, "우두머리 매머드", "Mammoth/mammothPrefab", 1.23f, "mammoth_ai", false, 5, 15.0f) },
        { 2110, new AnimalInfo(2110, "우두머리 랩터", "Raptor/RaptorPrefab", 2.8f, "raptor_ai", false, 2, 15.0f) },
        { 2111, new AnimalInfo(2111, "우두머리 스밀로돈", "Sabertooth/Sabertooth_elitePrefab", 2.2f, "sabertooth_ai", false, 4, 15.0f) },
        { 2112, new AnimalInfo(2112, "우두머리 스테고사우루스", "Stegosaurus/StegosaurusPrefab", 1.85f, "stegosaurus_ai", false, 4, 15.0f) },
        { 2113, new AnimalInfo(2113, "우두머리 설원 타르보사우루스", "Trex/Tarbosaurus_snowyPrefab", 1.03f, "trex_ai", false, 5, 15.0f) },
        { 2114, new AnimalInfo(2114, "우두머리 티라노사우루스", "Trex/TRexPrefab", 1.12f, "trex_ai", false, 7, 15.0f) },
        { 2115, new AnimalInfo(2115, "우두머리 타르보사우루스", "Trex/TarbosaurusPrefab", 1.03f, "trex_ai", false, 5, 15.0f) },
        { 2116, new AnimalInfo(2116, "우두머리 투오지앙고사우루스", "Stegosaurus/TuojiangosaurusPrefab", 2.0f, "tuojiangosaurus_ai", false, 4, 15.0f) },
        { 2117, new AnimalInfo(2117, "우두머리 켄트로사우루스", "Stegosaurus/KentrosaurusPrefab", 1.5f, "stegosaurus_ai", false, 4, 15.0f) },
        { 2118, new AnimalInfo(2118, "우두머리 갈리미무스", "Ornithomimus/GallimimusPrefab", 1.05f, "gallimimus_ai", false, 3, 15.0f) },
        { 2119, new AnimalInfo(2119, "우두머리 파보미무스", "Ornithomimus/Gallimimus_birdyPrefab", 1.05f, "gallimimus_ai", false, 2, 15.0f) },
        { 2120, new AnimalInfo(2120, "우두머리 코엘로피시스", "Ornithomimus/CoelophysisPrefab", 0.7f, "coelophysis_ai", false, 2, 15.0f) },
        { 2121, new AnimalInfo(2121, "우두머리 열대 스밀로돈", "Sabertooth/Sabertooth_goldenPrefab", 2.2f, "sabertooth_ai", false, 4, 15.0f) },
        { 2122, new AnimalInfo(2122, "우두머리 딜로포사우루스", "Raptor/DilophosaurusPrefab", 2.9f, "dilophosaurus_ai", false, 2, 15.0f) },
        { 2123, new AnimalInfo(2123, "우두머리 마크라우케니아", "Macrauchenia/MacraucheniaPrefab", 1.0f, "macrauchenia_ai", false, 2, 15.0f) },
        { 2124, new AnimalInfo(2124, "우두머리 브라키오사우루스", "Brachio/BrachioPrefab", 1.27f, "brachio_ai", false, 6, 15.0f) },
        { 2125, new AnimalInfo(2125, "우두머리 안킬로사우루스", "Ankylosaurus/Ankylosaurus_elitePrefab", 1.55f, "ankylosaurus_ai", false, 4, 15.0f) },
        { 2126, new AnimalInfo(2126, "우두머리 유오플로케팔루스", "Ankylosaurus/EuoplocephalusPrefab", 1.55f, "euoplocephalus_ai", false, 4, 15.0f) },
        { 2127, new AnimalInfo(2127, "붉은코 메갈로케로스", "Macrauchenia/Megaloceros_xmasPrefab", 1.24f, "megaloceros_ai", true, 3, 15.0f) },
        { 2128, new AnimalInfo(2128, "루돌프 메갈로케로스", "Macrauchenia/Megaloceros_xmas_elitePrefab", 1.5f, "megaloceros_ai", true, 3, 15.0f) },
        { 2129, new AnimalInfo(2129, "산타 페나코두스", "Phenacodus/Phenacodus_xmasPrefab", 1.2f, "phenacodus_ai", true, 1, 1.0f) },
        { 2130, new AnimalInfo(2130, "가스토르니스", "Ornithomimus/GastronisPrefab", 1.0f, "ornithomimus_ai", false, 3, 3.0f) },
        { 2131, new AnimalInfo(2131, "래브라도 리트리버", "Dog/LabradorPrefab", 1.0f, "direwolf_ai", false, 2, 3.0f) },
        { 2132, new AnimalInfo(2132, "땅거북안킬로", "Ankylosaurus/Tortoise_AnkylosaurusPrefab", 1.2f, "ankylosaurus_ai", false, 4, 10.0f) },
        { 2133, new AnimalInfo(2133, "아파토사우루스", "Apatosaurus/ApatosaurusPrefab", 1.0f, "apatosaurus_ai", false, 7, 30.0f) },
        { 2134, new AnimalInfo(2134, "징병관", "HumanFemale/HumanFemale", 0.25f, "recruiter_common_ai", false, 1, 3.0f) },
        { 2135, new AnimalInfo(2135, "징병관", "HumanMale/HumanMale", 0.25f, "recruiter_common_ai", false, 1, 3.0f) },
        { 2136, new AnimalInfo(2136, "징병대장", "HumanFemale/HumanFemale", 0.25f, "recruiter_lead_ai", false, 1, 5.0f) },
        { 2137, new AnimalInfo(2137, "징병대장", "HumanMale/HumanMale", 0.25f, "recruiter_lead_ai", false, 1, 5.0f) },
        { 2138, new AnimalInfo(2138, "흰털스밀로돈", "Sabertooth/SabertoothPrefab", 1.8f, "sabertooth_ai", false, 4, 3.0f) },
        { 2139, new AnimalInfo(2139, "수상한 수박", "Watermelon/WatermelonPrefab", 0.25f, "watermelon_ai", false, 1, 0.0f) },
        { 2140, new AnimalInfo(2140, "취한 스컹코두스", "Phenacodus/SkunkodusPrefab", 1.0f, "phenacodus_ai", false, 1, 1.0f) },
        { 2141, new AnimalInfo(2141, "병든 거대 쥐", "Phenacodus/Phenacodus_ratPrefab", 1.0f, "phenacodus_ai", false, 1, 1.0f) },
        { 2142, new AnimalInfo(2142, "다친 코엘로피시스", "Ornithomimus/Coelophysis_swampPrefab", 0.55f, "coelophysis_ai", false, 2, 2.0f) },
        { 2143, new AnimalInfo(2143, "기름독 아파토사우루스", "Apatosaurus/ApatosaurusPrefab", 1.0f, "apatosaurus_ai", false, 7, 15.0f) },
        { 2144, new AnimalInfo(2144, "중독된 트리케라톱스", "Tricera/TriceratopsPrefab", 1.0f, "triceratops_ai", false, 4, 5.0f) },
        { 2145, new AnimalInfo(2145, "흥분한 트리케라톱스", "Tricera/TriceratopsPrefab", 1.0f, "triceratops_ai", false, 4, 5.0f) },
        { 2146, new AnimalInfo(2146, "설원 코엘로피시스", "Ornithomimus/Coelophysis_snowfieldPrefab", 0.55f, "coelophysis_ai", true, 2, 1.0f) },
        { 2147, new AnimalInfo(2147, "스티라코사우루스", "Tricera/StyracosaurusPrefab", 0.88f, "styracosaurus_ai", true, 4, 5.0f) },
        { 2148, new AnimalInfo(2148, "붉은코 메갈로케로스", "Macrauchenia/Megaloceros_xmasPrefab", 1.24f, "megaloceros_ai", true, 3, 15.0f) },
        { 2149, new AnimalInfo(2149, "까치 가스토르니스", "Ornithomimus/Gastronis_kkachiPrefab", 1.0f, "ornithomimus_ai", false, 3, 3.0f) },
        { 2150, new AnimalInfo(2150, "흰 코끼리", "Mammoth/elephant_Albino_smallprefab", 0.65f, "mammoth_ai", false, 5, 3.0f) },
        { 2151, new AnimalInfo(2151, "흰 코끼리", "Mammoth/elephant_Albino_smallprefab", 0.65f, "mammoth_ai", false, 5, 3.0f) },
        { 2152, new AnimalInfo(2152, "깜장 래브라도 리트리버", "Dog/Labrador_blackPrefab", 1.0f, "direwolf_ai", false, 2, 3.0f) },
        { 2153, new AnimalInfo(2153, "아프리카 코끼리", "Mammoth/elephant_small_storeprefab", 0.65f, "mammoth_ai", false, 5, 3.0f) },
        { 2154, new AnimalInfo(2154, "아프리카 코끼리", "Mammoth/elephant_small_storeprefab", 0.65f, "mammoth_ai", false, 5, 3.0f) },
        { 2155, new AnimalInfo(2155, "대장 콤프소그나투스", "Compso/Compso_paper_guardPrefab", 0.38f, "compsognathus_ai", false, 1, 1.0f) },
        { 2156, new AnimalInfo(2156, "꿀벌 페나코두스", "Phenacodus/Phenacodus_bee", 1.2f, "phenacodus_ai", true, 1, 1.0f) },
        { 2157, new AnimalInfo(2157, "스밀로돈", "Sabertooth/Sabertooth_unstablePrefab", 1.8f, "sabertooth_ai", true, 4, 3.0f) },
        { 2158, new AnimalInfo(2158, "콤프소그나투스", "Compso/Compso_paper_followerPrefab", 0.3f, "compsognathus_ai", false, 1, 1.0f) },
        { 2159, new AnimalInfo(2159, "대장 페나코두스", "Phenacodus/Phenacodus_paper_guardPrefab", 1.3f, "phenacodus_ai", false, 1, 1.0f) },
        { 2160, new AnimalInfo(2160, "페나코두스", "Phenacodus/Phenacodus_paper_followerPrefab", 1.2f, "phenacodus_ai", false, 1, 1.0f) },
        { 2161, new AnimalInfo(2161, "데이노니쿠스", "Raptor/Deinonychus_unstablePrefab", 1.0f, "raptor_ai", true, 2, 1.0f) },
        { 2162, new AnimalInfo(2162, "갈리미무스", "Ornithomimus/Gallimimus_unstablePrefab", 0.85f, "gallimimus_ai", true, 3, 3.0f) },
        { 2163, new AnimalInfo(2163, "어린 갈리미무스", "Ornithomimus/Gallimimus_smallunstablePrefab", 0.45f, "gallimimus_baby_ai", false, 1, 1.0f) },
        { 2164, new AnimalInfo(2164, "수컷 메갈로케로스", "Macrauchenia/Megaloceros_unstablePrefab", 1.24f, "megaloceros_ai", true, 3, 3.0f) },
        { 2165, new AnimalInfo(2165, "어린 메갈로케로스", "Macrauchenia/Megaloceros_smallunstablePrefab", 0.65f, "megaloceros_baby_ai", false, 1, 0.3f) },
        { 2166, new AnimalInfo(2166, "암컷 메갈로케로스", "Macrauchenia/MegalocerosFunstablePrefab", 1.0f, "megaloceros_female_ai", false, 2, 1.0f) },
        { 2167, new AnimalInfo(2167, "스컹코두스", "Phenacodus/Skunkodus_unstablePrefab", 1.0f, "phenacodus_ai", true, 1, 1.0f) },
        { 2168, new AnimalInfo(2168, "센트로사우루스", "Tricera/CentrosaurusPrefab", 0.45f, "protoceratops_ai", true, 2, 5.0f) },
        { 2169, new AnimalInfo(2169, "작은 브라키오사우루스", "Brachio_junior/Brachio_junior", 0.25f, "brachio_baby_ai", false, 2, 5.0f) },
        { 2170, new AnimalInfo(2170, "앤드루사르쿠스", "Andrewsarchus/AndrewsarchusPrefab", 0.45f, "andrewsarchus_ai", false, 2, 1.0f) },
        { 2171, new AnimalInfo(2171, "투구게", "HorseshoeCrab/HorseshoeCrabPrefab", 0.45f, "horseshoecrab_ai", false, 2, 0.3f) },
        { 2172, new AnimalInfo(2172, "불점박이 이구아나", "Iguana/Iguana_spotted", 1.0f, "iguana_ai", false, 3, 3.0f) },
        { 2173, new AnimalInfo(2173, "해녀 스컹코두스", "Phenacodus/skunkodus_diver", 1.0f, "phenacodus_ai", true, 1, 1.0f) },
        { 2174, new AnimalInfo(2174, "유황 스컹코두스", "Phenacodus/Skunkodus_volcanic", 1.0f, "phenacodus_ai", true, 1, 1.0f) },
        { 2175, new AnimalInfo(2175, "검은머리 유타랍토르", "Raptor/Utahraptor_dark", 2.6f, "utahraptor_ai", false, 2, 1.0f) },
        { 2176, new AnimalInfo(2176, "흰줄무늬 이구아나", "Iguana/Iguana_dark", 1.0f, "iguana_ai", false, 3, 3.0f) },
        { 2177, new AnimalInfo(2177, "용암 하드로사우루스", "Parasaurolophus/Hadrosaurus_lava", 1.0f, "parasaurolophus_ai", true, 4, 3.0f) },
        { 2178, new AnimalInfo(2178, "화산재 카스모사우루스", "Tricera/Chasmosaurus_rocky", 0.82f, "chasmosaurus_ai", true, 4, 5.0f) },
        { 2179, new AnimalInfo(2179, "아마르가사우루스", "Apatosaurus/AmargasaurusPrefab", 1.0f, "apatosaurus_ai", false, 7, 10.0f) },
        { 2180, new AnimalInfo(2180, "축제 엘리펀툴루스", "Phenacodus/phenacodus_savana_hulaPrefab", 1.0f, "phenacodus_ai", true, 1, 0.3f) },
        { 2181, new AnimalInfo(2181, "상어 페나코두스", "Phenacodus/phenacodus_shark", 1.2f, "phenacodus_ai", true, 1, 1.0f) },
        { 2182, new AnimalInfo(2182, "광대 콤프소그나투스", "Compso/compsognathus_clownfishprefab", 0.35f, "compsognathus_ai", true, 1, 1.0f) },
        { 2183, new AnimalInfo(2183, "오색비늘 이구아나", "Iguana/IguanaPrefab", 1.0f, "iguana_ai", true, 3, 1.0f) },
        { 2184, new AnimalInfo(2184, "돌격전차 하드로사우루스", "Parasaurolophus/Hadrosaurus_tank", 1.0f, "parasaurolophus_ai", true, 4, 5.0f) },
        { 2185, new AnimalInfo(2185, "연구용 용암 하드로사우루스", "Parasaurolophus/Hadrosaurus_lava", 1.0f, "parasaurolophus_ai", true, 4, 3.0f) },
        { 2186, new AnimalInfo(2186, "청소부 앤드루사르쿠스", "Andrewsarchus/AndrewsarchusPrefab", 0.45f, "andrewsarchus_ai", false, 2, 1.0f) },
        { 2187, new AnimalInfo(2187, "통신용 아마르가사우루스", "Apatosaurus/AmargasaurusPrefab", 1.0f, "apatosaurus_ai", false, 7, 1.0f) },
        { 2188, new AnimalInfo(2188, "독 도마뱀", "Dimetrodon/Lizard_poisonousPrefab", 1.0f, "dimetrodon_ai", false, 3, 1.0f) },
        { 2189, new AnimalInfo(2189, "미확인 가스토르니스", "Ornithomimus/Gastronis_mut01", 1.0f, "ornithomimus_ai", false, 3, 3.0f) },
        { 2190, new AnimalInfo(2190, "미확인 파보미무스", "Ornithomimus/Gallimimus_birdy_mut01", 0.85f, "gallimimus_ai", false, 2, 3.0f) },
        { 2191, new AnimalInfo(2191, "미확인 흰털다이어울프", "Direwolf/Direwolf_snow_mut01", 1.0f, "direwolf_ai", false, 2, 3.0f) },
        { 2192, new AnimalInfo(2192, "어린 미확인 흰털다이어울프", "Direwolf/Direwolf_snow_small_mut01", 0.5f, "direwolf_baby_ai", false, 1, 1.0f) },
        { 2193, new AnimalInfo(2193, "미확인 땅거북안킬로", "Ankylosaurus/Tortoise_Ankylosaurus_mut01", 1.2f, "ankylosaurus_ai", false, 4, 10.0f) },
        { 2194, new AnimalInfo(2194, "미확인 마크라우케니아", "Macrauchenia/Macrauchenia_mut01", 0.82f, "macrauchenia_ai", false, 2, 1.0f) },
        { 2195, new AnimalInfo(2195, "미확인 갈리미무스", "Ornithomimus/Gallimimus_mut01", 0.85f, "gallimimus_ai", false, 3, 3.0f) },
        { 2196, new AnimalInfo(2196, "미확인 수컷 메갈로케로스", "Macrauchenia/Megaloceros_mut01", 1.24f, "megaloceros_ai", false, 3, 3.0f) },
        { 2197, new AnimalInfo(2197, "미확인 어린 메갈로케로스", "Macrauchenia/Megaloceros_small_mut01", 0.65f, "megaloceros_baby_ai", false, 1, 0.3f) },
        { 2198, new AnimalInfo(2198, "미확인 암컷 메갈로케로스", "Macrauchenia/MegalocerosF_mut01", 1.0f, "megaloceros_female_ai", false, 2, 1.0f) },
        { 2199, new AnimalInfo(2199, "흰털스밀로돈", "Sabertooth/SabertoothPrefab", 1.8f, "sabertooth_ai", false, 4, 3.0f) },
        { 2200, new AnimalInfo(2200, "미확인 어린 흰배메갈로케로스", "Macrauchenia/Megaloceros_snowy_small_mut01", 0.65f, "megaloceros_baby_ai", false, 1, 0.3f) },
        { 2201, new AnimalInfo(2201, "미확인 암컷 흰배메갈로케로스", "Macrauchenia/Megaloceros_snowyF_mut01", 1.0f, "megaloceros_female_ai", false, 2, 1.0f) },
        { 2202, new AnimalInfo(2202, "미확인 안킬로사우루스", "Ankylosaurus/Ankylosaurus_mut01", 1.2f, "ankylosaurus_ai", false, 4, 10.0f) },
        { 2203, new AnimalInfo(2203, "어린 미확인 검은 다이어울프", "Direwolf/Direwolf_smallPrefab", 0.5f, "direwolf_baby_ai", false, 1, 1.0f) },
        { 2204, new AnimalInfo(2204, "미확인 검은 다이어울프", "Direwolf/Direwolf_blackPrefab", 1.0f, "direwolf_ai", false, 2, 3.0f) },
        { 2205, new AnimalInfo(2205, "미확인 수컷 흰배메갈로케로스", "Macrauchenia/Megaloceros_snowy_mut01", 1.24f, "megaloceros_ai", false, 3, 3.0f) },
        { 2206, new AnimalInfo(2206, "흰털다이어울프", "Direwolf/Direwolf_snowPrefab", 1.0f, "direwolf_ai", false, 2, 3.0f) },
        { 2207, new AnimalInfo(2207, "미확인 땅거북안킬로", "Ankylosaurus/Tortoise_Ankylosaurus_mut01", 1.2f, "ankylosaurus_ai", false, 4, 10.0f) },
        { 2208, new AnimalInfo(2208, "미확인 마크라우케니아", "Macrauchenia/Macrauchenia_mut01", 0.82f, "macrauchenia_ai", false, 2, 1.0f) },
        { 2209, new AnimalInfo(2209, "미확인 갈리미무스", "Ornithomimus/Gallimimus_mut01", 0.85f, "gallimimus_ai", false, 3, 3.0f) },
        { 2210, new AnimalInfo(2210, "미확인 수컷 흰배메갈로케로스", "Macrauchenia/Megaloceros_snowy_mut01", 1.24f, "megaloceros_ai", false, 3, 3.0f) },
        { 2211, new AnimalInfo(2211, "미확인 어린 흰배메갈로케로스", "Macrauchenia/Megaloceros_snowy_small_mut01", 0.65f, "megaloceros_baby_ai", false, 1, 0.3f) },
        { 2212, new AnimalInfo(2212, "미확인 암컷 흰배메갈로케로스", "Macrauchenia/Megaloceros_snowyF_mut01", 1.0f, "megaloceros_female_ai", false, 2, 1.0f) },
        { 2999, new AnimalInfo(2999, "더미 샌드백", "Raptor/RaptorPrefab", 2.2f, "dummy_ai", false, 2, 15.0f) },
    };

    public static bool TryGet(ushort entityType, out AnimalInfo info)
    {
        return All.TryGetValue(entityType, out info);
    }
}
