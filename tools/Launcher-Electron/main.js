// DinoWorld Launcher — Electron main process
// หน้าต่าง frameless ธีมดำ-ชมพู + preload ให้ renderer เรียกฟังก์ชันเปิดเกม/อ่านไฟล์ผ่าน IPC
const { app, BrowserWindow, ipcMain, shell, dialog } = require('electron');
const path = require('path');
const fs = require('fs');
const { spawn, execFile } = require('child_process');
const http = require('http');
const https = require('https');
const crypto = require('crypto');
let mapEditorCore;
function resolveImportTerrainArg() {
  const idx = process.argv.findIndex((a) => typeof a === 'string' && a.startsWith('--import-terrain='));
  if (idx < 0) return null;
  let value = process.argv[idx].slice('--import-terrain='.length);
  if ((value.startsWith('"') && value.endsWith('"')) || (value.startsWith("'") && value.endsWith("'"))) {
    value = value.slice(1, -1);
  }
  const tryPath = (p) => {
    try { return p && fs.existsSync(p) && fs.statSync(p).isDirectory() ? path.resolve(p) : null; } catch (_) { return null; }
  };
  let hit = tryPath(value);
  if (hit) return hit;
  const parts = [value];
  for (let i = idx + 1; i < process.argv.length; i++) {
    const next = process.argv[i];
    if (!next || String(next).startsWith('--')) break;
    parts.push(next);
    hit = tryPath(parts.join(' '));
    if (hit) return hit;
  }
  return value ? path.resolve(parts.join(' ')) : null;
}

function loadMapEditorCore() {
  const candidates = [
    path.join(__dirname, '..', 'MapEditor', 'lib', 'map-editor-core'),
    path.join(__dirname, 'MapEditor', 'lib', 'map-editor-core'),
    path.join(process.resourcesPath || '', 'MapEditor', 'lib', 'map-editor-core'),
    path.join(path.dirname(process.execPath), 'resources', 'MapEditor', 'lib', 'map-editor-core'),
  ];
  let lastError = null;
  for (const candidate of candidates) {
    try {
      if (!fs.existsSync(candidate + '.js') && !fs.existsSync(candidate)) continue;
      const mod = require(candidate);
      console.log('[map-editor] core loaded from', candidate);
      return mod;
    } catch (error) {
      lastError = error;
    }
  }
  console.warn('[map-editor] core unavailable:', lastError && lastError.message, 'tried', candidates);
  return null;
}
mapEditorCore = loadMapEditorCore();

let win;
let mapEditorWin;

app.whenReady().then(() => {
  win = new BrowserWindow({
    width: 980,
    height: 620,
    resizable: true,
    frame: false,                    // หน้าต่างไร้กรอบ — titlebar วาดเองใน HTML (เหมือน mockup)
    backgroundColor: '#07070b',
    icon: path.join(__dirname, 'icon.png'),
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
      contextIsolation: true,
      nodeIntegration: false,
    },
  });
  win.setMenuBarVisibility(false);
  win.loadFile('index.html');
  if (process.argv.includes('--map-editor')) { setTimeout(() => win.webContents.executeJavaScript('window.launcher.openMapEditor()'), 700); }
});

// ───── ควบคุมหน้าต่าง (ปุ่ม – ✕ ใน titlebar) ─────
ipcMain.on('win-minimize', () => win?.minimize());
ipcMain.on('win-close', () => win?.close());  
ipcMain.handle('open-map-editor', () => {
  if (mapEditorWin && !mapEditorWin.isDestroyed()) {
    mapEditorWin.focus();
    return true;
  }
  mapEditorWin = new BrowserWindow({
    width: 1500,
    height: 920,
    minWidth: 1080,
    minHeight: 680,
    resizable: true,
    frame: false,
    backgroundColor: '#0a0d12',
    icon: path.join(__dirname, 'icon.png'),
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
      contextIsolation: true,
      nodeIntegration: false,
    },
  });
  mapEditorWin.setMenuBarVisibility(false);
  mapEditorWin.loadFile(path.join(__dirname, 'map-editor.html'));
  const importFolder = resolveImportTerrainArg();
  if (importFolder && mapEditorCore) {
    mapEditorWin.webContents.once('did-finish-load', () => {
      setTimeout(() => {
        mapEditorWin.webContents.executeJavaScript('importTerrain()').catch((error) => {
          console.error('[map-editor] auto-import failed:', error);
        });
      }, 400);
    });
  }
  mapEditorWin.on('closed', () => { mapEditorWin = null; });
  return true;
});
ipcMain.on('editor-minimize', () => mapEditorWin?.minimize());
ipcMain.on('editor-close', () => mapEditorWin?.close());

