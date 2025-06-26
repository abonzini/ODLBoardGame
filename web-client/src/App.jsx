import './App.css';
import { UIContextProvider, useUIContext } from './context/UIContext';
import GameScreen from './components/GameScreen';
import HighlightOverlay from './components/HighlightOverlay';
import { useEffect } from 'react';

function GlobalKeyHandler() {
  const { isOverlayActive, pushOverlay } = useUIContext();

  useEffect(() => {
    const handleKeyDown = (e) => {
      if (e.key === 'Escape' && !isOverlayActive) {
        // Activate overlay with no highlighted component when ESC is pressed and overlay is not active
        pushOverlay({ type: 'none' });
      }
    };

    document.addEventListener('keydown', handleKeyDown);
    return () => {
      document.removeEventListener('keydown', handleKeyDown);
    };
  }, [isOverlayActive, pushOverlay]);

  return null;
}

function App() {
  return (
    <UIContextProvider>
      <GlobalKeyHandler />
      <GameScreen />
      <HighlightOverlay />
    </UIContextProvider>
  );
}

export default App;
