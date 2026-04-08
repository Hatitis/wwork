import fs from 'node:fs/promises';
import path from 'node:path';

import { config } from './config.js';
import { eventArraySchema } from './models/eventSchema.js';
import { listingArraySchema } from './models/listingSchema.js';
import { ensureDirectory, readJson } from './services/storage.js';
import { renderPage } from './webServer.js';

const siteDir = path.join(config.projectRoot, 'site');
const siteDataDir = path.join(siteDir, 'data');
const repositorySlug = process.env.GITHUB_REPOSITORY ?? '';
const [repositoryOwner = '', repositoryName = ''] = repositorySlug.split('/');
const inferredPublicSiteUrl =
  config.publicSiteUrl ||
  (repositoryOwner && repositoryName ? `https://${repositoryOwner}.github.io/${repositoryName}/` : '');

const listings = listingArraySchema.parse(await readJson(config.snapshotPath, []));
const events = eventArraySchema.parse(await readJson(config.historyPath, []));
const state = await readJson(config.statePath, {});
const siteState = {
  ...state,
  searchUrl: config.searchUrl || state.searchUrl,
  publicSiteUrl: inferredPublicSiteUrl || state.publicSiteUrl
};

await ensureDirectory(siteDataDir);

await Promise.all([
  fs.writeFile(
    path.join(siteDir, 'index.html'),
    renderPage(listings, events, siteState),
    'utf8'
  ),
  fs.writeFile(path.join(siteDir, '.nojekyll'), '', 'utf8'),
  fs.writeFile(path.join(siteDataDir, 'snapshot.json'), `${JSON.stringify(listings, null, 2)}\n`, 'utf8'),
  fs.writeFile(path.join(siteDataDir, 'history.json'), `${JSON.stringify(events, null, 2)}\n`, 'utf8'),
  fs.writeFile(path.join(siteDataDir, 'state.json'), `${JSON.stringify(siteState, null, 2)}\n`, 'utf8')
]);

console.log(`Static site generated in ${siteDir}`);