const projectRoot = path.resolve(__dirname, '..', '..');
const editorMapsDir = path.join(projectRoot, 'maps');
ipcMain.handle('editor-open-map', async () => {
  const result = await dialog.showOpenDialog(mapEditorWin, {
    title: 'เปิดไฟล์แผนที่',
    defaultPath: editorMapsDir,
    filters: [{ name: 'DinoWorld Map', extensions: ['json'] }],
    properties: ['openFile'],
  });
  if (result.canceled || !result.filePaths[0]) return { canceled: true };
  try {
    const filePath = result.filePaths[0];
    return { canceled: false, filePath, data: JSON.parse(fs.readFileSync(filePath, 'utf8')) };
  } catch (error) {
    return { canceled: false, error: String(error.message || error) };
  }
});
ipcMain.handle('editor-save-map', async (_event, payload) => {
  const result = await dialog.showSaveDialog(mapEditorWin, {
    title: payload?.filePath ? 'บันทึกแผนที่' : 'บันทึกแผนที่ใหม่',
    defaultPath: payload?.filePath || path.join(editorMapsDir, (payload?.data?.name || 'new-map') + '.json'),
    filters: [{ name: 'DinoWorld Map', extensions: ['json'] }],
  });
  if (result.canceled || !result.filePath) return { canceled: true };
  try {
    fs.mkdirSync(path.dirname(result.filePath), { recursive: true });
    fs.writeFileSync(result.filePath, JSON.stringify(payload.data, null, 2), 'utf8');
    return { canceled: false, filePath: result.filePath };
  } catch (error) {
    return { canceled: false, error: String(error.message || error) };
  }
});
ipcMain.handle('editor-export-image', async (_event, payload) => {
  const result = await dialog.showSaveDialog(mapEditorWin, {
    title: 'ส่งออกภาพแผนที่',
    defaultPath: path.join(editorMapsDir, (payload?.name || 'map') + '.png'),
    filters: [{ name: 'PNG Image', extensions: ['png'] }],
  });
  if (result.canceled || !result.filePath) return { canceled: true };
  try {
    const base64 = String(payload.dataUrl || '').replace(/^data:image\/png;base64,/, '');
    fs.mkdirSync(path.dirname(result.filePath), { recursive: true });
    fs.writeFileSync(result.filePath, Buffer.from(base64, 'base64'));
    return { canceled: false, filePath: result.filePath };
  } catch (error) {
    return { canceled: false, error: String(error.message || error) };
  }
});

function mapEditorUnavailable() {
  return { canceled: false, error: 'ไม่พบ MapEditor core' };
}

const WRITABLE_TERRAIN_FILES = Object.freeze([
  'whole.biomes',
  'oceans.dm',
  'whole.garden',
  'whole.landmarks',
  'info.yml',
]);

let lastImport = { terrainFolder: null, hashes: {}, mapId: null };

function serializeTerrainImport(result) {
  if (!result || !result.terrain) return result;
  const terrain = result.terrain;
  return {
    report: result.report,
    terrain: {
      mapId: terrain.mapId,
      width: terrain.width,
      height: terrain.height,
      chunkSize: terrain.chunkSize,
      worldUnitsPerTile: terrain.worldUnitsPerTile,
      metadata: terrain.metadata,
      metadataRaw: terrain.metadataRaw,
      metadataUnknown: terrain.metadataUnknown,
      layers: {
        biomes: terrain.layers?.biomes?.toString('base64') || null,
        ocean: terrain.layers?.ocean?.toString('base64') || null,
        rivers: terrain.layers?.rivers?.toString('base64') || null,
        coastDistance: terrain.layers?.coastDistance?.toString('base64') || null,
      },
      garden: terrain.garden,
      landmarks: terrain.landmarks,
      opaque: terrain.opaque,
    },
  };
}

