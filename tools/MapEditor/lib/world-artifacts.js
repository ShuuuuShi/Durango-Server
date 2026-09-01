'use strict';

const fs = require('fs');
const path = require('path');
const crypto = require('crypto');

function issue(severity, code, message, file, details) {
  return { severity, code, message, file: file || 'world.json', details: details || null };
}

function sha256Buffer(buffer) {
  return crypto.createHash('sha256').update(buffer).digest('hex');
}

function isForbiddenDest(dest) {
  const normalized = path.resolve(dest).replace(/\\/g, '/');
  const lower = normalized.toLowerCase();
  if (/(^|\/)durango[^/]*_data(\/|$)/i.test(normalized)) return true;
  if (lower.includes('/game/') && lower.includes('durango')) return true;
  return false;
}

function backupTimestamp(raw) {
  const iso = raw || new Date().toISOString();
  return String(iso).replace(/:/g, '-');
}

function cloneJson(value) {
  return JSON.parse(JSON.stringify(value));
}

function ensureArtifactsArray(world) {
  if (!world || typeof world !== 'object' || Array.isArray(world)) {
    throw new TypeError('world must be an object');
  }
  if (!Array.isArray(world.Artifacts)) world.Artifacts = [];
  return world.Artifacts;
}

function parseWorldJson(input) {
  if (input && typeof input === 'object' && !Array.isArray(input) && !Buffer.isBuffer(input)) {
    const world = input;
    if (!Array.isArray(world.Artifacts)) world.Artifacts = [];
    return { ok: true, issues: [], world, artifacts: world.Artifacts };
  }

  let text;
  if (Buffer.isBuffer(input)) text = input.toString('utf8');
  else if (typeof input === 'string') text = input;
  else {
    return {
      ok: false,
      issues: [issue('error', 'WORLD_JSON_INVALID', 'world.json must be a string, buffer, or object')],
      world: null,
      artifacts: [],
    };
  }

  try {
    const world = JSON.parse(text);
    if (!world || typeof world !== 'object' || Array.isArray(world)) {
      return {
        ok: false,
        issues: [issue('error', 'WORLD_JSON_INVALID', 'world.json root must be an object')],
        world: null,
        artifacts: [],
      };
    }
    if (!Array.isArray(world.Artifacts)) world.Artifacts = [];
    return { ok: true, issues: [], world, artifacts: world.Artifacts };
  } catch (error) {
    return {
      ok: false,
      issues: [issue('error', 'WORLD_JSON_PARSE', `world.json JSON parse failed: ${error.message}`)],
      world: null,
      artifacts: [],
    };
  }
}

function serializeWorldJson(world) {
  const payload = world && typeof world === 'object' ? world : {};
  if (!Array.isArray(payload.Artifacts)) payload.Artifacts = [];
  return `${JSON.stringify(payload, null, 2)}\n`;
}

function originOf(art) {
  if (!art || typeof art !== 'object') return { x: NaN, y: NaN };
  const x = art.TileX != null ? art.TileX : art.x;
  const y = art.TileY != null ? art.TileY : art.y;
  return { x, y };
}

function sizeOf(art) {
  if (!art || typeof art !== 'object') return { w: 1, h: 1 };
  const wRaw = art.SizeX != null ? art.SizeX : art.width;
  const hRaw = art.SizeY != null ? art.SizeY : art.height;
  const w = Number.isFinite(wRaw) ? Math.trunc(wRaw) : 1;
  const h = Number.isFinite(hRaw) ? Math.trunc(hRaw) : 1;
  return { w: Math.max(1, w), h: Math.max(1, h) };
}

