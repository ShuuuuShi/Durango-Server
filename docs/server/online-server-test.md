# Dinoworld Server local route

The original MainUI `online` entry is the supported local test route. It is displayed as
`Dinoworld Server` in dark pink (`#C2185B`). The key remains `online` so the route and
server persistence logic are unchanged.

The active `game/DurangoV2_Data/Managed/Assembly-CSharp.dll` is rebuilt from the original
client DLL and patched only at `Durango.Offline.Server/<>c__DisplayClass21_0::<.ctor>b__2`:
after the embedded island is initialized, the callback calls
`ConnectTo(Preferences["last_connect_ip"] ?? "127.0.0.1")` for the `online` key.
This preserves the original MainUI/background and avoids the friend-island/direct-IP flow.

## Verification

- Server gateway: `127.0.0.1:8190`.
- Server game TCP: `127.0.0.1:8191`.
- The active client DLL must contain the `key == "online"` branch and `ConnectTo` call.
- After selecting the menu entry, the server log must contain `[world] player joined`.
- Do not set `DURANGO_AUTOCONNECT` during this test; that bypasses MainUI selection.

## Troubleshooting record: menu click appears frozen

Observed symptom: clicking `Online Server (For Test)` left the game apparently idle.

Root cause: the IL patch called `Durango.Offline.Server.ConnectTo` with
`callvirt` and pushed a server instance, but this method is `static` in the client DLL.
Unity then raised `InvalidProgramException` at the callback return instruction. The
server could receive a partial connection, but the client could not complete the UI flow.

Correct patch pattern:

1. Compare the captured method signature before emitting IL; confirm whether the target
   method is static or instance.
2. For this client, use `call`, not `callvirt`, and pass only the IP string.
3. Read `Preferences.GetString("last_connect_ip", "127.0.0.1")` and explicitly fall back
   to `127.0.0.1` when it is empty.
4. Rebuild `DllPatcher`, patch from `Assembly-CSharp.dll.bak`, replace the active DLL,
   restart the game, and check for `InvalidProgramException` in the Unity log.

Verified after the fix: `/sessions` and `/entry` returned 200, the client connected,
`[world] player joined` appeared, and the player remained connected.

## Persistence after restart

The Online entry must not use the embedded `OnRequestAccount` callback for its character
list. That callback only knows the current process memory and causes a new GUID to appear
after reopening the game. The patch therefore sets the Online cluster gateway to
`http://127.0.0.1:8190`, clears its embedded account callback, and lets
`Clusters.GetOrRequestAccounts` call the server `/accounts` endpoint.

Verification: `/accounts` returned the existing characters `dfsdf`, `พักด`, and `asas`
from the server save directory after the client was closed and reopened.

## Mobile UI mode

The Windows client is forced to use the mobile UI prefab/layout set. The active DLL patches
`Durango.System.Platform_PC.get_UsePCUI` to return `false`, so the setting does not depend
on PlayerPrefs or an environment variable. Repatch from `Assembly-CSharp.dll.bak` after
replacing the client DLL.
