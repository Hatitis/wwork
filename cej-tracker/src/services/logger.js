const levels = ['debug', 'info', 'warn', 'error'];

/**
 * Create a tiny console logger.
 * @param {string} level
 * @returns {{debug: Function, info: Function, warn: Function, error: Function}}
 */
export function createLogger(level = 'info') {
  const minimumLevelIndex = levels.indexOf(level);

  /**
   * @param {'debug'|'info'|'warn'|'error'} currentLevel
   * @param {...unknown} args
   * @returns {void}
   */
  function log(currentLevel, ...args) {
    const currentLevelIndex = levels.indexOf(currentLevel);
    if (currentLevelIndex < minimumLevelIndex) {
      return;
    }

    const stamp = new Date().toISOString();
    const method = currentLevel === 'debug' ? 'log' : currentLevel;
    console[method](`[${stamp}] [${currentLevel.toUpperCase()}]`, ...args);
  }

  return {
    debug: (...args) => log('debug', ...args),
    info: (...args) => log('info', ...args),
    warn: (...args) => log('warn', ...args),
    error: (...args) => log('error', ...args)
  };
}
