'use strict';

const fs = require('fs');
const path = require('path');
const crypto = require('crypto');

const TILE_SIZE = 16;
const WORLD_UNITS_PER_TILE = 200;
const MIN_TILE_COUNT = 64;
const MAX_TILE_COUNT = 2048;
const BIOME_FLAGS_MASK = 0xc0;
const BIOME_TYPE_MASK = 0x3f;

const TERRAIN_FILES = Object.freeze([
  'whole.biomes',
  'whole.ocean',
  'whole.rivers',
  'oceans.dm',
  'whole.garden',
  'whole.landmarks',
  'info.yml',
]);

const OPAQUE_TERRAIN_FILES = Object.freeze([
  'whole.elevations',
  'whole.waterdepths',
  'cliffs.dm',
  'fertilities',
  'lakes.dm',
  'rivers.dm',
  'scoops.dm',
  'whole.humidities',
  'whole.no_plant',
  'whole.temperatures',
  'config.yml',
  'pois.yml',
  'herds.yml',
]);

function issue(severity, code, message, file = null, details = null) {
  return { severity, code, message, file, details };
}

function isObject(value) {
  return value !== null && typeof value === 'object' && !Array.isArray(value);
}

function clone(value) {
  return JSON.parse(JSON.stringify(value));
}

function sha256File(filePath) {
  const hash = crypto.createHash('sha256');
  hash.update(fs.readFileSync(filePath));
  return hash.digest('hex');
}

function sha256Buffer(buffer) {
  return crypto.createHash('sha256').update(buffer).digest('hex');
}

function readFileSafe(filePath, report, label = path.basename(filePath)) {
  try {
    return fs.readFileSync(filePath);
  } catch (error) {
    report.issues.push(issue('error', 'FILE_READ_FAILED', `อ่านไฟล์ ${label} ไม่ได้: ${error.message}`, label));
    return null;
  }
}

function listFilesRecursive(root) {
  const result = [];
  if (!fs.existsSync(root)) return result;
  for (const entry of fs.readdirSync(root, { withFileTypes: true })) {
    const fullPath = path.join(root, entry.name);
    if (entry.isDirectory()) result.push(...listFilesRecursive(fullPath));
    else if (entry.isFile()) result.push(fullPath);
  }
  return result;
}

function scanGameFolder(gameFolder) {
  const report = { ok: true, issues: [], gameFolder: path.resolve(gameFolder), dataFolder: null, files: [], terrainCandidates: [] };
  if (!gameFolder || !fs.existsSync(gameFolder) || !fs.statSync(gameFolder).isDirectory()) {
    report.ok = false;
    report.issues.push(issue('error', 'GAME_FOLDER_NOT_FOUND', 'ไม่พบโฟลเดอร์เกม', gameFolder || null));
    return report;
  }

  const dataFolder = path.join(report.gameFolder, 'DurangoV2_Data');
  report.dataFolder = dataFolder;
  if (!fs.existsSync(dataFolder) || !fs.statSync(dataFolder).isDirectory()) {
    report.ok = false;
    report.issues.push(issue('error', 'UNITY_DATA_NOT_FOUND', 'ไม่พบ DurangoV2_Data', 'DurangoV2_Data'));
    return report;
  }

  const allFiles = listFilesRecursive(dataFolder);
  const lowerName = (p) => path.basename(p).toLowerCase();
  const selected = allFiles.filter((filePath) => {
    const name = lowerName(filePath);
    return name === 'resources.assets' || /^level\d+(\.ress)?$/.test(name) ||
      /^sharedassets\d+\.assets$/.test(name) || name.endsWith('.bundle');
  });
  report.files = selected.map((filePath) => {
    const relativePath = path.relative(dataFolder, filePath);
    try {
      const stat = fs.statSync(filePath);
      return { path: relativePath, size: stat.size, sha256: sha256File(filePath), readable: true };
    } catch (error) {
      report.ok = false;
      report.issues.push(issue('error', 'FILE_READ_FAILED', `อ่านไฟล์ Unity ไม่ได้: ${error.message}`, relativePath));
      return { path: relativePath, size: null, sha256: null, readable: false };
    }
  }).sort((a, b) => a.path.localeCompare(b.path));

  const resources = report.files.find((file) => file.path.toLowerCase() === 'resources.assets');
  if (!resources) {
    report.ok = false;
    report.issues.push(issue('error', 'RESOURCES_ASSETS_NOT_FOUND', 'ไม่พบ resources.assets', 'resources.assets'));
  } else {
    report.issues.push(issue(
      'warning',
      'UNITY_ASSET_INTEGRITY_UNVERIFIED',
      'resources.assets พบแล้ว แต่ความสมบูรณ์ของ Unity asset ต้องตรวจด้วย Unity/UnityPy เพิ่มเติม',
      resources.path,
    ));
  }
  if (!report.files.some((file) => /^level\d+$/.test(path.basename(file.path).toLowerCase()))) {
    report.issues.push(issue('warning', 'LEVEL_ASSETS_NOT_FOUND', 'ไม่พบไฟล์ level* ใน DurangoV2_Data', 'DurangoV2_Data'));
  }
  report.ok = !report.issues.some((entry) => entry.severity === 'error');
  return report;
}

