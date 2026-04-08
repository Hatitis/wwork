import { z } from 'zod';

export const eventTypes = /** @type {const} */ ([
  'LISTING_NEW',
  'LISTING_REMOVED',
  'STATUS_CHANGED',
  'PRICE_CHANGED',
  'DETAILS_CHANGED'
]);

/**
 * Listing change event.
 */
export const eventSchema = z.object({
  type: z.enum(eventTypes),
  listingId: z.string().min(1),
  listingUrl: z.string().min(1),
  occurredAt: z.string().datetime(),
  source: z.string().min(1),
  searchUrl: z.string().min(1),
  title: z.string().default(''),
  summary: z.string().min(1),
  previous: z.record(z.unknown()).default({}),
  current: z.record(z.unknown()).default({})
});

export const eventArraySchema = z.array(eventSchema);
