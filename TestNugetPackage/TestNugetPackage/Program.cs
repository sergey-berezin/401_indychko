using ArcFaceNuget;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;


using var comp = new Component();

var cancToken = new CancellationToken();

using var face1 = Image.Load<Rgb24>("..\\..\\..\\..\\face1.png");
using var face2 = Image.Load<Rgb24>("..\\..\\..\\..\\face2.png");

var result = await comp.GetDistanceAndSimilarity(new Image<Rgb24>[] { face1, face2 }, cancToken);

PrintMatrix(result.Item1, "Distance Matrix");
PrintMatrix(result.Item2, "Similarity matrix");


void PrintMatrix(float[,] matrix, string name)
{
    Console.WriteLine(name);
    for (int i = 0; i < matrix.GetLength(0); i++)
    {
        for (int j = 0; j < matrix.GetLength(1); j++)
        {
            Console.Write(matrix[i, j]);
            Console.Write(' ');
        }
        Console.WriteLine();
    }
    Console.WriteLine();
}
