"""สกัดชื่อคลิปอนิเมชันของสัตว์ (ยืน/เดิน/วิ่ง) จาก prefab bundle → AnimalMotionData.cs

ใช้: python scripts/extract_animal_motions.py <โฟลเดอร์ AssetBundles> <ServerCore/AnimalMotionData.cs>

ทำไมต้องอ่านจาก bundle: `Movement.MotionName` ที่ server ส่งไปคือชื่อคลิปที่ client จะเล่น
ส่ง null = สัตว์ยืนแข็ง แต่ชื่อคลิปจริงเป็น [SerializeField] อยู่ใน prefab ของสัตว์แต่ละตัว

ที่มาของชื่อ: ในแต่ละ prefab มี MonoBehaviour ชื่อ `<species>_ae` (animation events)
ซึ่งมี `AnimationEventPairs[].Name` = ชื่อคลิปทั้งหมดของตัวนั้น เช่น Raptor_Stand / Raptor_Walk / Raptor_Run

⚠️ อย่าใช้ตารางใน client/WildAnimalAI.cs — ตารางนั้นถูกใช้เฉพาะตอนโหมด offline
(มันเซ็ต `WildAnimalAI.CurType` เป็น static ก่อนสร้างตัว) เส้นทางออนไลน์ใช้ค่าที่ฝังใน prefab
เช่น Raptor ใช้ `Raptor_*` ไม่ใช่ `Allo_*` อย่างที่โค้ดนั้นเขียนไว้
"""
import io
import os
import re
import sys
import pathlib

import UnityPy

BUNDLE_DIR = pathlib.Path(sys.argv[1])
OUT = pathlib.Path(sys.argv[2])
ANIMAL_DATA = OUT.parent / "AnimalData.cs"

# entityType -> (ชื่อ, model path) จากไฟล์ที่ generate ไว้แล้ว
rows = re.findall(
    r'\{\s*(\d+),\s*new AnimalInfo\(\d+,\s*"([^"]*)",\s*"([^"]*)"',
    io.open(ANIMAL_DATA, encoding="utf-8").read())
print("อ่าน AnimalData ได้ %d ชนิด" % len(rows))

# index ชื่อไฟล์ bundle แบบตัดส่วน hash ทิ้ง
bundles = {}
for name in os.listdir(BUNDLE_DIR):
    if not name.endswith(".bundle"):
        continue
    trimmed = name[: -len(".bundle")]
    dot = trimmed.rfind(".")
    if dot > 0:
        bundles[trimmed[:dot].lower()] = name

CACHE = {}


def clips_of(model_path):
    """คืนชุดชื่อคลิปของ prefab นี้ (อ่านจาก MonoBehaviour ที่ลงท้ายด้วย _ae)"""
    key = "models$animals$" + model_path.lower().replace("/", "$") + ".prefab"
    if key in CACHE:
        return CACHE[key]
    file_name = bundles.get(key)
    if file_name is None:
        CACHE[key] = None
        return None
    names = []
    try:
        env = UnityPy.load(str(BUNDLE_DIR / file_name))
        for obj in env.objects:
            if obj.type.name != "MonoBehaviour":
                continue
            try:
                tt = obj.read_typetree()
            except Exception:            # noqa: BLE001 — MonoBehaviour บางตัวไม่มี typetree
                continue
            if not str(tt.get("m_Name", "")).endswith("_ae"):
                continue
            for pair in tt.get("AnimationEventPairs", []) or []:
                n = pair.get("Name")
                if n:
                    names.append(n)
    except Exception as e:               # noqa: BLE001
        print("  อ่าน %s ไม่ได้: %s" % (file_name, e))
    CACHE[key] = names or None
    return CACHE[key]


