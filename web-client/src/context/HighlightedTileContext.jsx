import React, { createContext, useContext, useState } from 'react';

const HighlightedTileContext = createContext({
  highlightedTileIds: [],
  setHighlightedTiles: () => {},
  clearHighlightedTiles: () => {},
  addHighlightedTile: () => {},
  removeHighlightedTile: () => {}
});

export const useHighlightedTiles = () => {
  const context = useContext(HighlightedTileContext);
  return context;
};

export const HighlightedTileProvider = ({ children }) => {
  const [highlightedTileIds, setHighlightedTileIds] = useState([]);

  const setHighlightedTiles = (tileIds) => {
    setHighlightedTileIds(tileIds);
  };

  const clearHighlightedTiles = () => {
    setHighlightedTileIds([]);
  };

  const addHighlightedTile = (tileId) => {
    if (!highlightedTileIds.includes(tileId)) {
      setHighlightedTileIds([...highlightedTileIds, tileId]);
    }
  };

  const removeHighlightedTile = (tileId) => {
    setHighlightedTileIds(highlightedTileIds.filter(id => id !== tileId));
  };

  const value = {
    highlightedTileIds,
    setHighlightedTiles,
    clearHighlightedTiles,
    addHighlightedTile,
    removeHighlightedTile
  };

  return (
    <HighlightedTileContext.Provider value={value}>
      {children}
    </HighlightedTileContext.Provider>
  );
}; 