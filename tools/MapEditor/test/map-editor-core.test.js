'use strict';

const assert = require('node:assert/strict');
const fs = require('node:fs');
const os = require('node:os');
const path = require('node:path');
const test = require('node:test');

const core = require('../lib/map-editor-core');

function tempDir() {
  return fs.mkdtempSync(path.join(os.tmpdir(), 'durango-map-editor-'));
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
  fs.writeFileSync(path.join(root, 'whole.elevations'), Buffer.alloc(width * height));
  return { width, height };
}

test('imports terrain layers, metadata and opaque files without losing unknown metadata', () => {
  const root = tempDir();
  writeTerrainFixture(root);
  const result = core.readTerrainSource(root, { mapId: 'fixture' });
  assert.equal(result.report.ok, true);
  assert.equal(result.terrain.mapId, 'fixture');
  assert.deepEqual(result.terrain.metadataUnknown, { unknown_field: { preserved: true } });
  assert.equal(result.terrain.layers.biomes.length, 4096);
  assert.equal(result.terrain.garden.length, 2);
  assert.equal(result.terrain.landmarks[0].offsetX, -2);
  assert.equal(result.terrain.opaque[0].name, 'whole.elevations');
});

test('validates exact layer lengths and fails closed on malformed data', () => {
  const root = tempDir();
  writeTerrainFixture(root);
  fs.writeFileSync(path.join(root, 'whole.rivers'), Buffer.alloc(3));
  const result = core.readTerrainSource(root);
  assert.equal(result.report.ok, false);
  assert.ok(result.report.issues.some((entry) => entry.code === 'INVALID_LAYER_LENGTH'));
});

test('rejects unaligned garden and out-of-bounds records', () => {
  const root = tempDir();
  writeTerrainFixture(root);
  const garden = Buffer.alloc(6);
  garden.writeUInt16LE(64, 0);
  garden.writeUInt16LE(0, 2);
  fs.writeFileSync(path.join(root, 'whole.garden'), garden);
  const result = core.readTerrainSource(root);
  assert.equal(result.report.ok, false);
  assert.ok(result.report.issues.some((entry) => entry.code === 'GARDEN_COORDINATE_OUT_OF_BOUNDS'));
  fs.writeFileSync(path.join(root, 'whole.garden'), Buffer.alloc(5));
  const malformed = core.readTerrainSource(root);
  assert.equal(malformed.report.ok, false);
  assert.ok(malformed.report.issues.some((entry) => entry.code === 'GARDEN_RECORD_ALIGNMENT'));
});

test('extracts server-compatible chunk dimensions and clamps map borders', () => {
  const root = tempDir();
  writeTerrainFixture(root, { width: 32, height: 32 });
  const { terrain } = core.readTerrainSource(root);
  const chunk = core.getTerrainChunk(terrain, 0, 0);
  assert.equal(chunk.biomes.length, 324);
  assert.equal(chunk.ocean.length, 289);
  assert.equal(chunk.rivers.length, 867);
  const edge = core.getTerrainChunk(terrain, 1, 1);
  assert.equal(edge.biomes.length, 324);
  assert.equal(edge.ocean.length, 289);
  assert.equal(edge.rivers.length, 867);
});

test('biome edits preserve upper flag bits', () => {
  assert.equal(core.setBiomeType(0xc5, 7), 0xc7);
  assert.throws(() => core.setBiomeType(1, 64), RangeError);
});

test('garden and landmark codecs round-trip exact records', () => {
  const garden = [{ x: 1, y: 2, entityType: 99 }, { x: 31, y: 30, entityType: 100 }];
  assert.deepEqual(core.decodeGarden(core.encodeGarden(garden), 32, 32, { issues: [] }), garden);
  const landmarks = [{ x: 4, y: 5, id: 7, rotate: 3, offsetX: -2, offsetY: 8, offsetZ: -9, scaleX: 10, scaleY: 11, scaleZ: 12 }];
  assert.deepEqual(core.decodeLandmarks(core.encodeLandmarks(landmarks), 32, 32, { issues: [] }), landmarks);
});

test('saves and loads a project without writing source files', () => {
  const source = tempDir();
  writeTerrainFixture(source);
  const result = core.readTerrainSource(source, { mapId: 'fixture' });
  const project = tempDir();
  const sourceBefore = fs.statSync(path.join(source, 'whole.biomes')).mtimeMs;
  const saved = core.ProjectStore.save(project, result.terrain, result.report, { sourceGameFolder: 'fixture-game' });
  const loaded = core.ProjectStore.load(project);
  assert.equal(saved.model.mapId, 'fixture');
  assert.equal(loaded.terrain.layers.biomes.length, 4096);
  assert.deepEqual(loaded.terrain.garden, result.terrain.garden);
  assert.equal(fs.statSync(path.join(source, 'whole.biomes')).mtimeMs, sourceBefore);
  assert.equal(fs.existsSync(path.join(project, 'project.json')), true);
});

test('imports the real ri35te terrain when available', { skip: !fs.existsSync(path.resolve(__dirname, '../../../server/data/terrains/extracted/ri35te')) }, () => {
  const source = path.resolve(__dirname, '../../../server/data/terrains/extracted/ri35te');
  const result = core.readTerrainSource(source, { mapId: 'ri35te' });
  assert.equal(result.terrain.width, 256);
  assert.equal(result.terrain.height, 256);
  assert.equal(result.terrain.layers.biomes.length, 65536);
  assert.equal(result.terrain.layers.ocean.length, 66049);
  assert.equal(result.terrain.layers.rivers.length, 198147);
  assert.equal(result.terrain.layers.coastDistance.length, 65536);
  assert.equal(result.terrain.garden.length, 2662);
  assert.equal(result.terrain.landmarks.length, 50);
});
