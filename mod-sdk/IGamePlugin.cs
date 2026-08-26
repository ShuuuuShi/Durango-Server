namespace DurangoServer.Modding;

/// <summary>
/// ทุก mod ต้องมีคลาสเดียวที่ implement อันนี้ (เจอกี่คลาสก็โหลดหมด แต่ปกติ mod หนึ่งตัวมีคลาสเดียว)
///
/// [แก้เอง] 24 ส.ค. 2026 — เจ้าของสั่งแยกเฟสโหลดแบบ Minecraft/Forge: PluginManager เรียก 3 เฟสนี้
/// แบบ "ครบทุก mod ก่อนถึงจะขึ้นเฟสถัดไป" (ไม่ใช่ mod ตัวเดียวไล่ 3 เฟสรวดแล้วค่อยไป mod ถัดไป) —
/// ทำไมต้องแยก: ถ้า mod B ต้องอ้างถึงของที่ mod A ลงทะเบียนไว้ (เช่นเช็คว่า mod A ลงทะเบียนคำสั่งอะไรบ้าง)
/// ลำดับที่ PluginManager สแกนไฟล์เจอ A ก่อน B เป็นเรื่องบังเอิญ (ขึ้นกับชื่อไฟล์/OS) ⇒ ถ้าทุก mod
/// ทำ 3 เฟสรวดของตัวเองก่อน อาจมีจังหวะที่ B "โหลดเสร็จเต็มที่" ไปอ้างถึง A ที่ยังไม่ได้ลงทะเบียนอะไรเลย
/// การแยกเฟสข้ามทุก mod รับประกันว่า "ทุก mod ลงทะเบียนของพื้นฐานเสร็จ (PreLoad) ก่อนใครจะเริ่ม Load จริง
/// และทุก mod โหลดเสร็จ (Load) ก่อนใครจะเริ่มอ้างถึงกันข้าม mod (PostLoad)"
///
///   PreLoad  — ลงทะเบียนของพื้นฐานที่ mod อื่นอาจต้องรู้จัก (ชื่อ/สถานะเริ่มต้น) ยังไม่ควรพึ่งพา mod อื่น
///   Load     — งานหลัก: RegisterCommand/OnPlayerJoined/OnTick ฯลฯ ส่วนใหญ่มาลงที่นี่
///   PostLoad — ตอนอยากอ้างถึงสิ่งที่ mod อื่นลงทะเบียนไว้ตอน Load (การันตีว่า mod อื่นทำ Load เสร็จหมดแล้ว)
///
/// mod ที่ไม่ได้ใช้เฟสไหนปล่อยเป็น method ว่างได้เลย ไม่ต้องมี logic ก็ได้
/// </summary>
public interface IGamePlugin
{
    /// <summary>ชื่อ mod — ใช้ขึ้น log ตอนโหลด/error เท่านั้น ไม่ต้อง unique เป๊ะ</summary>
    string Name { get; }

    /// <summary>เวอร์ชัน mod (string อิสระ เช่น "1.0.0") — ใช้ขึ้น log เฉย ๆ</summary>
    string Version { get; }

    /// <summary>เฟส 1/3 — เรียกกับทุก mod ก่อนใครจะเข้าเฟส Load เลย ห้ามพึ่งพาว่า mod อื่นลงทะเบียนอะไรไว้แล้ว</summary>
    void OnPreLoad(IModApi api);

    /// <summary>เฟส 2/3 — งานหลักของ mod ส่วนใหญ่มาลงที่นี่ (RegisterCommand/OnPlayerJoined/OnTick ฯลฯ)</summary>
    void OnLoad(IModApi api);

    /// <summary>เฟส 3/3 — เรียกหลังทุก mod ผ่านเฟส Load หมดแล้ว ใช้ตอนอยากอ้างถึงของที่ mod อื่นลงทะเบียนไว้</summary>
    void OnPostLoad(IModApi api);
}
