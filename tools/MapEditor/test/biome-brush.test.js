'use strict';

const assert = require('node:assert/strict');
const test = require('node:test');

const core = require('../lib/map-editor-core');

test('applyBiomeBrush paints type 5 at (3,3) r=1 and preserves 0x80 flags', () => {
  const width = 8;
  const height = 8;
  const biomes = Buffer.alloc(width * height, 0x80);
  const result = core.applyBiomeBrush(biomes, width, height, {
    x: 3,
    y: 3,
    radius: 1,
    biomeType: 5,
  });

  assert.ok(result && typeof result.changed === 'number');
  assert.ok(result.changed > 0);

  const at = (x, y) => biomes[x + y * width];
  const inCircle = (x, y) => {
    const dx = x - 3;
    const dy = y - 3;
    return dx * dx + dy * dy <= 1 * 1;
  };

  for (let y = 0; y < height; y++) {
    for (let x = 0; x < width; x++) {
      const raw = at(x, y);
      assert.equal(raw & core.BIOME_FLAGS_MASK, 0x80, `flags at (${x},${y})`);
      if (inCircle(x, y)) {
        assert.equal(raw & core.BIOME_TYPE_MASK, 5, `type at (${x},${y})`);
      } else {
        assert.equal(raw, 0x80, `untouched at (${x},${y})`);
      }
    }
  }

  assert.equal(result.changed, 5);
});