function parseMetadata(buffer, report) {
  if (!buffer) return { typed: null, raw: null, unknown: null };
  const raw = buffer.toString('utf8');
  try {
    const parsed = JSON.parse(raw);
    if (!isObject(parsed)) throw new Error('metadata root ต้องเป็น object');
    const known = new Set([
      'tile_count', 'is_cold_ocean', 'lake_type', 'river_type', 'ocean_type',
      'lake_biome', 'river_biome', 'ocean_biome', 'region_template', 'tile_set',
      'color_set', 'entry_points', 'landmarks', 'global_landmarks', 'indicators', 'time_zone',
    ]);
    const unknown = {};
    for (const [key, value] of Object.entries(parsed)) if (!known.has(key)) unknown[key] = value;
    return { typed: parsed, raw, unknown };
  } catch (error) {
    report.issues.push(issue('error', 'METADATA_PARSE_FAILED', `อ่าน info.yml ไม่ได้: ${error.message}`, 'info.yml'));
    return { typed: null, raw, unknown: null };
  }
}

function dimensionsFromMetadata(metadata, report) {
  const tileCount = metadata && metadata.tile_count;
  if (!Array.isArray(tileCount) || tileCount.length < 2 || !Number.isInteger(tileCount[0]) || !Number.isInteger(tileCount[1])) {
    report.issues.push(issue('error', 'INVALID_TILE_COUNT', 'info.yml ไม่มี tile_count เป็นจำนวนเต็ม 2 ค่า', 'info.yml'));
    return { width: 0, height: 0 };
  }
  const width = tileCount[0];
  const height = tileCount[1];
  if (width < MIN_TILE_COUNT || height < MIN_TILE_COUNT || width > MAX_TILE_COUNT || height > MAX_TILE_COUNT) {
    report.issues.push(issue('error', 'TILE_COUNT_OUT_OF_RANGE', `tile_count ต้องอยู่ระหว่าง ${MIN_TILE_COUNT} ถึง ${MAX_TILE_COUNT}`, 'info.yml', { width, height }));
  }
  if (width % TILE_SIZE !== 0 || height % TILE_SIZE !== 0) {
    report.issues.push(issue('error', 'TILE_COUNT_NOT_CHUNK_ALIGNED', 'ขนาดแมพต้องหารด้วย chunk size 16 ลงตัวก่อน export', 'info.yml', { width, height }));
  }
  return { width, height };
}

function expectedLayerLengths(width, height) {
  const vertices = (width + 1) * (height + 1);
  return {
    'whole.biomes': width * height,
    'whole.ocean': vertices,
    'whole.rivers': vertices * 3,
    'oceans.dm': width * height,
  };
}

function validateLength(name, data, expected, report) {
  if (!data) return;
  if (data.length !== expected) {
    report.issues.push(issue('error', 'INVALID_LAYER_LENGTH', `${name} มีขนาด ${data.length} แต่คาดว่า ${expected}`, name, { actual: data.length, expected }));
  }
}

