import { GameStateStruct, States, CurrentPlayer, Player, Board, AssortedCardCollection } from '../models/GameState';

// Default game state for testing
export const defaultGameState = new GameStateStruct({
  currentState: States.ACTION_PHASE,
  currentPlayer: CurrentPlayer.PLAYER_1,
  playerStates: [
    new Player({
      name: 'Player 1',
      currentGold: 8,
      hand: new AssortedCardCollection({
        _cards: new Map([[2, 2], [3, 3], [4, 4], [5, 5], [6, 6], [7, 7], [8, 8], [9, 9], [10, 10], [11, 11]]), // 2 copies of card 1, 1 copy of card 3, 3 copies of card 5
        _size: 6
      }),
      activePowerId: 1,
      powerAvailable: true
    }),
    new Player({
      name: 'Player 2', 
      currentGold: 5,
      hand: new AssortedCardCollection({
        _cards: new Map([[2, 1], [4, 2]]), // 1 copy of card 2, 2 copies of card 4
        _size: 3
      }),
      activePowerId: 2,
      powerAvailable: false
    })
  ],
  boardState: new Board(),
  entityData: {}
});