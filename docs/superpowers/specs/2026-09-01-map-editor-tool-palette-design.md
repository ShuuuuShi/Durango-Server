# Design: Map Editor Standard Tool Palette (Electron)

**Date:** 2026-09-01  
**Status:** Draft for user review  
**Surface:** Electron Launcher map-editor (`tools/Launcher-Electron/map-editor.html`) + core (`tools/MapEditor`)  
**Approach:** A — Electron palette, Map Project staging, export to server terrain with backups

## 1. Goal

Extend the existing read-only Map Editor importer/viewer into a **standard multi-layer tool palette** that can:

1. Import real Durango terrain binaries used by the server
2. Edit several map layers in a staged Map Project
3. Validate before write
4. Export into `server` terrain extracted folders with timestamped backups and rollback
5. Optionally export artifact/POI changes into `world.json` with the same backup rules

Out of scope for this v1:

- Writing Unity client assets (`resources.assets`, AssetBundles)
- Editing opaque/unproven layers (`whole.elevations`, `whole.waterdepths`)
- Full 3D Unity preview as the primary editor
- Live edit of server files on every brush stroke
- Navigation mesh / true physics collision baking

## 2. Decisions locked with user

| Topic | Decision |
|---|---|
| Focus | Full standard tool palette covering several layers |
| Host UI | Electron Launcher 2D top-down viewer |
| Persistence | Edit + export into server terrain files with backups |
| Architecture | Approach A: Map Project staging, then export |

## 3. Architecture

```text
[Electron map-editor.html]
        |  IPC
[tools/MapEditor core: lib/map-editor-core.js]
        |
   Map Project (staged)
        |
   Validate + Export
        |
 server/.../terrains/extracted/<terrainId>/   (+ backup/)
 server/.../saves/world.json                  (artifacts only)
```

- **Core** owns binary codecs, validation, project store, export/rollback.
- **Electron UI** owns tools, canvas, inspector, confirmations.
- **Server terrain files** are the export target. Game client folder is never overwritten by export.
- Reuse existing APIs where present: `readTerrainSource`, `encode/decodeGarden`, `encode/decodeLandmarks`, `setBiomeType`, `ProjectStore`, `validateTerrain`.

## 4. Source and target files

### 4.1 Terrain import/export (per island / terrain id)

Path pattern:

`server/data/terrains/extracted/<terrainId>/`  
(also accept the runtime copy under `server/bin/<config>/data/terrains/extracted/<terrainId>/` when that is the active server data root)

| File | Role | Editable in v1 |
|---|---|---|
| `info.yml` | width/height, entry points, metadata | entry points yes; unknown keys preserved |
| `whole.biomes` | 1 byte/tile, low 6 bits biome, high 2 bits flags | yes (preserve flags via `setBiomeType`) |
| `oceans.dm` | signed coast distance / land map | yes (Land/Coast tool) |
| `whole.garden` | naturals: 6-byte records `x:u16 y:u16 type:u16` | yes |
| `whole.landmarks` | deco/rocks: 16-byte LE records | yes |
| `whole.ocean` | pass-through / view assist | no write unless unchanged copy |
| `whole.rivers` | pass-through / view assist | no write unless unchanged copy |
| `whole.elevations` | opaque | never modify |
| other unknown files | opaque | never modify |

### 4.2 World / artifacts

| File | Role | Editable in v1 |
|---|---|---|
| server `world.json` (`Artifacts`, related fields) | POI / buildings | yes, separate export step |

Occupancy is a **derived overlay** only (garden + landmark + artifact). No occupancy export file.

## 5. Map Project format

```text
map-project/
  project.json
  terrain/
    info.yml
    whole.biomes
    oceans.dm
    whole.garden
    whole.landmarks
    whole.ocean          # unchanged copy when present
    whole.rivers         # unchanged copy when present
  world/
    world.json           # optional staged artifacts
  backup/                # local project autosaves (not server backups)
  exports/               # export receipts / manifests
```

`project.json` minimum fields:

- `editorVersion`
- `terrainId`
- `sourceTerrainDir`
- `sourceWorldPath` (optional)
- `sourceHashes` (SHA-256 of imported files)
- `mapVersion` (monotonic integer, bump on each successful export)
- `dirtyLayers[]`
- `lastExportAt`, `lastExportTarget`

## 6. Standard tool palette

One active layer/tool at a time.

| Tool | Layer | Interaction | Writes |
|---|---|---|---|
| Biome brush | biomes | paint radius; palette of biome types | `whole.biomes` |
| Land / Coast | oceans.dm | paint signed inland/sea values | `oceans.dm` |
| Garden natural | garden | place / erase harvestables (rocks/trees) | `whole.garden` |
| Landmark | landmarks | place / move / erase / inspect rotate+scale | `whole.landmarks` |
| Artifact / POI | world artifacts | place / move / erase from blueprint list | `world.json` |
| Inspect | all | click shows tile/chunk/ids/raw | none |
| Occupancy overlay | derived | toggle heat map of blocked tiles | none |
| Measure / Validate | — | bounds, alignment, record sizes | none |
| Export | — | confirm summary then write | server targets + server backup |
| Rollback | — | restore chosen server backup set | server targets |

