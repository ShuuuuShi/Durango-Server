using System;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// [แก้เอง] เครื่องมือส่องหน้าตา UI ตอนเกมรันอยู่
///
/// ทำไมต้องมี: layout ของหน้าจอทุกหน้าอยู่ใน **prefab ของ Unity** ไม่ได้อยู่ในซอร์ส C#
/// ที่ build ทับได้ ⇒ จะขยับ/ย่อ/เปลี่ยนสีอะไรจากโค้ด ต้องรู้ก่อนว่าในนั้นมีวิดเจ็ตชื่ออะไร
/// อยู่ตรงไหน ขนาดเท่าไร — เดาจากชื่อคลาสอย่างเดียวไม่พอ (เดาผิดมาแล้ว)
///
/// เปิดใช้ด้วย env `DURANGO_UIDUMP=1` แล้วเปิดหน้าจอไหนก็ได้ในเกม
/// ไฟล์จะไปโผล่ที่ `<โฟลเดอร์เกม>/ui-dump/<ชื่อหน้าจอ>.txt`
///
/// ปิดอยู่โดยปริยาย — ชุดที่แจกผู้เล่นจึงไม่เขียนไฟล์อะไรทิ้งไว้
/// </summary>
public static class UiDump
{
    private static bool? _enabled;

    public static bool Enabled
    {
        get
        {
            if (!_enabled.HasValue)
            {
                string v = null;
                try { v = Environment.GetEnvironmentVariable("DURANGO_UIDUMP"); }
                catch (Exception) { }
                _enabled = !string.IsNullOrEmpty(v) && v != "0";
            }
            return _enabled.Value;
        }
    }

    private static string Dir
    {
        get
        {
            string beside = Path.GetDirectoryName(Application.dataPath);
            return Path.Combine(beside, "ui-dump");
        }
    }

