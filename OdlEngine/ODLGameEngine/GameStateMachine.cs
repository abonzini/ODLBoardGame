using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    public class GameStateMachine
    {
        Random _rng;

        GameStateStruct _detailedState = null; // State info, will work over this to advance game
        public GameStateStruct GetDetailedState() { return _detailedState; }
        CardFinder _cardDb = null;
        CardFinder CardDb
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
            _detailedState = new GameStateStruct();
            int seed = (int)DateTime.Now.Ticks;
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
                    InitializePlayer(PlayerId.PLAYER_1);
                    RequestNewState(States.P2_INIT);
                    break;
                case States.P2_INIT:
                    InitializePlayer(PlayerId.PLAYER_2);
                    TogglePlayer(); // Init finished, now begin game w P1 active
                    RequestNewState(States.DRAW_PHASE);
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
            _detailedState = initialState; // Overrides state to whatever I wanted
            RequestNewState(_detailedState.CurrentState); // Asks to enter new state, will create next step too (new)
        }
        /// <summary>
        /// Starts new game from scratch
        /// </summary>
        /// <param name="p1">Initial data for player 1</param>
        /// <param name="p2">Initial data for player 2</param>
        public void StartNewGame(PlayerInitialData p1, PlayerInitialData p2)
        {
            LoadInitialPlayerData(PlayerId.PLAYER_1, p1);
            LoadInitialPlayerData(PlayerId.PLAYER_2, p2);
            RequestNewState(States.P1_INIT); // Switches to first actual state
        }

        void LoadInitialPlayerData(PlayerId player, PlayerInitialData playerData) // This function randomizes! Needs to restore seed after!
        {
            int playerId = GetPlayerIndexFromId(player);
            _detailedState.PlayerStates[playerId].Name = playerData.Name;
            _detailedState.PlayerStates[playerId].PlayerClass = playerData.PlayerClass;
            _detailedState.PlayerStates[playerId].Deck.InitializeDeck(playerData.InitialDecklist);
        }

        static int GetPlayerIndexFromId(PlayerId player)
        {
            return player switch
            {
                PlayerId.PLAYER_1 => 0,
                PlayerId.PLAYER_2 => 1,
                _ => throw new InvalidOperationException("Can only be used when intiializing player!"),
            };
        }

        /// <summary>
        /// Initializes player HP, gold, shuffles deck and draws cards. Needs to use correct RNG
        /// </summary>
        /// <param name="player">The player to init</param>
        void InitializePlayer(PlayerId player)
        {
            SetPlayerHp(player, GameConstants.STARTING_HP);
            SetPlayerGold(player, GameConstants.STARTING_GOLD);
            ShufflePlayerDeck(player);
            DeckDrawMultiple(player, GameConstants.STARTING_CARDS);
            NewRngSeed(_rng.Next(int.MinValue, int.MaxValue));
        }
        /// <summary>
        /// Shuffles a player deck
        /// </summary>
        /// <param name="player">Player</param>
        void ShufflePlayerDeck(PlayerId player)
        {
            AddMessageEvent($"P{GetPlayerIndexFromId(player) + 1}'s deck shuffled");
            int playerId = GetPlayerIndexFromId(player);
            // Fisher Yates Algorithm for Shuffling, mix starting from last, first card isn't swapped with itself
            for (int i = _detailedState.PlayerStates[playerId].Deck.Cards.Count - 1; i > 0; i--)
            {
                SwapCardsInDeck(player, i, _rng.Next(i+1));
            }
        }
        /// <summary>
        /// Player draws n cards
        /// </summary>
        /// <param name="player">Player</param>
        /// <param name="n">Cards to draw</param>
        void DeckDrawMultiple(PlayerId player, int n)
        {
            AddMessageEvent($"P{GetPlayerIndexFromId(player) + 1}'s draws {n}");
            for (int i  = 0; i < n; i++)
            {
                DeckDrawSingle(player);
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
                RevertEvent(_currentStep.events[i]); // Revert the event
            }
            _currentStep.events.Clear(); // Clear list as all events have been reverted
        }
        /// <summary>
        /// Executes an event to change game state, adds to current queue and moves state
        /// </summary>
        /// <param name="e">The event to add and excecute</param>
        void ExecuteEvent(Event e)
        {
            int auxPlayerId;
            _currentStep?.events.Add(e);
            switch (e.eventType)
            {
                case EventType.STATE_TRANSITION:
                    // Requested a transition to new state, which implies ending this step and creating a new one
                    bool firstStep = false;
                    if(_currentStep == null)
                    {
                        firstStep = true;
                    }
                    else
                    {
                        ((TransitionEvent<States>)e).oldValue = _detailedState.CurrentState;
                        _stepHistory.Add(_currentStep);
                    }
                    _detailedState.CurrentState = ((TransitionEvent<States>)e).newValue;
                    _currentStep = new StepResult();
                    if(firstStep) _currentStep.tag = Tag.FIRST_STATE; // Tag it as first if needed (step can't be reverted)
                    // State transition complete!
                    break;
                case EventType.PLAYER_TRANSITION:
                    // Requested a transition to new state, which implies ending this step and creating a new one
                    ((TransitionEvent<PlayerId>)e).oldValue = _detailedState.CurrentPlayer;
                    _detailedState.CurrentPlayer = ((TransitionEvent<PlayerId>)e).newValue; // Player transition complete!
                    break;
                case EventType.RNG_TRANSITION:
                    // Requested a transition to new rng seed
                    ((TransitionEvent<int>)e).oldValue = _detailedState.Seed;
                    _detailedState.Seed = ((TransitionEvent<int>)e).newValue; // Player transition complete!
                    _rng = new Random(_detailedState.Seed);
                    break;
                case EventType.PLAYER_HP_TRANSITION:
                    auxPlayerId = GetPlayerIndexFromId(((PlayerValueEvent<int>)e).playerId);
                    ((PlayerValueEvent<int>)e).oldValue = _detailedState.PlayerStates[auxPlayerId].Hp;
                    _detailedState.PlayerStates[auxPlayerId].Hp = ((PlayerValueEvent<int>)e).newValue;
                    break;
                case EventType.PLAYER_GOLD_TRANSITION:
                    auxPlayerId = GetPlayerIndexFromId(((PlayerValueEvent<int>)e).playerId);
                    ((PlayerValueEvent<int>)e).oldValue = _detailedState.PlayerStates[auxPlayerId].Gold;
                    _detailedState.PlayerStates[auxPlayerId].Gold = ((PlayerValueEvent<int>)e).newValue;
                    break;
                case EventType.MESSAGE:
                    break;
                case EventType.CARD_DECK_SWAP:
                    auxPlayerId = GetPlayerIndexFromId(((PlayerValueEvent<int>)e).playerId);
                    _detailedState.PlayerStates[auxPlayerId].Deck.SwapCards(
                        ((PlayerValueEvent<int>)e).newValue,
                        ((PlayerValueEvent<int>)e).oldValue
                        );
                    break;
                case EventType.DECK_DRAW:
                    auxPlayerId = GetPlayerIndexFromId(((PlayerEvent)e).playerId);
                    _detailedState.PlayerStates[auxPlayerId].Hand.InsertCard(
                        _detailedState.PlayerStates[auxPlayerId].Deck.PopCard(),
                        _detailedState.PlayerStates[auxPlayerId].Hand.CardsInHand.Count
                        ); // Pop last card from deck and add to hand last
                    break;
                default:
                    throw new NotImplementedException("Not a handled state rn");
            }
        }
        /// <summary>
        /// Performs the opposite action of an event. Doesn't remove from step! Just opposite
        /// </summary>
        /// <param name="e">Event to revert</param>
        void RevertEvent(Event e)
        {
            int auxPlayerId;
            switch (e.eventType)
            {
                case EventType.STATE_TRANSITION:
                    // Requested a transition to new state, which implies ending this step and creating a new one
                    _detailedState.CurrentState = ((TransitionEvent<States>)e).oldValue; // Just retrieves the prev state
                    break;
                case EventType.PLAYER_TRANSITION:
                    // Requested a transition to new state, which implies ending this step and creating a new one
                    _detailedState.CurrentPlayer = ((TransitionEvent<PlayerId>)e).oldValue; // Player transition complete!
                    break;
                case EventType.RNG_TRANSITION:
                    // Requested a transition to new rng seed
                    _detailedState.Seed = ((TransitionEvent<int>)e).oldValue; // Player transition complete!
                    _rng = new Random(_detailedState.Seed);
                    break;
                case EventType.PLAYER_HP_TRANSITION:
                    auxPlayerId = GetPlayerIndexFromId(((PlayerValueEvent<int>)e).playerId);
                    _detailedState.PlayerStates[auxPlayerId].Hp = ((PlayerValueEvent<int>)e).oldValue;
                    break;
                case EventType.PLAYER_GOLD_TRANSITION:
                    auxPlayerId = GetPlayerIndexFromId(((PlayerValueEvent<int>)e).playerId);
                    _detailedState.PlayerStates[auxPlayerId].Gold = ((PlayerValueEvent<int>)e).oldValue;
                    break;
                case EventType.MESSAGE:
                    break;
                case EventType.CARD_DECK_SWAP:
                    auxPlayerId = GetPlayerIndexFromId(((PlayerValueEvent<int>)e).playerId);
                    _detailedState.PlayerStates[auxPlayerId].Deck.SwapCards(
                        ((PlayerValueEvent<int>)e).newValue,
                        ((PlayerValueEvent<int>)e).oldValue
                        );
                    break;
                case EventType.DECK_DRAW:
                    auxPlayerId = GetPlayerIndexFromId(((PlayerEvent)e).playerId);
                    _detailedState.PlayerStates[auxPlayerId].Deck.InsertCard(
                        _detailedState.PlayerStates[auxPlayerId].Hand.RemoveCardAt(
                            _detailedState.PlayerStates[auxPlayerId].Hand.CardsInHand.Count-1),
                        _detailedState.PlayerStates[auxPlayerId].Deck.Cards.Count);
                    // Pop card from last place of hand and return to deck
                    break;
                default:
                    throw new NotImplementedException("Not a handled state rn");
            }
        }

        // --------------------------------------------------------------------------------------
        // -------------------------------  GAME ENGINE REQUESTS --------------------------------
        // --------------------------------------------------------------------------------------
        /// <summary>
        /// Advances state machine
        /// </summary>
        /// <param name="state">State to go to</param>
        void RequestNewState(States state)
        {
            ExecuteEvent(
                new TransitionEvent<States>()
                {
                    eventType = EventType.STATE_TRANSITION,
                    newValue = state,
                    description = $"Next state: {Enum.GetName(state)}"
                });
        }
        /// <summary>
        /// Toggles active player
        /// </summary>
        void TogglePlayer()
        {
            var nextPlayer = _detailedState.CurrentPlayer switch // Player is always 1 unless it goes from 1 -> 2
            {
                PlayerId.PLAYER_1 => PlayerId.PLAYER_2,
                _ => PlayerId.PLAYER_1,
            };
            ExecuteEvent(
                new TransitionEvent<PlayerId>()
                {
                    eventType = EventType.PLAYER_TRANSITION,
                    newValue = nextPlayer,
                    description = $"Switched to {Enum.GetName(nextPlayer)}"
                });
        }
        /// <summary>
        /// Next step will have a new rng seed, important to mantain determinism
        /// </summary>
        /// <param name="seed">Seed to adopt</param>
        void NewRngSeed(int seed)
        {
            ExecuteEvent(
                new TransitionEvent<int>()
                {
                    eventType = EventType.RNG_TRANSITION,
                    newValue = seed
                });
        }
        /// <summary>
        /// Sets a player HP to new value
        /// </summary>
        /// <param name="p">Which player</param>
        /// <param name="hp">Which value</param>
        void SetPlayerHp(PlayerId p, int hp)
        {
            ExecuteEvent(
                new PlayerValueEvent<int>()
                {
                    eventType = EventType.PLAYER_HP_TRANSITION,
                    playerId = p,
                    newValue = hp,
                    description = $"P{GetPlayerIndexFromId(p)+1} now has {hp} HP"
                });
        }
        /// <summary>
        /// Sets a player gold to new value
        /// </summary>
        /// <param name="p">Which player</param>
        /// <param name="gold">Which value</param>
        void SetPlayerGold(PlayerId p, int gold)
        {
            ExecuteEvent(
                new PlayerValueEvent<int>()
                {
                    eventType = EventType.PLAYER_GOLD_TRANSITION,
                    playerId = p,
                    newValue = gold,
                    description = $"P{GetPlayerIndexFromId(p) + 1} now has {gold} gold"
                });
        }
        void AddMessageEvent(string msg)
        {
            ExecuteEvent(
                new Event()
                {
                    eventType = EventType.MESSAGE,
                    description = msg
                });
        }
        void SwapCardsInDeck(PlayerId p, int card1, int card2)
        {
            ExecuteEvent(
                new PlayerValueEvent<int>()
                {
                    eventType = EventType.CARD_DECK_SWAP,
                    playerId = p,
                    newValue = card1,
                    oldValue = card2
                });
        }
        void DeckDrawSingle(PlayerId p)
        {
            ExecuteEvent(
                new PlayerEvent()
                {
                    eventType = EventType.DECK_DRAW,
                    playerId = p
                });
        }
        public override string ToString()
        {
            return $"{Enum.GetName(_detailedState.CurrentState)}";
        }
    }
}
