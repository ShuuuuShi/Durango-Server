'use strict';

const assert = require('node:assert/strict');
const test = require('node:test');
const fs = require('node:fs');
const os = require('node:os');
const path = require('node:path');
const crypto = require('node:crypto');

const core = require('../lib/map-editor-core');
const exportMod = require('../lib/export-terrain');

function sha256(buf) {
  return crypto.createHash('sha256').update(buf).digest('hex');
}

function makeTerrain(overrides) {
  const width = 8;
  const height = 8;
  const biomes = Buffer.alloc(width * height, 0x01);
  const coastDistance = Buffer.alloc(width * height, 0x05);
  const garden = [{ x: 1, y: 1, entityType: 7 }];
  const landmarks = [{
    x: 2,
    y: 2,
    id: 9,
    rotate: 0,
    offsetX: 0,
    offsetY: 0,
    offsetZ: 0,
    scaleX: 1,
    scaleY: 1,
    scaleZ: 1,
  }];
  const metadata = { tile_count: [width, height] };
  const metadataRaw = JSON.stringify(metadata);
  const terrain = {
    mapId: 'fixture',
    width,
    height,
    metadata,
    metadataRaw,
    layers: { biomes, coastDistance, ocean: null, rivers: null },
    garden,
    landmarks,
    opaque: [],
  };
  return Object.assign(terrain, overrides || {});
}

function writeDest(destDir, files) {
  fs.mkdirSync(destDir, { recursive: true });
  for (const [name, data] of Object.entries(files)) {
    fs.writeFileSync(path.join(destDir, name), data);
  }
}

test('validateExport, exportTerrain, rollbackTerrain are re-exported from map-editor-core', () => {
  assert.equal(typeof exportMod.validateExport, 'function');
  assert.equal(typeof exportMod.exportTerrain, 'function');
  assert.equal(typeof exportMod.rollbackTerrain, 'function');
  assert.equal(core.validateExport, exportMod.validateExport);
  assert.equal(core.exportTerrain, exportMod.exportTerrain);
  assert.equal(core.rollbackTerrain, exportMod.rollbackTerrain);
});

test('validateExport returns ok for a valid terrain', () => {
  const report = exportMod.validateExport(makeTerrain());
  assert.equal(report.ok, true);
  assert.ok(Array.isArray(report.issues));
});

test('validateExport fails on out-of-bounds garden coordinates', () => {
  const terrain = makeTerrain();
  terrain.garden = [{ x: 99, y: 1, entityType: 1 }];
  const report = exportMod.validateExport(terrain);
  assert.equal(report.ok, false);
  assert.ok(report.issues.some((entry) => entry.severity === 'error'));
});

test('validateExport fails on duplicate garden coordinates', () => {
  const terrain = makeTerrain();
  terrain.garden = [
    { x: 1, y: 1, entityType: 1 },
    { x: 1, y: 1, entityType: 2 },
  ];
  const report = exportMod.validateExport(terrain);
  assert.equal(report.ok, false);
});

