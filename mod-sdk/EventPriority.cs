namespace DurangoServer.Modding;

/// <summary>ลำดับ handler ของ event ใหม่ — priority สูงทำก่อน; ชื่อ mod/ลำดับ register ใช้ตัดสินกรณีเสมอ</summary>
public enum EventPriority
{
    Lowest = -200,
    Low = -100,
    Normal = 0,
    High = 100,
    Highest = 200,
    Monitor = 1000
}
