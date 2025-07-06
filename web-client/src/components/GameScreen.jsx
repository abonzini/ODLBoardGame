import { GameContextProvider } from '../context/GameContext';
import { HighlightedCardsProvider } from '../context/HighlightedCardsContext';
import { HighlightedEntitiesProvider } from '../context/HighlightedEntitiesContext';
import { HighlightedLaneProvider } from '../context/HighlightedLaneContext';
import { HighlightedTileProvider } from '../context/HighlightedTileContext';
import { useGameContext } from '../context/GameContext';
import { CurrentPlayer } from '../models/GameState';
import GameBoard from './GameBoard';
import PlayerHandBar from './PlayerHandBar';
import PlayerBar from './PlayerBar';
import './GameScreen.css';

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
      <div className="leftplayerbar">
        <PlayerBar position="left" player={leftPlayer} />
      </div>
      <div className="rightplayerbar">
        <PlayerBar position="right" player={rightPlayer} />
      </div>
      <div className="playerhandbar">
        <PlayerHandBar />
      </div>
      <div className="gameboard">
        <GameBoard />
      </div>
    </div>
  );
}

function GameScreen() {
  return (
    <GameContextProvider>
      <HighlightedCardsProvider>
        <HighlightedEntitiesProvider>
          <HighlightedLaneProvider>
            <HighlightedTileProvider>
              <GameScreenContent />
            </HighlightedTileProvider>
          </HighlightedLaneProvider>
        </HighlightedEntitiesProvider>
      </HighlightedCardsProvider>
    </GameContextProvider>
  );
}

export default GameScreen; 