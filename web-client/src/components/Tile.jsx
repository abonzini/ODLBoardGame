import React from 'react';
import { useHighlightedTiles } from '../context/HighlightedTileContext';
import { getLayoutElementPath } from '../utils/imagePaths';
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
  
  const isHighlighted = highlightedTileIds.includes(tileId);

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
      </div>
    </div>
  );
}

export default Tile; 