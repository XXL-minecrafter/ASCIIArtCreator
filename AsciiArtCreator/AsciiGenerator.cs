using System.Drawing;

/// <summary>
/// The color property that best describes the image to be converted to ASCII art.
/// </summary>
public enum ImageColorType
{
    Default = 0,
    LowContrast = 1,
    HighContrast = 2,
    HighContrastTwo = 3,
    HighColorRange = 4,
};

public class AsciiGenerator
{
    #region Palettes
    //NOTE: Honestly... just try what color works the best for a specific image, the comments are just guidelines,
    //that don't stay true for every type of image.

    private readonly string[] palettes =

        [
        " .░▒▓█", //Default (For Icons)
        " .░▒▒▒▓▓▓████", //This is better for dark images / low contrast
        " .░▒▓▓█", //This is better for high contrast images
        " .░░▒▓█", //This is better for high contrast images
        "  .░░░░▒▒▒▒▓▓▓██████" //This is better for images with higher color range
        ];


    private string paletteToUse = "";
    #endregion

    private readonly string imageLocation;
    private readonly string saveLocation;

    private char[,] imageArray;
    private bool useOutline;

    public AsciiGenerator(string imageLocation, string saveLocation)
    {
        this.imageLocation = imageLocation;
        this.saveLocation = saveLocation;
    }

    /// <summary>
    /// Outputs an ASCII image from the imageLocation specified in the constructor of the AsciiGenerator and outputs it
    /// in the saveLocation specified in the constructor of the AsciiGenerator.
    /// </summary>
    /// <param name="imageColorType">The color type of the image. E.g. low cotrast, high constrast, etc.</param>
    /// <param name="useOutline">Whether or not an outline should be automatically generated for the image</param>
    public void CreateImage(ImageColorType imageColorType, bool useOutline)
    {
        Bitmap? image = new Bitmap(imageLocation);
        if (image == null) return;

        paletteToUse = palettes[(int)imageColorType];

        this.useOutline = useOutline;

        CreateImageArray(image);
        GenerateImage(image);

        if (useOutline) GenerateOutlines();

        SwapPointsAndRemoveAts();
        PrintFinalArray();
    }

    /// <summary>
    /// Creates an array, the size of the image (in pixels) the ASCII art should be generated for.
    /// The size of the array may vary, depending on whether or not an outline should be used.
    /// </summary>
    /// <param name="image">The bitmap of the image, the ASCII art should be generated for</param>
    public void CreateImageArray(Bitmap image)
    {
        int imageHeight = useOutline ? image.Height + 2 : image.Height; //Size +2 because of borders / outline
        int imageWidth = useOutline ? image.Width + 2 : image.Width;

        imageArray = new char[imageHeight, imageWidth];

        //Fill array
        for (int i = 0; i < imageHeight; i++)
        {
            for (int j = 0; j < imageWidth; j++)
            {
                //Fill with @ -> can't use space, because space is used as darkest color in finalImage
                //-> Important when replacing later on
                imageArray[i, j] = '@';
            }
        }
    }

