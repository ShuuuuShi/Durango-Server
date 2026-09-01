'use strict';

function inBounds(x, y, width, height) {
  return Number.isInteger(x) && Number.isInteger(y) && x >= 0 && y >= 0 && x < width && y < height;
}

function mark(grid, width, height, x, y) {
  if (inBounds(x, y, width, height)) {
    grid[x + y * width] = 1;
  }
}

function markChebyshevPad(grid, width, height, x, y, padTiles) {
  const pad = padTiles > 0 ? padTiles : 0;
  for (let dy = -pad; dy <= pad; dy++) {
    for (let dx = -pad; dx <= pad; dx++) {
      mark(grid, width, height, x + dx, y + dy);
    }
  }
}

function artifactFootprintTiles(art) {
  if (!art || typeof art !== 'object') return [];
  if (Array.isArray(art.footprint)) {
    const tiles = [];
    for (const t of art.footprint) {
      if (t == null) continue;
      if (Array.isArray(t) && t.length >= 2) tiles.push({ x: t[0], y: t[1] });
      else if (typeof t === 'object') tiles.push({ x: t.x, y: t.y });
    }
    return tiles;
  }
  const ox = art.x;
  const oy = art.y;
  const wRaw = art.width;
  const hRaw = art.height;
  const w = Number.isFinite(wRaw) ? Math.trunc(wRaw) : 1;
  const h = Number.isFinite(hRaw) ? Math.trunc(hRaw) : 1;
  const ww = Math.max(1, w);
  const hh = Math.max(1, h);
  const tiles = [];
  for (let dy = 0; dy < hh; dy++) {
    for (let dx = 0; dx < ww; dx++) {
      tiles.push({ x: ox + dx, y: oy + dy });
    }
  }
  return tiles;
}

function buildOccupancy(opts) {
  const options = opts && typeof opts === 'object' ? opts : {};
  const width = options.width;
  const height = options.height;
  if (!Number.isInteger(width) || !Number.isInteger(height) || width <= 0 || height <= 0) {
    throw new TypeError('width and height must be positive integers');
  }

  const garden = Array.isArray(options.garden) ? options.garden : [];
  const landmarks = Array.isArray(options.landmarks) ? options.landmarks : [];
  const artifacts = Array.isArray(options.artifacts) ? options.artifacts : [];
  const padRaw = options.padTiles == null ? 0 : options.padTiles;
  const padTiles = Number.isFinite(padRaw) ? Math.max(0, Math.trunc(padRaw)) : 0;

  const grid = Buffer.alloc(width * height, 0);

  for (const item of garden) {
    if (!item) continue;
    mark(grid, width, height, item.x, item.y);
  }
  for (const item of landmarks) {
    if (!item) continue;
    mark(grid, width, height, item.x, item.y);
  }
  for (const art of artifacts) {
    for (const tile of artifactFootprintTiles(art)) {
      markChebyshevPad(grid, width, height, tile.x, tile.y, padTiles);
    }
  }

  return grid;
}

module.exports = { buildOccupancy };
