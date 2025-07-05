import React from 'react';
import IconWithLabel from './IconWithLabel';
import { getHpTextColor } from '../utils/colorUtils';
import './LivingEntity.css';

function LivingEntity({ entity, visualMode }) {
  return (
    <div className="living-entity">
      <div className={visualMode}>
        <div className="hp-area">
            <IconWithLabel 
              elementName="hp" 
              label={(entity?.hp?.total ?? 0) - (entity?.damageTokens ?? 0)} 
              textColor={getHpTextColor(entity)}
            />
        </div>
        <div className="attack-area">Attack</div>
        <div className="movement-area">Movement</div>
        <div className="mainbox-area">Main</div>
      </div>
    </div>
  );
}

export default LivingEntity; 