import React, { useRef, useEffect, useState } from 'react';
import './CounterCircle.css';

function CounterCircle({ count, width, height, top, left, showIfOne = true }) {
  const circleRef = useRef(null);
  const [fontSize, setFontSize] = useState('100%');

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

  // Don't render if count is 1 or less and showIfOne is false
  if (count <= 1 && !showIfOne) {
    return null;
  }

  return (
    <div 
      className="counter-circle" 
      ref={circleRef}
      style={{
        position: 'absolute',
        top: top,
        left: left,
        width: width,
        height: height,
        transform: 'translate(-50%, -50%)'
      }}
    >
      <div className="counter-label" style={{ fontSize }}>{count}</div>
    </div>
  );
}

export default CounterCircle; 