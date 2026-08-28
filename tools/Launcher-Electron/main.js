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
try {
  mapEditorCore = require(path.join(__dirname, '..', 'MapEditor', 'lib', 'map-editor-core'));
} catch (error) {
  console.warn('[map-editor] core unavailable:', error.message);
}

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
        biomes: terrain.layers.biomes?.toString('base64') || null,
        ocean: terrain.layers.ocean?.toString('base64') || null,
        rivers: terrain.layers.rivers?.toString('base64') || null,
        coastDistance: terrain.layers.coastDistance?.toString('base64') || null,
      },
      garden: terrain.garden,
      landmarks: terrain.landmarks,
      opaque: terrain.opaque,
    },
  };
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
  const result = await dialog.showOpenDialog(mapEditorWin, {
    title: 'เลือกโฟลเดอร์ terrain ที่สกัดแล้ว',
    defaultPath: path.join(projectRoot, 'server', 'data', 'terrains', 'extracted'),
    properties: ['openDirectory'],
  });
  if (result.canceled || !result.filePaths[0]) return { canceled: true };
  try {
    const imported = mapEditorCore.readTerrainSource(result.filePaths[0]);
    return { canceled: false, terrainFolder: result.filePaths[0], ...serializeTerrainImport(imported) };
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
