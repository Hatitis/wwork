import http from 'node:http';
import { fileURLToPath } from 'node:url';

import { config } from './config.js';
import { eventArraySchema } from './models/eventSchema.js';
import { listingArraySchema } from './models/listingSchema.js';
import { readJson } from './services/storage.js';
import { formatEventType, formatListingSummary, formatPriceDkk, formatTimestamp } from './utils/formatters.js';

/**
 * @param {string} value
 * @returns {string}
 */
function escapeHtml(value) {
  return value
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#39;');
}

/**
 * @param {import('./models/listingSchema.js').listingSchema._type[]} listings
 * @param {import('./models/eventSchema.js').eventSchema._type[]} events
 * @param {Record<string, unknown>} state
 * @returns {string}
 */
export function renderPage(listings, events, state) {
  const latestEvents = [...events].reverse().slice(0, 30);
  const statusCounts = listings.reduce(
    (accumulator, listing) => {
      accumulator[listing.status] = (accumulator[listing.status] ?? 0) + 1;
      return accumulator;
    },
    /** @type {Record<string, number>} */ ({})
  );
  const publicSiteUrl = String(state.publicSiteUrl ?? config.publicSiteUrl ?? '');

  return `<!doctype html>
<html lang="da">
  <head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>CEJ Tracker</title>
    <style>
      :root {
        color-scheme: light;
        --bg: #f3efe6;
        --panel: rgba(255, 252, 246, 0.86);
        --ink: #1f2a1f;
        --muted: #6d7468;
        --accent: #2f6a4f;
        --accent-2: #d96f32;
        --line: rgba(31, 42, 31, 0.1);
      }
      * { box-sizing: border-box; }
      body {
        margin: 0;
        font-family: Georgia, "Times New Roman", serif;
        color: var(--ink);
        background:
          radial-gradient(circle at top left, rgba(217, 111, 50, 0.14), transparent 30%),
          radial-gradient(circle at top right, rgba(47, 106, 79, 0.14), transparent 28%),
          linear-gradient(180deg, #f8f3e9 0%, var(--bg) 100%);
      }
      .shell {
        width: min(1180px, calc(100% - 32px));
        margin: 0 auto;
        padding: 32px 0 48px;
      }
      .hero {
        display: grid;
        gap: 16px;
        padding: 28px;
        border: 1px solid var(--line);
        border-radius: 28px;
        background: linear-gradient(135deg, rgba(255,255,255,0.82), rgba(255,248,238,0.95));
        box-shadow: 0 18px 50px rgba(64, 53, 35, 0.08);
      }
      h1, h2, h3, p { margin: 0; }
      h1 { font-size: clamp(2rem, 4vw, 3.6rem); line-height: 0.95; }
      .subtle { color: var(--muted); }
      .stats {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
        gap: 14px;
        margin-top: 8px;
      }
      .stat, .panel {
        border: 1px solid var(--line);
        border-radius: 22px;
        background: var(--panel);
        backdrop-filter: blur(8px);
      }
      .stat { padding: 18px; }
      .stat strong { display: block; font-size: 1.9rem; margin-top: 6px; }
      .layout {
        display: grid;
        gap: 20px;
        grid-template-columns: 1.5fr 1fr;
        margin-top: 22px;
      }
      .panel { padding: 22px; }
      .panel h2 { margin-bottom: 14px; font-size: 1.4rem; }
      .listing-grid {
        display: grid;
        gap: 14px;
      }
      .listing {
        border: 1px solid var(--line);
        border-radius: 18px;
        padding: 16px;
        background: rgba(255,255,255,0.72);
      }
      .listing-top, .event-top {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 12px;
      }
      .price { color: var(--accent); font-weight: 700; }
      .badge {
        display: inline-flex;
        padding: 5px 10px;
        border-radius: 999px;
        background: rgba(47, 106, 79, 0.12);
        color: var(--accent);
        font-size: 0.82rem;
      }
      .listing-meta, .event-meta {
        display: flex;
        flex-wrap: wrap;
        gap: 10px;
        margin-top: 10px;
        color: var(--muted);
        font-size: 0.95rem;
      }
      .listing a {
        color: inherit;
        text-decoration: none;
      }
      .listing a:hover {
        text-decoration: underline;
      }
      .event-list {
        display: grid;
        gap: 12px;
      }
      .event {
        border-left: 4px solid var(--accent-2);
        padding: 12px 14px;
        background: rgba(255,255,255,0.6);
        border-radius: 0 14px 14px 0;
      }
      code {
        font-family: "SFMono-Regular", ui-monospace, monospace;
        font-size: 0.88rem;
        background: rgba(31,42,31,0.05);
        padding: 2px 6px;
        border-radius: 8px;
      }
      .small-note {
        font-size: 0.92rem;
        color: var(--muted);
      }
      @media (max-width: 920px) {
        .layout { grid-template-columns: 1fr; }
        .shell { width: min(100% - 20px, 1180px); }
      }
    </style>
  </head>
  <body>
    <main class="shell">
      <section class="hero">
        <p class="subtle">Live view over the tracker data files</p>
        <h1>CEJ Tracker Dashboard</h1>
        <p class="subtle">Search URL: <code>${escapeHtml(String(state.searchUrl ?? config.searchUrl ?? ''))}</code></p>
        ${
          publicSiteUrl
            ? `<p class="subtle">Public site: <a href="${escapeHtml(publicSiteUrl)}" target="_blank" rel="noreferrer"><code>${escapeHtml(publicSiteUrl)}</code></a></p>`
            : ''
        }
        <p class="small-note">This dashboard auto-refreshes from the published JSON about once a minute.</p>
        <div class="stats">
          <div class="stat"><span class="subtle">Current listings</span><strong id="current-listings-count">${listings.length}</strong></div>
          <div class="stat"><span class="subtle">Events logged</span><strong id="events-logged-count">${events.length}</strong></div>
          <div class="stat"><span class="subtle">Last success</span><strong id="last-success-value">${escapeHtml(formatTimestamp(/** @type {string | null | undefined} */ (state.lastSuccessAt)))}</strong></div>
          <div class="stat"><span class="subtle">Failures in a row</span><strong id="failures-in-row-value">${escapeHtml(String(state.consecutiveFailures ?? 0))}</strong></div>
        </div>
      </section>
      <section class="layout">
        <section class="panel">
          <h2>Current Listings</h2>
          <div class="listing-grid" id="listings-container">
            ${listings
              .map(
                (listing) => `<article class="listing">
                  <div class="listing-top">
                    <div>
                      <h3><a href="${escapeHtml(listing.url)}" target="_blank" rel="noreferrer">${escapeHtml(listing.title)}</a></h3>
                      <p class="subtle">${escapeHtml(listing.address || listing.locationText || 'Ukendt adresse')}</p>
                    </div>
                    <div class="price">${escapeHtml(formatPriceDkk(listing.monthlyPriceDkk))}</div>
                  </div>
                  <div class="listing-meta">
                    <span class="badge">${escapeHtml(listing.status)}</span>
                    <span>${escapeHtml(String(listing.rooms ?? '?'))} vær.</span>
                    <span>${escapeHtml(String(listing.sizeM2 ?? '?'))} m2</span>
                    <span>${escapeHtml(listing.floor ?? 'Ukendt etage')}</span>
                    <span>Først set ${escapeHtml(formatTimestamp(listing.firstSeenAt))}</span>
                  </div>
                </article>`
              )
              .join('')}
          </div>
        </section>
        <aside class="panel">
          <h2>Status and Recent Changes</h2>
          <p class="subtle" id="status-summary">Statusfordeling: ${escapeHtml(
            Object.entries(statusCounts)
              .map(([status, count]) => `${status} ${count}`)
              .join(' · ') || 'Ingen'
          )}</p>
          <div class="event-list" id="events-container" style="margin-top: 16px;">
            ${latestEvents
              .map(
                (event) => `<article class="event">
                  <div class="event-top">
                    <strong>${escapeHtml(formatEventType(event.type))}</strong>
                    <span class="subtle">${escapeHtml(formatTimestamp(event.occurredAt))}</span>
                  </div>
                  <p style="margin-top: 6px;">${escapeHtml(event.title || event.summary)}</p>
                  <div class="event-meta">
                    <span>${escapeHtml(event.summary)}</span>
                  </div>
                </article>`
              )
              .join('')}
          </div>
        </aside>
      </section>
    </main>
    <script>
      const formatters = {
        timestamp(value) {
          if (!value) return 'Aldrig';
          return new Intl.DateTimeFormat('da-DK', { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(value));
        },
        price(value) {
          if (typeof value !== 'number') return 'ukendt pris';
          return new Intl.NumberFormat('da-DK').format(value) + ' kr./md';
        },
        eventType(value) {
          return {
            LISTING_NEW: 'Ny',
            LISTING_REMOVED: 'Fjernet',
            STATUS_CHANGED: 'Status',
            PRICE_CHANGED: 'Pris',
            DETAILS_CHANGED: 'Detaljer'
          }[value] || value;
        }
      };

      function escapeHtmlClient(value) {
        return String(value)
          .replaceAll('&', '&amp;')
          .replaceAll('<', '&lt;')
          .replaceAll('>', '&gt;')
          .replaceAll('"', '&quot;')
          .replaceAll("'", '&#39;');
      }

      function renderListings(listings) {
        return listings.map((listing) => \`<article class="listing">
          <div class="listing-top">
            <div>
              <h3><a href="\${escapeHtmlClient(listing.url)}" target="_blank" rel="noreferrer">\${escapeHtmlClient(listing.title)}</a></h3>
              <p class="subtle">\${escapeHtmlClient(listing.address || listing.locationText || 'Ukendt adresse')}</p>
            </div>
            <div class="price">\${escapeHtmlClient(formatters.price(listing.monthlyPriceDkk))}</div>
          </div>
          <div class="listing-meta">
            <span class="badge">\${escapeHtmlClient(listing.status)}</span>
            <span>\${escapeHtmlClient(String(listing.rooms ?? '?'))} vær.</span>
            <span>\${escapeHtmlClient(String(listing.sizeM2 ?? '?'))} m2</span>
            <span>\${escapeHtmlClient(listing.floor ?? 'Ukendt etage')}</span>
            <span>Først set \${escapeHtmlClient(formatters.timestamp(listing.firstSeenAt))}</span>
          </div>
        </article>\`).join('');
      }

      function renderEvents(events) {
        return events.slice().reverse().slice(0, 30).map((event) => \`<article class="event">
          <div class="event-top">
            <strong>\${escapeHtmlClient(formatters.eventType(event.type))}</strong>
            <span class="subtle">\${escapeHtmlClient(formatters.timestamp(event.occurredAt))}</span>
          </div>
          <p style="margin-top: 6px;">\${escapeHtmlClient(event.title || event.summary)}</p>
          <div class="event-meta">
            <span>\${escapeHtmlClient(event.summary)}</span>
          </div>
        </article>\`).join('');
      }

      function renderStatusSummary(listings) {
        const counts = listings.reduce((acc, listing) => {
          acc[listing.status] = (acc[listing.status] || 0) + 1;
          return acc;
        }, {});
        const text = Object.entries(counts).map(([status, count]) => \`\${status} \${count}\`).join(' · ') || 'Ingen';
        return 'Statusfordeling: ' + text;
      }

      async function refreshDashboardData() {
        const stamp = Date.now();
        const [snapshotResponse, historyResponse, stateResponse] = await Promise.all([
          fetch(\`data/snapshot.json?ts=\${stamp}\`, { cache: 'no-store' }),
          fetch(\`data/history.json?ts=\${stamp}\`, { cache: 'no-store' }),
          fetch(\`data/state.json?ts=\${stamp}\`, { cache: 'no-store' })
        ]);

        if (!snapshotResponse.ok || !historyResponse.ok || !stateResponse.ok) {
          return;
        }

        const [snapshot, history, state] = await Promise.all([
          snapshotResponse.json(),
          historyResponse.json(),
          stateResponse.json()
        ]);

        document.getElementById('current-listings-count').textContent = String(snapshot.length);
        document.getElementById('events-logged-count').textContent = String(history.length);
        document.getElementById('last-success-value').textContent = formatters.timestamp(state.lastSuccessAt);
        document.getElementById('failures-in-row-value').textContent = String(state.consecutiveFailures ?? 0);
        document.getElementById('status-summary').textContent = renderStatusSummary(snapshot);
        document.getElementById('listings-container').innerHTML = renderListings(snapshot);
        document.getElementById('events-container').innerHTML = renderEvents(history);
      }

      setInterval(() => {
        refreshDashboardData().catch(() => {});
      }, 60000);
    </script>
  </body>
</html>`;
}

