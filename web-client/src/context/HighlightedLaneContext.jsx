import React, { createContext, useContext, useState } from 'react';

const HighlightedLaneContext = createContext({
  highlightedLaneIds: [],
  setHighlightedLanes: () => {},
  clearHighlightedLanes: () => {},
  addHighlightedLane: () => {},
  removeHighlightedLane: () => {}
});

export const useHighlightedLanes = () => {
  const context = useContext(HighlightedLaneContext);
  return context;
};

export const HighlightedLaneProvider = ({ children }) => {
  const [highlightedLaneIds, setHighlightedLaneIds] = useState([]);

  const setHighlightedLanes = (laneIds) => {
    setHighlightedLaneIds(laneIds);
  };

  const clearHighlightedLanes = () => {
    setHighlightedLaneIds([]);
  };

  const addHighlightedLane = (laneId) => {
    if (!highlightedLaneIds.includes(laneId)) {
      setHighlightedLaneIds([...highlightedLaneIds, laneId]);
    }
  };

  const removeHighlightedLane = (laneId) => {
    setHighlightedLaneIds(highlightedLaneIds.filter(id => id !== laneId));
  };

  const value = {
    highlightedLaneIds,
    setHighlightedLanes,
    clearHighlightedLanes,
    addHighlightedLane,
    removeHighlightedLane
  };

  return (
    <HighlightedLaneContext.Provider value={value}>
      {children}
    </HighlightedLaneContext.Provider>
  );
}; 