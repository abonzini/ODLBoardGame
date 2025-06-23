import { GameStateStruct, States, CurrentPlayer, Player, Board } from '../models/GameState';

// Default game state for testing
export const defaultGameState = new GameStateStruct({
  currentState: States.ACTION_PHASE,
  currentPlayer: CurrentPlayer.PLAYER_1,
  playerStates: [new Player(), new Player()],
  boardState: new Board(),
  entityData: {}
});