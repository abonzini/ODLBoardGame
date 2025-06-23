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

// Basic classes matching the C# structures
export class Player {
  constructor(data = {}) {
    // Add player properties as needed
    Object.assign(this, data);
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
    return new GameStateStruct(json);
  }
} 