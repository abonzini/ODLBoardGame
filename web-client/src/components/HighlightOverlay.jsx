import React, { useEffect, useCallback, useState } from 'react';
import { useUIContext } from '../context/UIContext';
import { getCardImagePath, getBlueprintImagePath } from '../utils/imagePaths';
import { getCachedCardTooltip } from '../utils/cardDataCache.js';
import CardListContainer from './CardListContainer';
import Button3D from './Button3D';
import KeywordList from './KeywordList';
import './HighlightOverlay.css';

function HighlightOverlay() {
  const { isOverlayActive, highlightedComponent, popOverlay, closeOverlay, overlayStack } = useUIContext();
  
  const [cardTooltip, setCardTooltip] = useState(null);

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
  
  // Fetch card tooltip when in card mode and cardId changes
  useEffect(() => {
    if (highlightedComponent?.type === 'card' && highlightedComponent?.cardId) {
      getCachedCardTooltip(highlightedComponent.cardId).then(setCardTooltip);
    } else {
      setCardTooltip(null);
    }
  }, [highlightedComponent]);
  
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
      // Prepare related cards for CardListContainer
      const relatedCards = cardTooltip?.relatedCards?.map(cardId => ({ cardId, count: 1 })) || [];
      const assortedCardCollection = {
        getCards: () => relatedCards
      };
      return (
        <div className="highlighted-component">
          {cardTooltip?.hasBlueprint && (
            <img
              src={getBlueprintImagePath(highlightedComponent.cardId)}
              alt={`Blueprint ${highlightedComponent.cardId}`}
              style={{
                position: 'absolute',
                top: '50%',
                left: '25%',
                transform: 'translate(-50%, -50%)',
                height: '40%',
                width: 'auto',
                pointerEvents: 'none'
              }}
            />
          )}
          {cardTooltip?.keywords && cardTooltip.keywords.length > 0 && (
            <div style={{
              position: 'absolute',
              top: 0,
              left: '75%',
              transform: 'translate(-50%, 0)',
              height: '75%',
              width: '25%',
              display: 'flex',
              justifyContent: 'center',
              alignItems: 'flex-start'
            }}>
              <KeywordList keywordNames={cardTooltip.keywords} />
            </div>
          )}
          <div style={{ 
            position: 'absolute',
            top: '50%', 
            left: '50%', 
            transform: 'translate(-50%, -50%)',
            height: '50%',
            width: 'auto'
          }}>
            <img 
              src={getCardImagePath(highlightedComponent.cardId)} 
              alt={`Card ${highlightedComponent.cardId}`} 
              style={{ height: '100%', width: 'auto' }}
            />
          </div>
          <div style={{
            position: 'absolute',
            left: 0,
            bottom: 0,
            width: '100%',
            height: '25%',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            background: 'none'
          }}>
            <CardListContainer
              activePowerId={null}
              activePowerAvailable={false}
              assortedCardCollection={assortedCardCollection}
              centered={true}
            />
          </div>
        </div>
      );
    }

    if (highlightedComponent.type === 'cardList') {
      return (
        <div className="highlighted-component">
          <div style={{
            position: 'absolute',
            top: '50%',
            left: '50%',
            transform: 'translate(-50%, -50%)',
            width: '100%',
            height: '50%'
          }}>
            <CardListContainer 
              activePowerId={null}
              activePowerAvailable={false}
              assortedCardCollection={highlightedComponent.assortedCardCollection}
              centered={true}
            />
          </div>
        </div>
      );
    }

    return null;
  };

  return (
    <div className="highlight-overlay" onContextMenu={handleRightClick}>
      <div className="overlay-background"></div>
      {renderHighlightedComponent()}
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
    </div>
  );
}

export default HighlightOverlay; 