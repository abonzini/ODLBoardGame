import React from 'react';
import { useHighlightedTiles } from '../context/HighlightedTileContext';
import { useGameContext } from '../context/GameContext';
import { getLayoutElementPath } from '../utils/imagePaths';
import EntityList from './EntityList';
import LivingEntity from './LivingEntity';
import { CurrentPlayer, EntityType } from '../models/GameState';
import './Tile.css';

const getTileBackgroundPath = (tileId) => {
  if (tileId <= 3) {
    return getLayoutElementPath('plains');
  } else if (tileId <= 9) {
    return getLayoutElementPath('forest');
  } else if (tileId <= 17) {
    return getLayoutElementPath('mountain');
  }
  return getLayoutElementPath('plains'); // fallback
};

function Tile({ tileId }) {
  const { highlightedTileIds } = useHighlightedTiles();
  const { gameState, viewerIdentity } = useGameContext();
  
  const isHighlighted = highlightedTileIds.includes(tileId);

  // Find entities that belong to this tile
  const entitiesOnThisTile = Array.from(gameState?.entityData?.values() || [])
    .filter(entity => entity.tileCoordinate === tileId);

  // Determine current player context
  const currentPlayerId = viewerIdentity === CurrentPlayer.PLAYER_2 ? 1 : 0;

  // Separate entities by type and owner
  const leftEntities = [];
  const rightEntities = [];
  let buildingEntity = null;

  entitiesOnThisTile.forEach(entity => {
    if (entity.entityType === EntityType.BUILDING) {
      buildingEntity = entity;
    } else if (entity.entityType === EntityType.UNIT) {
      if (entity.owner === currentPlayerId) {
        leftEntities.push(entity);
      } else {
        rightEntities.push(entity);
      }
    }
  });

  return (
    <div 
      className="tile"
    >
      <div 
        className="tile-box"
        style={{
          backgroundImage: `url(${getTileBackgroundPath(tileId)})`,
          backgroundSize: 'cover',
          backgroundPosition: 'center',
          backgroundRepeat: 'no-repeat',
          boxShadow: isHighlighted ? '0 0 10px 2px var(--highlighted-color)' : 'none'

        }}
      >
        <div className="tile-overlay"></div>
        <div className="tile-entity-lists">
          <div className="tile-building">
            <LivingEntity 
              entity={buildingEntity}
              visualMode="bottom"
            />
          </div>
          <EntityList 
            entities={leftEntities}
            visualMode="left"
            width="35%"
            height="100%"
          />
          <EntityList 
            entities={rightEntities}
            visualMode="right"
            width="35%"
            height="100%"
          />
        </div>
      </div>
    </div>
  );
}

export default Tile; 