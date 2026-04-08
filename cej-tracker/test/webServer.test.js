import test from 'node:test';
import assert from 'node:assert/strict';

import { renderPage } from '../src/webServer.js';

test('renderPage includes listings and recent events', () => {
  const html = renderPage(
    [
      {
        id: 'a',
        url: 'https://cej.test/a',
        title: 'Lejlighed A',
        address: 'Adresse 1',
        locationText: 'Adresse 1',
        floor: '1. sal',
        rooms: 2,
        sizeM2: 50,
        monthlyPriceDkk: 8500,
        status: 'Ledig',
        source: 'cej',
        searchUrl: 'https://cej.test/search',
        firstSeenAt: '2026-03-23T10:00:00.000Z',
        lastSeenAt: '2026-03-23T10:00:00.000Z',
        rawText: 'raw'
      }
    ],
    [
      {
        type: 'LISTING_NEW',
        listingId: 'a',
        listingUrl: 'https://cej.test/a',
        occurredAt: '2026-03-23T10:00:00.000Z',
        source: 'cej',
        searchUrl: 'https://cej.test/search',
        title: 'Lejlighed A',
        summary: 'New listing',
        previous: {},
        current: {}
      }
    ],
    {
      lastSuccessAt: '2026-03-23T10:00:00.000Z',
      consecutiveFailures: 0,
      searchUrl: 'https://cej.test/search'
    }
  );

  assert.match(html, /CEJ Tracker Dashboard/);
  assert.match(html, /Lejlighed A/);
  assert.match(html, /New listing/);
});
