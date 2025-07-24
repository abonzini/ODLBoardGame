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
        [TestMethod]
        public void DrawToEnsureLethal()
        {
            // Situation: Player has 3 cards in deck, 2 bricks and a card that will give them lethal. Active power also a brick
            // In hand, they have a card that draws 2
            // Opponent has only one damage card at hand, needs to play 2 of them but their deck is full of those, therefore, need to play precisely otherwise player loses
            // All cards cost 2, each player has 2g, and 10hp/5hp respectively, no deckout damage should be seen in this scenario
            // The optimal play is to draw, to ensure lethal in hand one turn before opp
            // In debug, it could be seen discovery properly determines the 2-draw will have a 66% of giving the winning card at draw and a 100% afterwards
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                // Creation of cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Skill that does nothing, cost 2, also active power
                Skill skill1 = TestCardGenerator.CreateSkill(1, 2, [0], CardTargetingType.BOARD);
                // Card 2: Skill cost 2, does 5 to opponent
                Skill skill2 = TestCardGenerator.CreateSkill(2, 2, [0], CardTargetingType.BOARD);
                // Card 3: Skill cost 2, draws 2
                Skill skill3 = TestCardGenerator.CreateSkill(3, 2, [0], CardTargetingType.BOARD);
                // The effects
                Effect targetBoardEffect = new Effect() // Get board
                {
                    EffectType = EffectType.ADD_LOCATION_REFERENCE,
                    EffectLocation = EffectLocation.BOARD,
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
                Effect drawEffect = new Effect()
                {
                    EffectType = EffectType.CARD_DRAW,
                    TempVariable = 2,
                    Input = Variable.TEMP_VARIABLE,
                    TargetPlayer = EntityOwner.OWNER
                };
                skill2.Interactions = new Dictionary<InteractionType, List<Effect>>();
                skill2.Interactions.Add(InteractionType.WHEN_PLAYED, [targetBoardEffect, targetOppEffect, damageEffect]);
                skill3.Interactions = new Dictionary<InteractionType, List<Effect>>();
                skill3.Interactions.Add(InteractionType.WHEN_PLAYED, [drawEffect]);
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
                state.PlayerStates[playerIndex].Hand.AddToCollection(3); // Players have the cards in hand and the active power
                state.PlayerStates[playerIndex].Deck.InitializeDeck([1, 1, 2]); // Players won't experience deckout in this trial
                state.PlayerStates[opponentIndex].Deck.InitializeDeck([2, 2, 2, 2, 2]);
                state.PlayerStates[playerIndex].Hp.BaseValue = 10; // Player has 10Hp so it'll live two turns. Opp will die with 1 skill
                state.PlayerStates[opponentIndex].Hp.BaseValue = 5;
                state.PlayerStates[playerIndex].CurrentGold = 2; // Can spend on exactly one card per turn
                state.PlayerStates[opponentIndex].CurrentGold = 2;
                state.PlayerStates[playerIndex].PowerAvailable = true; // Player needs to avoid wasting their gold in the power
                state.PlayerStates[opponentIndex].PowerAvailable = true;
                // Hypothetical opp deck (I know their cards this time)
                AssortedCardCollection assumedOppDeck = new AssortedCardCollection();
                assumedOppDeck.AddToCollection(2);
                assumedOppDeck.AddToCollection(2);
                assumedOppDeck.AddToCollection(2);
                assumedOppDeck.AddToCollection(2);
                assumedOppDeck.AddToCollection(2);
                // State machine
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state);
                int preMinMaxHash = state.GetHashCode(); // Need to ensure integrity when returning
                // Now, to start the MinMax evaluation
                MinMaxAgent minMax = new MinMaxAgent();
                List<GameAction> winningActions = minMax.Evaluate(sm, new MinMaxWeights(), assumedOppDeck, 10); // Evaluate the minmax state with weights I don't care about and a depth of 10 turns which is more than enough
                Assert.AreEqual(preMinMaxHash, state.GetHashCode()); // State completely unchanged
                // Evaluate search results, there should only be one winning combination, on play 3 and then 2. Other things lead to loss so they shouldn't have been even considered (or explored)
                Assert.AreEqual(1, winningActions.Count);
                // First play 3
                Assert.AreEqual(ActionType.PLAY_CARD, winningActions[0].Type);
                Assert.AreEqual(3, winningActions[0].Card);
                Assert.AreEqual(0, winningActions[0].Target);
                // Should be no more actions as the next step was probably discovery of cards
            }
        }
        [TestMethod]
        public void DrawToTryGetLethal()
        {
            // Situation: Player has 3 cards in deck, 2 cards that will give them lethal and a brick. Active power also a brick
            // In hand, they have a card that draws 1, everything is free
            // Opponent has a card that kills player immediately so the player will have a guaranteed loss next turn
            // The optimal play is to draw, but has a 66% chance of lethal, which is still hopefully the best option
            // In debug, it could be seen discovery properly determines the 2-draw will have a 66% of giving the winning card at draw, which is better than the alternatives
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                // Creation of cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Skill that does nothing, cost 2, also active power
                Skill skill1 = TestCardGenerator.CreateSkill(1, 0, [0], CardTargetingType.BOARD);
                // Card 2: Skill that does 5 to opponent
                Skill skill2 = TestCardGenerator.CreateSkill(2, 0, [0], CardTargetingType.BOARD);
                // Card 3: Skill that draws 1
                Skill skill3 = TestCardGenerator.CreateSkill(3, 0, [0], CardTargetingType.BOARD);
                // The effects
                Effect targetBoardEffect = new Effect() // Get board
                {
                    EffectType = EffectType.ADD_LOCATION_REFERENCE,
                    EffectLocation = EffectLocation.BOARD,
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
                Effect drawEffect = new Effect()
                {
                    EffectType = EffectType.CARD_DRAW,
                    TempVariable = 1,
                    Input = Variable.TEMP_VARIABLE,
                    TargetPlayer = EntityOwner.OWNER
                };
                skill2.Interactions = new Dictionary<InteractionType, List<Effect>>();
                skill2.Interactions.Add(InteractionType.WHEN_PLAYED, [targetBoardEffect, targetOppEffect, damageEffect]);
                skill3.Interactions = new Dictionary<InteractionType, List<Effect>>();
                skill3.Interactions.Add(InteractionType.WHEN_PLAYED, [drawEffect]);
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
                state.PlayerStates[playerIndex].Hand.AddToCollection(3); // Players have a draw 1 card
                state.PlayerStates[playerIndex].Deck.InitializeDeck([2, 2, 1]); // 66% of lethal
                state.PlayerStates[opponentIndex].Deck.InitializeDeck([2, 2, 2, 2, 2]); // Opp WILL kill you
                state.PlayerStates[playerIndex].Hp.BaseValue = 5; // Both players will be one-shotted
                state.PlayerStates[opponentIndex].Hp.BaseValue = 5;
                state.PlayerStates[playerIndex].PowerAvailable = false; // Active power is not relevant in this test
                state.PlayerStates[opponentIndex].PowerAvailable = false;
                // Hypothetical opp deck (I know their cards this time)
                AssortedCardCollection assumedOppDeck = new AssortedCardCollection();
                assumedOppDeck.AddToCollection(2);
                assumedOppDeck.AddToCollection(2);
                assumedOppDeck.AddToCollection(2);
                assumedOppDeck.AddToCollection(2);
                assumedOppDeck.AddToCollection(2);
                // State machine
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state);
                int preMinMaxHash = state.GetHashCode(); // Need to ensure integrity when returning
                // Now, to start the MinMax evaluation
                MinMaxAgent minMax = new MinMaxAgent();
                List<GameAction> winningActions = minMax.Evaluate(sm, new MinMaxWeights(), assumedOppDeck, 10); // Evaluate the minmax state with weights I don't care about and a depth of 10 turns which is more than enough
                Assert.AreEqual(preMinMaxHash, state.GetHashCode()); // State completely unchanged
                // Evaluate search results, there should only be one winning combination, on play 3 and hope for the best
                // First play 3
                Assert.AreEqual(ActionType.PLAY_CARD, winningActions[0].Type);
                Assert.AreEqual(3, winningActions[0].Card);
                Assert.AreEqual(0, winningActions[0].Target);
                // Should be no more actions as the next step was probably discovery of cards and RNG dependent
            }
        }
        [TestMethod]
        public void EotBestOption()
        {
            // Situation: Opponent will die the moment I eot because of deck-out.
            // Player has no cards in deck, and a card in hand that will kill them if played.
            // Optimal play is to end turn, but not all of the options are EOT so the machine needs to think
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                // Creation of cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Skill cost 2, does 5 to user (??), also active power
                Skill skill1 = TestCardGenerator.CreateSkill(1, 2, [0], CardTargetingType.BOARD);
                // The effects
                Effect targetBoardEffect = new Effect() // Get board
                {
                    EffectType = EffectType.ADD_LOCATION_REFERENCE,
                    EffectLocation = EffectLocation.BOARD,
                };
                Effect targetUserEffect = new Effect() // Gets opponent only
                {
                    EffectType = EffectType.FIND_ENTITIES,
                    TargetPlayer = EntityOwner.OWNER,
                    TargetType = EntityType.PLAYER,
                    SearchCriterion = SearchCriterion.ALL,
                };
                Effect damageEffect = new Effect() // Deals 5 damage to targets
                {
                    EffectType = EffectType.EFFECT_DAMAGE,
                    TempVariable = 5,
                    Input = Variable.TEMP_VARIABLE
                };
                skill1.Interactions = new Dictionary<InteractionType, List<Effect>>();
                skill1.Interactions.Add(InteractionType.WHEN_PLAYED, [targetBoardEffect, targetUserEffect, damageEffect]);
                // Add to DB
                cardDb.InjectCard(1, skill1);
                // Assemble state
                int playerIndex = (int)player;
                int opponentIndex = 1 - playerIndex;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                state.PlayerStates[playerIndex].Hand.AddToCollection(1); // Players have the cards in hand and the active power
                state.PlayerStates[playerIndex].Hp.BaseValue = 1; // Player has 1Hp so they'll die immediately
                state.PlayerStates[opponentIndex].Hp.BaseValue = 1;
                state.PlayerStates[playerIndex].CurrentGold = 20; // Gold is not an issue
                state.PlayerStates[opponentIndex].CurrentGold = 20;
                state.PlayerStates[playerIndex].PowerAvailable = true; // Player needs to avoid usign the power too
                state.PlayerStates[opponentIndex].PowerAvailable = true;
                // Hypothetical opp deck (I know their cards this time)
                AssortedCardCollection assumedOppDeck = new AssortedCardCollection();
                // State machine
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state);
                int preMinMaxHash = state.GetHashCode(); // Need to ensure integrity when returning
                // Now, to start the MinMax evaluation
                MinMaxAgent minMax = new MinMaxAgent();
                List<GameAction> winningActions = minMax.Evaluate(sm, new MinMaxWeights(), assumedOppDeck, 10); // Evaluate the minmax state with weights I don't care about and a depth of 10 turns which is more than enough
                Assert.AreEqual(preMinMaxHash, state.GetHashCode()); // State completely unchanged
                // Evaluate search results, there should only be one winning combination, on play 3 and then 2. Other things lead to loss so they shouldn't have been even considered (or explored)
                Assert.AreEqual(1, winningActions.Count);
                // Only move is to pass
                Assert.AreEqual(ActionType.END_TURN, winningActions[0].Type);
                Assert.AreEqual(1, minMax.NumberOfEvaluatedNodes); // Shouldn't go too deep
                Assert.AreNotEqual(0, minMax.NumberOfEvaluatedTerminalNodes); // However unlike the "insta EOT leaving", it does investigate some (bad) options
            }
        }
        [TestMethod]
        public void FastReturnIfNothingToDo()
        {
            // Situation: Player can't play anything, their power is disabled, has a very expensive unit card, and a building with no units on board
            // In this case, where EOT is the only option, the exploration insta-finishes with 1 node depth
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                // Creation of cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Skill that does nothing, cost 0, also active power
                Skill skill1 = TestCardGenerator.CreateSkill(1, 0, [0], CardTargetingType.BOARD);
                // Card 2: Expensive unit
                Unit unit = TestCardGenerator.CreateUnit(2, "BIG GUY", 20, [0, 4, 10], 10, 10, 10, 1);
                // Card 3: Cheap building but can't be played as there's no units
                Building bldg = TestCardGenerator.CreateBuilding(3, "Bldg", 0, [], 2);
                // Add to DB
                cardDb.InjectCard(1, skill1);
                cardDb.InjectCard(2, unit);
                cardDb.InjectCard(3, bldg);
                // Assemble state
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                state.PlayerStates[playerIndex].Hand.AddToCollection(2); // Both cards to hand
                state.PlayerStates[playerIndex].Hand.AddToCollection(3);
                state.PlayerStates[playerIndex].Hp.BaseValue = 10; // Player has 10Hp so it'll live two turns. Opp will die with 1 skill
                state.PlayerStates[playerIndex].CurrentGold = 2; // Can't play anything anyway
                state.PlayerStates[playerIndex].PowerAvailable = false; // Can't play anything anyway
                // Inconsequential
                AssortedCardCollection assumedOppDeck = new AssortedCardCollection();
                // State machine
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state);
                int preMinMaxHash = state.GetHashCode(); // Need to ensure integrity when returning
                // Now, to start the MinMax evaluation
                MinMaxAgent minMax = new MinMaxAgent();
                List<GameAction> winningActions = minMax.Evaluate(sm, new MinMaxWeights(), assumedOppDeck, 10); // Should return immediately
                Assert.AreEqual(preMinMaxHash, state.GetHashCode()); // State completely unchanged
                // Evaluate search results, a single winning node with end of turn as result 
                Assert.AreEqual(1, winningActions.Count);
                Assert.AreEqual(ActionType.END_TURN, winningActions[0].Type);
                Assert.AreEqual(1, minMax.NumberUniqueNodes);
                Assert.AreEqual(1, minMax.NumberOfEvaluatedNodes);
                Assert.AreEqual(0, minMax.NumberOfEvaluatedTerminalNodes);
                Assert.AreEqual(0, minMax.NumberOfEvaluatedDiscoveryNodes);
            }
        }
        [TestMethod]
        public void MultiTargetPlay()
        {
            // Situation: Opponent has units in lanes 0,2. Will kill you on march with either of them.
            // Player can only play 2 units, needs to play them to block the march, in which case they'll win with the deckout damage
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                // Creation of cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Skill that does nothing (irrelevant here)
                Skill skill1 = TestCardGenerator.CreateSkill(1, 0, [0], CardTargetingType.BOARD);
                // Card 2: Unit
                Unit unit1 = TestCardGenerator.CreateUnit(2, "UNIT", 0, [0, 4, 10], 1, 1, 1, 1);
                // Add to DB
                cardDb.InjectCard(1, skill1);
                cardDb.InjectCard(2, unit1);
                // Assemble state
                int playerIndex = (int)player;
                int opponentIndex = 1 - playerIndex;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                state.PlayerStates[playerIndex].Hand.AddToCollection(2, 2); // Players have 2 units in hand
                state.PlayerStates[playerIndex].Hp.BaseValue = 1; // Player has 1Hp so they'll die immediately
                state.PlayerStates[opponentIndex].Hp.BaseValue = 1;
                state.PlayerStates[playerIndex].PowerAvailable = false; // No power in this test
                state.PlayerStates[opponentIndex].PowerAvailable = false;
                // Hypothetical opp deck (I know their cards this time)
                AssortedCardCollection assumedOppDeck = new AssortedCardCollection();
                // State machine and gamestate prep
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state);
                sm.UNIT_PlayUnit(opponentIndex, new PlayContext() { Actor = (Unit)unit1.Clone(), PlayedTarget = (playerIndex == 0) ? 0 : 3 });
                sm.UNIT_PlayUnit(opponentIndex, new PlayContext() { Actor = (Unit)unit1.Clone(), PlayedTarget = (playerIndex == 0) ? 10 : 17 });
                sm.CloseEventStack();
                int preMinMaxHash = state.GetHashCode(); // Need to ensure integrity when returning
                // Now, to start the MinMax evaluation
                MinMaxAgent minMax = new MinMaxAgent();
                List<GameAction> winningActions = minMax.Evaluate(sm, new MinMaxWeights(), assumedOppDeck, 10); // Evaluate the minmax state with weights I don't care about and a depth of 10 turns which is more than enough
                Assert.AreEqual(preMinMaxHash, state.GetHashCode()); // State completely unchanged
                // Evaluate search results, only winning move would be to lpay one unit on 0 and another on 10
                Assert.AreEqual(3, winningActions.Count);
                Assert.AreEqual(ActionType.PLAY_CARD, winningActions[0].Type);
                Assert.AreEqual(2, winningActions[0].Card);
                Assert.IsTrue(((playerIndex == 0) ? 0 : 3) == winningActions[0].Target || ((playerIndex == 0) ? 10 : 17) == winningActions[0].Target);
                Assert.AreEqual(ActionType.PLAY_CARD, winningActions[1].Type);
                Assert.AreEqual(2, winningActions[1].Card);
                Assert.IsTrue(((playerIndex == 0) ? 0 : 3) == winningActions[1].Target || ((playerIndex == 0) ? 10 : 17) == winningActions[1].Target);
                Assert.AreEqual(ActionType.END_TURN, winningActions[2].Type);
            }
        }
        [TestMethod]
        public void DrawWeights()
        {
            // Situation: Minmax with 1 depth, just want one best play. Give player the option to draw or not, and evaluate whether draw is chosen depending on gold + card value
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                // Creation of cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Active power that draws 2 card, costs 2
                Skill skill1 = TestCardGenerator.CreateSkill(1, 2, [0], CardTargetingType.BOARD);
                Effect drawEffect = new Effect()
                {
                    EffectType = EffectType.CARD_DRAW,
                    TempVariable = 2,
                    Input = Variable.TEMP_VARIABLE,
                    TargetPlayer = EntityOwner.OWNER
                };
                skill1.Interactions = new Dictionary<InteractionType, List<Effect>>();
                skill1.Interactions.Add(InteractionType.WHEN_PLAYED, [drawEffect]);
                // Add to DB
                cardDb.InjectCard(1, skill1);
                // Assemble state
                int playerIndex = (int)player;
                int opponentIndex = 1 - playerIndex;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                state.PlayerStates[playerIndex].PowerAvailable = true; // Power available, and only option
                state.PlayerStates[opponentIndex].PowerAvailable = true;
                state.PlayerStates[playerIndex].CurrentGold = 2; // Can only play one
                state.PlayerStates[playerIndex].Deck.InitializeDeck([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12]); // Initialize decks with random assorted (non-existant) cards to avoid discovery phase
                // Hypothetical opp deck (I know their cards this time)
                AssortedCardCollection assumedOppDeck = new AssortedCardCollection();
                // State machine and gamestate prep
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state);
                int preMinMaxHash = state.GetHashCode(); // Need to ensure integrity when returning
                // Now, to start the MinMax evaluation
                MinMaxAgent minMax = new MinMaxAgent();
                MinMaxWeights weights = new MinMaxWeights();
                weights.Gold[0] = 1; // player values gold at 1, means has 2 points and playing the card woud lose them
                float[] cardWeights = [0, 1]; // Having new cards is either valuable enough to play a draw 2, or not
                foreach (float cardWeight in cardWeights)
                {
                    weights.HandSize[0] = cardWeight;
                    List<GameAction> winningActions = minMax.Evaluate(sm, weights, assumedOppDeck, 1); // Depth 1 evaluation, that is, estimate only one turn
                    Assert.AreEqual(preMinMaxHash, state.GetHashCode()); // State completely unchanged
                    Assert.AreEqual(1, winningActions.Count); // Eiither end turn or draw unknown
                    if (cardWeight > 0) // Card valuable enough to be played
                    {
                        Assert.AreEqual(ActionType.ACTIVE_POWER, winningActions[0].Type);
                    }
                    else // playing card is not worth it
                    {
                        Assert.AreEqual(ActionType.END_TURN, winningActions[0].Type);
                    }
                }
            }
        }
        [TestMethod]
        public void WildcardBonus()
        {
            // Situation: Ensures drawing wildcards has an extra bonus score, to assume a card is valuable even when not known during state asusmption
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                // Creation of cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Active power that draws 2 card, costs 2
                Skill skill1 = TestCardGenerator.CreateSkill(1, 2, [0], CardTargetingType.BOARD);
                Effect drawEffect = new Effect()
                {
                    EffectType = EffectType.CARD_DRAW,
                    TempVariable = 2,
                    Input = Variable.TEMP_VARIABLE,
                    TargetPlayer = EntityOwner.OWNER
                };
                skill1.Interactions = new Dictionary<InteractionType, List<Effect>>();
                skill1.Interactions.Add(InteractionType.WHEN_PLAYED, [drawEffect]);
                // Add to DB
                cardDb.InjectCard(1, skill1);
                // Assemble state
                int playerIndex = (int)player;
                int opponentIndex = 1 - playerIndex;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                state.PlayerStates[playerIndex].PowerAvailable = true; // Power available, and only option
                state.PlayerStates[opponentIndex].PowerAvailable = true;
                state.PlayerStates[playerIndex].CurrentGold = 2; // Can only play one
                state.PlayerStates[playerIndex].Deck.InitializeDeck([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12]); // Initialize decks with random assorted (non-existant) cards to avoid discovery phase
                // Hypothetical opp deck (I know their cards this time)
                AssortedCardCollection assumedOppDeck = new AssortedCardCollection();
                // State machine and gamestate prep
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state);
                int preMinMaxHash = state.GetHashCode(); // Need to ensure integrity when returning
                // Now, to start the MinMax evaluation
                MinMaxAgent minMax = new MinMaxAgent();
                MinMaxWeights weights = new MinMaxWeights();
                weights.Gold[0] = 1; // player values gold at 1, means has 2 points and playing the card woud lose them
                weights.HandSize[0] = 0.99f; // Having hand is not valuable enough, meaning the "play card" option is never good, but wildcards would push this over the edge
                List<GameAction> winningActions = minMax.Evaluate(sm, weights, assumedOppDeck, 1); // Depth 1 evaluation, that is, estimate only one turn
                Assert.AreEqual(preMinMaxHash, state.GetHashCode()); // State completely unchanged
                Assert.AreEqual(1, winningActions.Count); // Eiither end turn or draw unknown
                Assert.AreEqual(ActionType.ACTIVE_POWER, winningActions[0].Type);
            }
        }
        [TestMethod]
        public void Tallness()
        {
            // Situation: gives 2 options for 12 worth of stats, either 3x 2-2 or 2x1-1 + a 4-4
            // With a talness score, decide which option is better
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                // Creation of cards
                CardFinder cardDb = new CardFinder();
                // Units, stats correspond to the number
                Unit unit11 = TestCardGenerator.CreateUnit(11, "1-1", 1, [0, 4, 10], 1, 1, 1, 1);
                Unit unit22 = TestCardGenerator.CreateUnit(22, "2-2", 0, [0, 4, 10], 2, 2, 1, 1);
                Unit unit44 = TestCardGenerator.CreateUnit(44, "4-4", 3, [0, 4, 10], 4, 4, 1, 1);
                // Card 1: Active power that summons 3x 2-2, costs 6
                Skill skill1 = TestCardGenerator.CreateSkill(1, 5, [0], CardTargetingType.BOARD);
                Effect targetPlains = new Effect()
                {
                    EffectType = EffectType.ADD_LOCATION_REFERENCE,
                    EffectLocation = EffectLocation.PLAINS
                };
                Effect targetForest = new Effect()
                {
                    EffectType = EffectType.ADD_LOCATION_REFERENCE,
                    EffectLocation = EffectLocation.FOREST
                };
                Effect targetMountains = new Effect()
                {
                    EffectType = EffectType.ADD_LOCATION_REFERENCE,
                    EffectLocation = EffectLocation.MOUNTAIN
                };
                Effect summonEffect = new Effect()
                {
                    EffectType = EffectType.SUMMON_UNIT,
                    TargetPlayer = EntityOwner.OWNER,
                    Input = Variable.TEMP_VARIABLE,
                    TempVariable = 22
                };
                skill1.Interactions = new Dictionary<InteractionType, List<Effect>>();
                skill1.Interactions.Add(InteractionType.WHEN_PLAYED, [targetForest, targetMountains, targetPlains, summonEffect]);
                // Add to DB
                cardDb.InjectCard(1, skill1);
                cardDb.InjectCard(11, unit11);
                cardDb.InjectCard(22, unit22);
                cardDb.InjectCard(44, unit44);
                // Assemble state
                int playerIndex = (int)player;
                int opponentIndex = 1 - playerIndex;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                state.PlayerStates[playerIndex].Hand.AddToCollection(11, 2);
                state.PlayerStates[playerIndex].Hand.AddToCollection(44, 1);
                state.PlayerStates[playerIndex].PowerAvailable = true; // Power available, and only option
                state.PlayerStates[playerIndex].CurrentGold = 5; // Can only play power or dump all units
                // Hypothetical opp deck (I know their cards this time)
                AssortedCardCollection assumedOppDeck = new AssortedCardCollection();
                // State machine and gamestate prep
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state);
                int preMinMaxHash = state.GetHashCode(); // Need to ensure integrity when returning
                // Now, to start the MinMax evaluation
                MinMaxAgent minMax = new MinMaxAgent();
                MinMaxWeights weights = new MinMaxWeights();
                weights.UnitStatCount[0] = 1; // Stats being valued means player will attempt to play every single thing
                weights.UnitTallness[0] = 1; // Tallness will be relevant
                bool[] tallnessPreferredOptions = [false, true];
                foreach (bool tallnessPreferred in tallnessPreferredOptions)
                {
                    weights.IsTallnessGrowthDirect[0] = tallnessPreferred;
                    List<GameAction> winningActions = minMax.Evaluate(sm, weights, assumedOppDeck, 1); // Depth 1 evaluation, that is, estimate only one turn
                    if (tallnessPreferred) // want the big dude here
                    {
                        Assert.AreEqual(4, winningActions.Count); // Play 3 dudes and skip
                        Assert.AreEqual(ActionType.END_TURN, winningActions[3].Type);
                        AssortedCardCollection cardsPlayed = new AssortedCardCollection();
                        // Verify play and then add to history (don't really care about target)
                        Assert.AreEqual(ActionType.PLAY_CARD, winningActions[0].Type);
                        cardsPlayed.AddToCollection(winningActions[0].Card);
                        Assert.AreEqual(ActionType.PLAY_CARD, winningActions[1].Type);
                        cardsPlayed.AddToCollection(winningActions[1].Card);
                        Assert.AreEqual(ActionType.PLAY_CARD, winningActions[2].Type);
                        cardsPlayed.AddToCollection(winningActions[2].Card);
                        // Finally, verify
                        Assert.AreEqual(2, cardsPlayed.CheckAmountInCollection(11));
                        Assert.AreEqual(1, cardsPlayed.CheckAmountInCollection(44));
                    }
                    else // Otherwise the 3 2-2 are better
                    {
                        Assert.AreEqual(2, winningActions.Count); // Power and skip
                        Assert.AreEqual(ActionType.ACTIVE_POWER, winningActions[0].Type);
                        Assert.AreEqual(ActionType.END_TURN, winningActions[1].Type);
                    }
                }
            }
        }
        // Dedicate some time to a full game state around midgame, check time and stuff. Returns a play of card hopefully. Use base set cards for this one, flood deck with stuff and see what happens. Expecting a time estimate, node + depth evaluation and hope for no weird exceptions
    }
}
