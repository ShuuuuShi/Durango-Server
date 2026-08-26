using System;
using Durango.UI;
using UnityEngine;

/// <summary>
/// [แก้เอง] ชุดประกอบ UI จากโค้ด — ใช้สร้างหน้าจอใหม่ทับของเดิมโดยไม่ต้องแก้ prefab
///
/// ทำไมต้องมีทั้งชุด: layout ของทุกหน้าอยู่ใน prefab ของ Unity ที่เราเปิดแก้ไม่ได้
/// (ลองแล้ว — บิลด์นี้ไม่มี TypeTree ติดมา เขียนฟิลด์ MonoBehaviour ตรง ๆ ไม่ได้)
/// แต่ NGUI สร้างวิดเจ็ตตอนรันได้ครบ (NGUITools.AddChild/AddWidget + UIEventListener)
/// และภาพประกอบก็หยิบจาก UISpriteManager ได้ทั้ง 4,500+ ชิ้นที่เกมโหลดไว้แล้ว
/// ⇒ วาดหน้าใหม่เองแล้วปิดของเดิมด้วย SetActive(false) จบ ไม่ต้องยุ่งกับ atlas
///
/// สี/ฟอนต์/สไปรต์ทุกตัวด้านล่าง **สุ่มวัดจากภาพหน้าจอจริง + ui-dump ของเกมเอง**
/// ไม่ได้คิดเอง — ดูให้เหมือนเกมมากที่สุดเท่าที่วาดเองจะทำได้
///
/// หน่วยพิกัดเป็นแบบ NGUI: จุด 0,0 อยู่กลางพาเนล แกน y ขึ้นเป็นบวก
/// </summary>
public static class UiKit
{
    // ── สี: สุ่มพิกเซลจากภาพหน้าจอจริงของเกม (shots/recipe-181547.png) ────────
    public static readonly Color Ground   = Hex(0x17, 0x17, 0x17);   // แถบหัวเรื่อง (เกือบดำ)
    public static readonly Color Panel    = Hex(0x1A, 0x2E, 0x51);   // พื้นแผงรายละเอียด (detail body)
    public static readonly Color Panel2   = Hex(0x31, 0x4A, 0x6F);   // แถวรายการ (recipe row)
    public static readonly Color PanelHi  = Hex(0x40, 0x4E, 0x60);   // แถวที่เลือกอยู่ (recipe row picked)
    public static readonly Color Line     = Hex(0x45, 0x5F, 0x86);
    public static readonly Color Gold     = Hex(0xF3, 0xBB, 0x32);   // ปุ่มสร้าง (craft button)
    public static readonly Color GoldDim  = Hex(0xA7, 0x91, 0x42);   // หมวดที่เลือกอยู่ (cat tile on)
    public static readonly Color Ink      = Hex(0xEF, 0xE6, 0xD4);
    public static readonly Color InkDim   = Hex(0x9B, 0xA9, 0xC0);
    public static readonly Color Ok       = Hex(0x8F, 0xB2, 0x68);
    public static readonly Color No       = Hex(0xD9, 0x72, 0x5A);

    // ── ฟอนต์: ขนาดที่เกมใช้จริงมีแค่ 10 / 14 / 16 (เก็บจาก ui-dump/_assets.txt) ─
    // [แก้เอง] เจ้าของทดลองแล้วบอกว่าเล็กไป — ขยับขึ้นทั้งชุดจากค่าที่วัดจากเกมตอนแรก
    // (บิตแมปฟอนต์ของ NGUI ขยายขนาดได้เรื่อย ๆ ไม่ได้ล็อกตายที่ 10/14/16 เหมือนที่เข้าใจตอนแรก)
    public const int FontTitle  = 20;   // หัวเรื่องหน้าต่าง
    public const int FontHead   = 17;   // หัวคอลัมน์/ป้ายหมวด
    public const int FontBody   = 16;   // ชื่อรายการ/ค่าทั่วไป
    public const int FontSmall  = 13;   // รายละเอียดย่อย/ป้ายกำกับ

    /// <summary>ฟอนต์ที่เกมใช้ — ต้องยืมจากป้ายที่มีอยู่ เพราะโหลดเองไม่ได้</summary>
    public static UIFont Font { get; private set; }

    private static Color Hex(int r, int g, int b)
    {
        return new Color(r / 255f, g / 255f, b / 255f, 1f);
    }

    /// <summary>สีเดิมแต่ปรับความทึบ — ใช้ทำพื้นแบบโปร่งใสให้เห็นฉาก 3 มิติทะลุออกมา</summary>
    public static Color WithAlpha(Color c, float a)
    {
        c.a = a;
        return c;
    }

    /// <summary>ยืมฟอนต์จากหน้าจอที่มีอยู่ — เรียกก่อนสร้างอะไรก็ตามที่มีตัวหนังสือ (เรียกซ้ำได้)</summary>
    public static bool Adopt(GameObject sample)
    {
        if (Font != null)
        {
            return true;
        }
        if (sample == null)
        {
            return false;
        }
        UILabel[] labels = sample.GetComponentsInChildren<UILabel>(true);
        for (int i = 0; i < labels.Length; i++)
        {
            if (labels[i].bitmapFont != null)
            {
                Font = labels[i].bitmapFont;
                return true;
            }
        }
        return false;
    }

