using ODLGameEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EngineTests
{
    [TestClass]
    public class PlayabilityAndTargetingTests
    {
        [TestMethod]
        public void VerifyAffordability() // Generate costs 0-9, half can be afforable, half can't
        {
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = player
                };
                for (int i = 0; i < 10; i++)
                {
                    state.PlayerStates[playerIndex].Hand.InsertCard(-(100+i*10)); // Insert test cards (brick) in hand costs 0-9
                }
                state.PlayerStates[playerIndex].Gold = 4; // Set gold to 4
                GameStateMachine sm = new GameStateMachine
                {
                    CardDb = TestCardGenerator.GenerateTestCardGenerator() // Add test cardDb
                };
                sm.LoadGame(state); // Start from here
                for(int i = 0; i < sm.GetDetailedState().PlayerStates[playerIndex].Hand.HandSize; i++) // Check for each card
                {
                    Tuple<PlayOutcome, CardTargets> res = sm.GetPlayableOptions(-(100 + i * 10));
                    if(i <= 4)
                    {
                        Assert.AreEqual(res.Item1, PlayOutcome.OK); // Could be played
                    }
                    else
                    {
                        Assert.AreEqual(res.Item1, PlayOutcome.CANT_AFFORD); // Could not
                    }
                }
            }
        }
        [TestMethod]
        public void VerifyTargetability() // Generate targets 0-9, verify targetability is same
        {
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = player
                };
                for (int i = 0; i < 10; i++)
                {
                    state.PlayerStates[playerIndex].Hand.InsertCard(-(100 + i)); // Insert test cards (brick) in hand all targets
                }
                GameStateMachine sm = new GameStateMachine
                {
                    CardDb = TestCardGenerator.GenerateTestCardGenerator() // Add test cardDb
                };
                sm.LoadGame(state); // Start from here
                for (int i = 0; i < sm.GetDetailedState().PlayerStates[playerIndex].Hand.HandSize; i++) // Check for each card
                {
                    Tuple<PlayOutcome, CardTargets> res = sm.GetPlayableOptions(-(100 + i)); // Ok in all cases with valid target
                    if (i <= 7)
                    {
                        Assert.AreEqual(res.Item1, PlayOutcome.OK); // OK
                        Assert.AreEqual(res.Item2, (CardTargets)i); // All them valid targets
                    }
                    else
                    {
                        Assert.AreEqual(res.Item1, PlayOutcome.NO_TARGET_AVAILABLE); // Would be an error!
                        Assert.AreEqual(res.Item2, CardTargets.INVALID); // Bc invalid...
                    }
                }
            }
        }
        [TestMethod]
        public void VerifyCorrectPlayabilityState() // Verifies if card playable in incorrect phase
        {
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                List<States> ls = [.. Enum.GetValues<States>()];
                foreach(States st in ls)
                {
                    GameStateStruct state = new GameStateStruct
                    {
                        CurrentState = st,
                        CurrentPlayer = player
                    };
                    state.PlayerStates[playerIndex].Hand.InsertCard(-100); // Insert only one card, I don't care
                    GameStateMachine sm = new GameStateMachine
                    {
                        CardDb = TestCardGenerator.GenerateTestCardGenerator() // Add test cardDb
                    };
                    sm.LoadGame(state); // Start from here
                    if(st != States.ACTION_PHASE) // Only check invalid states as valid state is used elsewhere during tests
                    {
                        Tuple<PlayOutcome, CardTargets> res = sm.GetPlayableOptions(-100);
                        Assert.AreEqual(res.Item1, PlayOutcome.INVALID_GAME_STATE);
                        Assert.AreEqual(res.Item2, CardTargets.INVALID);
                    }
                }
            }
        }
        [TestMethod]
        public void VerifyCardIndexValid() // Generate targets 0-4, verify i can only access valid cards
        {
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = player
                };
                for (int i = 0; i < 5; i++)
                {
                    state.PlayerStates[playerIndex].Hand.InsertCard(-100); // Insert test card (brick) in hand all targets 5 times
                }
                GameStateMachine sm = new GameStateMachine
                {
                    CardDb = TestCardGenerator.GenerateTestCardGenerator() // Add test cardDb
                };
                sm.LoadGame(state); // Start from here
                List<int> possibleCards = [-100, -100, -100, -100, -100, -110, -110, -110, -110, -110]; // First 5 are ok, last 5 are not
                for (int i = 0; i < possibleCards.Count; i++) // Check for each card
                {
                    if (i > 4) // Only test incorrect ones as correct ones are in another test
                    {
                        Tuple<PlayOutcome, CardTargets> res = sm.GetPlayableOptions(possibleCards[i]);
                        Assert.AreEqual(res.Item1, PlayOutcome.INVALID_CARD); // Would be an error!
                        Assert.AreEqual(res.Item2, CardTargets.INVALID); // Also this invalid...
                    }
                }
            }
        }
        // --------------------------------------------------------------------------------------
        // ---------------------------  ACTUALLY PLAYING THE CARD -------------------------------
        // --------------------------------------------------------------------------------------

        [TestMethod]
        public void PlayInIncorrectState() // Tries to play a card in incorrect phase
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
                    state.PlayerStates[playerIndex].Hand.InsertCard(-100); // Insert only one card, I don't care
                    GameStateMachine sm = new GameStateMachine
                    {
                        CardDb = TestCardGenerator.GenerateTestCardGenerator() // Add test cardDb
                    };
                    sm.LoadGame(state); // Start from here
                    if (st != States.ACTION_PHASE) // Only check invalid states as valid state is used elsewhere during tests
                    {
                        Tuple<PlayOutcome, StepResult> res = sm.PlayCard(-100, CardTargets.GLOBAL); // Should break every single time and nothing should happen (TODO hash verify?)
                        Assert.AreEqual(res.Item1, PlayOutcome.INVALID_GAME_STATE);
                        Assert.IsNull(res.Item2);
                    }
                }
            }
        }
        [TestMethod]
        public void PlayInvalidIndex() // Generate targets 0-4, verify i can only play valid cards
        {
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = player
                };
                for (int i = 0; i < 5; i++)
                {
                    state.PlayerStates[playerIndex].Hand.InsertCard(-100); // Insert test card (brick) in hand all targets 5 times
                }
                GameStateMachine sm = new GameStateMachine
                {
                    CardDb = TestCardGenerator.GenerateTestCardGenerator() // Add test cardDb
                };
                sm.LoadGame(state); // Start from here
                List<int> possibleCards = [-100, -100, -100, -100, -100, -110, -110, -110, -110, -110]; // First 5 are ok, last 5 are not
                for (int i = 0; i < possibleCards.Count; i++) // Check for each card
                {
                    if (i > 4) // Just test wrong case as other cases are tested elsewhere in detail
                    {
                        Tuple<PlayOutcome, StepResult> res = sm.PlayCard(possibleCards[i],CardTargets.GLOBAL);
                        Assert.AreEqual(res.Item1, PlayOutcome.INVALID_CARD); // Would be an error!
                        Assert.IsNull(res.Item2); // Also this invalid...
                    }
                }
            }
        }
        [TestMethod]
        public void PlayAllTargets() // Play absolutely all targets for all cards!
        {
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = player
                };
                for (int i = 0; i <= 7; i++)
                {
                    state.PlayerStates[playerIndex].Hand.InsertCard(-(100 + i)); // Insert test cards (brick) in hand all targets, but only valid ones because invalids test already performed
                }
                GameStateMachine sm = new GameStateMachine
                {
                    CardDb = TestCardGenerator.GenerateTestCardGenerator() // Add test cardDb
                };
                sm.LoadGame(state); // Start from here
                for (int i = 0; i < sm.GetDetailedState().PlayerStates[playerIndex].Hand.HandSize; i++) // Check for each card
                {
                    for(int j = 0; j < 10; j++) // Try and target absolutely all ways (many should break!)
                    {
                        Tuple<PlayOutcome, StepResult> res = sm.PlayCard(-(100 + i), (CardTargets)j); // Try to target card
                        if((j > 7) || (j&(j-1)) != 0) // Implying invalid targets no matter what! (Either non power of 2 or high target enum)
                        {
                            Assert.AreEqual(res.Item1, PlayOutcome.INVALID_TARGET); // Invalid target and no state change
                            Assert.AreEqual(res.Item2, null);
                        }
                        else if (i == 0) // If global targeting...
                        {
                            if(j == 0) // Global is only valid option, then should succeed
                            {
                                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                                Assert.IsNotNull(res.Item2);
                                sm.UndoPreviousStep(); // Undo to try next one
                            }
                            else
                            {
                                Assert.AreEqual(res.Item1, PlayOutcome.INVALID_TARGET); // Invalid target and no state change
                                Assert.IsNull(res.Item2);
                            }
                        }
                        else // Lane specific bricks, only playable if lane matches
                        {
                            if ((i & j) != 0) // Targets match!
                            {
                                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                                Assert.IsNotNull(res.Item2);
                                sm.UndoPreviousStep(); // Undo to try next one
                            }
                            else
                            {
                                Assert.AreEqual(res.Item1, PlayOutcome.INVALID_TARGET); // Invalid target and no state change
                                Assert.IsNull(res.Item2);
                            }
                        }
                    }
                }
            }
        }
        [TestMethod]
        public void PlayAffordability() // Generate costs 0-9, half can be afforable, half can't
        {
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = player
                };
                for (int i = 0; i < 10; i++)
                {
                    state.PlayerStates[playerIndex].Hand.InsertCard(-(100 + i * 10)); // Insert test cards (brick) in hand costs 0-9
                }
                state.PlayerStates[playerIndex].Gold = 4; // Set gold to 4
                GameStateMachine sm = new GameStateMachine
                {
                    CardDb = TestCardGenerator.GenerateTestCardGenerator() // Add test cardDb
                };
                sm.LoadGame(state); // Start from here
                for (int i = 0; i < sm.GetDetailedState().PlayerStates[playerIndex].Hand.HandSize; i++) // Check for each card
                {
                    Tuple<PlayOutcome, StepResult> res = sm.PlayCard(-(100 + i * 10), CardTargets.GLOBAL);
                    if (i <= 4)
                    {
                        Assert.AreEqual(res.Item1, PlayOutcome.OK); // Could be played
                        Assert.IsNotNull(res.Item2);
                        sm.UndoPreviousStep();
                    }
                    else
                    {
                        Assert.AreEqual(res.Item1, PlayOutcome.CANT_AFFORD); // Could not
                        Assert.IsNull(res.Item2);
                    }
                }
            }
        }
        [TestMethod]
        public void PlayCosts() // Verifies card cost is paid and card is discarded
        {
            Random rng = new Random(); // Random costs just for fun
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = player
                };
                List<int> possibleCards = new List<int>();
                for (int i = 0; i < 10; i++)
                {
                    int randomCard = -(100 + rng.Next(10) * 10);
                    state.PlayerStates[playerIndex].Hand.InsertCard(randomCard); // Insert test cards (brick) in hand with random cost 0-9
                    possibleCards.Add(randomCard); // Add it also to one of my choices
                }
                state.PlayerStates[playerIndex].Gold = rng.Next(1000,10000); // Set gold to random but high value
                GameStateMachine sm = new GameStateMachine
                {
                    CardDb = TestCardGenerator.GenerateTestCardGenerator() // Add test cardDb
                };
                sm.LoadGame(state); // Start from here
                for (int i = 0; i < 5; i++) // Will play 5 random cards
                {
                    // Know everything I'll do beforehand
                    int handSize = sm.GetDetailedState().PlayerStates[playerIndex].Hand.HandSize;
                    int cardIndexToPlay = rng.Next(handSize);
                    int cardIdToPlay = possibleCards[cardIndexToPlay]; // Get random card of the ones I generated
                    Card cardToPlay = sm.CardDb.GetCardData(cardIdToPlay);
                    int currentGold = sm.GetDetailedState().PlayerStates[playerIndex].Gold;
                    Tuple <PlayOutcome, StepResult> res = sm.PlayCard(cardIdToPlay, CardTargets.GLOBAL);
                    possibleCards.RemoveAt(cardIndexToPlay); // Remove this one
                    Assert.AreEqual(res.Item1, PlayOutcome.OK); // Could be played
                    Assert.IsNotNull(res.Item2); // Sth happened
                    Assert.AreEqual(sm.GetDetailedState().PlayerStates[playerIndex].DiscardPile.Last(), cardIdToPlay); // Card was discarded
                    Assert.AreEqual(sm.GetDetailedState().PlayerStates[playerIndex].Hand.HandSize, handSize-1); // One less card in hand
                    Assert.AreEqual(sm.GetDetailedState().PlayerStates[playerIndex].Gold, currentGold - int.Parse(cardToPlay.Cost)); // Spent the money
                    Assert.AreEqual(sm.GetDetailedState().CurrentPlayer, player); // Player still in command
                    Assert.AreEqual(sm.GetDetailedState().CurrentState, States.ACTION_PHASE); // Still in action phase
                }
            }
        }
    }
}
