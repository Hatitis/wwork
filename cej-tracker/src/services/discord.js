/**
 * @param {{ webhookUrl: string, username: string, avatarUrl: string, mentionUserId: string, mentionRoleId: string }} discordConfig
 * @returns {boolean}
 */
export function isDiscordConfigured(discordConfig) {
  return Boolean(discordConfig.webhookUrl);
}

/**
 * @param {{ mentionUserId: string, mentionRoleId: string }} discordConfig
 * @returns {{ prefix: string, allowedMentions: { parse: string[], users?: string[], roles?: string[] } }}
 */
function buildMentions(discordConfig) {
  if (discordConfig.mentionUserId) {
    return {
      prefix: `<@${discordConfig.mentionUserId}>`,
      allowedMentions: {
        parse: [],
        users: [discordConfig.mentionUserId]
      }
    };
  }

  if (discordConfig.mentionRoleId) {
    return {
      prefix: `<@&${discordConfig.mentionRoleId}>`,
      allowedMentions: {
        parse: [],
        roles: [discordConfig.mentionRoleId]
      }
    };
  }

  return {
    prefix: '',
    allowedMentions: {
      parse: []
    }
  };
}

/**
 * Split long Discord messages into smaller chunks.
 * @param {string} value
 * @returns {string[]}
 */
function chunkMessage(value) {
  /** @type {string[]} */
  const chunks = [];
  let remaining = value;

  while (remaining.length > 1900) {
    const slice = remaining.slice(0, 1900);
    const breakIndex = slice.lastIndexOf('\n');
    const chunk = breakIndex > 100 ? slice.slice(0, breakIndex) : slice;
    chunks.push(chunk);
    remaining = remaining.slice(chunk.length).trimStart();
  }

  if (remaining.length > 0) {
    chunks.push(remaining);
  }

  return chunks;
}

/**
 * @param {{ discord: { webhookUrl: string, username: string, avatarUrl: string, mentionUserId: string, mentionRoleId: string }, title: string, text: string }} options
 * @returns {Promise<void>}
 */
export async function sendDiscordMessage(options) {
  const mentions = buildMentions(options.discord);
  const payloads = chunkMessage(
    `${mentions.prefix ? `${mentions.prefix}\n` : ''}**${options.title}**\n${options.text}`
  );

  for (const content of payloads) {
    const response = await fetch(options.discord.webhookUrl, {
      method: 'POST',
      headers: {
        'content-type': 'application/json'
      },
      body: JSON.stringify({
        content,
        username: options.discord.username || 'CEJ Tracker',
        avatar_url: options.discord.avatarUrl || undefined,
        allowed_mentions: mentions.allowedMentions
      })
    });

    if (!response.ok) {
      const responseText = await response.text();
      throw new Error(`Discord webhook failed with status ${response.status}: ${responseText}`);
    }
  }
}
