/**
 * Normalize whitespace in free-form text.
 * @param {string | null | undefined} value
 * @returns {string}
 */
export function normalizeWhitespace(value) {
  return (value ?? '').replace(/\s+/g, ' ').trim();
}

/**
 * Convert a possibly relative URL to an absolute one.
 * @param {string | null | undefined} url
 * @param {string} baseUrl
 * @returns {string}
 */
export function normalizeUrl(url, baseUrl) {
  const normalized = normalizeWhitespace(url);
  if (!normalized) {
    return '';
  }

  try {
    return new URL(normalized, baseUrl).toString();
  } catch {
    return normalized;
  }
}

/**
 * Parse Danish price strings like "7.758 kr./md" into an integer.
 * @param {string | null | undefined} value
 * @returns {number | null}
 */
export function parseDanishPrice(value) {
  const normalized = normalizeWhitespace(value);
  if (!normalized) {
    return null;
  }

  const match = normalized.match(/([\d.\s]+)\s*kr/i);
  if (!match) {
    return null;
  }

  const digits = match[1].replace(/[.\s]/g, '');
  const parsed = Number.parseInt(digits, 10);
  return Number.isNaN(parsed) ? null : parsed;
}

/**
 * Parse the first numeric value from a string, keeping decimals.
 * @param {string | null | undefined} value
 * @returns {number | null}
 */
export function parseNumber(value) {
  const normalized = normalizeWhitespace(value).replace(',', '.');
  const match = normalized.match(/(\d+(?:\.\d+)?)/);
  if (!match) {
    return null;
  }

  const parsed = Number.parseFloat(match[1]);
  return Number.isNaN(parsed) ? null : parsed;
}

/**
 * Build a single searchable text blob from an array of text values.
 * @param {Array<string | null | undefined>} values
 * @returns {string}
 */
export function buildRawText(values) {
  return normalizeWhitespace(values.filter(Boolean).join(' '));
}
