# cej-tracker

`cej-tracker` is a Node.js 20+ ESM app that fetches a CEJ rental search page, parses listings, compares them with the previous snapshot, and emits batched Discord notifications. It can also expose a tiny local web UI for browsing the current snapshot and recent events.

## Features

- Uses built-in `fetch`, `cheerio`, and `zod`
- Stores the latest good snapshot in `data/snapshot.json`
- Appends change events to `data/history.json`
- Tracks runtime health in `data/state.json`
- Preserves `rawText` for each listing
- Avoids destructive snapshot updates when fetches fail or parsing returns zero listings
- Supports optional Discord webhook notifications
- Includes a small local dashboard at `http://localhost:8787` by default

## Install

```bash
npm install
```

## Environment variables

- `CEJ_SEARCH_URL`:
  The CEJ search page URL to track. Required.
- `CEJ_SOURCE`:
  Source label stored on listings and events. Default: `cej`
- `CEJ_REQUEST_TIMEOUT_MS`:
  Fetch timeout in milliseconds. Default: `15000`
- `CEJ_USER_AGENT`:
  Custom user agent for requests
- `LOG_LEVEL`:
  `debug`, `info`, `warn`, or `error`. Default: `info`
- `CEJ_DISCORD_WEBHOOK_URL`:
  Discord webhook URL for notifications
- `CEJ_DISCORD_USERNAME`:
  Optional bot-style display name. Default: `CEJ Tracker`
- `CEJ_DISCORD_AVATAR_URL`:
  Optional avatar URL for the webhook sender
- `CEJ_DISCORD_MENTION_USER_ID`:
  Optional Discord user ID to mention for actual ping-style notifications
- `CEJ_DISCORD_MENTION_ROLE_ID`:
  Optional Discord role ID to mention instead of a user
- `CEJ_WEB_PORT`:
  Local dashboard port. Default: `8787`
- `CEJ_PUBLIC_SITE_URL`:
  Optional public URL for the hosted dashboard. Used by the static Pages build.

## Run

```bash
CEJ_SEARCH_URL="https://cej.dk/ledige-lejeboliger" npm start
```

Run the dashboard:

```bash
npm run web
```

Then open [http://localhost:8787](http://localhost:8787) unless you changed `CEJ_WEB_PORT`.

## One-command start and stop

The easiest way to run everything is with a local config file plus the helper scripts.

1. Create your local config:

```bash
cp .env.local.example .env.local
```

2. Edit `.env.local` and replace `REPLACE_WITH_DISCORD_WEBHOOK_URL` with your Discord webhook URL

3. Start everything:

```bash
npm run up
```

This will:

- start the local dashboard
- run the tracker once immediately
- send a startup confirmation Discord notification
- install a cron job using `CRON_SCHEDULE`

4. Stop everything:

```bash
npm run down
```

This will:

- stop the local dashboard
- remove the cron job
- send a shutdown confirmation Discord notification

## GitHub hosting

If you want the tracker to run without your computer being on, the best GitHub-native setup is:

- GitHub Actions runs the tracker on a schedule
- the workflow commits updated `data/*.json` back to the repository so state persists between runs
- the same workflow builds a static dashboard and deploys it to GitHub Pages

Files:

- workflow: [.github/workflows/cej-tracker.yml](/Users/ludvig/Downloads/WWork/.github/workflows/cej-tracker.yml)
- static site builder: [src/buildStaticSite.js](/Users/ludvig/Downloads/WWork/cej-tracker/src/buildStaticSite.js)

Repository setup:

1. Push this repo to GitHub
2. In GitHub, open `Settings` -> `Secrets and variables` -> `Actions`
3. Add repository secrets:
   - `CEJ_SEARCH_URL`
   - `CEJ_DISCORD_WEBHOOK_URL`
   - `CEJ_DISCORD_MENTION_USER_ID` if you want user pings
   - `CEJ_DISCORD_MENTION_ROLE_ID` if you want role pings instead
4. Optionally add repository variables:
   - `CEJ_DISCORD_USERNAME`
   - `CEJ_DISCORD_AVATAR_URL`
   - `CEJ_PUBLIC_SITE_URL`
5. In `Settings` -> `Pages`, set the source to `GitHub Actions`
6. Run the `cej-tracker` workflow once manually from the Actions tab

After that:

- the workflow will run roughly every 5 minutes
- Discord notifications will come from GitHub-hosted runs
- the dashboard will be published on GitHub Pages
- the site URL will usually be `https://<your-github-username>.github.io/<your-repository-name>/`

Notes:

- GitHub Actions schedules run in UTC
- the workflow stores tracker state by committing the JSON data files back into the repo
- if you set `CEJ_PUBLIC_SITE_URL`, that explicit URL will be shown on the dashboard; otherwise the build infers the standard GitHub Pages pattern from the repository slug

## Discord notifications

If a Discord webhook is configured, any run that produces changes will also send one batched Discord message.

You can also send a manual test notification:

```bash
npm run test-discord
```

Example:

```bash
CEJ_SEARCH_URL="https://udlejning.cej.dk/find-bolig/overblik?collection=residences&monthlyPrice=0-10600&p=sj%C3%A6lland&types=apartment" \
CEJ_DISCORD_WEBHOOK_URL="https://discord.com/api/webhooks/..." \
CEJ_DISCORD_USERNAME="CEJ Tracker" \
npm run track
```

## Cron

The tracker command is cron-friendly because it runs once and exits.

Example crontab entry that runs every 5 minutes:

```cron
*/5 * * * * cd /Users/ludvig/Downloads/WWork/cej-tracker && CEJ_SEARCH_URL='https://udlejning.cej.dk/find-bolig/overblik?collection=residences&monthlyPrice=0-10600&p=sj%C3%A6lland&types=apartment' CEJ_DISCORD_WEBHOOK_URL='https://discord.com/api/webhooks/...' /usr/bin/env npm run track >> /tmp/cej-tracker.log 2>&1
```

Install or edit your crontab with:

```bash
crontab -e
```

The helper scripts automate this for you, so manual cron editing is optional.

## Test

```bash
npm test
```

## Data files

- `data/snapshot.json`:
  Current known good listing snapshot
- `data/history.json`:
  Event log
- `data/state.json`:
  Last run metadata, failure counts, and timestamps

## Notes

- The parser uses defensive selectors because markup can vary between CEJ pages.
- Listing identity prefers the normalized listing URL.
- If a run produces zero listings, the previous snapshot is kept and no removals are emitted.
- The dashboard reads directly from `snapshot.json`, `history.json`, and `state.json`.
