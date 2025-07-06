import React from 'react';
import { getCardImagePath } from '../utils/imagePaths';
import { useUIContext } from '../context/UIContext';
import { useHighlightedCards } from '../context/HighlightedCardsContext';
import './ActivePower.css';

function ActivePower({ cardId, available }) {
  const { setIsOverlayActive, setHighlightedComponent } = useUIContext();
  const { highlightedCardIds } = useHighlightedCards();

  const handleRightClick = (e) => {
    e.preventDefault();
    setIsOverlayActive(true);
    setHighlightedComponent({ type: 'card', cardId });
  };

  const isHighlighted = highlightedCardIds.includes(cardId);

  return (
    <div className={`active-power ${available ? 'available' : 'unavailable'}`} onContextMenu={handleRightClick}>
      <img 
        src={getCardImagePath(cardId)} 
        alt={`Active Power ${cardId}`}
        style={{
          boxShadow: isHighlighted ? '0 0 10px 5px var(--highlighted-color), inset 0 0 10px 2px var(--highlighted-color)' : 'none'
        }}
      />
    </div>
  );
}

export default ActivePower; 