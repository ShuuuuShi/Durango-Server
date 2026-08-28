using System;
using System.Collections.Generic;
using Durango.Modding;
using UnityEngine;

namespace ExampleClientMod
{

/// <summary>
/// Client mod ตัวอย่าง — สาธิตทุกจุดของ IClientModApi (V1.1) ในไฟล์เดียว:
///   1. Log — ข้อความขึ้น game log (เห็นใน output_log.txt / DbgView)
///   2. ShowMessage — popup กลางจอผ่าน UIManager.SystemMsg
///   3. RegisterHotkey — F10 = แจ้งตำแหน่งปัจจุบัน, F11 = นับของบางอย่าง (ตัวอย่างเรียกใช้คู่ข้อมูลจากเกม)
///   4. OnGameReady — ยิงครั้งเดียวเมื่อเข้าเกาะจริง (LocalPlayer != null แน่นอน)
///   5. OnUpdate [V1.1] — ทุกเฟรม: ตัวอย่างนับ FPS เฉลี่ยแบบเบา ๆ ด้วย dt
/// </summary>
public sealed class ExampleClientPlugin : IClientPlugin
{
    public string Name { get { return "ExampleClientMod"; } }
    public string Version { get { return "1.1.0"; } }

    private IClientModApi _api;
    // นับ FPS แบบเบา: สะสมเวลา 1 วินาทีแล้วเฉลี่ยทีเดียว (ไม่คูณหารทุกเฟรม)
    private double _fpsAccumSeconds;
    private int _fpsAccumFrames;
    private double _lastFps;
    // เก็บจุดเกิดไว้วาร์ปกลับ (ตัวอย่าง state ของ mod เอง — หายเมื่อปิดเกม ไม่มีระบบเซฟให้ mod ฝั่ง client)
    private Vector3 _anchor;
    private bool _hasAnchor;

    public void OnPreLoad(IClientModApi api)
    {
        // เตรียมของเบา ๆ ก่อน — ยังห้ามพึ่งพาว่า mod ตัวอื่นลงทะเบียนอะไรแล้ว
        _api = api;
        api.Log("PreLoad");
    }

    public void OnLoad(IClientModApi api)
    {
        api.Log("โหลดแล้ว — ปุ่มลัด: F10=แจ้งตำแหน่ง, F11=ตั้งจุด warp, F12=ย้อนกลับจุด warp, F9=FPS");

        // ปุ่มลัดผ่าน Input.GetKeyDown ตรง ๆ — ไม่ชนปุ่มลัดของเกม แต่ก็ไม่รู้ว่าผู้เล่นกำลังพิมพ์แชทอยู่
        // จึงควรใช้ปุ่มที่เกมไม่ได้ผูกอะไร (F9-F12 เป็นช่วงปลอดภัย)
        api.RegisterHotkey(KeyCode.F10, delegate
        {
            IClientPlayer p = api.LocalPlayer;
            if (p == null)
            {
                api.ShowMessage("ยังไม่ได้เข้าเกาะ — ลองใหม่หลังเข้าเกม");
                return;
            }
            api.ShowMessage("ตำแหน่ง: " + p.Position.ToString("F1"));
            api.Log("position=" + p.Position.ToString("F3"));
        });

        api.RegisterHotkey(KeyCode.F11, delegate
        {
            IClientPlayer p = api.LocalPlayer;
            if (p == null)
            {
                api.ShowMessage("ยังไม่ได้เข้าเกาะ — ตั้งจุด warp ไม่ได้");
                return;
            }
            _anchor = p.Position;
            _hasAnchor = true;
            api.ShowMessage("ตั้งจุด warp ที่ " + _anchor.ToString("F1"));
        });

        api.RegisterHotkey(KeyCode.F12, delegate
        {
            if (!_hasAnchor)
            {
                api.ShowMessage("ยังไม่ได้ตั้งจุด warp (กด F11 ก่อน)");
                return;
            }
            // ตัวอย่างนี้ไม่ย้ายตัวละคร (client mod ห้ามหลอกตำแหน่ง — เซิร์ฟ anti-cheat ดึงกลับ)
            // แค่รายงานว่าอยู่ห่างจากจุด warp เท่าไร จะได้ไม่สอนให้ mod ฝ่ากฎเกม
            IClientPlayer p = _api.LocalPlayer;
            if (p == null) { return; }
            float dist = Vector3.Distance(p.Position, _anchor);
            api.ShowMessage("ห่างจากจุด warp " + dist.ToString("F0") + " หน่วย");
        });

        api.RegisterHotkey(KeyCode.F9, delegate
        {
            api.ShowMessage("FPS เฉลี่ย ~" + _lastFps.ToString("F0"));
        });

        // เข้าเกาะจริงเมื่อไหร่ (ตัวละครเกิดในโลก ไม่ใช่แค่เกมบูต) — LocalPlayer การันตีไม่ null ตอนนี้
        api.OnGameReady(delegate
        {
            IClientPlayer p = api.LocalPlayer;
            api.Log("เข้าเกาะแล้ว ผู้เล่น: " + (p != null ? p.Name : "(null?!?)"));
            api.ShowMessage("ExampleClientMod พร้อมใช้ (F9-F12)");
        });

        // ทุกเฟรม — ห้ามทำงานหนัก (จริง ๆ ต่อ 120fps) ตัวอย่างนี้แค่สะสมตัวนับ
        api.OnUpdate(delegate(float dt)
        {
            _fpsAccumSeconds += dt;
            _fpsAccumFrames++;
            if (_fpsAccumSeconds >= 1.0)
            {
                _lastFps = _fpsAccumFrames / _fpsAccumSeconds;
                _fpsAccumSeconds = 0.0;
                _fpsAccumFrames = 0;
            }
        });
    }

    public void OnPostLoad(IClientModApi api)
    {
        // ตรงนี้ mod ทุกตัวผ่าน OnLoad หมดแล้ว — จะไปอ้างอิงของที่ mod อื่นลงทะเบียนไว้ได้อย่างอุ่นใจ
        api.Log("PostLoad — พร้อมใช้งานเต็มรูปแบบ");
    }
}

}