function decodeGarden(buffer, width, height, report) {
  const records = [];
  if (!buffer) return records;
  if (buffer.length % 6 !== 0) {
    report.issues.push(issue('error', 'GARDEN_RECORD_ALIGNMENT', 'whole.garden ต้องมีขนาดหาร 6 ลงตัว', 'whole.garden'));
    return records;
  }
  for (let offset = 0; offset < buffer.length; offset += 6) {
    const x = buffer.readUInt16LE(offset);
    const y = buffer.readUInt16LE(offset + 2);
    const entityType = buffer.readUInt16LE(offset + 4);
    records.push({ x, y, entityType });
    if (x >= width || y >= height) {
      report.issues.push(issue('error', 'GARDEN_COORDINATE_OUT_OF_BOUNDS', `natural (${x},${y}) อยู่นอกขอบเขตแมพ`, 'whole.garden', { x, y }));
    }
  }
  return records;
}

function decodeLandmarks(buffer, width, height, report) {
  const records = [];
  if (!buffer) return records;
  if (buffer.length % 16 !== 0) {
    report.issues.push(issue('error', 'LANDMARK_RECORD_ALIGNMENT', 'whole.landmarks ต้องมีขนาดหาร 16 ลงตัว', 'whole.landmarks'));
    return records;
  }
  for (let offset = 0; offset < buffer.length; offset += 16) {
    const record = {
      x: buffer.readUInt16LE(offset),
      y: buffer.readUInt16LE(offset + 2),
      id: buffer.readUInt16LE(offset + 4),
      rotate: buffer.readUInt8(offset + 6),
      offsetX: buffer.readInt16LE(offset + 7),
      offsetY: buffer.readInt16LE(offset + 9),
      offsetZ: buffer.readInt16LE(offset + 11),
      scaleX: buffer.readUInt8(offset + 13),
      scaleY: buffer.readUInt8(offset + 14),
      scaleZ: buffer.readUInt8(offset + 15),
    };
    records.push(record);
    if (record.x >= width || record.y >= height) {
      report.issues.push(issue('error', 'LANDMARK_COORDINATE_OUT_OF_BOUNDS', `landmark (${record.x},${record.y}) อยู่นอกขอบเขตแมพ`, 'whole.landmarks', { x: record.x, y: record.y }));
    }
  }
  return records;
}

function encodeGarden(records) {
  const buffer = Buffer.alloc(records.length * 6);
  records.forEach((record, index) => {
    const offset = index * 6;
    buffer.writeUInt16LE(record.x, offset);
    buffer.writeUInt16LE(record.y, offset + 2);
    buffer.writeUInt16LE(record.entityType, offset + 4);
  });
  return buffer;
}

function encodeLandmarks(records) {
  const buffer = Buffer.alloc(records.length * 16);
  records.forEach((record, index) => {
    const offset = index * 16;
    buffer.writeUInt16LE(record.x, offset);
    buffer.writeUInt16LE(record.y, offset + 2);
    buffer.writeUInt16LE(record.id, offset + 4);
    buffer.writeUInt8(record.rotate || 0, offset + 6);
    buffer.writeInt16LE(record.offsetX || 0, offset + 7);
    buffer.writeInt16LE(record.offsetY || 0, offset + 9);
    buffer.writeInt16LE(record.offsetZ || 0, offset + 11);
    buffer.writeUInt8(record.scaleX || 0, offset + 13);
    buffer.writeUInt8(record.scaleY || 0, offset + 14);
    buffer.writeUInt8(record.scaleZ || 0, offset + 15);
  });
  return buffer;
}

