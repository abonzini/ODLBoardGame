import React from 'react';
import { useHighlightedLanes } from '../context/HighlightedLaneContext';
import './Lane.css';

function Lane({ laneId }) {
  const { highlightedLaneIds } = useHighlightedLanes();
  
  const isHighlighted = highlightedLaneIds.includes(laneId);

  return (
    <div 
      className="lane"
      style={{
        boxShadow: isHighlighted ? '0 0 10px 2px var(--highlighted-color), inset 0 0 10px 2px var(--highlighted-color)' : 'none'
      }}
    >
    </div>
  );
}

export default Lane; 