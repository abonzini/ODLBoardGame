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

// Stat class matching C# structure
export class Stat {
  constructor(data = {}) {
    this.baseValue = data.baseValue ?? 0;
    this.modifier = data.modifier ?? 0;
  }

  get total() {
    return this.baseValue + this.modifier;
  }

  // Helper method to create from JSON (when received from server)
  static fromJson(json) {
    if (!json) return new Stat();
    
    // Handle both single integer and full object formats
    if (typeof json === 'number') {
      return new Stat({ baseValue: json, modifier: 0 });
    }
    
    return new Stat({
      baseValue: json.BaseValue ?? 0,
      modifier: json.Modifier ?? 0
    });
  }
}

// AssortedCardCollection class matching C# structure
export class AssortedCardCollection {
  constructor(data = {}) {
    this._cardHistogram = data._cardHistogram || new Map(); // SortedList<int, int> equivalent - cardId -> count
    this._size = data._size ?? 0;
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
    collection._size = json._size ?? 0;
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
    deck._size = json._size ?? 0;
    deck._orderedCards = json._orderedCards || [];
    
    return deck;
  }
}

// Basic classes matching the C# structures
export class Player {
  constructor(data = {}) {
    // LivingEntity properties
    this.hp = data.hp || new Stat();
    this.name = data.name || '';
    this.owner = data.owner ?? null;
    this.uniqueId = data.uniqueId ?? null;
    this.damageTokens = data.damageTokens ?? 0;
    
    // Player-specific properties
    this.currentGold = data.currentGold ?? 0;
    this.powerAvailable = data.powerAvailable ?? true;
    this.hand = data.hand || new AssortedCardCollection();
    this.deck = data.deck || new Deck();
    this.discardPile = data.discardPile || new AssortedCardCollection();
    this.activePowerId = data.activePowerId ?? 1;
  }

  // Helper method to create from JSON (when received from server)
  static fromJson(json) {
    if (!json) return new Player();
    
    const player = new Player();
    // LivingEntity properties
    player.hp = json.Hp ? Stat.fromJson(json.Hp) : new Stat();
    player.name = json.Name || '';
    player.owner = json.Owner ?? null;
    player.uniqueId = json.UniqueId ?? null;
    player.damageTokens = json.DamageTokens ?? 0;
    
    // Player-specific properties
    player.currentGold = json.CurrentGold ?? 0;
    player.powerAvailable = json.PowerAvailable ?? true;
    player.hand = json.Hand ? AssortedCardCollection.fromJson(json.Hand) : new AssortedCardCollection();
    player.deck = json.Deck ? Deck.fromJson(json.Deck) : new Deck();
    player.discardPile = json.DiscardPile ? AssortedCardCollection.fromJson(json.DiscardPile) : new AssortedCardCollection();
    player.activePowerId = json.ActivePowerId ?? 1;
    
    return player;
  }
}

export class Board {
  constructor(data = {}) {
    // Add board properties as needed
    Object.assign(this, data);
  }
}

export class PlacedEntity {
  constructor(data = {}) {
    // LivingEntity properties
    this.hp = data.hp || new Stat();
    this.name = data.name || '';
    this.owner = data.owner ?? null;
    this.uniqueId = data.uniqueId ?? null;
    this.id = data.id ?? 0;
    this.damageTokens = data.damageTokens ?? 0;
    
    // PlacedEntity-specific properties
    this.tileCoordinate = data.tileCoordinate ?? -1;
    this.movement = data.movement || null;
    this.attack = data.attack || null;
  }

  // Helper method to create from JSON (when received from server)
  static fromJson(json) {
    if (!json) return new PlacedEntity();
    
    const placedEntity = new PlacedEntity();
    // LivingEntity properties
    placedEntity.hp = json.Hp ? Stat.fromJson(json.Hp) : new Stat();
    placedEntity.name = json.Name || '';
    placedEntity.owner = json.Owner ?? null;
    placedEntity.uniqueId = json.UniqueId ?? null;
    placedEntity.id = json.Id ?? 0;
    placedEntity.damageTokens = json.DamageTokens ?? 0;
    
    // PlacedEntity-specific properties
    placedEntity.tileCoordinate = json.TileCoordinate ?? -1;
    placedEntity.movement = json.Movement ? Stat.fromJson(json.Movement) : null;
    placedEntity.attack = json.Attack ? Stat.fromJson(json.Attack) : null;
    
    return placedEntity;
  }
}

// Main GameStateStruct class
export class GameStateStruct {
  constructor(data = {}) {
    this.currentState = data.currentState || States.START;
    this.stateHash = data.stateHash ?? 0;
    this.currentPlayer = data.currentPlayer || CurrentPlayer.OMNISCIENT;
    this.playerStates = data.playerStates || [new Player(), new Player()];
    this.boardState = data.boardState || new Board();
    this.entityData = data.entityData || new Map();
  }

  // Helper method to create from JSON (when received from server)
  static fromJson(json) {
    if (!json) return new GameStateStruct();
    
    const gameState = new GameStateStruct();
    gameState.currentState = json.CurrentState || States.START;
    gameState.stateHash = json.StateHash ?? 0;
    gameState.currentPlayer = json.CurrentPlayer || CurrentPlayer.OMNISCIENT;
    gameState.playerStates = (json.PlayerStates || []).map(playerData => Player.fromJson(playerData));
    gameState.boardState = new Board(json.BoardState || {});
    
    // Convert EntityData from object to Map
    const entityDataMap = new Map();
    if (json.EntityData) {
      Object.entries(json.EntityData).forEach(([uniqueIdStr, entityJson]) => {
        const uniqueId = parseInt(uniqueIdStr);
        const entity = PlacedEntity.fromJson(entityJson);
        entityDataMap.set(uniqueId, entity);
      });
    }
    gameState.entityData = entityDataMap;
    
    return gameState;
  }
} 