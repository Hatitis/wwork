import { assertConfig } from './config.js';
import { runTracker } from './runTracker.js';

try {
  assertConfig();
  const result = await runTracker();
  process.exitCode = result.ok ? 0 : 1;
} catch (error) {
  console.error(/** @type {Error} */ (error).message);
  process.exitCode = 1;
}
