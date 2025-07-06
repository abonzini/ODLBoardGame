import React from 'react';
import { useHighlightedBoard } from '../context/HighlightedBoardContext';
import { useGameContext } from '../context/GameContext';
import { CurrentPlayer } from '../models/GameState';
import Lane from './Lane';
import Tile from './Tile';
import './GameBoard.css';

function GameBoard() {
  const { isHighlighted } = useHighlightedBoard();
  const { viewerIdentity } = useGameContext();

  const laneConfigs = [
    { laneId: 0, gridArea: '1 / 3 / 2 / 7' },
    { laneId: 1, gridArea: '2 / 2 / 3 / 8' },
    { laneId: 2, gridArea: '3 / 1 / 4 / 9' }
  ];

  const tileConfigs = [
    { id: 0, gridArea: '1 / 3' },
    { id: 1, gridArea: '1 / 4' },
    { id: 2, gridArea: '1 / 5' },
    { id: 3, gridArea: '1 / 6' },
    { id: 4, gridArea: '2 / 2' },
    { id: 5, gridArea: '2 / 3' },
    { id: 6, gridArea: '2 / 4' },
    { id: 7, gridArea: '2 / 5' },
    { id: 8, gridArea: '2 / 6' },
    { id: 9, gridArea: '2 / 7' },
    { id: 10, gridArea: '3 / 1' },
    { id: 11, gridArea: '3 / 2' },
    { id: 12, gridArea: '3 / 3' },
    { id: 13, gridArea: '3 / 4' },
    { id: 14, gridArea: '3 / 5' },
    { id: 15, gridArea: '3 / 6' },
    { id: 16, gridArea: '3 / 7' },
    { id: 17, gridArea: '3 / 8' }
  ];

  const transformIdToTileId = (id) => {
    if (viewerIdentity === CurrentPlayer.PLAYER_2) {
      // For PLAYER_2 view: apply the correct transformation formula
      if (id >= 0 && id <= 3) {
        return 3 - id;
      } else if (id >= 4 && id <= 9) {
        return 13 - id;
      } else if (id >= 10 && id <= 17) {
        return 27 - id;
      }
    }
    // For OMNISCIENT, PLAYER_1, SPECTATOR: normal order
    return id;
  };

  return (
    <div 
      className="board-area"
      style={{
        boxShadow: isHighlighted ? 'inset 0 0 10px 2px var(--highlighted-color)' : 'none'
      }}
    >
      {tileConfigs.map(({ id, gridArea }) => (
        <div key={id} style={{ gridArea }}>
          <Tile tileId={transformIdToTileId(id)} />
        </div>
      ))}
      {laneConfigs.map(({ laneId, gridArea }) => (
        <div key={laneId} style={{ gridArea }}>
          <Lane laneId={laneId} />
        </div>
      ))}
    </div>
  );
}

export default GameBoard; 