import { fileURLToPath } from 'node:url';

import { config } from './config.js';
import { isDiscordConfigured, sendDiscordMessage } from './services/discord.js';

const mode = process.argv[2] ?? 'manual';

export const titles = {
  manual: 'Discord test notification',
  started: 'Tracker turned on',
  stopped: 'Tracker turned off'
};

export const bodies = {
  manual: `This is a test notification from cej-tracker.\n\nIf you received this, the Discord webhook is configured correctly.`,
  started: `cej-tracker has been turned on.\n\nThe dashboard and cron flow were started successfully.`,
  stopped: `cej-tracker has been turned off.\n\nThe dashboard was stopped and the cron flow was removed.`
};

const entryPath = process.argv[1];
const currentPath = fileURLToPath(import.meta.url);

if (entryPath && currentPath === entryPath) {
  if (!isDiscordConfigured(config.discord)) {
    console.error('Discord is not configured. Fill in CEJ_DISCORD_WEBHOOK_URL in .env.local first.');
    process.exit(1);
  }

  await sendDiscordMessage({
    discord: config.discord,
    title: titles[mode] ?? titles.manual,
    text: bodies[mode] ?? bodies.manual
  });

  console.log(`Sent "${titles[mode] ?? titles.manual}" Discord notification.`);
}
