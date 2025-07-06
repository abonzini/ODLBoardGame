import React from 'react';
import { getCardImagePath } from '../utils/imagePaths';
import { useUIContext } from '../context/UIContext';
import { useHighlightedCards } from '../context/HighlightedCardsContext';
import CounterCircle from './CounterCircle';
import './Card.css';

function Card({ cardId, count }) {
  const { isOverlayActive, pushOverlay } = useUIContext();
  const { highlightedCardIds } = useHighlightedCards();

  const handleRightClick = (e) => {
    e.preventDefault();
    e.stopPropagation();
    pushOverlay({ type: 'card', cardId });
  };

  const isHighlighted = highlightedCardIds.includes(cardId);

  return (
    <div className="card" onContextMenu={handleRightClick}>
      <img 
        src={getCardImagePath(cardId)} 
        alt={`Card ${cardId}`}
        style={{
          boxShadow: isHighlighted ? '0 0 10px 5px var(--highlighted-color), inset 0 0 10px 2px var(--highlighted-color)' : 'none'
        }}
      />
      <CounterCircle 
        count={count}
        height="30%"
        top="75%"
        left="75%"
        showIfOne={false}
      />
    </div>
  );
}

export default Card; 