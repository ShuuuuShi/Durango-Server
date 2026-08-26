#!/usr/bin/env python3
"""
Durango asset browser — scan Unity .assets / .bundle files and build a
searchable web gallery of the game's visual assets (models, icons, graphics).

Usage:
    python asset_browser.py scan      # build catalog.json
    python asset_browser.py previews  # render thumbnails from catalog.json
    python asset_browser.py build     # scan + previews in one pass
    python asset_browser.py serve     # serve the gallery on http://127.0.0.1:8787
    python asset_browser.py export ID # dump one asset (mesh -> .obj, texture -> .png)

Requires: pip install UnityPy Pillow numpy
"""
import argparse
import hashlib
import json
import os
import sys
import time

import UnityPy

from PIL import Image, ImageDraw

try:
    import numpy as np
except Exception:  # numpy is optional; mesh wireframe falls back to pure python
    np = None

# ---------------------------------------------------------------------------
# Configuration
# ---------------------------------------------------------------------------

GAME_DIR = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)),
                                          "..", "..", "game", "DurangoV2_Data"))
OUT_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), "output")
THUMB_DIR = os.path.join(OUT_DIR, "thumbs")
CATALOG_PATH = os.path.join(OUT_DIR, "catalog.json")
INDEX_PATH = os.path.join(OUT_DIR, "index.html")

THUMB_SIZE = 256          # max edge of generated thumbnails
DEFAULT_PORT = 8787

# Types we surface in the gallery.
VISUAL_TYPES = ("Texture2D", "Sprite", "Mesh", "Material")

# Types we catalogue but only show as metadata (no thumbnail).
META_TYPES = ("GameObject", "AnimationClip", "Font", "TextAsset", "MonoScript",
              "Shader", "Prefab", "MonoBehaviour")


def log(msg):
    print(msg, flush=True)


def sanitize(name):
    """Make a name safe for a filename segment."""
    keep = []
    for ch in name:
        if ch.isalnum() or ch in "-_.":
            keep.append(ch)
        else:
            keep.append("_")
    return "".join(keep)[:80]


# ---------------------------------------------------------------------------
# Asset discovery
# ---------------------------------------------------------------------------

def discover_sources(game_dir):
    """Return the list of Unity asset files / bundles to scan."""
    sources = []
    for name in sorted(os.listdir(game_dir)):
        path = os.path.join(game_dir, name)
        if not os.path.isfile(path):
            continue
        low = name.lower()
        if low.endswith(".assets") or low.endswith(".bundle"):
            sources.append(path)
        elif (low.startswith("level") and "." not in name and
              os.path.exists(path)):
            sources.append(path)
    bundles_dir = os.path.join(game_dir, "StreamingAssets", "AssetBundles")
    if os.path.isdir(bundles_dir):
        for name in sorted(os.listdir(bundles_dir)):
            if name.lower().endswith(".bundle"):
                sources.append(os.path.join(bundles_dir, name))
    return sources


# ---------------------------------------------------------------------------
# Scan -> catalog
# ---------------------------------------------------------------------------

def mesh_summary(mesh):
    verts = getattr(mesh, "m_VertexCount", None)
    submeshes = getattr(mesh, "m_SubMeshes", None) or []
    n_sub = len(submeshes)
    if verts is None:
        try:
            vd = getattr(mesh, "m_VertexData", None)
            if vd is not None and getattr(vd, "m_VertexCount", None) is not None:
                verts = vd.m_VertexCount
        except Exception:
            verts = None
    return {"vertices": verts, "submeshes": n_sub}


def material_summary(mat):
    shader = ""
    try:
        sh = getattr(mat, "m_Shader", None)
        if sh is not None:
            shader = getattr(sh, "m_Name", "")
    except Exception:
        pass
    return {"shader": shader}


