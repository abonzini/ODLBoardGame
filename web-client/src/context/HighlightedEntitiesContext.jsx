import React, { createContext, useContext, useState } from 'react';

const HighlightedEntitiesContext = createContext({
  highlightedEntityIds: [],
  setHighlightedEntities: () => {},
  clearHighlightedEntities: () => {},
  addHighlightedEntity: () => {},
  removeHighlightedEntity: () => {}
});

export const useHighlightedEntities = () => {
  const context = useContext(HighlightedEntitiesContext);
  return context;
};

export const HighlightedEntitiesProvider = ({ children }) => {
  const [highlightedEntityIds, setHighlightedEntityIds] = useState([]);

  const setHighlightedEntities = (entityIds) => {
    setHighlightedEntityIds(entityIds);
  };

  const clearHighlightedEntities = () => {
    setHighlightedEntityIds([]);
  };

  const addHighlightedEntity = (entityId) => {
    if (!highlightedEntityIds.includes(entityId)) {
      setHighlightedEntityIds([...highlightedEntityIds, entityId]);
    }
  };

  const removeHighlightedEntity = (entityId) => {
    setHighlightedEntityIds(highlightedEntityIds.filter(id => id !== entityId));
  };

  const value = {
    highlightedEntityIds,
    setHighlightedEntities,
    clearHighlightedEntities,
    addHighlightedEntity,
    removeHighlightedEntity
  };

  return (
    <HighlightedEntitiesContext.Provider value={value}>
      {children}
    </HighlightedEntitiesContext.Provider>
  );
}; 