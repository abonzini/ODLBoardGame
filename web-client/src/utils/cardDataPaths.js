// Utility functions for accessing card data resources from local public directory

// Local paths for card data resources
export const CARD_DATA_PATHS = {
  // Card tooltips (JSON data for card tooltips)
  CARD_TOOLTIPS: '/card-tooltips',
  
  // Card keywords (JSON data for keyword definitions)
  CARD_KEYWORDS: '/card-keywords'
};

// Helper functions for card tooltips
export const getCardTooltipPath = (cardId) => {
  return `${CARD_DATA_PATHS.CARD_TOOLTIPS}/${cardId}.json`;
};

// Helper functions for card keywords
export const getCardKeywordPath = (keywordName) => {
  return `${CARD_DATA_PATHS.CARD_KEYWORDS}/${keywordName}.json`;
}; 