export function createWebServer() {
  return http.createServer(async (request, response) => {
    if (!request.url || request.url === '/favicon.ico') {
      response.writeHead(404).end();
      return;
    }

    if (request.url === '/api/snapshot') {
      const listings = listingArraySchema.parse(await readJson(config.snapshotPath, []));
      response.writeHead(200, { 'content-type': 'application/json; charset=utf-8' });
      response.end(JSON.stringify(listings, null, 2));
      return;
    }

    if (request.url === '/api/history') {
      const events = eventArraySchema.parse(await readJson(config.historyPath, []));
      response.writeHead(200, { 'content-type': 'application/json; charset=utf-8' });
      response.end(JSON.stringify(events, null, 2));
      return;
    }

    const listings = listingArraySchema.parse(await readJson(config.snapshotPath, []));
    const events = eventArraySchema.parse(await readJson(config.historyPath, []));
    const state = await readJson(config.statePath, {});

    response.writeHead(200, { 'content-type': 'text/html; charset=utf-8' });
    response.end(renderPage(listings, events, { ...state, searchUrl: config.searchUrl || state.searchUrl }));
  });
}

const entryPath = process.argv[1];
const currentPath = fileURLToPath(import.meta.url);

if (entryPath && currentPath === entryPath) {
  const server = createWebServer();
  server.listen(config.webPort, () => {
    console.log(`CEJ tracker UI running at http://localhost:${config.webPort}`);
  });
}