    /// <summary>เขียนผังวิดเจ็ตของหน้าจอหนึ่งลงไฟล์ (เขียนทับของเดิม ไฟล์ละหน้าจอ)</summary>
    public static void Dump(GameObject root, string tag)
    {
        if (!Enabled || root == null)
        {
            return;
        }
        try
        {
            Directory.CreateDirectory(Dir);
            StringBuilder sb = new StringBuilder();
            sb.Append("# ").Append(tag).Append("  (").Append(root.name).Append(")\n");
            sb.Append("# หน้าจอ ").Append(Screen.width).Append("x").Append(Screen.height).Append("\n");
            sb.Append("# คอลัมน์: ชื่อ | คลาส | ตำแหน่ง x,y | กว้างxสูง | depth | anchor/pivot | ข้อความ\n\n");
            Walk(root.transform, 0, sb);
            string path = Path.Combine(Dir, Safe(tag) + ".txt");
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(true));
        }
        catch (Exception e)
        {
            Debug.Log("[uidump] เขียนไม่ได้: " + e.Message);
        }
    }

    /// <summary>
    /// เขียนรายชื่อ "วัสดุ" ที่เอาไปประกอบ UI ใหม่ได้ — สไปรต์ชื่ออะไรบ้าง ฟอนต์ตัวไหน
    ///
    /// เกมนี้ UISprite ไม่ได้ผูก atlas ไว้กับตัวเอง แต่ค้นชื่อผ่าน UISpriteManager
    /// ⇒ UI ใหม่ที่สร้างจากโค้ดแค่ตั้ง spriteName ให้ถูก ก็ได้หน้าตาเดียวกับหน้าจออื่นของเกม
    /// เขียนครั้งเดียว — ไฟล์ ui-dump/_assets.txt
    /// </summary>
    public static void DumpAssets(GameObject root)
    {
        if (!Enabled || root == null || _assetsDone)
        {
            return;
        }
        try
        {
            var fonts = new System.Collections.Generic.List<string>();
            UILabel[] labels = root.GetComponentsInChildren<UILabel>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                string f = labels[i].bitmapFont != null
                    ? "bitmap:" + labels[i].bitmapFont.name
                    : (labels[i].trueTypeFont != null ? "ttf:" + labels[i].trueTypeFont.name : null);
                if (f != null && !fonts.Contains(f))
                {
                    fonts.Add(f + "  (ขนาดที่เจอ " + labels[i].fontSize + ")");
                }
            }

            // สไปรต์ "ทั้งหมดที่เกมโหลดไว้" ไม่ใช่เฉพาะที่หน้าจอนี้ใช้
            var used = new System.Collections.Generic.List<string>();
            UISpriteManager mgr = ResourceSingleton<UISpriteManager>.Instance();
            if (mgr != null)
            {
                foreach (string n in mgr.AllSpriteNames())
                {
                    used.Add(n);
                }
            }
            if (used.Count == 0)
            {
                return;   // ยังโหลดไม่เสร็จ รอหน้าจอถัดไป
            }
            used.Sort(StringComparer.Ordinal);

            Directory.CreateDirectory(Dir);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# วัสดุสำหรับประกอบ UI ใหม่ (เก็บจาก " + root.name + ")");
            sb.AppendLine();
            sb.AppendLine("## ฟอนต์");
            for (int i = 0; i < fonts.Count; i++)
            {
                sb.AppendLine("  " + fonts[i]);
            }
            sb.AppendLine();
            sb.AppendLine("## สไปรต์ทั้งหมดที่ใช้ได้ (" + used.Count + ")");
            for (int i = 0; i < used.Count; i++)
            {
                sb.AppendLine("  " + used[i]);
            }
            File.WriteAllText(Path.Combine(Dir, "_assets.txt"), sb.ToString(), new UTF8Encoding(true));
            _assetsDone = true;
        }
        catch (Exception e)
        {
            Debug.Log("[uidump] assets: " + e.Message);
        }
    }

    private static bool _assetsDone;

    private static void Walk(Transform t, int depth, StringBuilder sb)
    {
        // ลึกเกินนี้เป็นชิ้นส่วนย่อยของวิดเจ็ต ไม่ได้ช่วยตัดสินใจเรื่อง layout
        if (depth > 8)
        {
            return;
        }
        sb.Append(new string(' ', depth * 2));
        sb.Append(t.name);

        UIWidget w = t.GetComponent<UIWidget>();
        Component[] comps = t.GetComponents<Component>();
        string classes = "";
        for (int i = 0; i < comps.Length; i++)
        {
            if (comps[i] == null)
            {
                continue;
            }
            string n = comps[i].GetType().Name;
            if (n == "Transform" || n == "GameObject")
            {
                continue;
            }
            classes += (classes.Length > 0 ? "," : "") + n;
        }
        sb.Append("  | ").Append(classes);

        Vector3 p = t.localPosition;
        sb.Append("  | ").Append(Mathf.RoundToInt(p.x)).Append(",").Append(Mathf.RoundToInt(p.y));

        if (w != null)
        {
            sb.Append("  | ").Append(w.width).Append("x").Append(w.height);
            sb.Append("  | d").Append(w.depth);
            sb.Append("  | ").Append(w.pivot);
        }
        if (!t.gameObject.activeSelf)
        {
            sb.Append("  | [ปิดอยู่]");
        }

        UILabel lb = t.GetComponent<UILabel>();
        if (lb != null && !string.IsNullOrEmpty(lb.text))
        {
            string txt = lb.text.Replace("\n", " ");
            if (txt.Length > 40)
            {
                txt = txt.Substring(0, 40) + "…";
            }
            sb.Append("  | \"").Append(txt).Append("\"");
        }
        sb.Append('\n');

        for (int i = 0; i < t.childCount; i++)
        {
            Walk(t.GetChild(i), depth + 1, sb);
        }
    }

    private static string Safe(string raw)
    {
        if (string.IsNullOrEmpty(raw))
        {
            return "unknown";
        }
        char[] bad = Path.GetInvalidFileNameChars();
        char[] buf = raw.ToCharArray();
        for (int i = 0; i < buf.Length; i++)
        {
            if (Array.IndexOf(bad, buf[i]) != -1)
            {
                buf[i] = '_';
            }
        }
        return new string(buf);
    }
}