def scan_sources(sources, catalog_path):
    entries = []
    t0 = time.time()
    for src in sources:
        src_name = os.path.relpath(src, GAME_DIR) if src.startswith(GAME_DIR) else os.path.basename(src)
        try:
            env = UnityPy.load(src)
        except Exception as e:
            log(f"[skip] {src_name}: {e}")
            continue
        for obj in env.objects:
            tname = obj.type.name
            if tname not in VISUAL_TYPES and tname not in META_TYPES:
                continue
            name = ""
            extra = {}
            try:
                d = obj.read()
                name = getattr(d, "m_Name", "") or ""
                if tname == "Texture2D":
                    extra = {"w": getattr(d, "m_Width", 0),
                             "h": getattr(d, "m_Height", 0)}
                elif tname == "Sprite":
                    extra = {"w": getattr(d, "m_Rect", None) and round(getattr(d, "m_Rect", None).width or 0),
                             "h": getattr(d, "m_Rect", None) and round(getattr(d, "m_Rect", None).height or 0)}
                elif tname == "Mesh":
                    extra = mesh_summary(d)
                elif tname == "Material":
                    extra = material_summary(d)
                elif tname == "TextAsset":
                    try:
                        extra = {"size": len(d.m_Script) if d.m_Script else 0}
                    except Exception:
                        pass
            except Exception as e:
                name = f"<unreadable: {e}>"
            entries.append({
                "type": tname,
                "name": name,
                "source": src_name,
                "path_id": obj.path_id,
                "extra": extra,
            })
        log(f"[scan] {src_name}: {len(entries)} cumulative entries")
    _priority = {t: i for i, t in enumerate(("Texture2D", "Sprite", "Mesh", "Material"))}
    entries.sort(key=lambda e: (_priority.get(e["type"], 99), e["type"], e["name"].lower()))
    # assign stable sequential ids
    for i, e in enumerate(entries):
        e["id"] = i
    os.makedirs(os.path.dirname(catalog_path), exist_ok=True)
    with open(catalog_path, "w", encoding="utf-8") as f:
        json.dump(entries, f, ensure_ascii=False)
    log(f"[scan] done: {len(entries)} entries in {time.time() - t0:.1f}s -> {catalog_path}")
    return entries


# ---------------------------------------------------------------------------
# Thumbnail generation
# ---------------------------------------------------------------------------

def thumb_path(entry):
    return os.path.join(THUMB_DIR, f"{entry['id']:06d}.png")


def save_texture_thumb(entry, obj):
    im = obj.image
    if im is None:
        return False
    if im.mode not in ("RGB", "RGBA", "L", "LA"):
        im = im.convert("RGBA")
    im = im.convert("RGBA")
    im.thumbnail((THUMB_SIZE, THUMB_SIZE), Image.LANCZOS)
    im.save(thumb_path(entry))
    return True


def save_sprite_thumb(entry, obj):
    try:
        im = obj.image
    except Exception:
        return False
    if im is None:
        return False
    if im.mode not in ("RGB", "RGBA", "L", "LA"):
        im = im.convert("RGBA")
    im = im.convert("RGBA")
    im.thumbnail((THUMB_SIZE, THUMB_SIZE), Image.LANCZOS)
    im.save(thumb_path(entry))
    return True


def _parse_obj_geometry(obj_text):
    verts = []
    faces = []
    for line in obj_text.splitlines():
        if line.startswith("v "):
            parts = line.split()
            try:
                verts.append((float(parts[1]), float(parts[2]), float(parts[3])))
            except (ValueError, IndexError):
                pass
        elif line.startswith("f "):
            idx = []
            for tok in line.split()[1:]:
                # handles f v, f v/vt, f v/vt/vn, f v//vn
                first = tok.split("/")[0]
                try:
                    idx.append(int(first) - 1)
                except ValueError:
                    idx = []
                    break
            if len(idx) >= 3:
                faces.append(idx)
    return verts, faces


