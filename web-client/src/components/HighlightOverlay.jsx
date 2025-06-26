import React, { useEffect, useCallback } from 'react';
import { useUIContext } from '../context/UIContext';
import { getCardImagePath } from '../utils/imagePaths';
import CardListContainer from './CardListContainer';
import Button3D from './Button3D';
import './HighlightOverlay.css';

function HighlightOverlay() {
  const { isOverlayActive, highlightedComponent, popOverlay, closeOverlay, overlayStack } = useUIContext();
  
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