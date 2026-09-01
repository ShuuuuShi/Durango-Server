# Durango Map Editor Core

Node core under tools/MapEditor/lib.

Map Project staging plus validate, backup, write, and rollback for server extracted terrain. Electron MapLab UI (tasks 9-12) is separate; this package owns codecs, mutate APIs, occupancy, and export.

Design spec: docs/superpowers/specs/2026-09-01-map-editor-tool-palette-design.md

## Command line

Work from tools/MapEditor. Package tests use the Node test runner. The import-report bin takes a game folder then an extracted terrain folder such as ri35te under server/data/terrains/extracted.

The import report scans Unity sources, hashes them, reads info.yml (unknown keys preserved), reads binary layers the server already uses, and checks sizes, record alignment, coordinates, and chunk alignment. resources.assets is reported UNITY_ASSET_INTEGRITY_UNVERIFIED. Import and report never overwrite game or server files.

## Import to Map Project

1. Read extracted terrain (typical server/data/terrains/extracted/<terrainId>/, example ri35te). Also valid: the runtime copy under server/bin/<config>/data/terrains/extracted/<terrainId>/.
2. readTerrainSource(terrainFolder) plus validateTerrain(terrain).
3. Stage with ProjectStore.saveFromTerrain(projectDir, terrain, report, { sourceTerrainDir, sourceHashes }). mapVersion starts at 0. ProjectStore.load stays compatible.

Editable files: info.yml, whole.biomes, oceans.dm, whole.garden, whole.landmarks. Opaque layers (whole.elevations, whole.waterdepths, other unknowns) are never rewritten if present.

Garden records are 6 bytes. Landmark records are 16 bytes little-endian. Biome paint must preserve high flag bits.

## Tools (core APIs)

Require lib/map-editor-core.js:

- Biome brush: applyBiomeBrush(biomes, width, height, { x, y, radius, biomeType }) via setBiomeType. Writes whole.biomes.
- Land/Coast: encodeSignedByte, decodeSignedByte, applyCoastBrush with value matching server LandDistance. Writes oceans.dm.
- Garden: placeGarden / eraseGarden (duplicate tile rejected). Writes whole.garden.
- Landmark: placeLandmark, moveLandmark, eraseLandmarkAt. Writes whole.landmarks.
- Artifact/POI: placeArtifact, moveArtifact, removeArtifact; optional exportWorldJson. Writes world.json.
- Occupancy overlay: buildOccupancy({ width, height, garden, landmarks, artifacts, padTiles }) returns a byte grid (0 free, 1 blocked). Derived only; no occupancy export file.
- Validate: validateTerrain, validateExport.

Also exported: scanGameFolder, readTerrainSource, getTerrainChunk, encode/decodeGarden, encode/decodeLandmarks, ProjectStore.save, saveFromTerrain, and load.

Occupancy is display-only. A tile is blocked if a garden natural, landmark, or artifact footprint covers it.

## Export, backup, rollback

Target server extracted terrain only. Never write game/ or client Durango*_Data. Never delete unknown files. Never rewrite opaque layers.

Writable mapping: whole.biomes, oceans.dm, whole.garden, whole.landmarks, info.yml.

Core calls (from map-editor-core): validateExport(terrain); exportTerrain({ destDir, terrain, confirm: true }); rollbackTerrain({ destDir, backupId }) or backupDir under destDir/backup. Without confirm true, export refuses to write.

Export algorithm: (1) validateExport, fail blocks write; (2) confirm gate; (3) copy each target file that will change into destDir/backup/<timestamp>/; (4) write new binaries and yml; (5) optional world.json via exportWorldJson with the same backup rules; (6) result includes SHA-256 hashes. Map Project mapVersion is bumped by the project host after a successful export.

Rollback restores the chosen backup set over current writable targets. It does not auto-modify the Map Project; re-import after rollback if the staged project should match disk.

If source hashes drifted since import, refuse overwrite unless the host re-imports then re-applies, or the operator force-overwrites.

## Safety

- Do not overwrite game client folders on export
- Do not modify opaque terrain layers
- Preserve biome flag bits when painting
- Validate then confirm before export
- Backup timestamp folder before overwrite

This editor does not patch AnimalSpawner. Occupancy makes landmark/garden blocking visible; animals still need a separate server change.

## Electron / MapLab UI

tools/Launcher-Electron/map-editor.html is owned by MapLab (tasks 9-12). Core and export in this package are usable without that UI. Palette, canvas overlays, export confirm dialog, and rollback list live in MapLab.

## Spec section 14 acceptance (core + export manual)

Do not wait for MapLab UI. Core and export-terrain cover the non-UI criteria.

- [x] Import real ri35te (or active island) via readTerrainSource / ProjectStore.saveFromTerrain (Electron import is MapLab)
- [x] Edit at least biome, garden, and landmark (applyBiomeBrush, placeGarden/eraseGarden, placeLandmark/moveLandmark/eraseLandmarkAt)
- [x] Occupancy overlay data reflects garden + landmark (+ artifact if loaded) via buildOccupancy (canvas overlay is MapLab)
- [x] Export writes server extracted terrain with timestamped backup (exportTerrain with confirm true)
- [x] Rollback restores previous bytes (rollbackTerrain)
- [x] No client game files modified (forbidden dest + writable mapping only)

## Formats

Matches TerrainStore: tile 16x16 per chunk; biome chunk 18x18 = 324 bytes; ocean chunk 17x17 = 289 bytes; river chunk 17x17x3 = 867 bytes; garden record 6 bytes; landmark record 16 bytes, little-endian.

oceans.dm is coastDistance (signed LandDistance), not water depth. whole.elevations and whole.waterdepths stay opaque.

## Tests

From tools/MapEditor run the package test suite. Task 14 is docs-only; tests must stay green.
