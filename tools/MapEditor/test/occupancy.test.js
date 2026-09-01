'use strict';

const assert = require('node:assert/strict');
const test = require('node:test');

const core = require('../lib/map-editor-core');
const occupancyMod = require('../lib/occupancy');

function at(grid, width, x, y) {
  return grid[x + y * width];
}

function assertByteGrid(grid, width, height) {
  assert.ok(grid instanceof Uint8Array || Buffer.isBuffer(grid), 'expected Uint8Array or Buffer');
  assert.equal(grid.length, width * height);
  for (let i = 0; i < grid.length; i++) {
    assert.ok(grid[i] === 0 || grid[i] === 1, `cell ${i} must be 0 or 1, got ${grid[i]}`);
  }
}

function countBlocked(grid) {
  let n = 0;
  for (let i = 0; i < grid.length; i++) if (grid[i] === 1) n++;
  return n;
}

test('buildOccupancy is re-exported from map-editor-core and occupancy module', () => {
  assert.equal(typeof core.buildOccupancy, 'function');
  assert.equal(typeof occupancyMod.buildOccupancy, 'function');
  assert.equal(core.buildOccupancy, occupancyMod.buildOccupancy);
});

test('empty inputs yield an all-zero grid', () => {
  const width = 4;
  const height = 3;
  const grid = core.buildOccupancy({ width, height });
  assertByteGrid(grid, width, height);
  assert.equal(countBlocked(grid), 0);
  for (let i = 0; i < grid.length; i++) assert.equal(grid[i], 0);
});

test('missing garden, landmarks, and artifacts default to empty', () => {
  const grid = core.buildOccupancy({ width: 2, height: 2, padTiles: 0 });
  assertByteGrid(grid, 2, 2);
  assert.equal(countBlocked(grid), 0);
});

test('garden tiles are blocked', () => {
  const width = 8;
  const height = 8;
  const grid = core.buildOccupancy({
    width,
    height,
    garden: [
      { x: 1, y: 2, entityType: 10 },
      { x: 3, y: 3, entityType: 11 },
    ],
  });
  assertByteGrid(grid, width, height);
  assert.equal(at(grid, width, 1, 2), 1);
  assert.equal(at(grid, width, 3, 3), 1);
  assert.equal(at(grid, width, 0, 0), 0);
  assert.equal(at(grid, width, 2, 2), 0);
  assert.equal(countBlocked(grid), 2);
});

test('landmark tiles are blocked', () => {
  const width = 8;
  const height = 8;
  const grid = core.buildOccupancy({
    width,
    height,
    landmarks: [
      { x: 4, y: 5, id: 99 },
      { x: 0, y: 0, id: 1 },
    ],
  });
  assertByteGrid(grid, width, height);
  assert.equal(at(grid, width, 4, 5), 1);
  assert.equal(at(grid, width, 0, 0), 1);
  assert.equal(at(grid, width, 1, 0), 0);
  assert.equal(countBlocked(grid), 2);
});

test('artifact width/height footprint is blocked', () => {
  const width = 8;
  const height = 8;
  const grid = core.buildOccupancy({
    width,
    height,
    artifacts: [{ x: 2, y: 3, width: 2, height: 3 }],
  });
  assertByteGrid(grid, width, height);
  for (let y = 3; y < 6; y++) {
    for (let x = 2; x < 4; x++) {
      assert.equal(at(grid, width, x, y), 1, `expected blocked at ${x},${y}`);
    }
  }
  assert.equal(at(grid, width, 1, 3), 0);
  assert.equal(at(grid, width, 2, 2), 0);
  assert.equal(at(grid, width, 4, 3), 0);
  assert.equal(countBlocked(grid), 6);
});

test('artifact default footprint is a single tile at x,y', () => {
  const width = 5;
  const height = 5;
  const grid = core.buildOccupancy({
    width,
    height,
    artifacts: [{ x: 2, y: 2 }],
  });
  assertByteGrid(grid, width, height);
  assert.equal(at(grid, width, 2, 2), 1);
  assert.equal(countBlocked(grid), 1);
});

