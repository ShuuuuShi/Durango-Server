using Durango.System;
using Durango.UI.Control;
using UnityEngine;

/// <summary>
/// [แก้เอง v4] จัดตำแหน่งหน้าคราฟต์ใหม่ — ใช้วิดเจ็ตของเกม 100% ไม่วาดอะไรเอง
///
/// ทำไมรอบก่อนพัง: `Container` มี `RectLayoutComponent` ติดอยู่ ซึ่งจับตำแหน่งกลับไปที่
/// anchor เดิมทุกครั้งที่ถูกเปิด (ทำงานผ่าน `OnEnable()` ไม่ใช่ทุกเฟรมอย่างที่เข้าใจผิดตอนแรก
/// — แต่ผลคือเหมือนกัน: ตั้งตำแหน่งเองแล้วโดนดึงกลับตอนเปิดครั้งถัดไป)
///
/// ทางแก้จริง: **ปิดคอมโพเนนต์นั้นทิ้งไปเลย** (`enabled = false`) ก่อนตั้งตำแหน่งเอง
/// แล้วเช็คแล้วว่า `CategoryListWidget` กับ `RecipeListWidget` (สองตัวที่ต้องย้าย
/// ซ้าย/ขวา) **ไม่มี** RectLayoutComponent ติดอยู่เลย — ย้ายได้อิสระไม่มีใครดึงกลับ
/// (เช็คจาก ui-dump/RecipeSelectorGroup.txt ตรง ๆ ไม่ได้เดา)
///
/// สิ่งที่ทำ: ขยาย Container ให้กว้างพอใส่สองแผงข้างกัน (ไม่ทำเต็มจอ — จะได้ไม่ต้อง
/// รื้อ clip region ของ scrollview ข้างในซึ่งเสี่ยงพังกว่า) แล้วย้าย
///   CategoryListWidget → ซ้าย
///   RecipeListWidget    → ขวา
/// โดยไม่แตะขนาดของทั้งสองตัวเลย (ใช้ 582 กว้างเท่าที่ออกแบบไว้) ⇒ ไม่ต้องยุ่งกับ
/// clip region ภายในของมันเลย ลดความเสี่ยงลงมาก
///
/// ปิดกลับไปใช้ตำแหน่งเดิมได้ด้วย env `DURANGO_NEWUI=0` (ไม่ต้อง build ใหม่)
///
/// [แก้เอง v5] สองการเปลี่ยนแปลงรอบนี้:
///
/// 1) ย้ายกล่องทั้งก้อน (Container/BackSprite) ไปชิดขอบขวาจอแทนกึ่งกลาง — เดิม boxW ถูกขยาย
///    ให้ "เกือบเต็มความกว้างจอ" เสมอ (Max(ContentW, vw-BoxMargin*2)) ซึ่งพอเอาไปชิดขวาจริง ๆ
///    จะล้นขอบซ้ายจอทันที (กล่องกว้างเกือบเท่าจอ แต่ไม่ได้อยู่กึ่งกลางอีกต่อไป) ⇒ เปลี่ยนสูตร
///    ความกว้างกล่องเป็น "ContentW + padding ตกแต่งคงที่" แทน (ดู CraftUiLayoutConfig.BoxPadding)
///    แล้วค่อยเอา BoxMargin ไปใช้เป็นแค่เพดานปลอดภัยไม่ให้ล้นจอ ⇒ กล่องมีขนาดพอดีเนื้อหา
///    ชิดขวาได้จริงโดยไม่ล้นซ้าย (คำนวณจาก vw จริงทุกครั้ง ไม่ hardcode)
///
///    ผลข้างเคียงที่ต้องแก้ด้วย: clip region ของ root panel (ขั้นตอน 4 เดิม) เคยขยายแค่
///    "กว้าง/สูงขึ้น" รอบจุดศูนย์กลางเดิม (0,0) เฉย ๆ — ใช้ได้ตอนกล่องยังอยู่กึ่งกลาง แต่พอกล่อง
///    ขยับไปทางขวา จุดศูนย์กลาง clip เดิมจะไม่ตรงกับกล่องอีกต่อไป ⇒ ขอบขวาของกล่องจะโดน clip
///    (มองไม่เห็น/ถูกตัด) ทั้งที่ widget ไม่ได้พังอะไร ⇒ เปลี่ยนเป็นคำนวณ "กรอบรวม" (union) ของ
///    clip เดิมกับตำแหน่งกล่องใหม่แทนการขยายรอบจุดเดิม
///
/// 2) ค่าคงที่ทั้งหมด (EdgeMargin/PanelGap/PanelW/BoxPadding/BoxMargin/RightMargin) ย้ายไปอยู่ใน
///    CraftUiLayoutConfig อ่านจากไฟล์ JSON ข้าง ๆ exe แทน const ในโค้ด — แก้เลขแล้วปิด-เปิดหน้าคราฟ
///    ใหม่ในเกมเห็นผลทันที ไม่ต้อง build+relaunch ทั้งตัว
///
/// [แก้เอง v6] 23 ส.ค. 2026 — เจ้าของสั่งเอาระบบ hot-reload ของ v5 ออก: ไฟล์
/// <c>craft_ui_layout.json</c> ที่ตกค้างข้าง exe มี RightMargin เพี้ยนเป็น 100 (จาก 20) ทำให้กล่อง
/// คราฟต์ขยับหนีตำแหน่งที่เคยจูนไว้ดีแล้วแบบไม่มีใครรู้ตัว ⇒ กลับไปใช้ const ในโค้ดตรง ๆ เหมือนก่อน v5
/// (ค่าตรงกับที่เคยจูนไว้ดีทุกตัว) และลบป้าย debug สีแดงที่ค้างแสดงตลอดเวลา (`ShowDiagLabel`,
/// มีคอมเมนต์เดิมบอกไว้แล้วว่า "ลบทิ้งได้เมื่อจัดตำแหน่งเสร็จแน่นอนแล้ว") ทิ้งไปด้วย
/// </summary>
public static class CraftScreen
{
    private static bool? _enabled;

