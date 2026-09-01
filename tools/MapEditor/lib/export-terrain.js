'use strict';

function getCore() {
  return require('./map-editor-core');
}

const fs = require('fs');
const path = require('path');
const WRITABLE_TERRAIN_FILES = Object.freeze([
  'whole.biomes',
  'oceans.dm',
  'whole.garden',
  'whole.landmarks',
  'info.yml',
]);

function issue(severity, code, message, file, details) {
  return { severity, code, message, file: file || null, details: details || null };
}

function resolveTerrain(input) {
  if (!input) return input;
  if (input.terrain) return input.terrain;
  return input;
}

function validateExport(input) {
  const terrain = resolveTerrain(input);
  const report = getCore().validateTerrain(terrain);
  if (!report.issues) report.issues = [];

  const seenGarden = new Set();
  for (const record of (terrain && terrain.garden) || []) {
    if (!Number.isInteger(record.x) || !Number.isInteger(record.y)) continue;
    const key = `${record.x},${record.y}`;
    if (seenGarden.has(key)) {
      report.issues.push(issue(
        'error',
        'GARDEN_DUPLICATE_COORDINATE',
        `garden coordinate (${record.x},${record.y}) ซ้ำ`,
        'whole.garden',
        { x: record.x, y: record.y },
      ));
    }
    seenGarden.add(key);
  }

  report.ok = !report.issues.some((entry) => entry.severity === 'error');
  return report;
}

function infoYmlBuffer(terrain) {
  if (terrain.metadataRaw != null) {
    return Buffer.isBuffer(terrain.metadataRaw)
      ? terrain.metadataRaw
      : Buffer.from(String(terrain.metadataRaw), 'utf8');
  }
  if (terrain.metadata != null) {
    return Buffer.from(JSON.stringify(terrain.metadata), 'utf8');
  }
  return null;
}

function buildWritablePayloads(terrain) {
  const layers = terrain.layers || {};
  const payloads = {};
  if (Buffer.isBuffer(layers.biomes)) payloads['whole.biomes'] = layers.biomes;
  if (Buffer.isBuffer(layers.coastDistance)) payloads['oceans.dm'] = layers.coastDistance;
  if (Array.isArray(terrain.garden)) payloads['whole.garden'] = getCore().encodeGarden(terrain.garden);
  if (Array.isArray(terrain.landmarks)) payloads['whole.landmarks'] = getCore().encodeLandmarks(terrain.landmarks);
  const info = infoYmlBuffer(terrain);
  if (info) payloads['info.yml'] = info;
  return payloads;
}

function isForbiddenDest(destDir) {
  const normalized = path.resolve(destDir).replace(/\\/g, '/');
  const lower = normalized.toLowerCase();
  if (/(^|\/)durango[^/]*_data(\/|$)/i.test(normalized)) return true;
  if (lower.includes('/game/') && lower.includes('durango')) return true;
  return false;
}

function backupTimestamp(raw) {
  const iso = raw || new Date().toISOString();
  return String(iso).replace(/:/g, '-');
}

function resolveBackupDir(destDir, options) {
  if (options.backupDir) return path.resolve(options.backupDir);
  if (options.backupId) return path.join(path.resolve(destDir), 'backup', String(options.backupId));
  return null;
}

function assertBackupUnderDest(destDir, backupDir) {
  const backupRoot = path.resolve(destDir, 'backup');
  const resolved = path.resolve(backupDir);
  const rootPrefix = backupRoot.endsWith(path.sep) ? backupRoot : backupRoot + path.sep;
  const lowerResolved = resolved.toLowerCase();
  const lowerRoot = backupRoot.toLowerCase();
  const lowerPrefix = rootPrefix.toLowerCase();
  return lowerResolved === lowerRoot || lowerResolved.startsWith(lowerPrefix);
}

function exportTerrain(options) {
  const opts = options || {};
  const destDir = opts.destDir;
  const terrain = opts.terrain || opts.payload || null;
  const confirm = opts.confirm === true;

  if (!destDir || typeof destDir !== 'string') {
    return { ok: false, issues: [issue('error', 'DEST_DIR_REQUIRED', 'destDir is required')] };
  }
  if (isForbiddenDest(destDir)) {
    return { ok: false, issues: [issue('error', 'FORBIDDEN_DEST', 'refusing to write game client folders', destDir)] };
  }

  const validation = validateExport(terrain);
  if (!validation.ok) {
    return { ok: false, issues: validation.issues, destDir: path.resolve(destDir) };
  }
  if (!confirm) {
    return {
      ok: false,
      issues: [issue('error', 'CONFIRM_REQUIRED', 'export requires confirm:true')],
      destDir: path.resolve(destDir),
    };
  }

  const payloads = buildWritablePayloads(terrain);
  const backupId = backupTimestamp(opts.timestamp);
  const absDest = path.resolve(destDir);
  const backupDir = path.join(absDest, 'backup', backupId);
  fs.mkdirSync(absDest, { recursive: true });
  fs.mkdirSync(backupDir, { recursive: true });

  for (const name of WRITABLE_TERRAIN_FILES) {
    if (!payloads[name]) continue;
    const destPath = path.join(absDest, name);
    if (fs.existsSync(destPath) && fs.statSync(destPath).isFile()) {
      fs.copyFileSync(destPath, path.join(backupDir, name));
    }
  }

  const hashes = {};
  const filesWritten = [];
  for (const name of WRITABLE_TERRAIN_FILES) {
    const data = payloads[name];
    if (!data) continue;
    fs.writeFileSync(path.join(absDest, name), data);
    hashes[name] = getCore().sha256Buffer(data);
    filesWritten.push(name);
  }

  return {
    ok: true,
    destDir: absDest,
    backupDir,
    backupId,
    hashes,
    filesWritten,
  };
}

function rollbackTerrain(options) {
  const opts = options || {};
  const destDir = opts.destDir;
  if (!destDir || typeof destDir !== 'string') {
    return { ok: false, issues: [issue('error', 'DEST_DIR_REQUIRED', 'destDir is required')] };
  }
  if (isForbiddenDest(destDir)) {
    return { ok: false, issues: [issue('error', 'FORBIDDEN_DEST', 'refusing to write game client folders', destDir)] };
  }

  const backupDir = resolveBackupDir(destDir, opts);
  if (!backupDir) {
    return { ok: false, issues: [issue('error', 'BACKUP_REQUIRED', 'backupDir or backupId is required')] };
  }
  if (!assertBackupUnderDest(destDir, backupDir)) {
    return { ok: false, issues: [issue('error', 'BACKUP_PATH_INVALID', 'backup must be under destDir/backup')] };
  }
  if (!fs.existsSync(backupDir) || !fs.statSync(backupDir).isDirectory()) {
    return { ok: false, issues: [issue('error', 'BACKUP_NOT_FOUND', `backup not found: ${backupDir}`)] };
  }

  const absDest = path.resolve(destDir);
  const restored = [];
  for (const name of WRITABLE_TERRAIN_FILES) {
    const src = path.join(backupDir, name);
    if (!fs.existsSync(src) || !fs.statSync(src).isFile()) continue;
    fs.copyFileSync(src, path.join(absDest, name));
    restored.push(name);
  }

  return {
    ok: true,
    destDir: absDest,
    backupDir: path.resolve(backupDir),
    backupId: path.basename(path.resolve(backupDir)),
    filesRestored: restored,
  };
}

module.exports = {
  WRITABLE_TERRAIN_FILES,
  validateExport,
  exportTerrain,
  rollbackTerrain,
};
