import React from 'react';
import IconWithLabel from './IconWithLabel';
import { getHpTextColor, getStatColor, getBorderColor } from '../utils/colorUtils';
import { getRawCardImagePath } from '../utils/imagePaths';
import './LivingEntity.css';

function LivingEntity({ entity, visualMode }) {
  return (
    <div className="living-entity">
      <div className={visualMode}>
        {entity && (
          <div className="hp-area">
            <IconWithLabel 
              elementName="hp" 
              label={(entity?.hp?.total ?? 0) - (entity?.damageTokens ?? 0)} 
              textColor={getHpTextColor(entity)}
              hasBorder={false}
            />
          </div>
        )}
        {entity?.attack && (
          <div className="attack-area">
            <IconWithLabel 
              elementName="attack" 
              label={(entity?.attack?.total)}
              textColor={getStatColor(entity?.attack)}
              hasBorder={false}
            />
          </div>
        )}
        {entity?.movement && (
          <div className="movement-area">
            <IconWithLabel 
              elementName="movement" 
              label={(entity?.movement?.total)}
              textColor={getStatColor(entity?.movement)}
              hasBorder={false}
            />
          </div>
        )}
        <div 
          className="mainbox-area"
          style={{
            border: entity ? '5px solid ' + getBorderColor(entity) : undefined,
            backgroundImage: entity?.id ? `url(${getRawCardImagePath(entity.id)})` : undefined,
            backgroundSize: 'cover',
            backgroundPosition: 'center',
            backgroundRepeat: 'no-repeat'
          }}
        />
      </div>
    </div>
  );
}

export default LivingEntity; 