function readTerrainSource(sourceDir, options = {}) {
  const report = { ok: true, issues: [], sourceDir: path.resolve(sourceDir), files: {}, hashes: {}, opaque: [], dimensions: null };
  if (!sourceDir || !fs.existsSync(sourceDir) || !fs.statSync(sourceDir).isDirectory()) {
    report.ok = false;
    report.issues.push(issue('error', 'TERRAIN_SOURCE_NOT_FOUND', 'ไม่พบโฟลเดอร์ terrain source', sourceDir || null));
    return { report, terrain: null };
  }

  const buffers = {};
  for (const name of TERRAIN_FILES) {
    const filePath = path.join(report.sourceDir, name);
    if (!fs.existsSync(filePath)) {
      report.issues.push(issue(name === 'info.yml' ? 'error' : 'warning', 'TERRAIN_FILE_MISSING', `ไม่พบ ${name}`, name));
      continue;
    }
    const data = readFileSafe(filePath, report, name);
    if (data) {
      buffers[name] = data;
      report.files[name] = { size: data.length, sha256: sha256Buffer(data) };
      report.hashes[name] = report.files[name].sha256;
    }
  }

  for (const name of OPAQUE_TERRAIN_FILES) {
    const filePath = path.join(report.sourceDir, name);
    if (fs.existsSync(filePath)) {
      try {
        const data = fs.readFileSync(filePath);
        report.opaque.push({ name, size: data.length, sha256: sha256Buffer(data), supported: false });
      } catch (error) {
        report.issues.push(issue('warning', 'OPAQUE_FILE_READ_FAILED', `อ่าน opaque layer ${name} ไม่ได้: ${error.message}`, name));
      }
    }
  }

  const metadata = parseMetadata(buffers['info.yml'], report);
  const dimensions = dimensionsFromMetadata(metadata.typed, report);
  report.dimensions = dimensions;
  const expected = expectedLayerLengths(dimensions.width, dimensions.height);
  for (const [name, expectedLength] of Object.entries(expected)) validateLength(name, buffers[name], expectedLength, report);

  const garden = decodeGarden(buffers['whole.garden'], dimensions.width, dimensions.height, report);
  const landmarks = decodeLandmarks(buffers['whole.landmarks'], dimensions.width, dimensions.height, report);
  const terrain = {
    mapId: options.mapId || path.basename(report.sourceDir),
    width: dimensions.width,
    height: dimensions.height,
    chunkSize: TILE_SIZE,
    worldUnitsPerTile: WORLD_UNITS_PER_TILE,
    metadata: metadata.typed,
    metadataRaw: metadata.raw,
    metadataUnknown: metadata.unknown,
    layers: {
      biomes: buffers['whole.biomes'] || null,
      ocean: buffers['whole.ocean'] || null,
      rivers: buffers['whole.rivers'] || null,
      coastDistance: buffers['oceans.dm'] || null,
    },
    garden,
    landmarks,
    opaque: report.opaque,
  };
  report.ok = !report.issues.some((entry) => entry.severity === 'error');
  return { report, terrain };
}

function clamp(value, min, max) {
  return Math.max(min, Math.min(max, value));
}

function extractChunk(data, width, height, channels, chunkX, chunkY, before, after) {
  if (!Buffer.isBuffer(data)) throw new TypeError('data must be a Buffer');
  const sourceWidth = channels === 1 && before === 1 ? width : width + 1;
  const outputWidth = TILE_SIZE + before + after;
  const output = Buffer.alloc(outputWidth * outputWidth * channels);
  const originX = chunkX * TILE_SIZE;
  const originY = chunkY * TILE_SIZE;
  for (let localY = -before; localY < TILE_SIZE + after; localY++) {
    for (let localX = -before; localX < TILE_SIZE + after; localX++) {
      const sourceX = clamp(originX + localX, 0, sourceWidth - 1);
      const sourceY = clamp(originY + localY, 0, (channels === 1 && before === 1 ? height : height + 1) - 1);
      const sourceOffset = (sourceX + sourceY * sourceWidth) * channels;
      const outputOffset = ((localX + before) + (localY + before) * outputWidth) * channels;
      data.copy(output, outputOffset, sourceOffset, sourceOffset + channels);
    }
  }
  return output;
}

function getTerrainChunk(terrain, chunkX, chunkY) {
  if (!terrain || !Number.isInteger(chunkX) || !Number.isInteger(chunkY)) throw new TypeError('terrain and integer chunk coordinates are required');
  return {
    biomes: terrain.layers.biomes ? extractChunk(terrain.layers.biomes, terrain.width, terrain.height, 1, chunkX, chunkY, 1, 1) : null,
    ocean: terrain.layers.ocean ? extractChunk(terrain.layers.ocean, terrain.width, terrain.height, 1, chunkX, chunkY, 0, 1) : null,
    rivers: terrain.layers.rivers ? extractChunk(terrain.layers.rivers, terrain.width, terrain.height, 3, chunkX, chunkY, 0, 1) : null,
    landmarks: terrain.landmarks.filter((record) => Math.floor(record.x / TILE_SIZE) === chunkX && Math.floor(record.y / TILE_SIZE) === chunkY),
    garden: terrain.garden.filter((record) => Math.floor(record.x / TILE_SIZE) === chunkX && Math.floor(record.y / TILE_SIZE) === chunkY),
  };
}

