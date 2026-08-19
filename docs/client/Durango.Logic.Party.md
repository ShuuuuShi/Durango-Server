# namespace `Durango.Logic.Party`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

1 ไฟล์

## `Durango.Logic.Party/Member.cs`

145 บรรทัด

**class `Member`** — บรรทัด 10–144

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `public string EntityId { get; private set; }` | public |
| 22 | `public string RegionId { get; private set; }` | public |
| 24 | `public bool IsLeader { get; private set; }` | public |
| 26 | `public bool IsAccepted { get; private set; }` | public |
| 71 | `public Point2 Tile { get; private set; }` | public |
| 74 | `public Durango.Player.PlayerInfo PlayerInfo { get; private set; }` | public |
| 76 | `public string RegionName { get; private set; }` | public |
| 80 | `public Member(string entityId, bool isLeader, bool isAccepted)` | public |
| 100 | `public void SetPlayer(PlayerBehavior player)` | public |
| 105 | `public void SetStatus(PartierStatus status)` | public |
| 135 | `private static float GetGaugeRatio(Gauge gauge)` |  |

---
