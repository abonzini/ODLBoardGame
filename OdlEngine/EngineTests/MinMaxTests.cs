namespace MinMaxTests
{
    [TestClass]
    public class MinMaxTests
    {
        // Placeholder for now IG
        // Ideas:
        // Pruning if guaranteed win
        // Instant return if only EOT as option
        // A case where you need to play a combo to win
        // A case where you need to assume you have a specific remaining cards that you'll get eventually to win
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
