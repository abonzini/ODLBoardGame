using ODLGameEngine;

namespace GameInstance
{
    /// <summary>
    /// Weights are meant to score the gamestate, each value is an array, with [0] the current player and [1] the opponent
    /// </summary>
    public class MinMaxWeights
    {
        public float[] Hp = [0, 0];
        public float[] Gold = [0, 0];
        public float[] HandSize = [0, 0];
        public float[] NBuildings = [0, 0];
        public float[] UnitStatCount = [0, 0]; // HP + Attack, not movement
        public float[] UnitTallness = [0, 0]; // Tallness tells us whether this tends to have a big dude or a lot of smaller dudes
        public bool[] IsTallnessGrowthDirect = [false, false]; // Whether tallness grows in a direct proportion or inverse proportion (1 - tallness)
    }
    /// <summary>
    /// For easy access of hardcoded data
    /// </summary>
    public static class MinMaxConstants
    {
        public const float MAX_VALUE = 100f; // Score between +-100 which means someone is definitely winning
        public const float MIN_VALUE = -MAX_VALUE;
        public const float ALPHA_BETA_THRESHOLD = 1f; // Alpha/beta +-1 to prune when (almost) certain winning
        public const float ALPHA_INITIAL = MIN_VALUE + ALPHA_BETA_THRESHOLD;
        public const float BETA_INITIAL = MAX_VALUE - ALPHA_BETA_THRESHOLD;
        public const float WILDCARD_PROBABILITY_TRESHOLD = 0.75f; // A wildcard wil be discovered if the card has at least this chance of being in there
        public const float WILDCARD_VALUE_BOOST = 0.5F; // Boost applied to wildcards to encourage card draw even when card doesn't have an immediate deterministic value when "played"
    }
    public readonly struct NodeResult
    {
        public float Score { get; }
        public GameAction BestAction { get; }
        public NodeResult(float score, GameAction action)
        {
            Score = score;
            BestAction = action;
        }
        public override string ToString()
        {
            return $"{BestAction}, {Score}";
        }
    }
    public class MinMaxAgent
    {
        GameStateMachine _sm;
        CurrentPlayer _evaluatedPlayer;
        int _evaluatedPlayerIndex;
        int _opposingPlayerIndex;
        int _maxTurnCounter;
        MinMaxWeights _weights;
        readonly CalculatorLut _calculatorLut;
        readonly Dictionary<int, NodeResult> _stateLut = new Dictionary<int, NodeResult>();
        readonly Stack<GameAction> _sharedActionStack = new Stack<GameAction>();
        // PUBLIC FIELDS (ANALYTICS)
        public int NumberOfEvaluatedNodes { get; private set; }
        public int NumberOfEvaluatedDiscoveryNodes { get; private set; }
        public int NumberOfEvaluatedTerminalNodes { get; private set; }
        // FUNCTIONS
        public MinMaxAgent(CalculatorLut lut = null)
        {
            if (lut == null) // Adds calculator LUT to agent
            {
                _calculatorLut = new CalculatorLut();
            }
            else
            {
                _calculatorLut = lut;
            }
        }
        /// <summary>
        /// Entry point for minmax. Asks to get the best actions out of a gamestate + params
        /// </summary>
        /// <returns>List of actions that optimizes reward according to minmax algorithm</returns>
        public List<GameAction> Evaluate(GameStateMachine sm, MinMaxWeights weights, AssortedCardCollection opponentCardPool, int turnDepth)
        {
            // Init params
            _sm = sm;
            _evaluatedPlayer = sm.DetailedState.CurrentPlayer;
            _evaluatedPlayerIndex = (int)_evaluatedPlayer;
            _opposingPlayerIndex = 1 - _evaluatedPlayerIndex;
            _maxTurnCounter = sm.DetailedState.TurnCounter + turnDepth;
            _weights = weights;
            _stateLut.Clear();
            _sharedActionStack.Clear();
            NumberOfEvaluatedDiscoveryNodes = 0;
            NumberOfEvaluatedNodes = 0;
            NumberOfEvaluatedTerminalNodes = 0;
            // Now, start hypothetical mode for the state machine, assume I need to optimise for current player (otherwise this doesn't make any sense)
            _sm.StartHypotheticalMode((int)_evaluatedPlayer, opponentCardPool);
            // Now, need to evaluate minmax node
            NodeResult nextResult = EvaluateNode(MinMaxConstants.ALPHA_INITIAL, MinMaxConstants.BETA_INITIAL, true); // Evaluates the current state (first state), alpha and beta have to be min/max accordingly
            // Finished evaluating minmax tree. Now all that remains is to traverse the tree with the chosen actions
            List<GameAction> solution = new List<GameAction>();
            bool finishedNavigatingTree = false;
            do
            {
                if (nextResult.BestAction.Type != ActionType.NOP) // If node has a found result with a valid action
                {
                    solution.Add(nextResult.BestAction);
                    PerformAction(nextResult.BestAction); // We also advance the state to the next step
                    // Quick check to see if we finished
                    if (_sm.DetailedState.CurrentPlayer != _evaluatedPlayer) finishedNavigatingTree = true; // Not the current player anymore
                    else if (_sm.DetailedState.CurrentState != States.ACTION_PHASE) finishedNavigatingTree = true; // Not a playable state anymore
                    else if (_sm.PlayerHasRelevantWildcards(_evaluatedPlayerIndex)) finishedNavigatingTree = true; // I got wildcards now so I can't possibly continue advancing
                    // If there's additional non-deterministic situations, will add here
                    else // Good to continue, get the next result of current action
                    {
                        nextResult = _stateLut[_sm.DetailedState.GetHashCode()];
                    }
                }
                else // Otherwise we're done here as we can't go deeper
                {
                    finishedNavigatingTree = true;
                }
            } while (!finishedNavigatingTree);
            // Can finish the hypothetical mode now, should reverse everything for us
            _sm.EndHypotheticalMode();
            return solution;
        }
        NodeResult EvaluateNode(float alpha, float beta, bool isInitial = false)
        {
            int nodeCurrentPlayerIndex = (int)_sm.DetailedState.CurrentPlayer;
            float score = 0;
            GameAction bestAction = new GameAction();
            // Now, need to see which type of node this is
            if (_sm.DetailedState.CurrentState == States.EOG) // EOG, Terminal node and CurrentPlayer is the winner
            {
                NumberOfEvaluatedTerminalNodes++;
                // Get score accordingly to winner
                score = (nodeCurrentPlayerIndex == _evaluatedPlayerIndex) ? MinMaxConstants.MAX_VALUE : MinMaxConstants.MIN_VALUE;
            }
            else if (_sm.DetailedState.TurnCounter >= _maxTurnCounter) // Depth limit reached, Terminal node, need to evaluate
            {
                NumberOfEvaluatedTerminalNodes++;
                score = EvaluateTerminalGameState();
            }
            // Otherwise, check if player has wildcards of interest (discovery node)
            else if (_sm.PlayerHasRelevantWildcards(nodeCurrentPlayerIndex))
            {
                score = EvaluateDiscoveryNode(nodeCurrentPlayerIndex, alpha, beta);
            }
            else // Otherwise, it's a good old minmax node, evaluate accordingly and get the best action
            {
                NumberOfEvaluatedNodes++;
                int playerIndex = (int)_sm.DetailedState.CurrentPlayer;
                // Firstly, get all possible actions into the action stack
                int numberOfPossibleActions = 0;
                // Need to add stack in reverse order that I want, so first, EOT is always an option
                _sharedActionStack.Push(new GameAction(ActionType.END_TURN));
                numberOfPossibleActions++;
                // Then, active power playable?
                if (_sm.GetActivePowerPlayability().PlayOutcome == PlayOutcome.OK)
                {
                    _sharedActionStack.Push(new GameAction(ActionType.ACTIVE_POWER));
                    numberOfPossibleActions++;
                }
                // Finally, check each of the cards in hand
                foreach (KeyValuePair<int, int> cardInfo in _sm.DetailedState.PlayerStates[playerIndex].Hand.GetCards().Where(card => card.Key != 0))
                {
                    PlayContext cardPlayOptions = _sm.GetPlayabilityOptions(cardInfo.Key, PlayType.PLAY_FROM_HAND);
                    if (cardPlayOptions.PlayOutcome == PlayOutcome.OK) // If this card is playable
                    {
                        foreach (int target in cardPlayOptions.ValidTargets) // Will check play for all valid targets
                        {
                            _sharedActionStack.Push(new GameAction(ActionType.PLAY_CARD, cardInfo.Key, target));
                            numberOfPossibleActions++;
                        }
                    }
                }
                // Finished assembling all the children nodes for this node
                if (numberOfPossibleActions == 1 && isInitial) // In initial state, a node with only 1 child (EOT) will just return here, no need to explore
                {
                    bestAction = _sharedActionStack.Pop();
                }
                else // Otherwise just explore all of them (unless pruned)
                {
                    bool isMax = (playerIndex == _evaluatedPlayerIndex);
                    score = isMax ? float.NegativeInfinity : float.PositiveInfinity; // Init minmax score
                    bool pruningTriggered = false;
                    for (int actionNumber = 0; actionNumber < numberOfPossibleActions; actionNumber++) // Do each one of these then...
                    {
                        float actionScore;
                        // Do action
                        GameAction action = _sharedActionStack.Pop();
                        if (pruningTriggered) continue; // If pruning happened, need to pop all remaining options before leaving
                        PerformAction(action);
                        // Evaluate state, check if stored in LUT (i.e. if exists), or create otherwise
                        int newStateHash = _sm.DetailedState.GetHashCode();
                        if (!_stateLut.TryGetValue(newStateHash, out NodeResult stateResult)) // State already know, get directly
                        {
                            stateResult = EvaluateNode(alpha, beta);
                            _stateLut.Add(newStateHash, stateResult);
                        }
                        actionScore = stateResult.Score;
                        _sm.UndoPreviousStep(); // Leave that previous state
                        // Finally, will choose whether the action is the best one so far
                        if (isMax) // Maximizer node
                        {
                            if (actionScore > score) // Found a new best-case action
                            {
                                score = actionScore;
                                bestAction = action;
                            }
                            pruningTriggered = (score > beta); // Pruning, will break loop as there's no need to continue evaluating (this option will be chosen if agent has the chance)
                            alpha = Math.Max(alpha, score);
                        }
                        else // Minimizer node
                        {
                            if (actionScore < score) // Found a new worst-case action
                            {
                                score = actionScore;
                                bestAction = action;
                            }
                            pruningTriggered = (score < alpha); // Pruning, will break loop as there's no need to continue evaluating (this option will be chosen if agent has the chance)
                            beta = Math.Min(beta, score);
                        }
                    }
                }
            }
            return new NodeResult(score, bestAction);
        }
        /// <summary>
        /// Performs the desired action on the game state machine
        /// </summary>
        /// <param name="action">What action to perform</param>
        void PerformAction(GameAction action)
        {
            switch (action.Type)
            {
                case ActionType.ACTIVE_POWER:
                    _sm.PlayActivePower();
                    break;
                case ActionType.PLAY_CARD:
                    _sm.PlayFromHand(action.Card, action.Target);
                    break;
                case ActionType.END_TURN:
                    _sm.EndTurn();
                    break;
                default:
                    throw new ArgumentException("Invalid action for the MinMax tree");
            }
        }
        /// <summary>
        /// Evaluates the current game state, returns a score
        /// </summary>
        /// <returns></returns>
        float EvaluateTerminalGameState()
        {
            float score = 0;
            GameStateStruct state = _sm.DetailedState;
            for (int evalIndex = 0; evalIndex < 2; evalIndex++) // Check both players, first the evaluated, then the opponent
            {
                int evalPlayer = (evalIndex == 0) ? _evaluatedPlayerIndex : _opposingPlayerIndex;
                // Calculate the score given a current game state
                score += (state.PlayerStates[evalPlayer].Hp.Total - state.PlayerStates[evalPlayer].DamageTokens) * _weights.Hp[evalIndex];
                score += state.PlayerStates[evalPlayer].CurrentGold * _weights.Gold[evalIndex];
                score += state.PlayerStates[evalPlayer].Hand.CardCount * _weights.HandSize[evalIndex];
                // Board evaluation
                score += state.BoardState.GetPlacedEntities(EntityType.BUILDING, evalPlayer).Count * _weights.NBuildings[evalIndex];
                SortedSet<int> playerUnits = state.BoardState.GetPlacedEntities(EntityType.UNIT, evalPlayer);
                int nUnits = playerUnits.Count;
                int totalUnitStats = 0;
                int totalUnitStatsSquared = 0;
                foreach (int unitId in playerUnits)
                {
                    Unit unit = (Unit)state.EntityData[unitId];
                    int unitStats = unit.Attack.Total + unit.Hp.Total - unit.DamageTokens;
                    totalUnitStats += unitStats;
                    totalUnitStatsSquared += unitStats * unitStats;
                }
                score += totalUnitStats * _weights.UnitStatCount[evalIndex];
                float unitTallness = 0;
                if (nUnits > 1) // No tallness if no units, or if there's a single unit
                {
                    // Calculate tallness, rms/avg
                    float sqrtN = _calculatorLut.Sqrt(nUnits);
                    unitTallness = sqrtN * _calculatorLut.Sqrt(totalUnitStatsSquared) / totalUnitStats;
                    unitTallness -= 1; // Normalising...
                    unitTallness /= (sqrtN - 1); // End normalisation, tallness now between 0-1
                    if (!_weights.IsTallnessGrowthDirect[evalIndex]) // Flip proportion if needed
                    {
                        unitTallness = 1 - unitTallness;
                    }
                }
                score += unitTallness * _weights.UnitTallness[evalIndex];
            }
            score += state.PlayerStates[_evaluatedPlayerIndex].Hand.CheckAmountInCollection(0) * _weights.HandSize[0] * MinMaxConstants.WILDCARD_VALUE_BOOST; // Add wildcard boost to encourage player to value cards even if unknown
            // Clamp if score too high and return
            if (score > MinMaxConstants.MAX_VALUE) { score = MinMaxConstants.MAX_VALUE; }
            else if (score < MinMaxConstants.MIN_VALUE) { score = MinMaxConstants.MIN_VALUE; }
            return score;
        }
        float EvaluateDiscoveryNode(int discoveryPlayerIndex, float alpha, float beta)
        {
            float score = 0f;
            int nWildcards = _sm.DetailedState.PlayerStates[discoveryPlayerIndex].Hand.CheckAmountInCollection(0); // Check how many wildcards the player has
            AssortedCardCollection discoveryCardPool = _sm.GetPlayersHypotheticalCardPool(discoveryPlayerIndex);
            // Now, cases of discovery
            if (discoveryCardPool.CardCount == 0) // No cards to discover unfortunately, this is an uninteresting situation, just analyze state as I can without more discoveries
            {
                _sm.SetPlayerHasRelevantWildcards(discoveryPlayerIndex, false);
                score = EvaluateNode(alpha, beta).Score;
            }
            else if (nWildcards >= discoveryCardPool.CardCount) // In this case, all cards can fit in the hand so I just insert all of them
            {
                NumberOfEvaluatedDiscoveryNodes++;
                foreach (KeyValuePair<int, int> cardAndCount in discoveryCardPool.GetCards().ToArray()) // Check all copies of all cards
                {
                    for (int i = 0; i < cardAndCount.Value; i++) // Check how many of this card are there
                    {
                        _sm.DiscoverHypotheticalWildcard(discoveryPlayerIndex, cardAndCount.Key, false); // Continue adding, BUT NEED TO CLOSE THE EVENT QUEUE MANUALLY
                    }
                }
                _sm.CloseEventStack();
                _sm.SetPlayerHasRelevantWildcards(discoveryPlayerIndex, false); // I put all cards, surely they can't have relevant wildcards
                // Then, analyze this state (it will continue to go deep discovering all other cards)
                score = EvaluateNode(alpha, beta).Score;
                // Undo the multi-discover step I just did (mantain SM consistency)
                _sm.UndoPreviousStep();
            }
            else // In this case, need to evaluate whether some cards are worth discovering, and if so, will need to branch with all available discoveries and weigth by probabilities
            {
                NumberOfEvaluatedDiscoveryNodes++;
                float remainingPercentage = 1.0f; // Remaining chance of the non-discovered cards
                bool theresUninterestingCards = false;
                foreach (int countNumber in discoveryCardPool.CountHistogram.Keys.ToList()) // Check all of the counts in this deck (from highest to lowest)
                {
                    // Evaluate if this number of cards is worth exploring (threshold of 50%)
                    float probability = _calculatorLut.HyperGeometric(discoveryCardPool.CardCount, nWildcards, countNumber);
                    if (probability < MinMaxConstants.WILDCARD_PROBABILITY_TRESHOLD) // If doesn't surpass 50% threshold, then the lower counts will be even less likely
                    {
                        theresUninterestingCards = true;
                        break;
                    }
                    // If reached here, means there's interesting cards to discover, definitely the ones with this quantity at least
                    foreach (int card in discoveryCardPool.CountHistogram[countNumber].ToList()) // Attempt to discover each one of these
                    {
                        probability = _calculatorLut.SingleSample(discoveryCardPool.CardCount, countNumber);
                        // Delicate process, first, discover the card
                        _sm.DiscoverHypotheticalWildcard(discoveryPlayerIndex, card);
                        // Then, analyze this state
                        score += EvaluateNode(alpha, beta).Score * probability;
                        // Undo the step I just did (mantain SM consistency)
                        _sm.UndoPreviousStep();
                        remainingPercentage -= probability; // If all makes sense, this number has a min value of 0
                    }
                }
                if (theresUninterestingCards) // Finally, in the chance there was some cards that were not considered, need to calculate "the rest"
                {
                    _sm.SetPlayerHasRelevantWildcards(discoveryPlayerIndex, false); // No more interesting wildcards here
                    score += EvaluateNode(alpha, beta).Score * remainingPercentage; // Add the weighted equivalent to this case
                }
            }
            return score;
        }
    }
}
