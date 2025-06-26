import React from 'react';
import { getCardImagePath } from '../utils/imagePaths';
import { useUIContext } from '../context/UIContext';
import CounterCircle from './CounterCircle';
import './Card.css';

function Card({ cardId, count }) {
  const { isOverlayActive, pushOverlay } = useUIContext();

  const handleRightClick = (e) => {
    e.preventDefault();
    e.stopPropagation();
    pushOverlay({ type: 'card', cardId });
  };

  return (
    <div className="card" onContextMenu={handleRightClick}>
      <img src={getCardImagePath(cardId)} alt={`Card ${cardId}`} />
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