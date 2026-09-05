# Installation Guide (English)

[ไทย](INSTALL.th.md) · [中文](INSTALL.zh.md)

This guide covers setting up the Durango private server from scratch to having players connect.

---

## 1. Requirements

| Requirement | Version | Used for |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download/dotnet/9.0) | **9.0** | Building and running the server and test client (Windows / Linux / macOS) |
| [Git](https://git-scm.com/) | latest | Cloning the project |
| RAM / disk | ~512 MB / ~200 MB | The server itself only uses ~30–70 MB of RAM at runtime |

Verify your installation:

```bash
dotnet --version    # must start with 9.
git --version
```

## 2. Get the project

```bash
git clone https://github.com/ShuuuuShi/Durango-Server.git
cd Durango-Server
```

## 3. Prepare terrain data (from your own game copy)

The repository does **not** include the game's map data because it is NEXON's copyrighted material — prepare it from your own copy of the game:

```text
server/data/terrains/extracted/<island-id>/    ← put each island's terrain data here
server/data/gamefiles/                          ← (optional, for launcher patching) the PC game files
```

13 islands are supported, e.g. `pe10gr_1`–`pe10gr_5`, `ri35te`, `ri35de`, `ri40tr`, etc. — see `server/data/islands.json` for the full list.

> Without `terrains/extracted/` the server still starts, but players cannot enter islands that have no terrain data.

## 4. Run the server

```bash
cd server
dotnet run -- --whitelist data/whitelist.txt
```

Defaults (all safe):

| Setting | Default |
|---|---|
| Game port (TCP) | **8191** |
| Gateway (HTTP — account registration, web pages) | **8190** |
| Radiotower (TCP) | 8192 = game port + 1 (enable with `--radiotower`) |
| Cheat packets | Disabled — enable with `--enable-cheat` |
| Connection cap | 32 connections (max 4 per IP) |
| Auto-save | Every 60 seconds |

Other useful flags: `--game-port <port>`, `--max-connections-per-ip <n>`, `--admin <name>` (see `server/Program.cs` for all).

**Whitelist** — always recommended: `server/data/whitelist.txt`, one entity id or character name per line (`#` = comment). The file hot-reloads — **no server restart needed**.

**All gameplay config** lives in `server/data/config.json` — toggle Farming, Quests, Market, PvP, Android support, etc. It hot-reloads too.

A healthy server prints a stats line every 30 seconds:

```text
[loop] 120 tps, ผู้เล่นออนไลน์ 3, สัตว์ 34 ตัว (+ซาก 2), RAM 32 MB
```

(120 tps, online players, animals, RAM.)

## 5. Test with the test client

Open a second terminal:

```bash
cd test-client
dotnet run -- --gp-check        # gameplay suite — expect 36/36
dotnet run -- --multi-check     # multi-player — expect 9/9
dotnet run -- --estate-check 127.0.0.1 8191 8190   # estate/land system — requires --enable-cheat on the server
```

## 6. Mods

**Server-side** (`mod-sdk/` — .NET 9): create a project referencing `mod-sdk/DurangoModSdk.csproj`, implement `IGamePlugin`, drop the built dll into `server/data/mods/`. See the `mod-sdk/` sources for the plugin protocol.

**Game-side** (`client-mod-sdk/` — net35 / Unity Mono): needs `UnityEngine.CoreModule.dll` from your own game installation (point the HintPath in `DurangoClientModSdk.csproj` at your game's `Durango_Data/Managed/` folder).

**Complete example mod:** `tools/MemoryBotMod/` — an autonomous bot that gathers, crafts and does quests. See `tools/MemoryBotMod/HOW-TO-DRIVE.md`.

## 7. Troubleshooting

| Symptom | Cause / fix |
|---|---|
| Players can't connect | Check firewall for TCP 8191 and HTTP 8190 |
| Stuck loading / empty map on an island | Missing `data/terrains/extracted/<island-id>` — back to step 3 |
| `--gp-check` not 36/36 | Server and test client out of sync — `dotnet build` both folders |
| Repeated `[error]` lines in the log | The server won't crash (exceptions are caught), but read which system it is and open an issue |

## 8. Contributing

- Server code lives in `server/ServerCore/` — start at `Program.cs` and `ServerCore/ServerPlayer.Core.cs` (player core + 32 packet handlers).
- Pull requests welcome — if you touch gameplay, make `--gp-check` pass first.
