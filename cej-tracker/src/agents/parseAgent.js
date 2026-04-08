import * as cheerio from 'cheerio';

import { listingArraySchema } from '../models/listingSchema.js';
import { createListingId } from '../utils/identity.js';
import { buildRawText, normalizeUrl, normalizeWhitespace, parseDanishPrice, parseNumber } from '../utils/normalize.js';

/**
 * @typedef {import('../models/listingSchema.js').listingSchema._type} Listing
 */

/**
 * Extract label-value pairs from text blobs.
 * @param {string[]} textSegments
 * @returns {Record<string, string>}
 */
function extractFacts(textSegments) {
  /** @type {Record<string, string>} */
  const facts = {};

  for (const segment of textSegments) {
    const normalized = normalizeWhitespace(segment);
    const match = normalized.match(/^([^:]+):\s*(.+)$/);
    if (!match) {
      continue;
    }

    facts[match[1].toLowerCase()] = match[2];
  }

  return facts;
}

/**
 * Parse a CEJ bolig card rendered as an anchor tile.
 * @param {cheerio.CheerioAPI} $
 * @param {cheerio.Element} element
 * @param {{ searchUrl: string, source: string, seenAt: string }} options
 * @returns {Listing | null}
 */
function parseBoligCard($, element, options) {
  const root = $(element);
  const url = normalizeUrl(root.attr('href'), options.searchUrl);
  const rawText = buildRawText([root.text()]);
  const textNodes = root
    .find('p, h1, h2, h3, h4, h5, h6, div[title]')
    .map((_, node) => normalizeWhitespace($(node).text() || $(node).attr('title')))
    .get()
    .filter(Boolean);

  const title =
    normalizeWhitespace(
      root.find('h1, h2, h3, h4, h5').first().text() ||
        textNodes.find((value) => /lejlighed|bolig|vaerelses|værelses/i.test(value))
    ) || 'Ukendt bolig';

  const address =
    textNodes.find((value) => /\b\d{4}\b/.test(value) && !/kr\.?\/md/i.test(value) && !/vær\.|m²|m2/i.test(value)) ||
    '';

  const floor = textNodes.find((value) => /sal|stuen|st\./i.test(value) && !/\d{4}/.test(value)) || null;
  const rooms = parseNumber(textNodes.find((value) => /vær\./i.test(value)));
  const sizeM2 = parseNumber(textNodes.find((value) => /m²|m2/i.test(value)));
  const monthlyPriceDkk = parseDanishPrice(textNodes.find((value) => /kr\.?\/md/i.test(value)));
  const status =
    normalizeWhitespace(root.find('[title]').first().attr('title') || textNodes.find((value) => /reserveret|lukket/i.test(value))) ||
    'ACTIVE';

  const id = createListingId({ url, title, address, source: options.source }, options.searchUrl);

  return {
    id,
    url,
    title,
    address,
    locationText: address,
    floor,
    rooms,
    sizeM2,
    monthlyPriceDkk,
    status,
    source: options.source,
    searchUrl: options.searchUrl,
    firstSeenAt: options.seenAt,
    lastSeenAt: options.seenAt,
    rawText
  };
}

/**
 * Parse listings from CEJ search result HTML.
 * The selectors are intentionally defensive because CEJ markup may vary.
 * @param {{ html: string, searchUrl: string, source: string, seenAt?: string }} options
 * @returns {Listing[]}
 */
export function parseListings(options) {
  const $ = cheerio.load(options.html);
  const seenAt = options.seenAt ?? new Date().toISOString();

  const cardSelectors = [
    'a[href^="/boliger/"]',
    '[data-listing-id]',
    '[data-testid*="listing"]',
    '.property-item',
    '.property-list-item',
    '.search-result',
    '.result-item',
    'article',
    'li'
  ];

  /** @type {cheerio.Cheerio<any>} */
  let cards = $();
  for (const selector of cardSelectors) {
    const matches = $(selector).filter((_, element) => {
      const node = $(element);
      return node.is('a[href]') || node.find('a[href]').length > 0;
    });
    if (matches.length > 0) {
      cards = matches;
      break;
    }
  }

  /** @type {Listing[]} */
  const listings = [];
  const seenIds = new Set();

  cards.each((_, element) => {
    const root = $(element);
    if (root.is('a[href^="/boliger/"]')) {
      const parsed = parseBoligCard($, element, { ...options, seenAt });
      if (!parsed || seenIds.has(parsed.id)) {
        return;
      }

      seenIds.add(parsed.id);
      listings.push(parsed);
      return;
    }

    const links = root.find('a[href]');
    const primaryLink = links
      .toArray()
      .map((node) => $(node))
      .find((link) => /property|bolig|lejebolig|rental|\/[a-z0-9-]+/i.test(link.attr('href') ?? '')) ?? links.first();

    const url = normalizeUrl(primaryLink.attr('href'), options.searchUrl);
    const textSegments = root
      .find('*')
      .map((__, node) => normalizeWhitespace($(node).text()))
      .get()
      .filter(Boolean);
    const ownText = normalizeWhitespace(root.text());
    const rawText = buildRawText([ownText]);
    const facts = extractFacts(textSegments);

    const title =
      normalizeWhitespace(
        root.find('h1, h2, h3, h4, .title, .property-title, [class*="title"]').first().text() ||
          primaryLink.text()
      ) || 'Ukendt bolig';

    const address = normalizeWhitespace(
      root.find('.address, [class*="address"], [data-testid*="address"]').first().text() ||
        facts.adresse ||
        facts.address
    );

    const locationText = normalizeWhitespace(
      root.find('.location, [class*="location"], [data-testid*="location"]').first().text() ||
        facts.omraade ||
        facts.by ||
        facts.postnr
    );

    const floor = normalizeWhitespace(
      root.find('.floor, [class*="floor"], [data-testid*="floor"]').first().text() || facts.etage || facts.floor
    ) || null;

    const rooms = parseNumber(
      root.find('.rooms, [class*="rooms"], [data-testid*="rooms"]').first().text() || facts.vaerelser || facts.rooms
    );

    const sizeM2 = parseNumber(
      root.find('.size, [class*="size"], [data-testid*="size"]').first().text() || facts.storrelse || facts.area
    );

    const monthlyPriceDkk = parseDanishPrice(
      root.find('.price, [class*="price"], [data-testid*="price"]').first().text() || facts.husleje || facts.price
    );

    const status = normalizeWhitespace(
      root.find('.status, [class*="status"], [data-testid*="status"]').first().text() || facts.status
    ) || 'ACTIVE';

    if (!url && !title && !address) {
      return;
    }

    const id = createListingId({ url, title, address, source: options.source }, options.searchUrl);
    if (seenIds.has(id)) {
      return;
    }

    seenIds.add(id);
    listings.push({
      id,
      url,
      title,
      address,
      locationText,
      floor,
      rooms,
      sizeM2,
      monthlyPriceDkk,
      status,
      source: options.source,
      searchUrl: options.searchUrl,
      firstSeenAt: seenAt,
      lastSeenAt: seenAt,
      rawText
    });
  });

  return listingArraySchema.parse(listings);
}
