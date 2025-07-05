import React from 'react';
import useSound from 'use-sound';

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

  // Derive 3D colors from base color
  const getColorShades = (baseColor) => {
    // Simple color manipulation - in a real app you might want a proper color library
    const lighter = baseColor === '#FF0000' ? '#FF4444' : '#4CAF50';
    const darker = baseColor === '#FF0000' ? '#CC0000' : '#0D5C0D';
    const hover = baseColor === '#FF0000' ? '#FF6666' : '#5CBF60';
    const pressed = baseColor === '#FF0000' ? '#CC0000' : '#0D5C0D';
    
    return { lighter, darker, hover, pressed };
  };

  const colors = getColorShades(color);
  const isEnabled = !disabled;

  const handleClick = () => {
    if (isEnabled) {
      playClick();
      onClick();
    }
  };

  const handleMouseEnter = (e) => {
    if (isEnabled) {
      e.target.style.backgroundColor = colors.hover;
      e.target.style.transform = 'translateY(-1px)';
      e.target.style.boxShadow = '3px 3px 6px rgba(0,0,0,0.4)';
      e.target.style.borderTopColor = colors.hover;
      e.target.style.borderLeftColor = colors.hover;
      e.target.style.borderRightColor = colors.darker;
      e.target.style.borderBottomColor = colors.darker;
    }
  };

  const handleMouseLeave = (e) => {
    if (isEnabled) {
      e.target.style.backgroundColor = color;
      e.target.style.transform = 'translateY(0px)';
      e.target.style.boxShadow = '2px 2px 4px rgba(0,0,0,0.3)';
      e.target.style.borderTopColor = colors.lighter;
      e.target.style.borderLeftColor = colors.lighter;
      e.target.style.borderRightColor = colors.darker;
      e.target.style.borderBottomColor = colors.darker;
    }
  };

  const handleMouseDown = (e) => {
    if (isEnabled) {
      e.target.style.backgroundColor = colors.pressed;
      e.target.style.transform = 'translateY(2px)';
      e.target.style.boxShadow = 'inset 2px 2px 4px rgba(0,0,0,0.3)';
      e.target.style.borderTopColor = colors.pressed;
      e.target.style.borderLeftColor = colors.pressed;
      e.target.style.borderRightColor = colors.lighter;
      e.target.style.borderBottomColor = colors.lighter;
    }
  };

  const handleMouseUp = (e) => {
    if (isEnabled) {
      e.target.style.backgroundColor = colors.hover;
      e.target.style.transform = 'translateY(-1px)';
      e.target.style.boxShadow = '3px 3px 6px rgba(0,0,0,0.4)';
      e.target.style.borderTopColor = colors.hover;
      e.target.style.borderLeftColor = colors.hover;
      e.target.style.borderRightColor = colors.darker;
      e.target.style.borderBottomColor = colors.darker;
    }
  };

  return (
    <button 
      disabled={disabled}
      onClick={handleClick}
      onMouseEnter={handleMouseEnter}
      onMouseLeave={handleMouseLeave}
      onMouseDown={handleMouseDown}
      onMouseUp={handleMouseUp}
      style={{ 
        width,
        height,
        fontSize,
        fontFamily: 'Georgia',
        backgroundColor: isEnabled ? color : '#666',
        color: 'white',
        textShadow: '1px 1px 0px black, -1px -1px 0px black, 1px -1px 0px black, -1px 1px 0px black',
        // Classic 3D button styling
        border: '3px solid',
        borderTopColor: isEnabled ? colors.lighter : '#888',
        borderLeftColor: isEnabled ? colors.lighter : '#888',
        borderRightColor: isEnabled ? colors.darker : '#444',
        borderBottomColor: isEnabled ? colors.darker : '#444',
        cursor: isEnabled ? 'pointer' : 'not-allowed',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        // Add some padding to account for the 3D effect
        padding: '2px',
        // Box shadow for additional depth
        boxShadow: isEnabled ? '2px 2px 4px rgba(0,0,0,0.3)' : 'none',
        // Smooth transitions for hover and active states
        transition: 'all 0.1s ease',
        // Remove default button styling
        outline: 'none',
        // Position
        position,
        top,
        left,
        containerType: 'size',
        ...style
      }}
    >
      {text}
    </button>
  );
}

export default Button3D; 