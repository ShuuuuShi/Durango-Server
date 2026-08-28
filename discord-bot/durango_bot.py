import asyncio
import json
import logging
import re
import signal
import sys
import websockets
from pathlib import Path

logging.basicConfig(level=logging.INFO, format="%(asctime)s %(levelname)s %(message)s")
log = logging.getLogger("durango-bot")

BASE = Path(__file__).parent

fh = logging.FileHandler(BASE / "bot.log", encoding="utf-8")
fh.setFormatter(logging.Formatter("%(asctime)s %(levelname)s %(message)s"))
log.addHandler(fh)
CONFIG = json.loads((BASE / "config.json").read_text(encoding="utf-8"))
TOKEN = CONFIG["token"]
CHANNELS_ANSWER = set(CONFIG.get("channels", {}).get("answer", []))
CHANNEL_ANNOUNCE = CONFIG.get("channels", {}).get("announce", "")
BRIEF_PATH = CONFIG.get("brief_path", "")

GATEWAY = "wss://gateway.discord.gg/?v=10&encoding=json"
INTENTS = 1 << 0 | 1 << 9 | 1 << 15  # GUILDS + GUILD_MESSAGES + MESSAGE_CONTENT
HEARTBEAT_ACK_TIMEOUT = 30

BRIEF_CACHE = []


def load_brief():
    global BRIEF_CACHE
    try:
        text = Path(BRIEF_PATH).read_text(encoding="utf-8")
        BRIEF_CACHE = [l.strip() for l in text.splitlines() if l.strip()]
        log.info("brief loaded: %d lines", len(BRIEF_CACHE))
    except Exception as e:
        log.error("brief load failed: %s", e)
        BRIEF_CACHE = []


def brief_section(header_contains, max_lines=12):
    start = None
    for i, line in enumerate(BRIEF_CACHE):
        if any(k in line for k in header_contains):
            start = i
            break
    if start is None:
        return None
    out = []
    for line in BRIEF_CACHE[start + 1:]:
        if line.startswith("##") and out:
            break
        out.append(line)
    return "\n".join(out[:max_lines])


def build_reply(content: str) -> str:
    c = content.lower()

    if any(k in c for k in ["สวัสดี", "hello", "hi", "หวัดดี", "ทัก", "สวัดดี"]):
        return ("สวัสดีครับ 👋 ยินดีต้อนรับ! ถามได้เลย เช่น "
                "\"ตอนนี้ทำอะไรถึงไหนแล้ว?\" \"เหลืออะไรก่อนเปิด beta?\" "
                "\"เล่นอะไรได้บ้างแล้ว?\"")

    if any(k in c for k in ["สถานะ", "ถึงไหน", "ทำอะไร", "ความคืบหน้า", "progress", "how far"]):
        sec = brief_section(["## 2. สถานะตอนนี้"])
        if sec:
            return "📌 สถานะตอนนี้ (ล่าสุด 15 ส.ค. 2026):\n\n" + sec + "\n\nเต็มฉบับอยู่ใน PROJECT-BRIEF.md ครับ"

    if any(k in c for k in ["เหลือ", "ยังไม่ทำ", "ค้าง", "todo", "ถัดไป", "ต่อไป", "ก่อนเปิด", "beta"]):
        sec = brief_section(["## 3. สิ่งที่ยังไม่ทำ"])
        if sec:
            return "🚧 สิ่งที่เหลือ/ค้างอยู่:\n\n" + sec + "\n\nสำคัญสุดตอนนี้: เทสด้วยตัวเกมจริง 30 นาทีรอบสุดท้าย ก่อนเปิด Beta 1.0 ครับ"

    if any(k in c for k in ["เล่นอะไร", "เล่นได้", "ระบบอะไร", "feature", "มีอะไรบ้าง", "ได้แล้ว"]):
        sec = brief_section(["### เล่นได้แล้ว"])
        if sec:
            return "✅ เล่นได้แล้ว:\n\n" + sec + "\n\nรายละเอียดเพิ่มถามต่อได้ครับ"

    if any(k in c for k in ["กันโกง", "cheat", "ปลอดภัย", "hack", "ความปลอดภัย"]):
        sec = brief_section(["### กันโกง 42/42 test ผ่าน"])
        if sec:
            return "🛡️ ระบบกันโกง:\n\n" + sec + "\n\nทุกข้อเทสอัตโนมัติผ่าน (42/42) ครับ"

    if any(k in c for k in ["เปิด", "เข้าเล่น", "เล่นยังไง", "วิธีเล่น", "server", "เซิร์ฟ"]):
        return ("🖥️ เซิร์ฟเวอร์: gateway HTTP 8190 · game TCP 8191 (radiotower 8192 ปิดอยู่)\n"
                "เปิดเซิร์ฟ: `dotnet run -- --whitelist data/whitelist.txt`\n"
                "เปิดเกม+ต่ออัตโนมัติ: `tools/connect-game.ps1`\n"
                "⚠️ อย่าเปิดเซิร์ฟ/เกมซ้อน 2 ตัวนะครับ")

    if any(k in c for k in ["ใคร", "คนทำ", "โปรเจกต์", "คืออะไร", "เกี่ยวกับ"]):
        sec = brief_section(["## 1. โปรเจกต์นี้คืออะไร"])
        if sec:
            return "🏝️ โปรเจกต์นี้คือ:\n\n" + sec + "\n\nถามต่อได้เลยครับ"

    return ("ครับ บอทตอบได้แค่เรื่องความคืบหน้าของโปรเจกต์ Durango Private Server "
            "ลองถามแบบนี้ดูนะครับ:\n"
            "- \"ตอนนี้ทำอะไรถึงไหนแล้ว?\"\n"
            "- \"เหลืออะไรก่อนเปิด beta?\"\n"
            "- \"เล่นอะไรได้บ้างแล้ว?\"\n"
            "(บอทไม่แจกโค้ดเด็ดขาดครับ 🚫)")