function bufferFromBase64(value) {
  if (value == null || value === '') return null;
  if (Buffer.isBuffer(value)) return value;
  return Buffer.from(String(value), 'base64');
}

function deserializeTerrain(payload) {
  if (!payload) return null;
  const terrain = payload.terrain || payload;
  if (!terrain || typeof terrain !== 'object') return null;
  const layers = terrain.layers || {};
  return {
    mapId: terrain.mapId,
    width: terrain.width,
    height: terrain.height,
    chunkSize: terrain.chunkSize,
    worldUnitsPerTile: terrain.worldUnitsPerTile,
    metadata: terrain.metadata,
    metadataRaw: terrain.metadataRaw,
    metadataUnknown: terrain.metadataUnknown,
    layers: {
      biomes: bufferFromBase64(layers.biomes),
      ocean: bufferFromBase64(layers.ocean),
      rivers: bufferFromBase64(layers.rivers),
      coastDistance: bufferFromBase64(layers.coastDistance),
    },
    garden: Array.isArray(terrain.garden) ? terrain.garden : [],
    landmarks: Array.isArray(terrain.landmarks) ? terrain.landmarks : [],
    opaque: terrain.opaque || [],
  };
}

function defaultExportDir(payload, terrain) {
  if (payload && typeof payload.destDir === 'string' && payload.destDir) return payload.destDir;
  if (payload && typeof payload.terrainFolder === 'string' && payload.terrainFolder) return payload.terrainFolder;
  if (lastImport.terrainFolder) return lastImport.terrainFolder;
  const mapId = (terrain && terrain.mapId) || (payload && payload.mapId) || lastImport.mapId;
  if (mapId) return path.join(projectRoot, 'server', 'data', 'terrains', 'extracted', String(mapId));
  return path.join(projectRoot, 'server', 'data', 'terrains', 'extracted');
}

function filesToWrite(terrain) {
  const files = [];
  const layers = (terrain && terrain.layers) || {};
  if (Buffer.isBuffer(layers.biomes)) files.push('whole.biomes');
  if (Buffer.isBuffer(layers.coastDistance)) files.push('oceans.dm');
  if (Array.isArray(terrain.garden)) files.push('whole.garden');
  if (Array.isArray(terrain.landmarks)) files.push('whole.landmarks');
  if (terrain.metadataRaw != null || terrain.metadata != null) files.push('info.yml');
  return files.filter((name) => WRITABLE_TERRAIN_FILES.includes(name));
}

function checkHashDrift(destDir, sourceHashes) {
  const files = [];
  if (!destDir || !sourceHashes || typeof sourceHashes !== 'object') {
    return { drifted: false, files };
  }
  for (const [name, expected] of Object.entries(sourceHashes)) {
    const filePath = path.join(destDir, name);
    if (!fs.existsSync(filePath) || !fs.statSync(filePath).isFile()) continue;
    const actual = mapEditorCore.sha256File(filePath);
    if (String(actual).toLowerCase() !== String(expected).toLowerCase()) {
      files.push({ name, expected, actual });
    }
  }
  return { drifted: files.length > 0, files };
}

function listBackupFolders(destDir) {
  const backupRoot = path.join(path.resolve(destDir), 'backup');
  if (!fs.existsSync(backupRoot) || !fs.statSync(backupRoot).isDirectory()) return [];
  return fs.readdirSync(backupRoot, { withFileTypes: true })
    .filter((entry) => entry.isDirectory())
    .map((entry) => {
      const dir = path.join(backupRoot, entry.name);
      const files = fs.readdirSync(dir).filter((name) => {
        try { return fs.statSync(path.join(dir, name)).isFile(); } catch { return false; }
      });
      let mtime = null;
      try { mtime = fs.statSync(dir).mtime.toISOString(); } catch { mtime = null; }
      return { id: entry.name, backupDir: dir, files, mtime };
    })
    .sort((a, b) => String(b.id).localeCompare(String(a.id)));
}

