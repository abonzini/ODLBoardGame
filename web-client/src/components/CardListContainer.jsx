import React from 'react';
import ActivePower from './ActivePower';
import Card from './Card';
import './CardListContainer.css';

function CardListContainer({ activePowerId, activePowerAvailable, assortedCardCollection, centered = false }) {
  return (
    <div className={`card-list-container${centered ? ' centered' : ''}`}>
      {activePowerId && (
        <ActivePower cardId={activePowerId} available={activePowerAvailable} />
      )}
      {assortedCardCollection && assortedCardCollection.getCards && 
        assortedCardCollection.getCards().map(({ cardId, count }) => (
          <Card key={cardId} cardId={cardId} count={count} />
        ))
      }
    </div>
  );
}

export default CardListContainer; 