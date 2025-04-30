using ODLGameEngine;
using Newtonsoft.Json;
using System;

namespace DeserializationVerifier
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Which card to try and deserialize?");
            int cardId = int.Parse(Console.ReadLine());
            DeserializationHelper helper = new DeserializationHelper();
            if(helper.IsJsonValid(cardId))
            {
                Console.WriteLine(helper.GetJsonBack(cardId));
            }

            Console.ReadLine();
        }
    }
}
