import React, { useEffect, useCallback, useState } from 'react';
import { useUIContext } from '../context/UIContext';
import { getCardImagePath, getBlueprintImagePath } from '../utils/imagePaths';
import CardListContainer from './CardListContainer';
import Button3D from './Button3D';
import './HighlightOverlay.css';

function HighlightOverlay() {
  const { isOverlayActive, highlightedComponent, popOverlay, closeOverlay, overlayStack } = useUIContext();
  const [blueprintExists, setBlueprintExists] = useState(false);
  
  // Check if blueprint image exists
  const checkBlueprintExists = useCallback((cardId) => {
    const img = new Image();
    img.onload = () => setBlueprintExists(true);
    img.onerror = () => setBlueprintExists(false);
    img.src = getBlueprintImagePath(cardId);
  }, []);

  // Reset blueprint state when highlighted component changes
  useEffect(() => {
    if (highlightedComponent?.type === 'card' && highlightedComponent?.cardId) {
      setBlueprintExists(false);
      checkBlueprintExists(highlightedComponent.cardId);
    }
  }, [highlightedComponent, checkBlueprintExists]);

  const handleClose = useCallback(() => {
    closeOverlay();
  }, [closeOverlay]);

  const handleBack = useCallback(() => {
    popOverlay();
  }, [popOverlay]);

  // Handle ESC key to go back or close overlay
  useEffect(() => {
    const handleKeyDown = (e) => {
      if (e.key === 'Escape') {
        handleBack();
      }
    };

    if (isOverlayActive) {
      document.addEventListener('keydown', handleKeyDown);
      return () => {
        document.removeEventListener('keydown', handleKeyDown);
      };
    }
  }, [isOverlayActive, handleBack]);
  
  if (!isOverlayActive) {
    return null;
  }

  const handleRightClick = (e) => {
    e.preventDefault();
    handleBack();
  };

  const handleCloseClick = () => {
    handleBack();
  };

  // Determine button text based on stack depth
  const buttonText = overlayStack.length === 1 ? 'X' : '↩';

  const renderHighlightedComponent = () => {
    if (!highlightedComponent) return null;

    if (highlightedComponent.type === 'none') {
      return null;
    }

    if (highlightedComponent.type === 'card') {
      return (
        <div className="highlighted-component" style={{ 
          top: '50%', 
          left: '50%', 
          transform: 'translate(-50%, -50%)',
          height: '50vh',
          width: 'auto'
        }}>
          {blueprintExists && (
            <img 
              src={getBlueprintImagePath(highlightedComponent.cardId)} 
              alt={`Blueprint ${highlightedComponent.cardId}`} 
              style={{ 
                position: 'absolute',
                right: '100%',
                top: '50%',
                transform: 'translateY(-50%)',
                height: 'auto', 
                width: '25vw',
                maxHeight: '50vh',
                marginRight: '20px'
              }}
            />
          )}
          <img 
            src={getCardImagePath(highlightedComponent.cardId)} 
            alt={`Card ${highlightedComponent.cardId}`} 
            style={{ height: '100%', width: 'auto' }}
          />
        </div>
      );
    }

    if (highlightedComponent.type === 'cardList') {
      return (
        <div className="highlighted-component" style={{
          position: 'absolute',
          top: '50%',
          left: '50%',
          transform: 'translate(-50%, -50%)',
          width: '100vw',
          height: '50vh'
        }}>
          <CardListContainer 
            activePowerId={null}
            activePowerAvailable={false}
            assortedCardCollection={highlightedComponent.assortedCardCollection}
          />
        </div>
      );
    }

    return null;
  };

  return (
    <div className="highlight-overlay" onContextMenu={handleRightClick}>
      <div className="overlay-background"></div>
      <div className="close-button">
        <Button3D 
          text={buttonText}
          onClick={handleCloseClick}
          color="#FF0000"
          width="50px"
          height="50px"
          fontSize="24px"
        />
      </div>
      {renderHighlightedComponent()}
    </div>
  );
}

export default HighlightOverlay; 