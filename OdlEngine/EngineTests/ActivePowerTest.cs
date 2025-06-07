using ODLGameEngine;

namespace EngineTests
{
    [TestClass]
    public class ActivePowerTest
    {
        [TestMethod]
        public void VerifyCorrectPlayabilityState() // Verifies if power playable in incorrect phase
        {
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                List<States> ls = [.. Enum.GetValues<States>()];
                foreach (States st in ls)
                {
                    // Init game state
                    GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                    state.CurrentState = st;
                    state.CurrentPlayer = player;
                    GameStateMachine sm = new GameStateMachine();
                    sm.LoadGame(state); // Start from here
                    if (st != States.ACTION_PHASE) // Only check invalid states as valid state is used elsewhere during tests
                    {
                        Tuple<PlayContext, StepResult> res = sm.PlayActivePower();
                        Assert.AreEqual(res.Item1.PlayOutcome, PlayOutcome.INVALID_GAME_STATE);
                        Assert.AreEqual(res.Item2, null);
                    }
                }
            }
        }
        [TestMethod]
        public void VerifyNonAffordability() // Verifies if power fails due to lack of gold
        {
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                // Init game state
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                CardFinder cardDb = new CardFinder();
                // Card 1: Brick with cost 9
                cardDb.InjectCard(1, TestCardGenerator.CreateSkill(1, "BRICK", 9, PlayTargetLocation.BOARD));
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                sm.DetailedState.PlayerStates[(int)player].ActivePowerId = 1; // Use expensive brick as placeholder active effect
                Tuple<PlayContext, StepResult> res = sm.PlayActivePower();
                Assert.AreEqual(res.Item1.PlayOutcome, PlayOutcome.CANT_AFFORD);
                Assert.AreEqual(res.Item2, null);
            }
        }
        [TestMethod]
        public void VerifyNonRepeatability() // Verifies if power fails due to being previously used
        {
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                Player pl = new Player()
                {
                    PowerAvailable = false // Neither playe can use their power at this stage
                };
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = player,
                    PlayerStates = [pl, pl],
                };
                GameStateMachine sm = new GameStateMachine();
                sm.LoadGame(state); // Start from here
                Tuple<PlayContext, StepResult> res = sm.PlayActivePower();
                Assert.AreEqual(res.Item1.PlayOutcome, PlayOutcome.POWER_ALREADY_USED);
                Assert.AreEqual(res.Item2, null);
            }
        }
        [TestMethod]
        public void PowerRefreshesInNewTurn() // Verifies the power refreshes properly at BOT
        {
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                Player pl = new Player()
                {
                    PowerAvailable = false // Neither playe can use their power at this stage
                };
                pl.Deck.InitializeDeck("1,1,1"); // Add 3 cards just to avoid deck out
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = 1 - player,
                    PlayerStates = [pl, pl],
                };
                GameStateMachine sm = new GameStateMachine();
                sm.LoadGame(state); // Start from here
                sm.EndTurn(); // End opposing player's turn
                Assert.AreEqual(sm.DetailedState.PlayerStates[(int)player].PowerAvailable, false); // Ensure I couldn't use
                sm.Step();
                Assert.AreEqual(sm.DetailedState.PlayerStates[(int)player].PowerAvailable, true); // But now ensure I can
                sm.UndoPreviousStep();
                Assert.AreEqual(sm.DetailedState.PlayerStates[(int)player].PowerAvailable, false); // Ensure reverted properly
            }
        }
        [TestMethod]
        public void FlagHashTest() // Verifies the power flag properly changes hash
        {
            Player pl = new Player
            {
                PowerAvailable = true
            };
            int plHash = pl.GetHashCode();
            pl.PowerAvailable = false;
            Assert.AreNotEqual(plHash, pl.GetHashCode()); // Verify hash is now different
        }
        [TestMethod]
        public void ActivePowerCosts()
        {
            // Verifies if power charges properly and disables flag when played
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                CardFinder cardDb = new CardFinder();
                // Card 1: 5-cost brick
                cardDb.InjectCard(1, TestCardGenerator.CreateSkill(1, "BRICK", 5, PlayTargetLocation.BOARD));
                // Init game state
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.PlayerStates[0].CurrentGold = 10;
                state.PlayerStates[0].ActivePowerId = 1;
                state.PlayerStates[1].CurrentGold = 10;
                state.PlayerStates[1].ActivePowerId = 1;
                state.PlayerStates[0].Deck.InitializeDeck("1,1,1"); // Add 3 cards just to avoid deck out
                state.PlayerStates[1].Deck.InitializeDeck("1,1,1");
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Pre power assert
                int stateHash = sm.DetailedState.GetHashCode();
                Player currentPlayer = sm.DetailedState.PlayerStates[(int)sm.DetailedState.CurrentPlayer];
                Assert.AreEqual(currentPlayer.CurrentGold, 10);
                Assert.AreEqual(currentPlayer.PowerAvailable, true);
                // Now, power
                Tuple<PlayContext, StepResult> res = sm.PlayActivePower();
                Assert.AreEqual(res.Item1.PlayOutcome, PlayOutcome.OK);
                Assert.IsNotNull(res.Item2);
                currentPlayer = sm.DetailedState.PlayerStates[(int)sm.DetailedState.CurrentPlayer];
                Assert.AreNotEqual(stateHash, sm.DetailedState.GetHashCode());
                Assert.AreEqual(currentPlayer.CurrentGold, 5);
                Assert.AreEqual(currentPlayer.PowerAvailable, false);
                // Now, revert it
                sm.UndoPreviousStep();
                currentPlayer = sm.DetailedState.PlayerStates[(int)sm.DetailedState.CurrentPlayer];
                Assert.AreEqual(stateHash, sm.DetailedState.GetHashCode());
                Assert.AreEqual(currentPlayer.CurrentGold, 10);
                Assert.AreEqual(currentPlayer.PowerAvailable, true);
            }
        }
        [TestMethod]
        public void ActivePowerPlayedProperly()
        {
            // Verifies if power results in a full "played" event for the whole card effect resolution
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                CardFinder cardDb = new CardFinder();
                // Card 1: 5-cost brick
                Skill skill = TestCardGenerator.CreateSkill(1, "BRICK", 5, PlayTargetLocation.BOARD);
                Effect debugEffect = new Effect()
                {
                    EffectType = EffectType.STORE_DEBUG_IN_EVENT_PILE, // Triggers debug event and saves in list of results
                };
                skill.Interactions = new Dictionary<InteractionType, List<Effect>>();
                skill.Interactions.Add(InteractionType.WHEN_PLAYED, [debugEffect]); // Add interaction to card
                cardDb.InjectCard(1, skill);
                // Init game state
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.PlayerStates[0].CurrentGold = 10;
                state.PlayerStates[0].ActivePowerId = 1;
                state.PlayerStates[1].CurrentGold = 10;
                state.PlayerStates[1].ActivePowerId = 1;
                state.PlayerStates[0].Deck.InitializeDeck("1,1,1"); // Add 3 cards just to avoid deck out
                state.PlayerStates[1].Deck.InitializeDeck("1,1,1");
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Now, power
                Tuple<PlayContext, StepResult> res = sm.PlayActivePower();
                Assert.AreEqual(res.Item1.PlayOutcome, PlayOutcome.OK);
                Assert.IsNotNull(res.Item2);
                CpuState cpuState = TestHelperFunctions.FetchDebugEvent(res.Item2);
                Assert.IsNotNull(cpuState); // If not null, means effect resolved properly (effect related tests are in another file)
            }
        }
    }
}
