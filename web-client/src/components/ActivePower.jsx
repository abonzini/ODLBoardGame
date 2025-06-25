import React from 'react';
import { getCardImagePath } from '../utils/imagePaths';
import { useUIContext } from '../context/UIContext';
import './ActivePower.css';

function ActivePower({ cardId, available }) {
  const { setIsOverlayActive, setHighlightedComponent } = useUIContext();

  const handleRightClick = (e) => {
    e.preventDefault();
    setIsOverlayActive(true);
    setHighlightedComponent({ type: 'card', cardId });
  };

  return (
    <div className={`active-power ${available ? 'available' : 'unavailable'}`} onContextMenu={handleRightClick}>
      <img src={getCardImagePath(cardId)} alt={`Active Power ${cardId}`} />
    </div>
  );
}

export default ActivePower; 