    /// <summary>
    /// Goes through the image, the ASCII art should be generated for and looks at the color
    /// of each pixel to map it to a brightness value, which then gets mapped to the associated
    /// character in the used pixel palette.
    /// </summary>
    /// <param name="image">The bitmap of the image, the ASCII art should be generated for</param>
    public void GenerateImage(Bitmap image)
    {
        //Place image centered in imageArray

        //Loop through each pixel in image
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                Color color = image.GetPixel(x, y);
                double brightness = GetBrightness(color);
                bool isTransparent = color.A == 0;

                //Map brightness value (Number between 0-255) to value in string
                //Brightness / 255 -> get value from 0-1 depending on brightness (1 is pixel.Length - 1) - (0 is pixels[0])

                //Normally
                //Inverse it with ( 1 - ) -> if brightness = 255 (highest brightness) -> 255 / 255 -> 1
                //-> would lead to highest value in pixels, but 255 (brightest value) should be most transparent value in pixels aka lowest
                //int indexInString = (int) Math.Round((1 - (brightness / 255)) * (pixels.Length - 1));
                //Why not here -> dark values (black characters) are displayed as white on console, spaces are displayed as dark

                float index = (float)(brightness / 255 * (paletteToUse.Length - 1));

                int indexInString = (int)Math.Round(index);
                char brightnessChar;

                brightnessChar = isTransparent ? '@' : paletteToUse[indexInString];

                //+1 because because of border/outline (This will perfectly center image)
                if (useOutline) imageArray[y + 1, x + 1] = brightnessChar;
                else imageArray[y, x] = brightnessChar;
            }
        }
    }

    /// <summary>
    /// Automatically generates the outline for an image.
    /// </summary>
    public void GenerateOutlines()
    {
        //Swap @ with outline (up)
        for (int i = 0; i < imageArray.GetLength(0); i++)
        {
            for (int j = 0; j < imageArray.GetLength(1); j++)
            {
                if (i + 1 > imageArray.GetLength(0) - 1) continue; //Would be out of bounds

                if (imageArray[i, j] == '@')
                {
                    if (imageArray[i + 1, j] is ' ' or not ' ' and not '@') //Don't need to check for ³ here, since there won't be any placed yet
                        imageArray[i, j] = '³';
                }
            }
        }

        //Swap @ with outline (down)
        for (int i = 0; i < imageArray.GetLength(0); i++)
        {
            for (int j = 0; j < imageArray.GetLength(1); j++)
            {
                if (i == 0) continue; //Would be out of bounds

                if (imageArray[i, j] == '@')
                {
                    //Use ³ (A symbol that's not in our palette) to prevent cases where █ might already been drawn (if it's in palette), if border is completely white
                    //-> wouldn't draw other outline for sprites like that, if we instantly swap the @ to █
                    if (imageArray[i - 1, j] is ' ' or not ' ' and not '@' and not '³')
                    {
                        imageArray[i, j] = '³';
                    }
                }
            }
        }

        //Swap @ with outline (left)
        for (int i = 0; i < imageArray.GetLength(0); i++)
        {
            for (int j = 0; j < imageArray.GetLength(1); j++)
            {
                if (j + 1 > imageArray.GetLength(1) - 1) continue; //Would be out of bounds

                if (imageArray[i, j] == '@')
                {
                    if (imageArray[i, j + 1] is ' ' or not ' ' and not '@' and not '³')
                    {
                        imageArray[i, j] = '³';
                    }
                }
            }
        }

        //Swap @ with outline (right)
        for (int i = 0; i < imageArray.GetLength(0); i++)
        {
            for (int j = 0; j < imageArray.GetLength(1); j++)
            {
                if (j == 0) continue; //Would be out of bounds

                if (imageArray[i, j] == '@')
                {
                    if (imageArray[i, j - 1] is ' ' or not ' ' and not '@' and not '³')
                    {
                        imageArray[i, j] = '³';
                    }
                }
            }
        }

        //Swap ³ with outline character (█)
        for (int i = 0; i < imageArray.GetLength(0); i++)
        {
            for (int j = 0; j < imageArray.GetLength(1); j++)
            {
                if (imageArray[i, j] == '³')
                {
                    imageArray[i, j] = '█';
                }
            }
        }
    }

    /// <summary>
    /// Swap each '.' and '@' in the generated image array for an empty space.
    /// '@' are used to show transparency and '.' for really dark values, that shouldn't be visible.
    /// </summary>
    public void SwapPointsAndRemoveAts()
    {
        //Swap '.' and '@' with spaces (Invisible character)
        for (int i = 0; i < imageArray.GetLength(0); i++)
        {
            for (int j = 0; j < imageArray.GetLength(1); j++)
            {
                if (imageArray[i, j] == '.' || imageArray[i, j] == '@')
                    imageArray[i, j] = ' ';
            }
        }
    }

    /// <summary>
    /// Prints the final array to the console, and the file we want to use as the save location
    /// for the generated ASCII art. Each character in the image array is printed twice, since
    /// the width of a character is half of it's height, but a pixel should be equal width and height.
    /// Example: (█ vs ██) - NOTE: This doesn't look right in summaries -> see actual summary implementation)
    /// </summary>
    public void PrintFinalArray()
    {
        StreamWriter? writer = new StreamWriter(saveLocation);
        if (writer == null) return;

        //Print Array
        for (int i = 0; i < imageArray.GetLength(0); i++)
        {
            for (int j = 0; j < imageArray.GetLength(1); j++)
            {
                Console.Write(imageArray[i, j]);
                Console.Write(imageArray[i, j]);

                writer.Write(imageArray[i, j]);
                writer.Write(imageArray[i, j]);
            }
            writer.WriteLine(); //Go to next row
            Console.WriteLine();
        }

        writer.Close();
    }

    //Formula is from this post: https://www.nbdtech.com/Blog/archive/2008/04/27/calculating-the-perceived-brightness-of-a-color.aspx
    /// <summary>
    /// Calculates the perceived brightness of a color.
    /// </summary>
    /// <param name="color">The color, the perceived brightness should be calculated for</param>
    /// <returns></returns>
    private static double GetBrightness(Color color)
    {
        return Math.Sqrt(color.R * color.R * .241f + color.G * color.G * .691f + color.B * color.B * 0.068f);
    }
}