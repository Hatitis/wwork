import { z } from 'zod';

/**
 * Listing snapshot entry.
 */
export const listingSchema = z.object({
  id: z.string().min(1),
  url: z.string().min(1),
  title: z.string().default(''),
  address: z.string().default(''),
  locationText: z.string().default(''),
  floor: z.string().nullable().default(null),
  rooms: z.number().nullable().default(null),
  sizeM2: z.number().nullable().default(null),
  monthlyPriceDkk: z.number().int().nullable().default(null),
  status: z.string().default('UNKNOWN'),
  source: z.string().min(1),
  searchUrl: z.string().min(1),
  firstSeenAt: z.string().datetime(),
  lastSeenAt: z.string().datetime(),
  rawText: z.string().default('')
});

export const listingArraySchema = z.array(listingSchema);
