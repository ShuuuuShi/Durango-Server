using System;
using System.Collections.Generic;
using System.Reflection;
using Durango.Utils;
using UnityEngine;

namespace DurangoMemoryBot
{
    /// <summary>
    /// วาดเส้นทาง A* ของ MemoryBotRouteWalker บน minimap — GL overlay
    ///
    /// ## วิธีทำงาน
    /// 1. อ่าน `MemoryBotRouteWalker.CurrentRoute` + `CurrentIndex`
    /// 2. หา minimap transform ด้วย reflection (ไม่ต้อง reference NGUI/MapContext)
    /// 3. แปลง tile → screen position ด้วย camera.WorldToScreenPoint
    /// 4. วาดจุดสี + เส้นเชื่อม ด้วย GL
    ///
    /// ## ข้อดี
    /// - ไม่ต้อง reference Durango.UI, NGUI, MapContext ฯลฯ
    /// - ใช้ได้กับทุก DLL version
    /// - ไม่ต้องหา minimap screen rect ล่วงหน้า — ใช้ transform จริงในฉาก
    /// </summary>
    public sealed class MinimapPathRenderer : MonoBehaviour
    {
        public static MinimapPathRenderer Instance { get; private set; }

        private const float DotSize = 8f;
        private const float ActiveDotSize = 12f;

        private static readonly Color WaypointColor = new Color(0.2f, 0.8f, 1f, 0.9f);
        private static readonly Color ActiveColor = new Color(1f, 0.9f, 0.2f, 1f);
        private static readonly Color LineColor = new Color(0.3f, 0.7f, 1f, 0.6f);
        private static readonly Color TraveledColor = new Color(0.5f, 0.5f, 0.5f, 0.4f);

        private Material _glMat;
        private Transform _minimapTransform;
        private Camera _minimapCamera;
        private float _nextFindTime;

        public static MinimapPathRenderer Ensure()
        {
            if (Instance != null) { return Instance; }
            GameObject host = new GameObject("__DurangoMinimapPathRenderer");
            DontDestroyOnLoad(host);
            Instance = host.AddComponent<MinimapPathRenderer>();
            return Instance;
        }

        private void Awake()
        {
            _glMat = new Material(Shader.Find("Hidden/Internal-Colored"));
        }

        private void OnDestroy()
        {
            if (_glMat != null) { DestroyImmediate(_glMat); }
        }

