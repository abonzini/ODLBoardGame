import React from 'react';
import LivingEntity from './LivingEntity';
import './EntityList.css';

function EntityList({ entities = [], visualMode, width = '100%', height = '100%' }) {
  return (
    <div 
      className="entity-list"
      style={{
        width: width,
        height: height
      }}
    >
      {entities.map(entity => (
        <div key={entity.uniqueId} className="entity-item">
          <LivingEntity 
            entity={entity} 
            visualMode={visualMode}
          />
        </div>
      ))}
    </div>
  );
}

export default EntityList; 