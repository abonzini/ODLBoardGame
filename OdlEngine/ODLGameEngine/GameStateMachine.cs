using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    public partial class GameStateMachine
    {
        Random _rng;

        GameStateStruct _detailedState = null; // State info, will work over this to advance game
        public GameStateStruct GetDetailedState() { return _detailedState; }
        CardFinder _cardDb = null;
        public CardFinder CardDb
        {
            get
            {
                _cardDb ??= new CardFinder(".\\..\\..\\..\\..\\CardDatabase"); // Shouldn't happen unless testing!! Wonder if this path is always ok
                return _cardDb;
            }
            set
            {
                _cardDb = value;
            }
        }
        readonly List<StepResult> _stepHistory = new List<StepResult>();
        StepResult _currentStep = null;

        /// <summary>
        /// Initializes an empty game state, and will create a random seed unless overwritten later
        /// </summary>
        public GameStateMachine()
        {
            InitInternal(new GameStateStruct(), (int)DateTime.Now.Ticks);
        }
        /// <summary>
        /// Initializes internal stuff
        /// </summary>
        void InitInternal(GameStateStruct state, int seed)
        {
            _detailedState = state;
            _detailedState.Seed = seed;
            _rng = new Random(seed);
        }

        // --------------------------------------------------------------------------------------
        // ------------------------  STATE, ACTIONS AND STEP OPERATORS --------------------------
        // --------------------------------------------------------------------------------------

        /// <summary>
        /// Performs a step of the state, moves the game state forward. Does nothing if machine is awaiting actions instead
        /// </summary>
        /// <returns>The new state action, null if nothing happened</returns>
        public StepResult Step()
        {
            switch(_detailedState.CurrentState)
            {
                case States.START:
                case States.ACTION_PHASE:
                    return null;
                case States.P1_INIT:
                    InitializePlayer(0);
                    ENGINE_ChangeState(States.P2_INIT);
                    break;
                case States.P2_INIT:
                    InitializePlayer(1);
                    ENGINE_TogglePlayer(); // Init finished, now begin game w P1 active
                    ENGINE_ChangeState(States.DRAW_PHASE);
                    break;
                case States.DRAW_PHASE:
                    DrawPhase();
                    ENGINE_ChangeState(States.ACTION_PHASE);
                    break;
                default:
                    throw new NotImplementedException("State not yet implemented");
            }
            return _stepHistory.Last();
        }
        /// <summary>
        /// Starts a game from loading a state. Only works in very beginning
        /// </summary>
        /// <param name="initialState">State to load</param>
        public void LoadGame(GameStateStruct initialState)
        {
            if (_detailedState.CurrentState != States.START) return; // Only works first thing
            InitInternal(initialState, initialState.Seed); // Initializes game to this point
            ENGINE_ChangeState(_detailedState.CurrentState); // Asks to enter new state, will create next step too (new)
        }
        /// <summary>
        /// Starts new game from scratch
        /// </summary>
        /// <param name="p1">Initial data for player 1</param>
        /// <param name="p2">Initial data for player 2</param>
        public void StartNewGame(PlayerInitialData p1, PlayerInitialData p2)
        {
            LoadInitialPlayerData(0, p1);
            LoadInitialPlayerData(1, p2);
            ENGINE_ChangeState(States.P1_INIT); // Switches to first actual state
        }
        /// <summary>
        /// Ends turn of current player, will potentially call EOT effects, then switch to draw phase of next player (e.g. toggles player and transitions to DP)
        /// </summary>
        /// <returns>Actions occurring during EOT</returns>
        public StepResult EndTurn()
        {
            // HERE BE EOT EFFECTS
            ENGINE_TogglePlayer(); // Swap player
            ENGINE_ChangeState(States.DRAW_PHASE); // Next is draw phase
            return _stepHistory.Last(); // Returns everything that happened in this
        }

        void LoadInitialPlayerData(int player, PlayerInitialData playerData) // This function randomizes! Needs to restore seed after!
        {
            _detailedState.PlayerStates[player].Name = playerData.Name;
            _detailedState.PlayerStates[player].PlayerClass = playerData.PlayerClass;
            _detailedState.PlayerStates[player].Deck.InitializeDeck(playerData.InitialDecklist);
        }

        /// <summary>
        /// Initializes player HP, gold, shuffles deck and draws cards. Needs to use correct RNG
        /// </summary>
        /// <param name="player">The player to init</param>
        void InitializePlayer(int player)
        {
            ENGINE_SetPlayerHp(player, GameConstants.STARTING_HP);
            ENGINE_SetPlayerGold(player, GameConstants.STARTING_GOLD);
            ShufflePlayerDeck(player);
            DeckDrawMultiple(player, GameConstants.STARTING_CARDS);
            ENGINE_NewRngSeed(_rng.Next(int.MinValue, int.MaxValue));
        }
        /// <summary>
        /// Executes draw phase
        /// </summary>
        void DrawPhase()
        {
            int playerId = (int)_detailedState.CurrentPlayer;
            // Advance all units of that player
            if (_detailedState.PlayerStates[playerId].NUnits > 0) // Only advance if player has units
            {
                SortedList<int, Unit> liveUnits = _detailedState.BoardState.Units;
                List<int> liveUnitsIds = liveUnits.Keys.ToList(); // Obtain all elements in list to iterate on
                foreach (int unitId in liveUnitsIds) // Obtain unit one by one in order of play, need to do it like this in case units are deleted in the meanwhile
                {
                    if(liveUnits.TryGetValue(unitId, out Unit unit)) // Check if unit is still alive, if not, no need to march
                    {
                        if(unit.Owner == playerId) // This unit needs to march
                        {
                            UNIT_AdvanceUnit(unit); // Then the unit advances!
                        }
                    }
                }
            }
            DeckDrawMultiple(playerId, GameConstants.DRAW_PHASE_CARDS_DRAWN); // Current player draws
            ENGINE_PlayerGoldChange(playerId, GameConstants.DRAW_PHASE_GOLD_OBTAINED); // Current player gets gold
        }
        /// <summary>
        /// Shuffles a player deck
        /// </summary>
        /// <param name="player">Player</param>
        void ShufflePlayerDeck(int player)
        {
            ENGINE_AddMessageEvent($"P{player + 1}'s deck shuffled");
            // Fisher Yates Algorithm for Shuffling, mix starting from last, first card isn't swapped with itself
            for (int i = _detailedState.PlayerStates[player].Deck.DeckSize - 1; i > 0; i--)
            {
                ENGINE_SwapCardsInDeck(player, i, _rng.Next(i+1));
            }
        }
        /// <summary>
        /// Player draws n cards
        /// </summary>
        /// <param name="player">Player</param>
        /// <param name="n">Cards to draw</param>
        void DeckDrawMultiple(int player, int n)
        {
            ENGINE_AddMessageEvent($"P{player + 1}'s draws {n}");
            for (int i  = 0; i < n; i++)
            {
                int card = _detailedState.PlayerStates[player].Deck.PeepAt(); // Found card in deck
                ENGINE_DeckDrawSingle(player); // Removes from deck
                // Nothing happens for now "when drawn"
                ENGINE_AddCardToHand(player, card); // Therefore adds to hand
            }
        }

        /// <summary>
        /// Goes back to beggining of previous step (i.e. undoes the last thing that happened)
        /// </summary>
        public void UndoPreviousStep()
        {
            if (_currentStep == null || _currentStep.tag == Tag.FIRST_STATE) { return; } // Nothing to do here
            if(_currentStep.events.Count != 0) { throw new Exception("Standing in a non-empty current event!"); } // This should never happen

            _currentStep = _stepHistory.Last();
            _stepHistory.RemoveAt(_stepHistory.Count - 1); // Removes last step from history!
            for(int i = _currentStep.events.Count - 1; i >= 0; i--) // Pops events in reverse order, one by one
            {
                ENGINE_RevertEvent(_currentStep.events[i]); // Revert the event
            }
            _currentStep.events.Clear(); // Clear list as all events have been reverted
        }

        // --------------------------------------------------------------------------------------
        // ------------------------------------  MISC -------------------------------------------
        // --------------------------------------------------------------------------------------

        public override string ToString()
        {
            return $"{Enum.GetName(_detailedState.CurrentState)}";
        }
    }
}
