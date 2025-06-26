import React from 'react';
import PileOfCards from './PileOfCards';
import './PlayerBar.css';

function PlayerBar({ position, player }) {
  return (
    <div className={`player-bar player-bar-${position}`}>
      <div className="player-name-label">{player?.name || 'Player'}</div>
      <div className="deck-pile">
        <PileOfCards 
          assortedCardCollection={player?.deck} 
          show={false}
        />
      </div>
      <div className="discard-pile">
        <PileOfCards 
          assortedCardCollection={player?.discardPile} 
          show={true}
        />
      </div>
    </div>
  );
}

export default PlayerBar; 