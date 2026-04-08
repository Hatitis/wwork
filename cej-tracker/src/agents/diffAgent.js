import { eventArraySchema } from '../models/eventSchema.js';
import { detailKeys } from '../utils/guards.js';

/**
 * @typedef {import('../models/listingSchema.js').listingSchema._type} Listing
 */

/**
 * Compare the latest listing set against the prior snapshot.
 * @param {{ previousListings: Listing[], currentListings: Listing[], occurredAt?: string }} options
 * @returns {{ mergedListings: Listing[], events: import('../models/eventSchema.js').eventSchema._type[] }}
 */
export function diffListings(options) {
  const occurredAt = options.occurredAt ?? new Date().toISOString();
  const previousMap = new Map(options.previousListings.map((listing) => [listing.id, listing]));
  const currentMap = new Map(options.currentListings.map((listing) => [listing.id, listing]));

  /** @type {Listing[]} */
  const mergedListings = [];
  /** @type {import('../models/eventSchema.js').eventSchema._type[]} */
  const events = [];

  for (const currentListing of options.currentListings) {
    const previousListing = previousMap.get(currentListing.id);
    if (!previousListing) {
      mergedListings.push(currentListing);
      events.push({
        type: 'LISTING_NEW',
        listingId: currentListing.id,
        listingUrl: currentListing.url,
        occurredAt,
        source: currentListing.source,
        searchUrl: currentListing.searchUrl,
        title: currentListing.title,
        summary: `New listing: ${currentListing.title}`,
        previous: {},
        current: currentListing
      });
      continue;
    }

    const mergedListing = {
      ...currentListing,
      firstSeenAt: previousListing.firstSeenAt,
      lastSeenAt: occurredAt
    };
    mergedListings.push(mergedListing);

    if (previousListing.status !== currentListing.status) {
      events.push({
        type: 'STATUS_CHANGED',
        listingId: currentListing.id,
        listingUrl: currentListing.url,
        occurredAt,
        source: currentListing.source,
        searchUrl: currentListing.searchUrl,
        title: currentListing.title,
        summary: `Status changed: ${previousListing.status} -> ${currentListing.status}`,
        previous: { status: previousListing.status },
        current: { status: currentListing.status }
      });
    }

    if (previousListing.monthlyPriceDkk !== currentListing.monthlyPriceDkk) {
      events.push({
        type: 'PRICE_CHANGED',
        listingId: currentListing.id,
        listingUrl: currentListing.url,
        occurredAt,
        source: currentListing.source,
        searchUrl: currentListing.searchUrl,
        title: currentListing.title,
        summary: `Price changed: ${previousListing.monthlyPriceDkk} -> ${currentListing.monthlyPriceDkk}`,
        previous: { monthlyPriceDkk: previousListing.monthlyPriceDkk },
        current: { monthlyPriceDkk: currentListing.monthlyPriceDkk }
      });
    }

    const detailChanges = detailKeys.filter((key) => previousListing[key] !== currentListing[key]);
    if (detailChanges.length > 0) {
      events.push({
        type: 'DETAILS_CHANGED',
        listingId: currentListing.id,
        listingUrl: currentListing.url,
        occurredAt,
        source: currentListing.source,
        searchUrl: currentListing.searchUrl,
        title: currentListing.title,
        summary: `Details changed: ${detailChanges.join(', ')}`,
        previous: Object.fromEntries(detailChanges.map((key) => [key, previousListing[key]])),
        current: Object.fromEntries(detailChanges.map((key) => [key, currentListing[key]]))
      });
    }
  }

  for (const previousListing of options.previousListings) {
    if (currentMap.has(previousListing.id)) {
      continue;
    }

    events.push({
      type: 'LISTING_REMOVED',
      listingId: previousListing.id,
      listingUrl: previousListing.url,
      occurredAt,
      source: previousListing.source,
      searchUrl: previousListing.searchUrl,
      title: previousListing.title,
      summary: `Listing removed: ${previousListing.title}`,
      previous: previousListing,
      current: {}
    });
  }

  return {
    mergedListings,
    events: eventArraySchema.parse(events)
  };
}
