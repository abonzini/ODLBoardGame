import React, { createContext, useContext, useState } from 'react';
import { GameStateStruct, CurrentPlayer } from '../models/GameState';
import { defaultGameState } from '../testData/defaultGameState';

// Create the context
const GameContext = createContext();

export function GameContextProvider({ children }) {
  // State for viewer's identity/perspective
  const [viewerIdentity, setViewerIdentity] = useState(CurrentPlayer.PLAYER_1);
  // State for the current game state (will be a GameStateStruct instance)
  const [gameState, setGameState] = useState(defaultGameState);

  return (
    <GameContext.Provider value={{ viewerIdentity, setViewerIdentity, gameState, setGameState }}>
      {children}
    </GameContext.Provider>
  );
}

// Custom hook for easy access
export function useGameContext() {
  return useContext(GameContext);
} 