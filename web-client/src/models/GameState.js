// Enums matching the C# definitions
export const CurrentPlayer = {
  PLAYER_1: 'PLAYER_1',
  PLAYER_2: 'PLAYER_2',
  SPECTATOR: 'SPECTATOR',
  OMNISCIENT: 'OMNISCIENT',
};

export const States = {
  START: 'START',
  P1_INIT: 'P1_INIT',
  P2_INIT: 'P2_INIT',
  DRAW_PHASE: 'DRAW_PHASE',
  ACTION_PHASE: 'ACTION_PHASE',
  EOT: 'EOT',
  EOG: 'EOG',
};

// AssortedCardCollection class matching C# structure
export class AssortedCardCollection {
  constructor(data = {}) {
    this._cardHistogram = data._cardHistogram || new Map(); // SortedList<int, int> equivalent - cardId -> count
    this._size = data._size || 0;
  }

  // Get all cards as array of {cardId, count} for easy iteration
  getCards() {
    return Array.from(this._cardHistogram.entries()).map(([cardId, count]) => ({
      cardId,
      count
    }));
  }

  // Helper method to create from JSON (when received from server)
  static fromJson(json) {
    if (!json) return new AssortedCardCollection();
    
    const collection = new AssortedCardCollection();
    // Convert string keys back to numbers for the Map
    const cardsMap = new Map();
    if (json._cardHistogram) {
      Object.entries(json._cardHistogram).forEach(([cardIdStr, count]) => {
        cardsMap.set(parseInt(cardIdStr), count);
      });
    }
    collection._cardHistogram = cardsMap;
    collection._size = json._size || 0;
    return collection;
  }
}

// Deck class matching C# structure
export class Deck extends AssortedCardCollection {
  constructor(data = {}) {
    super(data);
    this._orderedCards = data._orderedCards || [];
  }

  // Helper method to create from JSON (when received from server)
  static fromJson(json) {
    if (!json) return new Deck();
    
    const deck = new Deck();
    // Convert string keys back to numbers for the Map
    const cardsMap = new Map();
    if (json._cardHistogram) {
      Object.entries(json._cardHistogram).forEach(([cardIdStr, count]) => {
        cardsMap.set(parseInt(cardIdStr), count);
      });
    }
    deck._cardHistogram = cardsMap;
    deck._size = json._size || 0;
    deck._orderedCards = json._orderedCards || [];
    
    return deck;
  }
}

// Basic classes matching the C# structures
export class Player {
  constructor(data = {}) {
    // LivingEntity properties
    this.hp = data.hp || 20;
    this.name = data.name || '';
    this.owner = data.owner || null;
    this.uniqueId = data.uniqueId || null;
    
    // Player-specific properties
    this.currentGold = data.currentGold || 5;
    this.powerAvailable = data.powerAvailable !== undefined ? data.powerAvailable : true;
    this.hand = data.hand || new AssortedCardCollection();
    this.deck = data.deck || new Deck();
    this.discardPile = data.discardPile || new AssortedCardCollection();
    this.activePowerId = data.activePowerId || 1;
  }

  // Helper method to create from JSON (when received from server)
  static fromJson(json) {
    if (!json) return new Player();
    
    const player = new Player();
    // LivingEntity properties
    player.hp = json.hp || 20;
    player.name = json.name || '';
    player.owner = json.owner || null;
    player.uniqueId = json.uniqueId || null;
    
    // Player-specific properties
    player.currentGold = json.currentGold || 5;
    player.powerAvailable = json.powerAvailable !== undefined ? json.powerAvailable : true;
    player.hand = json.hand ? AssortedCardCollection.fromJson(json.hand) : new AssortedCardCollection();
    player.deck = json.deck ? Deck.fromJson(json.deck) : new Deck();
    player.discardPile = json.discardPile ? AssortedCardCollection.fromJson(json.discardPile) : new AssortedCardCollection();
    player.activePowerId = json.activePowerId || 1;
    
    return player;
  }
}

export class Board {
  constructor(data = {}) {
    // Add board properties as needed
    Object.assign(this, data);
  }
}

export class LivingEntity {
  constructor(data = {}) {
    // Add entity properties as needed
    Object.assign(this, data);
  }
}

// Main GameStateStruct class
export class GameStateStruct {
  constructor(data = {}) {
    this.currentState = data.currentState || States.START;
    this.stateHash = data.stateHash || 0;
    this.currentPlayer = data.currentPlayer || CurrentPlayer.OMNISCIENT;
    this.playerStates = data.playerStates || [new Player(), new Player()];
    this.boardState = data.boardState || new Board();
    this.entityData = data.entityData || {};
  }

  // Helper method to create from JSON (when received from server)
  static fromJson(json) {
    if (!json) return new GameStateStruct();
    
    const gameState = new GameStateStruct();
    gameState.currentState = json.currentState || States.START;
    gameState.stateHash = json.stateHash || 0;
    gameState.currentPlayer = json.currentPlayer || CurrentPlayer.OMNISCIENT;
    gameState.playerStates = (json.playerStates || []).map(playerData => Player.fromJson(playerData));
    gameState.boardState = new Board(json.boardState || {});
    gameState.entityData = json.entityData || {};
    
    return gameState;
  }
} 