        /// <summary>หา minimap transform + camera ด้วย reflection — cache ไว้ ค้นใหม่ทุก 2 วินาที</summary>
        private void FindMinimap()
        {
            if (Time.time < _nextFindTime && _minimapTransform != null) { return; }
            _nextFindTime = Time.time + 2f;

            // วิธี 1: ลองหา MinimapGroupBase โดยตรง (ถ้ามีใน DLL)
            try
            {
                Type mmType = Type.GetType("Durango.UI.MinimapGroupBase, Assembly-CSharp");
                if (mmType != null)
                {
                    // Singleton<T>.HasInstance() + Instance()
                    Type singletonType = FindGenericSingletonType(mmType);
                    if (singletonType != null)
                    {
                        MethodInfo hasInst = singletonType.GetProperty("HasInstance",
                            BindingFlags.Public | BindingFlags.Static)?.GetGetMethod();
                        MethodInfo getInst = singletonType.GetProperty("Instance",
                            BindingFlags.Public | BindingFlags.Static)?.GetGetMethod();
                        if (hasInst != null && getInst != null)
                        {
                            bool has = (bool)hasInst.Invoke(null, null);
                            if (has)
                            {
                                object minimap = getInst.Invoke(null, null);
                                MethodInfo getTouch = mmType.GetMethod("GetTouchTransform");
                                if (getTouch != null)
                                {
                                    Transform t = getTouch.Invoke(minimap, null) as Transform;
                                    if (t != null)
                                    {
                                        _minimapTransform = t;
                                        _minimapCamera = FindMinimapCamera(t);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { }

            // วิธี 2: หา GameObject ที่ชื่อมี "minimap" / "Minimap"
            try
            {
                GameObject[] all = FindObjectsOfType<GameObject>();
                float bestY = -999f;
                foreach (GameObject go in all)
                {
                    if (go == null) { continue; }
                    string n = go.name.ToLowerInvariant();
                    if (n.Contains("minimap") || n.Contains("mini_map"))
                    {
                        // เลือกตัวที่อยู่สูงสุดบนหน้าจอ (minimap มักอยู่มุมขวาบน)
                        Vector3 sp = Camera.main != null
                            ? Camera.main.WorldToScreenPoint(go.transform.position)
                            : Vector3.zero;
                        if (sp.y > bestY && sp.y > 0)
                        {
                            bestY = sp.y;
                            _minimapTransform = go.transform;
                            _minimapCamera = go.GetComponent<Camera>();
                        }
                    }
                }
            }
            catch { }
        }

        private static Type FindGenericSingletonType(Type type)
        {
            Type t = type;
            while (t != null && t != typeof(object))
            {
                if (t.IsGenericType && t.GetGenericTypeDefinition().FullName == "Durango.Utils.Singleton`1")
                {
                    return t;
                }
                t = t.BaseType;
            }
            return null;
        }

        private static Camera FindMinimapCamera(Transform root)
        {
            Camera cam = root.GetComponent<Camera>();
            if (cam != null) { return cam; }
            cam = root.GetComponentInChildren<Camera>();
            if (cam != null) { return cam; }
            // หา camera ที่อยู่ใกล้ minimap ที่สุด
            Camera main = Camera.main;
            return main;
        }

        /// <summary>วาด path — เรียกจาก OnGUI ของ MonoBehaviour ตัวอื่น หรือจาก OnGUI เอง</summary>
        public void OnGUI()
        {
            if (!MemoryBotRouteWalker.Active) { return; }
            List<Point2> route = MemoryBotRouteWalker.CurrentRoute;
            if (route == null || route.Count < 2) { return;
            }

            FindMinimap();
            if (_minimapTransform == null) { return; }

            Camera cam = _minimapCamera ?? Camera.main;
            if (cam == null) { return; }

            // แปลง tile → world position แล้ว → screen position
            Vector2[] screenPts = new Vector2[route.Count];
            for (int i = 0; i < route.Count; i++)
            {
                Vector3 world = Durango.Terrain.Util.TilePositionToClientPosition(
                    new Vector2(route[i].x, route[i].y), true);
                Vector3 sp = cam.WorldToScreenPoint(world);
                // Unity screen: bottom-left origin, IMGUI: top-left origin
                screenPts[i] = new Vector2(sp.x, Screen.height - sp.y);
            }

            int activeIdx = MemoryBotRouteWalker.CurrentIndex;
            DrawGL(screenPts, activeIdx);
        }

        private void DrawGL(Vector2[] pts, int activeIdx)
        {
            if (_glMat == null) { return; }

            GL.PushMatrix();
            _glMat.SetPass(0);
            GL.LoadPixelMatrix();

            // เส้นที่เดินผ่านแล้ว (สีเทา)
            if (activeIdx > 1)
            {
                GL.Begin(GL.LINES);
                GL.Color(TraveledColor);
                for (int i = 0; i < activeIdx && i < pts.Length - 1; i++)
                {
                    GL.Vertex3(pts[i].x, pts[i].y, 0);
                    GL.Vertex3(pts[i + 1].x, pts[i + 1].y, 0);
                }
                GL.End();
            }

            // เส้นที่ยังไม่เดิน (สีฟ้า)
            if (activeIdx < pts.Length - 1)
            {
                GL.Begin(GL.LINES);
                GL.Color(LineColor);
                for (int i = activeIdx; i < pts.Length - 1; i++)
                {
                    GL.Vertex3(pts[i].x, pts[i].y, 0);
                    GL.Vertex3(pts[i + 1].x, pts[i + 1].y, 0);
                }
                GL.End();
            }

            // จุด waypoints ที่ยังไม่ถึง
            for (int i = activeIdx; i < pts.Length; i++)
            {
                bool active = (i == activeIdx);
                DrawDot(pts[i], active ? ActiveDotSize : DotSize,
                        active ? ActiveColor : WaypointColor);
            }

            GL.PopMatrix();
        }

        private static void DrawDot(Vector2 center, float size, Color color)
        {
            GL.Begin(GL.QUADS);
            GL.Color(color);
            float h = size * 0.5f;
            GL.Vertex3(center.x - h, center.y - h, 0);
            GL.Vertex3(center.x + h, center.y - h, 0);
            GL.Vertex3(center.x + h, center.y + h, 0);
            GL.Vertex3(center.x - h, center.y + h, 0);
            GL.End();
        }
    }
}
