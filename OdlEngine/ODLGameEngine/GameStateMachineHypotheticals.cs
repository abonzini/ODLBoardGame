namespace ODLGameEngine
{
    public partial class GameStateMachine // Deals with hypothetical playing and the hypothetical mode (for minmax checkings)
    {
        bool _hypotheticalMode = false; // Whether the state machine is currently in the hypohetical mode or not
        AssortedCardCollection _hypotheticalStoredOpponentHand = null;
        AssortedCardCollection[] _hypotheticalDecks = [null, null];
        int _hypotheticalPlayer;
        readonly bool[] _hasRelevantWildcards = [false, false];
        /// <summary>
        /// Begins hypothetical mode, by setting the opp hand to wildcards, will override draws, and some other stuff
        /// </summary>
        /// <param name="player">Hypothetical player (knows it's deck to make decisions and stuff)</param>
        /// <param name = "hypotheticalOpponentsDeck" > Hypothetical opponent's deck, which will be modified automatically as the model assumes stuff</param>
        public void StartHypotheticalMode(int player, AssortedCardCollection hypotheticalOpponentsDeck)
        {
            _hypotheticalPlayer = player;
            int opposingPlayer = 1 - player; // This is the opposing player
            // Players hands start uninteresting
            _hasRelevantWildcards[player] = false;
            _hasRelevantWildcards[opposingPlayer] = false;
            _hypotheticalStoredOpponentHand = DetailedState.PlayerStates[opposingPlayer].Hand; // Need to save their hand because I'm about to do some wild shit
            // Check opponent's hypothetical new hand
            AssortedCardCollection newOpponentHand = new AssortedCardCollection();
            if (_hypotheticalStoredOpponentHand.CardCount > 0)
            {
                newOpponentHand.AddToCollection(0, _hypotheticalStoredOpponentHand.CardCount); // Replaces opponen'ts whole hand with wildcards
                _hasRelevantWildcards[opposingPlayer] = true;
            }
            DetailedState.PlayerStates[opposingPlayer].Hand = newOpponentHand; // Changes opp hand
            _hypotheticalDecks[_hypotheticalPlayer] = (AssortedCardCollection)DetailedState.PlayerStates[_hypotheticalPlayer].Deck.Clone(); // Get a copy of player's deck which can have its contents altered
            _hypotheticalDecks[1 - _hypotheticalPlayer] = hypotheticalOpponentsDeck;
            HYPOTHETICAL_InitializeOpponentsDeck();
            _hypotheticalMode = true;
            _currentStep.tag = Tag.HYPOTHETICAL; // Sets to hypothetical mode, this is a protection/stopgap for controlled reversion of steps
        }
        /// <summary>
        /// Does a pass to initialise opponent's deck in hypothetical mode
        /// </summary>
        private void HYPOTHETICAL_InitializeOpponentsDeck()
        {
            int opponent = 1 - _hypotheticalPlayer;
            // Information of what opponent doesn't have in deck is for now only on discard pile
            // (To-do, enhance with "known hand" and "known deck" when this is implemented
            foreach (KeyValuePair<int, int> kvp in DetailedState.PlayerStates[opponent].DiscardPile.GetCards())
            {
                _hypotheticalDecks[1 - _hypotheticalPlayer].RemoveFromCollection(kvp.Key, kvp.Value); // The assumed deck now doesn't have these cards as they've been played already
            }
        }
        /// <summary>
        /// Ends hypothetical mode, restores game functionality
        /// </summary>
        public void EndHypotheticalMode()
        {
            // First, revert all that happened here, clean event chain in hypothetical mode
            while (_currentStep.tag != Tag.HYPOTHETICAL)
            {
                UndoPreviousStep(); // Reverts until tag is found
            }
            _currentStep.tag = Tag.NO_TAG; // Removes hypothetical tag
            int opposingPlayer = 1 - _hypotheticalPlayer; // This is the opposing player
            DetailedState.PlayerStates[opposingPlayer].Hand = _hypotheticalStoredOpponentHand; // Returns opp hand
            _hypotheticalDecks = [null, null];
            _hypotheticalMode = false;
        }
        /// <summary>
        /// Forces a wildcard (card with ID=0) to become the desired one. Will use to guess plays
        /// </summary>
        /// <param name="player">Player who will get the wildcard replaced</param>
        /// <param name="card">Card to replace wildcard with</param>
        public StepResult DiscoverHypotheticalWildcard(int player, int card, bool standalone = true)
        {
            // This one requires going through game engine
            Player cardOwner = DetailedState.PlayerStates[player];
            ENGINE_HYPOTHETICAL_RevealWildcard(cardOwner, card);
            // If player still has wildcards at hand, they will be relevant as the hand (and deck) just changed
            bool wildcardIsStillRelevant = cardOwner.Hand.HasCardInCollection(0);
            ENGINE_HYPOTHETICAL_SetWildcardRelevance(cardOwner.Owner, wildcardIsStillRelevant);
            if (standalone)
            {
                // Repeat current state to flush event queue and make it undoable
                ENGINE_ChangeState(DetailedState.CurrentState);
                return _stepHistory.Last(); // Returns everything that happened in this triggering, unneeded but needed to be able to reverse these stuffs
            }
            else
            {
                return null; // Someone may continue adding wildcards, their risk
            }
        }
        /// <summary>
        /// Returns whether a player has relevant wildcards in hand
        /// </summary>
        /// <param name="player">Which players</param>
        /// <returns>Whether it's of interest to analyze a player's wildcards</returns>
        public bool PlayerHasRelevantWildcards(int player)
        {
            return _hasRelevantWildcards[player];
        }
        /// <summary>
        /// Sets whether a player has relevant wildcards in hand, used after analysis of deck contents
        /// WARNING: This function doesn't go through game engine so that it doesn't need to create a new step result and doesn't duplicate states.
        /// However, this is a danger because this function isn't automatically "undone", and therefore can't guarantee Undo() states are consistent.
        /// Rule of thumb is to only use this function as the first thing you do in a state after checking whether you need to check relevant wildcards.
        /// </summary>
        /// <param name="player">Which players</param>
        /// <returns>Whether it's of interest to analyze a player's wildcards</returns>
        public void SetPlayerHasRelevantWildcards(int player, bool isRelevant)
        {
            _hasRelevantWildcards[player] = isRelevant;
        }
        /// <summary>
        /// Returns the card pool of hypothetical's player
        /// </summary>
        /// <param name="player">Which players</param>
        /// <returns>The remaining cardpool to consider</returns>
        public AssortedCardCollection GetPlayersHypotheticalCardPool(int player)
        {
            return _hypotheticalDecks[player];
        }
    }
}
