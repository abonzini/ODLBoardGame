import React from 'react';
import './CounterCircle.css';

function CounterCircle({ count, width, height, top, left, showIfOne = true }) {
  // Don't render if count is 1 or less and showIfOne is false
  if (count <= 1 && !showIfOne) {
    return null;
  }

  return (
    <div 
      className="counter-circle" 
      style={{
        position: 'absolute',
        top: top,
        left: left,
        width: width,
        height: height,
        transform: 'translate(-50%, -50%)'
      }}
    >
      <div className="counter-label">{count}</div>
    </div>
  );
}

export default CounterCircle; 