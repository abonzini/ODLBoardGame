import React from 'react';
import { getLayoutElementPath } from '../utils/imagePaths';
import './IconWithLabel.css';

function IconWithLabel({ elementName, label, fontSize, textColor = 'white' }) {
  return (
    <div className="icon-with-label">
      <div 
        className="icon"
        style={{ backgroundImage: elementName ? `url(${getLayoutElementPath(elementName)})` : 'none' }}
      >
        {label && (
          <div 
            className="label"
            style={{ 
              fontSize: fontSize,
              color: textColor
            }}
          >
            {label}
          </div>
        )}
      </div>
    </div>
  );
}

export default IconWithLabel; 