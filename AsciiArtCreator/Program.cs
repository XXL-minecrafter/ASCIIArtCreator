namespace AsciiArtCreator
{
    public class Program
    {
        /// <summary>
        /// The location of the image, the ASCII art should be generated from.
        /// </summary>
        private static readonly string imageLocation = "E:\\StudiumProjekte\\GP0924\\Portfolio_41_Serve\\2_MonsterkampfSimulator\\src\\UsedSprites\\MonsterSprites\\BulbasaurSpriteFront.png";

        /// <summary>
        /// The location, the generated ASCII art should be saved in. This needs to be a .txt file.
        /// </summary>
        private static readonly string saveLocation = "E:\\StudiumProjekte\\GeneratedSprites.txt";

        static void Main(string[] args)
        {
            AsciiGenerator asciiGenerator = new(imageLocation, saveLocation);

            asciiGenerator.CreateImage(ImageColorType.Default, false);
        }
    }
}
