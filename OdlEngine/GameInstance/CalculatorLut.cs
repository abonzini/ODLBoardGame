using System.Collections.Concurrent;

namespace GameInstance
{
    /// <summary>
    /// Class that allows for complex calculation cache, to be reused to make exploration/decision faster
    /// </summary>
    public class CalculatorLut
    {
        readonly ConcurrentDictionary<(int, int, int), float> _hyperGeometricLut = new ConcurrentDictionary<(int, int, int), float>();
        readonly ConcurrentDictionary<(int, int), float> _singleSampleLut = new ConcurrentDictionary<(int, int), float>();
        readonly ConcurrentDictionary<int, float> _sqrtLut = new ConcurrentDictionary<int, float>();
        /// <summary>
        /// Hyper geometric calc. Chance of a hand having a specific card.
        /// </summary>
        /// <param name="DeckSize">Size of deck</param>
        /// <param name="HandSize">Size of hand</param>
        /// <param name="CardCount">How many copies of this card are there</param>
        /// <returns>The cached or calculated result</returns>
        public float HyperGeometric(int DeckSize, int HandSize, int CardCount)
        {
            return _hyperGeometricLut.GetOrAdd((DeckSize, HandSize, CardCount), HyperGeometricCalc);
        }
        /// <summary>
        /// Implementation of hyper geometric calculator
        /// </summary>
        float HyperGeometricCalc((int, int, int) parameters)
        {
            int deckSize = parameters.Item1;
            int handSize = parameters.Item2;
            int cardCount = parameters.Item3;
            float result = 1.0f; // Start with a 100% chance
            if (handSize < deckSize) // Otherwise it was still 100%
            {
                for (int i = 0; i < handSize; i++) // Draw a sample for each card in hand, calculate the chance of NOT drawing
                {
                    result *= (float)(deckSize - cardCount - i) / (deckSize - i); // Each draw, deck shrinks
                }
                result = 1 - result; // Calculate the chance of drawing
            }
            return result;
        }
        /// <summary>
        /// Sample calc. Chance of drawing a card from deck.
        /// </summary>
        /// <param name="DeckSize">Size of deck</param>
        /// <param name="CardCount">How many copies of this card are there</param>
        /// <returns>The cached or calculated result</returns>
        public float SingleSample(int DeckSize, int CardCount)
        {
            return _singleSampleLut.GetOrAdd((DeckSize, CardCount), SingleSampleCalc);
        }
        /// <summary>
        /// Implementation of single sample calculator
        /// </summary>
        float SingleSampleCalc((int, int) parameters)
        {
            int deckSize = parameters.Item1;
            int cardCount = parameters.Item2;
            return (float)cardCount / deckSize;
        }
        /// <summary>
        /// Square root of integer.
        /// </summary>
        /// <param name="N">Number</param>
        /// <returns>The cached or calculated result</returns>
        public float Sqrt(int N)
        {
            return _sqrtLut.GetOrAdd(N, SqrtCalc);
        }
        /// <summary>
        /// Implementation of integer SQRT
        /// </summary>
        float SqrtCalc(int N)
        {
            return (float)Math.Sqrt(N);
        }
    }
}
