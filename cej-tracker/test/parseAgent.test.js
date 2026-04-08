import fs from 'node:fs/promises';
import path from 'node:path';
import test from 'node:test';
import assert from 'node:assert/strict';
import { fileURLToPath } from 'node:url';

import { parseListings } from '../src/agents/parseAgent.js';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

test('parseListings extracts normalized listing data', async () => {
  const html = await fs.readFile(path.join(__dirname, 'fixtures', 'search-page.html'), 'utf8');

  const listings = parseListings({
    html,
    searchUrl: 'https://cej.dk/ledige-lejeboliger',
    source: 'cej',
    seenAt: '2026-03-23T12:00:00.000Z'
  });

  assert.equal(listings.length, 2);
  assert.equal(listings[0].url, 'https://cej.dk/bolig/lyngby/lejlighed-a');
  assert.equal(listings[0].monthlyPriceDkk, 7758);
  assert.equal(listings[0].rooms, 3);
  assert.equal(listings[0].sizeM2, 82);
  assert.equal(listings[0].floor, '3. sal');
  assert.match(listings[0].rawText, /Lyngby Hovedgade 10/);
  assert.equal(listings[1].monthlyPriceDkk, 12500);
  assert.equal(listings[1].status, 'Reserveret');
});
