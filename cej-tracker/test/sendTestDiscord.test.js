import test from 'node:test';
import assert from 'node:assert/strict';

import { bodies, titles } from '../src/sendTestDiscord.js';

test('discord test titles and bodies cover manual, started, and stopped modes', () => {
  assert.equal(titles.manual, 'Discord test notification');
  assert.equal(titles.started, 'Tracker turned on');
  assert.equal(titles.stopped, 'Tracker turned off');
  assert.match(bodies.manual, /Discord webhook is configured correctly/);
  assert.match(bodies.started, /turned on/);
  assert.match(bodies.stopped, /turned off/);
});
