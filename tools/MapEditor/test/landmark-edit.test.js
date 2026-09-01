'use strict';

const assert = require('node:assert/strict');
const test = require('node:test');

const core = require('../lib/map-editor-core');

const LANDMARK_KEYS = ['id', 'offsetX', 'offsetY', 'offsetZ', 'rotate', 'scaleX', 'scaleY', 'scaleZ', 'x', 'y'];

function expectedRecord(fields) {
  return {
    x: fields.x,
    y: fields.y,
    id: fields.id,
    rotate: fields.rotate || 0,
    offsetX: fields.offsetX || 0,
    offsetY: fields.offsetY || 0,
    offsetZ: fields.offsetZ || 0,
    scaleX: fields.scaleX || 0,
    scaleY: fields.scaleY || 0,
    scaleZ: fields.scaleZ || 0,
  };
}

test('placeLandmark adds a record then encodeLandmarks + decodeLandmarks round-trips', () => {
  const width = 8;
  const height = 8;
  const records = [];
  const fields = {
    x: 3,
    y: 4,
    id: 99,
    rotate: 12,
    offsetX: -5,
    offsetY: 7,
    offsetZ: -9,
    scaleX: 10,
    scaleY: 20,
    scaleZ: 30,
  };
  const result = core.placeLandmark(records, width, height, fields);

  assert.ok(result && Array.isArray(result.records));
  assert.equal(result.records.length, 1);
  assert.equal(records.length, 1);
  assert.deepEqual(records[0], expectedRecord(fields));
  assert.deepEqual(Object.keys(records[0]).sort(), LANDMARK_KEYS);

  const report = { issues: [] };
  const decoded = core.decodeLandmarks(core.encodeLandmarks(records), width, height, report);
  assert.deepEqual(decoded, [expectedRecord(fields)]);
  assert.equal(report.issues.length, 0);
});

test('placeLandmark defaults optional rotate/offsets/scales to 0', () => {
  const records = [];
  core.placeLandmark(records, 8, 8, { x: 1, y: 2, id: 5 });
  assert.deepEqual(records[0], expectedRecord({ x: 1, y: 2, id: 5 }));
});

test('placeLandmark rejects duplicate tile at same x,y', () => {
  const records = [];
  core.placeLandmark(records, 8, 8, { x: 1, y: 2, id: 10 });
  assert.throws(
    () => core.placeLandmark(records, 8, 8, { x: 1, y: 2, id: 11 }),
    RangeError,
  );
  assert.equal(records.length, 1);
  assert.equal(records[0].id, 10);
});

test('placeLandmark rejects out-of-bounds and non-integer coords', () => {
  const records = [];
  assert.throws(() => core.placeLandmark(records, 8, 8, { x: -1, y: 0, id: 1 }), RangeError);
  assert.throws(() => core.placeLandmark(records, 8, 8, { x: 0, y: -1, id: 1 }), RangeError);
  assert.throws(() => core.placeLandmark(records, 8, 8, { x: 8, y: 0, id: 1 }), RangeError);
  assert.throws(() => core.placeLandmark(records, 8, 8, { x: 0, y: 8, id: 1 }), RangeError);
  assert.throws(() => core.placeLandmark(records, 8, 8, { x: 1.5, y: 0, id: 1 }), TypeError);
  assert.throws(() => core.placeLandmark(records, 8, 8, { x: 0, y: 1.5, id: 1 }), TypeError);
  assert.equal(records.length, 0);
});

test('moveLandmark relocates a record and keeps id/rotate/offsets/scales', () => {
  const records = [];
  core.placeLandmark(records, 8, 8, {
    x: 1,
    y: 2,
    id: 42,
    rotate: 3,
    offsetX: 4,
    offsetY: -6,
    offsetZ: 8,
    scaleX: 1,
    scaleY: 2,
    scaleZ: 3,
  });
  const moved = core.moveLandmark(records, 8, 8, { fromX: 1, fromY: 2, toX: 5, toY: 6 });
  assert.equal(moved.records.length, 1);
  assert.deepEqual(moved.records[0], expectedRecord({
    x: 5,
    y: 6,
    id: 42,
    rotate: 3,
    offsetX: 4,
    offsetY: -6,
    offsetZ: 8,
    scaleX: 1,
    scaleY: 2,
    scaleZ: 3,
  }));
  assert.equal(records[0].x, 5);
  assert.equal(records[0].y, 6);
});

test('moveLandmark also accepts { x, y } source plus { toX, toY }', () => {
  const records = [];
  core.placeLandmark(records, 8, 8, { x: 0, y: 0, id: 7 });
  core.moveLandmark(records, 8, 8, { x: 0, y: 0, toX: 2, toY: 3 });
  assert.deepEqual(records[0], expectedRecord({ x: 2, y: 3, id: 7 }));
});

test('moveLandmark rejects missing source, OOB dest, and occupied dest', () => {
  const records = [];
  core.placeLandmark(records, 8, 8, { x: 1, y: 1, id: 1 });
  core.placeLandmark(records, 8, 8, { x: 2, y: 2, id: 2 });
  assert.throws(() => core.moveLandmark(records, 8, 8, { fromX: 7, fromY: 7, toX: 0, toY: 0 }), RangeError);
  assert.throws(() => core.moveLandmark(records, 8, 8, { fromX: 1, fromY: 1, toX: 8, toY: 0 }), RangeError);
  assert.throws(() => core.moveLandmark(records, 8, 8, { fromX: 1, fromY: 1, toX: 2, toY: 2 }), RangeError);
  assert.equal(records[0].x, 1);
  assert.equal(records[0].y, 1);
});

test('moveLandmark allows dest equal to source tile', () => {
  const records = [];
  core.placeLandmark(records, 8, 8, { x: 3, y: 3, id: 9, rotate: 1 });
  core.moveLandmark(records, 8, 8, { fromX: 3, fromY: 3, toX: 3, toY: 3 });
  assert.deepEqual(records[0], expectedRecord({ x: 3, y: 3, id: 9, rotate: 1 }));
});

test('eraseLandmarkAt removes the record at a tile', () => {
  const records = [];
  core.placeLandmark(records, 8, 8, { x: 2, y: 3, id: 7 });
  core.placeLandmark(records, 8, 8, { x: 4, y: 5, id: 8 });
  const erased = core.eraseLandmarkAt(records, { x: 2, y: 3 });
  assert.equal(erased.removed, 1);
  assert.equal(erased.records.length, 1);
  assert.deepEqual(erased.records[0], expectedRecord({ x: 4, y: 5, id: 8 }));
  const miss = core.eraseLandmarkAt(records, { x: 2, y: 3 });
  assert.equal(miss.removed, 0);
  assert.equal(records.length, 1);
});

test('landmark record shape matches decodeLandmarks fields', () => {
  const records = [];
  core.placeLandmark(records, 8, 8, { x: 0, y: 0, id: 42 });
  assert.deepEqual(Object.keys(records[0]).sort(), LANDMARK_KEYS);
});
