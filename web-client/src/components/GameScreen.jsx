import { GameContextProvider } from '../context/GameContext';
import PlayerHandBar from './PlayerHandBar';

function GameScreen() {
  return (
    <GameContextProvider>
      <div className="game-screen">
        <div className="board-area">
          {/* Main game area goes here */}
        </div>
        <PlayerHandBar />
      </div>
    </GameContextProvider>
  );
}
export default GameScreen; 