// Utility functions for accessing card resources from local public/images directory

// Local paths for copied image resources
export const CARD_RESOURCE_PATHS = {
  // Layout elements (icons, borders, etc.)
  LAYOUT_ELEMENTS: '/images/card-layout-elements',
  
  // Raw card images (original card art)
  CARD_IMAGES_RAW: '/images/card-images-raw',
  
  // Generated card images (complete cards)
  GENERATED_CARD_IMAGES: '/images/card-images',
  
  // Generated blueprint images
  GENERATED_BLUEPRINT_IMAGES: '/images/blueprint-images'
};

// Helper functions to get specific image paths
export const getCardImagePath = (cardId) => {
  return `${CARD_RESOURCE_PATHS.GENERATED_CARD_IMAGES}/${cardId}.png`;
};

export const getBlueprintImagePath = (cardId) => {
  return `${CARD_RESOURCE_PATHS.GENERATED_BLUEPRINT_IMAGES}/${cardId}.png`;
};

export const getRawCardImagePath = (cardId) => {
  return `${CARD_RESOURCE_PATHS.CARD_IMAGES_RAW}/${cardId}.png`;
};

export const getLayoutElementPath = (elementName) => {
  return `${CARD_RESOURCE_PATHS.LAYOUT_ELEMENTS}/${elementName}.png`;
};

// Preload helper for card images
export const preloadCardImages = (cardIds) => {
  return cardIds.map(cardId => getCardImagePath(cardId));
};

// Preload helper for layout elements
export const preloadLayoutElements = (elementNames) => {
  return elementNames.map(name => getLayoutElementPath(name));
}; 