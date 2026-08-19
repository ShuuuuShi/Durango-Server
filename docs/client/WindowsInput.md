# namespace `WindowsInput`

> auto-generated จากซอร์สจริง — ลายเซ็นและเลขบรรทัดตรงกับไฟล์ เปิดตามได้ทันที

1 ไฟล์

## `WindowsInput/WinInput.cs`

365 บรรทัด

**class `WinInput`** — บรรทัด 7–364

| บรรทัด | สมาชิก | หมายเหตุ |
|---:|---|---|
| 13 | `static WinInput()` |  |
| 25 | `protected static extern short GetAsyncKeyState(int keyCode);` |  |
| 27 | `private static int KeyCodeToVkeyFullSet(KeyCode key)` |  |
| 282 | `private static int KeyCodeToVkey(KeyCode key)` |  |
| 319 | `public static bool GetKey(KeyCode key)` | public |
| 329 | `public static bool GetKeyDown(KeyCode key)` | public |
| 342 | `public static bool GetKeyUp(KeyCode key)` | public |
| 355 | `public static bool GetKeyFullCover(KeyCode key)` | public |

---
