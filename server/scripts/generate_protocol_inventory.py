#!/usr/bin/env python3
"""Generate a reviewed inventory of generated messages and ServerCore receive registrations."""
from __future__ import annotations

import argparse
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
MESSAGES = ROOT / "server" / "GameCode" / "Messages"
CORE = ROOT / "server" / "ServerCore"
OUTPUT = ROOT / "docs" / "server" / "protocol-inventory.json"

TYPE_RE = re.compile(r"\b(?:public\s+)?(?:readonly\s+)?struct\s+(\w+)|\bclass\s+(\w+)")
CODE_RE = re.compile(r"\bTypeCode\s*(?:=>|=)\s*(\d+)")
RECV_RE = re.compile(r"_conn\.Recv<\s*(\w+)\s*>\s*\(([^;\n]*)")


def line_for(text: str, offset: int) -> int:
    return text.count("\n", 0, offset) + 1


def message_types() -> dict[str, int | None]:
    result: dict[str, int | None] = {}
    for path in sorted(MESSAGES.rglob("*.cs")):
        text = path.read_text(encoding="utf-8-sig")
        match = TYPE_RE.search(text)
        if not match:
            continue
        name = match.group(1) or match.group(2)
        code = CODE_RE.search(text)
        result[name] = int(code.group(1)) if code else None
    return result


def registrations() -> list[dict[str, object]]:
    rows: list[dict[str, object]] = []
    for path in sorted(CORE.rglob("*.cs")):
        text = path.read_text(encoding="utf-8-sig")
        for match in RECV_RE.finditer(text):
            handler = match.group(2).strip()
            rows.append({
                "request": match.group(1),
                "file": path.relative_to(ROOT).as_posix(),
                "line": line_for(text, match.start()),
                "handler": handler or "inline",
                "status": "registered",
            })
    return rows


def build() -> dict[str, object]:
    messages = message_types()
    registered = registrations()
    rows = []
    registered_names = {row["request"] for row in registered}
    for row in registered:
        row["typeCode"] = messages.get(row["request"])
        row["generatedMessage"] = row["request"] in messages
        rows.append(row)
    for name in sorted(set(messages) - registered_names):
        rows.append({
            "request": name,
            "typeCode": messages[name],
            "generatedMessage": True,
            "status": "protocol-only/unowned",
        })
    rows.sort(key=lambda row: (row["status"], row["request"], row.get("file", ""), row.get("line", 0)))
    return {
        "schemaVersion": 1,
        "generatedAt": "deterministic-from-source",
        "generatedMessageCount": len(messages),
        "inboundRegistrationCount": len(registered),
        "rows": rows,
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--output", type=Path, default=OUTPUT)
    parser.add_argument("--check", action="store_true")
    args = parser.parse_args()
    rendered = json.dumps(build(), ensure_ascii=False, indent=2) + "\n"
    if args.check:
        if not args.output.exists() or args.output.read_text(encoding="utf-8") != rendered:
            print(f"stale protocol inventory: {args.output}")
            return 1
        print(f"protocol inventory is current: {args.output}")
        return 0
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(rendered, encoding="utf-8")
    print(f"wrote {args.output}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
