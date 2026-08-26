namespace DurangoServer.Modding;

/// <summary>
/// จุดเชื่อมต่อทั้งหมดที่ mod เรียกเข้าเซิร์ฟได้ — ตั้งใจให้เล็กและเสถียร (อินเทอร์เฟซ ไม่ใช่คลาสจริง)
/// เพื่อไม่ให้ mod ต้อง compile ใหม่ทุกครั้งที่ภายในเซิร์ฟเปลี่ยน ตราบใดที่อินเทอร์เฟซนี้ไม่เปลี่ยน
///
/// V1 — ครอบคลุมสิ่งที่ mod ส่วนใหญ่ต้องการ: เพิ่มคำสั่งเอง + รู้เหตุการณ์คนเข้า/ออก/ทุก tick +
/// คุยกับผู้เล่นที่ออนไลน์อยู่ ยังไม่มี hook ระดับ "ก่อนคราฟต์/ก่อนต่อสู้" (เพิ่มทีหลังได้ตามที่ mod
/// จริงต้องการ โดยไม่ทำลาย mod เก่าเพราะอินเทอร์เฟซเป็นแบบเพิ่มเมธอดใหม่ได้แต่ห้ามลบ/เปลี่ยน signature เดิม)
/// </summary>
public interface IModApi
{
    /// <summary>เขียนลง console log ของเซิร์ฟ — ขึ้นต้นด้วย [mod:ชื่อmod] ให้อัตโนมัติ</summary>
    void Log(string message);

    /// <summary>
    /// ลงทะเบียนคำสั่งใหม่ พิมพ์ในเกมด้วย <c>cheat &lt;verb&gt; [args...]</c> (ต้องเปิดเซิร์ฟด้วย
    /// --enable-cheat เหมือนคำสั่งทดสอบในตัวทุกอัน — mod ไม่ผ่านด่านนี้ไปเองได้)
    /// handler รับผู้เล่นที่พิมพ์คำสั่ง + argument ที่เหลือ (ไม่รวมตัว verb) คืนข้อความไปโชว์ในเกม
    /// ชนกับ verb ของ mod อื่นหรือคำสั่งในตัว = ปฏิเสธการลงทะเบียน (ดูผลจาก log ตอนโหลด)
    /// </summary>
    bool RegisterCommand(string verb, Func<IModPlayer, string[], string> handler);

    /// <summary>เรียกทุกครั้งที่ผู้เล่นคนใหม่เข้าเกมสำเร็จ (ตัวละครเกิดในโลกแล้ว)</summary>
    void OnPlayerJoined(Action<IModPlayer> handler);

    /// <summary>เรียกทุกครั้งที่ผู้เล่นตัดการเชื่อมต่อ (ปิดเกม/หลุดเน็ต)</summary>
    void OnPlayerLeft(Action<IModPlayer> handler);

    /// <summary>เรียกทุก tick ของ main loop (~120 ครั้ง/วินาทีตามค่าเซิร์ฟ) — deltaSeconds คือเวลาห่างจาก
    /// tick ก่อนหน้าจริง ๆ (ไม่คงที่เป๊ะ) ห้ามทำงานหนักในนี้ (ทำทุก tick จริง ไม่ใช่ทุกวินาที)</summary>
    void OnTick(Action<double> handler);

    /// <summary>รายชื่อผู้เล่นที่ออนไลน์อยู่ตอนนี้ทั้งหมด</summary>
    IReadOnlyList<IModPlayer> GetOnlinePlayers();

    /// <summary>หาผู้เล่นออนไลน์จากชื่อ (ขึ้นต้นตรงกัน ไม่สนตัวพิมพ์) หรือ entity id เป๊ะ — ไม่เจอคืน null</summary>
    IModPlayer? FindPlayer(string nameOrEntityId);

    /// <summary>ส่งข้อความ (Info popup) ไปทุกคนที่ออนไลน์อยู่</summary>
    void BroadcastMessage(string text);
}