Shared session features:

- Pan / zoom canvas
- Chunk grid toggle
- Layer visibility toggles
- Undo / Redo for edits in the current session
- Autosave Map Project every N minutes while dirty

## 7. UI layout (Electron)

- **Left:** layer tools + mode (brush / eraser / place / select / pan)
- **Center:** 2D canvas — biome base colors; overlays for garden, landmark, artifact, occupancy, chunk bounds, entry point
- **Right:** inspector (tile x/y, chunk, selected record fields) + type palette for active layer
- **Bottom:** dirty indicator, validate status, Export, Rollback

## 8. Tool behavior details

### Biome
- Brush radius in tiles
- Uses existing `setBiomeType(rawByte, biomeType)` so flag bits stay intact
- Invalid biome ids rejected by validate

### Land / Coast
- Edits `oceans.dm` signed values (−32..+32 semantics already used by server `LandDistance`)
- Brush can set absolute value or raise/lower
- Does not reinterpret `whole.ocean` water-depth bytes

### Garden
- One record per tile coordinate; placing on occupied tile replaces or rejects (reject in v1, show conflict)
- Eraser removes exact tile record
- Type picker from `entity_types/natural.json` when available, else raw type id

### Landmark
- 16-byte record fields editable: tile x/y, id, rotate, sub-tile offsets, scale (match client `LandmarkInfo`)
- Move updates tile + optional offsets
- Large decorative rocks are edited here (important for animal occupancy later)

### Artifact / POI
- Staged in project `world/world.json`
- Place requires blueprint id + tile + size footprint when known
- Export is optional checkbox on Export dialog (“include world.json”)

### Occupancy overlay
- Tile blocked if garden natural present OR landmark present OR artifact footprint covers tile
- Color scale: free / blocked / near-block pad (pad configurable, default 0 for display; animal server pad remains separate)

## 9. Export, backup, rollback

### Export steps
1. Run validate; fail blocks export
2. Show dry-run summary: target paths, files to write, hash before/after estimate, whether world.json included
3. User confirms
4. Copy each target file that will change into ` <targetTerrainDir>/backup/<timestamp>/ `
5. Write new binaries / yml
6. If world export selected: backup then write `world.json`
7. Write export receipt under project `exports/<timestamp>.json`
8. Bump `mapVersion`, clear dirty flags for exported layers

### Rollback
- List backup timestamps from target terrain `backup/`
- Restore selected set’s files over current targets
- Does not auto-modify Map Project; user may re-import after rollback

### Safety rules
- Never write into `game/` or client `Durango*_Data`
- Never delete unknown files in terrain folder
- Never rewrite opaque layers
- Refuse export if source hash changed on disk since import (server edited externally) unless user chooses “re-import then re-apply” or “force overwrite”

## 10. Validation rules

Must pass before export:

- `info.yml` tile_count present; biomes length `width*height`
- `oceans.dm` length `width*height` when present
- garden length `% 6 == 0`; all coords in bounds
- landmarks length `% 16 == 0`; all coords in bounds
- no duplicate garden coordinates
- biome flag-preserving encode round-trip on sampled tiles
- world.json JSON parse + artifact tile bounds when included

Warnings (non-blocking):

- landmark id unknown to local catalog
- occupancy shows animals could still clip if server ignores landmarks (document in report)

## 11. Error handling

- Import errors are per-file; missing optional layer continues with warning
- Brush outside map ignored
- Export IO failure stops and leaves backup folder intact; partial writes recorded in receipt as failed
- UI never silent-fails validate: show first N errors in panel

## 12. Testing plan

1. Unit: garden/landmark encode/decode round-trip fixtures
2. Unit: `setBiomeType` preserves high flags
3. Unit: validate catches bad lengths and OOB coords
4. Integration: import `ri35te` → paint biome → export to temp dir → server `TerrainStore` load equivalent sizes
5. Integration: backup + rollback restores original SHA-256
6. Manual Electron: toggle overlays, place/erase garden+landmark, occupancy updates, export confirm dialog

## 13. Implementation phases (after spec approval)

1. **Project store unlock** — allow save/load Map Project from Electron (remove read-only save block)
2. **Palette shell** — toolbar, layer toggles, inspector, undo stack
3. **Biome + Land/Coast brushes**
4. **Garden + Landmark editors**
5. **Occupancy overlay**
6. **Export/backup/rollback** to server terrain
7. **Artifact layer + optional world.json export**
8. **Polish** — autosave, force-hash conflict UX, type catalogs from `natural.json` / blueprints

## 14. Success criteria (v1)

- Import real `ri35te` (or active island) in Electron
- Edit at least biome, garden, and landmark
- See occupancy overlay reflecting garden+landmark(+artifact if loaded)
- Export writes server extracted terrain with backup
- Rollback restores previous bytes
- No client game files modified

## 15. Relationship to animal-through-rock bug

This editor does **not** by itself patch `AnimalSpawner`. It makes landmark/garden occupancy visible and editable so maps can be corrected, and feeds the same binaries the server already loads. A separate server change is still required for animals to query landmarks during spawn/walk.
