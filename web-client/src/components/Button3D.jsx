import React from 'react';
import useSound from 'use-sound';
import './Button3D.css';

function Button3D({ 
  text, 
  onClick, 
  color = '#119C00', 
  width = '60%', 
  height = '60%', 
  fontSize = '20px',
  position = 'relative',
  top = 'auto',
  left = 'auto',
  disabled = false,
  style = {}
}) {
  // Use use-sound for button click audio
  const [playClick] = useSound('/sounds/button-click.wav', {
    volume: 1.0,
    interrupt: true
  });

  const handleClick = () => {
    if (!disabled) {
      playClick();
      onClick();
    }
  };

  return (
    <button 
      className={`button3d ${disabled ? 'disabled' : ''}`}
      disabled={disabled}
      onClick={handleClick}
      style={{ 
        width,
        height,
        fontSize,
        position,
        top,
        left,
        backgroundColor: color,
        ...style
      }}
    >
      {text}
    </button>
  );
}

export default Button3D; 