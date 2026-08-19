# namespace `Durango.System`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

5 ไฟล์

## `Durango.System/NPCountry.cs`

256 บรรทัด

**enum `NPCountry`** — บรรทัด 3

---

## `Durango.System/Platform.cs`

232 บรรทัด

**class `Platform`** — บรรทัด 9–231

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 20 | `public static Platform Instance { get; private set; }` | public |
| 84 | `public virtual string CountryLetterCode => Country switch` | public |
| 91 | `public virtual NPCountry Country => Application.systemLanguage switch` | public |
| 124 | `static Platform()` |  |
| 129 | `public virtual void Login(Action onSuccess, Action<int> onFailure)` | public |
| 134 | `public virtual void Logout(Action<bool> onResult)` | public |
| 139 | `public virtual void Leave(Action<bool> onResult)` | public |
| 144 | `public virtual Dictionary<string, string> BuildSessionForm()` | public |
| 162 | `public virtual void ShowWeb(string title, [NotNull] string url)` | public |
| 167 | `public virtual void ShowNotice()` | public |
| 172 | `public virtual void ShowPlate()` | public |
| 176 | `public virtual void ShowCustomerServiece()` | public |
| 180 | `public virtual void ShowAccountMenu()` | public |
| 184 | `public virtual void SetLocale(string locale)` | public |
| 188 | `public virtual void ShowOfferwall()` | public |
| 192 | `public virtual void RequestPermission(string permission, Action<bool> callback)` | public |
| 197 | `public virtual bool GetScreenResolution(bool isPortrait, out int width, out int height)` | public |
| 227 | `public virtual void Quit()` | public |

   **enum `StoreType`** — บรรทัด 11

---

## `Durango.System/PlatformResources.cs`

18 บรรทัด

**class `PlatformResources`** — บรรทัด 6–17

---

## `Durango.System/Platform_Android.cs`

21 บรรทัด

**class `Platform_Android`** — บรรทัด 5–20

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 7 | `public override void RequestPermission(string permission, Action<bool> callback)` | public |

---

## `Durango.System/Platform_PC.cs`

55 บรรทัด

**class `Platform_PC`** — บรรทัด 6–54

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 34 | `public override bool GetScreenResolution(bool isPortrait, out int width, out int height)` | public |
| 42 | `public static Point2 GetScreenResolution()` | public |

---
