// สร้างด้วย scripts/extract_region_templates.py จาก data/assets/region_templates.json — อย่าแก้มือ
//
// "ใบสั่งเกิดสัตว์" ของแต่ละเกาะตามเกมต้นฉบับ: ฝูงต่อกลุ่มพื้นที่ (land/beach/lake_*/ocean)
// รหัสฝูงในไฟล์เกม = ชนิด×100 + จำนวนตัว (201520 = ชนิด 2015 ฝูงละ 20) — ถอดไว้ให้แล้ว
// ชื่อ template = <terrain><YYMMDD> หรือ <terrain>SubNN — ดู RegionTemplateData.Find

using System;
using System.Collections.Generic;

namespace DurangoServer.Core;

public static class RegionTemplateData
{
    public readonly struct HerdSpec
    {
        /// <summary>กลุ่มพื้นที่ใน herds.yml: land / beach / lake_shallow / lake_deep / ocean</summary>
        public readonly string Group;
        public readonly ushort EntityType;
        /// <summary>ตัวต่อฝูง (2 หลักท้ายของรหัสฝูง)</summary>
        public readonly int Size;
        /// <summary>กี่ฝูง</summary>
        public readonly int Count;
        public HerdSpec(string group, ushort entityType, int size, int count)
        {
            Group = group; EntityType = entityType; Size = size; Count = count;
        }
    }

    public sealed class Template
    {
        public string Name = "";
        public int Level;
        public int DesiredPopulation;
        public HerdSpec[] Herds = Array.Empty<HerdSpec>();
        /// <summary>หลุมอุกกาบาต: ชนิดที่สุ่ม (ratio) และจำนวนหลุม</summary>
        public (ushort EntityType, float Ratio)[] CraterSpecies = Array.Empty<(ushort, float)>();
        public int CraterCount;
        /// <summary>รหัสฝูงเต็ม (ชนิด×100+ตัว) สำหรับหลุมอุกกาบาตที่ปิด</summary>
        public int ClosedCraterHerdType;
        public int TotalAnimals
        {
            get { int n = 0; foreach (HerdSpec h in Herds) { n += h.Size * h.Count; } return n; }
        }
    }

    private static HerdSpec H(string g, ushort t, int s, int c) => new HerdSpec(g, t, s, c);

