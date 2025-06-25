import { GameContextProvider } from '../context/GameContext';
import { useGameContext } from '../context/GameContext';
import { CurrentPlayer } from '../models/GameState';
import GameBoard from './GameBoard';
import PlayerHandBar from './PlayerHandBar';
import PlayerBar from './PlayerBar';

function GameScreenContent() {
  const { viewerIdentity, gameState } = useGameContext();
  
  const player1 = gameState?.playerStates?.[0];
  const player2 = gameState?.playerStates?.[1];
  
  // Determine player assignment based on viewer identity
  let leftPlayer, rightPlayer;
  
  if (viewerIdentity === CurrentPlayer.PLAYER_2) {
    // For PLAYER_2 view: reversed
    leftPlayer = player2;
    rightPlayer = player1;
  } else {
    // For OMNISCIENT, PLAYER_1, SPECTATOR: normal
    leftPlayer = player1;
    rightPlayer = player2;
  }

  return (
    <div className="game-screen">
      <div className="game-screen-wood"></div>
      <div className="player-bar-container">
        <PlayerBar position="left" player={leftPlayer} />
        <PlayerBar position="right" player={rightPlayer} />
      </div>
      <GameBoard />
      <PlayerHandBar />
    </div>
  );
}

function GameScreen() {
  return (
    <GameContextProvider>
      <GameScreenContent />
    </GameContextProvider>
  );
}

export default GameScreen; 