    // ── ตัวสร้าง ──────────────────────────────────────────────────────

    public static UIWidget Group(GameObject parent, string name, int w, int h, int depth)
    {
        UIWidget g = parent.AddWidget<UIWidget>(depth);
        g.name = name;
        g.width = w;
        g.height = h;
        g.pivot = UIWidget.Pivot.Center;
        return g;
    }

    /// <summary>แผ่นภาพ — ใส่ชื่อสไปรต์ของเกม (ดูรายชื่อได้จาก ui-dump/_assets.txt)</summary>
    public static UISprite Box(GameObject parent, string sprite, int w, int h, int depth, Color color)
    {
        UISprite s = parent.AddWidget<UISprite>(depth);
        s.name = sprite;
        s.spriteName = sprite;
        s.type = UIBasicSprite.Type.Sliced;
        s.width = w;
        s.height = h;
        s.color = color;
        s.pivot = UIWidget.Pivot.Center;
        return s;
    }

    /// <summary>แผ่นสีล้วน — ใช้ bg_white แล้วทาสีเอา (ได้ทุกสีโดยไม่ต้องมีสไปรต์เฉพาะ)</summary>
    public static UISprite Fill(GameObject parent, int w, int h, int depth, Color color)
    {
        UISprite s = Box(parent, "bg_white", w, h, depth, color);
        s.type = UIBasicSprite.Type.Simple;
        return s;
    }

    /// <summary>ตัวหนังสือ — ขนาดฟอนต์ควรใช้ค่าคงที่ FontXxx ด้านบน ไม่ใช่ตัวเลขลอย ๆ</summary>
    public static UILabel Text(GameObject parent, string text, int size, int w, int depth, Color color)
    {
        UILabel l = parent.AddWidget<UILabel>(depth);
        l.name = "Label";
        if (Font != null)
        {
            l.bitmapFont = Font;
        }
        l.fontSize = size;
        l.width = w;
        l.height = size + 8;
        l.color = color;
        l.text = text ?? string.Empty;
        l.supportEncoding = true;
        l.overflowMethod = UILabel.Overflow.ShrinkContent;
        l.alignment = NGUIText.Alignment.Left;
        l.pivot = UIWidget.Pivot.Left;
        return l;
    }

    public static void OnClick(UIWidget w, Action action)
    {
        if (w == null || action == null)
        {
            return;
        }
        NGUITools.AddWidgetCollider(w.gameObject);
        UIEventListener.Get(w.gameObject).onClick = delegate
        {
            action();
        };
    }

    /// <summary>
    /// ปุ่มหลัก — ใช้สไปรต์ปุ่มจริงของเกม (btn_yellow_p_pc) ไม่ใช่กล่องทาสี
    /// ตัวหนังสือย้อมสีเข้ม (Ground) เพราะพื้นปุ่มเป็นสีทองสว่างอยู่แล้ว เหมือนปุ่ม "สร้าง" ตัวจริง
    /// </summary>
    public static UISprite PrimaryButton(GameObject parent, string label, int w, int h, int depth, bool enabled, Action onClick)
    {
        UISprite b = Box(parent, "btn_yellow_p_pc", w, h, depth, enabled ? Color.white : Hex(0x88, 0x88, 0x88));
        UILabel l = Text(b.gameObject, label, FontHead, w - 16, depth + 2, enabled ? Ground : Hex(0x55, 0x55, 0x55));
        l.alignment = NGUIText.Alignment.Center;
        l.pivot = UIWidget.Pivot.Center;
        At(l, 0, 0);
        if (enabled)
        {
            OnClick(b, onClick);
        }
        return b;
    }

    /// <summary>ปุ่มรอง — ใช้สไปรต์ปุ่มดำของเกม (btn_black_n_pc)</summary>
    public static UISprite SecondaryButton(GameObject parent, string label, int w, int h, int depth, bool on, Action onClick, int fontSize = 0)
    {
        UISprite b = Box(parent, "btn_black_n_pc", w, h, depth, Color.white);
        UILabel l = Text(b.gameObject, label, fontSize > 0 ? fontSize : FontHead, w - 16, depth + 2,
            on ? Gold : InkDim);
        l.alignment = NGUIText.Alignment.Center;
        l.pivot = UIWidget.Pivot.Center;
        At(l, 0, 0);
        OnClick(b, onClick);
        return b;
    }

    public static UISprite Divider(GameObject parent, int w, int depth, Color color)
    {
        return Fill(parent, w, 1, depth, color);
    }

    public static void At(Component c, int x, int y)
    {
        if (c != null)
        {
            c.transform.localPosition = new Vector3(x, y, 0f);
        }
    }

    public static void Destroy(ref GameObject go)
    {
        if (go != null)
        {
            UnityEngine.Object.Destroy(go);
            go = null;
        }
    }
}
