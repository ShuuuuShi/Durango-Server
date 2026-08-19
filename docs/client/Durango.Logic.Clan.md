# namespace `Durango.Logic.Clan`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

5 ไฟล์

## `Durango.Logic.Clan/Clan.cs`

180 บรรทัด

**class `Clan`** — บรรทัด 9–179

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 43 | `private readonly Dictionary<int, MemberRole> _roleInfos = new Dictionary<int, MemberRole>();` |  |
| 50 | `public int MemberCount => (Members != null) ? Members.Count : _memberCount;` | public |
| 52 | `public static Permissions[] Permissions => Enums<Shared.Clan.Permissions>.Greater(Shared.Clan.Permissions.None);` | public |
| 54 | `public void Set(ClanJson json, bool isDetail)` | public |
| 146 | `public Member GetMember(string entityId)` | public |
| 158 | `public Member GetApplier(string entityId)` | public |
| 170 | `public bool TryGetRole(int id, out MemberRole role)` | public |

---

## `Durango.Logic.Clan/ClanJson.cs`

37 บรรทัด

**struct `ClanJson`** — บรรทัด 5–36

---

## `Durango.Logic.Clan/ClanJsonContainer.cs`

7 บรรทัด

**struct `ClanJsonContainer`** — บรรทัด 3–6

---

## `Durango.Logic.Clan/Member.cs`

43 บรรทัด

**class `Member`** — บรรทัด 3–42

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `public Member(Pair<string, int> pair)` | public |
| 18 | `public Member(string applier)` | public |
| 25 | `public override bool Equals(object obj)` | public |
| 38 | `public override int GetHashCode()` | public |

---

## `Durango.Logic.Clan/RoleInfo.cs`

17 บรรทัด

**struct `RoleInfo`** — บรรทัด 5–16

---