def pick(names, suffix, extra=()):
    """เลือกคลิปที่ลงท้ายด้วย _Stand/_Walk/_Run แบบเป๊ะก่อน แล้วค่อยเผื่อชื่อสำรอง"""
    for n in names:
        if n.endswith(suffix):
            return n
    for alt in extra:
        for n in names:
            if n.endswith(alt):
                return n
    return None


# ท่าโจมตีที่ "ขยับตัวเอง" — root motion ในคลิปจะลากโมเดลไปข้างหน้า
# พอเล่นจบ server สั่งตำแหน่งเดิมกลับ ตัวเลยเด้งกลับที่เก่า (เจอกับ Tricera_Active_Attack_Dash)
MOVING_ATTACK = ("dash", "charge", "jump", "leap", "run", "turn", "roar", "counter", "howl")


def pick_attacks(names):
    """
    ท่าโจมตี **ทั้งหมด** ที่ตีอยู่กับที่ (เดิมเก็บแค่ท่าเดียว สัตว์เลยตีท่าเดิมซ้ำตลอด)
    เรียงท่าที่ "ตรงที่สุด" ไว้หน้า เพราะ server จะใช้ตัวแรกเป็นท่าหลักถ้าต้องเลือกอันเดียว
    """
    attacks = [n for n in names if "Attack" in n]
    still = [n for n in attacks if not any(k in n.lower() for k in MOVING_ATTACK)]
    if not still:
        return [attacks[0]] if attacks else []
    ordered = []
    for suffix in ("_Attack_Once", "_Attack_Bite", "_Attack_Head", "_Attack"):
        for n in still:
            if n.endswith(suffix) and n not in ordered:
                ordered.append(n)
    for n in still:                       # ที่เหลือ (เช่น _Attack_Tail, _Attack_02)
        if n not in ordered:
            ordered.append(n)
    return ordered


found = []
missing = []
for type_str, name, model in rows:
    entity_type = int(type_str)
    names = clips_of(model) if model else None
    if not names:
        missing.append((entity_type, model))
        continue
    stand = pick(names, "_Stand", ("_Idle",))
    walk = pick(names, "_Walk", ("_Run",))
    run = pick(names, "_Run", ("_Walk",))
    attack = pick_attacks(names)
    die = pick(names, "_Die", ("_Dead",))
    if stand or walk:
        found.append((entity_type, name, stand, walk, run, attack, die))
    else:
        missing.append((entity_type, model))


def cs(v):
    return "null" if not v else '"%s"' % v


out = []
out.append("using System.Collections.Generic;")
out.append("")
out.append("namespace DurangoServer.Core;")
out.append("")
out.append("// ชื่อคลิปอนิเมชันของสัตว์ (generated จาก prefab ใน StreamingAssets/AssetBundles)")
out.append("// สร้างด้วย scripts/extract_animal_motions.py — อย่าแก้มือ")
out.append("//")
out.append("// server ต้องส่งชื่อคลิปไปกับ Movement.MotionName ไม่งั้นสัตว์โผล่มาแล้วยืนแข็ง")
out.append("// (client เรียก Anim.CrossFade(motionName) ตรง ๆ ใน AnimalBehavior.PlayAnimationMovement)")
out.append("public static class AnimalMotionData")
out.append("{")
out.append("    public readonly struct Motions")
out.append("    {")
out.append("        public readonly string Stand;")
out.append("        public readonly string Walk;")
out.append("        public readonly string Run;")
out.append("        /// <summary>ท่าโจมตีทั้งหมดของชนิดนี้ (สุ่มเลือกตอนตี — เดิมมีท่าเดียวเลยตีซ้ำท่าเดิมตลอด)</summary>")
out.append("        public readonly string[] Attacks;")
out.append("        public readonly string Die;")
out.append("")
out.append("        public Motions(string stand, string walk, string run, string[] attacks, string die)")
out.append("        {")
out.append("            Stand = stand;")
out.append("            Walk = walk;")
out.append("            Run = run;")
out.append("            Attacks = attacks ?? System.Array.Empty<string>();")
out.append("            Die = die;")
out.append("        }")
out.append("    }")
out.append("")
out.append("    private static Motions M(string stand, string walk, string run, string[] attacks, string die)")
out.append("    {")
out.append("        return new Motions(stand, walk, run, attacks, die);")
out.append("    }")
out.append("")
out.append("    /// <summary>entity type → ชุดคลิป (%d ชนิดที่อ่าน prefab ได้)</summary>" % len(found))
out.append("    public static readonly Dictionary<ushort, Motions> All = new Dictionary<ushort, Motions>()")
out.append("    {")
for entity_type, name, stand, walk, run, attack, die in found:
    atk = "null" if not attack else ("new[] { " + ", ".join(cs(a) for a in attack) + " }")
    out.append("        { %d, M(%s, %s, %s, %s, %s) },   // %s" % (
        entity_type, cs(stand), cs(walk), cs(run), atk, cs(die), name))