ipcMain.handle('editor-scan-game', async () => {
  if (!mapEditorCore) return mapEditorUnavailable();
  const result = await dialog.showOpenDialog(mapEditorWin, {
    title: 'เลือกโฟลเดอร์เกม Durango',
    properties: ['openDirectory'],
  });
  if (result.canceled || !result.filePaths[0]) return { canceled: true };
  try {
    return { canceled: false, gameFolder: result.filePaths[0], report: mapEditorCore.scanGameFolder(result.filePaths[0]) };
  } catch (error) {
    return { canceled: false, error: String(error.message || error) };
  }
});

ipcMain.handle('editor-import-terrain', async () => {
  if (!mapEditorCore) return mapEditorUnavailable();
  let chosen = resolveImportTerrainArg();
    if (!chosen) {
      const result = await dialog.showOpenDialog(mapEditorWin, {
        title: 'เลือกโฟลเดอร์ terrain ที่สกัดแล้ว',
        defaultPath: path.join(projectRoot, 'server', 'bin', 'Debug', 'net9.0', 'data', 'terrains', 'extracted'),
        properties: ['openDirectory'],
      });
      if (result.canceled || !result.filePaths[0]) return { canceled: true };
      chosen = result.filePaths[0];
    }
    try {
      const infoYml = path.join(chosen, 'info.yml');
      const biomesFile = path.join(chosen, 'whole.biomes');
      if (!fs.existsSync(infoYml) || !fs.existsSync(biomesFile)) {
        return {
          canceled: false,
          error: 'โฟลเดอร์นี้ไม่ใช่ terrain map — เลือกโฟลเดอร์ย่อย เช่น ri35te (ต้องมี info.yml และ whole.biomes) ไม่ใช่โฟลเดอร์ extracted',
        };
      }
      const imported = mapEditorCore.readTerrainSource(chosen);
      if (!imported || !imported.terrain) {
        const details = ((imported && imported.report && imported.report.issues) || [])
          .map((x) => (x.severity || 'error').toUpperCase() + ': ' + (x.message || x.code || ''))
          .join('\n');
        return { canceled: false, error: details || ('ไม่พบข้อมูล terrain ใน ' + chosen) };
      }
      lastImport = {
        terrainFolder: chosen,
        hashes: (imported.report && imported.report.hashes) || {},
        mapId: imported.terrain && imported.terrain.mapId,
      };
      return { canceled: false, terrainFolder: chosen, ...serializeTerrainImport(imported) };
  } catch (error) {
    return { canceled: false, error: String(error.message || error) };
  }
});

ipcMain.handle('editor-export-report', async (_event, payload) => {
  const result = await dialog.showSaveDialog(mapEditorWin, {
    title: 'ส่งออกรายงานการนำเข้า',
    defaultPath: path.join(editorMapsDir, 'terrain-import-report.json'),
    filters: [{ name: 'JSON Report', extensions: ['json'] }],
  });
  if (result.canceled || !result.filePath) return { canceled: true };
  try {
    fs.mkdirSync(path.dirname(result.filePath), { recursive: true });
    fs.writeFileSync(result.filePath, JSON.stringify(payload || {}, null, 2) + '\\n', 'utf8');
    return { canceled: false, filePath: result.filePath };
  } catch (error) {
    return { canceled: false, error: String(error.message || error) };
  }
});

ipcMain.handle('editor-clear-cache', () => ({ canceled: false, cleared: false, message: 'ระยะ read-only ยังไม่มี cache ที่ต้องลบ' }));

ipcMain.handle('editor-save-project', async (_event, payload) => {
  if (!mapEditorCore) return mapEditorUnavailable();
  try {
    const terrain = deserializeTerrain(payload);
    if (!terrain) return { canceled: false, error: 'terrain is required' };
    let projectDir = payload && payload.projectDir;
    if (!projectDir) {
      const picked = await dialog.showOpenDialog(mapEditorWin, {
        title: 'เลือกโฟลเดอร์ Map Project',
        defaultPath: editorMapsDir,
        properties: ['openDirectory', 'createDirectory'],
      });
      if (picked.canceled || !picked.filePaths[0]) return { canceled: true };
      projectDir = picked.filePaths[0];
    }
    const sourceReport = {
      sourceDir: (payload && payload.terrainFolder) || lastImport.terrainFolder,
      hashes: (payload && payload.sourceHashes) || lastImport.hashes || {},
    };
    const saved = mapEditorCore.ProjectStore.save(projectDir, terrain, sourceReport, {
      sourceGameFolder: payload && payload.sourceGameFolder,
      mapVersion: (payload && payload.mapVersion) || 0,
    });
    return { canceled: false, ok: true, projectDir: saved.projectDir, model: saved.model, validation: saved.validation };
  } catch (error) {
    return { canceled: false, ok: false, error: String(error.message || error), report: error.report || null };
  }
});

