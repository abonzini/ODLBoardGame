import React from 'react';
import './PlayerBar.css';

function PlayerBar({ position, player }) {
  return (
    <div className={`player-bar player-bar-${position}`}>
      <div className="player-name-label">{player?.name || 'Player'}</div>
    </div>
  );
}

export default PlayerBar; 