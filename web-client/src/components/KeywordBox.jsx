import React from 'react';
import { getLayoutElementPath } from '../utils/imagePaths.js';
import './KeywordBox.css';

const KeywordBox = ({ keyword, style }) => {
  if (!keyword) {
    return null;
  }

  return (
    <div className="keyword-box" style={style}>
      {keyword.hasImage && (
        <img 
          src={getLayoutElementPath(keyword.name.toLowerCase())}
          alt={keyword.name}
          className="keyword-image"
        />
      )}
      <div className="keyword-content">
        <div className="keyword-title">{keyword.name}</div>
        <div className="keyword-description">{keyword.description}</div>
      </div>
    </div>
  );
};

export default KeywordBox; 