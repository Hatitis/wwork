import test from 'node:test';
import assert from 'node:assert/strict';

import { diffListings } from '../src/agents/diffAgent.js';

const previousListings = [
  {
    id: 'https://cej.dk/a',
    url: 'https://cej.dk/a',
    title: 'Lejlighed A',
    address: 'Adresse A',
    locationText: 'Kobenhavn',
    floor: '1. sal',
    rooms: 2,
    sizeM2: 60,
    monthlyPriceDkk: 10000,
    status: 'Ledig',
    source: 'cej',
    searchUrl: 'https://cej.dk/search',
    firstSeenAt: '2026-03-20T10:00:00.000Z',
    lastSeenAt: '2026-03-20T10:00:00.000Z',
    rawText: 'A'
  },
  {
    id: 'https://cej.dk/b',
    url: 'https://cej.dk/b',
    title: 'Lejlighed B',
    address: 'Adresse B',
    locationText: 'Aarhus',
    floor: null,
    rooms: 3,
    sizeM2: 80,
    monthlyPriceDkk: 12000,
    status: 'Ledig',
    source: 'cej',
    searchUrl: 'https://cej.dk/search',
    firstSeenAt: '2026-03-20T10:00:00.000Z',
    lastSeenAt: '2026-03-20T10:00:00.000Z',
    rawText: 'B'
  }
];

test('diffListings emits new, removed, price, status, and details events', () => {
  const currentListings = [
    {
      ...previousListings[0],
      monthlyPriceDkk: 10500,
      status: 'Reserveret',
      sizeM2: 62,
      lastSeenAt: '2026-03-23T10:00:00.000Z'
    },
    {
      id: 'https://cej.dk/c',
      url: 'https://cej.dk/c',
      title: 'Lejlighed C',
      address: 'Adresse C',
      locationText: 'Odense',
      floor: '2. sal',
      rooms: 1,
      sizeM2: 42,
      monthlyPriceDkk: 8000,
      status: 'Ledig',
      source: 'cej',
      searchUrl: 'https://cej.dk/search',
      firstSeenAt: '2026-03-23T10:00:00.000Z',
      lastSeenAt: '2026-03-23T10:00:00.000Z',
      rawText: 'C'
    }
  ];

  const result = diffListings({
    previousListings,
    currentListings,
    occurredAt: '2026-03-23T10:00:00.000Z'
  });

  assert.equal(result.mergedListings.length, 2);
  assert.equal(result.mergedListings[0].firstSeenAt, '2026-03-20T10:00:00.000Z');

  const eventTypes = result.events.map((event) => event.type).sort();
  assert.deepEqual(eventTypes, [
    'DETAILS_CHANGED',
    'LISTING_NEW',
    'LISTING_REMOVED',
    'PRICE_CHANGED',
    'STATUS_CHANGED'
  ]);
});
