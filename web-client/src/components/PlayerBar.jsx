import React from 'react';
import PileOfCards from './PileOfCards';
import IconWithLabel from './IconWithLabel';
import './PlayerBar.css';

function PlayerBar({ position, player }) {
  // Determine HP label text color based on conditions
  const getHpTextColor = () => {
    if (player?.damageTokens > 0) {
      return 'var(--text-damage)';
    } else if (player?.hp?.modifier > 0) {
      return 'var(--text-buff)';
    }
    return 'var(--text-white)';
  };

  return (
    <div className={`player-bar player-bar-${position}`}>
      <div className="player-name-label">{player?.name || 'Player'}</div>
      <div className="player-hp">
        <IconWithLabel 
          elementName="hp" 
          label={(player?.hp?.total || 0) - (player?.damageTokens || 0)} 
          fontSize="12vh" 
          textColor={getHpTextColor()}
        />
      </div>
      <div className="gold-icon">
        <IconWithLabel elementName="gold" label={player?.currentGold || 0} fontSize="12vh" />
      </div>
      <div className="player-hand">
        <IconWithLabel elementName="hand" label={player?.hand?._size || 0} fontSize="12vh" />
      </div>
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