out.append("    };")
out.append("")
out.append("    public static bool TryGet(ushort entityType, out Motions motions)")
out.append("    {")
out.append("        return All.TryGetValue(entityType, out motions);")
out.append("    }")
out.append("")
out.append("    /// <summary>คลิป \"ยืนเฉย ๆ\" (null = ไม่รู้จักชนิดนี้)</summary>")
out.append("    public static string Stand(ushort entityType)")
out.append("    {")
out.append("        return All.TryGetValue(entityType, out Motions m) ? m.Stand : null;")
out.append("    }")
out.append("")
out.append("    public static string Walk(ushort entityType)")
out.append("    {")
out.append("        return All.TryGetValue(entityType, out Motions m) ? m.Walk : null;")
out.append("    }")
out.append("")
out.append("    public static string Run(ushort entityType)")
out.append("    {")
out.append("        return All.TryGetValue(entityType, out Motions m) ? m.Run : null;")
out.append("    }")
out.append("")
out.append("    /// <summary>คลิปโจมตี (null = ไม่มีท่าโจมตีในข้อมูล)</summary>")
out.append("    private static readonly System.Random _rng = new System.Random();")
out.append("")
out.append("    /// <summary>สุ่มท่าโจมตี 1 ท่าจากที่ชนิดนี้มี (null = ไม่รู้จัก/ไม่มีท่าโจมตี)</summary>")
out.append("    public static string Attack(ushort entityType)")
out.append("    {")
out.append("        if (!All.TryGetValue(entityType, out Motions m) || m.Attacks.Length == 0)")
out.append("        {")
out.append("            return null;")
out.append("        }")
out.append("        if (m.Attacks.Length == 1)")
out.append("        {")
out.append("            return m.Attacks[0];")
out.append("        }")
out.append("        lock (_rng)")
out.append("        {")
out.append("            return m.Attacks[_rng.Next(m.Attacks.Length)];")
out.append("        }")
out.append("    }")
out.append("")
out.append("    /// <summary>ท่าโจมตีทั้งหมดของชนิดนี้ (ไว้ดูตอนดีบั๊ก)</summary>")
out.append("    public static string[] AllAttacks(ushort entityType)")
out.append("    {")
out.append("        return All.TryGetValue(entityType, out Motions m) ? m.Attacks : System.Array.Empty<string>();")
out.append("    }")
out.append("")
out.append("    /// <summary>คลิปตาย</summary>")
out.append("    public static string Die(ushort entityType)")
out.append("    {")
out.append("        return All.TryGetValue(entityType, out Motions m) ? m.Die : null;")
out.append("    }")
out.append("}")
out.append("")

OUT.write_text("\n".join(out), encoding="utf-8")
print("เขียน %s: %d ชนิดมีคลิป, %d ชนิดหา prefab/คลิปไม่เจอ" % (OUT, len(found), len(missing)))
if missing[:5]:
    print("ตัวอย่างที่ไม่เจอ:", missing[:5])
