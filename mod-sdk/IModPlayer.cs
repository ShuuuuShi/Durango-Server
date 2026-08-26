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
}
