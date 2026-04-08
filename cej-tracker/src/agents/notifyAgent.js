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
