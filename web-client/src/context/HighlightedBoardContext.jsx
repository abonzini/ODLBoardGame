import React, { createContext, useContext, useState } from 'react';

const HighlightedBoardContext = createContext({
  isHighlighted: false,
  setIsHighlighted: () => {}
});

export const useHighlightedBoard = () => {
  const context = useContext(HighlightedBoardContext);
  return context;
};

export const HighlightedBoardProvider = ({ children }) => {
  const [isHighlighted, setIsHighlighted] = useState(false);

  const value = {
    isHighlighted,
    setIsHighlighted
  };

  return (
    <HighlightedBoardContext.Provider value={value}>
      {children}
    </HighlightedBoardContext.Provider>
  );
}; 