    public static readonly Dictionary<string, Template> All = new Dictionary<string, Template>(StringComparer.Ordinal)
    {
        { "ev40te180312", new Template
        {
            Name = "ev40te180312", Level = 40, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2006, 20, 1),
                H("land", 2015, 20, 2),
            },
            CraterSpecies = new (ushort, float)[] { (2080, 2.0f) },
            CraterCount = 2, ClosedCraterHerdType = 0,
        } },
        { "ev45te180803", new Template
        {
            Name = "ev45te180803", Level = 45, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2006, 20, 3),
                H("land", 2017, 20, 3),
                H("land", 2022, 10, 6),
                H("land", 2033, 20, 2),
            },
            CraterSpecies = new (ushort, float)[] { (2033, 2.0f), (2081, 4.0f), (2082, 4.0f), (2083, 4.0f) },
            CraterCount = 14, ClosedCraterHerdType = 202220,
        } },
        { "ev50tr180723", new Template
        {
            Name = "ev50tr180723", Level = 50, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2086, 10, 1),
                H("land", 2093, 20, 3),
                H("land", 2139, 10, 5),
            },
            CraterSpecies = new (ushort, float)[] { (3151, 1.0f) },
            CraterCount = 3, ClosedCraterHerdType = 209320,
        } },
        { "op60te170615", new Template
        {
            Name = "op60te170615", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 0, 0, 10),
                H("land", 2011, 20, 10),
                H("land", 2013, 21, 10),
                H("land", 2029, 20, 10),
                H("land", 2033, 20, 10),
                H("land", 2041, 20, 10),
                H("land", 2047, 20, 10),
                H("land", 2069, 20, 10),
                H("land", 2087, 20, 10),
                H("land", 2093, 20, 10),
                H("land", 2094, 20, 10),
            },
            CraterSpecies = new (ushort, float)[] { (2031, 2.0f), (2032, 1.0f), (2033, 4.0f), (2034, 4.0f), (2035, 4.0f), (2036, 4.0f), (2037, 4.0f), (2038, 4.0f), (2039, 4.0f) },
            CraterCount = 70, ClosedCraterHerdType = 202920,
        } },
        { "op60te171228", new Template
        {
            Name = "op60te171228", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2010, 40, 1),
                H("land", 2011, 20, 7),
                H("land", 2013, 21, 7),
                H("land", 2020, 30, 21),
                H("land", 2023, 30, 21),
                H("land", 2024, 20, 7),
                H("land", 2025, 30, 21),
                H("land", 2029, 20, 7),
                H("land", 2033, 20, 7),
                H("land", 2041, 20, 7),
                H("land", 2047, 40, 3),
                H("land", 2069, 20, 7),
                H("land", 2087, 20, 7),
                H("land", 2093, 20, 7),
                H("land", 2094, 20, 7),
            },
            CraterSpecies = new (ushort, float)[] { (2031, 2.0f), (2032, 1.0f), (2033, 4.0f), (2034, 4.0f), (2035, 4.0f), (2036, 4.0f), (2037, 4.0f), (2038, 4.0f), (2039, 4.0f) },
            CraterCount = 50, ClosedCraterHerdType = 202920,
        } },
        { "op60te180220", new Template
        {
            Name = "op60te180220", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2010, 40, 1),
                H("land", 2011, 20, 7),
                H("land", 2013, 21, 7),
                H("land", 2020, 30, 21),
                H("land", 2023, 30, 21),
                H("land", 2024, 20, 7),
                H("land", 2025, 30, 21),
                H("land", 2029, 20, 7),
                H("land", 2033, 20, 7),
                H("land", 2041, 20, 7),
                H("land", 2047, 40, 3),
                H("land", 2069, 20, 7),
                H("land", 2087, 20, 7),
                H("land", 2093, 20, 7),
                H("land", 2094, 20, 7),
            },
            CraterSpecies = new (ushort, float)[] { (2031, 2.0f), (2032, 1.0f), (2033, 4.0f), (2034, 4.0f), (2035, 4.0f), (2036, 4.0f), (2037, 4.0f), (2038, 4.0f), (2039, 4.0f) },
            CraterCount = 50, ClosedCraterHerdType = 202920,
        } },
        { "op60tr170615", new Template
        {
            Name = "op60tr170615", Level = 60, DesiredPopulation = 9999999,
            Herds = new[] {
                H("land", 2000, 21, 10),
                H("land", 2001, 20, 10),
                H("land", 2004, 21, 10),
                H("land", 2009, 20, 10),
                H("land", 2012, 20, 10),
                H("land", 2028, 20, 10),
                H("land", 2036, 20, 10),
                H("land", 2042, 20, 10),
                H("land", 2078, 20, 10),
                H("land", 2083, 21, 10),
            },
            CraterCount = 0, ClosedCraterHerdType = 201520,
        } },
        { "op60tr170712", new Template
        {
            Name = "op60tr170712", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2000, 21, 10),
                H("land", 2001, 20, 10),
                H("land", 2004, 21, 10),
                H("land", 2009, 20, 10),
                H("land", 2012, 20, 10),
                H("land", 2028, 20, 10),
                H("land", 2036, 20, 10),
                H("land", 2042, 20, 10),
                H("land", 2078, 20, 10),
                H("land", 2083, 21, 10),
            },
            CraterSpecies = new (ushort, float)[] { (3060, 5.0f), (3061, 5.0f), (3062, 5.0f), (3063, 5.0f), (3064, 5.0f), (3065, 5.0f), (3066, 5.0f), (3067, 5.0f), (3068, 5.0f), (3069, 5.0f), (3070, 5.0f) },
            CraterCount = 55, ClosedCraterHerdType = 200120,
        } },
        { "op60tr171228", new Template
        {
            Name = "op60tr171228", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2000, 21, 7),
                H("land", 2001, 20, 7),
                H("land", 2004, 21, 7),
                H("land", 2009, 20, 7),
                H("land", 2010, 40, 1),
                H("land", 2012, 20, 7),
                H("land", 2020, 30, 21),
                H("land", 2023, 30, 21),
                H("land", 2025, 30, 21),
                H("land", 2028, 20, 7),
                H("land", 2036, 20, 7),
                H("land", 2042, 20, 10),
                H("land", 2078, 20, 7),
                H("land", 2083, 21, 7),
                H("land", 2088, 30, 3),
            },
            CraterSpecies = new (ushort, float)[] { (3060, 5.0f), (3061, 5.0f), (3062, 5.0f), (3063, 5.0f), (3064, 5.0f), (3065, 5.0f), (3066, 5.0f), (3067, 5.0f), (3068, 5.0f), (3069, 5.0f), (3070, 5.0f) },
            CraterCount = 50, ClosedCraterHerdType = 200120,
        } },
        { "op60tr180220", new Template
        {
            Name = "op60tr180220", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2000, 21, 7),
                H("land", 2001, 20, 7),
                H("land", 2004, 21, 7),
                H("land", 2009, 20, 7),
                H("land", 2010, 40, 1),
                H("land", 2012, 20, 7),
                H("land", 2020, 30, 21),
                H("land", 2023, 30, 21),
                H("land", 2025, 30, 21),
                H("land", 2028, 20, 7),
                H("land", 2036, 20, 7),
                H("land", 2042, 20, 10),
                H("land", 2078, 20, 7),
                H("land", 2083, 21, 7),
                H("land", 2088, 30, 3),
            },
            CraterSpecies = new (ushort, float)[] { (3060, 5.0f), (3061, 5.0f), (3062, 5.0f), (3063, 5.0f), (3064, 5.0f), (3065, 5.0f), (3066, 5.0f), (3067, 5.0f), (3068, 5.0f), (3069, 5.0f), (3070, 5.0f) },
            CraterCount = 50, ClosedCraterHerdType = 200120,
        } },
        { "ra30gr170615", new Template
        {
            Name = "ra30gr170615", Level = 30, DesiredPopulation = 50,
            Herds = new[] {
                H("land", 2046, 20, 1),
            },
            CraterCount = 0, ClosedCraterHerdType = 201520,
        } },
        { "ra35sa170615", new Template
        {
            Name = "ra35sa170615", Level = 35, DesiredPopulation = 50,
            Herds = new[] {
                H("land", 2037, 20, 3),
                H("land", 2039, 20, 1),
            },
            CraterCount = 0, ClosedCraterHerdType = 201520,
        } },
        { "ra40de170615", new Template
        {
            Name = "ra40de170615", Level = 40, DesiredPopulation = 50,
            Herds = new[] {
                H("land", 2023, 20, 1),
                H("land", 2043, 20, 1),
            },
            CraterCount = 0, ClosedCraterHerdType = 201520,
        } },
        { "ra40de170712", new Template
        {
            Name = "ra40de170712", Level = 40, DesiredPopulation = 50,
            Herds = new[] {
                H("land", 2023, 20, 1),
                H("land", 2043, 20, 1),
            },
            CraterCount = 0, ClosedCraterHerdType = 202320,
        } },
        { "ra45tu170615", new Template
        {
            Name = "ra45tu170615", Level = 45, DesiredPopulation = 50,
            Herds = new[] {
                H("land", 2020, 20, 1),
                H("land", 2033, 20, 3),
            },
            CraterCount = 0, ClosedCraterHerdType = 201520,
        } },
        { "ra50tr170615", new Template
        {
            Name = "ra50tr170615", Level = 50, DesiredPopulation = 50,
            Herds = new[] {
                H("land", 2001, 20, 2),
                H("land", 2078, 20, 2),
            },
            CraterCount = 0, ClosedCraterHerdType = 201520,
        } },
        { "ra55te170615", new Template
        {
            Name = "ra55te170615", Level = 55, DesiredPopulation = 50,
            Herds = new[] {
                H("land", 2000, 21, 1),
                H("land", 2015, 21, 2),
            },
            CraterCount = 0, ClosedCraterHerdType = 201520,
        } },
        { "ra60sn170615", new Template
        {
            Name = "ra60sn170615", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2035, 20, 7),
                H("land", 2077, 20, 7),
            },
            CraterSpecies = new (ushort, float)[] { (6060, 1.0f) },
            CraterCount = 1, ClosedCraterHerdType = 201520,
        } },
        { "ra60sn171228", new Template
        {
            Name = "ra60sn171228", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2035, 20, 7),
                H("land", 2077, 20, 7),
            },
            CraterSpecies = new (ushort, float)[] { (6060, 1.0f) },
            CraterCount = 1, ClosedCraterHerdType = 201520,
        } },
        { "ra60sw180226", new Template
        {
            Name = "ra60sw180226", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2032, 20, 3),
                H("land", 2045, 20, 1),
            },
            CraterSpecies = new (ushort, float)[] { (8080, 1.0f) },
            CraterCount = 1, ClosedCraterHerdType = 0,
        } },
        { "ri15sa170531", new Template
        {
            Name = "ri15sa170531", Level = 15, DesiredPopulation = 50,
            Herds = new[] {
                H("land", 2002, 4, 10),
                H("land", 2077, 4, 10),
                H("land", 2078, 4, 50),
            },
            CraterCount = 0, ClosedCraterHerdType = 201520,
        } },
        { "ri15sa170615", new Template
        {
            Name = "ri15sa170615", Level = 15, DesiredPopulation = 50,
            Herds = new[] {
                H("land", 2027, 20, 10),
                H("land", 2037, 20, 10),
                H("land", 2039, 20, 10),
                H("land", 2042, 20, 10),
            },
            CraterCount = 0, ClosedCraterHerdType = 204220,
        } },
        { "ri15sa170724", new Template
        {
            Name = "ri15sa170724", Level = 15, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2027, 20, 10),
                H("land", 2037, 20, 10),
                H("land", 2039, 20, 10),
                H("land", 2042, 20, 10),
            },
            CraterSpecies = new (ushort, float)[] { (1150, 2.0f), (1151, 2.0f), (1152, 2.0f), (1153, 2.0f), (1154, 2.0f), (1155, 2.0f), (1157, 2.0f), (1158, 3.0f), (1159, 3.0f) },
            CraterCount = 20, ClosedCraterHerdType = 204220,
        } },
        { "ri15sa171228", new Template
        {
            Name = "ri15sa171228", Level = 15, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2027, 20, 8),
                H("land", 2037, 20, 8),
                H("land", 2039, 20, 16),
                H("land", 2042, 20, 8),
            },
            CraterSpecies = new (ushort, float)[] { (1150, 2.0f), (1151, 2.0f), (1152, 2.0f), (1153, 2.0f), (1157, 2.0f), (1158, 3.0f), (1170, 3.0f) },
            CraterCount = 15, ClosedCraterHerdType = 204220,
        } },
        { "ri15sa190710", new Template
        {
            Name = "ri15sa190710", Level = 15, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2027, 20, 8),
                H("land", 2037, 20, 8),
                H("land", 2039, 20, 16),
                H("land", 2042, 20, 8),
            },
            CraterSpecies = new (ushort, float)[] { (1150, 2.0f), (1151, 2.0f), (1152, 2.0f), (1153, 2.0f), (1157, 2.0f), (1158, 3.0f), (1170, 3.0f) },
            CraterCount = 15, ClosedCraterHerdType = 204220,
        } },
        { "ri15trMain01_summerclub", new Template
        {
            Name = "ri15trMain01_summerclub", Level = 20, DesiredPopulation = 9000,
            Herds = new[] {
                H("beach", 2181, 20, 10),
            },
            CraterCount = 0, ClosedCraterHerdType = 0,
        } },
        { "ri18tr_01_01", new Template
        {
            Name = "ri18tr_01_01", Level = 18, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2001, 20, 5),
                H("land", 2028, 20, 5),
                H("land", 2078, 20, 5),
                H("land", 2199, 30, 15),
            },
            CraterSpecies = new (ushort, float)[] { (3157, 1.0f), (3158, 1.0f), (3160, 1.0f) },
            CraterCount = 8, ClosedCraterHerdType = 219930,
        } },
        { "ri18tr_02_01", new Template
        {
            Name = "ri18tr_02_01", Level = 18, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2001, 20, 2),
                H("land", 2028, 20, 2),
                H("land", 2036, 20, 3),
                H("land", 2040, 20, 2),
                H("land", 2078, 20, 2),
            },
            CraterCount = 0, ClosedCraterHerdType = 203620,
        } },
        { "ri18tr_03_01", new Template
        {
            Name = "ri18tr_03_01", Level = 18, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2001, 20, 1),
                H("land", 2028, 20, 1),
                H("land", 2036, 20, 1),
                H("land", 2078, 20, 1),
            },
            CraterSpecies = new (ushort, float)[] { (3164, 1.0f) },
            CraterCount = 1, ClosedCraterHerdType = 203620,
        } },
        { "ri18tr_04_01", new Template
        {
            Name = "ri18tr_04_01", Level = 18, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2001, 20, 2),
                H("land", 2028, 20, 2),
                H("land", 2036, 20, 3),
                H("land", 2040, 20, 2),
                H("land", 2078, 20, 2),
            },
            CraterSpecies = new (ushort, float)[] { (3022, 2.0f), (3023, 2.0f), (3024, 1.0f), (3025, 2.0f), (3026, 2.0f), (3027, 2.0f), (3028, 2.0f), (3031, 2.0f) },
            CraterCount = 5, ClosedCraterHerdType = 203620,
        } },
        { "ri18tr_05_01", new Template
        {
            Name = "ri18tr_05_01", Level = 18, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2028, 20, 1),
                H("land", 2036, 20, 1),
                H("land", 2078, 20, 1),
            },
            CraterSpecies = new (ushort, float)[] { (3022, 2.0f), (3023, 2.0f), (3024, 1.0f), (3025, 2.0f), (3026, 2.0f), (3027, 2.0f), (3028, 2.0f) },
            CraterCount = 5, ClosedCraterHerdType = 203620,
        } },
        { "ri18tr_05_02", new Template
        {
            Name = "ri18tr_05_02", Level = 18, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2001, 20, 1),
                H("land", 2028, 20, 1),
                H("land", 2036, 20, 1),
                H("land", 2078, 20, 1),
            },
            CraterSpecies = new (ushort, float)[] { (3165, 1.0f) },
            CraterCount = 1, ClosedCraterHerdType = 203620,
        } },
        { "ri18tr_05_03", new Template
        {
            Name = "ri18tr_05_03", Level = 18, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2001, 20, 1),
                H("land", 2028, 20, 1),
                H("land", 2036, 20, 1),
                H("land", 2078, 20, 1),
            },
            CraterSpecies = new (ushort, float)[] { (3166, 1.0f) },
            CraterCount = 1, ClosedCraterHerdType = 203620,
        } },
        { "ri18tr_06_01", new Template
        {
            Name = "ri18tr_06_01", Level = 18, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2207, 10, 1),
                H("land", 2208, 10, 1),
                H("land", 2209, 10, 1),
                H("land", 2210, 10, 1),
            },
            CraterSpecies = new (ushort, float)[] { (3167, 1.0f) },
            CraterCount = 1, ClosedCraterHerdType = 203620,
        } },
        { "ri18tr_06_02", new Template
        {
            Name = "ri18tr_06_02", Level = 18, DesiredPopulation = 9000,
            Herds = new[] {
                H("beach", 2177, 30, 7),
                H("beach", 2189, 4, 1),
                H("beach", 2190, 4, 1),
                H("beach", 2204, 16, 1),
            },
            CraterCount = 0, ClosedCraterHerdType = 203620,
        } },
        { "ri18tr_07_01", new Template
        {
            Name = "ri18tr_07_01", Level = 18, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2001, 20, 1),
                H("land", 2028, 20, 1),
                H("land", 2036, 20, 1),
                H("land", 2078, 20, 1),
            },
            CraterSpecies = new (ushort, float)[] { (3022, 2.0f), (3023, 2.0f), (3024, 1.0f), (3025, 2.0f), (3026, 2.0f), (3027, 2.0f), (3028, 0.0f) },
            CraterCount = 5, ClosedCraterHerdType = 203620,
        } },
        { "ri18tr_test", new Template
        {
            Name = "ri18tr_test", Level = 18, DesiredPopulation = 9000,
            CraterSpecies = new (ushort, float)[] { (3022, 2.0f), (3023, 2.0f), (3024, 1.0f), (3025, 2.0f), (3026, 2.0f), (3027, 2.0f), (3028, 2.0f) },
            CraterCount = 5, ClosedCraterHerdType = 0,
        } },
        { "ri20te170601", new Template
        {
            Name = "ri20te170601", Level = 20, DesiredPopulation = 50,
            Herds = new[] {
                H("land", 2002, 4, 10),
                H("land", 2077, 4, 10),
                H("land", 2078, 4, 50),
            },
            CraterCount = 0, ClosedCraterHerdType = 201520,
        } },
        { "ri20te170615", new Template
        {
            Name = "ri20te170615", Level = 20, DesiredPopulation = 50,
            Herds = new[] {
                H("land", 2002, 20, 10),
                H("land", 2006, 20, 10),
                H("land", 2012, 20, 10),
                H("land", 2015, 20, 10),
                H("land", 2017, 20, 10),
            },
            CraterCount = 0, ClosedCraterHerdType = 201520,
        } },
        { "ri20te170712", new Template
        {
            Name = "ri20te170712", Level = 20, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2002, 20, 10),
                H("land", 2006, 20, 10),
                H("land", 2012, 20, 10),
                H("land", 2015, 20, 10),
                H("land", 2017, 20, 10),
            },
            CraterSpecies = new (ushort, float)[] { (2012, 2.0f), (2013, 2.0f), (2014, 3.0f), (2015, 3.0f), (2016, 3.0f), (2017, 3.0f), (2018, 4.0f) },
            CraterCount = 20, ClosedCraterHerdType = 201520,
        } },
        { "ri20te171228", new Template
        {
            Name = "ri20te171228", Level = 20, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2002, 20, 8),
                H("land", 2006, 20, 6),
                H("land", 2012, 20, 8),
                H("land", 2015, 20, 8),
                H("land", 2017, 20, 8),
                H("beach", 2182, 5, 6),
            },
            CraterSpecies = new (ushort, float)[] { (2012, 2.0f), (2013, 2.0f), (2015, 3.0f), (2016, 3.0f), (2017, 3.0f), (2018, 4.0f) },
            CraterCount = 15, ClosedCraterHerdType = 201520,
        } },
        { "ri20te190710", new Template
        {
            Name = "ri20te190710", Level = 20, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2002, 20, 8),
                H("land", 2006, 20, 6),
                H("land", 2012, 20, 8),
                H("land", 2015, 20, 8),
                H("land", 2017, 20, 8),
            },
            CraterSpecies = new (ushort, float)[] { (2012, 2.0f), (2013, 2.0f), (2015, 3.0f), (2016, 3.0f), (2017, 3.0f), (2018, 4.0f) },
            CraterCount = 15, ClosedCraterHerdType = 201520,
        } },
        { "ri25tr170602", new Template
        {
            Name = "ri25tr170602", Level = 25, DesiredPopulation = 50,
            Herds = new[] {
                H("land", 2002, 4, 10),
                H("land", 2077, 4, 10),
                H("land", 2078, 4, 50),
            },
            CraterCount = 0, ClosedCraterHerdType = 201520,
        } },
        { "ri25tr170615", new Template
        {
            Name = "ri25tr170615", Level = 25, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2001, 20, 10),
                H("land", 2028, 20, 10),
                H("land", 2036, 20, 10),
                H("land", 2040, 20, 10),
                H("land", 2078, 20, 10),
            },
            CraterSpecies = new (ushort, float)[] { (3022, 2.0f), (3023, 2.0f), (3024, 2.0f), (3025, 2.0f), (3026, 2.0f), (3027, 3.0f), (3028, 2.0f) },
            CraterCount = 15, ClosedCraterHerdType = 203620,
        } },
        { "ri25tr171228", new Template
        {
            Name = "ri25tr171228", Level = 25, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2001, 20, 10),
                H("land", 2028, 20, 10),
                H("land", 2036, 20, 10),
                H("land", 2040, 20, 10),
                H("land", 2078, 20, 10),
            },
            CraterSpecies = new (ushort, float)[] { (3022, 2.0f), (3023, 2.0f), (3024, 1.0f), (3025, 2.0f), (3026, 2.0f), (3027, 2.0f), (3028, 2.0f), (3031, 2.0f) },
            CraterCount = 15, ClosedCraterHerdType = 203620,
        } },
        { "ri25tr180122", new Template
        {
            Name = "ri25tr180122", Level = 25, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2001, 20, 8),
                H("land", 2028, 20, 8),
                H("land", 2036, 20, 6),
                H("land", 2040, 20, 6),
                H("land", 2078, 20, 6),
                H("beach", 2182, 5, 6),
            },
            CraterSpecies = new (ushort, float)[] { (3022, 2.0f), (3023, 2.0f), (3024, 1.0f), (3025, 2.0f), (3026, 2.0f), (3027, 2.0f), (3028, 2.0f), (3031, 2.0f) },
            CraterCount = 15, ClosedCraterHerdType = 203620,
        } },
        { "ri25tr190710", new Template
        {
            Name = "ri25tr190710", Level = 25, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2001, 20, 8),
                H("land", 2028, 20, 8),
                H("land", 2036, 20, 6),
                H("land", 2040, 20, 6),
                H("land", 2078, 20, 6),
            },
            CraterSpecies = new (ushort, float)[] { (3022, 2.0f), (3023, 2.0f), (3024, 1.0f), (3025, 2.0f), (3026, 2.0f), (3027, 2.0f), (3028, 2.0f), (3031, 2.0f) },
            CraterCount = 15, ClosedCraterHerdType = 203620,
        } },
        { "ri30tu170601", new Template
        {
            Name = "ri30tu170601", Level = 30, DesiredPopulation = 50,
            Herds = new[] {
                H("land", 2002, 4, 10),
                H("land", 2077, 4, 10),
                H("land", 2078, 4, 50),
            },
            CraterCount = 0, ClosedCraterHerdType = 201520,
        } },
        { "ri30tu170615", new Template
        {
            Name = "ri30tu170615", Level = 30, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2013, 20, 10),
                H("land", 2016, 20, 10),
                H("land", 2020, 20, 10),
                H("land", 2033, 20, 10),
                H("land", 2041, 20, 10),
            },
            CraterSpecies = new (ushort, float)[] { (4016, 3.0f), (4017, 3.0f), (4018, 3.0f), (4019, 2.0f), (4020, 2.0f), (4021, 2.0f) },
            CraterCount = 15, ClosedCraterHerdType = 203320,
        } },
        { "ri30tu171228", new Template
        {
            Name = "ri30tu171228", Level = 30, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2013, 20, 6),
                H("land", 2016, 20, 6),
                H("land", 2020, 20, 6),
                H("land", 2033, 20, 6),
                H("land", 2041, 20, 6),
            },
            CraterSpecies = new (ushort, float)[] { (4016, 3.0f), (4017, 3.0f), (4018, 3.0f), (4019, 2.0f), (4020, 2.0f), (4021, 2.0f) },
            CraterCount = 15, ClosedCraterHerdType = 203320,
        } },
        { "ri30tuSub01", new Template
        {
            Name = "ri30tuSub01", Level = 30, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2013, 20, 3),
                H("land", 2016, 20, 3),
                H("land", 2020, 20, 3),
                H("land", 2033, 20, 3),
                H("land", 2041, 20, 3),
            },
            CraterSpecies = new (ushort, float)[] { (4016, 3.0f), (4017, 3.0f), (4018, 3.0f), (4019, 2.0f), (4020, 2.0f), (4021, 2.0f) },
            CraterCount = 6, ClosedCraterHerdType = 203320,
        } },
        { "ri30tuSub02", new Template
        {
            Name = "ri30tuSub02", Level = 30, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2013, 20, 3),
                H("land", 2016, 20, 3),
                H("land", 2020, 20, 3),
                H("land", 2033, 20, 3),
                H("land", 2041, 20, 3),
            },
            CraterSpecies = new (ushort, float)[] { (4016, 3.0f), (4017, 3.0f), (4018, 3.0f), (4019, 2.0f), (4020, 2.0f), (4021, 2.0f) },
            CraterCount = 6, ClosedCraterHerdType = 203320,
        } },
        { "ri30tuSub03_camp", new Template
        {
            Name = "ri30tuSub03_camp", Level = 30, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2013, 20, 3),
                H("land", 2016, 20, 3),
                H("land", 2020, 20, 3),
                H("land", 2033, 20, 3),
                H("land", 2041, 20, 3),
            },
            CraterSpecies = new (ushort, float)[] { (4016, 3.0f), (4017, 3.0f), (4018, 3.0f), (4019, 2.0f), (4020, 2.0f), (4021, 2.0f) },
            CraterCount = 3, ClosedCraterHerdType = 203320,
        } },
        { "ri35de170601", new Template
        {
            Name = "ri35de170601", Level = 35, DesiredPopulation = 50,
            Herds = new[] {
                H("land", 2002, 4, 10),
                H("land", 2077, 4, 10),
                H("land", 2078, 4, 50),
            },
            CraterCount = 0, ClosedCraterHerdType = 201520,
        } },
        { "ri35de170615", new Template
        {
            Name = "ri35de170615", Level = 35, DesiredPopulation = 50,
            Herds = new[] {
                H("land", 2002, 20, 10),
                H("land", 2023, 20, 10),
                H("land", 2025, 20, 10),
                H("land", 2043, 20, 10),
                H("land", 2083, 20, 10),
            },
            CraterCount = 0, ClosedCraterHerdType = 200220,
        } },
        { "ri35de170712", new Template
        {
            Name = "ri35de170712", Level = 35, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2002, 20, 10),
                H("land", 2023, 20, 10),
                H("land", 2025, 20, 10),
                H("land", 2043, 20, 10),
                H("land", 2083, 20, 10),
            },
            CraterSpecies = new (ushort, float)[] { (5035, 4.0f), (5036, 3.0f), (5037, 2.0f), (5038, 2.0f), (5039, 3.0f), (5040, 4.0f) },
            CraterCount = 18, ClosedCraterHerdType = 200220,
        } },
        { "ri35de171228", new Template
        {
            Name = "ri35de171228", Level = 35, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2002, 20, 8),
                H("land", 2023, 20, 8),
                H("land", 2025, 20, 8),
                H("land", 2043, 20, 8),
                H("land", 2083, 20, 8),
            },
            CraterSpecies = new (ushort, float)[] { (5035, 4.0f), (5036, 3.0f), (5037, 2.0f), (5038, 2.0f), (5039, 3.0f), (5040, 4.0f) },
            CraterCount = 18, ClosedCraterHerdType = 200220,
        } },
        { "ri35deSub01", new Template
        {
            Name = "ri35deSub01", Level = 35, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2002, 20, 4),
                H("land", 2023, 20, 4),
                H("land", 2025, 20, 4),
                H("land", 2043, 20, 4),
                H("land", 2083, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (5035, 4.0f), (5036, 3.0f), (5037, 2.0f), (5038, 2.0f), (5039, 3.0f), (5040, 4.0f) },
            CraterCount = 8, ClosedCraterHerdType = 200220,
        } },
        { "ri35deSub02", new Template
        {
            Name = "ri35deSub02", Level = 35, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2002, 20, 4),
                H("land", 2023, 20, 4),
                H("land", 2025, 20, 4),
                H("land", 2043, 20, 4),
                H("land", 2083, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (5035, 4.0f), (5036, 3.0f), (5037, 2.0f), (5038, 2.0f), (5039, 3.0f), (5040, 4.0f) },
            CraterCount = 8, ClosedCraterHerdType = 200220,
        } },
        { "ri35deSub03_car", new Template
        {
            Name = "ri35deSub03_car", Level = 35, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2002, 20, 4),
                H("land", 2023, 20, 4),
                H("land", 2025, 20, 4),
                H("land", 2043, 20, 4),
                H("land", 2083, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (5035, 4.0f), (5036, 3.0f), (5037, 2.0f), (5038, 2.0f), (5039, 3.0f), (5040, 4.0f) },
            CraterCount = 5, ClosedCraterHerdType = 200220,
        } },
        { "ri35te170601", new Template
        {
            Name = "ri35te170601", Level = 35, DesiredPopulation = 50,
            Herds = new[] {
                H("land", 2002, 4, 10),
                H("land", 2077, 4, 10),
                H("land", 2078, 4, 50),
            },
            CraterCount = 0, ClosedCraterHerdType = 201520,
        } },
        { "ri35te170615", new Template
        {
            Name = "ri35te170615", Level = 35, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2000, 20, 10),
                H("land", 2002, 20, 10),
                H("land", 2012, 20, 10),
                H("land", 2015, 20, 10),
                H("land", 2017, 20, 10),
                H("land", 2093, 20, 10),
            },
            CraterSpecies = new (ushort, float)[] { (2019, 2.0f), (2020, 2.0f), (2021, 2.0f), (2022, 2.0f), (2023, 2.0f), (2024, 2.0f), (2025, 2.0f) },
            CraterCount = 14, ClosedCraterHerdType = 201520,
        } },
        { "ri35te171228", new Template
        {
            Name = "ri35te171228", Level = 35, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2000, 20, 8),
                H("land", 2002, 20, 8),
                H("land", 2012, 20, 8),
                H("land", 2015, 20, 8),
                H("land", 2017, 20, 8),
                H("land", 2093, 20, 8),
                H("beach", 2182, 5, 6),
            },
            CraterSpecies = new (ushort, float)[] { (2019, 2.0f), (2020, 2.0f), (2021, 2.0f), (2022, 2.0f), (2023, 2.0f), (2024, 2.0f), (2025, 2.0f) },
            CraterCount = 14, ClosedCraterHerdType = 201520,
        } },
        { "ri35teSub01", new Template
        {
            Name = "ri35teSub01", Level = 35, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2000, 20, 4),
                H("land", 2002, 20, 4),
                H("land", 2012, 20, 4),
                H("land", 2015, 20, 4),
                H("land", 2017, 20, 4),
                H("land", 2093, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (2019, 2.0f), (2020, 2.0f), (2021, 2.0f), (2022, 2.0f), (2023, 2.0f), (2024, 2.0f), (2025, 2.0f) },
            CraterCount = 6, ClosedCraterHerdType = 201520,
        } },
        { "ri35teSub02", new Template
        {
            Name = "ri35teSub02", Level = 35, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2000, 20, 4),
                H("land", 2002, 20, 4),
                H("land", 2012, 20, 4),
                H("land", 2015, 20, 4),
                H("land", 2017, 20, 4),
                H("land", 2093, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (2019, 2.0f), (2020, 2.0f), (2021, 2.0f), (2022, 2.0f), (2023, 2.0f), (2024, 2.0f), (2025, 2.0f) },
            CraterCount = 6, ClosedCraterHerdType = 201520,
        } },
        { "ri35teSub03_copper", new Template
        {
            Name = "ri35teSub03_copper", Level = 35, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2000, 20, 4),
                H("land", 2002, 20, 4),
                H("land", 2012, 20, 4),
                H("land", 2015, 20, 4),
                H("land", 2017, 20, 4),
                H("land", 2093, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (2019, 2.0f), (2020, 2.0f), (2021, 2.0f), (2022, 2.0f), (2023, 2.0f), (2024, 2.0f), (2025, 2.0f) },
            CraterCount = 4, ClosedCraterHerdType = 201520,
        } },
        { "ri40tr170601", new Template
        {
            Name = "ri40tr170601", Level = 40, DesiredPopulation = 50,
            Herds = new[] {
                H("land", 2002, 4, 10),
                H("land", 2077, 4, 10),
                H("land", 2078, 4, 50),
            },
            CraterCount = 0, ClosedCraterHerdType = 201520,
        } },
        { "ri40tr170615", new Template
        {
            Name = "ri40tr170615", Level = 40, DesiredPopulation = 50,
            Herds = new[] {
                H("land", 2001, 20, 10),
                H("land", 2028, 20, 10),
                H("land", 2029, 20, 10),
                H("land", 2036, 20, 10),
                H("land", 2040, 20, 10),
                H("land", 2078, 20, 10),
                H("land", 2088, 20, 10),
            },
            CraterCount = 0, ClosedCraterHerdType = 203620,
        } },
        { "ri40tr170712", new Template
        {
            Name = "ri40tr170712", Level = 40, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2001, 20, 10),
                H("land", 2028, 20, 10),
                H("land", 2029, 20, 10),
                H("land", 2036, 20, 10),
                H("land", 2040, 20, 10),
                H("land", 2078, 20, 10),
                H("land", 2088, 20, 10),
            },
            CraterSpecies = new (ushort, float)[] { (3040, 4.0f), (3041, 2.0f), (3042, 2.0f), (3043, 3.0f), (3044, 3.0f), (3045, 2.0f), (3046, 1.0f), (3051, 3.0f) },
            CraterCount = 20, ClosedCraterHerdType = 203620,
        } },
        { "ri40tr171228", new Template
        {
            Name = "ri40tr171228", Level = 40, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2001, 20, 8),
                H("land", 2028, 20, 8),
                H("land", 2029, 20, 8),
                H("land", 2036, 20, 8),
                H("land", 2040, 20, 8),
                H("land", 2078, 20, 8),
                H("land", 2088, 20, 8),
                H("beach", 2182, 5, 6),
            },
            CraterSpecies = new (ushort, float)[] { (3040, 4.0f), (3041, 2.0f), (3042, 2.0f), (3043, 3.0f), (3044, 2.0f), (3045, 2.0f), (3046, 1.0f), (3051, 2.0f), (3052, 2.0f) },
            CraterCount = 20, ClosedCraterHerdType = 203620,
        } },
        { "ri40trSub01", new Template
        {
            Name = "ri40trSub01", Level = 40, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2001, 20, 4),
                H("land", 2028, 20, 4),
                H("land", 2029, 20, 4),
                H("land", 2036, 20, 4),
                H("land", 2040, 20, 4),
                H("land", 2078, 20, 4),
                H("land", 2088, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (3040, 4.0f), (3041, 2.0f), (3042, 2.0f), (3043, 3.0f), (3044, 2.0f), (3045, 2.0f), (3046, 1.0f), (3051, 2.0f), (3052, 2.0f) },
            CraterCount = 9, ClosedCraterHerdType = 203620,
        } },
        { "ri40trSub02", new Template
        {
            Name = "ri40trSub02", Level = 40, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2001, 20, 4),
                H("land", 2028, 20, 4),
                H("land", 2029, 20, 4),
                H("land", 2036, 20, 4),
                H("land", 2040, 20, 4),
                H("land", 2078, 20, 4),
                H("land", 2088, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (3040, 4.0f), (3041, 2.0f), (3042, 2.0f), (3043, 3.0f), (3044, 2.0f), (3045, 2.0f), (3046, 1.0f), (3051, 2.0f), (3052, 2.0f) },
            CraterCount = 9, ClosedCraterHerdType = 203620,
        } },
        { "ri40trSub03_car", new Template
        {
            Name = "ri40trSub03_car", Level = 40, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2001, 20, 4),
                H("land", 2028, 20, 4),
                H("land", 2029, 20, 4),
                H("land", 2036, 20, 4),
                H("land", 2040, 20, 4),
                H("land", 2078, 20, 4),
                H("land", 2088, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (3040, 4.0f), (3041, 2.0f), (3042, 2.0f), (3043, 3.0f), (3044, 2.0f), (3045, 2.0f), (3046, 1.0f), (3051, 2.0f), (3052, 2.0f) },
            CraterCount = 7, ClosedCraterHerdType = 203620,
        } },
        { "ri40tu170601", new Template
        {
            Name = "ri40tu170601", Level = 40, DesiredPopulation = 50,
            Herds = new[] {
                H("land", 2002, 4, 10),
                H("land", 2077, 4, 10),
                H("land", 2078, 4, 50),
            },
            CraterCount = 0, ClosedCraterHerdType = 201520,
        } },
        { "ri40tu170615", new Template
        {
            Name = "ri40tu170615", Level = 40, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2013, 20, 10),
                H("land", 2016, 20, 10),
                H("land", 2020, 20, 10),
                H("land", 2033, 20, 10),
                H("land", 2041, 20, 10),
                H("land", 2094, 20, 10),
            },
            CraterSpecies = new (ushort, float)[] { (4040, 2.0f), (4041, 2.0f), (4042, 2.0f), (4043, 2.0f), (4044, 2.0f), (4045, 2.0f), (4046, 2.0f), (4047, 1.0f) },
            CraterCount = 15, ClosedCraterHerdType = 203320,
        } },
        { "ri40tu171228", new Template
        {
            Name = "ri40tu171228", Level = 40, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2013, 20, 8),
                H("land", 2016, 20, 8),
                H("land", 2020, 20, 8),
                H("land", 2033, 20, 8),
                H("land", 2041, 20, 7),
                H("land", 2094, 20, 7),
            },
            CraterSpecies = new (ushort, float)[] { (4040, 2.0f), (4041, 2.0f), (4042, 2.0f), (4043, 2.0f), (4044, 2.0f), (4045, 2.0f), (4046, 2.0f), (4047, 1.0f) },
            CraterCount = 15, ClosedCraterHerdType = 203320,
        } },
        { "ri40tuSub01", new Template
        {
            Name = "ri40tuSub01", Level = 40, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2013, 20, 4),
                H("land", 2016, 20, 4),
                H("land", 2020, 20, 4),
                H("land", 2033, 20, 4),
                H("land", 2041, 20, 3),
                H("land", 2094, 20, 3),
            },
            CraterSpecies = new (ushort, float)[] { (4040, 2.0f), (4041, 2.0f), (4042, 2.0f), (4043, 2.0f), (4044, 2.0f), (4045, 2.0f), (4046, 2.0f), (4047, 1.0f) },
            CraterCount = 6, ClosedCraterHerdType = 203320,
        } },
        { "ri40tuSub02", new Template
        {
            Name = "ri40tuSub02", Level = 40, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2013, 20, 4),
                H("land", 2016, 20, 4),
                H("land", 2020, 20, 4),
                H("land", 2033, 20, 4),
                H("land", 2041, 20, 3),
                H("land", 2094, 20, 3),
            },
            CraterSpecies = new (ushort, float)[] { (4040, 2.0f), (4041, 2.0f), (4042, 2.0f), (4043, 2.0f), (4044, 2.0f), (4045, 2.0f), (4046, 2.0f), (4047, 1.0f) },
            CraterCount = 6, ClosedCraterHerdType = 203320,
        } },
        { "ri40tuSub03_powerbox", new Template
        {
            Name = "ri40tuSub03_powerbox", Level = 40, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2013, 20, 4),
                H("land", 2016, 20, 4),
                H("land", 2020, 20, 4),
                H("land", 2033, 20, 4),
                H("land", 2041, 20, 3),
                H("land", 2094, 20, 3),
            },
            CraterSpecies = new (ushort, float)[] { (4040, 2.0f), (4041, 2.0f), (4042, 2.0f), (4043, 2.0f), (4044, 2.0f), (4045, 2.0f), (4046, 2.0f), (4047, 1.0f) },
            CraterCount = 4, ClosedCraterHerdType = 203320,
        } },
        { "ri45sa170420", new Template
        {
            Name = "ri45sa170420", Level = 45, DesiredPopulation = 50,
            Herds = new[] {
                H("land", 2002, 4, 10),
                H("land", 2077, 4, 10),
                H("land", 2078, 4, 50),
            },
            CraterCount = 0, ClosedCraterHerdType = 201520,
        } },
        { "ri45sa170615", new Template
        {
            Name = "ri45sa170615", Level = 45, DesiredPopulation = 50,
            Herds = new[] {
                H("land", 2019, 20, 10),
                H("land", 2027, 20, 10),
                H("land", 2031, 20, 10),
                H("land", 2037, 20, 10),
                H("land", 2039, 20, 10),
                H("land", 2042, 20, 10),
                H("land", 2048, 20, 10),
            },
            CraterCount = 0, ClosedCraterHerdType = 204220,
        } },
        { "ri45sa170724", new Template
        {
            Name = "ri45sa170724", Level = 45, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2019, 20, 10),
                H("land", 2027, 20, 10),
                H("land", 2031, 20, 10),
                H("land", 2037, 20, 10),
                H("land", 2039, 20, 10),
                H("land", 2042, 20, 10),
                H("land", 2048, 20, 10),
            },
            CraterSpecies = new (ushort, float)[] { (1156, 2.0f), (1160, 1.0f), (1161, 1.0f), (1162, 1.0f), (1163, 1.0f), (1164, 1.0f), (1165, 1.0f), (1166, 1.0f), (1169, 1.0f) },
            CraterCount = 20, ClosedCraterHerdType = 204220,
        } },
        { "ri45sa171228", new Template
        {
            Name = "ri45sa171228", Level = 45, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2019, 20, 8),
                H("land", 2027, 20, 8),
                H("land", 2031, 20, 8),
                H("land", 2037, 20, 8),
                H("land", 2039, 20, 8),
                H("land", 2042, 20, 8),
                H("land", 2048, 20, 8),
                H("beach", 2182, 5, 6),
            },
            CraterSpecies = new (ushort, float)[] { (1156, 2.0f), (1160, 1.0f), (1161, 1.0f), (1162, 1.0f), (1163, 1.0f), (1164, 1.0f), (1165, 1.0f), (1166, 1.0f), (1169, 1.0f) },
            CraterCount = 20, ClosedCraterHerdType = 204220,
        } },
        { "ri45saSub01", new Template
        {
            Name = "ri45saSub01", Level = 45, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2019, 20, 4),
                H("land", 2027, 20, 4),
                H("land", 2031, 20, 4),
                H("land", 2037, 20, 4),
                H("land", 2039, 20, 4),
                H("land", 2042, 20, 4),
                H("land", 2048, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (1156, 2.0f), (1160, 1.0f), (1161, 1.0f), (1162, 1.0f), (1163, 1.0f), (1164, 1.0f), (1165, 1.0f), (1166, 1.0f), (1169, 1.0f) },
            CraterCount = 9, ClosedCraterHerdType = 204220,
        } },
        { "ri45saSub02", new Template
        {
            Name = "ri45saSub02", Level = 45, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2019, 20, 4),
                H("land", 2027, 20, 4),
                H("land", 2031, 20, 4),
                H("land", 2037, 20, 4),
                H("land", 2039, 20, 4),
                H("land", 2042, 20, 4),
                H("land", 2048, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (1156, 2.0f), (1160, 1.0f), (1161, 1.0f), (1162, 1.0f), (1163, 1.0f), (1164, 1.0f), (1165, 1.0f), (1166, 1.0f), (1169, 1.0f) },
            CraterCount = 9, ClosedCraterHerdType = 204220,
        } },
        { "ri45saSub03_mud", new Template
        {
            Name = "ri45saSub03_mud", Level = 45, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2019, 20, 4),
                H("land", 2027, 20, 4),
                H("land", 2031, 20, 4),
                H("land", 2037, 20, 4),
                H("land", 2039, 20, 4),
                H("land", 2042, 20, 4),
                H("land", 2048, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (1162, 1.0f) },
            CraterCount = 8, ClosedCraterHerdType = 204220,
        } },
        { "ri45sw170602", new Template
        {
            Name = "ri45sw170602", Level = 45, DesiredPopulation = 50,
            Herds = new[] {
                H("land", 2002, 4, 10),
                H("land", 2077, 4, 10),
                H("land", 2078, 4, 50),
            },
            CraterCount = 0, ClosedCraterHerdType = 201520,
        } },
        { "ri45sw170615", new Template
        {
            Name = "ri45sw170615", Level = 45, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2003, 20, 10),
                H("land", 2032, 20, 10),
                H("land", 2034, 20, 10),
                H("land", 2038, 20, 10),
                H("land", 2045, 20, 10),
            },
            CraterSpecies = new (ushort, float)[] { (8005, 1.0f), (8006, 1.0f), (8010, 1.0f), (8011, 1.0f) },
            CraterCount = 20, ClosedCraterHerdType = 203820,
        } },
        { "ri45sw171228", new Template
        {
            Name = "ri45sw171228", Level = 45, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2003, 20, 8),
                H("land", 2032, 20, 8),
                H("land", 2034, 20, 8),
                H("land", 2038, 20, 8),
                H("land", 2045, 20, 8),
            },
            CraterSpecies = new (ushort, float)[] { (8005, 1.0f), (8006, 1.0f), (8010, 1.0f), (8011, 1.0f) },
            CraterCount = 20, ClosedCraterHerdType = 203820,
        } },
        { "ri45swSub01", new Template
        {
            Name = "ri45swSub01", Level = 45, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2003, 20, 4),
                H("land", 2032, 20, 4),
                H("land", 2034, 20, 4),
                H("land", 2038, 20, 4),
                H("land", 2045, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (8005, 1.0f), (8006, 1.0f), (8010, 1.0f), (8011, 1.0f) },
            CraterCount = 6, ClosedCraterHerdType = 203820,
        } },
        { "ri45swSub02", new Template
        {
            Name = "ri45swSub02", Level = 45, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2003, 20, 4),
                H("land", 2032, 20, 4),
                H("land", 2034, 20, 4),
                H("land", 2038, 20, 4),
                H("land", 2045, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (8005, 1.0f), (8006, 1.0f), (8010, 1.0f), (8011, 1.0f) },
            CraterCount = 6, ClosedCraterHerdType = 203820,
        } },
        { "ri45swSub03_billboard", new Template
        {
            Name = "ri45swSub03_billboard", Level = 45, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2003, 20, 4),
                H("land", 2032, 20, 4),
                H("land", 2034, 20, 4),
                H("land", 2038, 20, 4),
                H("land", 2045, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (8005, 1.0f), (8006, 1.0f), (8010, 1.0f), (8011, 1.0f) },
            CraterCount = 3, ClosedCraterHerdType = 203820,
        } },
        { "ri50de170524", new Template
        {
            Name = "ri50de170524", Level = 50, DesiredPopulation = 50,
            Herds = new[] {
                H("land", 2002, 4, 10),
                H("land", 2077, 4, 10),
                H("land", 2078, 4, 50),
            },
            CraterCount = 0, ClosedCraterHerdType = 201520,
        } },
        { "ri50de170615", new Template
        {
            Name = "ri50de170615", Level = 50, DesiredPopulation = 50,
            Herds = new[] {
                H("land", 2002, 20, 10),
                H("land", 2010, 20, 10),
                H("land", 2023, 20, 10),
                H("land", 2025, 20, 10),
                H("land", 2026, 20, 10),
                H("land", 2043, 20, 10),
                H("land", 2083, 20, 10),
            },
            CraterCount = 0, ClosedCraterHerdType = 200220,
        } },
        { "ri50de170712", new Template
        {
            Name = "ri50de170712", Level = 50, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2002, 20, 10),
                H("land", 2010, 20, 10),
                H("land", 2023, 20, 10),
                H("land", 2025, 20, 10),
                H("land", 2026, 20, 10),
                H("land", 2043, 20, 10),
                H("land", 2083, 20, 10),
            },
            CraterSpecies = new (ushort, float)[] { (5051, 2.0f), (5052, 2.0f), (5053, 1.0f), (5054, 2.0f), (5055, 3.0f), (5056, 3.0f), (5057, 3.0f) },
            CraterCount = 20, ClosedCraterHerdType = 204320,
        } },
        { "ri50de171228", new Template
        {
            Name = "ri50de171228", Level = 50, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2002, 20, 8),
                H("land", 2010, 20, 8),
                H("land", 2023, 20, 8),
                H("land", 2025, 20, 8),
                H("land", 2026, 20, 8),
                H("land", 2043, 20, 8),
                H("land", 2083, 20, 8),
            },
            CraterSpecies = new (ushort, float)[] { (5051, 2.0f), (5052, 2.0f), (5053, 1.0f), (5054, 2.0f), (5055, 3.0f), (5056, 3.0f), (5057, 3.0f) },
            CraterCount = 20, ClosedCraterHerdType = 204320,
        } },
        { "ri50deSub01", new Template
        {
            Name = "ri50deSub01", Level = 50, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2002, 20, 4),
                H("land", 2010, 20, 4),
                H("land", 2023, 20, 4),
                H("land", 2025, 20, 4),
                H("land", 2026, 20, 4),
                H("land", 2043, 20, 4),
                H("land", 2083, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (5051, 2.0f), (5052, 2.0f), (5053, 1.0f), (5054, 2.0f), (5055, 3.0f), (5056, 3.0f), (5057, 3.0f) },
            CraterCount = 9, ClosedCraterHerdType = 204320,
        } },
        { "ri50deSub02", new Template
        {
            Name = "ri50deSub02", Level = 50, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2002, 20, 4),
                H("land", 2010, 20, 4),
                H("land", 2023, 20, 4),
                H("land", 2025, 20, 4),
                H("land", 2026, 20, 4),
                H("land", 2043, 20, 4),
                H("land", 2083, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (5051, 2.0f), (5052, 2.0f), (5053, 1.0f), (5054, 2.0f), (5055, 3.0f), (5056, 3.0f), (5057, 3.0f) },
            CraterCount = 9, ClosedCraterHerdType = 204320,
        } },
        { "ri50deSub03_overpass", new Template
        {
            Name = "ri50deSub03_overpass", Level = 50, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2002, 20, 4),
                H("land", 2010, 20, 4),
                H("land", 2023, 20, 4),
                H("land", 2025, 20, 4),
                H("land", 2026, 20, 4),
                H("land", 2043, 20, 4),
                H("land", 2083, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (5051, 2.0f), (5052, 2.0f), (5053, 1.0f), (5054, 2.0f), (5055, 3.0f), (5056, 3.0f), (5057, 3.0f) },
            CraterCount = 5, ClosedCraterHerdType = 204320,
        } },
        { "ri50sn170524", new Template
        {
            Name = "ri50sn170524", Level = 50, DesiredPopulation = 50,
            Herds = new[] {
                H("land", 2002, 4, 10),
                H("land", 2077, 4, 10),
                H("land", 2078, 4, 50),
            },
            CraterCount = 0, ClosedCraterHerdType = 201520,
        } },
        { "ri50sn170615", new Template
        {
            Name = "ri50sn170615", Level = 50, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2007, 20, 10),
                H("land", 2008, 20, 10),
                H("land", 2035, 20, 10),
                H("land", 2077, 20, 10),
                H("land", 2079, 21, 10),
                H("land", 2090, 20, 10),
            },
            CraterSpecies = new (ushort, float)[] { (6009, 2.0f), (6010, 2.0f), (6011, 2.0f), (6012, 3.0f), (6013, 2.0f), (6014, 3.0f), (6015, 2.0f) },
            CraterCount = 16, ClosedCraterHerdType = 203520,
        } },
        { "ri50sn171228", new Template
        {
            Name = "ri50sn171228", Level = 50, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2007, 20, 8),
                H("land", 2008, 20, 8),
                H("land", 2020, 10, 6),
                H("land", 2035, 20, 10),
                H("land", 2077, 20, 6),
                H("land", 2079, 21, 6),
                H("land", 2090, 20, 6),
            },
            CraterSpecies = new (ushort, float)[] { (6009, 2.0f), (6010, 2.0f), (6011, 2.0f), (6012, 3.0f), (6013, 2.0f), (6014, 3.0f), (6015, 2.0f) },
            CraterCount = 16, ClosedCraterHerdType = 202010,
        } },
        { "ri50snSub01", new Template
        {
            Name = "ri50snSub01", Level = 50, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2007, 20, 3),
                H("land", 2008, 20, 3),
                H("land", 2035, 20, 3),
                H("land", 2077, 20, 3),
                H("land", 2079, 21, 3),
                H("land", 2090, 20, 3),
            },
            CraterSpecies = new (ushort, float)[] { (6009, 2.0f), (6010, 2.0f), (6011, 2.0f), (6012, 3.0f), (6013, 2.0f), (6014, 3.0f), (6015, 2.0f) },
            CraterCount = 7, ClosedCraterHerdType = 202010,
        } },
        { "ri50snSub02", new Template
        {
            Name = "ri50snSub02", Level = 50, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2007, 20, 3),
                H("land", 2008, 20, 3),
                H("land", 2035, 20, 3),
                H("land", 2077, 20, 3),
                H("land", 2079, 21, 3),
                H("land", 2090, 20, 3),
            },
            CraterSpecies = new (ushort, float)[] { (6009, 2.0f), (6010, 2.0f), (6011, 2.0f), (6012, 3.0f), (6013, 2.0f), (6014, 3.0f), (6015, 2.0f) },
            CraterCount = 7, ClosedCraterHerdType = 202010,
        } },
        { "ri50snSub03_mammoth", new Template
        {
            Name = "ri50snSub03_mammoth", Level = 50, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2008, 20, 30),
            },
            CraterSpecies = new (ushort, float)[] { (6009, 2.0f), (6010, 2.0f), (6011, 2.0f), (6012, 3.0f), (6013, 2.0f), (6014, 3.0f), (6015, 2.0f) },
            CraterCount = 7, ClosedCraterHerdType = 202010,
        } },
        { "ri55sw171228", new Template
        {
            Name = "ri55sw171228", Level = 55, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2003, 20, 8),
                H("land", 2032, 20, 8),
                H("land", 2034, 20, 8),
                H("land", 2038, 20, 8),
                H("land", 2045, 20, 8),
            },
            CraterSpecies = new (ushort, float)[] { (8059, 1.0f), (8060, 1.0f), (8064, 1.0f), (8065, 1.0f) },
            CraterCount = 20, ClosedCraterHerdType = 203820,
        } },
        { "ri55swSub01", new Template
        {
            Name = "ri55swSub01", Level = 55, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2003, 20, 4),
                H("land", 2032, 20, 4),
                H("land", 2034, 20, 4),
                H("land", 2038, 20, 4),
                H("land", 2045, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (8059, 1.0f), (8060, 1.0f), (8064, 1.0f), (8065, 1.0f) },
            CraterCount = 7, ClosedCraterHerdType = 203820,
        } },
        { "ri55swSub02", new Template
        {
            Name = "ri55swSub02", Level = 55, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2003, 20, 4),
                H("land", 2032, 20, 4),
                H("land", 2034, 20, 4),
                H("land", 2038, 20, 4),
                H("land", 2045, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (8059, 1.0f), (8060, 1.0f), (8064, 1.0f), (8065, 1.0f) },
            CraterCount = 7, ClosedCraterHerdType = 203820,
        } },
        { "ri55swSub03_billboard", new Template
        {
            Name = "ri55swSub03_billboard", Level = 55, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2003, 20, 4),
                H("land", 2032, 20, 4),
                H("land", 2034, 20, 4),
                H("land", 2038, 20, 4),
                H("land", 2045, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (8059, 1.0f), (8060, 1.0f), (8064, 1.0f), (8065, 1.0f) },
            CraterCount = 5, ClosedCraterHerdType = 203820,
        } },
        { "ri55tb170615", new Template
        {
            Name = "ri55tb170615", Level = 55, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2009, 20, 10),
                H("land", 2028, 20, 10),
                H("land", 2029, 20, 10),
                H("land", 2040, 20, 10),
                H("land", 2044, 20, 10),
                H("land", 2087, 20, 10),
            },
            CraterSpecies = new (ushort, float)[] { (7055, 2.0f), (7056, 2.0f), (7057, 2.0f), (7058, 1.0f), (7059, 2.0f), (7060, 1.0f), (7061, 1.0f), (7062, 2.0f), (7063, 2.0f) },
            CraterCount = 15, ClosedCraterHerdType = 203620,
        } },
        { "ri55tb171228", new Template
        {
            Name = "ri55tb171228", Level = 55, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2009, 20, 8),
                H("land", 2028, 20, 8),
                H("land", 2029, 20, 8),
                H("land", 2040, 20, 8),
                H("land", 2044, 20, 8),
                H("land", 2087, 20, 8),
                H("beach", 2182, 5, 6),
            },
            CraterSpecies = new (ushort, float)[] { (7055, 2.0f), (7056, 2.0f), (7057, 2.0f), (7058, 1.0f), (7059, 2.0f), (7060, 1.0f), (7061, 1.0f), (7062, 2.0f), (7063, 2.0f) },
            CraterCount = 15, ClosedCraterHerdType = 203620,
        } },
        { "ri55trSub01", new Template
        {
            Name = "ri55trSub01", Level = 55, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2009, 20, 4),
                H("land", 2028, 20, 4),
                H("land", 2029, 20, 4),
                H("land", 2040, 20, 4),
                H("land", 2044, 20, 4),
                H("land", 2087, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (7055, 2.0f), (7056, 2.0f), (7057, 2.0f), (7058, 1.0f), (7059, 2.0f), (7060, 1.0f), (7061, 1.0f), (7062, 2.0f), (7063, 2.0f) },
            CraterCount = 6, ClosedCraterHerdType = 203620,
        } },
        { "ri55trSub02", new Template
        {
            Name = "ri55trSub02", Level = 55, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2009, 20, 4),
                H("land", 2028, 20, 4),
                H("land", 2029, 20, 4),
                H("land", 2040, 20, 4),
                H("land", 2044, 20, 4),
                H("land", 2087, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (7055, 2.0f), (7056, 2.0f), (7057, 2.0f), (7058, 1.0f), (7059, 2.0f), (7060, 1.0f), (7061, 1.0f), (7062, 2.0f), (7063, 2.0f) },
            CraterCount = 6, ClosedCraterHerdType = 203620,
        } },
        { "ri55trSub03_camp", new Template
        {
            Name = "ri55trSub03_camp", Level = 55, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2009, 20, 4),
                H("land", 2028, 20, 4),
                H("land", 2029, 20, 4),
                H("land", 2040, 20, 4),
                H("land", 2044, 20, 4),
                H("land", 2087, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (7055, 2.0f), (7056, 2.0f), (7057, 2.0f), (7058, 1.0f), (7059, 2.0f), (7060, 1.0f), (7061, 1.0f), (7062, 2.0f), (7063, 2.0f) },
            CraterCount = 4, ClosedCraterHerdType = 203620,
        } },
        { "ri55tu170615", new Template
        {
            Name = "ri55tu170615", Level = 55, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2013, 21, 10),
                H("land", 2014, 20, 10),
                H("land", 2016, 20, 10),
                H("land", 2020, 20, 10),
                H("land", 2033, 20, 10),
                H("land", 2041, 20, 10),
            },
            CraterSpecies = new (ushort, float)[] { (4055, 2.0f), (4056, 2.0f), (4057, 1.0f), (4058, 2.0f), (4059, 2.0f), (4060, 2.0f), (4061, 2.0f), (4062, 2.0f) },
            CraterCount = 15, ClosedCraterHerdType = 203320,
        } },
        { "ri55tu171228", new Template
        {
            Name = "ri55tu171228", Level = 55, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2013, 21, 8),
                H("land", 2014, 20, 7),
                H("land", 2016, 20, 7),
                H("land", 2020, 20, 8),
                H("land", 2033, 20, 8),
                H("land", 2041, 20, 8),
            },
            CraterSpecies = new (ushort, float)[] { (4055, 2.0f), (4056, 2.0f), (4057, 1.0f), (4058, 2.0f), (4059, 2.0f), (4060, 2.0f), (4061, 2.0f), (4062, 2.0f) },
            CraterCount = 15, ClosedCraterHerdType = 203320,
        } },
        { "ri55tuSub01", new Template
        {
            Name = "ri55tuSub01", Level = 55, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2013, 21, 4),
                H("land", 2014, 20, 3),
                H("land", 2016, 20, 4),
                H("land", 2020, 20, 4),
                H("land", 2033, 20, 4),
                H("land", 2041, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (4055, 2.0f), (4056, 2.0f), (4057, 1.0f), (4058, 2.0f), (4059, 2.0f), (4060, 2.0f), (4061, 2.0f), (4062, 2.0f) },
            CraterCount = 6, ClosedCraterHerdType = 203320,
        } },
        { "ri55tuSub02", new Template
        {
            Name = "ri55tuSub02", Level = 55, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2013, 21, 4),
                H("land", 2014, 20, 3),
                H("land", 2016, 20, 4),
                H("land", 2020, 20, 4),
                H("land", 2033, 20, 4),
                H("land", 2041, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (4055, 2.0f), (4056, 2.0f), (4057, 1.0f), (4058, 2.0f), (4059, 2.0f), (4060, 2.0f), (4061, 2.0f), (4062, 2.0f) },
            CraterCount = 6, ClosedCraterHerdType = 203320,
        } },
        { "ri55tuSub03_camp", new Template
        {
            Name = "ri55tuSub03_camp", Level = 55, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2013, 21, 4),
                H("land", 2014, 20, 3),
                H("land", 2016, 20, 4),
                H("land", 2020, 20, 4),
                H("land", 2033, 20, 4),
                H("land", 2041, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (4055, 2.0f), (4056, 2.0f), (4057, 1.0f), (4058, 2.0f), (4059, 2.0f), (4060, 2.0f), (4061, 2.0f), (4062, 2.0f) },
            CraterCount = 4, ClosedCraterHerdType = 203320,
        } },
        { "ri60vo180404", new Template
        {
            Name = "ri60vo180404", Level = 60, DesiredPopulation = 9000,
            CraterSpecies = new (ushort, float)[] { (9001, 1.0f), (9002, 1.0f), (9003, 1.0f), (9004, 1.0f), (9005, 1.0f), (9006, 1.0f), (9007, 1.0f) },
            CraterCount = 20, ClosedCraterHerdType = 0,
        } },
        { "ru10gr170511", new Template
        {
            Name = "ru10gr170511", Level = 10, DesiredPopulation = 700,
            Herds = new[] {
                H("land", 2002, 4, 10),
                H("land", 2077, 4, 10),
                H("land", 2078, 4, 10),
                H("beach", 2010, 4, 5),
                H("beach", 2011, 4, 5),
                H("lake_shallow", 2017, 3, 5),
                H("lake_shallow", 2018, 6, 5),
                H("lake_deep", 2013, 6, 5),
                H("lake_deep", 2013, 7, 5),
                H("ocean", 2009, 2, 5),
                H("ocean", 2009, 3, 5),
            },
            CraterCount = 0, ClosedCraterHerdType = 0,
        } },
        { "ru10gr170615", new Template
        {
            Name = "ru10gr170615", Level = 10, DesiredPopulation = 700,
            Herds = new[] {
                H("land", 2006, 20, 40),
                H("land", 2015, 20, 60),
            },
            CraterCount = 0, ClosedCraterHerdType = 201520,
        } },
        { "ru10gr170712", new Template
        {
            Name = "ru10gr170712", Level = 10, DesiredPopulation = 5000,
            Herds = new[] {
                H("land", 2006, 20, 40),
                H("land", 2015, 20, 60),
            },
            CraterCount = 0, ClosedCraterHerdType = 201520,
        } },
        { "s01_ri35tu180405", new Template
        {
            Name = "s01_ri35tu180405", Level = 35, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2013, 20, 5),
                H("land", 2016, 20, 5),
                H("land", 2020, 20, 5),
                H("land", 2033, 20, 5),
                H("land", 2041, 20, 5),
            },
            CraterSpecies = new (ushort, float)[] { (4016, 1.0f), (4017, 1.0f), (4018, 1.0f), (4019, 1.0f), (4020, 1.0f), (4021, 1.0f), (4035, 1.0f), (4036, 1.0f) },
            CraterCount = 16, ClosedCraterHerdType = 203320,
        } },
        { "s01_ri45tr180405", new Template
        {
            Name = "s01_ri45tr180405", Level = 45, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2001, 20, 2),
                H("land", 2028, 20, 2),
                H("land", 2029, 20, 3),
                H("land", 2036, 20, 2),
                H("land", 2040, 20, 2),
                H("land", 2078, 20, 3),
                H("land", 2088, 20, 3),
            },
            CraterSpecies = new (ushort, float)[] { (3089, 1.0f), (3090, 2.0f), (3091, 2.0f), (3092, 1.0f), (3093, 1.0f), (3094, 2.0f), (3095, 2.0f), (3096, 2.0f), (3097, 1.0f) },
            CraterCount = 14, ClosedCraterHerdType = 203620,
        } },
        { "s01_ri55sn180405", new Template
        {
            Name = "s01_ri55sn180405", Level = 55, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2008, 20, 5),
                H("land", 2070, 20, 8),
                H("land", 2077, 20, 5),
                H("land", 2079, 21, 5),
                H("land", 2090, 20, 5),
                H("land", 2135, 10, 8),
                H("land", 2138, 20, 8),
            },
            CraterSpecies = new (ushort, float)[] { (6012, 3.0f), (6013, 2.0f), (6020, 3.0f), (6021, 2.0f), (6022, 2.0f), (6023, 2.0f), (6024, 2.0f) },
            CraterCount = 16, ClosedCraterHerdType = 203520,
        } },
        { "s01_ri60de180405", new Template
        {
            Name = "s01_ri60de180405", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2002, 20, 2),
                H("land", 2043, 20, 1),
            },
            CraterSpecies = new (ushort, float)[] { (5060, 1.0f) },
            CraterCount = 1, ClosedCraterHerdType = 0,
        } },
        { "s02_ri60sw", new Template
        {
            Name = "s02_ri60sw", Level = 60, DesiredPopulation = 1,
            Herds = new[] {
                H("land", 2140, 2, 8),
                H("land", 2141, 2, 8),
                H("land", 2142, 2, 6),
                H("land", 2144, 1, 3),
                H("land", 2145, 1, 3),
            },
            CraterSpecies = new (ushort, float)[] { (8083, 6.0f), (8084, 3.0f), (8085, 1.0f) },
            CraterCount = 10, ClosedCraterHerdType = 214202,
        } },
        { "s03_ri60sw", new Template
        {
            Name = "s03_ri60sw", Level = 60, DesiredPopulation = 1,
            Herds = new[] {
                H("land", 2140, 3, 5),
                H("land", 2141, 3, 5),
                H("land", 2142, 3, 4),
                H("land", 2144, 1, 2),
                H("land", 2145, 1, 2),
            },
            CraterSpecies = new (ushort, float)[] { (8083, 4.0f), (8084, 2.0f) },
            CraterCount = 6, ClosedCraterHerdType = 214203,
        } },
        { "sh05tr170914", new Template
        {
            Name = "sh05tr170914", Level = 5, DesiredPopulation = 40,
            Herds = new[] {
                H("land", 2015, 20, 10),
            },
            CraterCount = 0, ClosedCraterHerdType = 201520,
        } },
        { "sh05tr170918", new Template
        {
            Name = "sh05tr170918", Level = 5, DesiredPopulation = 40,
            Herds = new[] {
                H("land", 2015, 20, 10),
            },
            CraterCount = 0, ClosedCraterHerdType = 201520,
        } },
        { "sh05tr170919", new Template
        {
            Name = "sh05tr170919", Level = 5, DesiredPopulation = 40,
            Herds = new[] {
                H("land", 2015, 20, 10),
            },
            CraterCount = 0, ClosedCraterHerdType = 201520,
        } },
        { "sh05tr170926", new Template
        {
            Name = "sh05tr170926", Level = 5, DesiredPopulation = 40,
            Herds = new[] {
                H("land", 2015, 20, 10),
            },
            CraterCount = 0, ClosedCraterHerdType = 201520,
        } },
        { "sh05tr170927", new Template
        {
            Name = "sh05tr170927", Level = 5, DesiredPopulation = 40,
            Herds = new[] {
                H("land", 2015, 20, 10),
                H("land", 2051, 10, 3),
            },
            CraterCount = 0, ClosedCraterHerdType = 201520,
        } },
        { "sh05tr171108", new Template
        {
            Name = "sh05tr171108", Level = 5, DesiredPopulation = 40,
            Herds = new[] {
                H("land", 2015, 20, 10),
            },
            CraterCount = 0, ClosedCraterHerdType = 201520,
        } },
        { "sh05tr171109", new Template
        {
            Name = "sh05tr171109", Level = 5, DesiredPopulation = 40,
            Herds = new[] {
                H("land", 2015, 20, 1),
            },
            CraterCount = 0, ClosedCraterHerdType = 201520,
        } },
        { "sh05tr171110", new Template
        {
            Name = "sh05tr171110", Level = 5, DesiredPopulation = 25,
            Herds = new[] {
                H("land", 2015, 20, 1),
            },
            CraterCount = 0, ClosedCraterHerdType = 201520,
        } },
        { "sh05tr180130", new Template
        {
            Name = "sh05tr180130", Level = 5, DesiredPopulation = 25,
            Herds = new[] {
                H("land", 2015, 20, 1),
            },
            CraterCount = 0, ClosedCraterHerdType = 201520,
        } },
        { "test60vol190508_sub2", new Template
        {
            Name = "test60vol190508_sub2", Level = 60, DesiredPopulation = 9000,
            CraterSpecies = new (ushort, float)[] { (9002, 1.0f), (9003, 1.0f), (9007, 1.0f), (9008, 1.0f) },
            CraterCount = 10, ClosedCraterHerdType = 201520,
        } },
        { "test60vol190508_sub3", new Template
        {
            Name = "test60vol190508_sub3", Level = 60, DesiredPopulation = 9000,
            CraterSpecies = new (ushort, float)[] { (9002, 1.0f), (9003, 1.0f), (9007, 1.0f), (9008, 1.0f) },
            CraterCount = 10, ClosedCraterHerdType = 201520,
        } },
        { "test60vol190516", new Template
        {
            Name = "test60vol190516", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2003, 20, 10),
                H("land", 2005, 30, 1),
                H("land", 2023, 20, 10),
                H("land", 2043, 20, 5),
                H("land", 2044, 20, 8),
                H("land", 2046, 20, 8),
                H("beach", 2043, 20, 15),
            },
            CraterSpecies = new (ushort, float)[] { (9002, 2.0f), (9003, 3.0f), (9007, 1.0f), (9008, 1.0f), (9012, 2.0f), (9015, 1.0f) },
            CraterCount = 11, ClosedCraterHerdType = 201520,
        } },
        { "test60vol190516_sub1", new Template
        {
            Name = "test60vol190516_sub1", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2003, 20, 5),
                H("land", 2023, 20, 5),
                H("land", 2043, 20, 2),
                H("land", 2044, 20, 3),
                H("land", 2046, 20, 3),
                H("beach", 2043, 20, 10),
            },
            CraterSpecies = new (ushort, float)[] { (9003, 1.0f), (9006, 1.0f), (9007, 1.0f), (9008, 1.0f), (9011, 1.0f), (9012, 1.0f) },
            CraterCount = 8, ClosedCraterHerdType = 201520,
        } },
        { "test60vol190625", new Template
        {
            Name = "test60vol190625", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2003, 20, 10),
                H("land", 2005, 30, 1),
                H("land", 2023, 20, 10),
                H("land", 2043, 20, 5),
                H("land", 2044, 20, 8),
                H("land", 2046, 20, 8),
                H("beach", 2043, 20, 15),
            },
            CraterSpecies = new (ushort, float)[] { (9002, 3.0f), (9003, 3.0f), (9012, 3.0f), (9013, 2.0f), (9015, 1.0f) },
            CraterCount = 12, ClosedCraterHerdType = 201520,
        } },
        { "test60vol_128", new Template
        {
            Name = "test60vol_128", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2170, 10, 2),
                H("land", 2174, 20, 2),
                H("land", 2175, 20, 2),
                H("land", 2177, 20, 2),
                H("beach", 2171, 10, 5),
            },
            CraterSpecies = new (ushort, float)[] { (9002, 1.0f), (9012, 1.0f), (9013, 1.0f) },
            CraterCount = 5, ClosedCraterHerdType = 217620,
        } },
        { "test60vol_256", new Template
        {
            Name = "test60vol_256", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2170, 10, 8),
                H("land", 2174, 20, 5),
                H("land", 2175, 20, 10),
                H("land", 2177, 20, 8),
                H("land", 2178, 10, 10),
                H("beach", 2171, 10, 15),
            },
            CraterSpecies = new (ushort, float)[] { (9002, 2.0f), (9003, 2.0f), (9012, 2.0f), (9013, 1.0f) },
            CraterCount = 8, ClosedCraterHerdType = 217620,
        } },
        { "test60vol_512", new Template
        {
            Name = "test60vol_512", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2005, 30, 1),
                H("land", 2170, 10, 8),
                H("land", 2174, 20, 5),
                H("land", 2175, 20, 10),
                H("land", 2176, 20, 10),
                H("land", 2177, 20, 8),
                H("land", 2178, 10, 10),
                H("beach", 2171, 10, 15),
            },
            CraterSpecies = new (ushort, float)[] { (9002, 3.0f), (9003, 3.0f), (9012, 3.0f), (9013, 2.0f), (9015, 1.0f) },
            CraterCount = 12, ClosedCraterHerdType = 217620,
        } },
        { "test60vol_cracks", new Template
        {
            Name = "test60vol_cracks", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2177, 20, 2),
                H("beach", 2171, 10, 5),
            },
            CraterSpecies = new (ushort, float)[] { (9006, 1.0f), (9007, 1.0f), (9008, 1.0f), (9011, 1.0f), (9018, 1.0f), (9021, 1.0f) },
            CraterCount = 6, ClosedCraterHerdType = 217620,
        } },
        { "test_phase", new Template
        {
            Name = "test_phase", Level = 60, DesiredPopulation = 1,
            CraterSpecies = new (ushort, float)[] { (8045, 1.0f), (8046, 1.0f), (8047, 1.0f) },
            CraterCount = 3, ClosedCraterHerdType = 0,
        } },
        { "tr60te170511", new Template
        {
            Name = "tr60te170511", Level = 60, DesiredPopulation = 9999999,
            Herds = new[] {
                H("land", 2002, 4, 10),
                H("land", 2077, 4, 10),
                H("land", 2078, 4, 90),
                H("beach", 2010, 4, 5),
                H("beach", 2011, 4, 5),
                H("lake_shallow", 2017, 3, 5),
                H("lake_shallow", 2018, 6, 5),
                H("lake_deep", 2013, 6, 5),
                H("lake_deep", 2013, 7, 5),
                H("ocean", 2009, 2, 5),
                H("ocean", 2009, 3, 5),
            },
            CraterCount = 0, ClosedCraterHerdType = 0,
        } },
        { "tr60te170613", new Template
        {
            Name = "tr60te170613", Level = 60, DesiredPopulation = 9999999,
            Herds = new[] {
                H("land", 2002, 4, 10),
                H("land", 2077, 4, 10),
                H("land", 2078, 4, 90),
                H("beach", 2010, 4, 5),
                H("beach", 2011, 4, 5),
                H("lake_shallow", 2017, 3, 5),
                H("lake_shallow", 2018, 6, 5),
                H("lake_deep", 2013, 6, 5),
                H("lake_deep", 2013, 7, 5),
                H("ocean", 2009, 2, 5),
                H("ocean", 2009, 3, 5),
            },
            CraterCount = 0, ClosedCraterHerdType = 201520,
        } },
        { "ua60de02Main01", new Template
        {
            Name = "ua60de02Main01", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2002, 20, 8),
                H("land", 2010, 20, 8),
                H("land", 2023, 20, 8),
                H("land", 2026, 20, 8),
                H("land", 2043, 20, 8),
                H("land", 2083, 20, 8),
                H("land", 2162, 20, 8),
            },
            CraterSpecies = new (ushort, float)[] { (5035, 2.0f), (5051, 2.0f), (5052, 2.0f), (5053, 1.0f), (5054, 2.0f), (5057, 2.0f), (5061, 3.0f), (5062, 2.0f), (5063, 2.0f) },
            CraterCount = 18, ClosedCraterHerdType = 204320,
        } },
        { "ua60de02Main02_elite", new Template
        {
            Name = "ua60de02Main02_elite", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2002, 20, 10),
                H("land", 2010, 20, 10),
                H("land", 2023, 20, 10),
                H("land", 2026, 20, 10),
                H("land", 2043, 20, 10),
                H("land", 2083, 20, 10),
                H("land", 2162, 20, 10),
            },
            CraterSpecies = new (ushort, float)[] { (5035, 2.0f), (5051, 2.0f), (5052, 2.0f), (5053, 1.0f), (5054, 2.0f), (5057, 2.0f), (5061, 3.0f), (5062, 2.0f), (5063, 2.0f) },
            CraterCount = 18, ClosedCraterHerdType = 204320,
        } },
        { "ua60de02Main03", new Template
        {
            Name = "ua60de02Main03", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2002, 20, 8),
                H("land", 2010, 20, 8),
                H("land", 2023, 20, 8),
                H("land", 2026, 20, 8),
                H("land", 2043, 20, 8),
                H("land", 2083, 20, 8),
                H("land", 2162, 20, 8),
            },
            CraterSpecies = new (ushort, float)[] { (5051, 2.0f), (5052, 2.0f), (5053, 1.0f), (5054, 2.0f), (5057, 2.0f), (5061, 3.0f) },
            CraterCount = 12, ClosedCraterHerdType = 204320,
        } },
        { "ua60de02Main04_elite", new Template
        {
            Name = "ua60de02Main04_elite", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2002, 20, 10),
                H("land", 2010, 20, 10),
                H("land", 2023, 20, 10),
                H("land", 2026, 20, 10),
                H("land", 2043, 20, 10),
                H("land", 2083, 20, 10),
                H("land", 2162, 20, 10),
            },
            CraterSpecies = new (ushort, float)[] { (5051, 2.0f), (5052, 2.0f), (5053, 1.0f), (5054, 2.0f), (5057, 2.0f), (5061, 3.0f) },
            CraterCount = 12, ClosedCraterHerdType = 204320,
        } },
        { "ua60de02Sub01", new Template
        {
            Name = "ua60de02Sub01", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2002, 20, 4),
                H("land", 2010, 20, 4),
                H("land", 2023, 20, 4),
                H("land", 2026, 20, 4),
                H("land", 2043, 20, 4),
                H("land", 2083, 20, 4),
                H("land", 2162, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (5051, 1.0f), (5053, 1.0f), (5054, 1.0f), (5056, 1.0f), (5061, 1.0f) },
            CraterCount = 5, ClosedCraterHerdType = 204320,
        } },
        { "ua60de02Sub02", new Template
        {
            Name = "ua60de02Sub02", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2002, 20, 4),
                H("land", 2010, 20, 4),
                H("land", 2023, 20, 4),
                H("land", 2026, 20, 4),
                H("land", 2043, 20, 4),
                H("land", 2083, 20, 4),
                H("land", 2162, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (5051, 1.0f), (5053, 1.0f), (5054, 1.0f), (5061, 1.0f) },
            CraterCount = 4, ClosedCraterHerdType = 204320,
        } },
        { "ua60de02Sub03_car", new Template
        {
            Name = "ua60de02Sub03_car", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2002, 20, 4),
                H("land", 2010, 20, 4),
                H("land", 2023, 20, 4),
                H("land", 2026, 20, 4),
                H("land", 2043, 20, 4),
                H("land", 2083, 20, 4),
                H("land", 2162, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (5051, 1.0f), (5061, 1.0f) },
            CraterCount = 2, ClosedCraterHerdType = 204320,
        } },
        { "ua60deMain01", new Template
        {
            Name = "ua60deMain01", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2002, 20, 8),
                H("land", 2010, 20, 8),
                H("land", 2023, 20, 8),
                H("land", 2025, 20, 8),
                H("land", 2026, 20, 8),
                H("land", 2043, 20, 8),
                H("land", 2083, 20, 8),
            },
            CraterSpecies = new (ushort, float)[] { (5035, 2.0f), (5051, 2.0f), (5052, 2.0f), (5053, 1.0f), (5054, 2.0f), (5055, 2.0f), (5056, 2.0f), (5057, 3.0f), (5063, 2.0f) },
            CraterCount = 18, ClosedCraterHerdType = 204320,
        } },
        { "ua60deMain02_elite", new Template
        {
            Name = "ua60deMain02_elite", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2002, 20, 10),
                H("land", 2010, 20, 10),
                H("land", 2023, 20, 10),
                H("land", 2025, 20, 10),
                H("land", 2026, 20, 10),
                H("land", 2043, 20, 10),
                H("land", 2083, 20, 10),
            },
            CraterSpecies = new (ushort, float)[] { (5035, 2.0f), (5051, 2.0f), (5052, 2.0f), (5053, 1.0f), (5054, 2.0f), (5055, 2.0f), (5056, 2.0f), (5057, 3.0f), (5063, 2.0f) },
            CraterCount = 18, ClosedCraterHerdType = 204320,
        } },
        { "ua60deMain03", new Template
        {
            Name = "ua60deMain03", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2002, 20, 8),
                H("land", 2010, 20, 8),
                H("land", 2023, 20, 8),
                H("land", 2025, 20, 8),
                H("land", 2026, 20, 8),
                H("land", 2043, 20, 8),
                H("land", 2083, 20, 8),
            },
            CraterSpecies = new (ushort, float)[] { (5051, 2.0f), (5052, 2.0f), (5053, 1.0f), (5054, 2.0f), (5056, 2.0f), (5057, 3.0f) },
            CraterCount = 12, ClosedCraterHerdType = 204320,
        } },
        { "ua60deMain04_elite", new Template
        {
            Name = "ua60deMain04_elite", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2002, 20, 10),
                H("land", 2010, 20, 10),
                H("land", 2023, 20, 10),
                H("land", 2025, 20, 10),
                H("land", 2026, 20, 10),
                H("land", 2043, 20, 10),
                H("land", 2083, 20, 10),
            },
            CraterSpecies = new (ushort, float)[] { (5051, 2.0f), (5052, 2.0f), (5053, 1.0f), (5054, 2.0f), (5056, 2.0f), (5057, 3.0f) },
            CraterCount = 12, ClosedCraterHerdType = 204320,
        } },
        { "ua60deSub01", new Template
        {
            Name = "ua60deSub01", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2002, 20, 4),
                H("land", 2010, 20, 4),
                H("land", 2023, 20, 4),
                H("land", 2025, 20, 4),
                H("land", 2026, 20, 4),
                H("land", 2043, 20, 4),
                H("land", 2083, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (5051, 2.0f), (5052, 2.0f), (5053, 1.0f), (5054, 2.0f), (5056, 3.0f), (5057, 3.0f) },
            CraterCount = 8, ClosedCraterHerdType = 204320,
        } },
        { "ua60deSub02", new Template
        {
            Name = "ua60deSub02", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2002, 20, 4),
                H("land", 2010, 20, 4),
                H("land", 2023, 20, 4),
                H("land", 2025, 20, 4),
                H("land", 2026, 20, 4),
                H("land", 2043, 20, 4),
                H("land", 2083, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (5051, 2.0f), (5052, 2.0f), (5053, 1.0f), (5054, 2.0f), (5056, 3.0f), (5057, 3.0f) },
            CraterCount = 8, ClosedCraterHerdType = 204320,
        } },
        { "ua60deSub03_car", new Template
        {
            Name = "ua60deSub03_car", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2002, 20, 4),
                H("land", 2010, 20, 4),
                H("land", 2023, 20, 4),
                H("land", 2025, 20, 4),
                H("land", 2026, 20, 4),
                H("land", 2043, 20, 4),
                H("land", 2083, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (5051, 2.0f), (5052, 2.0f), (5053, 1.0f), (5054, 2.0f), (5056, 3.0f), (5057, 3.0f) },
            CraterCount = 6, ClosedCraterHerdType = 204320,
        } },
        { "ua60sn02Main01", new Template
        {
            Name = "ua60sn02Main01", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2008, 20, 8),
                H("land", 2020, 10, 6),
                H("land", 2035, 20, 10),
                H("land", 2077, 20, 6),
                H("land", 2079, 21, 6),
                H("land", 2090, 20, 6),
                H("land", 2157, 20, 8),
            },
            CraterSpecies = new (ushort, float)[] { (6031, 2.0f), (6032, 3.0f), (6033, 2.0f), (6034, 2.0f), (6036, 2.0f), (6039, 2.0f), (6040, 3.0f) },
            CraterCount = 16, ClosedCraterHerdType = 202010,
        } },
        { "ua60sn02Main02_elite", new Template
        {
            Name = "ua60sn02Main02_elite", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2008, 20, 10),
                H("land", 2035, 20, 10),
                H("land", 2077, 20, 10),
                H("land", 2079, 21, 10),
                H("land", 2089, 30, 1),
                H("land", 2090, 20, 10),
                H("land", 2157, 20, 10),
            },
            CraterSpecies = new (ushort, float)[] { (6031, 2.0f), (6032, 3.0f), (6033, 2.0f), (6034, 2.0f), (6036, 2.0f), (6039, 2.0f), (6040, 3.0f) },
            CraterCount = 16, ClosedCraterHerdType = 202010,
        } },
        { "ua60sn02Main03", new Template
        {
            Name = "ua60sn02Main03", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2008, 20, 8),
                H("land", 2020, 10, 6),
                H("land", 2035, 20, 10),
                H("land", 2077, 20, 6),
                H("land", 2079, 21, 6),
                H("land", 2090, 20, 6),
                H("land", 2157, 20, 8),
            },
            CraterSpecies = new (ushort, float)[] { (6031, 2.0f), (6032, 3.0f), (6033, 2.0f), (6034, 2.0f), (6036, 2.0f) },
            CraterCount = 10, ClosedCraterHerdType = 202010,
        } },
        { "ua60sn02Main04_elite", new Template
        {
            Name = "ua60sn02Main04_elite", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2008, 20, 10),
                H("land", 2035, 20, 10),
                H("land", 2077, 20, 10),
                H("land", 2079, 21, 10),
                H("land", 2089, 30, 1),
                H("land", 2090, 20, 10),
                H("land", 2157, 20, 10),
            },
            CraterSpecies = new (ushort, float)[] { (6031, 2.0f), (6032, 3.0f), (6033, 2.0f), (6034, 2.0f), (6036, 2.0f) },
            CraterCount = 10, ClosedCraterHerdType = 202010,
        } },
        { "ua60sn02Sub01", new Template
        {
            Name = "ua60sn02Sub01", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2008, 20, 4),
                H("land", 2035, 20, 4),
                H("land", 2077, 20, 4),
                H("land", 2079, 21, 4),
                H("land", 2090, 20, 4),
                H("land", 2157, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (6031, 1.0f), (6032, 1.0f), (6033, 1.0f), (6034, 1.0f), (6036, 1.0f) },
            CraterCount = 7, ClosedCraterHerdType = 202010,
        } },
        { "ua60sn02Sub02", new Template
        {
            Name = "ua60sn02Sub02", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2008, 20, 4),
                H("land", 2035, 20, 4),
                H("land", 2077, 20, 4),
                H("land", 2079, 21, 4),
                H("land", 2090, 20, 4),
                H("land", 2157, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (6033, 1.0f), (6034, 1.0f), (6038, 1.0f) },
            CraterCount = 3, ClosedCraterHerdType = 202010,
        } },
        { "ua60sn02Sub03_mammoth", new Template
        {
            Name = "ua60sn02Sub03_mammoth", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2008, 20, 30),
                H("land", 2008, 30, 1),
            },
            CraterSpecies = new (ushort, float)[] { (6033, 1.0f), (6034, 1.0f), (6038, 1.0f) },
            CraterCount = 3, ClosedCraterHerdType = 202010,
        } },
        { "ua60snMain01", new Template
        {
            Name = "ua60snMain01", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2007, 20, 8),
                H("land", 2008, 20, 8),
                H("land", 2020, 10, 6),
                H("land", 2035, 20, 10),
                H("land", 2077, 20, 6),
                H("land", 2079, 21, 6),
                H("land", 2090, 20, 6),
            },
            CraterSpecies = new (ushort, float)[] { (6030, 2.0f), (6031, 2.0f), (6032, 2.0f), (6033, 3.0f), (6034, 2.0f), (6035, 3.0f), (6036, 2.0f) },
            CraterCount = 16, ClosedCraterHerdType = 202010,
        } },
        { "ua60snMain02_elite", new Template
        {
            Name = "ua60snMain02_elite", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2007, 20, 10),
                H("land", 2008, 20, 10),
                H("land", 2035, 20, 10),
                H("land", 2077, 20, 10),
                H("land", 2079, 21, 10),
                H("land", 2089, 30, 1),
                H("land", 2090, 20, 10),
            },
            CraterSpecies = new (ushort, float)[] { (6030, 2.0f), (6031, 2.0f), (6032, 2.0f), (6033, 3.0f), (6034, 2.0f), (6035, 3.0f), (6036, 2.0f) },
            CraterCount = 16, ClosedCraterHerdType = 202010,
        } },
        { "ua60snMain03", new Template
        {
            Name = "ua60snMain03", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2007, 20, 8),
                H("land", 2008, 20, 8),
                H("land", 2020, 10, 6),
                H("land", 2035, 20, 10),
                H("land", 2077, 20, 6),
                H("land", 2079, 21, 6),
                H("land", 2090, 20, 6),
            },
            CraterSpecies = new (ushort, float)[] { (6031, 2.0f), (6032, 1.0f), (6033, 3.0f), (6034, 2.0f), (6036, 2.0f) },
            CraterCount = 10, ClosedCraterHerdType = 202010,
        } },
        { "ua60snMain04_elite", new Template
        {
            Name = "ua60snMain04_elite", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2007, 20, 10),
                H("land", 2008, 20, 10),
                H("land", 2035, 20, 10),
                H("land", 2077, 20, 10),
                H("land", 2079, 21, 10),
                H("land", 2089, 30, 1),
                H("land", 2090, 20, 10),
            },
            CraterSpecies = new (ushort, float)[] { (6031, 2.0f), (6032, 1.0f), (6033, 3.0f), (6034, 2.0f), (6036, 2.0f) },
            CraterCount = 10, ClosedCraterHerdType = 202010,
        } },
        { "ua60snSub01", new Template
        {
            Name = "ua60snSub01", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2007, 20, 4),
                H("land", 2008, 20, 4),
                H("land", 2035, 20, 4),
                H("land", 2077, 20, 4),
                H("land", 2079, 21, 4),
                H("land", 2090, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (6031, 1.0f), (6032, 1.0f), (6033, 1.0f), (6034, 1.0f), (6036, 1.0f) },
            CraterCount = 7, ClosedCraterHerdType = 202010,
        } },
        { "ua60snSub02", new Template
        {
            Name = "ua60snSub02", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2007, 20, 4),
                H("land", 2008, 20, 4),
                H("land", 2035, 20, 4),
                H("land", 2077, 20, 4),
                H("land", 2079, 21, 4),
                H("land", 2090, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (6031, 1.0f), (6032, 1.0f), (6033, 1.0f), (6034, 1.0f), (6036, 1.0f) },
            CraterCount = 7, ClosedCraterHerdType = 202010,
        } },
        { "ua60snSub03_mammoth", new Template
        {
            Name = "ua60snSub03_mammoth", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2008, 20, 30),
                H("land", 2008, 30, 1),
            },
            CraterSpecies = new (ushort, float)[] { (6031, 1.0f), (6032, 1.0f), (6033, 1.0f), (6034, 1.0f), (6036, 1.0f) },
            CraterCount = 5, ClosedCraterHerdType = 202010,
        } },
        { "ua60sw02Main01", new Template
        {
            Name = "ua60sw02Main01", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2003, 20, 8),
                H("land", 2034, 20, 8),
                H("land", 2038, 20, 8),
                H("land", 2045, 20, 8),
                H("land", 2167, 20, 8),
            },
            CraterSpecies = new (ushort, float)[] { (8056, 2.0f), (8057, 1.0f), (8058, 1.0f), (8059, 1.0f), (8067, 1.0f), (8070, 1.0f), (8071, 2.0f) },
            CraterCount = 15, ClosedCraterHerdType = 203820,
        } },
        { "ua60sw02Main02_elite", new Template
        {
            Name = "ua60sw02Main02_elite", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2003, 20, 10),
                H("land", 2034, 20, 10),
                H("land", 2038, 20, 10),
                H("land", 2045, 20, 10),
                H("land", 2133, 30, 1),
                H("land", 2167, 20, 10),
            },
            CraterSpecies = new (ushort, float)[] { (8056, 2.0f), (8057, 1.0f), (8058, 1.0f), (8059, 1.0f), (8067, 1.0f), (8070, 1.0f), (8071, 2.0f) },
            CraterCount = 15, ClosedCraterHerdType = 203820,
        } },
        { "ua60sw02Main03", new Template
        {
            Name = "ua60sw02Main03", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2003, 20, 8),
                H("land", 2034, 20, 8),
                H("land", 2038, 20, 8),
                H("land", 2045, 20, 8),
                H("land", 2167, 20, 8),
            },
            CraterSpecies = new (ushort, float)[] { (8056, 2.0f), (8057, 1.0f), (8058, 1.0f), (8059, 1.0f), (8067, 1.0f), (8070, 1.0f), (8071, 2.0f), (8086, 3.0f) },
            CraterCount = 15, ClosedCraterHerdType = 203820,
        } },
        { "ua60sw02Main04_elite", new Template
        {
            Name = "ua60sw02Main04_elite", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2003, 20, 10),
                H("land", 2034, 20, 10),
                H("land", 2038, 20, 10),
                H("land", 2045, 20, 10),
                H("land", 2133, 30, 1),
                H("land", 2167, 20, 10),
            },
            CraterSpecies = new (ushort, float)[] { (8056, 2.0f), (8057, 1.0f), (8058, 1.0f), (8059, 1.0f), (8067, 1.0f), (8070, 1.0f), (8071, 2.0f), (8086, 3.0f) },
            CraterCount = 15, ClosedCraterHerdType = 203820,
        } },
        { "ua60sw02Main05", new Template
        {
            Name = "ua60sw02Main05", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2003, 20, 8),
                H("land", 2034, 20, 8),
                H("land", 2038, 20, 8),
                H("land", 2045, 20, 8),
                H("land", 2167, 20, 8),
            },
            CraterSpecies = new (ushort, float)[] { (8056, 2.0f), (8057, 1.0f), (8058, 1.0f), (8059, 1.0f), (8067, 1.0f), (8070, 1.0f), (8071, 2.0f) },
            CraterCount = 10, ClosedCraterHerdType = 203820,
        } },
        { "ua60sw02Main06_elite", new Template
        {
            Name = "ua60sw02Main06_elite", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2003, 20, 10),
                H("land", 2034, 20, 10),
                H("land", 2038, 20, 10),
                H("land", 2045, 20, 10),
                H("land", 2133, 30, 1),
                H("land", 2167, 20, 10),
            },
            CraterSpecies = new (ushort, float)[] { (8056, 2.0f), (8057, 1.0f), (8058, 1.0f), (8059, 1.0f), (8067, 1.0f), (8070, 1.0f), (8071, 2.0f) },
            CraterCount = 10, ClosedCraterHerdType = 203820,
        } },
        { "ua60sw02Sub01", new Template
        {
            Name = "ua60sw02Sub01", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2003, 20, 4),
                H("land", 2034, 20, 4),
                H("land", 2038, 20, 4),
                H("land", 2045, 20, 4),
                H("land", 2167, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (8058, 1.0f), (8067, 1.0f), (8070, 1.0f) },
            CraterCount = 3, ClosedCraterHerdType = 203820,
        } },
        { "ua60sw02Sub02", new Template
        {
            Name = "ua60sw02Sub02", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2003, 20, 4),
                H("land", 2034, 20, 4),
                H("land", 2038, 20, 4),
                H("land", 2045, 20, 4),
                H("land", 2167, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (8058, 1.0f), (8070, 1.0f) },
            CraterCount = 2, ClosedCraterHerdType = 203820,
        } },
        { "ua60sw02Sub03_billboard", new Template
        {
            Name = "ua60sw02Sub03_billboard", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2003, 20, 4),
                H("land", 2034, 20, 4),
                H("land", 2038, 20, 4),
                H("land", 2045, 20, 4),
                H("land", 2167, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (8058, 1.0f), (8070, 1.0f) },
            CraterCount = 2, ClosedCraterHerdType = 203820,
        } },
        { "ua60swMain01", new Template
        {
            Name = "ua60swMain01", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2003, 20, 8),
                H("land", 2032, 20, 8),
                H("land", 2034, 20, 8),
                H("land", 2038, 20, 8),
                H("land", 2045, 20, 8),
            },
            CraterSpecies = new (ushort, float)[] { (8056, 1.0f), (8057, 1.0f), (8058, 1.0f), (8059, 1.0f), (8060, 1.0f), (8064, 1.0f), (8065, 3.0f) },
            CraterCount = 15, ClosedCraterHerdType = 203820,
        } },
        { "ua60swMain02_elite", new Template
        {
            Name = "ua60swMain02_elite", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2003, 20, 10),
                H("land", 2032, 20, 10),
                H("land", 2034, 20, 10),
                H("land", 2038, 20, 10),
                H("land", 2045, 20, 10),
                H("land", 2133, 30, 1),
            },
            CraterSpecies = new (ushort, float)[] { (8056, 1.0f), (8057, 1.0f), (8058, 1.0f), (8059, 1.0f), (8060, 1.0f), (8064, 1.0f), (8065, 3.0f) },
            CraterCount = 15, ClosedCraterHerdType = 203820,
        } },
        { "ua60swMain03", new Template
        {
            Name = "ua60swMain03", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2003, 20, 8),
                H("land", 2032, 20, 8),
                H("land", 2034, 20, 8),
                H("land", 2038, 20, 8),
                H("land", 2045, 20, 8),
            },
            CraterSpecies = new (ushort, float)[] { (8056, 1.0f), (8057, 1.0f), (8058, 1.0f), (8059, 1.0f), (8060, 1.0f), (8064, 1.0f), (8065, 3.0f), (8086, 2.0f) },
            CraterCount = 15, ClosedCraterHerdType = 203820,
        } },
        { "ua60swMain04_elite", new Template
        {
            Name = "ua60swMain04_elite", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2003, 20, 10),
                H("land", 2032, 20, 10),
                H("land", 2034, 20, 10),
                H("land", 2038, 20, 10),
                H("land", 2045, 20, 10),
                H("land", 2133, 30, 1),
            },
            CraterSpecies = new (ushort, float)[] { (8056, 1.0f), (8057, 1.0f), (8058, 1.0f), (8059, 1.0f), (8060, 1.0f), (8064, 1.0f), (8065, 3.0f), (8086, 2.0f) },
            CraterCount = 15, ClosedCraterHerdType = 203820,
        } },
        { "ua60swMain05", new Template
        {
            Name = "ua60swMain05", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2003, 20, 8),
                H("land", 2032, 20, 8),
                H("land", 2034, 20, 8),
                H("land", 2038, 20, 8),
                H("land", 2045, 20, 8),
            },
            CraterSpecies = new (ushort, float)[] { (8056, 1.0f), (8057, 1.0f), (8058, 1.0f), (8059, 1.0f), (8060, 1.0f), (8064, 1.0f), (8065, 3.0f) },
            CraterCount = 10, ClosedCraterHerdType = 203820,
        } },
        { "ua60swMain06_elite", new Template
        {
            Name = "ua60swMain06_elite", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2003, 20, 10),
                H("land", 2032, 20, 10),
                H("land", 2034, 20, 10),
                H("land", 2038, 20, 10),
                H("land", 2045, 20, 10),
                H("land", 2133, 30, 1),
            },
            CraterSpecies = new (ushort, float)[] { (8056, 1.0f), (8057, 1.0f), (8058, 1.0f), (8059, 1.0f), (8060, 1.0f), (8064, 1.0f), (8065, 3.0f) },
            CraterCount = 10, ClosedCraterHerdType = 203820,
        } },
        { "ua60swSub01", new Template
        {
            Name = "ua60swSub01", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2003, 20, 4),
                H("land", 2032, 20, 4),
                H("land", 2034, 20, 4),
                H("land", 2038, 20, 4),
                H("land", 2045, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (8056, 1.0f), (8057, 1.0f), (8058, 1.0f), (8059, 1.0f), (8060, 1.0f), (8064, 1.0f), (8065, 3.0f) },
            CraterCount = 6, ClosedCraterHerdType = 203820,
        } },
        { "ua60swSub02", new Template
        {
            Name = "ua60swSub02", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2003, 20, 4),
                H("land", 2032, 20, 4),
                H("land", 2034, 20, 4),
                H("land", 2038, 20, 4),
                H("land", 2045, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (8056, 1.0f), (8057, 1.0f), (8058, 1.0f), (8059, 1.0f), (8060, 1.0f), (8064, 1.0f), (8065, 3.0f) },
            CraterCount = 6, ClosedCraterHerdType = 203820,
        } },
        { "ua60swSub03_billboard", new Template
        {
            Name = "ua60swSub03_billboard", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2003, 20, 4),
                H("land", 2032, 20, 4),
                H("land", 2034, 20, 4),
                H("land", 2038, 20, 4),
                H("land", 2045, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (8056, 1.0f), (8057, 1.0f), (8058, 1.0f), (8059, 1.0f), (8060, 1.0f), (8064, 1.0f), (8065, 3.0f) },
            CraterCount = 4, ClosedCraterHerdType = 203820,
        } },
        { "ua60tr02Main01", new Template
        {
            Name = "ua60tr02Main01", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2009, 20, 8),
                H("land", 2028, 20, 8),
                H("land", 2040, 20, 8),
                H("land", 2044, 20, 8),
                H("land", 2087, 20, 8),
                H("land", 2161, 20, 8),
                H("beach", 2182, 5, 6),
            },
            CraterSpecies = new (ushort, float)[] { (3061, 1.0f), (3062, 2.0f), (3063, 2.0f), (3065, 2.0f), (3068, 2.0f), (3069, 2.0f), (3070, 2.0f), (3102, 2.0f), (3104, 1.0f) },
            CraterCount = 18, ClosedCraterHerdType = 203620,
        } },
        { "ua60tr02Sub01", new Template
        {
            Name = "ua60tr02Sub01", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2009, 20, 4),
                H("land", 2028, 20, 4),
                H("land", 2040, 20, 4),
                H("land", 2044, 20, 4),
                H("land", 2087, 20, 4),
                H("land", 2161, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (3063, 1.0f), (3069, 1.0f), (3070, 1.0f), (3102, 1.0f), (3103, 1.0f) },
            CraterCount = 6, ClosedCraterHerdType = 203620,
        } },
        { "ua60tr02Sub02", new Template
        {
            Name = "ua60tr02Sub02", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2009, 20, 4),
                H("land", 2028, 20, 4),
                H("land", 2040, 20, 4),
                H("land", 2044, 20, 4),
                H("land", 2087, 20, 4),
                H("land", 2161, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (3063, 1.0f), (3069, 1.0f) },
            CraterCount = 2, ClosedCraterHerdType = 203620,
        } },
        { "ua60tr02Sub03_car", new Template
        {
            Name = "ua60tr02Sub03_car", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2009, 20, 4),
                H("land", 2028, 20, 4),
                H("land", 2040, 20, 4),
                H("land", 2044, 20, 4),
                H("land", 2087, 20, 4),
                H("land", 2161, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (3063, 1.0f), (3069, 1.0f) },
            CraterCount = 2, ClosedCraterHerdType = 203620,
        } },
        { "ua60trMain01", new Template
        {
            Name = "ua60trMain01", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2009, 20, 8),
                H("land", 2028, 20, 8),
                H("land", 2029, 20, 8),
                H("land", 2040, 20, 8),
                H("land", 2044, 20, 8),
                H("land", 2087, 20, 8),
                H("beach", 2182, 5, 6),
            },
            CraterSpecies = new (ushort, float)[] { (3061, 1.0f), (3062, 2.0f), (3063, 2.0f), (3065, 2.0f), (3067, 2.0f), (3068, 2.0f), (3069, 2.0f), (3070, 2.0f), (3104, 1.0f) },
            CraterCount = 18, ClosedCraterHerdType = 203620,
        } },
        { "ua60trSub01", new Template
        {
            Name = "ua60trSub01", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2009, 20, 4),
                H("land", 2028, 20, 4),
                H("land", 2029, 20, 4),
                H("land", 2040, 20, 4),
                H("land", 2044, 20, 4),
                H("land", 2087, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (3062, 2.0f), (3063, 2.0f), (3065, 2.0f), (3067, 2.0f), (3068, 2.0f), (3069, 2.0f), (3070, 2.0f) },
            CraterCount = 8, ClosedCraterHerdType = 203620,
        } },
        { "ua60trSub02", new Template
        {
            Name = "ua60trSub02", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2009, 20, 4),
                H("land", 2028, 20, 4),
                H("land", 2029, 20, 4),
                H("land", 2040, 20, 4),
                H("land", 2044, 20, 4),
                H("land", 2087, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (3062, 2.0f), (3063, 2.0f), (3065, 2.0f), (3067, 2.0f), (3068, 2.0f), (3069, 2.0f), (3070, 2.0f) },
            CraterCount = 8, ClosedCraterHerdType = 203620,
        } },
        { "ua60trSub03_car", new Template
        {
            Name = "ua60trSub03_car", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2009, 20, 4),
                H("land", 2028, 20, 4),
                H("land", 2029, 20, 4),
                H("land", 2040, 20, 4),
                H("land", 2044, 20, 4),
                H("land", 2087, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (3062, 2.0f), (3063, 2.0f), (3065, 2.0f), (3067, 2.0f), (3068, 2.0f), (3069, 2.0f), (3070, 2.0f) },
            CraterCount = 6, ClosedCraterHerdType = 203620,
        } },
        { "ua60tu02Main01", new Template
        {
            Name = "ua60tu02Main01", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2014, 20, 7),
                H("land", 2016, 20, 7),
                H("land", 2020, 20, 8),
                H("land", 2033, 20, 8),
                H("land", 2041, 20, 8),
                H("land", 2164, 21, 8),
            },
            CraterSpecies = new (ushort, float)[] { (4056, 2.0f), (4057, 1.0f), (4058, 2.0f), (4059, 2.0f), (4061, 2.0f), (4063, 3.0f), (4105, 2.0f), (4106, 2.0f), (4107, 2.0f) },
            CraterCount = 16, ClosedCraterHerdType = 203320,
        } },
        { "ua60tu02Sub01", new Template
        {
            Name = "ua60tu02Sub01", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2014, 20, 3),
                H("land", 2016, 20, 3),
                H("land", 2020, 20, 4),
                H("land", 2033, 20, 4),
                H("land", 2041, 20, 4),
                H("land", 2164, 21, 4),
            },
            CraterSpecies = new (ushort, float)[] { (4056, 1.0f), (4057, 1.0f), (4059, 1.0f), (4063, 1.0f), (4105, 1.0f) },
            CraterCount = 5, ClosedCraterHerdType = 203320,
        } },
        { "ua60tu02Sub02", new Template
        {
            Name = "ua60tu02Sub02", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2014, 20, 3),
                H("land", 2016, 20, 3),
                H("land", 2020, 20, 4),
                H("land", 2033, 20, 4),
                H("land", 2041, 20, 4),
                H("land", 2164, 21, 4),
            },
            CraterSpecies = new (ushort, float)[] { (4056, 1.0f), (4057, 1.0f), (4059, 1.0f), (4063, 1.0f), (4105, 1.0f) },
            CraterCount = 5, ClosedCraterHerdType = 203320,
        } },
        { "ua60tu02Sub03_camp", new Template
        {
            Name = "ua60tu02Sub03_camp", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2014, 20, 3),
                H("land", 2016, 20, 3),
                H("land", 2020, 20, 4),
                H("land", 2033, 20, 4),
                H("land", 2041, 20, 4),
                H("land", 2164, 21, 4),
            },
            CraterSpecies = new (ushort, float)[] { (4056, 1.0f), (4057, 1.0f), (4063, 1.0f) },
            CraterCount = 3, ClosedCraterHerdType = 203320,
        } },
        { "ua60tuMain01", new Template
        {
            Name = "ua60tuMain01", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2013, 21, 8),
                H("land", 2014, 20, 7),
                H("land", 2016, 20, 7),
                H("land", 2020, 20, 8),
                H("land", 2033, 20, 8),
                H("land", 2041, 20, 8),
            },
            CraterSpecies = new (ushort, float)[] { (4056, 2.0f), (4057, 1.0f), (4058, 2.0f), (4059, 2.0f), (4060, 2.0f), (4061, 2.0f), (4062, 2.0f), (4106, 2.0f), (4107, 2.0f) },
            CraterCount = 16, ClosedCraterHerdType = 203320,
        } },
        { "ua60tuSub01", new Template
        {
            Name = "ua60tuSub01", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2013, 21, 4),
                H("land", 2014, 20, 4),
                H("land", 2016, 20, 4),
                H("land", 2020, 20, 4),
                H("land", 2033, 20, 4),
                H("land", 2041, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (4056, 2.0f), (4057, 1.0f), (4058, 2.0f), (4059, 2.0f), (4060, 2.0f), (4061, 2.0f), (4062, 2.0f) },
            CraterCount = 7, ClosedCraterHerdType = 203320,
        } },
        { "ua60tuSub02", new Template
        {
            Name = "ua60tuSub02", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2013, 21, 4),
                H("land", 2014, 20, 4),
                H("land", 2016, 20, 4),
                H("land", 2020, 20, 4),
                H("land", 2033, 20, 4),
                H("land", 2041, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (4056, 2.0f), (4057, 1.0f), (4058, 2.0f), (4059, 2.0f), (4060, 2.0f), (4061, 2.0f), (4062, 2.0f) },
            CraterCount = 7, ClosedCraterHerdType = 203320,
        } },
        { "ua60tuSub03_camp", new Template
        {
            Name = "ua60tuSub03_camp", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2013, 21, 4),
                H("land", 2014, 20, 4),
                H("land", 2016, 20, 4),
                H("land", 2020, 20, 4),
                H("land", 2033, 20, 4),
                H("land", 2041, 20, 4),
            },
            CraterSpecies = new (ushort, float)[] { (4056, 2.0f), (4057, 1.0f), (4058, 2.0f), (4059, 2.0f), (4060, 2.0f), (4061, 2.0f), (4062, 2.0f) },
            CraterCount = 5, ClosedCraterHerdType = 203320,
        } },
        { "ua60vol_01_01", new Template
        {
            Name = "ua60vol_01_01", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2170, 10, 12),
                H("land", 2174, 20, 10),
                H("land", 2175, 20, 15),
                H("land", 2176, 20, 15),
                H("land", 2177, 20, 12),
                H("beach", 2171, 10, 50),
            },
            CraterSpecies = new (ushort, float)[] { (9002, 2.0f), (9003, 2.0f), (9012, 2.0f), (9013, 2.0f), (9015, 1.0f) },
            CraterCount = 9, ClosedCraterHerdType = 217620,
        } },
        { "ua60vol_01_02", new Template
        {
            Name = "ua60vol_01_02", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2170, 10, 1),
                H("land", 2174, 20, 1),
                H("land", 2175, 20, 1),
                H("land", 2177, 20, 1),
                H("beach", 2171, 10, 12),
            },
            CraterSpecies = new (ushort, float)[] { (9100, 2.0f) },
            CraterCount = 2, ClosedCraterHerdType = 217620,
        } },
        { "ua60vol_01_03", new Template
        {
            Name = "ua60vol_01_03", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2170, 10, 4),
                H("land", 2174, 20, 3),
                H("land", 2175, 20, 5),
                H("beach", 2171, 10, 30),
            },
            CraterSpecies = new (ushort, float)[] { (9002, 3.0f), (9003, 3.0f), (9012, 1.0f), (9013, 2.0f) },
            CraterCount = 8, ClosedCraterHerdType = 217620,
        } },
        { "ua60vol_02_01", new Template
        {
            Name = "ua60vol_02_01", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2170, 10, 12),
                H("land", 2174, 20, 10),
                H("land", 2175, 20, 15),
                H("land", 2176, 20, 15),
                H("land", 2177, 20, 12),
                H("land", 2178, 10, 10),
                H("beach", 2171, 10, 50),
            },
            CraterSpecies = new (ushort, float)[] { (9002, 2.0f), (9003, 2.0f), (9012, 2.0f), (9013, 2.0f), (9015, 1.0f) },
            CraterCount = 9, ClosedCraterHerdType = 217620,
        } },
        { "ua60vol_02_02", new Template
        {
            Name = "ua60vol_02_02", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2170, 10, 2),
                H("land", 2174, 20, 2),
                H("land", 2175, 20, 2),
                H("land", 2177, 20, 2),
                H("beach", 2171, 10, 12),
            },
            CraterSpecies = new (ushort, float)[] { (9013, 1.0f), (9100, 1.0f) },
            CraterCount = 2, ClosedCraterHerdType = 217620,
        } },
        { "ua60vol_02_03", new Template
        {
            Name = "ua60vol_02_03", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2170, 10, 3),
                H("land", 2174, 20, 3),
                H("land", 2175, 20, 3),
                H("land", 2176, 20, 3),
                H("land", 2188, 6, 8),
                H("beach", 2171, 10, 30),
            },
            CraterSpecies = new (ushort, float)[] { (9003, 2.0f), (9006, 1.0f), (9100, 2.0f), (9104, 3.0f) },
            CraterCount = 7, ClosedCraterHerdType = 217620,
        } },
        { "ua60vol_02_04", new Template
        {
            Name = "ua60vol_02_04", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2174, 20, 5),
                H("beach", 2171, 10, 12),
            },
            CraterSpecies = new (ushort, float)[] { (9002, 1.0f) },
            CraterCount = 1, ClosedCraterHerdType = 217620,
        } },
        { "ua60vol_03_01", new Template
        {
            Name = "ua60vol_03_01", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2005, 30, 1),
                H("land", 2170, 10, 8),
                H("land", 2174, 20, 5),
                H("land", 2175, 20, 10),
                H("land", 2176, 20, 10),
                H("land", 2177, 20, 8),
                H("land", 2178, 10, 8),
                H("beach", 2171, 10, 50),
            },
            CraterSpecies = new (ushort, float)[] { (9002, 3.0f), (9003, 2.0f), (9012, 2.0f), (9013, 2.0f), (9015, 1.0f), (9020, 2.0f) },
            CraterCount = 12, ClosedCraterHerdType = 217620,
        } },
        { "ua60vol_03_02", new Template
        {
            Name = "ua60vol_03_02", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2170, 10, 8),
                H("land", 2174, 20, 5),
                H("land", 2176, 20, 4),
                H("land", 2177, 20, 10),
                H("beach", 2171, 10, 15),
            },
            CraterSpecies = new (ushort, float)[] { (9002, 1.0f), (9003, 3.0f), (9020, 3.0f) },
            CraterCount = 8, ClosedCraterHerdType = 217620,
        } },
        { "ua60vol_04_01", new Template
        {
            Name = "ua60vol_04_01", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2005, 30, 1),
                H("land", 2170, 10, 10),
                H("land", 2174, 20, 5),
                H("land", 2175, 20, 10),
                H("land", 2176, 20, 10),
                H("land", 2177, 20, 8),
                H("land", 2178, 10, 8),
                H("land", 2179, 1, 5),
                H("beach", 2171, 10, 50),
            },
            CraterSpecies = new (ushort, float)[] { (9002, 2.0f), (9003, 2.0f), (9012, 2.0f), (9013, 2.0f), (9015, 1.0f), (9204, 1.0f) },
            CraterCount = 10, ClosedCraterHerdType = 217620,
        } },
        { "ua60vol_04_02", new Template
        {
            Name = "ua60vol_04_02", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2170, 10, 2),
                H("land", 2174, 20, 2),
                H("land", 2175, 20, 2),
                H("land", 2177, 20, 2),
                H("beach", 2171, 10, 30),
            },
            CraterSpecies = new (ushort, float)[] { (9205, 5.0f) },
            CraterCount = 5, ClosedCraterHerdType = 217620,
        } },
        { "ua60vol_04_03", new Template
        {
            Name = "ua60vol_04_03", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2185, 10, 12),
                H("land", 2185, 20, 10),
                H("beach", 2171, 10, 30),
            },
            CraterSpecies = new (ushort, float)[] { (9002, 1.0f), (9003, 3.0f), (9020, 3.0f) },
            CraterCount = 8, ClosedCraterHerdType = 217620,
        } },
        { "ua60vol_05_01", new Template
        {
            Name = "ua60vol_05_01", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2005, 30, 1),
                H("land", 2170, 10, 10),
                H("land", 2174, 20, 10),
                H("land", 2175, 20, 10),
                H("land", 2176, 20, 10),
                H("land", 2177, 20, 10),
                H("land", 2178, 10, 8),
                H("land", 2179, 1, 5),
                H("beach", 2171, 10, 50),
            },
            CraterSpecies = new (ushort, float)[] { (9002, 2.0f), (9003, 2.0f), (9012, 2.0f), (9013, 2.0f), (9015, 1.0f) },
            CraterCount = 9, ClosedCraterHerdType = 217620,
        } },
        { "ua60vol_05_02", new Template
        {
            Name = "ua60vol_05_02", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2174, 20, 2),
                H("land", 2177, 20, 2),
                H("beach", 2171, 10, 12),
            },
            CraterSpecies = new (ushort, float)[] { (9206, 1.0f) },
            CraterCount = 1, ClosedCraterHerdType = 217620,
        } },
        { "ua60vol_05_03", new Template
        {
            Name = "ua60vol_05_03", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2170, 10, 8),
                H("land", 2174, 20, 5),
                H("land", 2175, 20, 10),
                H("land", 2176, 20, 4),
                H("beach", 2171, 10, 30),
            },
            CraterSpecies = new (ushort, float)[] { (9003, 2.0f), (9006, 0.0f), (9100, 2.0f), (9104, 2.0f) },
            CraterCount = 7, ClosedCraterHerdType = 217620,
        } },
        { "ua60vol_06_01", new Template
        {
            Name = "ua60vol_06_01", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2005, 30, 1),
                H("land", 2170, 10, 10),
                H("land", 2174, 20, 10),
                H("land", 2175, 20, 10),
                H("land", 2176, 20, 10),
                H("land", 2177, 20, 10),
                H("land", 2178, 10, 8),
                H("land", 2179, 1, 5),
                H("beach", 2171, 10, 50),
            },
            CraterSpecies = new (ushort, float)[] { (9002, 2.0f), (9003, 2.0f), (9012, 2.0f), (9013, 2.0f), (9015, 1.0f) },
            CraterCount = 9, ClosedCraterHerdType = 217620,
        } },
        { "ua60vol_06_02", new Template
        {
            Name = "ua60vol_06_02", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2170, 10, 8),
                H("land", 2174, 20, 5),
                H("land", 2175, 20, 10),
                H("land", 2176, 20, 4),
                H("beach", 2171, 10, 30),
            },
            CraterSpecies = new (ushort, float)[] { (9003, 2.0f), (9006, 1.0f), (9100, 2.0f), (9104, 3.0f) },
            CraterCount = 7, ClosedCraterHerdType = 217620,
        } },
        { "ua60vol_06_03", new Template
        {
            Name = "ua60vol_06_03", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2170, 10, 8),
                H("land", 2174, 20, 5),
                H("land", 2175, 20, 10),
                H("land", 2177, 20, 8),
                H("beach", 2171, 10, 30),
            },
            CraterSpecies = new (ushort, float)[] { (9002, 1.0f), (9003, 1.0f), (9012, 1.0f), (9013, 1.0f) },
            CraterCount = 4, ClosedCraterHerdType = 217620,
        } },
        { "ua60vol_07_01", new Template
        {
            Name = "ua60vol_07_01", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2005, 30, 1),
                H("land", 2170, 10, 8),
                H("land", 2174, 20, 5),
                H("land", 2175, 20, 10),
                H("land", 2176, 20, 10),
                H("land", 2177, 20, 8),
                H("land", 2178, 10, 8),
                H("land", 2179, 1, 5),
                H("beach", 2171, 10, 50),
            },
            CraterSpecies = new (ushort, float)[] { (9002, 2.0f), (9003, 2.0f), (9012, 2.0f), (9013, 1.0f), (9015, 1.0f), (9208, 1.0f) },
            CraterCount = 9, ClosedCraterHerdType = 217620,
        } },
        { "ua60vol_07_02", new Template
        {
            Name = "ua60vol_07_02", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2174, 20, 2),
                H("land", 2175, 20, 2),
                H("land", 2177, 20, 2),
                H("land", 2186, 10, 8),
                H("beach", 2171, 10, 30),
            },
            CraterSpecies = new (ushort, float)[] { (9002, 1.0f), (9012, 1.0f), (9013, 1.0f) },
            CraterCount = 5, ClosedCraterHerdType = 217620,
        } },
        { "ua60vol_07_03", new Template
        {
            Name = "ua60vol_07_03", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2170, 10, 1),
                H("land", 2175, 20, 1),
                H("beach", 2171, 10, 10),
            },
            CraterSpecies = new (ushort, float)[] { (9209, 1.0f), (9212, 1.0f) },
            CraterCount = 2, ClosedCraterHerdType = 217620,
        } },
        { "ua60vol_08_01", new Template
        {
            Name = "ua60vol_08_01", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2005, 30, 1),
                H("land", 2170, 10, 10),
                H("land", 2174, 20, 10),
                H("land", 2175, 20, 10),
                H("land", 2176, 20, 10),
                H("land", 2177, 20, 10),
                H("land", 2178, 10, 8),
                H("land", 2179, 1, 5),
                H("beach", 2171, 10, 50),
            },
            CraterSpecies = new (ushort, float)[] { (9002, 2.0f), (9003, 2.0f), (9012, 2.0f), (9013, 2.0f), (9015, 1.0f) },
            CraterCount = 9, ClosedCraterHerdType = 217620,
        } },
        { "ua60vol_08_02", new Template
        {
            Name = "ua60vol_08_02", Level = 60, DesiredPopulation = 9000,
            Herds = new[] {
                H("land", 2170, 10, 3),
                H("land", 2174, 20, 3),
                H("land", 2176, 20, 3),
                H("land", 2187, 1, 8),
                H("beach", 2171, 10, 30),
            },
            CraterSpecies = new (ushort, float)[] { (9002, 1.0f), (9003, 3.0f), (9020, 3.0f) },
            CraterCount = 8, ClosedCraterHerdType = 217620,
        } },
        { "ur40gr170511", new Template
        {
            Name = "ur40gr170511", Level = 40, DesiredPopulation = 700,
            Herds = new[] {
                H("land", 2002, 4, 10),
                H("land", 2077, 4, 10),
                H("land", 2078, 4, 20),
                H("beach", 2010, 4, 5),
                H("beach", 2011, 4, 5),
                H("lake_shallow", 2017, 3, 5),
                H("lake_shallow", 2018, 6, 5),
                H("lake_deep", 2013, 6, 5),
                H("lake_deep", 2013, 7, 5),
                H("ocean", 2009, 2, 5),
                H("ocean", 2009, 3, 5),
            },
            CraterCount = 0, ClosedCraterHerdType = 0,
        } },
        { "ur40gr170615", new Template
        {
            Name = "ur40gr170615", Level = 40, DesiredPopulation = 3000,
            Herds = new[] {
                H("land", 2006, 20, 10),
                H("land", 2015, 20, 10),
                H("land", 2019, 21, 15),
                H("land", 2031, 20, 15),
                H("land", 2039, 11, 10),
                H("land", 2048, 10, 10),
                H("land", 2194, 7, 22),
                H("land", 2195, 7, 22),
            },
            CraterCount = 0, ClosedCraterHerdType = 201520,
        } },
        { "ur40te170511", new Template
        {
            Name = "ur40te170511", Level = 40, DesiredPopulation = 700,
            Herds = new[] {
                H("land", 2002, 4, 10),
                H("land", 2077, 4, 10),
                H("land", 2078, 4, 20),
                H("beach", 2010, 4, 5),
                H("beach", 2011, 4, 5),
                H("lake_shallow", 2017, 3, 5),
                H("lake_shallow", 2018, 6, 5),
                H("lake_deep", 2013, 6, 5),
                H("lake_deep", 2013, 7, 5),
                H("ocean", 2009, 2, 5),
                H("ocean", 2009, 3, 5),
            },
            CraterCount = 0, ClosedCraterHerdType = 0,
        } },
        { "ur40te170615", new Template
        {
            Name = "ur40te170615", Level = 40, DesiredPopulation = 3000,
            Herds = new[] {
                H("land", 2011, 20, 15),
                H("land", 2012, 20, 10),
                H("land", 2017, 20, 10),
                H("land", 2024, 20, 15),
                H("land", 2047, 20, 15),
                H("land", 2093, 20, 15),
                H("land", 2189, 4, 22),
                H("land", 2190, 4, 22),
            },
            CraterCount = 0, ClosedCraterHerdType = 201520,
        } },
        { "ur40tr170511", new Template
        {
            Name = "ur40tr170511", Level = 40, DesiredPopulation = 700,
            Herds = new[] {
                H("land", 2002, 4, 10),
                H("land", 2077, 4, 10),
                H("land", 2078, 4, 20),
                H("beach", 2010, 4, 5),
                H("beach", 2011, 4, 5),
                H("lake_shallow", 2017, 3, 5),
                H("lake_shallow", 2018, 6, 5),
                H("lake_deep", 2013, 6, 5),
                H("lake_deep", 2013, 7, 5),
                H("ocean", 2009, 2, 5),
                H("ocean", 2009, 3, 5),
            },
            CraterCount = 0, ClosedCraterHerdType = 0,
        } },
        { "ur40tr170615", new Template
        {
            Name = "ur40tr170615", Level = 40, DesiredPopulation = 3000,
            Herds = new[] {
                H("land", 2000, 21, 15),
                H("land", 2026, 21, 15),
                H("land", 2040, 20, 10),
                H("land", 2078, 20, 15),
                H("land", 2193, 5, 22),
                H("land", 2202, 1, 22),
                H("land", 2202, 8, 22),
            },
            CraterCount = 0, ClosedCraterHerdType = 201520,
        } },
        { "ur40tu170511", new Template
        {
            Name = "ur40tu170511", Level = 40, DesiredPopulation = 700,
            Herds = new[] {
                H("land", 2002, 4, 10),
                H("land", 2077, 4, 10),
                H("land", 2078, 4, 20),
                H("beach", 2010, 4, 5),
                H("beach", 2011, 4, 5),
                H("lake_shallow", 2017, 3, 5),
                H("lake_shallow", 2018, 6, 5),
                H("lake_deep", 2013, 6, 5),
                H("lake_deep", 2013, 7, 5),
                H("ocean", 2009, 2, 5),
                H("ocean", 2009, 3, 5),
            },
            CraterCount = 0, ClosedCraterHerdType = 0,
        } },
        { "ur40tu170615", new Template
        {
            Name = "ur40tu170615", Level = 40, DesiredPopulation = 3000,
            Herds = new[] {
                H("land", 2020, 20, 10),
                H("land", 2033, 20, 15),
                H("land", 2191, 16, 40),
                H("land", 2198, 10, 18),
                H("land", 2201, 10, 18),
                H("land", 2204, 16, 22),
            },
            CraterCount = 0, ClosedCraterHerdType = 201520,
        } },
    };

    /// <summary>
    /// หา template ให้ terrain id (เช่น "ri35te"): ตรงชื่อเป๊ะก่อน · ไม่งั้นเอาชื่อที่ขึ้นต้นด้วย id
    /// แล้วตามด้วยวันที่ 6 หลัก เวอร์ชันล่าสุด (ri35te171228 > ri35te170615) · ไม่มีเลยคืน null
    /// </summary>
    public static Template Find(string terrainId)
    {
        if (string.IsNullOrEmpty(terrainId)) { return null; }
        if (All.TryGetValue(terrainId, out Template exact)) { return exact; }
        Template best = null;
        foreach (KeyValuePair<string, Template> kv in All)
        {
            string n = kv.Key;
            if (n.Length != terrainId.Length + 6 || !n.StartsWith(terrainId, StringComparison.Ordinal)) { continue; }
            bool digits = true;
            for (int i = terrainId.Length; i < n.Length; i++) { if (!char.IsDigit(n[i])) { digits = false; break; } }
            if (!digits) { continue; }
            if (best == null || string.CompareOrdinal(n, best.Name) > 0) { best = kv.Value; }
        }
        return best;
    }
}
