import React from 'react';
import { getLayoutElementPath } from '../utils/imagePaths';
import './IconWithLabel.css';

function IconWithLabel({ elementName, label, textColor = 'white', hasBorder = true }) {
  return (
    <div className="icon-with-label">
      <div 
        className="icon"
        style={{ 
          backgroundImage: elementName ? `url(${getLayoutElementPath(elementName)})` : 'none',
          border: hasBorder ? undefined : 'none'
        }}
      >
        {label != null && (
          <div 
            className="label"
            style={{
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