def _draw_wireframe(verts, faces, size=THUMB_SIZE):
    """Orthographic-ish projection of a triangle mesh onto a PNG."""
    if not verts:
        return None
    if np is not None:
        V = np.array(verts, dtype=np.float64)
    else:
        V = [list(v) for v in verts]
        V = [[v[0], v[1], v[2]] for v in verts]

    # Center at origin
    if np is not None:
        center = (V.min(axis=0) + V.max(axis=0)) / 2.0
        V = V - center
        # fixed isometric rotation
        ry = np.radians(-30)
        rx = np.radians(-20)
        RY = np.array([[np.cos(ry), 0, np.sin(ry)],
                       [0, 1, 0],
                       [-np.sin(ry), 0, np.cos(ry)]])
        RX = np.array([[1, 0, 0],
                       [0, np.cos(rx), -np.sin(rx)],
                       [0, np.sin(rx), np.cos(rx)]])
        P = V @ RY.T @ RX.T
        scale = max(np.ptp(P[:, 0]), np.ptp(P[:, 1])) or 1.0
        pad = size * 0.08
        k = (size - 2 * pad) / scale
        px = (P[:, 0] - P[:, 0].min()) * k + pad
        py = (P[:, 1] - P[:, 1].min()) * k + pad
        px = px.astype(int)
        py = py.astype(int)
        proj = list(zip(px.tolist(), py.tolist()))
    else:
        xs = [v[0] for v in V]
        ys = [v[1] for v in V]
        zs = [v[2] for v in V]
        cx = (min(xs) + max(xs)) / 2.0
        cy = (min(ys) + max(ys)) / 2.0
        cz = (min(zs) + max(zs)) / 2.0
        import math
        ry = math.radians(-30)
        rx = math.radians(-20)
        proj = []
        for x, y, z in V:
            x -= cx; y -= cy; z -= cz
            x2 = x * math.cos(ry) + z * math.sin(ry)
            z2 = -x * math.sin(ry) + z * math.cos(ry)
            y2 = y * math.cos(rx) - z2 * math.sin(rx)
            proj.append((x2, y2))
        xs = [p[0] for p in proj]
        ys = [p[1] for p in proj]
        scale = max(max(xs) - min(xs), max(ys) - min(ys)) or 1.0
        pad = size * 0.08
        k = (size - 2 * pad) / scale
        proj = [((p[0] - min(xs)) * k + pad, (p[1] - min(ys)) * k + pad) for p in proj]

    img = Image.new("RGBA", (size, size), (34, 34, 38, 255))
    draw = ImageDraw.Draw(img)
    # unique edges
    seen = set()
    n_faces = min(len(faces), 40000)
    for f in faces[:n_faces]:
        n = len(f)
        for i in range(n):
            a, b = f[i], f[(i + 1) % n]
            if a < 0 or b < 0 or a >= len(proj) or b >= len(proj):
                continue
            key = (a, b) if a < b else (b, a)
            if key in seen:
                continue
            seen.add(key)
            draw.line([proj[a], proj[b]], fill=(140, 200, 255, 255), width=1)
    return img


def save_mesh_thumb(entry, obj):
    try:
        obj_text = obj.export()
    except Exception:
        return False
    if not isinstance(obj_text, str) or "v " not in obj_text:
        return False
    verts, faces = _parse_obj_geometry(obj_text)
    img = _draw_wireframe(verts, faces)
    if img is None:
        return False
    img.save(thumb_path(entry))
    return True


def save_material_thumb(entry, obj):
    """Simple colour swatch derived from the material/shader name."""
    seed = (entry["name"] + entry["source"]).encode("utf-8")
    h = hashlib.md5(seed).hexdigest()
    r = int(h[0:2], 16)
    g = int(h[2:4], 16)
    b = int(h[4:6], 16)
    img = Image.new("RGB", (THUMB_SIZE, THUMB_SIZE), (r, g, b))
    draw = ImageDraw.Draw(img)
    draw.rectangle([4, 4, THUMB_SIZE - 5, THUMB_SIZE - 5], outline=(255, 255, 255), width=2)
    img.convert("RGB").save(thumb_path(entry))
    return True


