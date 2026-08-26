namespace Durango.Modding
{
    /// <summary>ทุก client mod ต้องมีคลาสเดียวที่ implement อันนี้ — คู่กับ IGamePlugin ฝั่งเซิร์ฟ
    /// (mod-sdk/) แต่คนละไฟล์กันเพราะ target framework คนละตัว (net35 vs net9.0)
    ///
    /// [แก้เอง] 24 ส.ค. 2026 — แยก 3 เฟสเหมือนฝั่งเซิร์ฟ (ดู comment เต็มที่ mod-sdk/IGamePlugin.cs):
    /// ClientModLoader เรียกครบทุก mod ทีละเฟส ไม่ใช่ mod ละ 3 เฟสรวดแล้วค่อยไป mod ถัดไป</summary>
    public interface IClientPlugin
    {
        string Name { get; }
        string Version { get; }

        /// <summary>เฟส 1/3 — ก่อนใครจะเข้าเฟส Load ห้ามพึ่งพาว่า mod อื่นลงทะเบียนอะไรไว้แล้ว</summary>
        void OnPreLoad(IClientModApi api);

        /// <summary>เฟส 2/3 — งานหลัก (ผูกปุ่มลัด/OnGameReady ฯลฯ) มาลงที่นี่</summary>
        void OnLoad(IClientModApi api);

        /// <summary>เฟส 3/3 — เรียกหลังทุก mod ผ่านเฟส Load หมดแล้ว</summary>
        void OnPostLoad(IClientModApi api);
    }
}
