import { getCardTooltipPath, getCardKeywordPath } from './cardDataPaths.js';
import { CardTooltip } from '../models/CardTooltip.js';
import { Keyword } from '../models/Keyword.js';

// Cache for deserialized models
const tooltipCache = new Map();
const keywordCache = new Map();

/**
 * Load and cache a card tooltip by card ID
 * @param {number} cardId - The card ID to load tooltip for
 * @returns {Promise<CardTooltip>} - Promise that resolves to a cached CardTooltip instance
 */
export const getCachedCardTooltip = async (cardId) => {
  // Check cache first
  if (tooltipCache.has(cardId)) {
    return tooltipCache.get(cardId);
  }

  try {
    const response = await fetch(getCardTooltipPath(cardId));
    if (!response.ok) {
      throw new Error(`Failed to load tooltip for card ${cardId}: ${response.statusText}`);
    }
    
    const jsonData = await response.json();
    const tooltip = CardTooltip.fromJson(jsonData);
    
    // Cache the deserialized model
    tooltipCache.set(cardId, tooltip);
    
    return tooltip;
  } catch (error) {
    console.error(`Error loading tooltip for card ${cardId}:`, error);
    // Return a default tooltip and cache it
    const defaultTooltip = new CardTooltip(cardId, false, [], []);
    tooltipCache.set(cardId, defaultTooltip);
    return defaultTooltip;
  }
};

/**
 * Load and cache a keyword by name
 * @param {string} keywordName - The keyword name to load
 * @returns {Promise<Keyword>} - Promise that resolves to a cached Keyword instance
 */
export const getCachedCardKeyword = async (keywordName) => {
  // Check cache first
  if (keywordCache.has(keywordName)) {
    return keywordCache.get(keywordName);
  }

  try {
    const response = await fetch(getCardKeywordPath(keywordName));
    if (!response.ok) {
      throw new Error(`Failed to load keyword ${keywordName}: ${response.statusText}`);
    }
    
    const jsonData = await response.json();
    const keyword = Keyword.fromJson(jsonData);
    
    // Cache the deserialized model
    keywordCache.set(keywordName, keyword);
    
    return keyword;
  } catch (error) {
    console.error(`Error loading keyword ${keywordName}:`, error);
    // Return a default keyword and cache it
    const defaultKeyword = new Keyword(keywordName, 'Keyword not found', false, [], []);
    keywordCache.set(keywordName, defaultKeyword);
    return defaultKeyword;
  }
};

/**
 * Clear the tooltip cache
 */
export const clearTooltipCache = () => {
  tooltipCache.clear();
};

/**
 * Clear the keyword cache
 */
export const clearKeywordCache = () => {
  keywordCache.clear();
};

/**
 * Clear all caches
 */
export const clearAllCaches = () => {
  clearTooltipCache();
  clearKeywordCache();
}; 