ipcMain.handle('editor-export-terrain', async (_event, payload) => {
  if (!mapEditorCore) return mapEditorUnavailable();
  try {
    const terrain = deserializeTerrain(payload);
    if (!terrain) return { canceled: false, ok: false, error: 'terrain is required' };
    const destDir = defaultExportDir(payload, terrain);
    const confirm = !!(payload && payload.confirm === true);
    const force = !!(payload && payload.force === true);
    const sourceHashes = (payload && payload.sourceHashes) || lastImport.hashes || {};
    const validation = mapEditorCore.validateExport(terrain);
    const hashDrift = checkHashDrift(destDir, sourceHashes);
    const planned = filesToWrite(terrain);
    if (!confirm) {
      const core = mapEditorCore.exportTerrain({ destDir, terrain, confirm: false });
      return {
        canceled: false,
        ok: validation.ok,
        dryRun: true,
        destDir: path.resolve(destDir),
        filesToWrite: planned,
        validation,
        hashDrift,
        core,
      };
    }
    if (!validation.ok) {
      return { canceled: false, ok: false, destDir: path.resolve(destDir), validation, issues: validation.issues };
    }
    if (hashDrift.drifted && !force) {
      return {
        canceled: false,
        ok: false,
        error: 'SOURCE_HASH_DRIFT',
        destDir: path.resolve(destDir),
        hashDrift,
        validation,
      };
    }
    const result = mapEditorCore.exportTerrain({ destDir, terrain, confirm: true });
    return { canceled: false, ...result, validation, hashDrift };
  } catch (error) {
    return { canceled: false, ok: false, error: String(error.message || error) };
  }
});

ipcMain.handle('editor-list-backups', async (_event, payload) => {
  if (!mapEditorCore) return mapEditorUnavailable();
  try {
    const destDir = defaultExportDir(payload, payload && payload.terrain);
    const backups = listBackupFolders(destDir);
    return { canceled: false, ok: true, destDir: path.resolve(destDir), backups };
  } catch (error) {
    return { canceled: false, ok: false, error: String(error.message || error) };
  }
});

ipcMain.handle('editor-rollback-terrain', async (_event, payload) => {
  if (!mapEditorCore) return mapEditorUnavailable();
  try {
    const destDir = defaultExportDir(payload, payload && payload.terrain);
    const result = mapEditorCore.rollbackTerrain({
      destDir,
      backupDir: payload && payload.backupDir,
      backupId: payload && payload.backupId,
    });
    return { canceled: false, ...result };
  } catch (error) {
    return { canceled: false, ok: false, error: String(error.message || error) };
  }
});

function catchMutate(fn) {
  try { return fn(); }
  catch (error) { return { ok: false, error: String(error.message || error) }; }
}

ipcMain.handle('editor-apply-biome-brush', async (_event, payload) => {
  if (!mapEditorCore) return mapEditorUnavailable();
  return catchMutate(() => {
    const pld = payload || {};
    const biomes = bufferFromBase64(pld.biomes);
    const result = mapEditorCore.applyBiomeBrush(biomes, pld.width, pld.height, {
      x: pld.x, y: pld.y, radius: pld.radius, biomeType: pld.biomeType,
    });
    return { ok: true, changed: result.changed, biomes: biomes.toString('base64') };
  });
});

ipcMain.handle('editor-apply-coast-brush', async (_event, payload) => {
  if (!mapEditorCore) return mapEditorUnavailable();
  return catchMutate(() => {
    const pld = payload || {};
    const buf = bufferFromBase64(pld.coastDistance || pld.buf);
    const result = mapEditorCore.applyCoastBrush(buf, pld.width, pld.height, {
      x: pld.x, y: pld.y, radius: pld.radius, value: pld.value,
    });
    return { ok: true, changed: result.changed, coastDistance: buf.toString('base64') };
  });
});