function validateWorldArtifacts(artifacts, width, height) {
  const report = { ok: true, issues: [] };
  if (!Array.isArray(artifacts)) {
    report.ok = false;
    report.issues.push(issue('error', 'ARTIFACTS_NOT_ARRAY', 'Artifacts must be an array'));
    return report;
  }
  const haveBounds = Number.isInteger(width) && Number.isInteger(height) && width > 0 && height > 0;
  const seen = new Set();
  for (const art of artifacts) {
    if (!art || typeof art !== 'object') {
      report.issues.push(issue('error', 'ARTIFACT_INVALID', 'artifact record is not an object'));
      continue;
    }
    const { x, y } = originOf(art);
    const { w, h } = sizeOf(art);
    if (!Number.isInteger(x) || !Number.isInteger(y)) {
      report.issues.push(issue('error', 'ARTIFACT_COORD_INVALID', 'artifact tile must be integers', 'world.json', { x, y }));
      continue;
    }
    if (haveBounds && (x < 0 || y < 0 || x >= width || y >= height)) {
      report.issues.push(issue('error', 'ARTIFACT_OOB', `artifact tile (${x},${y}) is out of bounds`, 'world.json', { x, y }));
    } else if (haveBounds && (x + w - 1 >= width || y + h - 1 >= height)) {
      report.issues.push(issue('error', 'ARTIFACT_FOOTPRINT_OOB', `artifact footprint at (${x},${y}) size ${w}x${h} is out of bounds`, 'world.json', { x, y, w, h }));
    }
    const key = `${x},${y}`;
    if (seen.has(key)) {
      report.issues.push(issue('error', 'ARTIFACT_DUPLICATE_COORDINATE', `artifact origin (${x},${y}) is duplicate`, 'world.json', { x, y }));
    }
    seen.add(key);
  }
  report.ok = !report.issues.some((entry) => entry.severity === 'error');
  return report;
}

function placeArtifact(artifacts, width, height, fields) {
  if (!Array.isArray(artifacts)) throw new TypeError('artifacts must be an array');
  if (!Number.isInteger(width) || !Number.isInteger(height) || width <= 0 || height <= 0) {
    throw new RangeError('width and height must be positive integers');
  }
  const params = fields || {};
  const blueprintId = params.BlueprintId != null ? params.BlueprintId : params.blueprintId;
  if (typeof blueprintId !== 'string' || blueprintId.length === 0) {
    throw new TypeError('BlueprintId is required');
  }
  const x = params.TileX != null ? params.TileX : params.x;
  const y = params.TileY != null ? params.TileY : params.y;
  if (!Number.isInteger(x) || !Number.isInteger(y)) throw new TypeError('TileX and TileY must be integers');
  if (x < 0 || y < 0 || x >= width || y >= height) {
    throw new RangeError('artifact coordinate is out of bounds');
  }
  if (artifacts.some((art) => originOf(art).x === x && originOf(art).y === y)) {
    throw new RangeError('artifact tile already occupied');
  }

  const sizeXRaw = params.SizeX != null ? params.SizeX : params.width;
  const sizeYRaw = params.SizeY != null ? params.SizeY : params.height;
  const SizeX = Number.isInteger(sizeXRaw) && sizeXRaw > 0 ? sizeXRaw : 1;
  const SizeY = Number.isInteger(sizeYRaw) && sizeYRaw > 0 ? sizeYRaw : 1;
  if (x + SizeX - 1 >= width || y + SizeY - 1 >= height) {
    throw new RangeError('artifact footprint is out of bounds');
  }

  let entityId = params.EntityId != null ? params.EntityId : params.entityId;
  if (entityId == null || entityId === '') {
    entityId = `artifact_${blueprintId}_${x}_${y}`;
  }
  if (typeof entityId !== 'string') throw new TypeError('EntityId must be a string');
  if (artifacts.some((art) => art && art.EntityId === entityId)) {
    throw new RangeError('artifact EntityId already exists');
  }

  const entityTypeRaw = params.EntityType != null ? params.EntityType : params.entityType;
  const EntityType = Number.isInteger(entityTypeRaw) ? entityTypeRaw : 0;

  artifacts.push({
    EntityId: entityId,
    EntityType,
    BlueprintId: blueprintId,
    TileX: x,
    TileY: y,
    SizeX,
    SizeY,
    Rotation: Number.isInteger(params.Rotation) ? params.Rotation : 0,
    Floor: Number.isInteger(params.Floor) ? params.Floor : 0,
    Stories: Number.isInteger(params.Stories) ? params.Stories : 1,
    BuildingState: Number.isInteger(params.BuildingState) ? params.BuildingState : 0,
    ArchitectEntityIds: Array.isArray(params.ArchitectEntityIds) ? params.ArchitectEntityIds : [null],
  });
  return { artifacts };
}

