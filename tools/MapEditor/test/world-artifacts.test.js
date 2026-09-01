'use strict';

const assert = require('node:assert/strict');
const test = require('node:test');
const fs = require('node:fs');
const os = require('node:os');
const path = require('node:path');
const crypto = require('node:crypto');

const core = require('../lib/map-editor-core');
const worldArt = require('../lib/world-artifacts');

function sha256(buf) {
  return crypto.createHash('sha256').update(buf).digest('hex');
}

function sampleWorld(overrides) {
  const world = {
    TerrainId: 'ri35te',
    Artifacts: [
      {
        EntityId: 'poi_near_warp_accelerator_0',
        EntityType: 6282,
        BlueprintId: 'warp_accelerator',
        TileX: 3,
        TileY: 4,
        SizeX: 2,
        SizeY: 3,
        Rotation: 0,
        Floor: 0,
        Stories: 1,
        BuildingState: 0,
        ArchitectEntityIds: [null],
      },
    ],
    RemovedNaturals: [],
    Boxes: [],
    ArtifactMaterials: {},
    Version: 1,
  };
  return Object.assign(world, overrides || {});
}

function at(grid, width, x, y) {
  return grid[x + y * width];
}

test('world-artifacts APIs are re-exported from map-editor-core', () => {
  assert.equal(typeof worldArt.parseWorldJson, 'function');
  assert.equal(typeof worldArt.serializeWorldJson, 'function');
  assert.equal(typeof worldArt.placeArtifact, 'function');
  assert.equal(typeof worldArt.moveArtifact, 'function');
  assert.equal(typeof worldArt.removeArtifact, 'function');
  assert.equal(typeof worldArt.validateWorldArtifacts, 'function');
  assert.equal(typeof worldArt.exportWorldJson, 'function');
  assert.equal(core.parseWorldJson, worldArt.parseWorldJson);
  assert.equal(core.serializeWorldJson, worldArt.serializeWorldJson);
  assert.equal(core.placeArtifact, worldArt.placeArtifact);
  assert.equal(core.moveArtifact, worldArt.moveArtifact);
  assert.equal(core.removeArtifact, worldArt.removeArtifact);
  assert.equal(core.validateWorldArtifacts, worldArt.validateWorldArtifacts);
  assert.equal(core.exportWorldJson, worldArt.exportWorldJson);
});

test('parseWorldJson reads Artifacts from a world.json string', () => {
  const parsed = worldArt.parseWorldJson(JSON.stringify(sampleWorld()));
  assert.ok(parsed && Array.isArray(parsed.artifacts));
  assert.equal(parsed.artifacts.length, 1);
  assert.equal(parsed.artifacts[0].BlueprintId, 'warp_accelerator');
  assert.equal(parsed.artifacts[0].TileX, 3);
  assert.equal(parsed.artifacts[0].TileY, 4);
  assert.equal(parsed.artifacts[0].SizeX, 2);
  assert.equal(parsed.artifacts[0].SizeY, 3);
  assert.equal(parsed.world.TerrainId, 'ri35te');
  assert.deepEqual(parsed.world.RemovedNaturals, []);
});

test('serializeWorldJson round-trips Artifacts and preserves sibling keys', () => {
  const original = sampleWorld();
  original.Clans = [{ Id: 1 }];
  const text = worldArt.serializeWorldJson(original);
  const again = JSON.parse(text);
  assert.deepEqual(again.Artifacts, original.Artifacts);
  assert.equal(again.TerrainId, 'ri35te');
  assert.deepEqual(again.Clans, [{ Id: 1 }]);
  assert.ok(Array.isArray(again.RemovedNaturals));
});

test('placeArtifact adds a blueprint at a tile then serialize round-trips', () => {
  const artifacts = [];
  const result = worldArt.placeArtifact(artifacts, 16, 16, {
    BlueprintId: 'camp_warphole',
    EntityType: 9101,
    TileX: 5,
    TileY: 6,
    SizeX: 6,
    SizeY: 6,
  });
  assert.ok(result && Array.isArray(result.artifacts));
  assert.equal(result.artifacts.length, 1);
  assert.equal(artifacts.length, 1);
  assert.equal(artifacts[0].BlueprintId, 'camp_warphole');
  assert.equal(artifacts[0].EntityType, 9101);
  assert.equal(artifacts[0].TileX, 5);
  assert.equal(artifacts[0].TileY, 6);
  assert.equal(artifacts[0].SizeX, 6);
  assert.equal(artifacts[0].SizeY, 6);
  assert.equal(typeof artifacts[0].EntityId, 'string');
  assert.ok(artifacts[0].EntityId.length > 0);

  const world = { TerrainId: 'fixture', Artifacts: artifacts };
  const decoded = worldArt.parseWorldJson(worldArt.serializeWorldJson(world));
  assert.equal(decoded.artifacts.length, 1);
  assert.equal(decoded.artifacts[0].BlueprintId, 'camp_warphole');
  assert.equal(decoded.artifacts[0].TileX, 5);
  assert.equal(decoded.artifacts[0].TileY, 6);
});