ipcMain.handle('editor-place-garden', async (_event, payload) => {
  if (!mapEditorCore) return mapEditorUnavailable();
  return catchMutate(() => {
    const p = payload || {};
    const records = Array.isArray(p.records) ? p.records.map((row) => ({ ...row })) : [];
    try {
      mapEditorCore.placeGarden(records, p.width, p.height, { x: p.x, y: p.y, entityType: p.entityType });
      return { ok: true, records, conflict: false };
    } catch (error) {
      const conflict = /occupied|already/i.test(String(error.message || error));
      return { ok: false, conflict, error: String(error.message || error), records: p.records || [] };
    }
  });
});

ipcMain.handle('editor-erase-garden', async (_event, payload) => {
  if (!mapEditorCore) return mapEditorUnavailable();
  return catchMutate(() => {
    const p = payload || {};
    const records = Array.isArray(p.records) ? p.records.map((row) => ({ ...row })) : [];
    const result = mapEditorCore.eraseGarden(records, { x: p.x, y: p.y });
    return { ok: true, records: result.records, removed: result.removed };
  });
});

ipcMain.handle('editor-place-landmark', async (_event, payload) => {
  if (!mapEditorCore) return mapEditorUnavailable();
  return catchMutate(() => {
    const p = payload || {};
    const records = Array.isArray(p.records) ? p.records.map((row) => ({ ...row })) : [];
    try {
      mapEditorCore.placeLandmark(records, p.width, p.height, p);
      return { ok: true, records };
    } catch (error) {
      return { ok: false, error: String(error.message || error), records: p.records || [] };
    }
  });
});

ipcMain.handle('editor-move-landmark', async (_event, payload) => {
  if (!mapEditorCore) return mapEditorUnavailable();
  return catchMutate(() => {
    const p = payload || {};
    const records = Array.isArray(p.records) ? p.records.map((row) => ({ ...row })) : [];
    mapEditorCore.moveLandmark(records, p.width, p.height, p);
    return { ok: true, records };
  });
});

ipcMain.handle('editor-erase-landmark', async (_event, payload) => {
  if (!mapEditorCore) return mapEditorUnavailable();
  return catchMutate(() => {
    const p = payload || {};
    const records = Array.isArray(p.records) ? p.records.map((row) => ({ ...row })) : [];
    const result = mapEditorCore.eraseLandmarkAt(records, { x: p.x, y: p.y });
    return { ok: true, records: result.records, removed: result.removed };
  });
});

ipcMain.handle('editor-build-occupancy', async (_event, payload) => {
  if (!mapEditorCore) return mapEditorUnavailable();
  return catchMutate(() => {
    const p = payload || {};
    const grid = mapEditorCore.buildOccupancy({
      width: p.width, height: p.height, garden: p.garden || [], landmarks: p.landmarks || [],
      artifacts: p.artifacts || [], padTiles: p.padTiles || 0,
    });
    let blocked = 0;
    for (let i = 0; i < grid.length; i++) if (grid[i]) blocked += 1;
    return { ok: true, occupancy: grid.toString('base64'), blocked };
  });
});

// ───── path helpers ─────
const gameDir = (() => {
  // portable app บน Windows แตกตัวเองไว้ temp; PORTABLE_EXECUTABLE_DIR คือโฟลเดอร์ที่ผู้ใช้วาง exe จริง
  const self = process.env.PORTABLE_EXECUTABLE_DIR || path.dirname(app.getPath('exe'));
  if (fs.existsSync(path.join(self, 'DurangoV2.exe'))) return self;
  // ตอน dev รันจาก tools/Launcher-Electron → ใช้ชุดทดสอบล่าสุด
  const dev = 'C:\\Users\\thana\\Desktop\\Durango Opencode\\dist\\DurangoTH';
  return fs.existsSync(dev) ? dev : self;
})();
const gameExe = path.join(gameDir, 'DurangoV2.exe');

ipcMain.handle('get-state', () => ({
  gameDir,
  hasGame: fs.existsSync(gameExe),
  localVersion: readFileOrNull(path.join(gameDir, 'version.txt'))?.trim() || null,
  server: readSettings().server,
}));