    /// <summary>
    /// [แก้เอง v7] 23 ส.ค. 2026 — งานนี้จัดตำแหน่งจริงเฉพาะ **prefab ฝั่ง PC** (`RecipeSelectorGroup_PC`,
    /// ตรวจ resources.assets ตรง ๆ แล้วพบว่า path_id 3245 ต่างจาก mobile path_id 1054 จริง — ลำดับ/
    /// ชื่อลูกใน hierarchy คนละชุดกันเลย เช่น mobile ไม่มี "RecipeFilterWidget" เป็นลูกตรงของ
    /// Container แบบ PC แต่ซ้อนอยู่ใต้ RecipeListWidget แทน, "BackSprite" ของ mobile เป็นพี่น้องกับ
    /// Container ไม่ใช่ลูกของมันแบบ PC) เดิม `Enabled` เช็คแค่ env `DURANGO_NEWUI` เฉย ๆ ⇒ พอสลับ
    /// default เป็น UI มือถือ (Platform_PC.cs) โค้ดนี้ยังรันทับ prefab มือถือของจริงอยู่ดี ทั้งที่
    /// container.Find("BackSprite") หาไม่เจอ (เงียบ ๆ ไม่ throw) ⇒ พังครึ่ง ๆ กลาง ๆ ดูเหมือนเอา
    /// เลย์เอาต์ PC มาใช้ทั้งที่จริง ๆ คือ mobile prefab ที่ถูกจัดตำแหน่งทับผิด ๆ
    /// ⇒ ให้ทำงานเฉพาะตอน UsePCUI==true เท่านั้น โหมดมือถือปล่อย prefab มือถือ (ที่ NEXON ออกแบบ
    /// มาเองแล้ว) render ตามธรรมชาติ ไม่ไปยุ่งด้วย
    /// </summary>
    public static bool Enabled
    {
        get
        {
            if (!_enabled.HasValue)
            {
                string v = null;
                try { v = System.Environment.GetEnvironmentVariable("DURANGO_NEWUI"); }
                catch (System.Exception) { }
                bool envAllows = string.IsNullOrEmpty(v) || v != "0";
                _enabled = envAllows && Platform.Instance.UsePCUI;
            }
            return _enabled.Value;
        }
    }

