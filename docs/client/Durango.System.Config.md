# namespace `Durango.System.Config`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

10 ไฟล์

## `Durango.System.Config/ConfigInstance.cs`

954 บรรทัด
- **ส่ง packet:** `AcceptTENCoupon`, `DeregisterUser`, `RequestDumpedPersonalIsland`

**class `ConfigInstance`** — บรรทัด 25–953

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 33 | `public static void Initialize()` | public |
| 47 | `private static void LoadFromJson()` |  |
| 126 | `private static void LoadConfigValue()` |  |
| 199 | `private static string GetSaveKey(string key)` |  |
| 204 | `public static void UpdateValue(string key, string resolution)` | public |
| 210 | `public static string ChangeValue(string key, string value, bool save = true)` | public |
| 256 | `public static void MuteAll()` | public |
| 264 | `public static void UnMuteAll()` | public |
| 272 | `public static float ChangeValue(string key, float value, bool save = true)` | public |
| 300 | `public static bool ChangeValue(string key, bool value, bool save = true)` | public |
| 357 | `private static void ChangePostProcessing(string key, bool value)` |  |
| 383 | `private static void ChangeVignettingEffect(bool value)` |  |
| 388 | `private static void ChangeVerticalSync(bool value)` |  |
| 393 | `public static void UpdatePostProcessingSettings()` | public |
| 408 | `private static void SaveValue(string key, string value)` |  |
| 414 | `private static void SaveValue(string key, float value)` |  |
| 420 | `private static void SaveValue(string key, bool value)` |  |
| 426 | `public static void RefreshValue(string key)` | public |
| 467 | `private static void SetValue<TV>(string key, TV value)` |  |
| 488 | `private static ValueSetting GetValue(string key)` |  |
| 505 | `public static TV GetValue<TV>(string key, TV defaultValue = default(TV))` | public |
| 526 | `public static void NotifyAction(string key, ValueSetting op = null)` | public |
| 620 | `private static void SendCoupon(string coupon)` |  |
| 635 | `private static string ChangeResolution(string value)` |  |
| 649 | `private static string ChangeResolution_PC(string value)` |  |
| 655 | `private static string ChangeScreenMode(string value)` |  |
| 662 | `private static string ChangeAntiAliasing(string value)` |  |
| 669 | `private static void ChangeUISize(string value)` |  |
| 677 | `private static void ChangeFps(string value)` |  |
| 683 | `private static string ChangeLocale(string value)` |  |
| 738 | `private static string ChangeVoiceLocale(string value)` |  |
| 765 | `private static IEnumerator NoRestartChangeLocalize()` | coroutine |
| 777 | `private static void ChangeOrientation(string value, bool update)` |  |
| 787 | `private static void ChangeShadowOption(string value)` |  |
| 802 | `private static void ChangeMouseReversed(bool value)` |  |
| 807 | `private static void ChangeSfxVolume(float val)` |  |
| 812 | `private static void ChangeAmbienceVolume(float val)` |  |
| 817 | `private static void ChangeMidiVolume(float val)` |  |
| 822 | `private static void ChangeBgmVolume(float val)` |  |
| 831 | `private static void ChangeMaxFrameRate(float val)` |  |
| 836 | `private static void ConnectSnsAccount()` |  |
| 848 | `private static void DeleteAccount()` |  |
| 872 | `public static void Logout()` | public |
| 884 | `private static void OnLogout(bool success)` |  |
| 898 | `public static string GetPresetValue(PresetValue value)` | public |
| 910 | `public static void OpenOfficialCommunityUrl()` | public |
| 915 | `private static void OpenUrl(string key, ValueSetting op)` |  |
| 939 | `private static void ShowTerms()` |  |

---

## `Durango.System.Config/DropdownSetting.cs`

18 บรรทัด

**class `DropdownSetting`** — บรรทัด 5–17

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `public bool Contains(string value)` | public |

---

## `Durango.System.Config/GridSetting.cs`

7 บรรทัด

**class `GridSetting`** — บรรทัด 3–6

---

## `Durango.System.Config/LabelSetting.cs`

7 บรรทัด

**class `LabelSetting`** — บรรทัด 3–6

---

## `Durango.System.Config/PresetValue.cs`

10 บรรทัด

**enum `PresetValue`** — บรรทัด 3

---

## `Durango.System.Config/Setting.cs`

60 บรรทัด

**class `Setting`** — บรรทัด 7–59

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 27 | `public static bool IsHidden(Setting op)` | public |

---

## `Durango.System.Config/SettingType.cs`

22 บรรทัด

**enum `SettingType`** — บรรทัด 3

---

## `Durango.System.Config/SliderSetting.cs`

11 บรรทัด

**class `SliderSetting`** — บรรทัด 3–10

---

## `Durango.System.Config/ToggleSetting.cs`

14 บรรทัด

**class `ToggleSetting`** — บรรทัด 5–13

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 9 | `public bool Contains(string value)` | public |

---

## `Durango.System.Config/ValueSetting.cs`

15 บรรทัด

**class `ValueSetting`** — บรรทัด 3–14

---
