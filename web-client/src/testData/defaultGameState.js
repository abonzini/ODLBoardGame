import { GameStateStruct, States, CurrentPlayer, Player, Board, AssortedCardCollection, Stat, PlacedEntity } from '../models/GameState';

// Default game state for testing
export const defaultGameState = new GameStateStruct({
  currentState: States.ACTION_PHASE,
  currentPlayer: CurrentPlayer.PLAYER_1,
  playerStates: [
    new Player({
      name: 'Player 1',
      currentGold: 8,
      hp: new Stat({ baseValue: 20, modifier: 0 }),
      damageTokens: 0,
      owner: 0,
      uniqueId: 0,
      hand: new AssortedCardCollection({
        _cardHistogram: new Map([[2, 2], [3, 3], [4, 4], [5, 5], [6, 6], [7, 7], [8, 8], [9, 9], [10, 10], [11, 11]]), // 2 copies of card 1, 1 copy of card 3, 3 copies of card 5
        _size: 6
      }),
      deck: new AssortedCardCollection({
        _cardHistogram: new Map([[1, 3], [2, 2], [3, 2], [4, 2], [5, 2], [6, 2], [7, 2]]), // 3 copies of card 1, 2 copies each of cards 2-7
        _size: 15
      }),
      discardPile: new AssortedCardCollection({
        _cardHistogram: new Map([[1, 1], [2, 2], [3, 3]]), // 1 copy of card 1, 2 copies of card 2, 3 copies of card 3
        _size: 6
      }),
      activePowerId: 1,
      powerAvailable: true
    }),
    new Player({
      name: 'Player 2', 
      currentGold: 5,
      hp: new Stat({ baseValue: 20 }),
      damageTokens: 0,
      owner: 1,
      uniqueId: 1,
      hand: new AssortedCardCollection({
        _cardHistogram: new Map([[2, 1], [4, 2]]), // 1 copy of card 2, 2 copies of card 4
        _size: 3
      }),
      deck: new AssortedCardCollection({
        _cardHistogram: new Map([[1, 2], [2, 2], [3, 2], [4, 2], [5, 2], [6, 2]]), // 2 copies each of cards 1-6
        _size: 12
      }),
      discardPile: new AssortedCardCollection({
        _cardHistogram: new Map(), // Empty discard pile
        _size: 0
      }),
      activePowerId: 2,
      powerAvailable: false
    })
  ],
  boardState: new Board(),
  entityData: new Map([
    [2, new PlacedEntity({
      name: 'Test Unit',
      hp: new Stat({ baseValue: 2, modifier: 0 }),
      owner: 0,
      uniqueId: 2,
      id: 1,
      damageTokens: 0,
      tileCoordinate: 5,
      movement: new Stat({ baseValue: 1, modifier: 0 }),
      attack: new Stat({ baseValue: 2, modifier: 0 })
    })]
  ])
});