def generate_previews(entries, game_dir, only_types=None, limit=None):
    os.makedirs(THUMB_DIR, exist_ok=True)
    only = set(only_types) if only_types else None

    # Select the entries we need to (re)generate, grouped by source file so each
    # Unity archive is loaded exactly once.
    pending = []
    for entry in entries:
        if only and entry["type"] not in only:
            continue
        if entry["type"] not in VISUAL_TYPES:
            continue
        if limit is not None and len(pending) >= limit:
            break
        if os.path.exists(thumb_path(entry)):
            continue
        pending.append(entry)

    by_source = {}
    for entry in pending:
        src = os.path.join(game_dir, entry["source"]) if not os.path.isabs(entry["source"]) else entry["source"]
        by_source.setdefault(src, []).append(entry)

    done = 0
    skipped = 0
    failed = 0
    t0 = time.time()
    for src, src_entries in by_source.items():
        if not os.path.exists(src):
            failed += len(src_entries)
            continue
        env = None
        try:
            env = UnityPy.load(src)
        except Exception as e:
            failed += len(src_entries)
            continue
        by_key = {}
        for o in env.objects:
            by_key.setdefault((o.path_id, o.type.name), o)
        for entry in src_entries:
            o = by_key.get((entry["path_id"], entry["type"]))
            ok = False
            if o is not None:
                try:
                    obj = o.read()
                    if entry["type"] == "Texture2D":
                        ok = save_texture_thumb(entry, obj)
                    elif entry["type"] == "Sprite":
                        ok = save_sprite_thumb(entry, obj)
                    elif entry["type"] == "Mesh":
                        ok = save_mesh_thumb(entry, obj)
                    elif entry["type"] == "Material":
                        ok = save_material_thumb(entry, obj)
                except Exception:
                    ok = False
            if ok:
                done += 1
            else:
                failed += 1
            if (done + skipped + failed) % 100 == 0:
                log(f"[previews] done={done} skipped={skipped} failed={failed} "
                    f"({time.time() - t0:.1f}s)")
    log(f"[previews] finished: done={done} skipped={skipped} failed={failed} "
        f"in {time.time() - t0:.1f}s")
    return done, skipped, failed


# ---------------------------------------------------------------------------
# Gallery HTML
# ---------------------------------------------------------------------------

