import { useGameContext } from '../context/GameContext';
import { CurrentPlayer } from '../models/GameState';
import CardListContainer from './CardListContainer';
import Button3D from './Button3D';

function PlayerHandBar() {
  const { viewerIdentity, gameState } = useGameContext();
  
  // Don't render anything for spectators
  if (viewerIdentity === CurrentPlayer.SPECTATOR) {
    return null;
  }
  
  // For OMNISCIENT, show both players' hands side by side
  else if (viewerIdentity === CurrentPlayer.OMNISCIENT) {
    const player1 = gameState?.playerStates?.[0];
    const player2 = gameState?.playerStates?.[1];
    
    return (
      <div className="player-hand-bar">
        <div className="player-hand-left">
          <CardListContainer 
            activePowerId={player1?.activePowerId || 1}
            activePowerAvailable={player1?.powerAvailable || false}
            assortedCardCollection={player1?.hand || { cards: new Map() }}
          />
        </div>
        <div className="player-hand-right">
          <CardListContainer 
            activePowerId={player2?.activePowerId || 1}
            activePowerAvailable={player2?.powerAvailable || false}
            assortedCardCollection={player2?.hand || { cards: new Map() }}
          />
        </div>
      </div>
    );
  }
  
  // For regular players (PLAYER_1, PLAYER_2), show their own hand + end turn button
  else {
    // Check if it's the viewer's turn
    const isViewerTurn = gameState && gameState.currentPlayer === viewerIdentity;
    
    // Get the current player's data
    const playerIndex = viewerIdentity === CurrentPlayer.PLAYER_1 ? 0 : 1;
    const currentPlayer = gameState?.playerStates?.[playerIndex];
    
    console.log('PlayerHandBar - currentPlayer:', currentPlayer);
    console.log('PlayerHandBar - currentPlayer?.hand:', currentPlayer?.hand);
    console.log('PlayerHandBar - hand type:', typeof currentPlayer?.hand);
    console.log('PlayerHandBar - hand getCards:', currentPlayer?.hand?.getCards);
    
    return (
      <div className="player-hand-bar">
        <div className="player-hand-left-regular">
          <CardListContainer 
            activePowerId={currentPlayer?.activePowerId || 1}
            activePowerAvailable={currentPlayer?.powerAvailable || false}
            assortedCardCollection={currentPlayer?.hand || { cards: new Map() }}
          />
        </div>
        <div style={{ width: '20%', height: '100%', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
          <Button3D 
            text="End Turn"
            onClick={() => {
              // End turn logic will go here
              console.log('End turn clicked');
            }}
            color={isViewerTurn ? '#119C00' : '#666'}
            width="60%"
            height="60%"
            fontSize="4vh"
            disabled={!isViewerTurn}
          />
        </div>
      </div>
    );
  }
}

export default PlayerHandBar; 