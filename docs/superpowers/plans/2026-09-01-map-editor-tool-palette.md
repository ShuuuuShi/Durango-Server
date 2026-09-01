# Map Editor Tool Palette Implementation Plan

Goal: Electron Map Lab multi-layer editor with Map Project staging and server terrain export backups.

Spec: docs/superpowers/specs/2026-09-01-map-editor-tool-palette-design.md

## Architecture note

Node core under tools/MapEditor/lib. Electron IPC in Launcher-Electron. Canvas UI in map-editor.html. Export only to server terrain extracted folders.

## Global Constraints

- Do not overwrite game client folders on export
- Do not modify opaque terrain layers
- Garden record size 6 bytes; landmark record size 16 bytes
- Preserve biome flag bits when painting
- Validate then confirm before export
- Backup timestamp folder before overwrite
- Test driven; commit after each green task

## File map

| Path | Responsibility |
|---|---|
| tools/MapEditor/lib/map-editor-core | Codecs plus mutate/validate APIs |
| tools/MapEditor/lib/export-terrain | Backup, write, rollback, receipt |
| tools/MapEditor/lib/occupancy | Derived blocked-tile map |
| tools/MapEditor/test/* | Tests |
| tools/Launcher-Electron/main | IPC save/export/rollback |
| tools/Launcher-Electron/preload | mapEditor bridge |
| tools/Launcher-Electron/map-editor.html | Palette UI |
| tools/MapEditor/README.md | Usage docs |

---

### Task 1: Biome brush helper (preserve flags)

**Files:** modify map-editor-core; test biome-brush

**Interfaces:** applyBiomeBrush(biomes, width, height, {x, y, radius, biomeType}) -> { changed }; uses setBiomeType

- [ ] Step 1: Failing test — paint type 5 at (3,3) r=1 on 8x8 buffer of 0x80; type bits 5; flags stay 0x80
- [ ] Step 2: Run node test runner in tools/MapEditor — expect FAIL
- [ ] Step 3: Implement circular brush calling setBiomeType; export from core
- [ ] Step 4: Expect PASS
- [ ] Step 5: Commit feat(map-editor): add biome brush helper preserving flags

---

### Task 2: Coast / land brush for oceans.dm

**Files:** modify map-editor-core; test coast-brush

**Interfaces:** encodeSignedByte, decodeSignedByte, applyCoastBrush(... {value}) matching server LandDistance

- [ ] Step 1: Failing tests for +5 and -3
- [ ] Step 2: FAIL
- [ ] Step 3: Implement encode/decode plus circular brush
- [ ] Step 4: PASS
- [ ] Step 5: Commit feat(map-editor): add oceans.dm coast brush

---

### Task 3: Garden place/erase with conflict reject

**Files:** modify map-editor-core; test garden-edit

**Interfaces:** placeGarden / eraseGarden; reject duplicate tile; shape matches decodeGarden

- [ ] Step 1: Tests place, duplicate reject, erase, OOB, encode round-trip
- [ ] Step 2: FAIL
- [ ] Step 3: Implement
- [ ] Step 4: PASS
- [ ] Step 5: Commit feat(map-editor): garden place and erase helpers

---

### Task 4: Landmark place move erase

**Files:** modify map-editor-core; test landmark-edit

**Interfaces:** placeLandmark, moveLandmark, eraseLandmarkAt; fields match decodeLandmarks; encode round-trip

- [ ] Step 1: Tests place move erase encode
- [ ] Step 2: FAIL
- [ ] Step 3: Implement
- [ ] Step 4: PASS
- [ ] Step 5: Commit feat(map-editor): landmark place move erase helpers

---

### Task 5: Occupancy overlay builder

**Files:** create occupancy module under tools/MapEditor/lib; test occupancy; re-export from core

**Interfaces:** buildOccupancy({ width, height, garden, landmarks, artifacts, padTiles }) returns byte grid (0 free, 1 blocked)

- [ ] Step 1: Failing tests garden landmark artifact pad
- [ ] Step 2: FAIL
- [ ] Step 3: Implement grid filler
- [ ] Step 4: PASS
- [ ] Step 5: Commit feat(map-editor): occupancy overlay from garden landmark artifacts

---

### Task 6: Export validate plus backup writer

**Files:** create export-terrain module; test export-terrain

**Interfaces:** validateExport, exportTerrain, rollbackTerrain

**Writable mapping:** whole.biomes, oceans.dm, whole.garden, whole.landmarks, info.yml. Opaque never rewritten if present.

**Algorithm:** validate then backup timestamp folder then copy-changed-then-write then hash receipt

- [ ] Step 1: Temp-dir tests export backup rollback validate-fail
- [ ] Step 2: FAIL
- [ ] Step 3: Implement export-terrain module
- [ ] Step 4: PASS
- [ ] Step 5: Commit feat(map-editor): terrain export with backup and rollback

---

### Task 7: ProjectStore save-from-import

**Files:** modify ProjectStore in map-editor-core; test project-save

**Interfaces:** ProjectStore.saveFromTerrain(projectDir, terrain, report, { sourceTerrainDir, sourceHashes }); mapVersion starts at 0; keep load compatible

- [ ] Step 1: Test save/load equality
- [ ] Step 2: FAIL
- [ ] Step 3: Implement
- [ ] Step 4: PASS full package test suite
- [ ] Step 5: Commit feat(map-editor): save imported terrain as Map Project

---

### Task 8: Electron bridge for project save export rollback

**Files:** modify Launcher-Electron main process and preload bridge

**Bridge methods:** saveProject, exportTerrain (dry-run unless confirm true), listBackups, rollbackTerrain

**main:** deserializeTerrain inverse of serializeTerrainImport; default export path imported folder or server/data/terrains/extracted/<mapId>

- [ ] Step 1: Add main handlers plus preload bridge
- [ ] Step 2: Smoke dry-run after import
- [ ] Step 3: Commit feat(map-editor): electron bridge for project save export rollback

---

### Task 9: Electron UI unlock editing plus layer palette shell

**Files:** modify map-editor.html

- [ ] Step 1: Remove read-only save alert; wire Save to Map Project bridge
- [ ] Step 2: Add layers Biome Coast Garden Landmark Inspect Occupancy Export Rollback and modes brush erase place select pan
- [ ] Step 3: Draw garden plus landmark overlays; inspect on click
- [ ] Step 4: Manual launch with map-editor flag and import terrain
- [ ] Step 5: Commit feat(map-editor): unlock project save and layer palette shell

---

### Task 10: Electron UI biome plus coast brushes

**Files:** modify map-editor.html, main process, preload bridge
**Bridge:** applyBiomeBrush / applyCoastBrush return updated base64 plus changed count

- [ ] Step 1: Keep layer buffers in renderer after import
- [ ] Step 2: Drag-paint via bridge; redraw
- [ ] Step 3: Manual paint; dry-run lists dirty layers
- [ ] Step 4: Commit feat(map-editor): biome and coast brushes in Electron UI

---

### Task 11: Electron UI garden landmark occupancy

**Files:** modify map-editor.html plus mutate/occupancy bridge helpers

- [ ] Step 1: Garden place erase with duplicate status
- [ ] Step 2: Landmark place move erase; inspector rotate scale
- [ ] Step 3: Occupancy toggle overlay
- [ ] Step 4: Manual verify blocked tiles over rocks
- [ ] Step 5: Commit feat(map-editor): garden landmark tools and occupancy overlay

---

### Task 12: Export dialog plus rollback UI

**Files:** modify map-editor.html

- [ ] Step 1: Export dry-run summary modal
- [ ] Step 2: Confirm write plus status
- [ ] Step 3: Rollback list restore plus re-import prompt
- [ ] Step 4: Force checkbox when source hash drift
- [ ] Step 5: Commit feat(map-editor): export confirm dialog and rollback UI

---

### Task 13: Optional world.json artifact layer

**Files:** create world-artifacts module; test world-artifacts; Electron Artifact tool; optional export with backup

- [ ] Step 1: Parse serialize Artifacts; place move remove
- [ ] Step 2: Occupancy includes footprints
- [ ] Step 3: Optional world export
- [ ] Step 4: Commit feat(map-editor): artifact layer export to world.json

---

### Task 14: Docs plus acceptance pass

**Files:** modify tools/MapEditor/README.md

- [ ] Step 1: Document edit export backup flow
- [ ] Step 2: Package tests all green
- [ ] Step 3: Manual acceptance vs spec section 14
- [ ] Step 4: Commit docs(map-editor): document tool palette edit and export

---

## Spec coverage checklist

| Spec section | Tasks |
|---|---|
| Electron host plus Approach A | 8-12 |
| Import terrain binaries | existing plus 7 |
| Biome coast garden landmark tools | 1-4, 10-11 |
| Occupancy overlay | 5, 11 |
| Map Project staging | 7, 9 |
| Export backup rollback | 6, 8, 12 |
| Validate before write | 6, 12 |
| Opaque layers untouched | 6 |
| Artifact world.json | 13 |
| No client file writes | 6, 12 |

## Out of scope

- Unity 3D preview gray-screen fixes
- AnimalSpawner landmark collision (separate server work)
- Editing whole.elevations / unproven schemas
