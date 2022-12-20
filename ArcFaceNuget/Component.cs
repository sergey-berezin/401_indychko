﻿using Microsoft.ML.OnnxRuntime;
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
        /// Tuple with 2 matrix of size N x N and list of embeddings. 
        /// First matrix is distance matrix and another is similarity matrix.
        /// </returns>
        public async Task<(float[,], float[,], List<float[]>)> GetDistanceAndSimilarity(
            Image<Rgb24>[] images, CancellationToken token, IProgress<int> progress)
        {
            float[,] distanceMatrix = new float[images.Length, images.Length];

            float[,] similarityMatrix = new float[images.Length, images.Length];

            try
            {
                List<float[]> embeddings = new();

                progress.Report(0);

                foreach (var image in images)
                {
                    // check cancellation token 
                    CheckToken(token);
                    // get new embegging
                    embeddings.Add(await GetEmbeddings(image, token));
                    // report progress state
                    progress.Report(embeddings.Count * 100 / images.Count());
                }

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

                return (distanceMatrix, similarityMatrix, embeddings);
            }
            catch
            {
                return (new float[0,0], new float[0,0], new List<float[]>());
            }  
        }

        /// <summary>
        /// Method gets N images and calculates embeddings for them.
        /// </summary>
        /// <returns>
        /// List of embeddings. 
        /// </returns>
        public async Task<List<float[]>> GetEmbeddings(Image<Rgb24>[] images)
        {
            List<float[]> embeddings = new();

            foreach (var image in images)
            {
                // get new embegging
                embeddings.Add(await GetEmbeddings(image, null));
            }

            return embeddings;
        }

        /// <summary>
        /// Dispose instance of NN (session).
        /// </summary>
        public void Dispose()
        {
            session.Dispose();
        }

        #region Private methods

        private async Task<float[]> GetEmbeddings(Image<Rgb24> face, CancellationToken? token)
        {
            var newToken =  token ?? new CancellationToken();
            return await Task<float[]>.Factory.StartNew(() =>
            {
                CheckToken(newToken);

                var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("data", ImageToTensor(face)) };

                CheckToken(newToken);

                lock (session)
                {
                    using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs);
                    return Normalize(results.First(v => v.Name == "fc1").AsEnumerable<float>().ToArray());
                }
            }, newToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private void CheckToken(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                token.ThrowIfCancellationRequested();
        }

        private float Length(float[] v) => (float)Math.Sqrt(v.Select(x => x * x).Sum());

        private float[] Normalize(float[] v)
        {
            var len = Length(v);
            return v.Select(x => x / len).ToArray();
        }

        public float Distance(float[] v1, float[] v2) => Length(v1.Zip(v2).Select(p => p.First - p.Second).ToArray());

        public float Similarity(float[] v1, float[] v2) => v1.Zip(v2).Select(p => p.First * p.Second).Sum();

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