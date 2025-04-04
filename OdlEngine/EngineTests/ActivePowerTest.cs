using ODLGameEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    GameStateStruct state = new GameStateStruct
                    {
                        CurrentState = st,
                        CurrentPlayer = player
                    };
                    GameStateMachine sm = new GameStateMachine();
                    sm.LoadGame(state); // Start from here
                    if (st != States.ACTION_PHASE) // Only check invalid states as valid state is used elsewhere during tests
                    {
                        Tuple<PlayOutcome, StepResult> res = sm.PlayActivePower();
                        Assert.AreEqual(res.Item1, PlayOutcome.INVALID_GAME_STATE);
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
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = player
                };
                CardFinder cardDb = new CardFinder();
                // Card 1: Brick with cost 9
                cardDb.InjectCard(1, TestCardGenerator.CreateSkill(1, "BRICK", 9, CardTargets.BOARD));
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                sm.DetailedState.PlayerStates[(int)player].ActivePowerCast = 1; // Use expensive brick as placeholder active effect
                Tuple<PlayOutcome, StepResult> res = sm.PlayActivePower();
                Assert.AreEqual(res.Item1, PlayOutcome.CANT_AFFORD);
                Assert.AreEqual(res.Item2, null);
            }
        }
        [TestMethod]
        public void VerifyNonRepeatability() // Verifies if power fails due to being previously used
        {
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                PlayerState pl = new PlayerState()
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
                Tuple<PlayOutcome, StepResult> res = sm.PlayActivePower();
                Assert.AreEqual(res.Item1, PlayOutcome.POWER_ALREADY_USED);
                Assert.AreEqual(res.Item2, null);
            }
        }
        [TestMethod]
        public void PowerRefreshesInNewTurn() // Verifies the power refreshes properly at BOT
        {
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                PlayerState pl = new PlayerState()
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
                sm.EndTurn(); // End opposingplayer's turn
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
            PlayerState pl = new PlayerState
            {
                PowerAvailable = true
            };
            int plHash = pl.GetGameStateHash();
            pl.PowerAvailable = false;
            Assert.AreNotEqual(plHash, pl.GetGameStateHash()); // Verify hash is now different
        }
        [TestMethod]
        public void ActivePowerCast()
        {
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                CardFinder cardDb = new CardFinder();
                // Card 1: 5-cost brick
                cardDb.InjectCard(1, TestCardGenerator.CreateSkill(1, "BRICK", 5, CardTargets.BOARD));
                PlayerState pl1 = new PlayerState()
                {
                    Gold = 10, // Let them afford the power
                    ActivePowerCast = 1 // Cost 5 brick
                }; PlayerState pl2 = new PlayerState()
                {
                    Gold = 10, // Let them afford the power
                    ActivePowerCast = 1 // Cost 5 brick
                };
                pl1.Deck.InitializeDeck("1,1,1"); // Add 3 cards just to avoid deck out
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = player,
                    PlayerStates = [pl1, pl2],
                };
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Pre rush assert
                int stateHash = sm.DetailedState.GetGameStateHash();
                PlayerState currentPlayer = sm.DetailedState.PlayerStates[(int)sm.DetailedState.CurrentPlayer];
                Assert.AreEqual(currentPlayer.Gold, 10);
                Assert.AreEqual(currentPlayer.PowerAvailable, true);
                // Now, rush
                Tuple<PlayOutcome, StepResult> res = sm.PlayActivePower();
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsNotNull(res.Item2);
                currentPlayer = sm.DetailedState.PlayerStates[(int)sm.DetailedState.CurrentPlayer];
                Assert.AreNotEqual(stateHash, sm.DetailedState.GetGameStateHash());
                Assert.AreEqual(currentPlayer.Gold, 5);
                Assert.AreEqual(currentPlayer.PowerAvailable, false);
                // Now, revert it
                sm.UndoPreviousStep();
                currentPlayer = sm.DetailedState.PlayerStates[(int)sm.DetailedState.CurrentPlayer];
                Assert.AreEqual(stateHash, sm.DetailedState.GetGameStateHash());
                Assert.AreEqual(currentPlayer.Gold, 10);
                Assert.AreEqual(currentPlayer.PowerAvailable, true);
            }
        }
    }
}
