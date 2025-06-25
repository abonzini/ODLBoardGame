import React, { useRef, useEffect, useState } from 'react';
import { getCardImagePath } from '../utils/imagePaths';
import { useUIContext } from '../context/UIContext';
import './Card.css';

function Card({ cardId, count }) {
  const circleRef = useRef(null);
  const [fontSize, setFontSize] = useState('100%');
  const { setIsOverlayActive, setHighlightedComponent } = useUIContext();

  const calculateFontSize = () => {
    if (circleRef.current) {
      const circleHeight = circleRef.current.offsetHeight;
      const calculatedFontSize = circleHeight * 0.7;
      setFontSize(`${calculatedFontSize}px`);
    }
  };

  useEffect(() => {
    if (circleRef.current) {
      const resizeObserver = new ResizeObserver(calculateFontSize);
      resizeObserver.observe(circleRef.current);
      
      // Initial calculation
      calculateFontSize();
      
      return () => resizeObserver.disconnect();
    }
  }, [count]); // Re-run when count changes (for dynamic components)

  const handleRightClick = (e) => {
    e.preventDefault();
    setIsOverlayActive(true);
    setHighlightedComponent({ type: 'card', cardId });
  };

  return (
    <div className="card" onContextMenu={handleRightClick}>
      <img src={getCardImagePath(cardId)} alt={`Card ${cardId}`} />
      {count > 1 && (
        <div className="counter-circle" ref={circleRef}>
          <div className="counter-label" style={{ fontSize }}>{count}</div>
        </div>
      )}
    </div>
  );
}

export default Card; 