function setBiomeType(rawByte, biomeType) {
  if (!Number.isInteger(biomeType) || biomeType < 0 || biomeType > BIOME_TYPE_MASK) throw new RangeError('biome type must be 0..63');
  return (rawByte & BIOME_FLAGS_MASK) | (biomeType & BIOME_TYPE_MASK);
}

function applyBiomeBrush(biomes, width, height, options) {
  if (!Buffer.isBuffer(biomes)) throw new TypeError('biomes must be a Buffer');
  if (!Number.isInteger(width) || !Number.isInteger(height) || width <= 0 || height <= 0) {
    throw new RangeError('width and height must be positive integers');
  }
  if (biomes.length !== width * height) {
    throw new RangeError('biomes length must equal width * height');
  }
  const params = options || {};
  const x = params.x;
  const y = params.y;
  const radius = params.radius;
  const biomeType = params.biomeType;
  if (!Number.isInteger(x) || !Number.isInteger(y)) throw new TypeError('x and y must be integers');
  if (!Number.isFinite(radius) || radius < 0) throw new RangeError('radius must be a non-negative number');

  const r2 = radius * radius;
  let changed = 0;
  const minX = Math.max(0, Math.floor(x - radius));
  const maxX = Math.min(width - 1, Math.ceil(x + radius));
  const minY = Math.max(0, Math.floor(y - radius));
  const maxY = Math.min(height - 1, Math.ceil(y + radius));

  for (let py = minY; py <= maxY; py++) {
    for (let px = minX; px <= maxX; px++) {
      const dx = px - x;
      const dy = py - y;
      if ((dx * dx) + (dy * dy) > r2) continue;
      const index = px + py * width;
      const next = setBiomeType(biomes[index], biomeType);
      if (next !== biomes[index]) {
        biomes[index] = next;
        changed += 1;
      }
    }
  }
  return { changed };
}

function encodeSignedByte(n) {
  if (!Number.isInteger(n) || n < -128 || n > 127) throw new RangeError('signed byte must be -128..127');
  return n & 0xff;
}

function decodeSignedByte(b) {
  const v = Number(b) & 0xff;
  return v > 127 ? v - 256 : v;
}

function applyCoastBrush(buf, width, height, options) {
  if (!Buffer.isBuffer(buf)) throw new TypeError('buf must be a Buffer');
  if (!Number.isInteger(width) || !Number.isInteger(height) || width <= 0 || height <= 0) {
    throw new RangeError('width and height must be positive integers');
  }
  if (buf.length !== width * height) {
    throw new RangeError('buf length must equal width * height');
  }
  const params = options || {};
  const x = params.x;
  const y = params.y;
  const radius = params.radius;
  const value = params.value;
  if (!Number.isInteger(x) || !Number.isInteger(y)) throw new TypeError('x and y must be integers');
  if (!Number.isFinite(radius) || radius < 0) throw new RangeError('radius must be a non-negative number');

  const encoded = encodeSignedByte(value);
  const r2 = radius * radius;
  let changed = 0;
  const minX = Math.max(0, Math.floor(x - radius));
  const maxX = Math.min(width - 1, Math.ceil(x + radius));
  const minY = Math.max(0, Math.floor(y - radius));
  const maxY = Math.min(height - 1, Math.ceil(y + radius));

  for (let py = minY; py <= maxY; py++) {
    for (let px = minX; px <= maxX; px++) {
      const dx = px - x;
      const dy = py - y;
      if ((dx * dx) + (dy * dy) > r2) continue;
      const index = px + py * width;
      if (buf[index] !== encoded) {
        buf[index] = encoded;
        changed += 1;
      }
    }
  }
  return { changed };
}