// เรียก HTTP จาก main process เพื่อไม่ให้ Chromium file:// ติด CORS เมื่อเซิร์ฟอยู่เครื่อง Linux/LAN
ipcMain.handle('http-json', async (_event, url) => {
  try {
    const body = await requestText(url);
    return { ok: true, status: 200, data: JSON.parse(body) };
  } catch (error) {
    return { ok: false, error: String(error.message || error) };
  }
});

function requestText(url) {
  return new Promise((resolve, reject) => {
    const transport = url.startsWith('https:') ? https : http;
    const req = transport.get(url, { headers: { 'User-Agent': 'DinoWorldLauncher/1.0' } }, (res) => {
      let body = '';
      res.setEncoding('utf8');
      res.on('data', (chunk) => { body += chunk; });
      res.on('end', () => {
        if (res.statusCode < 200 || res.statusCode >= 300) {
          reject(new Error('HTTP ' + res.statusCode));
        } else {
          resolve(body);
        }
      });
    });
    req.setTimeout(8000, () => req.destroy(new Error('หมดเวลาการเชื่อมต่อ')));
    req.on('error', reject);
  });
}

function readFileOrNull(p) { try { return fs.readFileSync(p, 'utf8'); } catch { return null; } }

// ───── settings ─────
const settingsPath = () => path.join(gameDir, 'launcher_settings.json');
function readSettings() {
  try { return JSON.parse(fs.readFileSync(settingsPath(), 'utf8')); }
  catch { return { server: '127.0.0.1:8190', autoPatch: true }; }
}
ipcMain.handle('save-settings', (_e, s) => {
  fs.writeFileSync(settingsPath(), JSON.stringify(s, null, 2));
  return true;
});

// ───── เปิดเกมผ่าน batch ที่ชุดแจกใช้จริง (ซ่อน console) ─────
ipcMain.handle('launch-game', async (_e, serverAddr) => {
  const batPath = path.join(gameDir, 'เล่นเกม.bat');
  if (!fs.existsSync(gameExe)) return { ok: false, error: 'ไม่พบ DurangoV2.exe ใน ' + gameDir };
  if (!fs.existsSync(batPath)) return { ok: false, error: 'ไม่พบ เล่นเกม.bat ใน ' + gameDir };

  const host = serverAddr.includes(':') ? serverAddr : serverAddr + ':8190';
  const command = `call "${batPath}" "${host}"`;
  const shell = process.env.ComSpec || 'cmd.exe';
  const result = await new Promise((resolve) => {
    let settled = false;
    const finish = (value) => {
      if (settled) return;
      settled = true;
      resolve(value);
    };
    const child = spawn(shell, ['/d', '/c', command], {
      cwd: gameDir,
      detached: true,
      windowsHide: true,
      windowsVerbatimArguments: true,
      stdio: 'ignore',
    });
    child.unref();
    child.on('error', (error) => finish({ ok: false, error: 'เรียก เล่นเกม.bat ไม่สำเร็จ: ' + error.message }));
    child.on('close', (code) => finish(code === 0
      ? { ok: true }
      : { ok: false, error: 'เล่นเกม.bat จบผิดพลาด (exit ' + code + ')' }));
    setTimeout(() => finish({ ok: true }), 1500); // batch ใช้ start จึงจบก่อนเกม
  });
  return result;
});

// ───── อัปเดตเกม (manifest → zip → SHA256 → temp → robocopy /MIR — logic เดียวกับ tools/Updater)
// ทำใน main process แล้ว push progress เข้า renderer ─────
let patchAbort = false;
ipcMain.on('patch-abort', () => { patchAbort = true; });