GALLERY_HTML = r"""<!doctype html>
<html lang="th">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Durango Asset Browser</title>
<style>
  :root { color-scheme: dark; }
  * { box-sizing: border-box; }
  body { margin: 0; font-family: system-ui, -apple-system, "Segoe UI", sans-serif;
         background: #17181c; color: #e6e6e6; }
  header { position: sticky; top: 0; z-index: 5; background: #1f2027;
           border-bottom: 1px solid #2e2f37; padding: 12px 18px; }
  header h1 { font-size: 18px; margin: 0 0 10px; }
  .controls { display: flex; gap: 10px; flex-wrap: wrap; align-items: center; }
  input[type=search] { flex: 1; min-width: 200px; background: #2a2b33;
           color: #eee; border: 1px solid #3a3b45; border-radius: 6px;
           padding: 8px 12px; font-size: 14px; }
  .chip { background: #2a2b33; border: 1px solid #3a3b45; color: #ccc;
           border-radius: 999px; padding: 6px 12px; cursor: pointer; font-size: 13px; }
  .chip.active { background: #3d6cff; border-color: #3d6cff; color: #fff; }
  .count { font-size: 13px; color: #8b8d98; }
  main { padding: 18px; }
  .grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
          gap: 12px; }
  .card { background: #1f2027; border: 1px solid #2e2f37; border-radius: 8px;
          overflow: hidden; cursor: pointer; transition: transform .06s, border-color .12s; }
  .card:hover { border-color: #3d6cff; transform: translateY(-2px); }
  .card .thumb { width: 100%; aspect-ratio: 1; object-fit: contain; background: #121216; }
  .card .no-thumb { width: 100%; aspect-ratio: 1; display: flex; align-items: center;
          justify-content: center; background: #121216; color: #8b8d98; font-size: 12px;
          text-transform: uppercase; letter-spacing: .05em; }
  .card .meta { padding: 8px 10px; }
  .card .name { font-size: 12px; font-weight: 600; word-break: break-word;
          display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical;
          overflow: hidden; }
  .card .type { font-size: 11px; color: #7f86f0; margin-top: 3px; }
  #modal { display: none; position: fixed; inset: 0; background: rgba(0,0,0,.72);
           z-index: 10; align-items: center; justify-content: center; padding: 24px; }
  #modal.open { display: flex; }
  #modal .box { background: #1f2027; border: 1px solid #34353e; border-radius: 10px;
           max-width: 720px; width: 100%; max-height: 90vh; overflow: auto; padding: 18px; }
  #modal img { max-width: 100%; max-height: 55vh; display: block; margin: 0 auto;
           background: #121216; border-radius: 6px; }
  #modal h2 { font-size: 16px; margin: 12px 0 4px; word-break: break-all; }
  #modal .kv { font-size: 13px; color: #b9bac4; }
  #modal .kv b { color: #e6e6e6; }
  #modal .close { float: right; background: #2a2b33; border: 1px solid #3a3b45;
           color: #ccc; border-radius: 6px; padding: 4px 10px; cursor: pointer; }
  .empty { padding: 40px; text-align: center; color: #8b8d98; }
</style>
</head>
<body>
<header>
  <h1>Durango Asset Browser</h1>
  <div class="controls">
    <input id="q" type="search" placeholder="ค้นหาชื่อ / source / type ...">
    <button class="chip active" data-type="all">All</button>
    <button class="chip" data-type="Texture2D">Texture2D</button>
    <button class="chip" data-type="Sprite">Sprite</button>
    <button class="chip" data-type="Mesh">Mesh</button>
    <button class="chip" data-type="Material">Material</button>
    <button class="chip" data-type="GameObject">GameObject</button>
    <button class="chip" data-type="AnimationClip">AnimationClip</button>
    <button class="chip" data-type="Font">Font</button>
    <button class="chip" data-type="TextAsset">TextAsset</button>
    <span class="count" id="count"></span>
  </div>
</header>
<main><div class="grid" id="grid"></div><div class="empty" id="empty" style="display:none">ไม่พบรายการ</div></main>
<div id="modal"><div class="box">
  <button class="close" onclick="closeModal()">ปิด</button>
  <div id="modal-body"></div>
</div></div>
<script>
let DATA = [];
let activeType = 'all';
let query = '';
fetch('catalog.json').then(r => r.json()).then(d => {
  DATA = d;
  render();
}).catch(e => { document.getElementById('grid').innerHTML =
  '<div class="empty">โหลด catalog.json ไม่ได้ — รัน `python asset_browser.py serve` ก่อน</div>'; });

function thumb(entry) {
  const t = entry.type;
  if (['Texture2D','Sprite','Mesh','Material'].includes(t)) {
    return `thumbs/${String(entry.id).padStart(6,'0')}.png`;
  }
  return null;
}
function render() {
  const q = query.toLowerCase();
  const list = DATA.filter(e => {
    if (activeType !== 'all' && e.type !== activeType) return false;
    if (q && !(e.name || '').toLowerCase().includes(q)
          && !(e.source || '').toLowerCase().includes(q)
          && !(e.type || '').toLowerCase().includes(q)) return false;
    return true;
  });
  document.getElementById('count').textContent = `${list.length} / ${DATA.length}`;
  const grid = document.getElementById('grid');
  grid.innerHTML = '';
  document.getElementById('empty').style.display = list.length ? 'none' : 'block';
  const shown = list.slice(0, 2000);
  for (const e of shown) {
    const card = document.createElement('div');
    card.className = 'card';
    card.onclick = () => openModal(e);
    const t = thumb(e);
    const extra = e.extra || {};
    let sub = '';
    if (e.type === 'Texture2D' || e.type === 'Sprite') sub = (extra.w||'') + 'x' + (extra.h||'');
    else if (e.type === 'Mesh') sub = (extra.vertices != null ? extra.vertices + ' verts' : '') +
        (extra.submeshes ? ', ' + extra.submeshes + ' sub' : '');
    card.innerHTML = `<div>${ t
        ? `<img class="thumb" loading="lazy" src="${t}" onerror="this.replaceWith(Object.assign(document.createElement('div'),{className:'no-thumb',textContent:'${e.type}'}))">`
        : `<div class="no-thumb">${e.type}</div>` }</div>
      <div class="meta"><div class="name">${escapeHtml(e.name)}</div>
      <div class="type">${e.type}${sub ? ' · ' + sub : ''}</div></div>`;
    grid.appendChild(card);
  }
  if (list.length > shown.length) {
    const more = document.createElement('div');
    more.className = 'empty';
    more.textContent = `แสดง ${shown.length} จาก ${list.length} รายการ (กรองเพิ่มเพื่อดูทั้งหมด)`;
    grid.appendChild(more);
  }
}
function openModal(e) {
  const t = thumb(e);
  const extra = e.extra || {};
  let img = '';
  if (t) img = `<img src="${t}">`;
  let kv = `<div class="kv"><b>Type:</b> ${e.type}</div>
            <div class="kv"><b>Source:</b> ${escapeHtml(e.source)}</div>
            <div class="kv"><b>Path ID:</b> ${e.path_id}</div>
            <div class="kv"><b>ID:</b> ${e.id}</div>`;
  if (e.type === 'Texture2D' || e.type === 'Sprite')
    kv += `<div class="kv"><b>Size:</b> ${extra.w||'?'} x ${extra.h||'?'}</div>`;
  if (e.type === 'Mesh')
    kv += `<div class="kv"><b>Vertices:</b> ${extra.vertices ?? '?'}</div>
           <div class="kv"><b>Submeshes:</b> ${extra.submeshes ?? '?'}</div>`;
  if (e.type === 'Material')
    kv += `<div class="kv"><b>Shader:</b> ${escapeHtml(extra.shader||'')}</div>`;
  document.getElementById('modal-body').innerHTML =
    `<h2>${escapeHtml(e.name)}</h2>${img}${kv}`;
  document.getElementById('modal').classList.add('open');
}
function closeModal() { document.getElementById('modal').classList.remove('open'); }
document.getElementById('modal').addEventListener('click', ev => {
  if (ev.target === document.getElementById('modal')) closeModal();
});
function escapeHtml(s) { return String(s).replace(/[&<>"']/g,
  c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c])); }
document.getElementById('q').addEventListener('input', e => { query = e.target.value; render(); });
document.querySelectorAll('.chip').forEach(c => c.addEventListener('click', () => {
  document.querySelectorAll('.chip').forEach(x => x.classList.remove('active'));
  c.classList.add('active');
  activeType = c.dataset.type;
  render();
}));
</script>
</body>
</html>
"""


