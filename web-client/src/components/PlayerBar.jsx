import React from 'react';
import PileOfCards from './PileOfCards';
import IconWithLabel from './IconWithLabel';
import { getBorderColor, getHpTextColor } from '../utils/colorUtils';
import './PlayerBar.css';

function PlayerBar({ position, player }) {

  return (
    <div 
      className={`player-bar player-bar-${position}`}
      style={{ borderColor: getBorderColor(player) }}
    >
      <div className="player-name-label">{player?.name || 'Player'}</div>
      <div className="player-hp">
        <IconWithLabel 
          elementName="hp" 
          label={(player?.hp?.total ?? 0) - (player?.damageTokens ?? 0)} 
          textColor={getHpTextColor(player)}
        />
      </div>
      <div className="gold-icon">
        <IconWithLabel elementName="gold" label={player?.currentGold ?? 0} />
      </div>
      <div className="player-hand">
        <IconWithLabel elementName="hand" label={player?.hand?._size ?? 0} />
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