    private struct Saved
    {
        public Transform T;
        public Vector3 Pos;
        public bool HasWidget;
        public int W, H;
    }

    private static Saved _container, _back, _cats, _list;
    private static RectLayoutComponent _containerLayout;
    private static bool _layoutWasEnabled;
    private static UIPanel _rootPanel;
    private static Vector4 _rootClipBefore;
    private static bool _moved;

    public static void Show(GameObject group, System.Action onClose, System.Action<RecipeSystem.RecipeType, string> onCraft)
    {
        if (!Enabled || group == null)
        {
            return;
        }
        try
        {
            Apply(group);
        }
        catch (System.Exception e)
        {
            Debug.Log("[craftui] จัดตำแหน่งไม่สำเร็จ (" + e.Message + ") — คืนของเดิม");
            Restore();
        }
    }

    public static void Hide(GameObject group)
    {
        Restore();
    }

    private static void Apply(GameObject group)
    {
        Transform container = group.transform.Find("Container");
        if (container == null)
        {
            return;
        }
        Transform back = container.Find("BackSprite");
        Transform cats = container.Find("CategoryListWidget");
        Transform list = container.Find("RecipeListWidget");
        if (cats == null || list == null)
        {
            return;
        }

        Remember(container, back, cats, list);

        // ── 1) ปิดตัวบังคับ anchor ของ Container ก่อน ไม่งั้นเปิดรอบหน้าโดนดึงกลับ ──
        _containerLayout = container.GetComponent<RectLayoutComponent>();
        if (_containerLayout != null)
        {
            _layoutWasEnabled = _containerLayout.enabled;
            _containerLayout.enabled = false;
        }

        // ── 2) ค่าคงที่ตำแหน่ง/ขนาด ──────────────────────────────────────────────
        // [แก้เอง v6] 23 ส.ค. 2026 — เจ้าของสั่งเอาระบบ hot-reload (CraftUiLayoutConfig,
        // อ่านไฟล์ craft_ui_layout.json ข้าง ๆ exe ทุกครั้งที่เปิดหน้าจอ) ออก กลับไปเป็นค่าคงที่
        // ในโค้ดตรง ๆ เหมือนก่อน v5 — เคยมีไว้ช่วยลองเลขระหว่างจูนเลย์เอาต์ไม่ต้อง build ใหม่ทุกครั้ง
        // แต่ไฟล์ JSON ที่ตกค้างอยู่ข้าง exe (RightMargin เพี้ยนเป็น 100 จาก 20) ทำให้กล่องคราฟต์
        // ขยับหนีขอบจอผิดที่ไปโดยไม่มีใครรู้ตัว ⇒ ตัดจุดที่ทำให้ผลลัพธ์ไม่แน่นอนออก ค่าตรงนี้คือค่า
        // เดิมที่เคยจูนไว้ดีแล้ว (ตรงกับ default เดิมของ CraftUiLayoutConfig ทุกตัว ยกเว้น
        // RightMargin ที่ตั้งใจคง 20 ตามดีไซน์เดิม ไม่ใช้ค่า 100 ที่หลุดมาจากไฟล์)
        const int edgeMargin = 20;
        const int panelGap = 20;
        const int panelW = 582;
        const int boxPadding = 60;
        const int boxMargin = 40;
        const int rightMargin = 20;
        int contentW = panelW * 2 + edgeMargin * 2 + panelGap;

        // ── 3) ขยาย Container + พื้นหลังของมันให้กว้างพอใส่สองแผง ──────────────
        // [แก้เอง] เดิมคำนวณความกว้างเสมือนจาก vh*aspect — ผิด เพราะ UIRoot ของเกมนี้
        // ล็อกความกว้างไว้คงที่ (Constraint.FitWidth: activeHeight = manualWidth/aspect
        // ⇒ **ความกว้างคงที่ที่ manualWidth เสมอ ความสูงต่างหากที่ขยับตามจอ**)
        // ⇒ ใช้ root.manualWidth ตรง ๆ แทนการคำนวณจาก aspect เอง
        int vh = 900;
        int vw = 1280;
        UIRoot root = group.GetComponentInParent<UIRoot>();
        if (root != null && root.activeHeight > 0)
        {
            vh = root.activeHeight;
            vw = root.manualWidth > 0 ? root.manualWidth : vw;
        }

        // [แก้เอง v5] เดิม boxW = Max(ContentW, vw-BoxMargin*2) ⇒ เกือบเต็มจอเสมอ ทำให้ "ชิดขวา"
        // ไม่มีความหมาย (กล่องกว้างขนาดนั้นต้องอยู่กลางจอไม่งั้นล้นซ้าย) ⇒ ตอนนี้ขนาดกล่องมาจาก
        // เนื้อหาจริง (contentW) + padding ตกแต่งคงที่ (boxPadding) ส่วน vw ใช้แค่เป็น "เพดาน
        // ปลอดภัย" ห้ามกล่องกว้างเกินจอ (ยังคงคำนวณจากความกว้างจอจริงเหมือนเดิม ไม่ hardcode)
        int boxW = Mathf.Clamp(contentW + boxPadding * 2, contentW, Mathf.Max(contentW, vw - boxMargin * 2));

        // ── 4) ดันกล่องทั้งก้อนไปชิดขอบขวาจอ (แทนกึ่งกลางเดิม) ───────────────────
        // localPosition.x = 0 คือกึ่งกลางจอ (แกน x ของ UIRoot วิ่ง -vw/2 .. +vw/2) ⇒ ตำแหน่งที่ทำให้
        // ขอบขวาของกล่องอยู่ห่างขอบขวาจอ = rightMargin คือ containerX = (vw/2 - rightMargin) - boxW/2
        // กันไว้อีกชั้น: ห้ามขอบซ้ายของกล่องหลุดเลย (-vw/2 + boxMargin) ไปทาง HUD ซ้ายบนเด็ดขาด
        float half = vw / 2f;
        float containerX = (half - rightMargin) - boxW / 2f;
        float minContainerX = -half + boxMargin + boxW / 2f;
        if (containerX < minContainerX)
        {
            containerX = minContainerX;
        }

        Resize(container, boxW, vh);
        container.localPosition = new Vector3(containerX, 0f, container.localPosition.z);
        if (back != null)
        {
            // back เป็นลูกของ container ⇒ ตำแหน่ง (0,0) สัมพัทธ์เดิมถูกต้องอยู่แล้ว ขยับตาม container ไปเอง
            Resize(back, boxW + 2, vh + 2);
            back.localPosition = Vector3.zero;
        }

        // ── 5) ย้าย CategoryListWidget ไปซ้าย, RecipeListWidget ไปขวา (ภายใน container) ──
        // ไม่แตะขนาดทั้งสองตัวเลย (panelW เท่าที่ออกแบบไว้) ⇒ clip region/scrollview ภายใน
        // ยังถูกต้อง ไม่ต้องคำนวณใหม่ — คิดจาก contentW (ไม่ใช่ boxW ที่ขยายไปแล้ว) เพื่อไม่ให้
        // สองแผงถูกดันห่างออกจากกันตามกล่องพื้นหลังที่ใหญ่ขึ้น ตำแหน่งนี้เป็น local ของ container
        // เอง (สัมพัทธ์) จึงไม่ต้องรู้เรื่อง containerX ด้านบนเลย — ย้ายกล่องแม่พอ ลูกตามไปเอง
        int contentHalf = contentW / 2;
        int panelHalf = panelW / 2;
        int catsX = -contentHalf + edgeMargin + panelHalf;
        int listX = contentHalf - edgeMargin - panelHalf;
        cats.localPosition = new Vector3(catsX, cats.localPosition.y, cats.localPosition.z);
        list.localPosition = new Vector3(listX, list.localPosition.y, list.localPosition.z);

        // ── 6) กันโดนตัดภาพ — ถ้า panel รากมี clip region ต้องครอบคลุมตำแหน่งกล่องใหม่ ────────
        // [แก้เอง v5] เดิมขยาย "กว้าง/สูงขึ้นรอบจุดศูนย์กลางเดิม (c.x,c.y)" เฉย ๆ ใช้ได้ตอนกล่อง
        // ยังอยู่กึ่งกลางจอ (containerX เดิม = 0) แต่พอกล่องขยับไปทางขวาแล้ว จุดศูนย์กลาง clip
        // เดิมจะไม่ตรงกับกล่องอีกต่อไป ⇒ ขอบขวาของกล่องโดนตัด (มองไม่เห็น) ทั้งที่ widget ไม่พัง
        // ⇒ เปลี่ยนเป็นคำนวณ "กรอบรวม" (union) ของ clip เดิม ∪ ตำแหน่งกล่องใหม่แทน รับประกันว่า
        // ทั้งของเดิมที่เคยมองเห็นได้ และกล่องคราฟต์ตำแหน่งใหม่ อยู่ในกรอบ clip เสมอ
        _rootPanel = group.GetComponentInParent<UIPanel>();
        if (_rootPanel != null && _rootPanel.clipping != UIDrawCall.Clipping.None)
        {
            _rootClipBefore = _rootPanel.baseClipRegion;
            Vector4 c = _rootClipBefore;
            float pad = 40f;
            float oldLeft = c.x - c.z / 2f;
            float oldRight = c.x + c.z / 2f;
            float oldTop = c.y - c.w / 2f;
            float oldBottom = c.y + c.w / 2f;
            float boxLeft = containerX - boxW / 2f - pad;
            float boxRight = containerX + boxW / 2f + pad;
            float boxTop = -vh / 2f - pad;
            float boxBottom = vh / 2f + pad;
            float newLeft = Mathf.Min(oldLeft, boxLeft);
            float newRight = Mathf.Max(oldRight, boxRight);
            float newTop = Mathf.Min(oldTop, boxTop);
            float newBottom = Mathf.Max(oldBottom, boxBottom);
            _rootPanel.baseClipRegion = new Vector4((newLeft + newRight) / 2f, (newTop + newBottom) / 2f,
                newRight - newLeft, newBottom - newTop);
        }

        _moved = true;
        Debug.Log("[craftui] จัดตำแหน่งแล้ว: กล่อง=" + boxW + "x" + vh + " ที่ x=" + containerX
            + " (rightMargin=" + rightMargin + ") หมวดหมู่ซ้าย(x=" + catsX + ") / รายการขวา(x=" + listX + ")");
    }

