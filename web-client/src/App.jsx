import './App.css';
import { UIContextProvider } from './context/UIContext';
import GameScreen from './components/GameScreen';
import HighlightOverlay from './components/HighlightOverlay';

function App() {
  return (
    <UIContextProvider>
      <GameScreen />
      <HighlightOverlay />
    </UIContextProvider>
  );
}

export default App;
