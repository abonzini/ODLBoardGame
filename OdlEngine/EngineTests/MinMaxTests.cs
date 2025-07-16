using GameInstance;
using ODLGameEngine;

namespace EngineTests
{
    [TestClass]
    public class MinMaxTests
    {
        [TestMethod]
        public void TestLethalCombination()
        {
            // Given a series of 3 cards, one that does nothing (active power, won't be used), one that does damage to both players, and one only to enemy.
            // Both players have the same cards. Try and ensure whether the correct combination can be found (3 -> 2)
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                // Creation of cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Skill that does nothing, cost 5 brick, active power
                Skill skill1 = TestCardGenerator.CreateSkill(1, 5, [0], CardTargetingType.BOARD);
                // Card 2: Skill cost 5, does 5 to each player, opponent first
                Skill skill2 = TestCardGenerator.CreateSkill(2, 5, [0], CardTargetingType.BOARD);
                // Card 3: Skill cost 5, does 5 to opponent
                Skill skill3 = TestCardGenerator.CreateSkill(3, 5, [0], CardTargetingType.BOARD);
                // The effects
                Effect targetBoardEffect = new Effect() // Get board
                {
                    EffectType = EffectType.ADD_LOCATION_REFERENCE,
                    EffectLocation = EffectLocation.BOARD,
                };
                Effect targetUserEffect = new Effect() // Gets user only
                {
                    EffectType = EffectType.FIND_ENTITIES,
                    TargetPlayer = EntityOwner.OWNER,
                    TargetType = EntityType.PLAYER,
                    SearchCriterion = SearchCriterion.ALL,
                };
                Effect targetOppEffect = new Effect() // Gets opponent only
                {
                    EffectType = EffectType.FIND_ENTITIES,
                    TargetPlayer = EntityOwner.OPPONENT,
                    TargetType = EntityType.PLAYER,
                    SearchCriterion = SearchCriterion.ALL,
                };
                Effect damageEffect = new Effect() // Deals 5 damage to targets
                {
                    EffectType = EffectType.EFFECT_DAMAGE,
                    TempVariable = 5,
                    Input = Variable.TEMP_VARIABLE
                };
                skill2.Interactions = new Dictionary<InteractionType, List<Effect>>();
                skill2.Interactions.Add(InteractionType.WHEN_PLAYED, [targetBoardEffect, targetOppEffect, damageEffect, targetUserEffect, damageEffect]);
                skill3.Interactions = new Dictionary<InteractionType, List<Effect>>();
                skill3.Interactions.Add(InteractionType.WHEN_PLAYED, [targetBoardEffect, targetOppEffect, damageEffect]);
                // Add to DB
                cardDb.InjectCard(1, skill1);
                cardDb.InjectCard(2, skill2);
                cardDb.InjectCard(3, skill3);
                // Assemble state
                int playerIndex = (int)player;
                int opponentIndex = 1 - playerIndex;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                state.PlayerStates[playerIndex].Hand.AddToCollection(2); // Players have the cards in hand and the active power
                state.PlayerStates[playerIndex].Hand.AddToCollection(3);
                state.PlayerStates[opponentIndex].Hand.AddToCollection(2);
                state.PlayerStates[opponentIndex].Hand.AddToCollection(3);
                state.PlayerStates[playerIndex].Deck.InitializeDeck([1]); // Players won't experience deckout in this trial
                state.PlayerStates[opponentIndex].Deck.InitializeDeck([1]);
                state.PlayerStates[playerIndex].Hp.BaseValue = 5; // Player has 5Hp so it needs to play 3-2 exactly. Opp has 10 because they'll be hit by 2 5-damage skills, no deckout
                state.PlayerStates[opponentIndex].Hp.BaseValue = 10;
                state.PlayerStates[playerIndex].CurrentGold = 10; // Can spend on exactly 2 and only 2
                state.PlayerStates[opponentIndex].CurrentGold = 10;
                state.PlayerStates[playerIndex].PowerAvailable = false; // Player can't use their powers anyway
                state.PlayerStates[opponentIndex].PowerAvailable = false;
                // Hypothetical opp deck (I know their cards this time)
                AssortedCardCollection assumedOppDeck = new AssortedCardCollection();
                assumedOppDeck.AddToCollection(2);
                assumedOppDeck.AddToCollection(3);
                // State machine
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state);
                int preMinMaxHash = state.GetHashCode(); // Need to ensure integrity when returning
                // Now, to start the MinMax evaluation
                MinMaxAgent minMax = new MinMaxAgent();
                List<GameAction> winningActions = minMax.Evaluate(sm, new MinMaxWeights(), assumedOppDeck, 10); // Evaluate the minmax state with weights I don't care about and a depth of 10 turns which is more than enough
                Assert.AreEqual(preMinMaxHash, state.GetHashCode()); // State completely unchanged
                // Evaluate search results, there should only be one winning combination, on play 3 and then 2. Other things lead to loss so they shouldn't have been even considered (or explored)
                Assert.AreEqual(2, winningActions.Count);
                // First play 3
                Assert.AreEqual(ActionType.PLAY_CARD, winningActions[0].Type);
                Assert.AreEqual(3, winningActions[0].Card);
                Assert.AreEqual(0, winningActions[0].Target);
                // And then 2
                Assert.AreEqual(ActionType.PLAY_CARD, winningActions[0].Type);
                Assert.AreEqual(2, winningActions[1].Card);
                Assert.AreEqual(0, winningActions[1].Target);
            }
        }
        // Placeholder for now IG
        // Ideas:
        // Win guaranteed but only if discovered in wildcards
        // Instant return if only EOT as option, add unaffordable unit, affordable building, and unavail AP, shhould explore 1 node only
        // A case where ending turn is the best option (deckout win e.g.)
        // Similar to above but you need to kill an opp unit to win (option between correct and incorrect one)
        // Similar to above but the card to kill opp will come later (guarantee)
        // Order of card play irrelevant for minmax (lut usage)
        // Similar to above but have to choose between march card and deckout damage, march wins as it's better than EOT?
        // State evaluation: Different things prioritised more than others? I.e. prefer tallness or prefer multiple bros
        // E.g. there can be a card that deals a ton of damage to enemy but also summons units, and choose which one to choose then from these?
        // A guaranteed loss case where the only winning move is to play a card that lowers opp health enough (don't score health tho)
        // Same/similar as above but the chance is depending on drawing another of these from a pool of couple of cards. Ensures average works
        // Testing of a combo that draws many but hurts you. It'd score really bad but the (hypothetical) cards in deck can create a ohko if played in the right order (because one of them hurts you for lethal if played first). Ensures discovery of all works
    }
}
