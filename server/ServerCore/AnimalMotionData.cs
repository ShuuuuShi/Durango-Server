using System.Collections.Generic;

namespace DurangoServer.Core;

// ชื่อคลิปอนิเมชันของสัตว์ (generated จาก prefab ใน StreamingAssets/AssetBundles)
// สร้างด้วย scripts/extract_animal_motions.py — อย่าแก้มือ
//
// server ต้องส่งชื่อคลิปไปกับ Movement.MotionName ไม่งั้นสัตว์โผล่มาแล้วยืนแข็ง
// (client เรียก Anim.CrossFade(motionName) ตรง ๆ ใน AnimalBehavior.PlayAnimationMovement)
public static class AnimalMotionData
{
    public readonly struct Motions
    {
        public readonly string Stand;
        public readonly string Walk;
        public readonly string Run;
        /// <summary>ท่าโจมตีทั้งหมดของชนิดนี้ (สุ่มเลือกตอนตี — เดิมมีท่าเดียวเลยตีซ้ำท่าเดิมตลอด)</summary>
        public readonly string[] Attacks;
        public readonly string Die;

        public Motions(string stand, string walk, string run, string[] attacks, string die)
        {
            Stand = stand;
            Walk = walk;
            Run = run;
            Attacks = attacks ?? System.Array.Empty<string>();
            Die = die;
        }
    }

    private static Motions M(string stand, string walk, string run, string[] attacks, string die)
    {
        return new Motions(stand, walk, run, attacks, die);
    }