test('placeArtifact requires BlueprintId', () => {
  const artifacts = [];
  assert.throws(
    () => worldArt.placeArtifact(artifacts, 8, 8, { TileX: 1, TileY: 1 }),
    TypeError,
  );
  assert.equal(artifacts.length, 0);
});

test('placeArtifact rejects out-of-bounds and non-integer tiles', () => {
  const artifacts = [];
  assert.throws(() => worldArt.placeArtifact(artifacts, 8, 8, { BlueprintId: 'a', TileX: -1, TileY: 0 }), RangeError);
  assert.throws(() => worldArt.placeArtifact(artifacts, 8, 8, { BlueprintId: 'a', TileX: 0, TileY: -1 }), RangeError);
  assert.throws(() => worldArt.placeArtifact(artifacts, 8, 8, { BlueprintId: 'a', TileX: 8, TileY: 0 }), RangeError);
  assert.throws(() => worldArt.placeArtifact(artifacts, 8, 8, { BlueprintId: 'a', TileX: 0, TileY: 8 }), RangeError);
  assert.throws(() => worldArt.placeArtifact(artifacts, 8, 8, { BlueprintId: 'a', TileX: 1.5, TileY: 0 }), TypeError);
  assert.equal(artifacts.length, 0);
});

test('placeArtifact rejects duplicate origin tile', () => {
  const artifacts = [];
  worldArt.placeArtifact(artifacts, 8, 8, { BlueprintId: 'a', TileX: 1, TileY: 2 });
  assert.throws(
    () => worldArt.placeArtifact(artifacts, 8, 8, { BlueprintId: 'b', TileX: 1, TileY: 2 }),
    RangeError,
  );
  assert.equal(artifacts.length, 1);
  assert.equal(artifacts[0].BlueprintId, 'a');
});

test('moveArtifact relocates TileX/TileY and keeps blueprint/size', () => {
  const artifacts = [];
  worldArt.placeArtifact(artifacts, 16, 16, {
    EntityId: 'keep-me',
    BlueprintId: 'warp_accelerator',
    EntityType: 6282,
    TileX: 1,
    TileY: 2,
    SizeX: 4,
    SizeY: 4,
    Rotation: 90,
  });
  const moved = worldArt.moveArtifact(artifacts, 16, 16, { fromX: 1, fromY: 2, toX: 7, toY: 8 });
  assert.equal(moved.artifacts.length, 1);
  assert.equal(artifacts[0].TileX, 7);
  assert.equal(artifacts[0].TileY, 8);
  assert.equal(artifacts[0].EntityId, 'keep-me');
  assert.equal(artifacts[0].BlueprintId, 'warp_accelerator');
  assert.equal(artifacts[0].SizeX, 4);
  assert.equal(artifacts[0].SizeY, 4);
  assert.equal(artifacts[0].Rotation, 90);
});

test('removeArtifact erases by tile or EntityId', () => {
  const artifacts = [];
  worldArt.placeArtifact(artifacts, 8, 8, { EntityId: 'one', BlueprintId: 'a', TileX: 1, TileY: 1 });
  worldArt.placeArtifact(artifacts, 8, 8, { EntityId: 'two', BlueprintId: 'b', TileX: 3, TileY: 3 });
  const byTile = worldArt.removeArtifact(artifacts, { TileX: 1, TileY: 1 });
  assert.equal(byTile.removed, 1);
  assert.equal(artifacts.length, 1);
  assert.equal(artifacts[0].EntityId, 'two');
  const byId = worldArt.removeArtifact(artifacts, { EntityId: 'two' });
  assert.equal(byId.removed, 1);
  assert.equal(artifacts.length, 0);
  const miss = worldArt.removeArtifact(artifacts, { EntityId: 'two' });
  assert.equal(miss.removed, 0);
});

