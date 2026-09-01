'use strict';

const assert = require('node:assert/strict');
const fs = require('node:fs');
const os = require('node:os');
const path = require('node:path');
const test = require('node:test');

const core = require('../lib/map-editor-core');

function tempDir() {
  return fs.mkdtempSync(path.join(os.tmpdir(), 'durango-project-save-'));
}

function writeTerrainFixture(root, options = {}) {
  const width = options.width || 64;
  const height = options.height || 64;
  fs.mkdirSync(root, { recursive: true });
  fs.writeFileSync(path.join(root, 'info.yml'), JSON.stringify({
    tile_count: [width, height],
    region_template: 'fixtureTemplate',
    entry_points: [[2, 3]],
    unknown_field: { preserved: true },
  }));
  fs.writeFileSync(path.join(root, 'whole.biomes'), Buffer.alloc(width * height, 0x80));
  fs.writeFileSync(path.join(root, 'whole.ocean'), Buffer.alloc((width + 1) * (height + 1), 127));
  fs.writeFileSync(path.join(root, 'whole.rivers'), Buffer.alloc((width + 1) * (height + 1) * 3, 0));
  fs.writeFileSync(path.join(root, 'oceans.dm'), Buffer.alloc(width * height, 1));
  const garden = Buffer.alloc(12);
  garden.writeUInt16LE(1, 0);
  garden.writeUInt16LE(2, 2);
  garden.writeUInt16LE(99, 4);
  garden.writeUInt16LE(width - 1, 6);
  garden.writeUInt16LE(height - 1, 8);
  garden.writeUInt16LE(100, 10);
  fs.writeFileSync(path.join(root, 'whole.garden'), garden);
  const landmark = Buffer.alloc(16);
  landmark.writeUInt16LE(4, 0);
  landmark.writeUInt16LE(5, 2);
  landmark.writeUInt16LE(7, 4);
  landmark.writeUInt8(3, 6);
  landmark.writeInt16LE(-2, 7);
  landmark.writeInt16LE(8, 9);
  landmark.writeInt16LE(-9, 11);
  landmark.writeUInt8(10, 13);
  landmark.writeUInt8(11, 14);
  landmark.writeUInt8(12, 15);
  fs.writeFileSync(path.join(root, 'whole.landmarks'), landmark);
  fs.writeFileSync(path.join(root, 'whole.elevations'), Buffer.alloc(width * height, 9));
  return { width, height };
}

test('ProjectStore.saveFromTerrain is the import staging API', () => {
  assert.equal(typeof core.ProjectStore.saveFromTerrain, 'function');
});

test('saveFromTerrain writes native terrain files and load round-trips equality', () => {
  const source = tempDir();
  writeTerrainFixture(source);
  const result = core.readTerrainSource(source, { mapId: 'fixture' });
  const project = tempDir();
  const sourceBefore = fs.statSync(path.join(source, 'whole.biomes')).mtimeMs;
  const hashes = Object.assign({}, result.report.hashes);
  const saved = core.ProjectStore.saveFromTerrain(project, result.terrain, result.report, {
    sourceTerrainDir: source,
    sourceHashes: hashes,
  });

  const terrainDir = path.join(project, 'terrain');
  for (const name of ['info.yml', 'whole.biomes', 'oceans.dm', 'whole.garden', 'whole.landmarks', 'whole.ocean', 'whole.rivers']) {
    assert.equal(fs.existsSync(path.join(terrainDir, name)), true, 'missing ' + name);
  }
  assert.equal(fs.existsSync(path.join(project, 'project.json')), true);
  assert.equal(fs.existsSync(path.join(project, 'backup')), true);
  assert.equal(fs.existsSync(path.join(project, 'exports')), true);
  assert.equal(fs.existsSync(path.join(project, 'world')), true);
  assert.equal(fs.existsSync(path.join(terrainDir, 'whole.elevations')), true);

  const loaded = core.ProjectStore.load(project);
  assert.equal(loaded.terrain.mapId, 'fixture');
  assert.equal(loaded.terrain.width, result.terrain.width);
  assert.equal(loaded.terrain.height, result.terrain.height);
  assert.deepEqual(loaded.terrain.layers.biomes, result.terrain.layers.biomes);
  assert.deepEqual(loaded.terrain.layers.ocean, result.terrain.layers.ocean);
  assert.deepEqual(loaded.terrain.layers.rivers, result.terrain.layers.rivers);
  assert.deepEqual(loaded.terrain.layers.coastDistance, result.terrain.layers.coastDistance);
  assert.deepEqual(loaded.terrain.garden, result.terrain.garden);
  assert.deepEqual(loaded.terrain.landmarks, result.terrain.landmarks);
  assert.deepEqual(loaded.terrain.metadataUnknown, result.terrain.metadataUnknown);
  assert.equal(fs.statSync(path.join(source, 'whole.biomes')).mtimeMs, sourceBefore);
  assert.equal(saved.projectDir, path.resolve(project));
});

test('saveFromTerrain mapVersion starts at 0 and records source hashes', () => {
  const source = tempDir();
  writeTerrainFixture(source);
  const result = core.readTerrainSource(source, { mapId: 'ri35te' });
  const project = tempDir();
  const hashes = { 'whole.biomes': 'abc', 'oceans.dm': 'def' };
  const saved = core.ProjectStore.saveFromTerrain(project, result.terrain, result.report, {
    sourceTerrainDir: source,
    sourceHashes: hashes,
  });
  const model = saved.model;
  assert.equal(model.mapVersion, 0);
  assert.equal(model.terrainId, 'ri35te');
  assert.equal(model.sourceTerrainDir, path.resolve(source));
  assert.deepEqual(model.sourceHashes, hashes);
  assert.ok(Array.isArray(model.dirtyLayers));
  assert.equal(model.dirtyLayers.length, 0);
  assert.equal(model.lastExportAt, null);
  assert.equal(model.lastExportTarget, null);
  assert.equal(typeof model.editorVersion, 'string');
  assert.ok(model.editorVersion.length > 0);

  const onDisk = JSON.parse(fs.readFileSync(path.join(project, 'project.json'), 'utf8'));
  assert.equal(onDisk.mapVersion, 0);
  assert.equal(onDisk.terrainId, 'ri35te');
  assert.deepEqual(onDisk.sourceHashes, hashes);
});

test('ProjectStore.load remains compatible with ProjectStore.save projects', () => {
  const source = tempDir();
  writeTerrainFixture(source);
  const result = core.readTerrainSource(source, { mapId: 'legacy' });
  const project = tempDir();
  core.ProjectStore.save(project, result.terrain, result.report, { sourceGameFolder: 'fixture-game' });
  const loaded = core.ProjectStore.load(project);
  assert.equal(loaded.terrain.mapId, 'legacy');
  assert.equal(loaded.terrain.layers.biomes.length, 4096);
  assert.deepEqual(loaded.terrain.garden, result.terrain.garden);
  assert.deepEqual(loaded.terrain.landmarks, result.terrain.landmarks);
});
