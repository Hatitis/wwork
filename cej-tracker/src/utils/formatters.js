/**
 * Format an integer price in DKK for console output.
 * @param {number | null | undefined} value
 * @returns {string}
 */
export function formatPriceDkk(value) {
  if (typeof value !== 'number') {
    return 'ukendt pris';
  }

  return `${new Intl.NumberFormat('da-DK').format(value)} kr./md`;
}

/**
 * Short human-readable summary line for a listing.
 * @param {{ title?: string, address?: string, sizeM2?: number | null, rooms?: number | null, monthlyPriceDkk?: number | null }} listing
 * @returns {string}
 */
export function formatListingSummary(listing) {
  const parts = [
    listing.title || listing.address || 'Ukendt bolig',
    listing.address || null,
    listing.sizeM2 ? `${listing.sizeM2} m2` : null,
    listing.rooms ? `${listing.rooms} vaer.` : null,
    formatPriceDkk(listing.monthlyPriceDkk)
  ];

  return parts.filter(Boolean).join(' | ');
}

/**
 * Format a timestamp for humans.
 * @param {string | null | undefined} value
 * @returns {string}
 */
export function formatTimestamp(value) {
  if (!value) {
    return 'Aldrig';
  }

  return new Intl.DateTimeFormat('da-DK', {
    dateStyle: 'medium',
    timeStyle: 'short'
  }).format(new Date(value));
}

/**
 * Create a compact label for a change event.
 * @param {string} eventType
 * @returns {string}
 */
export function formatEventType(eventType) {
  return (
    {
      LISTING_NEW: 'Ny',
      LISTING_REMOVED: 'Fjernet',
      STATUS_CHANGED: 'Status',
      PRICE_CHANGED: 'Pris',
      DETAILS_CHANGED: 'Detaljer'
    }[eventType] ?? eventType
  );
}
