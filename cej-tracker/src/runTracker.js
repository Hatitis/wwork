import { config } from './config.js';
import { diffListings } from './agents/diffAgent.js';
import { fetchSearchPage } from './agents/fetchAgent.js';
import { notifyEvents } from './agents/notifyAgent.js';
import { parseListings } from './agents/parseAgent.js';
import { eventArraySchema } from './models/eventSchema.js';
import { listingArraySchema } from './models/listingSchema.js';
import { createLogger } from './services/logger.js';
import { ensureDirectory, readJson, writeJson } from './services/storage.js';
import { hasListings } from './utils/guards.js';

/**
 * @typedef {import('./models/listingSchema.js').listingSchema._type} Listing
 */

/**
 * Run the tracker once.
 * @returns {Promise<{ ok: boolean, events: import('./models/eventSchema.js').eventSchema._type[], listings: Listing[] }>}
 */
export async function runTracker() {
  const logger = createLogger(config.logLevel);
  const startedAt = new Date().toISOString();

  await ensureDirectory(config.dataDir);

  const [previousSnapshotRaw, historyRaw, state] = await Promise.all([
    readJson(config.snapshotPath, []),
    readJson(config.historyPath, []),
    readJson(config.statePath, {
      lastRunAt: null,
      lastSuccessAt: null,
      lastFetchAt: null,
      lastParseAt: null,
      lastError: null,
      consecutiveFailures: 0,
      lastSnapshotCount: 0
    })
  ]);

  const previousSnapshot = listingArraySchema.parse(previousSnapshotRaw);
  const history = eventArraySchema.parse(historyRaw);

  await writeJson(config.statePath, {
    ...state,
    lastRunAt: startedAt,
    searchUrl: config.searchUrl
  });

  logger.info(`Fetching listings from ${config.searchUrl}`);
  const fetchResult = await fetchSearchPage({
    searchUrl: config.searchUrl,
    requestTimeoutMs: config.requestTimeoutMs,
    userAgent: config.userAgent
  });

  if (!fetchResult.ok) {
    logger.error('Fetch failed, leaving snapshot untouched.', fetchResult.error.message);
    await writeJson(config.statePath, {
      ...state,
      lastRunAt: startedAt,
      searchUrl: config.searchUrl,
      lastError: fetchResult.error.message,
      consecutiveFailures: (state.consecutiveFailures ?? 0) + 1
    });
    return { ok: false, events: [], listings: previousSnapshot };
  }

  /** @type {Listing[]} */
  let currentListings;
  try {
    currentListings = parseListings({
      html: fetchResult.html,
      searchUrl: config.searchUrl,
      source: config.source,
      seenAt: startedAt
    });
  } catch (error) {
    logger.error('Parsing failed, leaving snapshot untouched.', /** @type {Error} */ (error).message);
    await writeJson(config.statePath, {
      ...state,
      lastRunAt: startedAt,
      lastFetchAt: fetchResult.fetchedAt,
      searchUrl: config.searchUrl,
      lastError: /** @type {Error} */ (error).message,
      consecutiveFailures: (state.consecutiveFailures ?? 0) + 1
    });
    return { ok: false, events: [], listings: previousSnapshot };
  }

  if (!hasListings(currentListings)) {
    logger.warn('Parser returned zero listings, preserving the last good snapshot.');
    await writeJson(config.statePath, {
      ...state,
      lastRunAt: startedAt,
      lastFetchAt: fetchResult.fetchedAt,
      lastParseAt: startedAt,
      searchUrl: config.searchUrl,
      lastError: 'Parser returned zero listings.',
      consecutiveFailures: (state.consecutiveFailures ?? 0) + 1,
      lastSnapshotCount: previousSnapshot.length
    });
    return { ok: false, events: [], listings: previousSnapshot };
  }

  const { mergedListings, events } = diffListings({
    previousListings: previousSnapshot,
    currentListings,
    occurredAt: startedAt
  });

  await Promise.all([
    writeJson(config.snapshotPath, mergedListings),
    writeJson(config.historyPath, [...history, ...events]),
    writeJson(config.statePath, {
      ...state,
      lastRunAt: startedAt,
      lastSuccessAt: startedAt,
      lastFetchAt: fetchResult.fetchedAt,
      lastParseAt: startedAt,
      searchUrl: config.searchUrl,
      lastError: null,
      consecutiveFailures: 0,
      lastSnapshotCount: mergedListings.length
    })
  ]);

  await notifyEvents({
    events,
    listingsById: new Map(mergedListings.map((listing) => [listing.id, listing])),
    logger,
    discord: config.discord
  });

  return { ok: true, events, listings: mergedListings };
}
