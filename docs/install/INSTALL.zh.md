# 安装指南（中文）

[ไทย](INSTALL.th.md) · [English](INSTALL.en.md)

本指南介绍如何从零开始安装并运行 Durango 私人服务器，直到玩家可以进入游戏。

---

## 1. 环境要求

| 要求 | 版本 | 用途 |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download/dotnet/9.0) | **9.0** | 构建并运行服务器与测试客户端（支持 Windows / Linux / macOS） |
| [Git](https://git-scm.com/) | 最新版 | 克隆项目 |
| 内存 / 磁盘 | ~512 MB / ~200 MB | 服务器运行时仅占用约 30–70 MB 内存 |

检查安装：

```bash
dotnet --version    # 必须以 9. 开头
git --version
```

## 2. 获取项目

```bash
git clone https://github.com/ShuuuuShi/Durango-Server.git
cd Durango-Server
```

## 3. 准备地形数据（需使用你自己的游戏副本）

仓库**不包含**游戏的地图数据（属于 NEXON 版权内容）——请从你自己的游戏中准备：

```text
server/data/terrains/extracted/<island-id>/    ← 将每个岛屿的地形数据放这里
server/data/gamefiles/                          ← （可选，供启动器下载补丁）PC 版游戏文件
```

支持 13 个岛屿，例如 `pe10gr_1`–`pe10gr_5`、`ri35te`、`ri35de`、`ri40tr` 等 —— 完整列表见 `server/data/islands.json`。

> 没有 `terrains/extracted/` 时服务器仍可启动，但玩家无法进入缺少地形数据的岛屿。

## 4. 启动服务器

```bash
cd server
dotnet run -- --whitelist data/whitelist.txt
```

默认配置（均为安全值）：

| 配置项 | 默认值 |
|---|---|
| 游戏端口 (TCP) | **8191** |
| Gateway (HTTP — 注册账号/网页) | **8190** |
| Radiotower (TCP) | 8192 = 游戏端口 + 1（用 `--radiotower` 开启） |
| 作弊指令 (Cheat packet) | 关闭 —— 用 `--enable-cheat` 开启 |
| 连接上限 | 32 条连接（每个 IP 最多 4 条） |
| 自动存档 | 每 60 秒一次 |

其他常用参数：`--game-port <端口>`、`--max-connections-per-ip <n>`、`--admin <名字>`（完整列表见 `server/Program.cs`）。

**白名单** —— 建议始终开启：`server/data/whitelist.txt`，每行一个 entity id 或角色名（`#` 为注释）。文件会热重载，**无需重启服务器**。

**所有玩法配置**都在 `server/data/config.json` —— 可开关 Farming、Quests、Market、PvP、Android 支持等，同样支持热重载。

服务器正常时每 30 秒输出一行状态：

```text
[loop] 120 tps, ผู้เล่นออนไลน์ 3, สัตว์ 34 ตัว (+ซาก 2), RAM 32 MB
```

（120 tps、在线人数、动物数量、内存。）

## 5. 使用测试客户端

打开第二个终端：

```bash
cd test-client
dotnet run -- --gp-check        # 玩法测试 —— 应通过 36/36
dotnet run -- --multi-check     # 多人测试 —— 应通过 9/9
dotnet run -- --estate-check 127.0.0.1 8191 8190   # 土地系统 —— 服务器需加 --enable-cheat
```

## 6. 模组（Mod）

**服务器端**（`mod-sdk/` — .NET 9）：新建项目并引用 `mod-sdk/DurangoModSdk.csproj`，实现 `IGamePlugin`，把编译好的 dll 放进 `server/data/mods/`。插件协议详见 `mod-sdk/` 源码。

**游戏端**（`client-mod-sdk/` — net35 / Unity Mono）：需要你自己游戏中的 `UnityEngine.CoreModule.dll`（把 `DurangoClientModSdk.csproj` 中的 HintPath 指向游戏的 `Durango_Data/Managed/` 目录）。

**完整示例模组：** `tools/MemoryBotMod/` —— 可自动采集、制作、做任务的机器人，见 `tools/MemoryBotMod/HOW-TO-DRIVE.md`。

## 7. 常见问题

| 现象 | 原因 / 解决方法 |
|---|---|
| 玩家无法连接 | 检查防火墙放行 TCP 8191 和 HTTP 8190 |
| 进入岛屿卡住 / 地图空白 | 缺少 `data/terrains/extracted/<island-id>` —— 回到第 3 步 |
| `--gp-check` 不足 36/36 | 服务器与测试客户端版本不一致 —— 两个目录分别 `dotnet build` |
| 日志中反复出现 `[error]` | 服务器不会崩溃（有异常捕获），但应查看是哪个系统并提交 issue |

## 8. 参与开发

- 服务器代码在 `server/ServerCore/` —— 从 `Program.cs` 和 `ServerCore/ServerPlayer.Core.cs`（玩家核心 + 32 个包处理器）入手。
- 欢迎 Pull Request —— 修改玩法相关代码前，请先确保 `--gp-check` 全部通过。