def write_gallery():
    os.makedirs(OUT_DIR, exist_ok=True)
    with open(INDEX_PATH, "w", encoding="utf-8") as f:
        f.write(GALLERY_HTML)
    log(f"[gallery] wrote {INDEX_PATH}")


# ---------------------------------------------------------------------------
# Export single asset
# ---------------------------------------------------------------------------

def export_asset(entries, game_dir, asset_id):
    entry = None
    for e in entries:
        if str(e["id"]) == str(asset_id):
            entry = e
            break
    if entry is None:
        log(f"no asset with id {asset_id}")
        return
    src = os.path.join(game_dir, entry["source"])
    env = UnityPy.load(src)
    obj = None
    for o in env.objects:
        if o.path_id == entry["path_id"] and o.type.name == entry["type"]:
            obj = o.read()
            break
    if obj is None:
        log("object not found in source")
        return
    out_dir = os.path.join(OUT_DIR, "export")
    os.makedirs(out_dir, exist_ok=True)
    base = f"{sanitize(entry['name']) or entry['type']}_{entry['id']}"
    if entry["type"] == "Mesh":
        try:
            obj_text = obj.export()
            path = os.path.join(out_dir, base + ".obj")
            with open(path, "w", encoding="utf-8") as f:
                f.write(obj_text)
            log(f"wrote {path}")
        except Exception as e:
            log(f"mesh export failed: {e}")
    elif entry["type"] in ("Texture2D", "Sprite"):
        try:
            im = obj.image
            path = os.path.join(out_dir, base + ".png")
            im.save(path)
            log(f"wrote {path}")
        except Exception as e:
            log(f"texture export failed: {e}")
    else:
        log("export supported for Mesh / Texture2D / Sprite only")