test('occupancy includes parsed artifact SizeX/SizeY footprints', () => {
  const width = 10;
  const height = 10;
  const parsed = worldArt.parseWorldJson(sampleWorld());
  const grid = core.buildOccupancy({
    width,
    height,
    artifacts: parsed.artifacts,
  });
  for (let y = 4; y < 7; y++) {
    for (let x = 3; x < 5; x++) {
      assert.equal(at(grid, width, x, y), 1, `expected blocked at ${x},${y}`);
    }
  }
  assert.equal(at(grid, width, 2, 4), 0);
  assert.equal(at(grid, width, 3, 3), 0);
  assert.equal(at(grid, width, 5, 4), 0);
});

test('validateWorldArtifacts fails on out-of-bounds tiles and invalid JSON', () => {
  const ok = worldArt.validateWorldArtifacts(sampleWorld().Artifacts, 16, 16);
  assert.equal(ok.ok, true);

  const oob = worldArt.validateWorldArtifacts(
    [{ BlueprintId: 'a', TileX: 99, TileY: 1, SizeX: 1, SizeY: 1 }],
    16,
    16,
  );
  assert.equal(oob.ok, false);
  assert.ok(oob.issues.some((entry) => entry.severity === 'error'));

  const badJson = worldArt.parseWorldJson('{not json');
  assert.equal(badJson.ok, false);
  assert.ok(Array.isArray(badJson.issues));
});

test('exportWorldJson is optional: includeWorldJson false does not write', () => {
  const destDir = fs.mkdtempSync(path.join(os.tmpdir(), 'map-editor-world-skip-'));
  const destPath = path.join(destDir, 'world.json');
  const original = Buffer.from(JSON.stringify({ TerrainId: 'old', Artifacts: [] }), 'utf8');
  fs.writeFileSync(destPath, original);
  const result = worldArt.exportWorldJson({
    destPath,
    world: sampleWorld(),
    confirm: true,
    includeWorldJson: false,
    width: 16,
    height: 16,
  });
  assert.equal(result.ok, true);
  assert.equal(result.written, false);
  assert.deepEqual(fs.readFileSync(destPath), original);
  assert.equal(fs.existsSync(path.join(destDir, 'backup')), false);
});

test('exportWorldJson without confirm does not write', () => {
  const destDir = fs.mkdtempSync(path.join(os.tmpdir(), 'map-editor-world-noconfirm-'));
  const destPath = path.join(destDir, 'world.json');
  const original = Buffer.from(JSON.stringify({ TerrainId: 'old', Artifacts: [] }), 'utf8');
  fs.writeFileSync(destPath, original);
  const result = worldArt.exportWorldJson({
    destPath,
    world: sampleWorld(),
    confirm: false,
    includeWorldJson: true,
    width: 16,
    height: 16,
  });
  assert.equal(result.ok, false);
  assert.deepEqual(fs.readFileSync(destPath), original);
});

test('exportWorldJson backs up then writes world.json when included and confirmed', () => {
  const destDir = fs.mkdtempSync(path.join(os.tmpdir(), 'map-editor-world-ok-'));
  const destPath = path.join(destDir, 'world.json');
  const oldWorld = { TerrainId: 'old', Artifacts: [], Extra: 'keep-on-backup' };
  const original = Buffer.from(JSON.stringify(oldWorld), 'utf8');
  fs.writeFileSync(destPath, original);

  const world = sampleWorld();
  const result = worldArt.exportWorldJson({
    destPath,
    world,
    confirm: true,
    includeWorldJson: true,
    width: 16,
    height: 16,
  });
  assert.equal(result.ok, true);
  assert.equal(result.written, true);
  assert.ok(result.backupDir);
  assert.ok(fs.existsSync(path.join(result.backupDir, 'world.json')));
  assert.deepEqual(fs.readFileSync(path.join(result.backupDir, 'world.json')), original);

  const written = JSON.parse(fs.readFileSync(destPath, 'utf8'));
  assert.equal(written.TerrainId, 'ri35te');
  assert.equal(written.Artifacts.length, 1);
  assert.equal(written.Artifacts[0].BlueprintId, 'warp_accelerator');
  assert.ok(result.hashes);
  assert.equal(result.hashes['world.json'], sha256(fs.readFileSync(destPath)));
});

test('exportWorldJson refuses dest under game client folders', () => {
  const result = worldArt.exportWorldJson({
    destPath: 'C:\\game\\DurangoV2_Data\\world.json',
    world: sampleWorld(),
    confirm: true,
    includeWorldJson: true,
    width: 16,
    height: 16,
  });
  assert.equal(result.ok, false);
  assert.ok(result.issues.some((entry) => entry.code === 'FORBIDDEN_DEST'));
});
