# namespace `Durango.Player`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

8 ไฟล์

## `Durango.Player/FoundPlayerInfo.cs`

16 บรรทัด

**struct `FoundPlayerInfo`** — บรรทัด 5–15

---

## `Durango.Player/FoundPlayersJson.cs`

10 บรรทัด

**struct `FoundPlayersJson`** — บรรทัด 5–9

---

## `Durango.Player/PlayerClanInfoJson.cs`

13 บรรทัด

**struct `PlayerClanInfoJson`** — บรรทัด 5–12

---

## `Durango.Player/PlayerConnected.cs`

42 บรรทัด

**struct `PlayerConnected`** — บรรทัด 8–41

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 16 | `public string GetConnectedString()` | public |
| 32 | `public int CompareTo(PlayerConnected other)` | public |

---

## `Durango.Player/PlayerCostumeTable.cs`

148 บรรทัด

**class `PlayerCostumeTable`** — บรรทัด 10–147

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 95 | `public PreviewableDatum GetRandom(Category type, bool isMale)` | public |
| 110 | `public List<PreviewableDatum> GetDataArray(Category type, bool dataIsMale)` | public |
| 127 | `public string GetPlayerDefaultBodyModelAssetBundlePath(bool isMale, int dataJob, ClothState clothState)` | public |

   **enum `Category`** — บรรทัด 12

   **enum `ClothState`** — บรรทัด 20

   **class `PreviewableDatum`** — บรรทัด 28–41

   **class `SimpleDatum`** — บรรทัด 44–54

---

## `Durango.Player/PlayerEquipment.cs`

83 บรรทัด

**class `PlayerEquipment`** — บรรทัด 3–82

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 22 | `public string GetCurrentPath()` | public |
| 27 | `public ItemColor GetCurrentColor()` | public |
| 32 | `public bool IsMotionEquipped()` | public |
| 37 | `public void SetMotionEquipImmediately(string path, ItemColor color = default(ItemColor))` | public |
| 44 | `public void ReserveMotionEquipment(string path, ItemColor color = default(ItemColor))` | public |
| 55 | `public void ResetMotionEquipment()` | public |
| 61 | `public void ChangePath(string path)` | public |
| 66 | `public void ChangeColor(ItemColor color)` | public |
| 71 | `public void AnimMotionChanged()` | public |

   **enum `State`** — บรรทัด 5

---

## `Durango.Player/PlayerInfo.cs`

98 บรรทัด

**class `PlayerInfo`** — บรรทัด 7–97

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 37 | `public bool HasClan => !string.IsNullOrEmpty(ClanId);` | public |
| 39 | `public string RegionName => (Region == null) ? string.Empty : Region.Name;` | public |
| 41 | `public string ReturningRegionName => (ReturningRegion == null) ? string.Empty : ReturningRegion.Name;` | public |
| 43 | `public bool IsMale => Display.DefaultBody == null \|\| !Display.DefaultBody.Contains("Female");` | public |
| 45 | `public void Set(PlayerInfoJson json)` | public |
| 68 | `public PortraitBuilder.Argument GetPortraitArgument()` | public |
| 83 | `public string GetFreq(int? freqSize = null)` | public |
| 88 | `public static string ToFreq(int freq, int? freqSize = null)` | public |
| 93 | `public string GetNameFreq(int freqSize = 21, string freqHexCode = "")` | public |

---

## `Durango.Player/PlayerInfoJson.cs`

39 บรรทัด

**struct `PlayerInfoJson`** — บรรทัด 7–38

---