# ---------------------------------------------------------------------------
# CLI
# ---------------------------------------------------------------------------

def cmd_scan(args):
    sources = discover_sources(args.game_dir)
    log(f"[scan] found {len(sources)} sources")
    scan_sources(sources, CATALOG_PATH)


def cmd_previews(args):
    if not os.path.exists(CATALOG_PATH):
        log("catalog.json not found — run `scan` first")
        sys.exit(1)
    with open(CATALOG_PATH, encoding="utf-8") as f:
        entries = json.load(f)
    generate_previews(entries, args.game_dir, only_types=args.types, limit=args.limit)


def cmd_build(args):
    sources = discover_sources(args.game_dir)
    log(f"[build] found {len(sources)} sources")
    entries = scan_sources(sources, CATALOG_PATH)
    generate_previews(entries, args.game_dir, only_types=args.types, limit=args.limit)
    write_gallery()


def cmd_serve(args):
    if not os.path.exists(INDEX_PATH):
        write_gallery()
    import http.server
    import socketserver
    os.chdir(OUT_DIR)

    class Handler(http.server.SimpleHTTPRequestHandler):
        def log_message(self, *a):
            pass

    with socketserver.TCPServer(("127.0.0.1", args.port), Handler) as httpd:
        log(f"[serve] http://127.0.0.1:{args.port}  (Ctrl+C to stop)")
        try:
            httpd.serve_forever()
        except KeyboardInterrupt:
            pass


def main():
    p = argparse.ArgumentParser(description="Durango asset browser")
    p.add_argument("command", choices=["scan", "previews", "build", "serve", "export"])
    p.add_argument("id", nargs="?", help="asset id for export")
    p.add_argument("--game-dir", default=GAME_DIR, help="path to DurangoV2_Data")
    p.add_argument("--types", nargs="*", default=None,
                   help="restrict previews to these types (e.g. Texture2D Mesh)")
    p.add_argument("--limit", type=int, default=None, help="limit number of previews")
    p.add_argument("--port", type=int, default=DEFAULT_PORT)
    args = p.parse_args()

    if args.command == "serve":
        cmd_serve(args)
        return
    if args.command == "scan":
        cmd_scan(args)
        return
    if args.command == "build":
        cmd_build(args)
        return
    if args.command == "previews":
        cmd_previews(args)
        return
    if args.command == "export":
        if not os.path.exists(CATALOG_PATH):
            log("catalog.json not found — run `scan` first")
            sys.exit(1)
        with open(CATALOG_PATH, encoding="utf-8") as f:
            entries = json.load(f)
        export_asset(entries, args.game_dir, args.id)
        return


if __name__ == "__main__":
    main()
