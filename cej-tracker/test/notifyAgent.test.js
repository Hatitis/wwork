import test from 'node:test';
import assert from 'node:assert/strict';

import { buildNotificationMessage } from '../src/agents/notifyAgent.js';

test('buildNotificationMessage includes event labels and listing links', () => {
  const listing = {
    id: 'a',
    url: 'https://cej.test/boliger/a',
    title: 'Test listing',
    address: 'Address 1',
    locationText: 'Address 1',
    floor: '2. sal',
    rooms: 2,
    sizeM2: 55,
    monthlyPriceDkk: 9000,
    status: 'ACTIVE'
  };

  const message = buildNotificationMessage({
    events: [
      {
        type: 'LISTING_NEW',
        listingId: 'a',
        listingUrl: listing.url,
        occurredAt: '2026-03-23T10:00:00.000Z',
        source: 'cej',
        searchUrl: 'https://cej.test/search',
        title: listing.title,
        summary: 'New listing',
        previous: {},
        current: listing
      }
    ],
    listingsById: new Map([[listing.id, listing]])
  });

  assert.match(message.subject, /1 change/);
  assert.match(message.text, /LISTING_NEW/);
  assert.match(message.text, /https:\/\/cej\.test\/boliger\/a/);
});
