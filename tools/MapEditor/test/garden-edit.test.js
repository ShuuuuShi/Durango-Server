'use strict';

const assert = require('node:assert/strict');
const test = require('node:test');

const core = require('../lib/map-editor-core');

test('placeGarden adds a natural then encodeGarden + decodeGarden round-trips', () => {
  const width = 8;
  const height = 8;
  const records = [];
  const result = core.placeGarden(records, width, height, { x: 3, y: 4, entityType: 99 });

  assert.ok(result && Array.isArray(result.records));
  assert.equal(result.records.length, 1);
  assert.equal(records.length, 1);
  assert.deepEqual(records[0], { x: 3, y: 4, entityType: 99 });
  assert.deepEqual(Object.keys(records[0]).sort(), ['entityType', 'x', 'y']);

  const report = { issues: [] };
  const decoded = core.decodeGarden(core.encodeGarden(records), width, height, report);
  assert.deepEqual(decoded, [{ x: 3, y: 4, entityType: 99 }]);
  assert.equal(report.issues.length, 0);
});

test('placeGarden rejects duplicate tile at same x,y', () => {
  const records = [];
  core.placeGarden(records, 8, 8, { x: 1, y: 2, entityType: 10 });
  assert.throws(
    () => core.placeGarden(records, 8, 8, { x: 1, y: 2, entityType: 11 }),
    RangeError,
  );
  assert.equal(records.length, 1);
  assert.equal(records[0].entityType, 10);
});

test('eraseGarden removes the record at a tile', () => {
  const records = [];
  core.placeGarden(records, 8, 8, { x: 2, y: 3, entityType: 7 });
  core.placeGarden(records, 8, 8, { x: 4, y: 5, entityType: 8 });
  const erased = core.eraseGarden(records, { x: 2, y: 3 });
  assert.equal(erased.removed, 1);
  assert.equal(erased.records.length, 1);
  assert.deepEqual(erased.records[0], { x: 4, y: 5, entityType: 8 });
  const miss = core.eraseGarden(records, { x: 2, y: 3 });
  assert.equal(miss.removed, 0);
  assert.equal(records.length, 1);
});

test('placeGarden rejects out-of-bounds and non-integer coords', () => {
  const records = [];
  assert.throws(() => core.placeGarden(records, 8, 8, { x: -1, y: 0, entityType: 1 }), RangeError);
  assert.throws(() => core.placeGarden(records, 8, 8, { x: 0, y: -1, entityType: 1 }), RangeError);
  assert.throws(() => core.placeGarden(records, 8, 8, { x: 8, y: 0, entityType: 1 }), RangeError);
  assert.throws(() => core.placeGarden(records, 8, 8, { x: 0, y: 8, entityType: 1 }), RangeError);
  assert.throws(() => core.placeGarden(records, 8, 8, { x: 1.5, y: 0, entityType: 1 }), TypeError);
  assert.throws(() => core.placeGarden(records, 8, 8, { x: 0, y: 1.5, entityType: 1 }), TypeError);
  assert.equal(records.length, 0);
});

test('garden record shape is { x, y, entityType } only', () => {
  const records = [];
  core.placeGarden(records, 8, 8, { x: 0, y: 0, entityType: 42 });
  const keys = Object.keys(records[0]).sort();
  assert.deepEqual(keys, ['entityType', 'x', 'y']);
  assert.equal(typeof records[0].x, 'number');
  assert.equal(typeof records[0].y, 'number');
  assert.equal(typeof records[0].entityType, 'number');
});