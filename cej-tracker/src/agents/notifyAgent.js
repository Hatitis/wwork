import { sendDiscordMessage, isDiscordConfigured } from '../services/discord.js';
import { formatEventType, formatListingSummary } from '../utils/formatters.js';

/**
 * Build a batched notification message.
 * @param {{ events: import('../models/eventSchema.js').eventSchema._type[], listingsById: Map<string, any> }} options
 * @returns {{ subject: string, text: string }}
 */
export function buildNotificationMessage(options) {
  if (options.events.length === 0) {
    return {
      subject: 'Ingen ændringer',
      text: 'No listing changes detected.'
    };
  }

  const lines = options.events.map((event) => {
    const listing = options.listingsById.get(event.listingId);
    const summary = listing ? formatListingSummary(listing) : event.title || event.listingUrl;
    const url = listing?.url || event.listingUrl;
    return `- ${formatEventType(event.type)} (${event.type}): ${summary}\n  ${url}`;
  });

  return {
    subject: `${options.events.length} change(s) detected`,
    text: `Detected ${options.events.length} change(s):\n${lines.join('\n')}`
  };
}

/**
 * Emit notifications for the current run.
 * @param {{ events: import('../models/eventSchema.js').eventSchema._type[], listingsById: Map<string, any>, logger: { info: Function, warn: Function } , discord: { webhookUrl: string, username: string, avatarUrl: string } }} options
 * @returns {Promise<void>}
 */
export async function notifyEvents(options) {
  const message = buildNotificationMessage(options);

  options.logger.info(message.text);

  if (options.events.length === 0 || !isDiscordConfigured(options.discord)) {
    return;
  }

  try {
    await sendDiscordMessage({
      discord: options.discord,
      title: message.subject,
      text: message.text
    });
    options.logger.info('Discord notification sent.');
  } catch (error) {
    options.logger.warn(`Discord notification failed: ${/** @type {Error} */ (error).message}`);
  }
}

/**
 * @param {string} isoTimestamp
 * @param {string} timeZone
 * @returns {{ localDate: string, localHour: number }}
 */
function getLocalDateParts(isoTimestamp, timeZone) {
  const formatter = new Intl.DateTimeFormat('en-CA', {
    timeZone,
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    hour12: false
  });
  const parts = Object.fromEntries(formatter.formatToParts(new Date(isoTimestamp)).map((part) => [part.type, part.value]));
  return {
    localDate: `${parts.year}-${parts.month}-${parts.day}`,
    localHour: Number.parseInt(parts.hour, 10)
  };
}

/**
 * @param {{ startedAt: string, previousState: Record<string, unknown>, listingsCount: number, discord: { webhookUrl: string, username: string, avatarUrl: string, mentionUserId: string, mentionRoleId: string }, heartbeat: { enabled: boolean, hourLocal: number, timezone: string }, logger: { info: Function, warn: Function } }} options
 * @returns {Promise<{ lastHeartbeatLocalDate?: string, lastHeartbeatSentAt?: string }>}
 */
export async function maybeSendHeartbeat(options) {
  if (!options.heartbeat.enabled || !isDiscordConfigured(options.discord)) {
    return {};
  }

  const { localDate, localHour } = getLocalDateParts(options.startedAt, options.heartbeat.timezone);
  if (localHour !== options.heartbeat.hourLocal) {
    return {};
  }

  if (options.previousState.lastHeartbeatLocalDate === localDate) {
    return {};
  }

  try {
    await sendDiscordMessage({
      discord: options.discord,
      title: 'Morning heartbeat',
      text: `Tracker healthy.\nLocal date: ${localDate}\nCurrent listings: ${options.listingsCount}\nLast success: ${options.startedAt}`
    });
    options.logger.info(`Morning heartbeat sent for ${localDate}.`);
    return {
      lastHeartbeatLocalDate: localDate,
      lastHeartbeatSentAt: options.startedAt
    };
  } catch (error) {
    options.logger.warn(`Heartbeat notification failed: ${/** @type {Error} */ (error).message}`);
    return {};
  }
}
