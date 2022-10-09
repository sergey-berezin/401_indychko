using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ArcFaceNuget
{
    public class Component : IDisposable
    {
        private InferenceSession session;

        private static readonly string arcFaceModelPath = "ArcFaceNuget.arcfaceresnet100-8.onnx";

        /// <summary>
        /// Initialize session to work with ArcFace NN.
        /// </summary>
        public Component()
        {
            var assembly = typeof(Component).Assembly;
            using var modelStream = assembly.GetManifestResourceStream(arcFaceModelPath);
            if (modelStream == null)
                throw new Exception("Embedded resource is not loaded!");

            using var memoryStream = new MemoryStream();
            modelStream.CopyTo(memoryStream);

            session = new InferenceSession(memoryStream.ToArray());

            if (session == null)
                throw new Exception("Model is not loaded correclty!");
        }

        /// <summary>
        /// Method gets N images and calculates distance and similarity between every two images.
        /// </summary>
        /// <returns>
        /// Tuple with 2 matrix of size N x N. First matrix is distance matrix and another is similarity matrix.
        /// </returns>
        public async Task<(float[,], float[,])> GetDistanceAndSimilarity(Image<Rgb24>[] images, CancellationToken token)
        {
            float[,] distanceMatrix = new float[images.Length, images.Length];

            float[,] similarityMatrix = new float[images.Length, images.Length];

            try
            {
                CheckToken(token);
                var tasks = new List<Task<float[]>>();

                Array.ForEach(images, image => tasks.Add(GetEmbeddings(image, token)));

                float[][] embeddings = await Task.WhenAll(tasks);

                int i = 0;

                foreach (var emb1 in embeddings)
                {
                    int j = 0;
                    foreach (var emb2 in embeddings)
                    {
                        distanceMatrix[i, j] = Distance(emb1, emb2);
                        similarityMatrix[i, j] = Similarity(emb1, emb2);
                        j++;
                    }
                    i++;
                }

                return (distanceMatrix, similarityMatrix);
            }
            catch
            {
                return (new float[0,0], new float[0,0]);
            }  
        }

        /// <summary>
        /// Dispose instance of NN (session).
        /// </summary>
        public void Dispose()
        {
            session.Dispose();
        }

        #region Private methods

        private async Task<float[]> GetEmbeddings(Image<Rgb24> face, CancellationToken token)
        {
            return await Task<float[]>.Factory.StartNew(() =>
            {
                CheckToken(token);

                var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("data", ImageToTensor(face)) };

                CheckToken(token);

                lock (session)
                {
                    using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs);
                    return Normalize(results.First(v => v.Name == "fc1").AsEnumerable<float>().ToArray());
                }
            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private void CheckToken(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                token.ThrowIfCancellationRequested();
        }

        private string MetadataToString(NodeMetadata metadata)
            => $"{metadata.ElementType}[{String.Join(",", metadata.Dimensions.Select(i => i.ToString()))}]";

        private float Length(float[] v) => (float)Math.Sqrt(v.Select(x => x * x).Sum());

        private float[] Normalize(float[] v)
        {
            var len = Length(v);
            return v.Select(x => x / len).ToArray();
        }

        private float Distance(float[] v1, float[] v2) => Length(v1.Zip(v2).Select(p => p.First - p.Second).ToArray());

        private float Similarity(float[] v1, float[] v2) => v1.Zip(v2).Select(p => p.First * p.Second).Sum();

        private DenseTensor<float> ImageToTensor(Image<Rgb24> img)
        {
            var w = img.Width;
            var h = img.Height;
            var t = new DenseTensor<float>(new[] { 1, 3, h, w });

            img.ProcessPixelRows(pa =>
            {
                for (int y = 0; y < h; y++)
                {
                    Span<Rgb24> pixelSpan = pa.GetRowSpan(y);
                    for (int x = 0; x < w; x++)
                    {
                        t[0, 0, y, x] = pixelSpan[x].R;
                        t[0, 1, y, x] = pixelSpan[x].G;
                        t[0, 2, y, x] = pixelSpan[x].B;
                    }
                }
            });

            return t;
        }

        #endregion
    }
}