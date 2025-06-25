import { useGameContext } from '../context/GameContext';
import useSound from 'use-sound';
import { CurrentPlayer } from '../models/GameState';
import CardListContainer from './CardListContainer';

function PlayerHandBar() {
  const { viewerIdentity, gameState } = useGameContext();
  
  // Use use-sound for button click audio
  const [playClick] = useSound('/sounds/button-click.wav', {
    volume: 1.0,
    interrupt: true // Allow interrupting previous plays
  });

  // Don't render anything for spectators
  if (viewerIdentity === CurrentPlayer.SPECTATOR) {
    return null;
  }
  // For OMNISCIENT, show both players' hands side by side
  else if (viewerIdentity === CurrentPlayer.OMNISCIENT) {
    return (
      <div className="player-hand-bar">
        <div className="player-hand-left">
          <CardListContainer />
        </div>
        <div className="player-hand-right">
          <CardListContainer />
        </div>
      </div>
    );
  }
  // For regular players (PLAYER_1, PLAYER_2), show hand + end turn button
  else {
    // Check if it's the viewer's turn
    const isViewerTurn = gameState && gameState.currentPlayer === viewerIdentity;
    
    return (
      <div className="player-hand-bar">
        <div className="player-hand-left-regular">
          <CardListContainer />
        </div>
        <div style={{ width: '20%', height: '100%', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
          <button 
            disabled={!isViewerTurn}
            onClick={playClick}
            style={{ 
              width: '60%',
              height: '60%',
              fontSize: '4vh',
              fontFamily: 'Georgia',
              backgroundColor: isViewerTurn ? '#119C00' : '#666',
              color: 'white',
              // Classic 3D button styling
              border: '3px solid',
              borderTopColor: isViewerTurn ? '#4CAF50' : '#888',
              borderLeftColor: isViewerTurn ? '#4CAF50' : '#888',
              borderRightColor: isViewerTurn ? '#0D5C0D' : '#444',
              borderBottomColor: isViewerTurn ? '#0D5C0D' : '#444',
              cursor: isViewerTurn ? 'pointer' : 'not-allowed',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              // Add some padding to account for the 3D effect
              padding: '2px',
              // Box shadow for additional depth
              boxShadow: isViewerTurn ? '2px 2px 4px rgba(0,0,0,0.3)' : 'none',
              // Smooth transitions for hover and active states
              transition: 'all 0.1s ease',
              // Remove default button styling
              outline: 'none',
              // Add hover effect
              ':hover': isViewerTurn ? {
                backgroundColor: '#15B300',
                transform: 'translateY(-1px)',
                boxShadow: '3px 3px 6px rgba(0,0,0,0.4)',
                borderTopColor: '#5CBF60',
                borderLeftColor: '#5CBF60',
                borderRightColor: '#0F8A00',
                borderBottomColor: '#0F8A00'
              } : {},
              // Add active (pressed) effect
              ':active': isViewerTurn ? {
                backgroundColor: '#0D5C0D',
                transform: 'translateY(2px)',
                boxShadow: 'inset 2px 2px 4px rgba(0,0,0,0.3)',
                borderTopColor: '#0D5C0D',
                borderLeftColor: '#0D5C0D',
                borderRightColor: '#4CAF50',
                borderBottomColor: '#4CAF50'
              } : {}
            }}
            onMouseEnter={(e) => {
              if (isViewerTurn) {
                e.target.style.backgroundColor = '#15B300';
                e.target.style.transform = 'translateY(-1px)';
                e.target.style.boxShadow = '3px 3px 6px rgba(0,0,0,0.4)';
                e.target.style.borderTopColor = '#5CBF60';
                e.target.style.borderLeftColor = '#5CBF60';
                e.target.style.borderRightColor = '#0F8A00';
                e.target.style.borderBottomColor = '#0F8A00';
              }
            }}
            onMouseLeave={(e) => {
              if (isViewerTurn) {
                e.target.style.backgroundColor = '#119C00';
                e.target.style.transform = 'translateY(0px)';
                e.target.style.boxShadow = '2px 2px 4px rgba(0,0,0,0.3)';
                e.target.style.borderTopColor = '#4CAF50';
                e.target.style.borderLeftColor = '#4CAF50';
                e.target.style.borderRightColor = '#0D5C0D';
                e.target.style.borderBottomColor = '#0D5C0D';
              }
            }}
            onMouseDown={(e) => {
              if (isViewerTurn) {
                e.target.style.backgroundColor = '#0D5C0D';
                e.target.style.transform = 'translateY(2px)';
                e.target.style.boxShadow = 'inset 2px 2px 4px rgba(0,0,0,0.3)';
                e.target.style.borderTopColor = '#0D5C0D';
                e.target.style.borderLeftColor = '#0D5C0D';
                e.target.style.borderRightColor = '#4CAF50';
                e.target.style.borderBottomColor = '#4CAF50';
              }
            }}
            onMouseUp={(e) => {
              if (isViewerTurn) {
                e.target.style.backgroundColor = '#15B300';
                e.target.style.transform = 'translateY(-1px)';
                e.target.style.boxShadow = '3px 3px 6px rgba(0,0,0,0.4)';
                e.target.style.borderTopColor = '#5CBF60';
                e.target.style.borderLeftColor = '#5CBF60';
                e.target.style.borderRightColor = '#0F8A00';
                e.target.style.borderBottomColor = '#0F8A00';
              }
            }}
          >
            End Turn
          </button>
        </div>
      </div>
    );
  }
}

export default PlayerHandBar; 