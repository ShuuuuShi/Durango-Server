# namespace `Durango.Logic.Statistics`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

2 ไฟล์

## `Durango.Logic.Statistics/FavoriteTitles.cs`

50 บรรทัด

**class `FavoriteTitles`** — บรรทัด 8–49

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 12 | `private static HashSet<string> _favoriteTitles = new HashSet<string>();` |  |
| 16 | `public void Save()` | public |
| 27 | `public void Load(Dictionary<string, byte[]> storage)` | public |
| 37 | `public bool IsFavorite(string targetId)` | public |
| 42 | `public void Toggle(string targetId)` | public |

---

## `Durango.Logic.Statistics/Title.cs`

76 บรรทัด

**class `Title`** — บรรทัด 9–75

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public bool Enabled { get; set; }` | public |
| 15 | `public string Id { get; private set; }` | public |
| 17 | `public bool IsNew { get; set; }` | public |
| 23 | `public Title(string key, Yaml.Title title)` | public |
| 29 | `public Dictionary<Basic, int> GetAbilities()` | public |
| 34 | `public int GetAbility(Basic key)` | public |
| 39 | `public Dictionary<string, float> GetModifiers()` | public |
| 44 | `public List<string> GetAbilityModifiersText()` | public |

---