test('artifact footprint tiles array is blocked', () => {
  const width = 6;
  const height = 6;
  const grid = core.buildOccupancy({
    width,
    height,
    artifacts: [
      {
        x: 0,
        y: 0,
        footprint: [
          { x: 1, y: 1 },
          { x: 2, y: 1 },
          { x: 1, y: 2 },
        ],
      },
    ],
  });
  assertByteGrid(grid, width, height);
  assert.equal(at(grid, width, 1, 1), 1);
  assert.equal(at(grid, width, 2, 1), 1);
  assert.equal(at(grid, width, 1, 2), 1);
  assert.equal(at(grid, width, 0, 0), 0);
  assert.equal(countBlocked(grid), 3);
});

test('padTiles expands artifact blocked cells with Chebyshev distance, not landmarks or garden', () => {
  const width = 9;
  const height = 9;
  const grid = core.buildOccupancy({
    width,
    height,
    padTiles: 1,
    garden: [{ x: 0, y: 0, entityType: 1 }],
    landmarks: [{ x: 8, y: 8, id: 2 }],
    artifacts: [{ x: 4, y: 4 }],
  });
  assertByteGrid(grid, width, height);
  // Artifact at (4,4) plus Chebyshev pad 1 => 3x3 block
  for (let y = 3; y <= 5; y++) {
    for (let x = 3; x <= 5; x++) {
      assert.equal(at(grid, width, x, y), 1, `pad expected at ${x},${y}`);
    }
  }
  assert.equal(at(grid, width, 2, 4), 0);
  assert.equal(at(grid, width, 4, 2), 0);
  // Garden and landmark stay single-tile (spec silent on padding them)
  assert.equal(at(grid, width, 0, 0), 1);
  assert.equal(at(grid, width, 1, 0), 0);
  assert.equal(at(grid, width, 0, 1), 0);
  assert.equal(at(grid, width, 8, 8), 1);
  assert.equal(at(grid, width, 7, 8), 0);
  assert.equal(at(grid, width, 8, 7), 0);
  assert.equal(countBlocked(grid), 9 + 1 + 1);
});

test('padTiles default is 0', () => {
  const width = 5;
  const height = 5;
  const grid = core.buildOccupancy({
    width,
    height,
    artifacts: [{ x: 2, y: 2, width: 1, height: 1 }],
  });
  assert.equal(at(grid, width, 2, 2), 1);
  assert.equal(at(grid, width, 1, 2), 0);
  assert.equal(at(grid, width, 3, 2), 0);
  assert.equal(countBlocked(grid), 1);
});

test('garden, landmark, and artifact combine with OR; index is x + y*width', () => {
  const width = 4;
  const height = 3;
  const grid = core.buildOccupancy({
    width,
    height,
    garden: [{ x: 1, y: 0, entityType: 1 }],
    landmarks: [{ x: 1, y: 0, id: 9 }],
    artifacts: [{ x: 3, y: 2 }],
  });
  assert.equal(grid[1 + 0 * width], 1);
  assert.equal(grid[3 + 2 * width], 1);
  assert.equal(countBlocked(grid), 2);
});

test('out-of-bounds records are ignored or clipped and do not throw', () => {
  const width = 4;
  const height = 4;
  assert.doesNotThrow(() => {
    const grid = core.buildOccupancy({
      width,
      height,
      garden: [
        { x: -1, y: 0, entityType: 1 },
        { x: 4, y: 0, entityType: 1 },
        { x: 0, y: -1, entityType: 1 },
        { x: 0, y: 4, entityType: 1 },
      ],
      landmarks: [{ x: 99, y: 99, id: 1 }, { x: -5, y: 1, id: 2 }],
      artifacts: [
        { x: 3, y: 3, width: 4, height: 4 },
        { x: -2, y: -2, width: 3, height: 3 },
        { x: 10, y: 10 },
      ],
      padTiles: 2,
    });
    assertByteGrid(grid, width, height);
    // Artifact at (3,3) size 4x4 clipped to in-bounds tiles; pad expands but stays in bounds
    assert.equal(at(grid, width, 3, 3), 1);
  });
});

test('padTiles around artifact clips to map bounds', () => {
  const width = 3;
  const height = 3;
  const grid = core.buildOccupancy({
    width,
    height,
    artifacts: [{ x: 0, y: 0 }],
    padTiles: 2,
  });
  assertByteGrid(grid, width, height);
  for (let i = 0; i < grid.length; i++) assert.equal(grid[i], 1);
});
