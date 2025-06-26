import React, { createContext, useContext, useState } from 'react';

// Create the context
const UIContext = createContext();

export function UIContextProvider({ children }) {
  // State for overlay stack management
  const [overlayStack, setOverlayStack] = useState([]);

  // Helper functions for overlay stack
  const pushOverlay = (component) => {
    setOverlayStack(prev => [...prev, component]);
  };

  const popOverlay = () => {
    setOverlayStack(prev => prev.slice(0, -1));
  };

  const closeOverlay = () => {
    setOverlayStack([]);
  };

  // Computed values for backward compatibility
  const isOverlayActive = overlayStack.length > 0;
  const highlightedComponent = overlayStack.length > 0 ? overlayStack[overlayStack.length - 1] : null;

  return (
    <UIContext.Provider value={{ 
      isOverlayActive, 
      setIsOverlayActive: (active) => {
        if (!active) {
          closeOverlay();
        }
      },
      highlightedComponent, 
      setHighlightedComponent: pushOverlay,
      pushOverlay,
      popOverlay,
      closeOverlay,
      overlayStack
    }}>
      {children}
    </UIContext.Provider>
  );
}

// Custom hook for easy access
export function useUIContext() {
  return useContext(UIContext);
} 