    /// <summary>entity type → ชุดคลิป (213 ชนิดที่อ่าน prefab ได้)</summary>
    public static readonly Dictionary<ushort, Motions> All = new Dictionary<ushort, Motions>()
    {
        { 2000, M("Stego_Stand", "Stego_Walk", "Stego_Run", new[] { "Stego_Attack_Head", "Stego_Attack", "Stego_Attack_Side", "Stego_Attack_Side_L", "Stego_Attack_Side_R", "Stego_Attack_woundedTail", "Stego_Attack_WoundedTail" }, "Stego_Die") },   // 스테고사우루스
        { 2001, M("Raptor_Stand", "Raptor_Walk", "Raptor_Run", new[] { "Raptor_Attack", "Raptor_Attack_Tail" }, "Raptor_Die") },   // 랩터
        { 2002, M("Raptor_Stand", "Raptor_Walk", "Raptor_Run", new[] { "Raptor_Attack", "Raptor_Attack_Tail" }, "Raptor_Die") },   // 오비랍토르
        { 2003, M("Tricera_Stand", "Tricera_Walk", "Tricera_Run", new[] { "Tricera_Attack_Once", "Tricera_Attack_Head" }, "Tricera_Die") },   // 트리케라톱스
        { 2004, M("Brachio_Stand", "Brachio_Walk", "Brachio_Run", new[] { "Brachio_Attack", "Brachio_Attack_Offspring", "Brachio_Attack_Tail", "Brachio_Attack_Tail_Offspring", "Brachio_Attack_WoundedTail" }, "Brachio_Die") },   // 브라키오사우루스
        { 2005, M("TRex_Stand", "Trex_Idle_Walk", "TRex_Run", new[] { "TRex_Attack_Head", "TRex_Attack_Bite_Normal", "TRex_Attack_Stamp", "TRex_Attack_Stamp_Foot", "TRex_Attack_Stamp_Left", "TRex_Attack_Stamp_Right", "TRex_Attack_Tail_Back", "TRex_Attack01_Tail", "TRex_Attack02_Bite", "TRex_Attack03_Wounded_Tail" }, "TRex_Die") },   // 티라노사우루스
        { 2006, M("Phenaco_Stand", "Phenaco_Walk", "Phenaco_Run", new[] { "Phenaco_Attack_Bite", "Phenaco_Attack", "Phenaco_Active_Attack_Gas", "Phenaco_Attack_Escape", "Phenaco_Attack_Gas" }, "Phenaco_Die") },   // 페나코두스
        { 2007, M("Sabertooth_Stand", "Sabertooth_Walk", "Sabertooth_Run", new[] { "Sarco_Attack_Bite", "Sabertooth_Attack", "Sabertooth_Attack_Claw" }, "Sabertooth_Die") },   // 스밀로돈
        { 2008, M("Mammoth_Stand", "Mammoth_Walk", "Mammoth_Run", new[] { "Mammoth_Attack_Head" }, "Mammoth_Die") },   // 매머드
        { 2009, M("Para_Stand", "Para_Walk", "Para_Run", new[] { "Para_Attack_Spit", "Para_Attack_Stamp", "Para_Attack_Tail" }, "Para_Die") },   // 파라사우롤로푸스
        { 2010, M("Ankylo_Stand", "Ankylo_Walk", "Ankylo_Run", new[] { "Ankylo_Attack_Head", "Ankylo_Attack_Side", "Ankylo_Attack_Side_L", "Ankylo_Attack_Side_R", "Ankylo_Attack_Tail_Down", "Ankylo_Attack_Tail_Swing", "Ankylo_Attack_Wounded_Tail_Swing" }, "Ankylo_Die") },   // 안킬로사우루스
        { 2011, M("Ankylo_Stand", "Ankylo_Walk", "Ankylo_Run", new[] { "Ankylo_Attack_Head", "Ankylo_Attack_Side", "Ankylo_Attack_Side_L", "Ankylo_Attack_Side_R", "Ankylo_Attack_Tail_Down", "Ankylo_Attack_Tail_Swing", "Ankylo_Attack_Wounded_Tail_Swing" }, "Ankylo_Die") },   // 유오플로케팔루스
        { 2012, M("Macrau_Stand", "Macrau_Walk", "Macrau_Run", new[] { "Macrau_Active_Attack_Head", "Macrau_Attack_Head", "Macrau_Active_Attack_FrontLeg", "Macrau_Active_Attack_Kick", "Macrau_Attack_FrontLeg", "Macrau_Attack_Kick", "Macrau_Attack_Kick_Normal", "Macrau_Attack_Spit" }, "Macrau_Die") },   // 마크라우케니아
        { 2013, M("Macrau_Stand", "Macrau_Walk", "Macrau_Run", new[] { "Macrau_Active_Attack_Head", "Macrau_Attack_Head", "Macrau_Active_Attack_FrontLeg", "Macrau_Active_Attack_Kick", "Macrau_Attack_FrontLeg", "Macrau_Attack_Kick", "Macrau_Attack_Kick_Normal", "Macrau_Attack_Spit" }, "Macrau_Die") },   // 수컷 메갈로케로스
        { 2014, M("Ornitho_Stand", "Compso_Walk", "Ornitho_Run", new[] { "Ornitho_Attack_Head", "Compso_Attack_Strong", "Ornitho_Active_Attack_Mad", "Ornitho_Attack_Mad", "Ornitho_Attack_Strong" }, "Compso_Die") },   // 오르니토미무스
        { 2015, M("Compso_Stand", "Compso_Walk", "Compso_Run", new[] { "Compso_Attack_Head", "Compso_Attack_Strong" }, "Compso_Die") },   // 콤프소그나투스
        { 2016, M("Ornitho_Stand", "Compso_Walk", "Ornitho_Run", new[] { "Ornitho_Attack_Head", "Compso_Attack_Strong", "Ornitho_Active_Attack_Mad", "Ornitho_Attack_Mad", "Ornitho_Attack_Strong" }, "Compso_Die") },   // 코엘로피시스
        { 2017, M("Tricera_Stand", "Tricera_Walk", "Tricera_Run", new[] { "Tricera_Attack_Once", "Tricera_Attack_Head" }, "Tricera_Die") },   // 프로토케라톱스
        { 2018, M("Tricera_Stand", "Tricera_Walk", "Tricera_Run", new[] { "Tricera_Attack_Once", "Tricera_Attack_Head" }, "Tricera_Die") },   // 카스모사우루스
        { 2019, M("Tricera_Stand", "Tricera_Walk", "Tricera_Run", new[] { "Tricera_Attack_Once", "Tricera_Attack_Head" }, "Tricera_Die") },   // 스티라코사우루스
        { 2020, M("Dog_Stand", "Dog_Walk", "Dog_Run", new[] { "Wolf_Active_Attack_Bite", "Wolf_Attack_Bite" }, "Wolf_Die") },   // 다이어울프
        { 2021, M("Allo_Stand", "Allo_Walk", "Allo_Run", new[] { "Allo_Attack_Bite", "Allo_Attack_Bite_Normal", "Allo_Attack_Stamp", "Allo_Attack_Tail", "Allo_Attack_Tail_Back", "Allo_Attack_Tail_L", "Allo_Attack_Tail_R" }, "Allo_Die") },   // 알로사우루스
        { 2022, M("Pachy_Stand", "Pachy_Walk", "Pachy_Run", new[] { "Pachy_Attack_Bite", "Pachy_Attack_Head", "Pachy_Active_Attack_Head_Strong", "Pachy_Attack_Bite_Normal", "Pachy_Attack_Head_Strong", "Pachy_Attack_Stamp", "Pachy_Attack_Tail", "Pachy_Attack_Tail_Back", "Pachy_Attack_Tail_L", "Pachy_Attack_Tail_R" }, "Pachy_Die") },   // 파키케팔로사우루스
        { 2023, M("Raptor_Stand", "Raptor_Walk", "Raptor_Run", new[] { "Raptor_Attack", "Raptor_Attack_Tail" }, "Raptor_Die") },   // 유타랍토르
        { 2024, M("Raptor_Stand", "Raptor_Walk", "Raptor_Run", new[] { "Raptor_Attack", "Raptor_Attack_Tail" }, "Raptor_Die") },   // 딜로포사우루스
        { 2025, M("Ornitho_Stand", "Compso_Walk", "Ornitho_Run", new[] { "Ornitho_Attack_Head", "Compso_Attack_Strong", "Ornitho_Active_Attack_Mad", "Ornitho_Attack_Mad", "Ornitho_Attack_Strong" }, "Compso_Die") },   // 갈리미무스
        { 2026, M("Stego_Stand", "Stego_Walk", "Stego_Run", new[] { "Stego_Attack_Head", "Stego_Attack", "Stego_Attack_Side", "Stego_Attack_Side_L", "Stego_Attack_Side_R", "Stego_Attack_woundedTail", "Stego_Attack_WoundedTail" }, "Stego_Die") },   // 투오지앙고사우루스
        { 2027, M("Tricera_Stand", "Tricera_Walk", "Tricera_Run", new[] { "Tricera_Attack_Once", "Tricera_Attack_Head" }, "Tricera_Die") },   // 제브라케라톱스
        { 2028, M("Para_Stand", "Para_Walk", "Para_Run", new[] { "Para_Attack_Spit", "Para_Attack_Stamp", "Para_Attack_Tail" }, "Para_Die") },   // 사우롤로푸스
        { 2029, M("Raptor_Stand", "Raptor_Walk", "Raptor_Run", new[] { "Raptor_Attack", "Raptor_Attack_Tail" }, "Raptor_Die") },   // 데이노니쿠스
        { 2030, M("Para_Stand", "Para_Walk", "Para_Run", new[] { "Para_Attack_Spit", "Para_Attack_Stamp", "Para_Attack_Tail" }, "Para_Die") },   // 코리토사우루스
        { 2031, M("Para_Stand", "Para_Walk", "Para_Run", new[] { "Para_Attack_Spit", "Para_Attack_Stamp", "Para_Attack_Tail" }, "Para_Die") },   // 에드몬토사우루스
        { 2032, M("Phenaco_Stand", "Phenaco_Walk", "Phenaco_Run", new[] { "Phenaco_Attack_Bite", "Phenaco_Attack", "Phenaco_Active_Attack_Gas", "Phenaco_Attack_Escape", "Phenaco_Attack_Gas" }, "Phenaco_Die") },   // 스컹코두스
        { 2033, M("Compso_Stand", "Compso_Walk", "Compso_Run", new[] { "Compso_Attack_Head", "Compso_Attack_Strong" }, "Compso_Die") },   // 도도피시스
        { 2034, M("Dimetrodon_Stand", "Dimetrodon_Walk", "Dimetrodon_Run", new[] { "Dimetrodon_Attack_Head", "Dimetrodon_Attack_Spit", "Dimetrodon_Attack_Tail", "Dimetrodon_Attack_Torso" }, "Dimetrodon_Die") },   // 디메트로돈
        { 2035, M("Compso_Stand", "Compso_Walk", "Compso_Run", new[] { "Compso_Attack_Head", "Compso_Attack_Strong" }, "Compso_Die") },   // 도도피시스
        { 2036, M("Compso_Stand", "Compso_Walk", "Compso_Run", new[] { "Compso_Attack_Head", "Compso_Attack_Strong" }, "Compso_Die") },   // 줄무늬 콤프소그나투스
        { 2037, M("Phenaco_Stand", "Phenaco_Walk", "Phenaco_Run", new[] { "Phenaco_Attack_Bite", "Phenaco_Attack", "Phenaco_Active_Attack_Gas", "Phenaco_Attack_Escape", "Phenaco_Attack_Gas" }, "Phenaco_Die") },   // 엘리펀툴루스
        { 2038, M("Phenaco_Stand", "Phenaco_Walk", "Phenaco_Run", new[] { "Phenaco_Attack_Bite", "Phenaco_Attack", "Phenaco_Active_Attack_Gas", "Phenaco_Attack_Escape", "Phenaco_Attack_Gas" }, "Phenaco_Die") },   // 거대 쥐
        { 2039, M("Raptor_Stand", "Raptor_Walk", "Raptor_Run", new[] { "Raptor_Attack", "Raptor_Attack_Tail" }, "Raptor_Die") },   // 아케니쿠스
        { 2040, M("Tricera_Stand", "Tricera_Walk", "Tricera_Run", new[] { "Tricera_Attack_Once", "Tricera_Attack_Head" }, "Tricera_Die") },   // 연분홍 프로토케라톱스
        { 2041, M("Tricera_Stand", "Tricera_Walk", "Tricera_Run", new[] { "Tricera_Attack_Once", "Tricera_Attack_Head" }, "Tricera_Die") },   // 센트로사우루스
        { 2042, M("Lizard_Stand", "Lizard_Walk", "Lizard_Run", new[] { "Lizard_Attack_Head", "Lizard_Attack_Spit", "Lizard_Attack_Tail", "Lizard_Attack_Torso" }, "Lizard_Die") },   // 도마뱀
        { 2043, M("Lizard_Stand", "Lizard_Walk", "Lizard_Run", new[] { "Lizard_Attack_Head", "Lizard_Attack_Spit", "Lizard_Attack_Tail", "Lizard_Attack_Torso" }, "Lizard_Die") },   // 모래 도마뱀
        { 2044, M("Dimetrodon_Stand", "Dimetrodon_Walk", "Dimetrodon_Run", new[] { "Dimetrodon_Attack_Head", "Dimetrodon_Attack_Spit", "Dimetrodon_Attack_Tail", "Dimetrodon_Attack_Torso" }, "Dimetrodon_Die") },   // 붉은점도마뱀
        { 2045, M("Ornitho_Stand", "Compso_Walk", "Ornitho_Run", new[] { "Ornitho_Attack_Head", "Compso_Attack_Strong", "Ornitho_Active_Attack_Mad", "Ornitho_Attack_Mad", "Ornitho_Attack_Strong" }, "Compso_Die") },   // 코엘로피시스
        { 2046, M("Para_Stand", "Para_Walk", "Para_Run", new[] { "Para_Attack_Spit", "Para_Attack_Stamp", "Para_Attack_Tail" }, "Para_Die") },   // 하드로사우루스
        { 2048, M("Para_Stand", "Para_Walk", "Para_Run", new[] { "Para_Attack_Spit", "Para_Attack_Stamp", "Para_Attack_Tail" }, "Para_Die") },   // 이구아노돈
        { 2049, M("Phenaco_Stand", "Phenaco_Walk", "Phenaco_Run", new[] { "Phenaco_Attack_Bite", "Phenaco_Attack", "Phenaco_Active_Attack_Gas", "Phenaco_Attack_Escape", "Phenaco_Attack_Gas" }, "Phenaco_Die") },   // 한복 페나코두스
        { 2050, M("Phenaco_Stand", "Phenaco_Walk", "Phenaco_Run", new[] { "Phenaco_Attack_Bite", "Phenaco_Attack", "Phenaco_Active_Attack_Gas", "Phenaco_Attack_Escape", "Phenaco_Attack_Gas" }, "Phenaco_Die") },   // 마녀 페나코두스
        { 2051, M("Raptor_Stand", "Raptor_Walk", "Raptor_Run", new[] { "Raptor_Attack", "Raptor_Attack_Tail" }, "Raptor_Die") },   // 겁쟁이 랩터
        { 2052, M("Phenaco_Stand", "Phenaco_Walk", "Phenaco_Run", new[] { "Phenaco_Attack_Bite", "Phenaco_Attack", "Phenaco_Active_Attack_Gas", "Phenaco_Attack_Escape", "Phenaco_Attack_Gas" }, "Phenaco_Die") },   // 호박마녀 페나코두스
        { 2053, M("Macrau_Stand", "Macrau_Walk", "Macrau_Run", new[] { "Macrau_Active_Attack_Head", "Macrau_Attack_Head", "Macrau_Active_Attack_FrontLeg", "Macrau_Active_Attack_Kick", "Macrau_Attack_FrontLeg", "Macrau_Attack_Kick", "Macrau_Attack_Kick_Normal", "Macrau_Attack_Spit" }, "Macrau_Die") },   // 루돌프 메갈로케로스
        { 2054, M("Ankylo_Stand", "Ankylo_Walk", "Ankylo_Run", new[] { "Ankylo_Attack_Head", "Ankylo_Attack_Side", "Ankylo_Attack_Side_L", "Ankylo_Attack_Side_R", "Ankylo_Attack_Tail_Down", "Ankylo_Attack_Tail_Swing", "Ankylo_Attack_Wounded_Tail_Swing" }, "Ankylo_Die") },   // 땅거북안킬로
        { 2055, M("Dog_Stand", "Dog_Walk", "Dog_Run", new[] { "Wolf_Active_Attack_Bite", "Wolf_Attack_Bite" }, "Wolf_Die") },   // 다이어울프
        { 2056, M("Macrau_Stand", "Macrau_Walk", "Macrau_Run", new[] { "Macrau_Active_Attack_Head", "Macrau_Attack_Head", "Macrau_Active_Attack_FrontLeg", "Macrau_Active_Attack_Kick", "Macrau_Attack_FrontLeg", "Macrau_Attack_Kick", "Macrau_Attack_Kick_Normal", "Macrau_Attack_Spit" }, "Macrau_Die") },   // 마크라우케니아
        { 2057, M("Macrau_Stand", "Macrau_Walk", "Macrau_Run", new[] { "Macrau_Active_Attack_Head", "Macrau_Attack_Head", "Macrau_Active_Attack_FrontLeg", "Macrau_Active_Attack_Kick", "Macrau_Attack_FrontLeg", "Macrau_Attack_Kick", "Macrau_Attack_Kick_Normal", "Macrau_Attack_Spit" }, "Macrau_Die") },   // 수컷 메갈로케로스
        { 2058, M("Ornitho_Stand", "Compso_Walk", "Ornitho_Run", new[] { "Ornitho_Attack_Head", "Compso_Attack_Strong", "Ornitho_Active_Attack_Mad", "Ornitho_Attack_Mad", "Ornitho_Attack_Strong" }, "Compso_Die") },   // 코엘로피시스
        { 2059, M("Pachy_Stand", "Pachy_Walk", "Pachy_Run", new[] { "Pachy_Attack_Bite", "Pachy_Attack_Head", "Pachy_Active_Attack_Head_Strong", "Pachy_Attack_Bite_Normal", "Pachy_Attack_Head_Strong", "Pachy_Attack_Stamp", "Pachy_Attack_Tail", "Pachy_Attack_Tail_Back", "Pachy_Attack_Tail_L", "Pachy_Attack_Tail_R" }, "Pachy_Die") },   // 파키케팔로사우루스
        { 2060, M("Phenaco_Stand", "Phenaco_Walk", "Phenaco_Run", new[] { "Phenaco_Attack_Bite", "Phenaco_Attack", "Phenaco_Active_Attack_Gas", "Phenaco_Attack_Escape", "Phenaco_Attack_Gas" }, "Phenaco_Die") },   // 스컹코두스
        { 2061, M("Stego_Stand", "Stego_Walk", "Stego_Run", new[] { "Stego_Attack_Head", "Stego_Attack", "Stego_Attack_Side", "Stego_Attack_Side_L", "Stego_Attack_Side_R", "Stego_Attack_woundedTail", "Stego_Attack_WoundedTail" }, "Stego_Die") },   // 스테고사우루스
        { 2062, M("Tricera_Stand", "Tricera_Walk", "Tricera_Run", new[] { "Tricera_Attack_Once", "Tricera_Attack_Head" }, "Tricera_Die") },   // 트리케라톱스
        { 2063, M("Macrau_Stand", "Macrau_Walk", "Macrau_Run", new[] { "Macrau_Active_Attack_Head", "Macrau_Attack_Head", "Macrau_Active_Attack_FrontLeg", "Macrau_Active_Attack_Kick", "Macrau_Attack_FrontLeg", "Macrau_Attack_Kick", "Macrau_Attack_Kick_Normal", "Macrau_Attack_Spit" }, "Macrau_Die") },   // 암컷 메갈로케로스
        { 2064, M("Macrau_Stand", "Macrau_Walk", "Macrau_Run", new[] { "Macrau_Active_Attack_Head", "Macrau_Attack_Head", "Macrau_Active_Attack_FrontLeg", "Macrau_Active_Attack_Kick", "Macrau_Attack_FrontLeg", "Macrau_Attack_Kick", "Macrau_Attack_Kick_Normal", "Macrau_Attack_Spit" }, "Macrau_Die") },   // 어린 메갈로케로스
        { 2065, M("Tricera_Stand", "Tricera_Walk", "Tricera_Run", new[] { "Tricera_Attack_Once", "Tricera_Attack_Head" }, "Tricera_Die") },   // 어린 트리케라톱스
        { 2066, M("Brachio_Stand", "Brachio_Walk", "Brachio_Run", new[] { "Brachio_Attack", "Brachio_Attack_Offspring", "Brachio_Attack_Tail", "Brachio_Attack_Tail_Offspring", "Brachio_Attack_WoundedTail" }, "Brachio_Die") },   // 어린 브라키오사우루스
        { 2067, M("Ankylo_Stand", "Ankylo_Walk", "Ankylo_Run", new[] { "Ankylo_Attack_Head", "Ankylo_Attack_Side_L", "Ankylo_Attack_Side_R", "Ankylo_Attack_Tail_Down", "Ankylo_Attack_Tail_Swing", "Ankylo_Attack_Wounded_Tail_Swing" }, "Ankylo_Die") },   // 어린 안킬로사우루스
        { 2068, M("Ankylo_Stand", "Ankylo_Walk", "Ankylo_Run", new[] { "Ankylo_Attack_Head", "Ankylo_Attack_Side_L", "Ankylo_Attack_Side_R", "Ankylo_Attack_Tail_Down", "Ankylo_Attack_Tail_Swing", "Ankylo_Attack_Wounded_Tail_Swing" }, "Ankylo_Die") },   // 어린 유오플로케팔루스
        { 2069, M("Dog_Stand", "Dog_Walk", "Dog_Run", new[] { "Wolf_Active_Attack_Bite", "Wolf_Attack_Bite" }, "Wolf_Die") },   // 다이어울프
        { 2070, M("Dog_Stand", "Dog_Walk", "Dog_Run", new[] { "Wolf_Active_Attack_Bite", "Wolf_Attack_Bite" }, "Wolf_Die") },   // 흰털다이어울프
        { 2071, M("Dog_Stand", "Dog_Walk", "Dog_Run", new[] { "Wolf_Active_Attack_Bite", "Wolf_Attack_Bite" }, "Wolf_Die") },   // 어린 다이어울프
        { 2072, M("Tricera_Stand", "Tricera_Walk", "Tricera_Run", new[] { "Tricera_Attack_Once", "Tricera_Attack_Head" }, "Tricera_Die") },   // 어린 카스모사우루스
        { 2073, M("Tricera_Stand", "Tricera_Walk", "Tricera_Run", new[] { "Tricera_Attack_Once", "Tricera_Attack_Head" }, "Tricera_Die") },   // 어린 스티라코사우루스
        { 2074, M("Stego_Stand", "Stego_Walk", "Stego_Run", new[] { "Stego_Attack_Head", "Stego_Attack", "Stego_Attack_Side_L", "Stego_Attack_Side_R", "Stego_Attack_woundedTail", "Stego_Attack_WoundedTail" }, "Stego_Die") },   // 어린 투오지앙고사우루스
        { 2075, M("Stego_Stand", "Stego_Walk", "Stego_Run", new[] { "Stego_Attack_Head", "Stego_Attack", "Stego_Attack_Side_L", "Stego_Attack_Side_R", "Stego_Attack_woundedTail", "Stego_Attack_WoundedTail" }, "Stego_Die") },   // 어린 스테고사우루스
        { 2076, M("Ornitho_Stand", "Compso_Walk", "Ornitho_Run", new[] { "Ornitho_Attack_Head", "Compso_Attack_Strong", "Ornitho_Active_Attack_Mad", "Ornitho_Attack_Mad", "Ornitho_Attack_Strong" }, "Compso_Die") },   // 어린 갈리미무스
        { 2077, M("Raptor_Stand", "Raptor_Walk", "Raptor_Run", new[] { "Raptor_Attack", "Raptor_Attack_Tail" }, "Raptor_Die") },   // 알비랍토르
        { 2078, M("Raptor_Stand", "Raptor_Walk", "Raptor_Run", new[] { "Raptor_Attack", "Raptor_Attack_Tail" }, "Raptor_Die") },   // 픽타랍토르
        { 2079, M("Macrau_Stand", "Macrau_Walk", "Macrau_Run", new[] { "Macrau_Active_Attack_Head", "Macrau_Attack_Head", "Macrau_Active_Attack_FrontLeg", "Macrau_Active_Attack_Kick", "Macrau_Attack_FrontLeg", "Macrau_Attack_Kick", "Macrau_Attack_Kick_Normal", "Macrau_Attack_Spit" }, "Macrau_Die") },   // 수컷 흰배메갈로케로스
        { 2080, M("Macrau_Stand", "Macrau_Walk", "Macrau_Run", new[] { "Macrau_Active_Attack_Head", "Macrau_Attack_Head", "Macrau_Active_Attack_FrontLeg", "Macrau_Active_Attack_Kick", "Macrau_Attack_FrontLeg", "Macrau_Attack_Kick", "Macrau_Attack_Kick_Normal", "Macrau_Attack_Spit" }, "Macrau_Die") },   // 암컷 흰배메갈로케로스
        { 2081, M("Macrau_Stand", "Macrau_Walk", "Macrau_Run", new[] { "Macrau_Active_Attack_Head", "Macrau_Attack_Head", "Macrau_Active_Attack_FrontLeg", "Macrau_Active_Attack_Kick", "Macrau_Attack_FrontLeg", "Macrau_Attack_Kick", "Macrau_Attack_Kick_Normal", "Macrau_Attack_Spit" }, "Macrau_Die") },   // 어린 흰배메갈로케로스
        { 2082, M("Allo_Stand", "Allo_Walk", "Allo_Run", new[] { "Allo_Attack_Bite", "Allo_Attack_Bite_Normal", "Allo_Attack_Stamp", "Allo_Attack_Tail", "Allo_Attack_Tail_Back", "Allo_Attack_Tail_L", "Allo_Attack_Tail_R" }, "Allo_Die") },   // 케라토사우루스
        { 2083, M("Stego_Stand", "Stego_Walk", "Stego_Run", new[] { "Stego_Attack_Head", "Stego_Attack", "Stego_Attack_Side", "Stego_Attack_Side_L", "Stego_Attack_Side_R", "Stego_Attack_woundedTail", "Stego_Attack_WoundedTail" }, "Stego_Die") },   // 켄트로사우루스
        { 2084, M("Stego_Stand", "Stego_Walk", "Stego_Run", new[] { "Stego_Attack_Head", "Stego_Attack", "Stego_Attack_Side_L", "Stego_Attack_Side_R", "Stego_Attack_woundedTail", "Stego_Attack_WoundedTail" }, "Stego_Die") },   // 어린 켄트로사우루스
        { 2085, M("Ornitho_Stand", "Compso_Walk", "Ornitho_Run", new[] { "Ornitho_Attack_Head", "Compso_Attack_Strong", "Ornitho_Active_Attack_Mad", "Ornitho_Attack_Mad", "Ornitho_Attack_Strong" }, "Compso_Die") },   // 어린 오르니토미무스
        { 2086, M("TRex_Stand", "Trex_Idle_Walk", "TRex_Run", new[] { "TRex_Attack_Head", "TRex_Attack_Bite_Normal", "TRex_Attack_Stamp", "TRex_Attack_Stamp_Foot", "TRex_Attack_Stamp_Left", "TRex_Attack_Stamp_Right", "TRex_Attack_Tail_Back", "TRex_Attack01_Tail", "TRex_Attack02_Bite", "TRex_Attack03_Wounded_Tail" }, "TRex_Die") },   // 타르보사우루스
        { 2087, M("Mammoth_Stand", "Mammoth_Walk", "Mammoth_Run", new[] { "Mammoth_Attack_Head" }, "Mammoth_Die") },   // 아랫뿔코끼리
        { 2088, M("Sabertooth_Stand", "Sabertooth_Walk", "Sabertooth_Run", new[] { "Sarco_Attack_Bite", "Sabertooth_Attack", "Sabertooth_Attack_Claw" }, "Sabertooth_Die") },   // 스밀로돈
        { 2089, M("TRex_Stand", "Trex_Idle_Walk", "TRex_Run", new[] { "TRex_Attack_Head", "TRex_Attack_Bite_Normal", "TRex_Attack_Stamp", "TRex_Attack_Stamp_Foot", "TRex_Attack_Stamp_Left", "TRex_Attack_Stamp_Right", "TRex_Attack_Tail_Back", "TRex_Attack01_Tail", "TRex_Attack02_Bite", "TRex_Attack03_Wounded_Tail" }, "TRex_Die") },   // 타르보사우루스
        { 2090, M("Ornitho_Stand", "Compso_Walk", "Ornitho_Run", new[] { "Ornitho_Attack_Head", "Compso_Attack_Strong", "Ornitho_Active_Attack_Mad", "Ornitho_Attack_Mad", "Ornitho_Attack_Strong" }, "Compso_Die") },   // 설원 코엘로피시스
        { 2091, M("Dog_Stand", "Dog_Walk", "Dog_Run", new[] { "Wolf_Active_Attack_Bite", "Wolf_Attack_Bite" }, "Wolf_Die") },   // 흰털다이어울프
        { 2092, M("Macrau_Stand", "Macrau_Walk", "Macrau_Run", new[] { "Macrau_Active_Attack_Head", "Macrau_Attack_Head", "Macrau_Active_Attack_FrontLeg", "Macrau_Active_Attack_Kick", "Macrau_Attack_FrontLeg", "Macrau_Attack_Kick", "Macrau_Attack_Kick_Normal", "Macrau_Attack_Spit" }, "Macrau_Die") },   // 수컷 흰배메갈로케로스
        { 2093, M("Ornitho_Stand", "Compso_Walk", "Ornitho_Run", new[] { "Ornitho_Attack_Head", "Compso_Attack_Strong", "Ornitho_Active_Attack_Mad", "Ornitho_Attack_Mad", "Ornitho_Attack_Strong" }, "Compso_Die") },   // 파보미무스
        { 2094, M("Tricera_Stand", "Tricera_Walk", "Tricera_Run", new[] { "Tricera_Attack_Once", "Tricera_Attack_Head" }, "Tricera_Die") },   // 보누사우루스
        { 2095, M("Phenaco_Stand", "Phenaco_Walk", "Phenaco_Run", new[] { "Phenaco_Attack_Bite", "Phenaco_Attack", "Phenaco_Active_Attack_Gas", "Phenaco_Attack_Escape", "Phenaco_Attack_Gas" }, "Phenaco_Die") },   // 우두머리 페나코두스
        { 2096, M("Tricera_Stand", "Tricera_Walk", "Tricera_Run", new[] { "Tricera_Attack_Once", "Tricera_Attack_Head" }, "Tricera_Die") },   // 센트로사우루스
        { 2097, M("Ornitho_Stand", "Compso_Walk", "Ornitho_Run", new[] { "Ornitho_Attack_Head", "Compso_Attack_Strong", "Ornitho_Active_Attack_Mad", "Ornitho_Attack_Mad", "Ornitho_Attack_Strong" }, "Compso_Die") },   // 우두머리 오르니토미무스
        { 2098, M("Tricera_Stand", "Tricera_Walk", "Tricera_Run", new[] { "Tricera_Attack_Once", "Tricera_Attack_Head" }, "Tricera_Die") },   // 우두머리 트리케라톱스
        { 2099, M("Tricera_Stand", "Tricera_Walk", "Tricera_Run", new[] { "Tricera_Attack_Once", "Tricera_Attack_Head" }, "Tricera_Die") },   // 우두머리 프로토케라톱스
        { 2100, M("Raptor_Stand", "Raptor_Walk", "Raptor_Run", new[] { "Raptor_Attack", "Raptor_Attack_Tail" }, "Raptor_Die") },   // 우두머리 유타랍토르
        { 2101, M("Para_Stand", "Para_Walk", "Para_Run", new[] { "Para_Attack_Spit", "Para_Attack_Stamp", "Para_Attack_Tail" }, "Para_Die") },   // 우두머리 파라사우롤로푸스
        { 2102, M("Mammoth_Stand", "Mammoth_Walk", "Mammoth_Run", new[] { "Mammoth_Attack_Head" }, "Mammoth_Die") },   // 우두머리 데이노테리움
        { 2103, M("Macrau_Stand", "Macrau_Walk", "Macrau_Run", new[] { "Macrau_Active_Attack_Head", "Macrau_Attack_Head", "Macrau_Active_Attack_FrontLeg", "Macrau_Active_Attack_Kick", "Macrau_Attack_FrontLeg", "Macrau_Attack_Kick", "Macrau_Attack_Kick_Normal", "Macrau_Attack_Spit" }, "Macrau_Die") },   // 우두머리 메갈로케로스
        { 2104, M("Dog_Stand", "Dog_Walk", "Dog_Run", new[] { "Wolf_Active_Attack_Bite", "Wolf_Attack_Bite" }, "Wolf_Die") },   // 우두머리 다이어울프
        { 2105, M("Tricera_Stand", "Tricera_Walk", "Tricera_Run", new[] { "Tricera_Attack_Once", "Tricera_Attack_Head" }, "Tricera_Die") },   // 우두머리 스티라코사우루스
        { 2106, M("Tricera_Stand", "Tricera_Walk", "Tricera_Run", new[] { "Tricera_Attack_Once", "Tricera_Attack_Head" }, "Tricera_Die") },   // 우두머리 보누사우루스
        { 2107, M("Tricera_Stand", "Tricera_Walk", "Tricera_Run", new[] { "Tricera_Attack_Once", "Tricera_Attack_Head" }, "Tricera_Die") },   // 우두머리 카스모사우루스
        { 2108, M("Compso_Stand", "Compso_Walk", "Compso_Run", new[] { "Compso_Attack_Head", "Compso_Attack_Strong" }, "Compso_Die") },   // 우두머리 콤프소그나투스
        { 2109, M("Mammoth_Stand", "Mammoth_Walk", "Mammoth_Run", new[] { "Mammoth_Attack_Head" }, "Mammoth_Die") },   // 우두머리 매머드
        { 2110, M("Raptor_Stand", "Raptor_Walk", "Raptor_Run", new[] { "Raptor_Attack", "Raptor_Attack_Tail" }, "Raptor_Die") },   // 우두머리 랩터
        { 2111, M("Sabertooth_Stand", "Sabertooth_Walk", "Sabertooth_Run", new[] { "Sarco_Attack_Bite", "Sabertooth_Attack", "Sabertooth_Attack_Claw" }, "Sabertooth_Die") },   // 우두머리 스밀로돈
        { 2112, M("Stego_Stand", "Stego_Walk", "Stego_Run", new[] { "Stego_Attack_Head", "Stego_Attack", "Stego_Attack_Side", "Stego_Attack_Side_L", "Stego_Attack_Side_R", "Stego_Attack_woundedTail", "Stego_Attack_WoundedTail" }, "Stego_Die") },   // 우두머리 스테고사우루스
        { 2113, M("TRex_Stand", "Trex_Idle_Walk", "TRex_Run", new[] { "TRex_Attack_Head", "TRex_Attack_Bite_Normal", "TRex_Attack_Stamp", "TRex_Attack_Stamp_Foot", "TRex_Attack_Stamp_Left", "TRex_Attack_Stamp_Right", "TRex_Attack_Tail_Back", "TRex_Attack01_Tail", "TRex_Attack02_Bite", "TRex_Attack03_Wounded_Tail" }, "TRex_Die") },   // 우두머리 설원 타르보사우루스
        { 2114, M("TRex_Stand", "Trex_Idle_Walk", "TRex_Run", new[] { "TRex_Attack_Head", "TRex_Attack_Bite_Normal", "TRex_Attack_Stamp", "TRex_Attack_Stamp_Foot", "TRex_Attack_Stamp_Left", "TRex_Attack_Stamp_Right", "TRex_Attack_Tail_Back", "TRex_Attack01_Tail", "TRex_Attack02_Bite", "TRex_Attack03_Wounded_Tail" }, "TRex_Die") },   // 우두머리 티라노사우루스
        { 2115, M("TRex_Stand", "Trex_Idle_Walk", "TRex_Run", new[] { "TRex_Attack_Head", "TRex_Attack_Bite_Normal", "TRex_Attack_Stamp", "TRex_Attack_Stamp_Foot", "TRex_Attack_Stamp_Left", "TRex_Attack_Stamp_Right", "TRex_Attack_Tail_Back", "TRex_Attack01_Tail", "TRex_Attack02_Bite", "TRex_Attack03_Wounded_Tail" }, "TRex_Die") },   // 우두머리 타르보사우루스
        { 2116, M("Stego_Stand", "Stego_Walk", "Stego_Run", new[] { "Stego_Attack_Head", "Stego_Attack", "Stego_Attack_Side", "Stego_Attack_Side_L", "Stego_Attack_Side_R", "Stego_Attack_woundedTail", "Stego_Attack_WoundedTail" }, "Stego_Die") },   // 우두머리 투오지앙고사우루스
        { 2117, M("Stego_Stand", "Stego_Walk", "Stego_Run", new[] { "Stego_Attack_Head", "Stego_Attack", "Stego_Attack_Side", "Stego_Attack_Side_L", "Stego_Attack_Side_R", "Stego_Attack_woundedTail", "Stego_Attack_WoundedTail" }, "Stego_Die") },   // 우두머리 켄트로사우루스
        { 2118, M("Ornitho_Stand", "Compso_Walk", "Ornitho_Run", new[] { "Ornitho_Attack_Head", "Compso_Attack_Strong", "Ornitho_Active_Attack_Mad", "Ornitho_Attack_Mad", "Ornitho_Attack_Strong" }, "Compso_Die") },   // 우두머리 갈리미무스
        { 2119, M("Ornitho_Stand", "Compso_Walk", "Ornitho_Run", new[] { "Ornitho_Attack_Head", "Compso_Attack_Strong", "Ornitho_Active_Attack_Mad", "Ornitho_Attack_Mad", "Ornitho_Attack_Strong" }, "Compso_Die") },   // 우두머리 파보미무스
        { 2120, M("Ornitho_Stand", "Compso_Walk", "Ornitho_Run", new[] { "Ornitho_Attack_Head", "Compso_Attack_Strong", "Ornitho_Active_Attack_Mad", "Ornitho_Attack_Mad", "Ornitho_Attack_Strong" }, "Compso_Die") },   // 우두머리 코엘로피시스
        { 2121, M("Sabertooth_Stand", "Sabertooth_Walk", "Sabertooth_Run", new[] { "Sarco_Attack_Bite", "Sabertooth_Attack", "Sabertooth_Attack_Claw" }, "Sabertooth_Die") },   // 우두머리 열대 스밀로돈
        { 2122, M("Raptor_Stand", "Raptor_Walk", "Raptor_Run", new[] { "Raptor_Attack", "Raptor_Attack_Tail" }, "Raptor_Die") },   // 우두머리 딜로포사우루스
        { 2123, M("Macrau_Stand", "Macrau_Walk", "Macrau_Run", new[] { "Macrau_Active_Attack_Head", "Macrau_Attack_Head", "Macrau_Active_Attack_FrontLeg", "Macrau_Active_Attack_Kick", "Macrau_Attack_FrontLeg", "Macrau_Attack_Kick", "Macrau_Attack_Kick_Normal", "Macrau_Attack_Spit" }, "Macrau_Die") },   // 우두머리 마크라우케니아
        { 2124, M("Brachio_Stand", "Brachio_Walk", "Brachio_Run", new[] { "Brachio_Attack", "Brachio_Attack_Offspring", "Brachio_Attack_Tail", "Brachio_Attack_Tail_Offspring", "Brachio_Attack_WoundedTail" }, "Brachio_Die") },   // 우두머리 브라키오사우루스
        { 2125, M("Ankylo_Stand", "Ankylo_Walk", "Ankylo_Run", new[] { "Ankylo_Attack_Head", "Ankylo_Attack_Side", "Ankylo_Attack_Side_L", "Ankylo_Attack_Side_R", "Ankylo_Attack_Tail_Down", "Ankylo_Attack_Tail_Swing", "Ankylo_Attack_Wounded_Tail_Swing" }, "Ankylo_Die") },   // 우두머리 안킬로사우루스
        { 2126, M("Ankylo_Stand", "Ankylo_Walk", "Ankylo_Run", new[] { "Ankylo_Attack_Head", "Ankylo_Attack_Side", "Ankylo_Attack_Side_L", "Ankylo_Attack_Side_R", "Ankylo_Attack_Tail_Down", "Ankylo_Attack_Tail_Swing", "Ankylo_Attack_Wounded_Tail_Swing" }, "Ankylo_Die") },   // 우두머리 유오플로케팔루스
        { 2127, M("Macrau_Stand", "Macrau_Walk", "Macrau_Run", new[] { "Macrau_Active_Attack_Head", "Macrau_Attack_Head", "Macrau_Active_Attack_FrontLeg", "Macrau_Active_Attack_Kick", "Macrau_Attack_FrontLeg", "Macrau_Attack_Kick", "Macrau_Attack_Kick_Normal", "Macrau_Attack_Spit" }, "Macrau_Die") },   // 붉은코 메갈로케로스
        { 2128, M("Macrau_Stand", "Macrau_Walk", "Macrau_Run", new[] { "Macrau_Active_Attack_Head", "Macrau_Attack_Head", "Macrau_Active_Attack_FrontLeg", "Macrau_Active_Attack_Kick", "Macrau_Attack_FrontLeg", "Macrau_Attack_Kick", "Macrau_Attack_Kick_Normal", "Macrau_Attack_Spit" }, "Macrau_Die") },   // 루돌프 메갈로케로스
        { 2129, M("Phenaco_Stand", "Phenaco_Walk", "Phenaco_Run", new[] { "Phenaco_Attack_Bite", "Phenaco_Attack", "Phenaco_Active_Attack_Gas", "Phenaco_Attack_Escape", "Phenaco_Attack_Gas" }, "Phenaco_Die") },   // 산타 페나코두스
        { 2130, M("Ornitho_Stand", "Compso_Walk", "Ornitho_Run", new[] { "Ornitho_Attack_Head", "Compso_Attack_Strong", "Ornitho_Active_Attack_Mad", "Ornitho_Attack_Mad", "Ornitho_Attack_Strong" }, "Compso_Die") },   // 가스토르니스
        { 2131, M("Dog_Stand", "Dog_Walk", "Dog_Run", new[] { "Dog_Attack_Bite" }, "Dog_Die") },   // 래브라도 리트리버
        { 2132, M("Ankylo_Stand", "Ankylo_Walk", "Ankylo_Run", new[] { "Ankylo_Attack_Head", "Ankylo_Attack_Side", "Ankylo_Attack_Side_L", "Ankylo_Attack_Side_R", "Ankylo_Attack_Tail_Down", "Ankylo_Attack_Tail_Swing", "Ankylo_Attack_Wounded_Tail_Swing" }, "Ankylo_Die") },   // 땅거북안킬로
        { 2133, M("Apato_Stand", "Apato_Walk", "Apato_Run", new[] { "Apato_Attack_CCW", "Apato_Attack_CW", "Apato_Attack_Front", "Apato_Attack_Head_Swing", "Apato_Attack_Stamp", "Apato_Attack_Tail_Down", "Apato_Attack_Tail_Down_B", "Apato_Attack_Tail_Down_L", "Apato_Attack_Tail_Down_R", "Apato_Attack_Tail_Swing" }, "Apato_Die") },   // 아파토사우루스
        { 2134, M("Barehand_Craft_Stand", "Barehand_Lift_Walk", "Barehand_Run", new[] { "Bow_Attack", "Musket_Attack", "Novice_Barehand_Attack", "Novice_Lance_Attack", "Novice_Twohand_Attack", "Shield_Attack", "Sling_Attack", "Torch_Attack", "Twohand_Longspear_Attack", "Barehand_Attack_B", "Barehand_Attack_C", "Barehand_Attack_Kick", "Barehand_Attack_Kick_A", "Barehand_Attack_Kick_B", "Barehand_Attack_Pursuit", "Barehand_Attack_TriplePunch", "Barehand_AttackPunch", "Barehand_AttackStrong", "Bow_Attack_AimedShot", "Bow_Attack_B", "Bow_Attack_C", "Bow_Attack_Fast", "Bow_Attack_Shoot", "Bow_Attack_Shoot_B", "Bow_Attack_Shoot_C", "Chainsaw_Attack_A", "Crossbow_Attack_AimedShot", "Crossbow_Attack_B", "Crossbow_Attack_C", "CrossBow_Attack_Fast", "Crossbow_Attack_Shoot", "Lance_Attack_B", "Lance_Attack_C", "Lance_Attack_Deep", "Lance_Attack_Large", "Lance_Attack_Small", "Novice_Twohand_AttackStrong", "Onehand_Attack_A", "Onehand_Attack_B", "Onehand_Attack_C", "Onehand_Attack_Stab", "Onehand_Attack_TripleSwing", "Onehand_AttackStrong", "Onehand_AttackSwing_Fail", "Prologue_Twohand_Attack_A", "Sling_Attack_Strong", "Twohand_Attack_A", "Twohand_Attack_B", "Twohand_Attack_C", "Twohand_AttackStrong", "Twohand_AttackSwing_Fail", "Twohand_AttackSwing_Lower", "Twohand_AttackSwing_test" }, "Barehand_Die") },   // 징병관
        { 2135, M("Barehand_Craft_Stand", "Barehand_Lift_Walk", "Barehand_Run", new[] { "Bow_Attack", "Musket_Attack", "Novice_Barehand_Attack", "Novice_Lance_Attack", "Novice_Twohand_Attack", "Shield_Attack", "Sling_Attack", "Torch_Attack", "Twohand_Longspear_Attack", "Barehand_Attack_B", "Barehand_Attack_C", "Barehand_Attack_Kick", "Barehand_Attack_Kick_A", "Barehand_Attack_Kick_B", "Barehand_Attack_Pursuit", "Barehand_Attack_TriplePunch", "Barehand_AttackPunch", "Barehand_AttackStrong", "Bow_Attack_AimedShot", "Bow_Attack_B", "Bow_Attack_C", "Bow_Attack_Fast", "Bow_Attack_Shoot", "Bow_Attack_Shoot_B", "Bow_Attack_Shoot_C", "Chainsaw_Attack_A", "Crossbow_Attack_AimedShot", "Crossbow_Attack_B", "Crossbow_Attack_C", "CrossBow_Attack_Fast", "Crossbow_Attack_Shoot", "Lance_Attack_B", "Lance_Attack_C", "Lance_Attack_Deep", "Lance_Attack_Large", "Lance_Attack_Small", "Novice_Twohand_AttackStrong", "Onehand_Attack_A", "Onehand_Attack_B", "Onehand_Attack_C", "Onehand_Attack_Stab", "Onehand_Attack_TripleSwing", "Onehand_AttackStrong", "Onehand_AttackSwing_Fail", "Prologue_Twohand_Attack_A", "Sling_Attack_Strong", "Twohand_Attack_A", "Twohand_Attack_B", "Twohand_Attack_C", "Twohand_AttackStrong", "Twohand_AttackSwing_Fail", "Twohand_AttackSwing_Lower", "Twohand_AttackSwing_test" }, "Barehand_Die") },   // 징병관
        { 2136, M("Barehand_Craft_Stand", "Barehand_Lift_Walk", "Barehand_Run", new[] { "Bow_Attack", "Musket_Attack", "Novice_Barehand_Attack", "Novice_Lance_Attack", "Novice_Twohand_Attack", "Shield_Attack", "Sling_Attack", "Torch_Attack", "Twohand_Longspear_Attack", "Barehand_Attack_B", "Barehand_Attack_C", "Barehand_Attack_Kick", "Barehand_Attack_Kick_A", "Barehand_Attack_Kick_B", "Barehand_Attack_Pursuit", "Barehand_Attack_TriplePunch", "Barehand_AttackPunch", "Barehand_AttackStrong", "Bow_Attack_AimedShot", "Bow_Attack_B", "Bow_Attack_C", "Bow_Attack_Fast", "Bow_Attack_Shoot", "Bow_Attack_Shoot_B", "Bow_Attack_Shoot_C", "Chainsaw_Attack_A", "Crossbow_Attack_AimedShot", "Crossbow_Attack_B", "Crossbow_Attack_C", "CrossBow_Attack_Fast", "Crossbow_Attack_Shoot", "Lance_Attack_B", "Lance_Attack_C", "Lance_Attack_Deep", "Lance_Attack_Large", "Lance_Attack_Small", "Novice_Twohand_AttackStrong", "Onehand_Attack_A", "Onehand_Attack_B", "Onehand_Attack_C", "Onehand_Attack_Stab", "Onehand_Attack_TripleSwing", "Onehand_AttackStrong", "Onehand_AttackSwing_Fail", "Prologue_Twohand_Attack_A", "Sling_Attack_Strong", "Twohand_Attack_A", "Twohand_Attack_B", "Twohand_Attack_C", "Twohand_AttackStrong", "Twohand_AttackSwing_Fail", "Twohand_AttackSwing_Lower", "Twohand_AttackSwing_test" }, "Barehand_Die") },   // 징병대장
        { 2137, M("Barehand_Craft_Stand", "Barehand_Lift_Walk", "Barehand_Run", new[] { "Bow_Attack", "Musket_Attack", "Novice_Barehand_Attack", "Novice_Lance_Attack", "Novice_Twohand_Attack", "Shield_Attack", "Sling_Attack", "Torch_Attack", "Twohand_Longspear_Attack", "Barehand_Attack_B", "Barehand_Attack_C", "Barehand_Attack_Kick", "Barehand_Attack_Kick_A", "Barehand_Attack_Kick_B", "Barehand_Attack_Pursuit", "Barehand_Attack_TriplePunch", "Barehand_AttackPunch", "Barehand_AttackStrong", "Bow_Attack_AimedShot", "Bow_Attack_B", "Bow_Attack_C", "Bow_Attack_Fast", "Bow_Attack_Shoot", "Bow_Attack_Shoot_B", "Bow_Attack_Shoot_C", "Chainsaw_Attack_A", "Crossbow_Attack_AimedShot", "Crossbow_Attack_B", "Crossbow_Attack_C", "CrossBow_Attack_Fast", "Crossbow_Attack_Shoot", "Lance_Attack_B", "Lance_Attack_C", "Lance_Attack_Deep", "Lance_Attack_Large", "Lance_Attack_Small", "Novice_Twohand_AttackStrong", "Onehand_Attack_A", "Onehand_Attack_B", "Onehand_Attack_C", "Onehand_Attack_Stab", "Onehand_Attack_TripleSwing", "Onehand_AttackStrong", "Onehand_AttackSwing_Fail", "Prologue_Twohand_Attack_A", "Sling_Attack_Strong", "Twohand_Attack_A", "Twohand_Attack_B", "Twohand_Attack_C", "Twohand_AttackStrong", "Twohand_AttackSwing_Fail", "Twohand_AttackSwing_Lower", "Twohand_AttackSwing_test" }, "Barehand_Die") },   // 징병대장
        { 2138, M("Sabertooth_Stand", "Sabertooth_Walk", "Sabertooth_Run", new[] { "Sarco_Attack_Bite", "Sabertooth_Attack", "Sabertooth_Attack_Claw" }, "Sabertooth_Die") },   // 흰털스밀로돈
        { 2139, M("Watermelon_Stand", null, null, null, "Watermelon_Die") },   // 수상한 수박
        { 2140, M("Phenaco_Stand", "Phenaco_Walk", "Phenaco_Run", new[] { "Phenaco_Attack_Bite", "Phenaco_Attack", "Phenaco_Active_Attack_Gas", "Phenaco_Attack_Escape", "Phenaco_Attack_Gas" }, "Phenaco_Die") },   // 취한 스컹코두스
        { 2141, M("Phenaco_Stand", "Phenaco_Walk", "Phenaco_Run", new[] { "Phenaco_Attack_Bite", "Phenaco_Attack", "Phenaco_Active_Attack_Gas", "Phenaco_Attack_Escape", "Phenaco_Attack_Gas" }, "Phenaco_Die") },   // 병든 거대 쥐
        { 2142, M("Ornitho_Stand", "Compso_Walk", "Ornitho_Run", new[] { "Ornitho_Attack_Head", "Compso_Attack_Strong", "Ornitho_Active_Attack_Mad", "Ornitho_Attack_Mad", "Ornitho_Attack_Strong" }, "Compso_Die") },   // 다친 코엘로피시스
        { 2143, M("Apato_Stand", "Apato_Walk", "Apato_Run", new[] { "Apato_Attack_CCW", "Apato_Attack_CW", "Apato_Attack_Front", "Apato_Attack_Head_Swing", "Apato_Attack_Stamp", "Apato_Attack_Tail_Down", "Apato_Attack_Tail_Down_B", "Apato_Attack_Tail_Down_L", "Apato_Attack_Tail_Down_R", "Apato_Attack_Tail_Swing" }, "Apato_Die") },   // 기름독 아파토사우루스
        { 2144, M("Tricera_Stand", "Tricera_Walk", "Tricera_Run", new[] { "Tricera_Attack_Once", "Tricera_Attack_Head" }, "Tricera_Die") },   // 중독된 트리케라톱스
        { 2145, M("Tricera_Stand", "Tricera_Walk", "Tricera_Run", new[] { "Tricera_Attack_Once", "Tricera_Attack_Head" }, "Tricera_Die") },   // 흥분한 트리케라톱스
        { 2146, M("Ornitho_Stand", "Compso_Walk", "Ornitho_Run", new[] { "Ornitho_Attack_Head", "Compso_Attack_Strong", "Ornitho_Active_Attack_Mad", "Ornitho_Attack_Mad", "Ornitho_Attack_Strong" }, "Compso_Die") },   // 설원 코엘로피시스
        { 2147, M("Tricera_Stand", "Tricera_Walk", "Tricera_Run", new[] { "Tricera_Attack_Once", "Tricera_Attack_Head" }, "Tricera_Die") },   // 스티라코사우루스
        { 2148, M("Macrau_Stand", "Macrau_Walk", "Macrau_Run", new[] { "Macrau_Active_Attack_Head", "Macrau_Attack_Head", "Macrau_Active_Attack_FrontLeg", "Macrau_Active_Attack_Kick", "Macrau_Attack_FrontLeg", "Macrau_Attack_Kick", "Macrau_Attack_Kick_Normal", "Macrau_Attack_Spit" }, "Macrau_Die") },   // 붉은코 메갈로케로스
        { 2149, M("Ornitho_Stand", "Compso_Walk", "Ornitho_Run", new[] { "Ornitho_Attack_Head", "Compso_Attack_Strong", "Ornitho_Active_Attack_Mad", "Ornitho_Attack_Mad", "Ornitho_Attack_Strong" }, "Compso_Die") },   // 까치 가스토르니스
        { 2150, M("Mammoth_Stand", "Mammoth_Walk", "Mammoth_Run", new[] { "Mammoth_Attack_Head" }, "Mammoth_Die") },   // 흰 코끼리
        { 2151, M("Mammoth_Stand", "Mammoth_Walk", "Mammoth_Run", new[] { "Mammoth_Attack_Head" }, "Mammoth_Die") },   // 흰 코끼리
        { 2152, M("Dog_Stand", "Dog_Walk", "Dog_Run", new[] { "Dog_Attack_Bite" }, "Dog_Die") },   // 깜장 래브라도 리트리버
        { 2153, M("Mammoth_Stand", "Mammoth_Walk", "Mammoth_Run", new[] { "Mammoth_Attack_Head" }, "Mammoth_Die") },   // 아프리카 코끼리
        { 2154, M("Mammoth_Stand", "Mammoth_Walk", "Mammoth_Run", new[] { "Mammoth_Attack_Head" }, "Mammoth_Die") },   // 아프리카 코끼리
        { 2155, M("Compso_Stand", "Compso_Walk", "Compso_Run", new[] { "Compso_Attack_Head", "Compso_Attack_Strong" }, "Compso_Die") },   // 대장 콤프소그나투스
        { 2156, M("Phenaco_Stand", "Phenaco_Walk", "Phenaco_Run", new[] { "Phenaco_Attack_Bite", "Phenaco_Attack", "Phenaco_Active_Attack_Gas", "Phenaco_Attack_Escape", "Phenaco_Attack_Gas" }, "Phenaco_Die") },   // 꿀벌 페나코두스
        { 2157, M("Sabertooth_Stand", "Sabertooth_Walk", "Sabertooth_Run", new[] { "Sarco_Attack_Bite", "Sabertooth_Attack", "Sabertooth_Attack_Claw" }, "Sabertooth_Die") },   // 스밀로돈
        { 2158, M("Compso_Stand", "Compso_Walk", "Compso_Run", new[] { "Compso_Attack_Head", "Compso_Attack_Strong" }, "Compso_Die") },   // 콤프소그나투스
        { 2159, M("Phenaco_Stand", "Phenaco_Walk", "Phenaco_Run", new[] { "Phenaco_Attack_Bite", "Phenaco_Attack", "Phenaco_Active_Attack_Gas", "Phenaco_Attack_Gas" }, "Phenaco_Die") },   // 대장 페나코두스
        { 2160, M("Phenaco_Stand", "Phenaco_Walk", "Phenaco_Run", new[] { "Phenaco_Attack_Bite", "Phenaco_Attack", "Phenaco_Active_Attack_Gas", "Phenaco_Attack_Gas" }, "Phenaco_Die") },   // 페나코두스
        { 2161, M("Raptor_Stand", "Raptor_Walk", "Raptor_Run", new[] { "Raptor_Attack", "Raptor_Attack_Tail" }, "Raptor_Die") },   // 데이노니쿠스
        { 2162, M("Ornitho_Stand", "Compso_Walk", "Ornitho_Run", new[] { "Ornitho_Attack_Head", "Compso_Attack_Strong", "Ornitho_Active_Attack_Mad", "Ornitho_Attack_Mad", "Ornitho_Attack_Strong" }, "Compso_Die") },   // 갈리미무스
        { 2163, M("Ornitho_Stand", "Compso_Walk", "Ornitho_Run", new[] { "Ornitho_Attack_Head", "Compso_Attack_Strong", "Ornitho_Active_Attack_Mad", "Ornitho_Attack_Mad", "Ornitho_Attack_Strong" }, "Compso_Die") },   // 어린 갈리미무스
        { 2164, M("Macrau_Stand", "Macrau_Walk", "Macrau_Run", new[] { "Macrau_Active_Attack_Head", "Macrau_Attack_Head", "Macrau_Active_Attack_FrontLeg", "Macrau_Active_Attack_Kick", "Macrau_Attack_FrontLeg", "Macrau_Attack_Kick", "Macrau_Attack_Kick_Normal", "Macrau_Attack_Spit" }, "Macrau_Die") },   // 수컷 메갈로케로스
        { 2165, M("Macrau_Stand", "Macrau_Walk", "Macrau_Run", new[] { "Macrau_Active_Attack_Head", "Macrau_Attack_Head", "Macrau_Active_Attack_FrontLeg", "Macrau_Active_Attack_Kick", "Macrau_Attack_FrontLeg", "Macrau_Attack_Kick", "Macrau_Attack_Kick_Normal", "Macrau_Attack_Spit" }, "Macrau_Die") },   // 어린 메갈로케로스
        { 2166, M("Macrau_Stand", "Macrau_Walk", "Macrau_Run", new[] { "Macrau_Active_Attack_Head", "Macrau_Attack_Head", "Macrau_Active_Attack_FrontLeg", "Macrau_Active_Attack_Kick", "Macrau_Attack_FrontLeg", "Macrau_Attack_Kick", "Macrau_Attack_Kick_Normal", "Macrau_Attack_Spit" }, "Macrau_Die") },   // 암컷 메갈로케로스
        { 2167, M("Phenaco_Stand", "Phenaco_Walk", "Phenaco_Run", new[] { "Phenaco_Attack_Bite", "Phenaco_Attack", "Phenaco_Active_Attack_Gas", "Phenaco_Attack_Escape", "Phenaco_Attack_Gas" }, "Phenaco_Die") },   // 스컹코두스
        { 2168, M("Tricera_Stand", "Tricera_Walk", "Tricera_Run", new[] { "Tricera_Attack_Once", "Tricera_Attack_Head" }, "Tricera_Die") },   // 센트로사우루스
        { 2169, M("Brachio_junior_Stand", "Brachio_junior_Walk", "Brachio_junior_Walk", new[] { "Brachio_junior_Active_Attack", "Brachio_Attack_Offspring", "Brachio_junior_Attack_Stamp" }, "Brachio_junior_Die") },   // 작은 브라키오사우루스
        { 2170, M("Andrew_Stand", "Andrew_Walk", "Andrew_Run", new[] { "Andrew_Attack_Bite", "Andrew_Attack_Head", "Andrew_Attack", "Andrew_Attack_Body" }, "Andrew_Die") },   // 앤드루사르쿠스
        { 2171, M("HorseshoeCrab_Stand", "HorseshoeCrab_Walk", "HorseshoeCrab_Run", new[] { "HorseshoeCrab_Attack_Chela" }, "HorseshoeCrab_Die") },   // 투구게
        { 2172, M("Iguana_Stand", "Iguana_Walk", "Iguana_Run", new[] { "Iguana_Attack_Head", "Iguana_Attack_Back", "Iguana_Attack_Spit", "Iguana_Attack_Tail", "Iguana_Attack_Torso" }, "Iguana_Die") },   // 불점박이 이구아나
        { 2173, M("Phenaco_Stand", "Phenaco_Walk", "Phenaco_Run", new[] { "Phenaco_Attack_Bite", "Phenaco_Attack", "Phenaco_Active_Attack_Gas", "Phenaco_Attack_Escape", "Phenaco_Attack_Gas" }, "Phenaco_Die") },   // 해녀 스컹코두스
        { 2174, M("Phenaco_Stand", "Phenaco_Walk", "Phenaco_Run", new[] { "Phenaco_Attack_Bite", "Phenaco_Attack", "Phenaco_Active_Attack_Gas", "Phenaco_Attack_Escape", "Phenaco_Attack_Gas" }, "Phenaco_Die") },   // 유황 스컹코두스
        { 2175, M("Raptor_Stand", "Raptor_Walk", "Raptor_Run", new[] { "Raptor_Attack", "Raptor_Attack_Tail" }, "Raptor_Die") },   // 검은머리 유타랍토르
        { 2176, M("Iguana_Stand", "Iguana_Walk", "Iguana_Run", new[] { "Iguana_Attack_Head", "Iguana_Attack_Back", "Iguana_Attack_Spit", "Iguana_Attack_Tail", "Iguana_Attack_Torso" }, "Iguana_Die") },   // 흰줄무늬 이구아나
        { 2177, M("Para_Stand", "Para_Walk", "Para_Run", new[] { "Para_Attack_Spit", "Para_Attack_Stamp", "Para_Attack_Tail" }, "Para_Die") },   // 용암 하드로사우루스
        { 2178, M("Tricera_Stand", "Tricera_Walk", "Tricera_Run", new[] { "Tricera_Attack_Once", "Tricera_Attack_Head" }, "Tricera_Die") },   // 화산재 카스모사우루스
        { 2179, M("Apato_Stand", "Apato_Walk", "Apato_Run", new[] { "Apato_Attack_CCW", "Apato_Attack_CW", "Apato_Attack_Front", "Apato_Attack_Head_Swing", "Apato_Attack_Stamp", "Apato_Attack_Tail_Down", "Apato_Attack_Tail_Down_B", "Apato_Attack_Tail_Down_L", "Apato_Attack_Tail_Down_R", "Apato_Attack_Tail_Swing" }, "Apato_Die") },   // 아마르가사우루스
        { 2180, M("Phenaco_Stand", "Phenaco_Walk", "Phenaco_Run", new[] { "Phenaco_Attack_Bite", "Phenaco_Attack", "Phenaco_Active_Attack_Gas", "Phenaco_Attack_Escape", "Phenaco_Attack_Gas" }, "Phenaco_Die") },   // 축제 엘리펀툴루스
        { 2181, M("Phenaco_Stand", "Phenaco_Walk", "Phenaco_Run", new[] { "Phenaco_Attack_Bite", "Phenaco_Attack", "Phenaco_Active_Attack_Gas", "Phenaco_Attack_Escape", "Phenaco_Attack_Gas" }, "Phenaco_Die") },   // 상어 페나코두스
        { 2182, M("Compso_Stand", "Compso_Walk", "Compso_Run", new[] { "Compso_Attack_Head", "Compso_Attack_Strong" }, "Compso_Die") },   // 광대 콤프소그나투스
        { 2183, M("Iguana_Stand", "Iguana_Walk", "Iguana_Run", new[] { "Iguana_Attack_Head", "Iguana_Attack_Back", "Iguana_Attack_Spit", "Iguana_Attack_Tail", "Iguana_Attack_Torso" }, "Iguana_Die") },   // 오색비늘 이구아나
        { 2184, M("Para_Stand", "Para_Walk", "Para_Run", new[] { "Para_Attack_Spit", "Para_Attack_Stamp", "Para_Attack_Tail" }, "Para_Die") },   // 돌격전차 하드로사우루스
        { 2185, M("Para_Stand", "Para_Walk", "Para_Run", new[] { "Para_Attack_Spit", "Para_Attack_Stamp", "Para_Attack_Tail" }, "Para_Die") },   // 연구용 용암 하드로사우루스
        { 2186, M("Andrew_Stand", "Andrew_Walk", "Andrew_Run", new[] { "Andrew_Attack_Bite", "Andrew_Attack_Head", "Andrew_Attack", "Andrew_Attack_Body" }, "Andrew_Die") },   // 청소부 앤드루사르쿠스
        { 2187, M("Apato_Stand", "Apato_Walk", "Apato_Run", new[] { "Apato_Attack_CCW", "Apato_Attack_CW", "Apato_Attack_Front", "Apato_Attack_Head_Swing", "Apato_Attack_Stamp", "Apato_Attack_Tail_Down", "Apato_Attack_Tail_Down_B", "Apato_Attack_Tail_Down_L", "Apato_Attack_Tail_Down_R", "Apato_Attack_Tail_Swing" }, "Apato_Die") },   // 통신용 아마르가사우루스
        { 2188, M("Dimetrodon_Stand", "Dimetrodon_Walk", "Dimetrodon_Run", new[] { "Dimetrodon_Attack_Head", "Dimetrodon_Attack_Spit", "Dimetrodon_Attack_Tail", "Dimetrodon_Attack_Torso" }, "Dimetrodon_Die") },   // 독 도마뱀
        { 2189, M("Ornitho_Stand", "Compso_Walk", "Ornitho_Run", new[] { "Ornitho_Attack_Head", "Compso_Attack_Strong", "Ornitho_Active_Attack_Mad", "Ornitho_Attack_Mad", "Ornitho_Attack_Strong" }, "Compso_Die") },   // 미확인 가스토르니스
        { 2190, M("Ornitho_Stand", "Compso_Walk", "Ornitho_Run", new[] { "Ornitho_Attack_Head", "Compso_Attack_Strong", "Ornitho_Active_Attack_Mad", "Ornitho_Attack_Mad", "Ornitho_Attack_Strong" }, "Compso_Die") },   // 미확인 파보미무스
        { 2191, M("Dog_Stand", "Dog_Walk", "Dog_Run", new[] { "Wolf_Active_Attack_Bite", "Wolf_Attack_Bite" }, "Wolf_Die") },   // 미확인 흰털다이어울프
        { 2192, M("Dog_Stand", "Dog_Walk", "Dog_Run", new[] { "Wolf_Active_Attack_Bite", "Wolf_Attack_Bite" }, "Wolf_Die") },   // 어린 미확인 흰털다이어울프
        { 2193, M("Ankylo_Stand", "Ankylo_Walk", "Ankylo_Run", new[] { "Ankylo_Attack_Head", "Ankylo_Attack_Side", "Ankylo_Attack_Side_L", "Ankylo_Attack_Side_R", "Ankylo_Attack_Tail_Down", "Ankylo_Attack_Tail_Swing", "Ankylo_Attack_Wounded_Tail_Swing" }, "Ankylo_Die") },   // 미확인 땅거북안킬로
        { 2194, M("Macrau_Stand", "Macrau_Walk", "Macrau_Run", new[] { "Macrau_Active_Attack_Head", "Macrau_Attack_Head", "Macrau_Active_Attack_FrontLeg", "Macrau_Active_Attack_Kick", "Macrau_Attack_FrontLeg", "Macrau_Attack_Kick", "Macrau_Attack_Kick_Normal", "Macrau_Attack_Spit" }, "Macrau_Die") },   // 미확인 마크라우케니아
        { 2195, M("Ornitho_Stand", "Compso_Walk", "Ornitho_Run", new[] { "Ornitho_Attack_Head", "Compso_Attack_Strong", "Ornitho_Active_Attack_Mad", "Ornitho_Attack_Mad", "Ornitho_Attack_Strong" }, "Compso_Die") },   // 미확인 갈리미무스
        { 2196, M("Macrau_Stand", "Macrau_Walk", "Macrau_Run", new[] { "Macrau_Active_Attack_Head", "Macrau_Attack_Head", "Macrau_Active_Attack_FrontLeg", "Macrau_Active_Attack_Kick", "Macrau_Attack_FrontLeg", "Macrau_Attack_Kick", "Macrau_Attack_Kick_Normal", "Macrau_Attack_Spit" }, "Macrau_Die") },   // 미확인 수컷 메갈로케로스
        { 2197, M("Macrau_Stand", "Macrau_Walk", "Macrau_Run", new[] { "Macrau_Active_Attack_Head", "Macrau_Attack_Head", "Macrau_Active_Attack_FrontLeg", "Macrau_Active_Attack_Kick", "Macrau_Attack_FrontLeg", "Macrau_Attack_Kick", "Macrau_Attack_Kick_Normal", "Macrau_Attack_Spit" }, "Macrau_Die") },   // 미확인 어린 메갈로케로스
        { 2198, M("Macrau_Stand", "Macrau_Walk", "Macrau_Run", new[] { "Macrau_Active_Attack_Head", "Macrau_Attack_Head", "Macrau_Active_Attack_FrontLeg", "Macrau_Active_Attack_Kick", "Macrau_Attack_FrontLeg", "Macrau_Attack_Kick", "Macrau_Attack_Kick_Normal", "Macrau_Attack_Spit" }, "Macrau_Die") },   // 미확인 암컷 메갈로케로스
        { 2199, M("Sabertooth_Stand", "Sabertooth_Walk", "Sabertooth_Run", new[] { "Sarco_Attack_Bite", "Sabertooth_Attack", "Sabertooth_Attack_Claw" }, "Sabertooth_Die") },   // 흰털스밀로돈
        { 2200, M("Macrau_Stand", "Macrau_Walk", "Macrau_Run", new[] { "Macrau_Active_Attack_Head", "Macrau_Attack_Head", "Macrau_Active_Attack_FrontLeg", "Macrau_Active_Attack_Kick", "Macrau_Attack_FrontLeg", "Macrau_Attack_Kick", "Macrau_Attack_Kick_Normal", "Macrau_Attack_Spit" }, "Macrau_Die") },   // 미확인 어린 흰배메갈로케로스
        { 2201, M("Macrau_Stand", "Macrau_Walk", "Macrau_Run", new[] { "Macrau_Active_Attack_Head", "Macrau_Attack_Head", "Macrau_Active_Attack_FrontLeg", "Macrau_Active_Attack_Kick", "Macrau_Attack_FrontLeg", "Macrau_Attack_Kick", "Macrau_Attack_Kick_Normal", "Macrau_Attack_Spit" }, "Macrau_Die") },   // 미확인 암컷 흰배메갈로케로스
        { 2202, M("Ankylo_Stand", "Ankylo_Walk", "Ankylo_Run", new[] { "Ankylo_Attack_Head", "Ankylo_Attack_Side", "Ankylo_Attack_Side_L", "Ankylo_Attack_Side_R", "Ankylo_Attack_Tail_Down", "Ankylo_Attack_Tail_Swing", "Ankylo_Attack_Wounded_Tail_Swing" }, "Ankylo_Die") },   // 미확인 안킬로사우루스
        { 2203, M("Dog_Stand", "Dog_Walk", "Dog_Run", new[] { "Wolf_Active_Attack_Bite", "Wolf_Attack_Bite" }, "Wolf_Die") },   // 어린 미확인 검은 다이어울프
        { 2204, M("Dog_Stand", "Dog_Walk", "Dog_Run", new[] { "Wolf_Active_Attack_Bite", "Wolf_Attack_Bite" }, "Wolf_Die") },   // 미확인 검은 다이어울프
        { 2205, M("Macrau_Stand", "Macrau_Walk", "Macrau_Run", new[] { "Macrau_Active_Attack_Head", "Macrau_Attack_Head", "Macrau_Active_Attack_FrontLeg", "Macrau_Active_Attack_Kick", "Macrau_Attack_FrontLeg", "Macrau_Attack_Kick", "Macrau_Attack_Kick_Normal", "Macrau_Attack_Spit" }, "Macrau_Die") },   // 미확인 수컷 흰배메갈로케로스
        { 2206, M("Dog_Stand", "Dog_Walk", "Dog_Run", new[] { "Wolf_Active_Attack_Bite", "Wolf_Attack_Bite" }, "Wolf_Die") },   // 흰털다이어울프
        { 2207, M("Ankylo_Stand", "Ankylo_Walk", "Ankylo_Run", new[] { "Ankylo_Attack_Head", "Ankylo_Attack_Side", "Ankylo_Attack_Side_L", "Ankylo_Attack_Side_R", "Ankylo_Attack_Tail_Down", "Ankylo_Attack_Tail_Swing", "Ankylo_Attack_Wounded_Tail_Swing" }, "Ankylo_Die") },   // 미확인 땅거북안킬로
        { 2208, M("Macrau_Stand", "Macrau_Walk", "Macrau_Run", new[] { "Macrau_Active_Attack_Head", "Macrau_Attack_Head", "Macrau_Active_Attack_FrontLeg", "Macrau_Active_Attack_Kick", "Macrau_Attack_FrontLeg", "Macrau_Attack_Kick", "Macrau_Attack_Kick_Normal", "Macrau_Attack_Spit" }, "Macrau_Die") },   // 미확인 마크라우케니아
        { 2209, M("Ornitho_Stand", "Compso_Walk", "Ornitho_Run", new[] { "Ornitho_Attack_Head", "Compso_Attack_Strong", "Ornitho_Active_Attack_Mad", "Ornitho_Attack_Mad", "Ornitho_Attack_Strong" }, "Compso_Die") },   // 미확인 갈리미무스
        { 2210, M("Macrau_Stand", "Macrau_Walk", "Macrau_Run", new[] { "Macrau_Active_Attack_Head", "Macrau_Attack_Head", "Macrau_Active_Attack_FrontLeg", "Macrau_Active_Attack_Kick", "Macrau_Attack_FrontLeg", "Macrau_Attack_Kick", "Macrau_Attack_Kick_Normal", "Macrau_Attack_Spit" }, "Macrau_Die") },   // 미확인 수컷 흰배메갈로케로스
        { 2211, M("Macrau_Stand", "Macrau_Walk", "Macrau_Run", new[] { "Macrau_Active_Attack_Head", "Macrau_Attack_Head", "Macrau_Active_Attack_FrontLeg", "Macrau_Active_Attack_Kick", "Macrau_Attack_FrontLeg", "Macrau_Attack_Kick", "Macrau_Attack_Kick_Normal", "Macrau_Attack_Spit" }, "Macrau_Die") },   // 미확인 어린 흰배메갈로케로스
        { 2212, M("Macrau_Stand", "Macrau_Walk", "Macrau_Run", new[] { "Macrau_Active_Attack_Head", "Macrau_Attack_Head", "Macrau_Active_Attack_FrontLeg", "Macrau_Active_Attack_Kick", "Macrau_Attack_FrontLeg", "Macrau_Attack_Kick", "Macrau_Attack_Kick_Normal", "Macrau_Attack_Spit" }, "Macrau_Die") },   // 미확인 암컷 흰배메갈로케로스
        { 2999, M("Raptor_Stand", "Raptor_Walk", "Raptor_Run", new[] { "Raptor_Attack", "Raptor_Attack_Tail" }, "Raptor_Die") },   // 더미 샌드백
    };

