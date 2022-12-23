using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using WPFArcFaceApi;
using WPFArcFaceApi.DTO;

namespace WpfArcFace
{
    public class Service
    {
        private AsyncRetryPolicy retryPolicy;

        private static readonly string serverAddres = "https://localhost:7125/api/arcFace/images/";


        public Service()
        {
            retryPolicy = Policy.Handle<HttpRequestException>().WaitAndRetryAsync(
                5, _ => TimeSpan.FromMilliseconds(200));
        }

        public async Task<int> GetImageId(byte[] bytes, string title, CancellationToken token)
        {
            var content = CreateImageDataContent(bytes, title);

            HttpResponseMessage response = new();

            try
            {
                await retryPolicy.ExecuteAsync(async () => {
                    using var client = new HttpClient()
                    {
                        BaseAddress = new Uri(serverAddres)
                    };

                    response = await client.PostAsync("", content, token);

                    response.EnsureSuccessStatusCode();
                });
            }
            catch
            {
                return -1;
            }

            return int.Parse(await response.Content.ReadAsStringAsync(token));
        }

        public async Task<ImageInDb?> GetImageInfoById(int id, CancellationToken token)
        {
            HttpResponseMessage response = new();

            try
            {
                await retryPolicy.ExecuteAsync(async () => {
                    using var client = new HttpClient()
                    {
                        BaseAddress = new Uri(serverAddres)
                    };

                    response = await client.GetAsync($"id?id={id}", token);

                    response.EnsureSuccessStatusCode();
                });
            }
            catch
            {
                return null;
            }

            return JsonConvert.DeserializeObject<ImageInDb>(await response.Content.ReadAsStringAsync(token));
        }

        public async Task<bool> DeleteAllFromDatabase()
        {
            HttpResponseMessage response = new();

            try
            {
                await retryPolicy.ExecuteAsync(async () => {
                    using var client = new HttpClient()
                    {
                        BaseAddress = new Uri(serverAddres)
                    };

                    response = await client.DeleteAsync("");

                    response.EnsureSuccessStatusCode();
                });
            }
            catch
            {
                return false;
            }

            return true;
        }

        public async Task<List<int>?> GetAllImagesIdsFromDatabase()
        {
            HttpResponseMessage response = new();

            try
            {
                await retryPolicy.ExecuteAsync(async () => {
                    using var client = new HttpClient()
                    {
                        BaseAddress = new Uri(serverAddres)
                    };

                    response = await client.GetAsync("");

                    response.EnsureSuccessStatusCode();
                });
            }
            catch
            {
                return null;
            }

            return JsonConvert.DeserializeObject<List<int>>(await response.Content.ReadAsStringAsync());
        }


        #region Private methods

        private static HttpContent CreateImageDataContent(byte[] imageBytes, string title)
        {
            var newData = new SaveImageToDbRequest()
            {
                Image = imageBytes,
                Title = title
            };

            var content = new StringContent(JsonConvert.SerializeObject(newData));

            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return content;
        }

        private static float[] GetEmbeddingFromString(string emb)
        {
            List<float> embedding = new();
            Array.ForEach(emb.Split(' '), token => { embedding.Add(float.Parse(token)); });
            return embedding.ToArray();
        }

        #endregion
    }
}