ipcMain.handle('patch-game', async (_e, manifest) => {
  patchAbort = false;
  const os = require('os');
  const tempRoot = path.join(os.tmpdir(), 'dinoworld-update-' + crypto.randomBytes(8).toString('hex'));
  const zipPath = tempRoot + '.zip';
  const extractDir = tempRoot + '-extract';
  const send = (pct, label) => { if (!win.isDestroyed()) win.webContents.send('patch-progress', pct, label); };

  try {
    // 1) โหลด zip (ยังไม่แตะไฟล์เกมจริงเลย)
    send(2, '⬇️ กำลังโหลดอัปเดต… 0 MB');
    const res = await fetch(manifest.zip_url);
    if (!res.ok) throw new Error('โหลด zip ไม่สำเร็จ (HTTP ' + res.status + ')');
    const total = Number(res.headers.get('content-length') || 0);
    const buf = Buffer.from(await res.arrayBuffer());
    send(55, `⬇️ โหลดแล้ว ${(buf.length / 1048576).toFixed(0)} MB`);

    // 2) SHA256
    send(60, '🔍 ตรวจสอบไฟล์ (SHA256)…');
    if (manifest.sha256) {
      const actual = crypto.createHash('sha256').update(buf).digest('hex');
      if (actual.toLowerCase() !== manifest.sha256.toLowerCase()) {
        throw new Error('ไฟล์ที่โหลดไม่ตรง SHA256 — ยกเลิก (ไฟล์เกมยังไม่ถูกแตะ)');
      }
    }
    fs.writeFileSync(zipPath, buf);

    // 3) แตกลง temp
    send(70, '📦 แตกไฟล์…');
    await extractZip(zipPath, extractDir);

    // 4) หา root จริง (zip อาจห่อโฟลเดอร์ชั้นนอก — บั๊กจริงจาก Updater เดิม)
    let sourceRoot = extractDir;
    if (!fs.existsSync(path.join(extractDir, 'DurangoV2.exe'))) {
      const found = findFile(extractDir, 'DurangoV2.exe');
      if (!found) throw new Error('ไฟล์ที่แตกออกมาไม่ครบ (ไม่เจอ DurangoV2.exe)');
      sourceRoot = path.dirname(found);
    }

    // 5) robocopy /MIR — จุดเดียวที่แตะไฟล์จริง
    send(85, '🔁 ติดตั้งไฟล์…');
    await robocopyMirror(sourceRoot, gameDir);

    fs.writeFileSync(path.join(gameDir, 'version.txt'), manifest.version);
    send(100, '✅ อัปเดตเสร็จ');
    return { ok: true, version: manifest.version };
  } catch (err) {
    send(0, 'พร้อมเล่น');
    return { ok: false, error: String(err.message || err) };
  } finally {
    try { fs.unlinkSync(zipPath); } catch {}
    try { fs.rmSync(extractDir, { recursive: true, force: true }); } catch {}
  }
});

function extractZip(zipPath, dest) {
  return new Promise((resolve, reject) => {
    execFile('tar', ['-xf', zipPath, '-C', dest], (err) => {
      if (err) {
        // fallback: PowerShell Expand-Archive (tar มีใน Windows 10+ อยู่แล้ว แต่กันไว้)
        execFile('powershell', ['-NoProfile', '-Command',
          `Expand-Archive -LiteralPath '${zipPath}' -DestinationPath '${dest}' -Force`],
          (err2) => err2 ? reject(new Error('แตกไฟล์ไม่สำเร็จ: ' + err2.message)) : resolve());
      } else resolve();
    });
  });
}

function robocopyMirror(source, dest) {
  return new Promise((resolve, reject) => {
    execFile('robocopy', [source, dest, '/MIR', '/NFL', '/NDL', '/NJH', '/NJS', '/R:2', '/W:1',
      '/XD', path.join(dest, 'AppData'), path.join(dest, 'AppData2'),
      '/XF', 'server.txt', 'update-manifest-url.txt', 'version.txt', 'game.log', 'launcher_settings.json'],
      (err) => {
        // robocopy exit code 0-7 = สำเร็จ, >=8 = error จริง
        if (err && err.code >= 8) reject(new Error('robocopy ล้มเหลว (exit ' + err.code + ')'));
        else resolve();
      });
  });
}

function findFile(root, name) {
  const q = [root];
  while (q.length) {
    const d = q.shift();
    for (const e of fs.readdirSync(d, { withFileTypes: true })) {
      const p = path.join(d, e.name);
      if (e.isDirectory()) q.push(p);
      else if (e.name === name) return p;
    }
  }
  return null;
}

app.on('window-all-closed', () => app.quit());