test('successful export writes writable files and backs up previous bytes', () => {
  const destDir = fs.mkdtempSync(path.join(os.tmpdir(), 'map-editor-export-ok-'));
  const oldBiomes = Buffer.alloc(64, 0xaa);
  const oldCoast = Buffer.alloc(64, 0xbb);
  const oldGarden = Buffer.alloc(6, 0xcc);
  const oldLandmarks = Buffer.alloc(16, 0xdd);
  const oldInfo = Buffer.from('{"tile_count":[8,8],"old":true}', 'utf8');
  const opaqueBytes = Buffer.from('OPAQUE-ELEVATIONS-DO-NOT-TOUCH');
  const oceanBytes = Buffer.from('OCEAN-PASSTHROUGH');
  writeDest(destDir, {
    'whole.biomes': oldBiomes,
    'oceans.dm': oldCoast,
    'whole.garden': oldGarden,
    'whole.landmarks': oldLandmarks,
    'info.yml': oldInfo,
    'whole.elevations': opaqueBytes,
    'whole.ocean': oceanBytes,
  });

  const terrain = makeTerrain();
  terrain.layers.biomes.fill(0x21);
  const result = exportMod.exportTerrain({ destDir, terrain, confirm: true });
  assert.equal(result.ok, true);
  assert.ok(result.backupDir);
  assert.ok(fs.existsSync(result.backupDir));
  const backupRel = path.relative(destDir, result.backupDir);
  assert.match(backupRel.replace(/\\/g, '/'), /^backup\//);

  assert.deepEqual(fs.readFileSync(path.join(result.backupDir, 'whole.biomes')), oldBiomes);
  assert.deepEqual(fs.readFileSync(path.join(result.backupDir, 'oceans.dm')), oldCoast);
  assert.deepEqual(fs.readFileSync(path.join(result.backupDir, 'whole.garden')), oldGarden);
  assert.deepEqual(fs.readFileSync(path.join(result.backupDir, 'whole.landmarks')), oldLandmarks);
  assert.deepEqual(fs.readFileSync(path.join(result.backupDir, 'info.yml')), oldInfo);

  assert.deepEqual(fs.readFileSync(path.join(destDir, 'whole.biomes')), terrain.layers.biomes);
  assert.deepEqual(fs.readFileSync(path.join(destDir, 'oceans.dm')), terrain.layers.coastDistance);
  assert.deepEqual(fs.readFileSync(path.join(destDir, 'whole.garden')), core.encodeGarden(terrain.garden));
  assert.deepEqual(fs.readFileSync(path.join(destDir, 'whole.landmarks')), core.encodeLandmarks(terrain.landmarks));
  assert.equal(fs.readFileSync(path.join(destDir, 'info.yml'), 'utf8'), terrain.metadataRaw);

  assert.ok(result.hashes);
  assert.equal(result.hashes['whole.biomes'], sha256(terrain.layers.biomes));
  assert.equal(result.hashes['oceans.dm'], sha256(terrain.layers.coastDistance));
  assert.equal(result.hashes['whole.garden'], sha256(core.encodeGarden(terrain.garden)));
  assert.equal(result.hashes['whole.landmarks'], sha256(core.encodeLandmarks(terrain.landmarks)));
  assert.equal(result.hashes['info.yml'], sha256(Buffer.from(terrain.metadataRaw)));
});

test('opaque file present in dest is not overwritten on export', () => {
  const destDir = fs.mkdtempSync(path.join(os.tmpdir(), 'map-editor-export-opaque-'));
  const opaqueBytes = Buffer.from('OPAQUE-ELEVATIONS-DO-NOT-TOUCH');
  const oceanBytes = Buffer.from('OCEAN-PASSTHROUGH');
  writeDest(destDir, {
    'whole.biomes': Buffer.alloc(64, 0xaa),
    'whole.elevations': opaqueBytes,
    'whole.ocean': oceanBytes,
  });
  const result = exportMod.exportTerrain({ destDir, terrain: makeTerrain(), confirm: true });
  assert.equal(result.ok, true);
  assert.deepEqual(fs.readFileSync(path.join(destDir, 'whole.elevations')), opaqueBytes);
  assert.deepEqual(fs.readFileSync(path.join(destDir, 'whole.ocean')), oceanBytes);
  assert.equal(fs.existsSync(path.join(result.backupDir, 'whole.elevations')), false);
});

test('validate-fail does not mutate dest and does not create backup', () => {
  const destDir = fs.mkdtempSync(path.join(os.tmpdir(), 'map-editor-export-bad-'));
  const oldBiomes = Buffer.alloc(64, 0x11);
  const opaqueBytes = Buffer.from('keep-opaque');
  writeDest(destDir, {
    'whole.biomes': oldBiomes,
    'whole.elevations': opaqueBytes,
  });
  const before = fs.readdirSync(destDir).sort();

  const terrain = makeTerrain();
  terrain.garden = [{ x: 99, y: 99, entityType: 1 }];
  const result = exportMod.exportTerrain({ destDir, terrain, confirm: true });
  assert.equal(result.ok, false);
  assert.deepEqual(fs.readFileSync(path.join(destDir, 'whole.biomes')), oldBiomes);
  assert.deepEqual(fs.readFileSync(path.join(destDir, 'whole.elevations')), opaqueBytes);
  assert.deepEqual(fs.readdirSync(destDir).sort(), before);
  assert.equal(fs.existsSync(path.join(destDir, 'backup')), false);
});

test('rollbackTerrain restores previous writable files from backup', () => {
  const destDir = fs.mkdtempSync(path.join(os.tmpdir(), 'map-editor-export-rb-'));
  const oldBiomes = Buffer.alloc(64, 0xaa);
  const oldCoast = Buffer.alloc(64, 0xbb);
  const oldGarden = Buffer.alloc(6, 0xcc);
  const oldLandmarks = Buffer.alloc(16, 0xdd);
  const oldInfo = Buffer.from('{"tile_count":[8,8],"old":true}', 'utf8');
  const opaqueBytes = Buffer.from('OPAQUE-STAYS');
  writeDest(destDir, {
    'whole.biomes': oldBiomes,
    'oceans.dm': oldCoast,
    'whole.garden': oldGarden,
    'whole.landmarks': oldLandmarks,
    'info.yml': oldInfo,
    'whole.elevations': opaqueBytes,
  });

  const exported = exportMod.exportTerrain({ destDir, terrain: makeTerrain(), confirm: true });
  assert.equal(exported.ok, true);
  assert.notDeepEqual(fs.readFileSync(path.join(destDir, 'whole.biomes')), oldBiomes);

  const rolled = exportMod.rollbackTerrain({ destDir, backupDir: exported.backupDir });
  assert.equal(rolled.ok, true);
  assert.deepEqual(fs.readFileSync(path.join(destDir, 'whole.biomes')), oldBiomes);
  assert.deepEqual(fs.readFileSync(path.join(destDir, 'oceans.dm')), oldCoast);
  assert.deepEqual(fs.readFileSync(path.join(destDir, 'whole.garden')), oldGarden);
  assert.deepEqual(fs.readFileSync(path.join(destDir, 'whole.landmarks')), oldLandmarks);
  assert.deepEqual(fs.readFileSync(path.join(destDir, 'info.yml')), oldInfo);
  assert.deepEqual(fs.readFileSync(path.join(destDir, 'whole.elevations')), opaqueBytes);
});

test('rollbackTerrain accepts backupId as the backup folder name', () => {
  const destDir = fs.mkdtempSync(path.join(os.tmpdir(), 'map-editor-export-rbid-'));
  const oldBiomes = Buffer.alloc(64, 0x77);
  writeDest(destDir, { 'whole.biomes': oldBiomes });
  const exported = exportMod.exportTerrain({ destDir, terrain: makeTerrain(), confirm: true });
  assert.equal(exported.ok, true);
  const backupId = exported.backupId || path.basename(exported.backupDir);
  const rolled = exportMod.rollbackTerrain({ destDir, backupId });
  assert.equal(rolled.ok, true);
  assert.deepEqual(fs.readFileSync(path.join(destDir, 'whole.biomes')), oldBiomes);
});
