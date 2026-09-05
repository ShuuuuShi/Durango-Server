// สร้างด้วย scripts/extract_animal_kind.py จาก data/assets/entity_types/animal.json — อย่าแก้มือ
//
// type: Carnivore / Herbivore / Scavenger / Sandbag · attack_cooltime (วินาที) · combat_level_ranges

using System.Collections.Generic;

namespace DurangoServer.Core;

public static class AnimalKindData
{
    public enum Kind { Herbivore, Carnivore, Scavenger, Sandbag }

    public readonly struct Info
    {
        public readonly Kind Kind;
        public readonly float AttackCooltime;
        public readonly int LevelMin, LevelMax;
        public Info(Kind kind, float cd, int lo, int hi) { Kind = kind; AttackCooltime = cd; LevelMin = lo; LevelMax = hi; }
    }

    private static Info I(Kind k, float cd, int lo, int hi) => new Info(k, cd, lo, hi);

    public static readonly Dictionary<ushort, Info> All = new Dictionary<ushort, Info>()
    {
        { 2000, I(Kind.Herbivore, 2.1f, 1, 80) },  // 스테고사우루스
        { 2001, I(Kind.Carnivore, 1.7f, 1, 80) },  // 랩터
        { 2002, I(Kind.Scavenger, 1.3f, 1, 80) },  // 오비랍토르
        { 2003, I(Kind.Herbivore, 1.4f, 1, 80) },  // 트리케라톱스
        { 2004, I(Kind.Herbivore, 3.2f, 1, 80) },  // 브라키오사우루스
        { 2005, I(Kind.Carnivore, 1.9f, 60, 90) },  // 티라노사우루스
        { 2006, I(Kind.Herbivore, 1.6f, 1, 80) },  // 페나코두스
        { 2007, I(Kind.Carnivore, 2.1f, 1, 80) },  // 스밀로돈
        { 2008, I(Kind.Herbivore, 1.9f, 1, 80) },  // 매머드
        { 2009, I(Kind.Herbivore, 2.0f, 1, 80) },  // 파라사우롤로푸스
        { 2010, I(Kind.Herbivore, 1.6f, 1, 80) },  // 안킬로사우루스
        { 2011, I(Kind.Herbivore, 1.8f, 1, 80) },  // 유오플로케팔루스
        { 2012, I(Kind.Herbivore, 3.2f, 1, 80) },  // 마크라우케니아
        { 2013, I(Kind.Herbivore, 4.0f, 25, 80) },  // 수컷 메갈로케로스
        { 2014, I(Kind.Carnivore, 3.6f, 1, 80) },  // 오르니토미무스
        { 2015, I(Kind.Scavenger, 3.0f, 1, 80) },  // 콤프소그나투스
        { 2016, I(Kind.Carnivore, 3.6f, 1, 80) },  // 코엘로피시스
        { 2017, I(Kind.Herbivore, 1.3f, 1, 80) },  // 프로토케라톱스
        { 2018, I(Kind.Herbivore, 1.1f, 80, 120) },  // 카스모사우루스
        { 2019, I(Kind.Herbivore, 1.4f, 1, 80) },  // 스티라코사우루스
        { 2020, I(Kind.Carnivore, 1.8f, 1, 80) },  // 다이어울프
        { 2021, I(Kind.Carnivore, 2.0f, 1, 80) },  // 알로사우루스
        { 2022, I(Kind.Herbivore, 2.0f, 1, 80) },  // 파키케팔로사우루스
        { 2023, I(Kind.Carnivore, 1.7f, 1, 80) },  // 유타랍토르
        { 2024, I(Kind.Carnivore, 1.7f, 1, 80) },  // 딜로포사우루스
        { 2025, I(Kind.Herbivore, 3.6f, 1, 80) },  // 갈리미무스
        { 2026, I(Kind.Herbivore, 2.1f, 1, 80) },  // 투오지앙고사우루스
        { 2027, I(Kind.Herbivore, 1.3f, 1, 80) },  // 제브라케라톱스
        { 2028, I(Kind.Herbivore, 2.0f, 1, 80) },  // 사우롤로푸스
        { 2029, I(Kind.Carnivore, 1.7f, 1, 80) },  // 데이노니쿠스
        { 2030, I(Kind.Herbivore, 2.0f, 40, 60) },  // 코리토사우루스
        { 2031, I(Kind.Herbivore, 2.2f, 1, 80) },  // 에드몬토사우루스
        { 2032, I(Kind.Herbivore, 1.6f, 1, 80) },  // 스컹코두스
        { 2033, I(Kind.Carnivore, 3.0f, 1, 80) },  // 도도피시스
        { 2034, I(Kind.Carnivore, 1.4f, 1, 80) },  // 디메트로돈
        { 2035, I(Kind.Carnivore, 3.0f, 1, 80) },  // 도도피시스
        { 2036, I(Kind.Carnivore, 3.0f, 1, 80) },  // 줄무늬 콤프소그나투스
        { 2037, I(Kind.Herbivore, 1.6f, 1, 80) },  // 엘리펀툴루스
        { 2038, I(Kind.Herbivore, 1.6f, 1, 80) },  // 거대 쥐
        { 2039, I(Kind.Carnivore, 1.7f, 1, 80) },  // 아케니쿠스
        { 2040, I(Kind.Herbivore, 1.3f, 1, 80) },  // 연분홍 프로토케라톱스
        { 2041, I(Kind.Herbivore, 1.3f, 1, 80) },  // 센트로사우루스
        { 2042, I(Kind.Herbivore, 1.4f, 1, 80) },  // 도마뱀
        { 2043, I(Kind.Herbivore, 1.4f, 1, 80) },  // 모래도마뱀
        { 2044, I(Kind.Carnivore, 1.4f, 1, 80) },  // 붉은점도마뱀
        { 2045, I(Kind.Carnivore, 3.6f, 1, 80) },  // 코엘로피시스
        { 2046, I(Kind.Herbivore, 2.2f, 1, 80) },  // 하드로사우루스
        { 2047, I(Kind.Carnivore, 2.1f, 1, 80) },  // 스밀로돈
        { 2048, I(Kind.Herbivore, 2.2f, 1, 80) },  // 이구아노돈
        { 2049, I(Kind.Herbivore, 1.6f, 1, 80) },  // 한복 페나코두스
        { 2050, I(Kind.Herbivore, 1.6f, 1, 80) },  // 마녀 페나코두스
        { 2051, I(Kind.Scavenger, 1.7f, 1, 80) },  // 겁쟁이 랩터
        { 2052, I(Kind.Herbivore, 1.6f, 1, 80) },  // 호박마녀 페나코두스
        { 2053, I(Kind.Herbivore, 4.0f, 56, 84) },  // 루돌프 메갈로케로스
        { 2054, I(Kind.Herbivore, 1.6f, 1, 80) },  // 땅거북안킬로
        { 2055, I(Kind.Carnivore, 1.8f, 1, 80) },  // 다이어울프
        { 2056, I(Kind.Herbivore, 3.2f, 1, 80) },  // 마크라우케니아
        { 2057, I(Kind.Herbivore, 4.0f, 25, 80) },  // 수컷 메갈로케로스
        { 2058, I(Kind.Carnivore, 3.6f, 1, 80) },  // 코엘로피시스
        { 2059, I(Kind.Herbivore, 2.0f, 1, 80) },  // 파키케팔로사우루스
        { 2060, I(Kind.Herbivore, 1.6f, 1, 80) },  // 스컹코두스
        { 2061, I(Kind.Herbivore, 2.7f, 1, 80) },  // 스테고사우루스
        { 2062, I(Kind.Herbivore, 1.4f, 1, 80) },  // 트리케라톱스
        { 2063, I(Kind.Herbivore, 4.0f, 25, 80) },  // 암컷 메갈로케로스
        { 2064, I(Kind.Herbivore, 4.0f, 25, 80) },  // 어린 메갈로케로스
        { 2065, I(Kind.Herbivore, 1.4f, 1, 80) },  // 어린 트리케라톱스
        { 2066, I(Kind.Herbivore, 3.6f, 1, 80) },  // 어린 브라키오사우루스
        { 2067, I(Kind.Herbivore, 1.6f, 1, 80) },  // 어린 안킬로사우루스
        { 2068, I(Kind.Herbivore, 1.8f, 1, 80) },  // 어린 유오플로케팔루스
        { 2069, I(Kind.Carnivore, 1.8f, 1, 80) },  // 다이어울프
        { 2070, I(Kind.Carnivore, 1.8f, 1, 80) },  // 흰털다이어울프
        { 2071, I(Kind.Carnivore, 1.8f, 1, 80) },  // 어린 다이어울프
        { 2072, I(Kind.Herbivore, 1.1f, 30, 45) },  // 어린 카스모사우루스
        { 2073, I(Kind.Herbivore, 1.2f, 1, 80) },  // 어린 스티라코사우루스
        { 2074, I(Kind.Herbivore, 2.1f, 1, 80) },  // 어린 투오지앙고사우루스
        { 2075, I(Kind.Herbivore, 2.1f, 1, 80) },  // 어린 스테고사우루스
        { 2076, I(Kind.Herbivore, 3.6f, 1, 80) },  // 어린 갈리미무스
        { 2077, I(Kind.Scavenger, 1.3f, 1, 80) },  // 알비랍토르
        { 2078, I(Kind.Scavenger, 1.3f, 1, 80) },  // 픽타랍토르
        { 2079, I(Kind.Herbivore, 4.0f, 1, 80) },  // 수컷 흰배메갈로케로스
        { 2080, I(Kind.Herbivore, 4.0f, 1, 80) },  // 암컷 흰배메갈로케로스
        { 2081, I(Kind.Herbivore, 4.0f, 1, 80) },  // 어린 흰배메갈로케로스
        { 2082, I(Kind.Carnivore, 2.0f, 1, 80) },  // 케라토사우루스
        { 2083, I(Kind.Herbivore, 2.1f, 1, 80) },  // 켄트로사우루스
        { 2084, I(Kind.Herbivore, 2.1f, 1, 80) },  // 어린 켄트로사우루스
        { 2085, I(Kind.Carnivore, 3.6f, 1, 80) },  // 어린 오르니토미무스
        { 2086, I(Kind.Carnivore, 2.0f, 1, 80) },  // 타르보사우루스
        { 2087, I(Kind.Herbivore, 2.1f, 1, 80) },  // 아랫뿔코끼리
        { 2088, I(Kind.Carnivore, 2.1f, 1, 80) },  // 스밀로돈
        { 2089, I(Kind.Carnivore, 2.0f, 1, 80) },  // 타르보사우루스
        { 2090, I(Kind.Carnivore, 3.6f, 1, 80) },  // 설원 코엘로피시스
        { 2091, I(Kind.Carnivore, 1.8f, 1, 80) },  // 흰털다이어울프
        { 2092, I(Kind.Herbivore, 4.0f, 1, 80) },  // 수컷 흰배메갈로케로스
        { 2093, I(Kind.Carnivore, 3.6f, 1, 80) },  // 파보미무스
        { 2094, I(Kind.Herbivore, 1.4f, 1, 80) },  // 보누사우루스
        { 2095, I(Kind.Herbivore, 1.6f, 28, 42) },  // 우두머리 페나코두스
        { 2096, I(Kind.Herbivore, 1.3f, 1, 80) },  // 센트로사우루스
        { 2097, I(Kind.Carnivore, 3.6f, 64, 96) },  // 우두머리 오르니토미무스
        { 2098, I(Kind.Herbivore, 1.4f, 112, 168) },  // 우두머리 트리케라톱스
        { 2099, I(Kind.Herbivore, 1.3f, 32, 48) },  // 우두머리 프로토케라톱스
        { 2100, I(Kind.Carnivore, 1.7f, 50, 75) },  // 우두머리 유타랍토르
        { 2101, I(Kind.Herbivore, 2.2f, 88, 132) },  // 우두머리 파라사우롤로푸스
        { 2102, I(Kind.Herbivore, 2.7f, 106, 159) },  // 우두머리 데이노테리움
        { 2103, I(Kind.Herbivore, 4.0f, 56, 84) },  // 우두머리 메갈로케로스
        { 2104, I(Kind.Carnivore, 1.8f, 32, 48) },  // 우두머리 다이어울프
        { 2105, I(Kind.Herbivore, 1.4f, 72, 108) },  // 우두머리 스티라코사우루스
        { 2106, I(Kind.Herbivore, 1.4f, 36, 54) },  // 우두머리 보누사우루스
        { 2107, I(Kind.Herbivore, 1.1f, 80, 120) },  // 우두머리 카스모사우루스
        { 2108, I(Kind.Carnivore, 3.0f, 12, 18) },  // 우두머리 콤프소그나투스
        { 2109, I(Kind.Herbivore, 2.7f, 100, 150) },  // 우두머리 매머드
        { 2110, I(Kind.Carnivore, 1.7f, 40, 60) },  // 우두머리 랩터
        { 2111, I(Kind.Carnivore, 2.1f, 80, 120) },  // 우두머리 스밀로돈
        { 2112, I(Kind.Herbivore, 2.7f, 52, 78) },  // 우두머리 스테고사우루스
        { 2113, I(Kind.Carnivore, 2.5f, 108, 162) },  // 우두머리 설원 타르보사우루스
        { 2114, I(Kind.Carnivore, 2.5f, 120, 180) },  // 우두머리 티라노사우루스
        { 2115, I(Kind.Carnivore, 2.5f, 108, 162) },  // 우두머리 타르보사우루스
        { 2116, I(Kind.Herbivore, 2.7f, 76, 114) },  // 우두머리 투오지앙고사우루스
        { 2117, I(Kind.Herbivore, 2.7f, 60, 90) },  // 우두머리 켄트로사우루스
        { 2118, I(Kind.Carnivore, 3.6f, 46, 69) },  // 우두머리 갈리미무스
        { 2119, I(Kind.Carnivore, 3.6f, 40, 60) },  // 우두머리 파보미무스
        { 2120, I(Kind.Carnivore, 3.6f, 36, 54) },  // 우두머리 코엘로피시스
        { 2121, I(Kind.Carnivore, 2.1f, 68, 102) },  // 우두머리 열대 스밀로돈
        { 2122, I(Kind.Carnivore, 1.7f, 44, 66) },  // 우두머리 딜로포사우루스
        { 2123, I(Kind.Herbivore, 3.2f, 22, 33) },  // 우두머리 마크라우케니아
        { 2124, I(Kind.Herbivore, 7.8f, 112, 168) },  // 우두머리 브라키오사우루스
        { 2125, I(Kind.Herbivore, 1.6f, 100, 150) },  // 우두머리 안킬로사우루스
        { 2126, I(Kind.Herbivore, 1.8f, 92, 138) },  // 우두머리 유오플로케팔루스
        { 2127, I(Kind.Herbivore, 4.0f, 48, 72) },  // 붉은코 메갈로케로스
        { 2128, I(Kind.Herbivore, 4.0f, 56, 84) },  // 루돌프 메갈로케로스
        { 2129, I(Kind.Herbivore, 1.6f, 1, 80) },  // 산타 페나코두스
        { 2130, I(Kind.Carnivore, 3.6f, 1, 80) },  // 가스토르니스
        { 2131, I(Kind.Carnivore, 1.8f, 32, 48) },  // 래브라도 리트리버
        { 2132, I(Kind.Herbivore, 1.6f, 1, 80) },  // 땅거북안킬로
        { 2133, I(Kind.Herbivore, 1.9f, 1, 80) },  // 아파토사우루스
        { 2134, I(Kind.Herbivore, 3.0f, 1, 80) },  // 징병관
        { 2135, I(Kind.Herbivore, 3.0f, 1, 80) },  // 징병관
        { 2136, I(Kind.Herbivore, 3.0f, 1, 80) },  // 징병대장
        { 2137, I(Kind.Herbivore, 3.0f, 1, 80) },  // 징병대장
        { 2138, I(Kind.Carnivore, 2.1f, 1, 80) },  // 흰털스밀로돈
        { 2139, I(Kind.Herbivore, 3.0f, 1, 80) },  // 수상한 수박
        { 2140, I(Kind.Herbivore, 1.6f, 1, 80) },  // 취한 스컹코두스
        { 2141, I(Kind.Herbivore, 1.6f, 1, 80) },  // 병든 거대 쥐
        { 2142, I(Kind.Carnivore, 3.6f, 1, 80) },  // 다친 코엘로피시스
        { 2143, I(Kind.Herbivore, 2.0f, 1, 80) },  // 기름독 아파토사우루스
        { 2144, I(Kind.Herbivore, 1.4f, 1, 80) },  // 중독된 트리케라톱스
        { 2145, I(Kind.Herbivore, 1.4f, 1, 80) },  // 흥분한 트리케라톱스
        { 2146, I(Kind.Carnivore, 3.6f, 1, 80) },  // 설원 코엘로피시스
        { 2147, I(Kind.Herbivore, 1.4f, 1, 80) },  // 스티라코사우루스
        { 2148, I(Kind.Herbivore, 4.0f, 48, 72) },  // 붉은코 메갈로케로스
        { 2149, I(Kind.Carnivore, 3.6f, 1, 80) },  // 까치 가스토르니스
        { 2150, I(Kind.Herbivore, 2.7f, 1, 80) },  // 흰 코끼리
        { 2151, I(Kind.Herbivore, 2.7f, 1, 80) },  // 흰 코끼리
        { 2152, I(Kind.Carnivore, 1.8f, 32, 48) },  // 깜장 래브라도 리트리버
        { 2153, I(Kind.Herbivore, 2.7f, 1, 80) },  // 아프리카 코끼리
        { 2154, I(Kind.Herbivore, 2.7f, 1, 80) },  // 아프리카 코끼리
        { 2155, I(Kind.Herbivore, 1.6f, 1, 80) },  // 대장 콤프소그나투스
        { 2156, I(Kind.Herbivore, 1.6f, 1, 80) },  // 꿀벌 페나코두스
        { 2157, I(Kind.Carnivore, 2.1f, 1, 80) },  // 스밀로돈
        { 2158, I(Kind.Herbivore, 1.6f, 1, 80) },  // 콤프소그나투스
        { 2159, I(Kind.Herbivore, 1.6f, 1, 80) },  // 대장 페나코두스
        { 2160, I(Kind.Herbivore, 1.6f, 1, 80) },  // 페나코두스
        { 2161, I(Kind.Carnivore, 1.7f, 1, 80) },  // 데이노니쿠스
        { 2162, I(Kind.Herbivore, 3.6f, 1, 80) },  // 갈리미무스
        { 2163, I(Kind.Herbivore, 3.6f, 1, 80) },  // 어린 갈리미무스
        { 2164, I(Kind.Herbivore, 4.0f, 25, 80) },  // 수컷 메갈로케로스
        { 2165, I(Kind.Herbivore, 4.0f, 25, 80) },  // 어린 메갈로케로스
        { 2166, I(Kind.Herbivore, 4.0f, 25, 80) },  // 암컷 메갈로케로스
        { 2167, I(Kind.Herbivore, 1.6f, 1, 80) },  // 스컹코두스
        { 2168, I(Kind.Herbivore, 1.3f, 1, 80) },  // 센트로사우루스
        { 2169, I(Kind.Herbivore, 4.4f, 1, 80) },  // 작은 브라키오사우루스
        { 2170, I(Kind.Carnivore, 3.6f, 1, 80) },  // 앤드루사르쿠스
        { 2171, I(Kind.Herbivore, 4.0f, 1, 80) },  // 투구게
        { 2172, I(Kind.Carnivore, 2.0f, 1, 80) },  // 불점박이 이구아나
        { 2173, I(Kind.Herbivore, 1.6f, 1, 80) },  // 해녀 스컹코두스
        { 2174, I(Kind.Herbivore, 1.6f, 1, 80) },  // 유황 스컹코두스
        { 2175, I(Kind.Carnivore, 1.7f, 1, 80) },  // 검은머리 유타랍토르
        { 2176, I(Kind.Carnivore, 1.4f, 1, 80) },  // 흰줄무늬 이구아나
        { 2177, I(Kind.Herbivore, 2.2f, 1, 80) },  // 용암 하드로사우루스
        { 2178, I(Kind.Herbivore, 1.4f, 1, 80) },  // 화산재 카스모사우루스
        { 2179, I(Kind.Herbivore, 1.9f, 1, 80) },  // 아마르가사우루스
        { 2180, I(Kind.Herbivore, 1.6f, 1, 80) },  // 축제 엘리펀툴루스
        { 2181, I(Kind.Herbivore, 1.6f, 1, 80) },  // 상어 페나코두스
        { 2182, I(Kind.Herbivore, 3.0f, 10, 60) },  // 광대 콤프소그나투스
        { 2183, I(Kind.Carnivore, 1.4f, 1, 80) },  // 오색비늘 이구아나
        { 2184, I(Kind.Herbivore, 4.4f, 1, 80) },  // 돌격전차 하드로사우루스
        { 2185, I(Kind.Herbivore, 2.2f, 1, 80) },  // 연구용 용암 하드로사우루스
        { 2186, I(Kind.Carnivore, 3.6f, 1, 80) },  // 청소부 앤드루사르쿠스
        { 2187, I(Kind.Herbivore, 1.7f, 1, 80) },  // 통신용 아마르가사우루스
        { 2188, I(Kind.Carnivore, 1.4f, 1, 80) },  // 독 도마뱀
        { 2189, I(Kind.Carnivore, 3.6f, 1, 80) },  // 미확인 가스토르니스
        { 2190, I(Kind.Carnivore, 3.6f, 1, 80) },  // 미확인 파보미무스
        { 2191, I(Kind.Carnivore, 1.8f, 1, 80) },  // 미확인 흰털다이어울프
        { 2192, I(Kind.Carnivore, 1.8f, 1, 80) },  // 어린 미확인 흰털다이어울프
        { 2193, I(Kind.Herbivore, 1.6f, 1, 80) },  // 미확인 땅거북안킬로
        { 2194, I(Kind.Herbivore, 3.2f, 1, 80) },  // 미확인 마크라우케니아
        { 2195, I(Kind.Herbivore, 3.6f, 1, 80) },  // 미확인 갈리미무스
        { 2196, I(Kind.Herbivore, 4.0f, 25, 80) },  // 미확인 수컷 메갈로케로스
        { 2197, I(Kind.Herbivore, 4.0f, 25, 80) },  // 미확인 어린 메갈로케로스
        { 2198, I(Kind.Herbivore, 4.0f, 25, 80) },  // 미확인 암컷 메갈로케로스
        { 2199, I(Kind.Carnivore, 2.1f, 1, 80) },  // 흰털스밀로돈
        { 2200, I(Kind.Herbivore, 4.0f, 1, 80) },  // 미확인 어린 흰배메갈로케로스
        { 2201, I(Kind.Herbivore, 4.0f, 1, 80) },  // 미확인 암컷 흰배메갈로케로스
        { 2202, I(Kind.Herbivore, 1.6f, 1, 80) },  // 미확인 안킬로사우루스
        { 2203, I(Kind.Carnivore, 1.8f, 1, 80) },  // 어린 미확인 검은 다이어울프
        { 2204, I(Kind.Carnivore, 1.8f, 1, 80) },  // 미확인 검은 다이어울프
        { 2205, I(Kind.Herbivore, 4.0f, 1, 80) },  // 미확인 수컷 흰배메갈로케로스
        { 2206, I(Kind.Carnivore, 1.8f, 1, 80) },  // 흰털다이어울프
        { 2207, I(Kind.Herbivore, 1.6f, 1, 80) },  // 미확인 땅거북안킬로
        { 2208, I(Kind.Herbivore, 3.2f, 1, 80) },  // 미확인 마크라우케니아
        { 2209, I(Kind.Herbivore, 3.6f, 1, 80) },  // 미확인 갈리미무스
        { 2210, I(Kind.Herbivore, 4.0f, 1, 80) },  // 미확인 수컷 흰배메갈로케로스
        { 2211, I(Kind.Herbivore, 4.0f, 1, 80) },  // 미확인 어린 흰배메갈로케로스
        { 2212, I(Kind.Herbivore, 4.0f, 1, 80) },  // 미확인 암컷 흰배메갈로케로스
        { 2999, I(Kind.Sandbag, 2.0f, 36, 54) },  // 더미 샌드백
    };

    public static bool TryGet(ushort entityType, out Info info) => All.TryGetValue(entityType, out info);
}
