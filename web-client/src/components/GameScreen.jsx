import { GameContextProvider } from '../context/GameContext';
import GameBoard from './GameBoard';
import PlayerHandBar from './PlayerHandBar';

function GameScreen() {
  return (
    <GameContextProvider>
      <div className="game-screen">
        <div className="game-screen-wood"></div>
        <GameBoard />
        <PlayerHandBar />
      </div>
    </GameContextProvider>
  );
}
export default GameScreen; 