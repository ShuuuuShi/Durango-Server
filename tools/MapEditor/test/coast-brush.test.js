'use strict';

const assert = require('node:assert/strict');
const test = require('node:test');

const core = require('../lib/map-editor-core');

test('encodeSignedByte / decodeSignedByte round-trip +5', () => {
  const encoded = core.encodeSignedByte(5);
  assert.equal(encoded, 5);
  assert.equal(core.decodeSignedByte(encoded), 5);
});

test('encodeSignedByte / decodeSignedByte round-trip -3', () => {
  const encoded = core.encodeSignedByte(-3);
  assert.equal(encoded, 253);
  assert.equal(core.decodeSignedByte(encoded), -3);
});

test('applyCoastBrush writes +5 then decodeSignedByte round-trips', () => {
  const width = 8;
  const height = 8;
  const buf = Buffer.alloc(width * height, 0);
  const result = core.applyCoastBrush(buf, width, height, {
    x: 3,
    y: 3,
    radius: 1,
    value: 5,
  });

  assert.ok(result && typeof result.changed === 'number');
  assert.equal(result.changed, 5);

  const at = (x, y) => buf[x + y * width];
  const inCircle = (x, y) => {
    const dx = x - 3;
    const dy = y - 3;
    return dx * dx + dy * dy <= 1 * 1;
  };

  for (let y = 0; y < height; y++) {
    for (let x = 0; x < width; x++) {
      const raw = at(x, y);
      if (inCircle(x, y)) {
        assert.equal(raw, core.encodeSignedByte(5), `encoded +5 at (${x},${y})`);
        assert.equal(core.decodeSignedByte(raw), 5, `decoded +5 at (${x},${y})`);
      } else {
        assert.equal(raw, 0, `untouched at (${x},${y})`);
      }
    }
  }
});

test('applyCoastBrush writes -3 then decodeSignedByte round-trips', () => {
  const width = 8;
  const height = 8;
  const buf = Buffer.alloc(width * height, 0);
  const result = core.applyCoastBrush(buf, width, height, {
    x: 3,
    y: 3,
    radius: 1,
    value: -3,
  });

  assert.ok(result && typeof result.changed === 'number');
  assert.equal(result.changed, 5);

  const at = (x, y) => buf[x + y * width];
  const inCircle = (x, y) => {
    const dx = x - 3;
    const dy = y - 3;
    return dx * dx + dy * dy <= 1 * 1;
  };

  for (let y = 0; y < height; y++) {
    for (let x = 0; x < width; x++) {
      const raw = at(x, y);
      if (inCircle(x, y)) {
        assert.equal(raw, core.encodeSignedByte(-3), `encoded -3 at (${x},${y})`);
        assert.equal(core.decodeSignedByte(raw), -3, `decoded -3 at (${x},${y})`);
        assert.equal(raw, 253, `uint8 two's complement for -3 at (${x},${y})`);
      } else {
        assert.equal(raw, 0, `untouched at (${x},${y})`);
      }
    }
  }
});