    private static void Remember(Transform container, Transform back, Transform cats, Transform list)
    {
        _container = Snapshot(container);
        _back = Snapshot(back);
        _cats = Snapshot(cats);
        _list = Snapshot(list);
    }

    private static Saved Snapshot(Transform t)
    {
        if (t == null)
        {
            return default;
        }
        UIWidget w = t.GetComponent<UIWidget>();
        return new Saved
        {
            T = t,
            Pos = t.localPosition,
            HasWidget = w != null,
            W = w != null ? w.width : 0,
            H = w != null ? w.height : 0
        };
    }

    private static void Restore()
    {
        if (_moved)
        {
            RestoreOne(_container);
            RestoreOne(_back);
            RestoreOne(_cats);
            RestoreOne(_list);
            if (_containerLayout != null)
            {
                _containerLayout.enabled = _layoutWasEnabled;
            }
            if (_rootPanel != null && _rootPanel.clipping != UIDrawCall.Clipping.None)
            {
                _rootPanel.baseClipRegion = _rootClipBefore;
            }
        }
        _container = default;
        _back = default;
        _cats = default;
        _list = default;
        _containerLayout = null;
        _rootPanel = null;
        _moved = false;
    }

    private static void RestoreOne(Saved s)
    {
        if (s.T == null)
        {
            return;
        }
        try
        {
            s.T.localPosition = s.Pos;
            if (s.HasWidget)
            {
                UIWidget w = s.T.GetComponent<UIWidget>();
                if (w != null)
                {
                    w.width = s.W;
                    w.height = s.H;
                }
            }
        }
        catch (System.Exception)
        {
            // โดนทำลายไปพร้อมฉากแล้ว ไม่เป็นไร
        }
    }

    private static void Resize(Transform t, int w, int h)
    {
        UIWidget widget = t.GetComponent<UIWidget>();
        if (widget != null)
        {
            widget.width = Mathf.Max(2, w);
            widget.height = Mathf.Max(2, h);
        }
    }

}
