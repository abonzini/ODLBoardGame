using ODLGameEngine;

namespace GameInstance
{
    /// <summary>
    /// Weights are meant to score the gamestate, each value is an array, with [0] the current player and [1] the opponent
    /// </summary>
    public class MinMaxWeights
    {
        public float[] Hp;
        public float[] Gold;
        public float[] HandSize;
        public float[] NBuildings;
        public float[] NUnits;
        public float[] UnitStatCount; // HP + Attack, not movement
        public float[] UnitTallness; // Tallness tells us whether this tends to have a big dude or a lot of smaller dudes
        public bool[] IsTallnessGrowthDirect; // Whether tallness grows in a direct proportion or inverse proportion (1 - tallness)
    }
    public class MinMaxAgent
    {
        GameStateMachine _sm;
        CurrentPlayer _currentPlayer;
        int _currentPlayerIndex;
        int _opposingPlayerIndex;
        int _maxTurnCounter;
        MinMaxWeights _weights;
        readonly CalculatorLut _calculatorLut;
        Dictionary<int, Tuple<float, GameAction>> _stateLut;
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
        public List<GameAction> CalculateBestActions(GameStateMachine sm, MinMaxWeights weights, AssortedCardCollection opponentCardPool, int maxDepth)
        {
            // Init params
            _sm = sm;
            _currentPlayer = sm.DetailedState.CurrentPlayer;
            _currentPlayerIndex = (int)_currentPlayer;
            _opposingPlayerIndex = 1 - _currentPlayerIndex;
            _maxTurnCounter = sm.DetailedState.TurnCounter + maxDepth;
            _weights = weights;
            _stateLut = new Dictionary<int, Tuple<float, GameAction>>();
            // Now, start hypothetical mode for the state machine, assume I need to optimise for current player (otherwise this doesn't make any sense)
            _sm.StartHypotheticalMode((int)_currentPlayer, opponentCardPool);
            // Now, need to evaluate minmax node
            EvaluateCurrentState(int.MinValue, int.MaxValue, true); // Evaluates the current state (first state), alpha and beta have to be min/max accordingly
            // TODO: Finally, once minmax node is done, traverse the game state again to get the chain of recommended actions (until becomes non-deterministic)
            // Can finish the hypothetical mode now
            _sm.EndHypotheticalMode();
            return null;
        }
        float EvaluateCurrentState(float alpha, float beta, bool isInitial = false)
        {
            // First, need to see which type of node this is
            // Check if players have wildcards of interest, because if so, I'll need to do a potential discovery phase
            if (_sm.PlayerHasRelevantWildcards(_currentPlayerIndex))
            {
                EvaluateDiscoveryNode(_currentPlayerIndex, alpha, beta);
            }
            else if (_sm.PlayerHasRelevantWildcards(_opposingPlayerIndex))
            {
                EvaluateDiscoveryNode(_opposingPlayerIndex, alpha, beta);
            }
            else
            {
                // Otherwise, it's a good old minmax state
            }
            // Add this state into state LUT if I'm not there already, has null value but it will help avoid loops in the tree
            int stateHash = _sm.DetailedState.GetHashCode();
            if (!_stateLut.ContainsKey(stateHash))
            {
                _stateLut.Add(stateHash, null);
            }
            // Now, ready to evaluate states and actions, create minmax fn, get chain of actions, etc
            return 0f;
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
                score = EvaluateCurrentState(alpha, beta);
            }
            else if (nWildcards >= discoveryCardPool.CardCount) // In this case, all cards can fit in the hand so I just insert all of them
            {
                foreach (KeyValuePair<int, int> cardAndCount in discoveryCardPool.GetCards().ToArray()) // Check all copies of all cards
                {
                    for (int i = 0; i < cardAndCount.Value; i++) // Check how many of this card are there
                    {
                        _sm.DiscoverHypotheticalWildcard(discoveryPlayerIndex, cardAndCount.Key, false); // Continue adding, BUT NEED TO CLOSE THE EVENT QUEUE MANUALLY
                    }
                }
                _sm.CloseEventStack();
                _sm.SetPlayerHasRelevantWildcards(discoveryPlayerIndex, false); // I put all cards, surely they can;t have relevant wildcards
                // Then, analyze this state (it will continue to go deep discovering all other cards)
                score = EvaluateCurrentState(alpha, beta);
                // Undo the multi-discover step I just did (mantain SM consistency)
                _sm.UndoPreviousStep();
            }
            else // In this case, need to evaluate whether some cards are worth discovering, and if so, will need to branch with all available discoveries and weigth by probabilities
            {
                float remainingPercentage = 1.0f; // Remaining chance of the non-discovered cards
                bool theresUninterestingCards = false;
                foreach (int countNumber in discoveryCardPool.CountHistogram.Keys.ToList()) // Check all of the counts in this deck (from highest to lowest)
                {
                    // Evaluate if this number of cards is worth exploring (threshold of 50%)
                    float probability = _calculatorLut.HyperGeometric(discoveryCardPool.CardCount, nWildcards, countNumber);
                    if (probability < 0.5) // If doesn't surpass 50% threshold, then the lower counts will be even less likely
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
                        score += EvaluateCurrentState(alpha, beta) * probability;
                        // Undo the step I just did (mantain SM consistency)
                        _sm.UndoPreviousStep();
                        remainingPercentage -= probability; // If all makes sense, this number has a min value of 0
                    }
                }
                if (theresUninterestingCards) // Finally, in the chance there was some cards that were not considered, need to calculate "the rest"
                {
                    _sm.SetPlayerHasRelevantWildcards(discoveryPlayerIndex, false); // No more interesting wildcards here
                    score += EvaluateCurrentState(alpha, beta) * remainingPercentage; // Add the weighted equivalent to this case
                }
            }
            return score;
        }
    }
}
