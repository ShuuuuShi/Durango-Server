namespace DurangoServer.Modding;

/// <summary>ตัวแทนผู้เล่นที่ mod มองเห็น — คนละคลาสกับ ServerPlayer จริงในเซิร์ฟ (ตั้งใจ กัน mod
/// อ้างอิงลึกเกินไปจนพังตอนเซิร์ฟแก้โครงสร้างภายใน)</summary>
public interface IModPlayer
{
    string EntityId { get; }
    string Name { get; }
    int Level { get; }
    bool IsDead { get; }
    int TileX { get; }
    int TileY { get; }

    /// <summary>ส่งข้อความ (Info popup) ให้ผู้เล่นคนนี้คนเดียว</summary>
    void SendMessage(string text);

    /// <summary>วาร์ปผู้เล่นไปยัง tile ที่ระบุ</summary>
    void Teleport(int tileX, int tileY);

    /// <summary>[V1.1] นับของในกระเป๋าติดตัวที่ prototype ตรงกัน (id ตามข้อมูลเกม เช่น "stone",
    /// "blade_stone" — case-sensitive) กล่องที่วางบนพื้นไม่นับ เฉพาะของในตัว</summary>
    int CountItem(string prototypeId);

    /// <summary>[V1.1] สรุปของถือติดตัวทั้งหมด key = prototype id, value = จำนวนชิ้น</summary>
    IReadOnlyDictionary<string, int> GetInventorySummary();

    /// <summary>[V1.1] เพิ่มของเข้ากระเป๋าเหมือน "เก็บได้เอง" (durability/tag/performance ผ่านข้อมูล
    /// เกมครบ + sync client ทันที) name/icon โชว์เป็น prototype-id; false ถ้า count &lt;= 0</summary>
    bool GiveItem(string prototypeId, int count = 1);
}