class DurangoBot:
    def __init__(self):
        self.ws = None
        self.seq = None
        self.session_id = None
        self.running = True

    async def connect(self):
        while self.running:
            try:
                async with websockets.connect(GATEWAY, max_size=8 * 1024 * 1024) as ws:
                    self.ws = ws
                    await self.run_ws()
            except websockets.ConnectionClosed as e:
                log.warning("connection closed: %s — reconnecting", e.code)
            except Exception as e:
                log.exception("ws loop error: %s", e)
            await asyncio.sleep(3)

    async def run_ws(self):
        hello = json.loads(await self.ws.recv())
        if hello["op"] != 10:
            log.error("expected hello, got op=%s", hello.get("op"))
            return
        hb_interval = hello["d"]["heartbeat_interval"] / 1000
        await self.ws.send(json.dumps({
            "op": 2,
            "d": {
                "token": TOKEN,
                "intents": INTENTS,
                "properties": {"os": "windows", "browser": "durango-bot", "device": "durango-bot"},
            },
        }))
        hb_task = asyncio.create_task(self.heartbeat(hb_interval))
        try:
            async for raw in self.ws:
                msg = json.loads(raw)
                op = msg["op"]
                if op == 0:
                    self.seq = msg["s"]
                    t = msg["t"]
                    if t == "READY":
                        self.session_id = msg["d"].get("session_id", "")
                        log.info("READY as %s", msg["d"]["user"]["username"])
                    elif t == "MESSAGE_CREATE":
                        asyncio.create_task(self.handle_message(msg["d"]))
                elif op == 1:
                    await self.ws.send(json.dumps({"op": 1, "d": self.seq}))
                elif op == 11:
                    pass
                elif op == 7:
                    log.warning("server asked reconnect")
                    break
        finally:
            hb_task.cancel()

    async def heartbeat(self, interval):
        while True:
            await asyncio.sleep(interval)
            try:
                await self.ws.send(json.dumps({"op": 1, "d": self.seq}))
            except Exception:
                return

    async def handle_message(self, data):
        if data.get("author", {}).get("bot"):
            return
        if data.get("channel_id") not in CHANNELS_ANSWER:
            return
        content = (data.get("content") or "").strip()
        if not content:
            return
        log.info("got: %s", content[:60])
        reply = build_reply(content)
        await self.send(reply)

    async def send(self, text, channel_id=None):
        channel_id = channel_id or (next(iter(CHANNELS_ANSWER), None))
        if not channel_id:
            log.error("no channel to send")
            return False
        payload = json.dumps({"content": text}).encode("utf-8")
        url = "https://discord.com/api/v10/channels/%s/messages" % channel_id
        proc = await asyncio.create_subprocess_exec(
            "curl.exe", "-s", "-X", "POST",
            "-w", "\nHTTP_STATUS:%{http_code}",
            "-H", "Authorization: Bot " + TOKEN,
            "-H", "Content-Type: application/json",
            "-d", "@-", url,
            stdin=asyncio.subprocess.PIPE,
            stdout=asyncio.subprocess.PIPE,
            stderr=asyncio.subprocess.PIPE,
        )
        stdout, stderr = await proc.communicate(payload)
        response = stdout.decode("utf-8", "replace")
        match = re.search(r"HTTP_STATUS:(\d+)\s*$", response)
        status = int(match.group(1)) if match else 0
        if 200 <= status < 300:
            log.info("discord send ok: channel=%s status=%s", channel_id, status)
        else:
            detail = response[:500].replace("\n", " | ")
            err = stderr.decode("utf-8", "replace")[:300]
            log.error("discord send failed: channel=%s status=%s response=%s stderr=%s", channel_id, status, detail, err)
        await asyncio.sleep(1.2)  # rate limit guard
        return 200 <= status < 300


async def main():
    if "--announce" in sys.argv:
        text = " ".join(sys.argv[sys.argv.index("--announce") + 1:])
        if not CHANNEL_ANNOUNCE:
            log.error("ยังไม่ได้ตั้งช่องประกาศ (announce) ใน config.json")
            return
        bot = DurangoBot()
        sent = await bot.send(text, CHANNEL_ANNOUNCE)
        if sent:
            log.info("ประกาศแล้ว: %s", text[:60])
        else:
            log.error("ประกาศไม่สำเร็จ: %s", text[:60])
        return
    load_brief()
    bot = DurangoBot()
    await bot.connect()


if __name__ == "__main__":
    asyncio.run(main())
