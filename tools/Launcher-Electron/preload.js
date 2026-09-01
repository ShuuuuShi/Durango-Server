const { contextBridge, ipcRenderer } = require('electron');

contextBridge.exposeInMainWorld('launcher', {
  minimize: () => ipcRenderer.send('win-minimize'),
  close: () => ipcRenderer.send('win-close'),
  getState: () => ipcRenderer.invoke('get-state'),
  httpJson: (url) => ipcRenderer.invoke('http-json', url),
  saveSettings: (s) => ipcRenderer.invoke('save-settings', s),
  launchGame: (server) => ipcRenderer.invoke('launch-game', server),
  patchGame: (manifest) => ipcRenderer.invoke('patch-game', manifest),
  onPatchProgress: (cb) => ipcRenderer.on('patch-progress', (_e, pct, label) => cb(pct, label)),
  openMapEditor: () => ipcRenderer.invoke('open-map-editor'),
});

contextBridge.exposeInMainWorld('mapEditor', {
  minimize: () => ipcRenderer.send('editor-minimize'),
  close: () => ipcRenderer.send('editor-close'),
  openMap: () => ipcRenderer.invoke('editor-open-map'),
  saveMap: (data, filePath) => ipcRenderer.invoke('editor-save-map', { data, filePath }),
  exportImage: (name, dataUrl) => ipcRenderer.invoke('editor-export-image', { name, dataUrl }),
  scanGame: () => ipcRenderer.invoke('editor-scan-game'),
  importTerrain: () => ipcRenderer.invoke('editor-import-terrain'),
  exportReport: (data) => ipcRenderer.invoke('editor-export-report', data),
  clearCache: () => ipcRenderer.invoke('editor-clear-cache'),
  saveProject: (payload) => ipcRenderer.invoke('editor-save-project', payload),
  exportTerrain: (payload) => ipcRenderer.invoke('editor-export-terrain', payload),
  listBackups: (payload) => ipcRenderer.invoke('editor-list-backups', payload),
  rollbackTerrain: (payload) => ipcRenderer.invoke('editor-rollback-terrain', payload),
  applyBiomeBrush: (payload) => ipcRenderer.invoke('editor-apply-biome-brush', payload),
  applyCoastBrush: (payload) => ipcRenderer.invoke('editor-apply-coast-brush', payload),
});
