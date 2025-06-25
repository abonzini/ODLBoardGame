import React from 'react';
import { useUIContext } from '../context/UIContext';
import { getCardImagePath } from '../utils/imagePaths';
import Button3D from './Button3D';
import './HighlightOverlay.css';

function HighlightOverlay() {
  const { isOverlayActive, setIsOverlayActive, highlightedComponent, overlayContent } = useUIContext();
  
  if (!isOverlayActive) {
    return null;
  }

  const handleRightClick = (e) => {
    e.preventDefault();
    setIsOverlayActive(false);
  };

  const handleCloseClick = () => {
    setIsOverlayActive(false);
  };

  const renderHighlightedComponent = () => {
    if (!highlightedComponent) return null;

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

    return null;
  };

  return (
    <div className="highlight-overlay" onContextMenu={handleRightClick}>
      <div className="overlay-background"></div>
      <Button3D 
        text="X"
        onClick={handleCloseClick}
        color="#FF0000"
        width="50px"
        height="50px"
        fontSize="24px"
        position="absolute"
        top="20px"
        left="20px"
      />
      {renderHighlightedComponent()}
      {overlayContent && (
        <div className="overlay-content">
          {/* Render any additional overlay content here */}
        </div>
      )}
    </div>
  );
}

export default HighlightOverlay; 