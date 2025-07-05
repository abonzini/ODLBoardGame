import React from 'react';
import LivingEntity from './LivingEntity';
import { PlacedEntity, Stat } from '../models/GameState';
import './GameBoard.css';

function GameBoard() {
  // Test entity from defaultGameState
  const testEntity = new PlacedEntity({
    name: 'Test Unit',
    hp: new Stat({ baseValue: 20, modifier: 0 }),
    owner: 0,
    uniqueId: 2,
    id: 1,
    damageTokens: 0,
    tileCoordinate: 5,
    movement: new Stat({ baseValue: 1, modifier: 0 }),
    attack: new Stat({ baseValue: 2, modifier: 0 })
  });

  return (
    <div className="board-area">
      <LivingEntity entity={testEntity} visualMode="left" />
    </div>
  );
}

export default GameBoard; 