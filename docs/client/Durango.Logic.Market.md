# namespace `Durango.Logic.Market`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

6 ไฟล์

## `Durango.Logic.Market/Category.cs`

45 บรรทัด

**class `Category`** — บรรทัด 3–44

   **class `Sub`** — บรรทัด 5–17

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 9 | `public string Id { get; private set; }` | public |
   | 11 | `public string Name => (_name != null) ? _name : (_name = LocalizeSystem.Get($"#prototype_sub_category_{Id}"));` | public |
   | 13 | `public Sub(string id)` | public |

   **class `Main`** — บรรทัด 19–39

   | บรรทัด | สมาชิก | หมายเหตุ |
   |---:|---|---|
   | 27 | `public string Id { get; private set; }` | public |
   | 29 | `private string Key => (_key != null) ? _key : (_key = $"#prototype_category_{Id}");` |  |
   | 31 | `public string Name => (_name != null) ? _name : (_name = LocalizeSystem.Get(Key));` | public |
   | 33 | `public string Icon => (_icon != null) ? _icon : (_icon = IconMap.Get(Key));` | public |
   | 35 | `public Main(string id)` | public |

---

## `Durango.Logic.Market/Commodities.cs`

209 บรรทัด

**class `Commodities`** — บรรทัด 9–208

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 11 | `public readonly RequestOption Request = new RequestOption();` | public |
| 13 | `public readonly List<Commodity> Goods = new List<Commodity>();` | public |
| 22 | `public void Reset()` | public |
| 30 | `public void Get(bool reset)` | public |
| 58 | `public ReplyMessageHandlerRegistrar CreateGetProductMessage(ProductType type, SortCondition condition, int pageIndex)` | public |
| 104 | `private void OnResult(Products products)` |  |
| 151 | `public void Buy(Commodity item)` | public |
| 168 | `public void Unregister(Commodity item)` | public |
| 185 | `public void Withdraw(Commodity item)` | public |

---

## `Durango.Logic.Market/Commodity.cs`

94 บรรทัด

**class `Commodity`** — บรรทัด 10–93

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 36 | `public void Set(Product msg)` | public |
| 66 | `public ItemData GetItem()` | public |
| 71 | `public bool IsWaiting()` | public |
| 84 | `public void SetToWait()` | public |
| 89 | `public void Responsed()` | public |

---

## `Durango.Logic.Market/ProductType.cs`

12 บรรทัด

**enum `ProductType`** — บรรทัด 3

---

## `Durango.Logic.Market/RequestOption.cs`

17 บรรทัด

**class `RequestOption`** — บรรทัด 5–16

---

## `Durango.Logic.Market/SearchOption.cs`

92 บรรทัด

**class `SearchOption`** — บรรทัด 9–91

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 23 | `public HashSet<TagFilterBase> Tags = new HashSet<TagFilterBase>(new TagFilterComparer());` | public |
| 25 | `public HashSet<TagFilterBase> Materials = new HashSet<TagFilterBase>(new TagFilterComparer());` | public |
| 29 | `public bool IsResultSearch => MainCategory != null && !string.IsNullOrEmpty(SearchKeyword);` | public |
| 31 | `public bool Filter(string name)` | public |
| 40 | `public void Clear()` | public |
| 55 | `public void ClearExceptCategory()` | public |
| 64 | `public string[][] GetNestedTag()` | public |
| 79 | `public SearchProducts ToMessage()` | public |

---
