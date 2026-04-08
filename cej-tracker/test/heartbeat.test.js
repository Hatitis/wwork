import test from 'node:test';
import assert from 'node:assert/strict';

import { maybeSendHeartbeat } from '../src/agents/notifyAgent.js';

test('maybeSendHeartbeat skips when local hour does not match configured hour', async () => {
  const result = await maybeSendHeartbeat({
    startedAt: '2026-04-08T10:05:00.000Z',
    previousState: {},
    listingsCount: 2,
    discord: {
      webhookUrl: 'https://discord.test/webhook',
      username: 'CEJ Tracker',
      avatarUrl: '',
      mentionUserId: '',
      mentionRoleId: ''
    },
    heartbeat: {
      enabled: true,
      hourLocal: 8,
      timezone: 'Europe/Copenhagen'
    },
    logger: {
      info: () => {},
      warn: () => {}
    }
  });

  assert.deepEqual(result, {});
});

test('maybeSendHeartbeat skips when heartbeat was already sent for local day', async () => {
  const result = await maybeSendHeartbeat({
    startedAt: '2026-04-08T06:05:00.000Z',
    previousState: {
      lastHeartbeatLocalDate: '2026-04-08'
    },
    listingsCount: 2,
    discord: {
      webhookUrl: 'https://discord.test/webhook',
      username: 'CEJ Tracker',
      avatarUrl: '',
      mentionUserId: '',
      mentionRoleId: ''
    },
    heartbeat: {
      enabled: true,
      hourLocal: 8,
      timezone: 'Europe/Copenhagen'
    },
    logger: {
      info: () => {},
      warn: () => {}
    }
  });

  assert.deepEqual(result, {});
});
