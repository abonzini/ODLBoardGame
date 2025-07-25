﻿namespace ODLGameEngine
{
    /// <summary>
    /// Occurs when game ends and someone loses the game
    /// </summary>
    public class EndOfGameException : Exception
    {
        public int PlayerWhoWon;
        public EndOfGameException(string message, int playerWhoWon) : base(message)
        {
            PlayerWhoWon = playerWhoWon;
        }
    }
    public partial class GameStateMachine
    {
        Random _rng;
        public GameStateStruct DetailedState = null; // State info, will work over this to advance game
        CardFinder _cardDb = null;
        public CardFinder CardDb
        {
            get
            {
                _cardDb ??= new CardFinder(".\\..\\..\\..\\..\\..\\CardResources\\CardData"); // Shouldn't happen unless testing!! Wonder if this path is always ok
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
        /// Initializes a game state, can define a pre-existing cardDb and rng seed if needed
        /// </summary>
        /// <param name="cardDb"></param>
        /// <param name="seed"></param>
        public GameStateMachine(CardFinder cardDb = null, int seed = 0)
        {
            if (seed == 0)
            {
                seed = (int)DateTime.Now.Ticks;
            }
            _cardDb = cardDb;
            STATE_InitInternal(new GameStateStruct(), seed);
        }
        /// <summary>
        /// Initializes internal stuff
        /// </summary>
        void STATE_InitInternal(GameStateStruct state, int seed)
        {
            DetailedState = state;
            DetailedState.Seed = seed;
            _rng = new Random(seed);
        }
        // --------------------------------------------------------------------------------------
        // ------------------------  STATE, ACTIONS AND STEP OPERATORS --------------------------
        // --------------------------------------------------------------------------------------

        /// <summary>
        /// Returns the next player that'd play (by default toggles between 1-2 but may get more complex)
        /// </summary>
        /// <returns>Next active player</returns>
        private CurrentPlayer STATE_GetNextPlayer()
        {
            return DetailedState.CurrentPlayer switch // Player is always 1 unless it goes from 1 -> 2
            {
                CurrentPlayer.PLAYER_1 => CurrentPlayer.PLAYER_2,
                _ => CurrentPlayer.PLAYER_1,
            };
        }
        /// <summary>
        /// Starts a game from loading a state. Only works in very beginning
        /// </summary>
        /// <param name="initialState">State to load</param>
        public void LoadGame(GameStateStruct initialState)
        {
            if (DetailedState.CurrentState != States.START) return; // Only works first thing
            STATE_InitInternal(initialState, initialState.Seed); // Initializes game to this point
            ENGINE_ChangeState(DetailedState.CurrentState); // Asks to enter new state, will create next step too (new)
        }
        /// <summary>
        /// Starts new game from scratch
        /// </summary>
        /// <param name="p1">Initial data for player 1</param>
        /// <param name="p2">Initial data for player 2</param>
        public void StartNewGame(PlayerInitialData p1, PlayerInitialData p2)
        {
            if (DetailedState.CurrentState != States.START) return; // Only works first thing
            // Loads player data and initialises
            STATE_LoadInitialPlayerData(0, p1);
            STATE_InitializePlayer(0);
            STATE_LoadInitialPlayerData(1, p2);
            STATE_InitializePlayer(1);
            // Set first player
            ENGINE_SetNextPlayer(STATE_GetNextPlayer()); // Make first player the first
            ENGINE_ChangeState(States.ACTION_PHASE); // Switches to first actual state
        }
        /// <summary>
        /// Ends turn of current player, will potentially call EOT effects, then switch to draw phase of next player (e.g. toggles player and transitions to DP)
        /// </summary>
        /// <returns>Actions occurring during EOT</returns>
        public StepResult EndTurn()
        {
            if (DetailedState.CurrentState != States.ACTION_PHASE) // Need to be in action phase!
            {
                return null;
            }
            try
            {
                STATE_ProcessEot();
                STATE_ProcessBot();
                ENGINE_ChangeState(States.ACTION_PHASE); // Next action phase of other player unless someone died
            }
            catch (EndOfGameException e)
            {
                STATE_TriggerEndOfGame(e.PlayerWhoWon);
            }
            return _stepHistory.Last(); // Returns everything that happened in this
        }
        /// <summary>
        /// Test API to activate a trigger to pretend it comes from any ingame event
        /// </summary>
        /// <param name="trigger">Trigger type</param>
        /// <param name="place">In which place to do this</param>
        /// <param name="specificContext">Specific context, to pretend it's a specific situation</param>
        /// <returns></returns>
        public StepResult TestActivateTrigger(TriggerType trigger, EffectLocation location, EffectContext specificContext)
        {
            try
            {
                EFFECT_ActivateTrigger(trigger, location, specificContext); // Does the trigger
                ENGINE_ChangeState(DetailedState.CurrentState); // Repeat current state to flush event queue
            }
            catch (EndOfGameException e)
            {
                STATE_TriggerEndOfGame(e.PlayerWhoWon);
            }
            return _stepHistory.Last(); // Returns everything that happened in this triggering
        }
        /// <summary>
        /// Closes event queue manually, to ensure the SM state is not broken after some manual fiddling
        /// </summary>
        /// <returns></returns>
        public StepResult CloseEventStack()
        {
            ENGINE_ChangeState(DetailedState.CurrentState); // Repeat current state to flush event queue
            return _stepHistory.Last(); // Returns everything that happened in this triggering
        }
        /// <summary>
        /// Test API to activate a trigger to pretend it comes from any ingame event, checks a literal place of the board
        /// </summary>
        /// <param name="trigger">What trigger</param>
        /// <param name="location">Board location (absolute, can reference a tile too)</param>
        /// <param name="specificContext">Specific context to send</param>
        /// <returns></returns>
        public StepResult TestActivateTrigger(TriggerType trigger, BoardElement location, EffectContext specificContext)
        {
            try
            {
                TRIGINTER_ProcessTrigger(trigger, location, specificContext); // Does the trigger
                ENGINE_ChangeState(DetailedState.CurrentState); // Repeat current state to flush event queue
            }
            catch (EndOfGameException e)
            {
                STATE_TriggerEndOfGame(e.PlayerWhoWon);
            }
            return _stepHistory.Last(); // Returns everything that happened in this triggering
        }
        /// <summary>
        /// Loads the initial player data including deck sizes name and class
        /// </summary>
        /// <param name="player">Which player</param>
        /// <param name="playerData">Container with initial data needed to start the game</param>
        void STATE_LoadInitialPlayerData(int player, PlayerInitialData playerData)
        {
            // Get all the player's card info
            Player playerInstance = (Player)CardDb.GetCard((int)playerData.PlayerClass);
            playerInstance = (Player)playerInstance.Clone();
            // Fill remaining
            playerInstance.Name = playerData.Name;
            playerInstance.Owner = player;
            playerInstance.UniqueId = player;
            playerInstance.Deck.InitializeDeck(playerData.InitialDecklist);

            DetailedState.PlayerStates[player] = playerInstance; // Make sure Players are init properly
            DetailedState.EntityData[player] = playerInstance;
        }
        /// <summary>
        /// Initializes player HP, gold, shuffles deck and draws cards. Needs to use correct RNG
        /// </summary>
        /// <param name="player">The player to init</param>
        void STATE_InitializePlayer(int player)
        {
            STATE_ShufflePlayerDeck(player);
            DrawContext drawContext = new DrawContext()
            {
                Actor = DetailedState.PlayerStates[player],
                DrawAmount = GameConstants.STARTING_CARDS
            };
            STATE_DeckDrawMultiple(drawContext);
            ENGINE_NewRngSeed(_rng.Next(int.MinValue, int.MaxValue));
        }
        /// <summary>
        /// Executes draw phase
        /// </summary>
        void STATE_ProcessBot()
        {
            int playerId = (int)DetailedState.CurrentPlayer;
            Player player = DetailedState.PlayerStates[playerId];
            // New turn, increment turn counter
            ENGINE_IncrementTurnCounter();
            // Advance all units of that player
            List<int> playerUnitsIds = DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, playerId).ToList(); // (Clone)
            if (playerUnitsIds.Count > 0) // Only advance if player has units
            {
                // Obtain all elements in list to iterate on, do it like this to allow iteration even if a unit dies during the march (iteration integrity)
                foreach (int unitId in playerUnitsIds)
                {
                    if (DetailedState.EntityData.TryGetValue(unitId, out LivingEntity unit)) // Check if unit is still alive, if not, no need to march
                    {
                        UNIT_UnitMarch((Unit)unit); // Then the unit marches on
                    }
                }
            }
            DrawContext drawContext = new DrawContext()
            {
                Actor = player,
                DrawAmount = GameConstants.DRAW_PHASE_CARDS_DRAWN
            };
            drawContext = STATE_DeckDrawMultiple(drawContext);
            if (drawContext.MissedDraws > 0) // If couldn't draw the required cards, means there's a deckout event
            {
                DamageContext deckoutDamageContext = new DamageContext()
                {
                    Actor = player,
                    Affected = player,
                    DamageAmount = GameConstants.DECKOUT_DAMAGE
                };
                LIVINGENTITY_DamageStep(deckoutDamageContext);
            }
            EFFECTS_ModifyPlayersGold(playerId, GameConstants.DRAW_PHASE_GOLD_OBTAINED, ModifierOperation.ADD);
            ENGINE_ChangePlayerPowerAvailability(player, true); // Player can now use active power again
        }
        /// <summary>
        /// Executes end-phase
        /// </summary>
        void STATE_ProcessEot()
        {
            // EOT effects
            LivingEntity currentPlayer = DetailedState.EntityData[(int)DetailedState.CurrentPlayer];
            EndOfTurnContext eotCtx = new EndOfTurnContext()
            {
                Actor = currentPlayer
            };
            TRIGINTER_ProcessTrigger(TriggerType.ON_END_OF_TURN, DetailedState.BoardState, eotCtx); // Trigger EOT event
            // Transition proper
            ENGINE_SetNextPlayer(STATE_GetNextPlayer()); // Swap player
        }
        /// <summary>
        /// Shuffles a player deck
        /// </summary>
        /// <param name="player">Player</param>
        void STATE_ShufflePlayerDeck(int player)
        {
            // Fisher Yates Algorithm for Shuffling, mix starting from last, first card isn't swapped with itself
            for (int i = DetailedState.PlayerStates[player].Deck.DeckSize - 1; i > 0; i--)
            {
                ENGINE_SwapCardsInDeck(player, i, _rng.Next(i + 1));
            }
        }
        /// <summary>
        /// Performs draw from deck operation
        /// </summary>
        /// <param name="drawCtx">Contains the player and number of cards to draw</param>
        /// <returns>The possibly modified draw context (same as input)</returns>
        DrawContext STATE_DeckDrawMultiple(DrawContext drawCtx)
        {
            Player player = (Player)drawCtx.Actor;
            int cardsToDraw = drawCtx.DrawAmount;
            int cardsDrawn = 0;
            for (int i = 0; i < cardsToDraw; i++)
            {
                if (player.Deck.DeckSize > 0)
                {
                    int cardDrawn;
                    if (_hypotheticalMode)
                    {
                        cardDrawn = 0; // Adds wildcard instead
                        ENGINE_HYPOTHETICAL_AlterDeckAmount(player, -1); // "Removes" one card from deck
                        ENGINE_HYPOTHETICAL_SetWildcardRelevance(player.Owner, true); // This new wildcard will be relevant at least at the beginning
                    }
                    else
                    {
                        cardDrawn = player.Deck.PeepAt(); // Found card in deck
                        ENGINE_DeckDrawSingle(player); // Removes from deck
                    }
                    // Nothing happens for now "when drawn"
                    ENGINE_AddCardToHand(player, cardDrawn); // Therefore adds to hand
                    cardsDrawn++;
                }
            }
            drawCtx.DrawAmount = cardsDrawn; // Calculate how much was actually drawn
            drawCtx.MissedDraws = cardsToDraw - cardsDrawn;
            return drawCtx;
        }
        /// <summary>
        /// Goes back to beggining of previous step (i.e. undoes the last thing that happened)
        /// </summary>
        public void UndoPreviousStep()
        {
            if (_currentStep == null || _currentStep.tag != Tag.NO_TAG) { return; } // Nothing to do here
            if (_currentStep.events.Count != 0) { throw new Exception("Standing in a non-empty current event!"); } // This should never happen

            _currentStep = _stepHistory.Last();
            _stepHistory.RemoveAt(_stepHistory.Count - 1); // Removes last step from history!
            for (int i = _currentStep.events.Count - 1; i >= 0; i--) // Pops events in reverse order, one by one
            {
                ENGINE_RevertEvent(_currentStep.events[i]); // Revert the event
            }
            _currentStep.events.Clear(); // Clear list as all events have been reverted
        }
        /// <summary>
        /// To be called by an action to signal the end of the game!
        /// </summary>
        /// <param name="playerWhoWon">Which player won?</param>
        private void STATE_TriggerEndOfGame(int playerWhoWon) // Gets stuck in EOG forever for now
        {
            if (_chainContext.Count > 0) // If EOG happened as part of an effect chain, this will not flush properly
            {
                _chainContext.Clear();
            }
            ENGINE_SetNextPlayer((CurrentPlayer)playerWhoWon); // The "current player" in this status is also the one who won the game
            ENGINE_ChangeState(States.EOG); // Switches to EOG and the game then gets stuck here
        }

        // --------------------------------------------------------------------------------------
        // ------------------------------------  MISC -------------------------------------------
        // --------------------------------------------------------------------------------------

        public override string ToString()
        {
            return $"{Enum.GetName(DetailedState.CurrentState)}";
        }
    }
}