    public static bool TryGet(ushort entityType, out Motions motions)
    {
        return All.TryGetValue(entityType, out motions);
    }

    /// <summary>คลิป "ยืนเฉย ๆ" (null = ไม่รู้จักชนิดนี้)</summary>
    public static string Stand(ushort entityType)
    {
        return All.TryGetValue(entityType, out Motions m) ? m.Stand : null;
    }

    public static string Walk(ushort entityType)
    {
        return All.TryGetValue(entityType, out Motions m) ? m.Walk : null;
    }

    public static string Run(ushort entityType)
    {
        return All.TryGetValue(entityType, out Motions m) ? m.Run : null;
    }

    /// <summary>คลิปโจมตี (null = ไม่มีท่าโจมตีในข้อมูล)</summary>
    private static readonly System.Random _rng = new System.Random();

    /// <summary>สุ่มท่าโจมตี 1 ท่าจากที่ชนิดนี้มี (null = ไม่รู้จัก/ไม่มีท่าโจมตี)</summary>
    public static string Attack(ushort entityType)
    {
        if (!All.TryGetValue(entityType, out Motions m) || m.Attacks.Length == 0)
        {
            return null;
        }
        if (m.Attacks.Length == 1)
        {
            return m.Attacks[0];
        }
        lock (_rng)
        {
            return m.Attacks[_rng.Next(m.Attacks.Length)];
        }
    }

    /// <summary>ท่าโจมตีทั้งหมดของชนิดนี้ (ไว้ดูตอนดีบั๊ก)</summary>
    public static string[] AllAttacks(ushort entityType)
    {
        return All.TryGetValue(entityType, out Motions m) ? m.Attacks : System.Array.Empty<string>();
    }

    /// <summary>คลิปตาย</summary>
    public static string Die(ushort entityType)
    {
        return All.TryGetValue(entityType, out Motions m) ? m.Die : null;
    }
}
