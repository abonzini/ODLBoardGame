import React from 'react';
import { getCardImagePath } from '../utils/imagePaths';
import { useUIContext } from '../context/UIContext';
import CounterCircle from './CounterCircle';
import './PileOfCards.css';

function PileOfCards({ assortedCardCollection, show = false }) {
  const { pushOverlay } = useUIContext();
  
  // Calculate collection size
  const collectionSize = assortedCardCollection?._size || 0;
  
  // Get the first card ID from the map for the image
  const getFirstCardId = () => {
    if (!assortedCardCollection?._cardHistogram || assortedCardCollection._cardHistogram.size === 0) {
      return null;
    }
    return assortedCardCollection._cardHistogram.keys().next().value;
  };
  
  const firstCardId = getFirstCardId();
  
  const handleRightClick = (e) => {
    e.preventDefault();
    pushOverlay({
      type: 'cardList',
      assortedCardCollection: assortedCardCollection
    });
  };
  
  // Calculate cardtop position and 3D border based on size
  const getCardtopStyle = () => {
    if (collectionSize === 0) {
      return { display: 'none' };
    }
    
    if (collectionSize === 1) {
      return { 
        top: '50%', 
        left: '50%', 
        transform: 'translate(-50%, -50%)',
        borderBottom: '2px solid #444',
        borderRight: '2px solid #444'
      };
    }
    
    // Calculate offset based on size (max at 30+)
    const maxSize = 30;
    const normalizedSize = Math.min(collectionSize, maxSize);
    const progress = (normalizedSize - 1) / (maxSize - 1); // 0 to 1
    
    // Calculate offset (3.5:2.5 ratio - up and left)
    const maxOffset = 10; // percentage (reduced from 35%)
    const verticalOffset = progress * maxOffset * (3.5 / (3.5 + 2.5)); // up
    const horizontalOffset = progress * maxOffset * (2.5 / (3.5 + 2.5)); // left
    
    // Calculate 3D border thickness based on offset
    const baseBorderThickness = 2;
    const maxBorderThickness = 8;
    const verticalBorderThickness = baseBorderThickness + (progress * (maxBorderThickness - baseBorderThickness));
    const horizontalBorderThickness = baseBorderThickness + (progress * (maxBorderThickness - baseBorderThickness));
    
    return {
      top: `calc(50% - ${verticalOffset}%)`,
      left: `calc(50% - ${horizontalOffset}%)`,
      transform: 'translate(-50%, -50%)',
      borderBottom: `${verticalBorderThickness}px solid #444`,
      borderRight: `${horizontalBorderThickness}px solid #666`
    };
  };

  return (
    <div className="pile-of-cards" onContextMenu={handleRightClick}>
      <div className="pile-rectangle">
        <div className="pile-label">X</div>
        <div 
          className="cardtop" 
          style={getCardtopStyle()}
        >
          {show && firstCardId && (
            <img 
              src={getCardImagePath(firstCardId)} 
              alt={`Card ${firstCardId}`}
              className="card-image-overlay"
            />
          )}
          <CounterCircle 
            count={collectionSize}
            height="25%"
            top="75%"
            left="75%"
            showIfOne={true}
          />
        </div>
      </div>
    </div>
  );
}

export default PileOfCards; 