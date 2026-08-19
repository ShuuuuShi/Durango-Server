# `ServerCore/ServerCommon.cs`

ของจิปาถะ 2 อย่างที่ไม่มีบ้าน รวมไว้ที่เดียว

## `DictExtensions.Get(dict, key)` — บรรทัด 36
```csharp
return dict != null && dict.TryGetValue(key, out string value) ? value : null;
```
extension method อ่าน dict แบบไม่โยน exception — ใช้เยอะใน `Gateway` ตอนอ่าน `postData.Get("player")`
(กันทั้ง dict เป็น null และ key ไม่มี)

## `ServerKnock.HostName` — บรรทัด 44
```csharp
public static volatile string HostName = "DurangoServer";
```
ชื่อที่ตอบกลับไปตอนมีใครยิง **UDP knock** มาถาม (ใช้ค้นหาเกาะใน LAN)
อ่านที่ `GameCode/Durango.Offline/WebServer.cs:220`

`volatile` เพราะถูกอ่านจาก **thread pool** (callback ของ `UdpClient.BeginReceive`) แต่เขียนจาก main thread

⚠️ ถูกเขียน 2 ที่และตัวหลังทับตัวแรก:
- `Program.cs:65` → `serverName` (`"Multi Play Server"`) ✅ ถูกต้อง
- `GameServer.cs` ใน `Ready` handler → `playerName` ❌ ทับด้วยชื่อคนที่เข้าล่าสุด (GP-11)
