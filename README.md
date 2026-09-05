# Durango Private Server

**ภาษา / Language:** [ไทย](docs/install/INSTALL.th.md) · [English](docs/install/INSTALL.en.md) · [中文](docs/install/INSTALL.zh.md)

---

## ไทย (ย่อ)

โปรเจกต์นี้คือเซิร์ฟเวอร์ส่วนตัวสำหรับเกม **Durango: Wild Lands** ที่เขียนใหม่ทั้งหมดด้วย C# (.NET 9) พร้อมโปรโตคอลที่ implement เอง, ตัว test client สำหรับรันชุดทดสอบอัตโนมัติ และระบบม็อดทั้งฝั่งเซิร์ฟเวอร์และฝั่งเกม

**โครงสร้างของ repo:**

| โฟลเดอร์ | คืออะไร |
|---|---|
| `server/` | ตัวเซิร์ฟเวอร์ (เกม TCP 8191 · Gateway HTTP 8190 · Radiotower 8192) พร้อม `data/` config |
| `test-client/` | ตัวเกมเทส — คอนโซล client ที่รันชุดทดสอบอัตโนมัติ (`--gp-check`, `--estate-check` ฯลฯ) |
| `mod-sdk/` | SDK สำหรับเขียนม็อดฝั่งเซิร์ฟเวอร์ (.NET 9) |
| `client-mod-sdk/` | SDK สำหรับเขียนม็อดฝั่งเกม (net35 / Unity Mono) |
| `tools/MemoryBotMod/` | ม็อดตัวอย่าง — บอทที่เล่นเกมเองได้ |
| `docs/install/` | คู่มือติดตั้งฉบับเต็ม 3 ภาษา |

**เริ่มเร็ว:**

```bash
git clone https://github.com/ShuuuuShi/Durango-Server.git
cd Durango-Server/server
dotnet run -- --whitelist data/whitelist.txt
```

อ่านคู่มือติดตั้งฉบับเต็ม: **[docs/install/INSTALL.th.md](docs/install/INSTALL.th.md)**

---

## English (summary)

A from-scratch private server for **Durango: Wild Lands**, written in C# (.NET 9), with a self-implemented protocol, a console test client that runs automated test suites, and a modding system for both the server and the game client.

| Folder | What it is |
|---|---|
| `server/` | The server (game TCP 8191 · Gateway HTTP 8190 · Radiotower 8192) with `data/` config |
| `test-client/` | Test game — console client running automated checks (`--gp-check`, `--estate-check`, …) |
| `mod-sdk/` | Server-side mod SDK (.NET 9) |
| `client-mod-sdk/` | Game-side mod SDK (net35 / Unity Mono) |
| `tools/MemoryBotMod/` | Example mod — an autonomous gameplay bot |
| `docs/install/` | Full installation guides in 3 languages |

**Quick start:**

```bash
git clone https://github.com/ShuuuuShi/Durango-Server.git
cd Durango-Server/server
dotnet run -- --whitelist data/whitelist.txt
```

Full guide: **[docs/install/INSTALL.en.md](docs/install/INSTALL.en.md)**

---

## Disclaimer / คำเตือน

> **ไทย:** โปรเจกต์นี้เป็นงานของแฟนเกม ทำขึ้นเพื่อการศึกษาและใช้งานส่วนตัว *Durango: Wild Lands* เป็นทรัพย์สินทางปัญญาของ NEXON — โปรเจกต์นี้ไม่มีส่วนเกี่ยวข้องกับ NEXON และไม่มีการแจกจ่ายไฟล์ของเกมต้นฉบับ (ผู้ใช้ต้องเตรียมข้อมูลเทอร์เรนจากเกมของตัวเอง ดูในคู่มือติดตั้ง)
>
> **English:** This is a fan-made project for educational and personal use. *Durango: Wild Lands* is the intellectual property of NEXON. This project is not affiliated with NEXON and does not distribute original game files (you must prepare terrain data from your own copy of the game — see the installation guide).

---

## สนับสนุนโปรเจกต์ / Support & Donate

**ไทย:** ถ้าอยากสนับสนุนค่าเซิร์ฟเวอร์/การพัฒนาต่อ หรือติดต่อสอบถามเรื่องโดเนท โดเนทรายเดือน หรือเปิดเซิร์ฟเวอร์ของตัวเอง ติดต่อได้ที่อีเมล: **[supercodeth@gmail.com](mailto:supercodeth@gmail.com)**

**English:** If you'd like to support server costs and ongoing development, or have questions about donations, monthly support, or running your own server, contact: **[supercodeth@gmail.com](mailto:supercodeth@gmail.com)**

---

## License

Newly-written code is licensed under **[GPL-3.0](LICENSE)**. Game-derived content is **not** covered by this license — see [NOTICE.md](NOTICE.md) for details. · โค้ดที่เขียนขึ้นใหม่อยู่ภายใต้ [GPL-3.0](LICENSE) ส่วนที่มาจากตัวเกมอยู่นอก license — รายละเอียดที่ [NOTICE.md](NOTICE.md)
