import React, { createContext, useContext, useState } from 'react';

// Create the context
const UIContext = createContext();

export function UIContextProvider({ children }) {
  // State for overlay management
  const [isOverlayActive, setIsOverlayActive] = useState(false);
  const [highlightedComponent, setHighlightedComponent] = useState(null);
  const [overlayContent, setOverlayContent] = useState(null);

  return (
    <UIContext.Provider value={{ 
      isOverlayActive, 
      setIsOverlayActive, 
      highlightedComponent, 
      setHighlightedComponent,
      overlayContent,
      setOverlayContent
    }}>
      {children}
    </UIContext.Provider>
  );
}

// Custom hook for easy access
export function useUIContext() {
  return useContext(UIContext);
} 