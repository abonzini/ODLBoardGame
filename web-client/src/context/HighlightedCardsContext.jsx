import React, { createContext, useContext, useState } from 'react';

const HighlightedCardsContext = createContext({
  highlightedCardIds: [],
  setHighlightedCards: () => {},
  clearHighlightedCards: () => {},
  addHighlightedCard: () => {},
  removeHighlightedCard: () => {}
});

export const useHighlightedCards = () => {
  const context = useContext(HighlightedCardsContext);
  return context;
};

export const HighlightedCardsProvider = ({ children }) => {
  const [highlightedCardIds, setHighlightedCardIds] = useState([]);

  const setHighlightedCards = (cardIds) => {
    setHighlightedCardIds(cardIds);
  };

  const clearHighlightedCards = () => {
    setHighlightedCardIds([]);
  };

  const addHighlightedCard = (cardId) => {
    if (!highlightedCardIds.includes(cardId)) {
      setHighlightedCardIds([...highlightedCardIds, cardId]);
    }
  };

  const removeHighlightedCard = (cardId) => {
    setHighlightedCardIds(highlightedCardIds.filter(id => id !== cardId));
  };

  const value = {
    highlightedCardIds,
    setHighlightedCards,
    clearHighlightedCards,
    addHighlightedCard,
    removeHighlightedCard
  };

  return (
    <HighlightedCardsContext.Provider value={value}>
      {children}
    </HighlightedCardsContext.Provider>
  );
}; 