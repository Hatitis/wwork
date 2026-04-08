import crypto from 'node:crypto';

import { normalizeWhitespace, normalizeUrl } from './normalize.js';

/**
 * Create a stable listing identity, preferring the listing URL.
 * @param {{ url?: string, title?: string, address?: string, source?: string }} listing
 * @param {string} baseUrl
 * @returns {string}
 */
export function createListingId(listing, baseUrl) {
  const normalizedUrl = normalizeUrl(listing.url, baseUrl);
  if (normalizedUrl) {
    return normalizedUrl;
  }

  const fallback = [
    normalizeWhitespace(listing.title),
    normalizeWhitespace(listing.address),
    normalizeWhitespace(listing.source)
  ].join('|');

  return crypto.createHash('sha1').update(fallback).digest('hex');
}
