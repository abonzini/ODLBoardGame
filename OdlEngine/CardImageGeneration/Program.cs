using CardGenerationHelper;
using Newtonsoft.Json;
using ODLGameEngine;

namespace CardImageGeneration
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string? destinationPath = args.Length > 0 ? args[0] : "";
            string? resourcesPath = args.Length > 1 ? args[1] : "";

            if (string.IsNullOrWhiteSpace(destinationPath))
            {
                Console.Write("Enter the destination path: ");
                destinationPath = Console.ReadLine();
            }
            // Ensure output directories exist
            string generatedPath = Path.Combine(destinationPath, "Generated");
            string cardsPath = Path.Combine(generatedPath, "CardImages");
            string blueprintsPath = Path.Combine(generatedPath, "BlueprintImages");
            if (!Directory.Exists(cardsPath))
            {
                Directory.CreateDirectory(cardsPath);
            }
            if (!Directory.Exists(blueprintsPath))
            {
                Directory.CreateDirectory(blueprintsPath);
            }

            if (string.IsNullOrWhiteSpace(resourcesPath))
            {
                Console.Write("Enter the resources path: ");
                resourcesPath = Console.ReadLine();
            }

            string cardDataPath = Path.Combine(resourcesPath, "CardData");
            string indexFile = Path.Combine(cardDataPath, "index.csv");
            if (!File.Exists(indexFile))
            {
                Console.WriteLine($"index.csv not found at {indexFile}");
                return;
            }

            string[] indices = File.ReadAllLines(indexFile)[0].Split(',');
            int min = int.Parse(indices[0]);
            int max = int.Parse(indices[1]);
            CardFinder cardFinder = new CardFinder(cardDataPath);
            for (int i = min; i <= max; i++)
            {
                string cardPath = Path.Combine(cardsPath, $"{i}.png");
                string bpPath = Path.Combine(blueprintsPath, $"{i}.png");
                if (i == 0) continue; // Skip 0
                string illustrationJson = Path.Combine(cardDataPath, $"{i}-illustration.json");
                CardIllustrationInfo illustrationInfo = JsonConvert.DeserializeObject<CardIllustrationInfo>(File.ReadAllText(illustrationJson));
                EntityBase entity = cardFinder.GetCard(i);
                if (!File.Exists(cardPath))
                {
                    Bitmap theCard = DrawHelper.DrawCard(illustrationInfo, resourcesPath);
                    theCard.Save(cardPath, System.Drawing.Imaging.ImageFormat.Png);
                }
                if (entity.EntityType == EntityType.BUILDING)
                {
                    if (!File.Exists(bpPath))
                    {
                        Bitmap theBp = DrawHelper.DrawBlueprint(illustrationInfo, (Building)entity, resourcesPath);
                        theBp.Save(bpPath, System.Drawing.Imaging.ImageFormat.Png);
                    }
                }
            }
        }
    }
}
