/**
 * Fetch the raw HTML for a search page.
 * @param {{ searchUrl: string, requestTimeoutMs: number, userAgent: string }} options
 * @returns {Promise<{ ok: true, html: string, fetchedAt: string } | { ok: false, error: Error, fetchedAt: string }>}
 */
export async function fetchSearchPage(options) {
  const fetchedAt = new Date().toISOString();
  const controller = new AbortController();
  const timeout = setTimeout(() => controller.abort(), options.requestTimeoutMs);

  try {
    const response = await fetch(options.searchUrl, {
      headers: {
        'user-agent': options.userAgent,
        accept: 'text/html,application/xhtml+xml'
      },
      signal: controller.signal
    });

    if (!response.ok) {
      return {
        ok: false,
        error: new Error(`Fetch failed with status ${response.status}`),
        fetchedAt
      };
    }

    const html = await response.text();
    return { ok: true, html, fetchedAt };
  } catch (error) {
    return {
      ok: false,
      error: /** @type {Error} */ (error),
      fetchedAt
    };
  } finally {
    clearTimeout(timeout);
  }
}
