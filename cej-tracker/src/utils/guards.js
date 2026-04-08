/**
 * True when a parsed listing collection should be treated as valid.
 * @param {unknown[]} listings
 * @returns {boolean}
 */
export function hasListings(listings) {
  return Array.isArray(listings) && listings.length > 0;
}

/**
 * Fields that should count as material details for diffing.
 * @type {Array<'title' | 'address' | 'locationText' | 'floor' | 'rooms' | 'sizeM2'>}
 */
export const detailKeys = ['title', 'address', 'locationText', 'floor', 'rooms', 'sizeM2'];