function validateTerrain(terrain) {
  const report = { ok: true, issues: [] };
  if (!terrain || !Number.isInteger(terrain.width) || !Number.isInteger(terrain.height)) {
    report.issues.push(issue('error', 'INVALID_TERRAIN_MODEL', 'terrain model ไม่ถูกต้อง'));
    report.ok = false;
    return report;
  }
  const expected = expectedLayerLengths(terrain.width, terrain.height);
  validateLength('whole.biomes', terrain.layers && terrain.layers.biomes, expected['whole.biomes'], report);
  validateLength('whole.ocean', terrain.layers && terrain.layers.ocean, expected['whole.ocean'], report);
  validateLength('whole.rivers', terrain.layers && terrain.layers.rivers, expected['whole.rivers'], report);
  validateLength('oceans.dm', terrain.layers && terrain.layers.coastDistance, expected['oceans.dm'], report);
  for (const record of terrain.garden || []) {
    if (!Number.isInteger(record.x) || !Number.isInteger(record.y) || record.x < 0 || record.y < 0 || record.x >= terrain.width || record.y >= terrain.height) {
      report.issues.push(issue('error', 'GARDEN_COORDINATE_OUT_OF_BOUNDS', 'garden coordinate อยู่นอกขอบเขต'));
    }
  }
  for (const record of terrain.landmarks || []) {
    if (!Number.isInteger(record.x) || !Number.isInteger(record.y) || record.x < 0 || record.y < 0 || record.x >= terrain.width || record.y >= terrain.height) {
      report.issues.push(issue('error', 'LANDMARK_COORDINATE_OUT_OF_BOUNDS', 'landmark coordinate อยู่นอกขอบเขต'));
    }
  }
  report.ok = !report.issues.some((entry) => entry.severity === 'error');
  return report;
}

function createProjectModel(terrain, sourceReport, options = {}) {
  if (!terrain) throw new TypeError('terrain is required');
  const now = options.exportTime || new Date().toISOString();
  const sourceHashes = sourceReport && sourceReport.hashes ? clone(sourceReport.hashes) : {};
  return {
    schema: 1,
    mapId: terrain.mapId,
    mapVersion: options.mapVersion || '0.1.0',
    sourceGameFolder: options.sourceGameFolder || null,
    sourceTerrainFolder: sourceReport ? sourceReport.sourceDir : null,
    unityVersion: options.unityVersion || '2017.4.34f1',
    editorVersion: options.editorVersion || 'map-editor-core/0.1.0',
    sourceFileHashes: sourceHashes,
    exportTime: now,
    dimensions: { width: terrain.width, height: terrain.height, chunkSize: TILE_SIZE },
    metadata: terrain.metadata,
    metadataUnknown: terrain.metadataUnknown,
    layers: {
      coastDistanceFile: 'terrain/coast-distance.bin',
      opaque: terrain.opaque,
    },
    objectsFile: 'objects.json',
    spawnPointsFile: 'spawn-points.json',
  };
}

function ensureDir(dirPath) {
  fs.mkdirSync(dirPath, { recursive: true });
}

function writeJsonAtomic(filePath, value) {
  const tempPath = `${filePath}.tmp-${process.pid}-${Date.now()}`;
  fs.writeFileSync(tempPath, JSON.stringify(value, null, 2) + '\n', 'utf8');
  fs.renameSync(tempPath, filePath);
}

function copyIfPresent(sourceDir, fileName, targetPath) {
  const sourcePath = path.join(sourceDir, fileName);
  if (fs.existsSync(sourcePath)) fs.copyFileSync(sourcePath, targetPath);
}

