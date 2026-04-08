import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const projectRoot = path.resolve(__dirname, '..');
const envFilePath = path.join(projectRoot, '.env.local');

/**
 * Load simple KEY=value pairs from .env.local into process.env without
 * overwriting already-exported shell values.
 * @param {string} filePath
 * @returns {void}
 */
function loadLocalEnv(filePath) {
  if (!fs.existsSync(filePath)) {
    return;
  }

  const contents = fs.readFileSync(filePath, 'utf8');
  for (const line of contents.split(/\r?\n/)) {
    const trimmed = line.trim();
    if (!trimmed || trimmed.startsWith('#')) {
      continue;
    }

    const separatorIndex = trimmed.indexOf('=');
    if (separatorIndex <= 0) {
      continue;
    }

    const key = trimmed.slice(0, separatorIndex).trim();
    if (!key || process.env[key] !== undefined) {
      continue;
    }

    let value = trimmed.slice(separatorIndex + 1).trim();
    if (
      (value.startsWith("'") && value.endsWith("'")) ||
      (value.startsWith('"') && value.endsWith('"'))
    ) {
      value = value.slice(1, -1);
    }

    process.env[key] = value;
  }
}

loadLocalEnv(envFilePath);

/**
 * Runtime configuration for the tracker.
 */
export const config = {
  projectRoot,
  dataDir: path.join(projectRoot, 'data'),
  snapshotPath: path.join(projectRoot, 'data', 'snapshot.json'),
  historyPath: path.join(projectRoot, 'data', 'history.json'),
  statePath: path.join(projectRoot, 'data', 'state.json'),
  searchUrl: process.env.CEJ_SEARCH_URL ?? '',
  source: process.env.CEJ_SOURCE ?? 'cej',
  requestTimeoutMs: Number.parseInt(process.env.CEJ_REQUEST_TIMEOUT_MS ?? '15000', 10),
  userAgent:
    process.env.CEJ_USER_AGENT ??
    'cej-tracker/1.0 (+https://example.local; Node.js built-in fetch)',
  logLevel: process.env.LOG_LEVEL ?? 'info',
  webPort: Number.parseInt(process.env.CEJ_WEB_PORT ?? '8787', 10),
  publicSiteUrl: process.env.CEJ_PUBLIC_SITE_URL ?? '',
  heartbeat: {
    enabled: (process.env.CEJ_HEARTBEAT_ENABLED ?? 'true').toLowerCase() === 'true',
    hourLocal: Number.parseInt(process.env.CEJ_HEARTBEAT_HOUR_LOCAL ?? '8', 10),
    timezone: process.env.CEJ_HEARTBEAT_TIMEZONE ?? 'Europe/Copenhagen'
  },
  discord: {
    webhookUrl: process.env.CEJ_DISCORD_WEBHOOK_URL ?? '',
    username: process.env.CEJ_DISCORD_USERNAME ?? 'CEJ Tracker',
    avatarUrl: process.env.CEJ_DISCORD_AVATAR_URL ?? '',
    mentionUserId: process.env.CEJ_DISCORD_MENTION_USER_ID ?? '',
    mentionRoleId: process.env.CEJ_DISCORD_MENTION_ROLE_ID ?? ''
  }
};

/**
 * Throw if required runtime config is missing.
 * @returns {void}
 */
export function assertConfig() {
  if (!config.searchUrl) {
    throw new Error('Missing CEJ_SEARCH_URL environment variable.');
  }
}