function moveArtifact(artifacts, width, height, options) {
  if (!Array.isArray(artifacts)) throw new TypeError('artifacts must be an array');
  if (!Number.isInteger(width) || !Number.isInteger(height) || width <= 0 || height <= 0) {
    throw new RangeError('width and height must be positive integers');
  }
  const params = options || {};
  const fromX = params.fromX !== undefined ? params.fromX : (params.TileX !== undefined ? params.TileX : params.x);
  const fromY = params.fromY !== undefined ? params.fromY : (params.TileY !== undefined ? params.TileY : params.y);
  const toX = params.toX;
  const toY = params.toY;
  if (!Number.isInteger(fromX) || !Number.isInteger(fromY) || !Number.isInteger(toX) || !Number.isInteger(toY)) {
    throw new TypeError('from and to coordinates must be integers');
  }
  if (toX < 0 || toY < 0 || toX >= width || toY >= height) {
    throw new RangeError('artifact destination is out of bounds');
  }
  const index = artifacts.findIndex((art) => originOf(art).x === fromX && originOf(art).y === fromY);
  if (index === -1) throw new RangeError('artifact source tile not found');
  if (!(toX === fromX && toY === fromY) && artifacts.some((art) => originOf(art).x === toX && originOf(art).y === toY)) {
    throw new RangeError('artifact tile already occupied');
  }
  const { w, h } = sizeOf(artifacts[index]);
  if (toX + w - 1 >= width || toY + h - 1 >= height) {
    throw new RangeError('artifact footprint is out of bounds');
  }
  artifacts[index].TileX = toX;
  artifacts[index].TileY = toY;
  return { artifacts };
}

function removeArtifact(artifacts, options) {
  if (!Array.isArray(artifacts)) throw new TypeError('artifacts must be an array');
  const params = options || {};
  let index = -1;
  if (params.EntityId != null || params.entityId != null) {
    const id = params.EntityId != null ? params.EntityId : params.entityId;
    index = artifacts.findIndex((art) => art && art.EntityId === id);
  } else {
    const x = params.TileX != null ? params.TileX : params.x;
    const y = params.TileY != null ? params.TileY : params.y;
    if (!Number.isInteger(x) || !Number.isInteger(y)) throw new TypeError('TileX and TileY must be integers');
    index = artifacts.findIndex((art) => originOf(art).x === x && originOf(art).y === y);
  }
  if (index === -1) return { artifacts, removed: 0 };
  artifacts.splice(index, 1);
  return { artifacts, removed: 1 };
}

function exportWorldJson(options) {
  const opts = options || {};
  const destPath = opts.destPath;
  if (!destPath || typeof destPath !== 'string') {
    return { ok: false, issues: [issue('error', 'DEST_PATH_REQUIRED', 'destPath is required')] };
  }
  if (isForbiddenDest(destPath)) {
    return { ok: false, issues: [issue('error', 'FORBIDDEN_DEST', 'refusing to write game client folders', destPath)] };
  }

  if (opts.includeWorldJson !== true) {
    return { ok: true, written: false, destPath: path.resolve(destPath) };
  }

  const parsed = parseWorldJson(opts.world != null ? opts.world : opts.payload);
  if (!parsed.ok) {
    return { ok: false, issues: parsed.issues, destPath: path.resolve(destPath) };
  }

  const width = opts.width;
  const height = opts.height;
  const validation = validateWorldArtifacts(parsed.artifacts, width, height);
  if (!validation.ok) {
    return { ok: false, issues: validation.issues, destPath: path.resolve(destPath) };
  }

  if (opts.confirm !== true) {
    return {
      ok: false,
      issues: [issue('error', 'CONFIRM_REQUIRED', 'world.json export requires confirm:true')],
      destPath: path.resolve(destPath),
    };
  }

  const absDest = path.resolve(destPath);
  const destDir = path.dirname(absDest);
  const backupId = backupTimestamp(opts.timestamp);
  const backupDir = path.join(destDir, 'backup', backupId);
  fs.mkdirSync(destDir, { recursive: true });
  fs.mkdirSync(backupDir, { recursive: true });

  if (fs.existsSync(absDest) && fs.statSync(absDest).isFile()) {
    fs.copyFileSync(absDest, path.join(backupDir, 'world.json'));
  }

  const body = serializeWorldJson(parsed.world);
  const data = Buffer.from(body, 'utf8');
  fs.writeFileSync(absDest, data);

  return {
    ok: true,
    written: true,
    destPath: absDest,
    backupDir,
    backupId,
    hashes: { 'world.json': sha256Buffer(data) },
    filesWritten: ['world.json'],
  };
}

module.exports = {
  parseWorldJson,
  serializeWorldJson,
  placeArtifact,
  moveArtifact,
  removeArtifact,
  validateWorldArtifacts,
  exportWorldJson,
  ensureArtifactsArray,
  cloneJson,
};
