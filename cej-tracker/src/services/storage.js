import fs from 'node:fs/promises';
import path from 'node:path';

/**
 * Ensure a directory exists.
 * @param {string} directoryPath
 * @returns {Promise<void>}
 */
export async function ensureDirectory(directoryPath) {
  await fs.mkdir(directoryPath, { recursive: true });
}

/**
 * Read JSON from disk or return a fallback.
 * @template T
 * @param {string} filePath
 * @param {T} fallbackValue
 * @returns {Promise<T>}
 */
export async function readJson(filePath, fallbackValue) {
  try {
    const contents = await fs.readFile(filePath, 'utf8');
    return /** @type {T} */ (JSON.parse(contents));
  } catch (error) {
    if (/** @type {NodeJS.ErrnoException} */ (error).code === 'ENOENT') {
      return fallbackValue;
    }
    throw error;
  }
}

/**
 * Write JSON atomically by using a temporary file and rename.
 * @param {string} filePath
 * @param {unknown} value
 * @returns {Promise<void>}
 */
export async function writeJson(filePath, value) {
  await ensureDirectory(path.dirname(filePath));
  const temporaryPath = `${filePath}.tmp`;
  const payload = `${JSON.stringify(value, null, 2)}\n`;
  await fs.writeFile(temporaryPath, payload, 'utf8');
  await fs.rename(temporaryPath, filePath);
}