class ProjectStore {
  static save(projectDir, terrain, sourceReport, options = {}) {
    const validation = validateTerrain(terrain);
    if (!validation.ok) {
      const error = new Error('ไม่สามารถบันทึก project ที่ validation ไม่ผ่าน');
      error.report = validation;
      throw error;
    }
    ensureDir(projectDir);
    ensureDir(path.join(projectDir, 'terrain'));
    ensureDir(path.join(projectDir, 'backup'));
    ensureDir(path.join(projectDir, 'exports'));
    const model = createProjectModel(terrain, sourceReport, options);
    writeJsonAtomic(path.join(projectDir, 'project.json'), model);
    writeJsonAtomic(path.join(projectDir, 'map.json'), {
      schema: 1,
      mapId: terrain.mapId,
      width: terrain.width,
      height: terrain.height,
      chunkSize: TILE_SIZE,
      metadata: terrain.metadata,
      metadataRaw: terrain.metadataRaw,
      metadataUnknown: terrain.metadataUnknown,
    });
    writeJsonAtomic(path.join(projectDir, 'objects.json'), {
      schema: 1,
      garden: terrain.garden,
      landmarks: terrain.landmarks,
      pois: options.pois || [],
    });
    writeJsonAtomic(path.join(projectDir, 'spawn-points.json'), options.spawnPoints || {
      entryPoints: terrain.metadata && terrain.metadata.entry_points ? terrain.metadata.entry_points : [],
      player: [],
      animalZones: [],
    });
    const layers = terrain.layers;
    if (layers.biomes) fs.writeFileSync(path.join(projectDir, 'terrain', 'biome.bin'), layers.biomes);
    if (layers.ocean) fs.writeFileSync(path.join(projectDir, 'terrain', 'ocean.bin'), layers.ocean);
    if (layers.rivers) fs.writeFileSync(path.join(projectDir, 'terrain', 'river.bin'), layers.rivers);
    if (layers.coastDistance) fs.writeFileSync(path.join(projectDir, 'terrain', 'coast-distance.bin'), layers.coastDistance);
    if (terrain.metadataRaw !== null) fs.writeFileSync(path.join(projectDir, 'terrain', 'info.yml'), terrain.metadataRaw, 'utf8');
    const sourceDir = sourceReport && sourceReport.sourceDir;
    if (sourceDir) {
      for (const name of OPAQUE_TERRAIN_FILES) copyIfPresent(sourceDir, name, path.join(projectDir, 'terrain', name));
    }
    return { projectDir: path.resolve(projectDir), model, validation };
  }

  static load(projectDir) {
    const readJson = (name) => JSON.parse(fs.readFileSync(path.join(projectDir, name), 'utf8'));
    const project = readJson('project.json');
    const map = readJson('map.json');
    const objects = readJson('objects.json');
    const spawnPoints = readJson('spawn-points.json');
    const terrainDir = path.join(projectDir, 'terrain');
    const readBuffer = (name) => {
      const filePath = path.join(terrainDir, name);
      return fs.existsSync(filePath) ? fs.readFileSync(filePath) : null;
    };
    const infoRaw = fs.existsSync(path.join(terrainDir, 'info.yml')) ? fs.readFileSync(path.join(terrainDir, 'info.yml'), 'utf8') : null;
    return {
      project,
      terrain: {
        mapId: project.mapId,
        width: map.width,
        height: map.height,
        chunkSize: map.chunkSize,
        worldUnitsPerTile: WORLD_UNITS_PER_TILE,
        metadata: map.metadata,
        metadataRaw: infoRaw,
        metadataUnknown: map.metadataUnknown || {},
        layers: {
          biomes: readBuffer('biome.bin'),
          ocean: readBuffer('ocean.bin'),
          rivers: readBuffer('river.bin'),
          coastDistance: readBuffer('coast-distance.bin'),
        },
        garden: objects.garden || [],
        landmarks: objects.landmarks || [],
        opaque: project.layers && project.layers.opaque ? project.layers.opaque : [],
      },
      spawnPoints,
    };
  }
}

module.exports = {
  TILE_SIZE,
  WORLD_UNITS_PER_TILE,
  MIN_TILE_COUNT,
  MAX_TILE_COUNT,
  BIOME_FLAGS_MASK,
  BIOME_TYPE_MASK,
  TERRAIN_FILES,
  OPAQUE_TERRAIN_FILES,
  scanGameFolder,
  readTerrainSource,
  validateTerrain,
  expectedLayerLengths,
  extractChunk,
  getTerrainChunk,
  decodeGarden,
  encodeGarden,
  decodeLandmarks,
  encodeLandmarks,
  setBiomeType,
  applyBiomeBrush,
  encodeSignedByte,
  decodeSignedByte,
  applyCoastBrush,
  sha256File,
  sha256Buffer,
  createProjectModel,
  ProjectStore,
};
