using ODLGameEngine;

namespace EngineTests
{
    public static class TestHelperFunctions
    {
        /// <summary>
        /// Gets a blank gamestate, with 2 vanilla base players, everythign instantiated in the INITP1 player state
        /// </summary>
        /// <returns></returns>
        static public GameStateStruct GetBlankGameState()
        {
            CardFinder cardFinder = new CardFinder();
            InjectBasePlayerToDb(cardFinder);
            GameStateMachine sm = new GameStateMachine(cardFinder);
            PlayerInitialData playerInit1 = new PlayerInitialData()
            {
                Name = "TestPlayer1",
                PlayerClass = PlayerTribe.BASE,
            };
            PlayerInitialData playerInit2 = new PlayerInitialData()
            {
                Name = "TestPlayer2",
                PlayerClass = PlayerTribe.BASE,
            };
            sm.StartNewGame(playerInit1, playerInit2);
            return sm.DetailedState;
        }
        /// <summary>
        /// With the new Player=Card thing, I need to ensure every test now injects the card of base class here
        /// </summary>
        /// <param name="cardFinder">Db to inject to</param>
        public static void InjectBasePlayerToDb(CardFinder cardFinder)
        {
            Player player = new Player()
            {
                Id = -1,
                EntityType = EntityType.PLAYER,
                Name = "PlayerName",
                Hp = new Min0Stat() { BaseValue = GameConstants.STARTING_HP },
                CurrentGold = GameConstants.STARTING_GOLD,
                ActivePowerId = GameConstants.DEFAULT_ACTIVE_POWER_ID
            };
            cardFinder.InjectCard(-1, player);
        }
        /// <summary>
        /// Checks if player state has is already present or not in a set
        /// </summary>
        /// <param name="st">State of player</param>
        /// <param name="set">Hash set</param>
        /// <param name="shouldBe">Should it be present?</param>
        public static void HashSetVerification(object st, HashSet<int> set, bool shouldBe)
        {
            Assert.IsTrue(shouldBe == set.Contains(st.GetHashCode()));
            if (!set.Contains(st.GetHashCode()))
            {
                set.Add(st.GetHashCode());
            }
        }
        /// <summary>
        /// Checks if deck 1-30 is properly shuffled, has a 2.6525286e+32 chance of messing up because you may get a perfect shuffle
        /// </summary>
        /// <returns>If deck's shuffled</returns>
        public static bool IsDeckShuffled(Player p)
        {
            for (int i = 0; i < p.Deck.DeckSize; i++)
            {
                if (p.Deck.PeepAt(i) != i + 1)
                {
                    return true; // A single difference is all it takes
                }
            }
            return false;
        }
        /// <summary>
        /// Checks whether a player left init stage perfectly
        /// </summary>
        /// <param name="p">Player</param>
        /// <returns>True if properly init</returns>
        public static void VerifyPlayerInitialised(Player p)
        {
            Assert.AreEqual(p.Hp.Total, GameConstants.STARTING_HP);
            Assert.AreEqual(p.CurrentGold, GameConstants.STARTING_GOLD);
            Assert.AreEqual(p.Hand.CardCount, GameConstants.STARTING_CARDS);
            Assert.AreEqual(p.Deck.DeckSize, GameConstants.DECK_SIZE - GameConstants.STARTING_CARDS);
        }
        /// <summary>
        /// Verifies from a state machine in draw phase, that draw phase succeeds properly. Leaves the state machine as it began
        /// </summary>
        /// <param name="sm">State machine to try</param>
        public static void VerifyDrawPhaseResult(GameStateMachine sm)
        {
            HashSet<int> hashes = new HashSet<int>(); // Checks all hashes resulting
            GameStateStruct testState = sm.DetailedState;
            int firstPlayer = (int)testState.CurrentPlayer;
            int playerToVerify = 1 - firstPlayer;
            Assert.AreEqual(testState.CurrentState, States.ACTION_PHASE); // Am I in action phase
            int preCards = testState.PlayerStates[playerToVerify].Hand.CardCount;
            int preGold = testState.PlayerStates[playerToVerify].CurrentGold;
            int preDeck = testState.PlayerStates[playerToVerify].Deck.DeckSize;
            int turnCounter = testState.TurnCounter;
            // Player hashes init for first time, also hands and decks, also state!
            HashSetVerification(testState.PlayerStates[0], hashes, false);
            HashSetVerification(testState.PlayerStates[1], hashes, false);
            HashSetVerification(testState.PlayerStates[0].Hand, hashes, false);
            HashSetVerification(testState.PlayerStates[1].Hand, hashes, false);
            HashSetVerification(testState.PlayerStates[0].Deck, hashes, false);
            HashSetVerification(testState.PlayerStates[1].Deck, hashes, false);
            HashSetVerification(testState, hashes, false);
            // Now draw (end of turn leads to draw phase)
            sm.EndTurn();
            testState = sm.DetailedState;
            int postCards = testState.PlayerStates[playerToVerify].Hand.CardCount;
            int postGold = testState.PlayerStates[playerToVerify].CurrentGold;
            int postDeck = testState.PlayerStates[playerToVerify].Deck.DeckSize;
            Assert.AreEqual(testState.CurrentState, States.ACTION_PHASE); // Am I in next phase
            Assert.AreEqual(postCards - preCards, GameConstants.DRAW_PHASE_CARDS_DRAWN); // Did player draw exact amount of cards
            Assert.AreEqual(postGold - preGold, GameConstants.DRAW_PHASE_GOLD_OBTAINED); // Did player gain exact amount of gold
            Assert.AreEqual(postDeck - preDeck, -GameConstants.DRAW_PHASE_CARDS_DRAWN); // Did player deck shrink the exact amount
            Assert.AreEqual(turnCounter + 1, testState.TurnCounter);
            // Only one player should've changed, the current one, so that one should have brand new hashes, state is obviosuly always new
            HashSetVerification(testState.PlayerStates[0], hashes, testState.CurrentPlayer != CurrentPlayer.PLAYER_1);
            HashSetVerification(testState.PlayerStates[1], hashes, testState.CurrentPlayer != CurrentPlayer.PLAYER_2);
            HashSetVerification(testState.PlayerStates[0].Hand, hashes, testState.CurrentPlayer != CurrentPlayer.PLAYER_1);
            HashSetVerification(testState.PlayerStates[1].Hand, hashes, testState.CurrentPlayer != CurrentPlayer.PLAYER_2);
            HashSetVerification(testState.PlayerStates[0].Deck, hashes, testState.CurrentPlayer != CurrentPlayer.PLAYER_1);
            HashSetVerification(testState.PlayerStates[1].Deck, hashes, testState.CurrentPlayer != CurrentPlayer.PLAYER_2);
            HashSetVerification(testState, hashes, false);
            // Now revert
            sm.UndoPreviousStep(); // Go back to beginning of drawphase
            testState = sm.DetailedState;
            preCards = testState.PlayerStates[playerToVerify].Hand.CardCount;
            preGold = testState.PlayerStates[playerToVerify].CurrentGold;
            preDeck = testState.PlayerStates[playerToVerify].Deck.DeckSize;
            Assert.AreEqual(testState.CurrentState, States.ACTION_PHASE);
            Assert.AreEqual(postCards - preCards, GameConstants.DRAW_PHASE_CARDS_DRAWN); // Did player restore cards
            Assert.AreEqual(postGold - preGold, GameConstants.DRAW_PHASE_GOLD_OBTAINED); // Did player restore gold
            Assert.AreEqual(postDeck - preDeck, -GameConstants.DRAW_PHASE_CARDS_DRAWN); // Did player deck recover the card
            Assert.AreEqual(turnCounter, testState.TurnCounter);
            // All hashes should be present still
            HashSetVerification(testState.PlayerStates[0], hashes, true);
            HashSetVerification(testState.PlayerStates[1], hashes, true);
            HashSetVerification(testState.PlayerStates[0].Hand, hashes, true);
            HashSetVerification(testState.PlayerStates[1].Hand, hashes, true);
            HashSetVerification(testState.PlayerStates[0].Deck, hashes, true);
            HashSetVerification(testState.PlayerStates[1].Deck, hashes, true);
            HashSetVerification(testState, hashes, true);
        }
        /// <summary>
        /// Inits and adds an entity into a game state, no checking or summoning, NO CLONING, just init, regoster and adding into BLT
        /// </summary>
        /// <param name="state"></param>
        /// <param name="tileCoord"></param>
        /// <param name="uniqueId"></param>
        /// <param name="owner"></param>
        /// <param name="entity"></param>
        static public void ManualInitEntity(GameStateStruct state, int tileCoord, int uniqueId, int owner, PlacedEntity entity)
        {
            entity.Owner = owner;
            entity.UniqueId = uniqueId;
            // Add to board and sm
            state.EntityData.Add(uniqueId, entity);
            state.BoardState.EntityListOperation(entity, BoardElementListOperation.ADD);
            // Add to lane
            Lane laneToAddTo = state.BoardState.GetLaneContainingTile(tileCoord);
            laneToAddTo.EntityListOperation(entity, BoardElementListOperation.ADD);
            // Add to tile
            entity.TileCoordinate = tileCoord;
            Tile tile = state.BoardState.Tiles[tileCoord];
            tile.EntityListOperation(entity, BoardElementListOperation.ADD);
        }
        /// <summary>
        /// Given a SM list of events, fetches the first DebugEvent found and returns the Event CPU state for that
        /// </summary>
        /// <param name="stepResult">Step result</param>
        /// <returns>Cpu state if debug found, or null if no debug event present</returns>
        static public CpuState FetchDebugEvent(StepResult stepResult)
        {
            CpuState cpuState = null;
            foreach (GameEngineEvent ev in stepResult.events)
            {
                if (ev.eventType == EventType.DEBUG_EVENT)
                {
                    cpuState = ((EntityEvent<CpuState>)ev).entity;
                    break;
                }
            }
            return cpuState;
        }
        /// <summary>
        /// Given a SM list of events, returns all of them found
        /// </summary>
        /// <param name="stepResult">Step result</param>
        /// <returns>How many debug events were found</returns>
        static public List<CpuState> FetchDebugEvents(StepResult stepResult)
        {
            List<CpuState> events = new List<CpuState>();
            foreach (GameEngineEvent ev in stepResult.events)
            {
                if (ev.eventType == EventType.DEBUG_EVENT)
                {
                    events.Add(((EntityEvent<CpuState>)ev).entity);
                }
            }
            return events;
        }
        /// <summary>
        /// Returns a random number from options
        /// </summary>
        /// <returns>One of these 3</returns>
        public static T GetRandomChoice<T>(List<T> collection)
        {
            Random _rng = new Random();
            int choice = _rng.Next(collection.Count);
            return collection[choice];
        }
    }
}
