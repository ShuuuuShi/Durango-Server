using System;
using UnityEngine;

namespace Durango.Modding
{
    /// <summary>จุดเชื่อมต่อทั้งหมดที่ client mod เรียกเข้าตัวเกมได้ — เล็กและเสถียรเหมือน IModApi
    /// ฝั่งเซิร์ฟ V1 นี้เน้นของที่ mod ส่วนใหญ่ต้องการก่อน: ปุ่มลัดของตัวเอง + รู้ว่าเข้าเกาะแล้ว +
    /// ข้อความ popup — ยังไม่มี hook ระดับ UI (เปิด/ปิดหน้าจอไหน) หรือแก้ค่าตัวละครโดยตรง เพิ่มทีหลังได้</summary>
    public interface IClientModApi
    {
        /// <summary>เขียนลง Unity console/log (Debug.Log) ขึ้นต้นด้วย [clientmod:ชื่อ mod] ให้เอง</summary>
        void Log(string message);

        /// <summary>โชว์ข้อความ popup กลางจอ (UIManager.SystemMsg) — ใช้ในเกมเท่านั้น ก่อนเข้าเกาะจะไม่ขึ้นอะไร</summary>
        void ShowMessage(string text);

        /// <summary>ผูกปุ่มลัดของ mod เอง (เช็คทุกเฟรมแบบ Input.GetKeyDown ไม่ผ่านระบบ Layer/Command
        /// ของเกม ⇒ ไม่ชนกับปุ่มลัดในตัวเกม แต่ก็ไม่รู้บริบท เช่นกำลังพิมพ์แชทอยู่หรือเปล่า — mod
        /// ควรเลือกปุ่มที่ไม่ชนกับที่คนอาจพิมพ์ในแชท เช่น F9-F12)</summary>
        void RegisterHotkey(KeyCode key, Action onPressed);

        /// <summary>เรียกครั้งเดียวตอนตัวละครเกิดในโลกจริงสำเร็จ (เข้าเกาะแล้ว ไม่ใช่แค่เกมบูต) —
        /// ถ้าเข้าเกาะไปแล้วก่อน mod จะโหลด (ไม่ควรเกิดปกติ) จะเรียกทันทีที่ลงทะเบียน</summary>
        void OnGameReady(Action handler);

        /// <summary>ตัวละครของผู้เล่นเอง — null ถ้ายังไม่เข้าเกาะ (เช็คก่อนใช้เสมอ หรือรอ OnGameReady ก่อน)</summary>
        IClientPlayer LocalPlayer { get; }
    }

    public interface IClientPlayer
    {
        string Name { get; }
        Vector3 Position { get; }
    }
}
