import React from 'react';
import { getCardImagePath } from '../utils/imagePaths';
import { useUIContext } from '../context/UIContext';
import CounterCircle from './CounterCircle';
import './Card.css';

function Card({ cardId, count }) {
  const { setIsOverlayActive, setHighlightedComponent } = useUIContext();

  const handleRightClick = (e) => {
    e.preventDefault();
    setIsOverlayActive(true);
    setHighlightedComponent({ type: 'card', cardId });
  };

  return (
    <div className="card" onContextMenu={handleRightClick}>
      <img src={getCardImagePath(cardId)} alt={`Card ${cardId}`} />
      {count > 1 && (
        <CounterCircle 
          count={count}
          height="30%"
          top="75%"
          left="75%"
        />
      )}
    </div>
  );
}

export default Card; 