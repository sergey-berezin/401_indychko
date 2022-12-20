using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;
using ArcFaceNuget;
using System.ComponentModel.DataAnnotations;
using WPFArcFaceApi.DTO;

namespace WPFArcFaceApi.Controllers
{
    [ApiController]
    [Route("api/arcFace")]
    public class ArcFaceController : ControllerBase
    {
        private readonly ILogger<ArcFaceController> _logger;
        private readonly Component arcFaceComponent = new();


        public ArcFaceController(ILogger<ArcFaceController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Counts embedding for image and adds info about image in database, 
        /// if it does not already exists.
        /// </summary>
        /// <returns>
        /// ID (int) of image in database.
        /// </returns>
        [HttpPost("images")]
        [ProducesResponseType(typeof(int), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> AddImage([FromBody, Required] SaveImageToDbRequest imageData)
        {
            var title = imageData.Title;
            var bytes = imageData.Image;

            if ((imageData.Image is null) || string.IsNullOrEmpty(title))
            {
                _logger.LogError("User request is wrong: image or title are null");

                return BadRequest();
            }

            _logger.LogInformation($"User adds image with title '{title}' to database.");

            // check if image exists in database
            var imageFromDb = FindImageInDatabase(bytes);

            if (imageFromDb == null)
            {
                // if image does not exist in Db => count embeddings
                var embedding = await CountImageEmbedding(bytes);

                // save image to database
                var newImageId = SaveImageToDatabase(bytes, title, embedding);

                _logger.LogInformation($"Image with title '{title}' was added to " +
                                       $"database (id = {newImageId})");

                return Ok(newImageId);
            }
            else
            {
                // if image is already in Db => return its ID
                _logger.LogInformation($"Image with title '{title}' is already in " +
                                       $"database with id = {imageFromDb.Id}");
                return Ok(imageFromDb.Id);
            }
        }

        /// <summary>
        /// Gets all images from database.
        /// </summary>
        /// <returns>
        /// List of images IDs.
        /// </returns>
        [HttpGet("images")]
        [ProducesResponseType(typeof(List<int>), 200)]
        public ActionResult GetImagesList()
        {
            _logger.LogInformation("User gets all images ids from database.");

            using var database = new ImageDatabase();

            var result = database.Faces.Select(img => img.Id).ToList();

            _logger.LogInformation($"GetImagesList result contains {result.Count} " +
                $"images with IDs: {String.Join(' ', result.Select(id => id.ToString()).ToArray())}");

            return Ok(result);
        }

        /// <summary>
        /// Gets image info by its ID.
        /// </summary>
        /// <returns>
        /// Info about image (<see cref="ImageInDb"/> )
        /// </returns>
        [HttpGet("images/id")]
        [ProducesResponseType(typeof(ImageInDb), 200)]
        [ProducesResponseType(404)]
        public ActionResult GetImageInfo([FromQuery, Required] int id)
        {
            _logger.LogInformation($"User gets image info by id = {id} from database.");

            using var database = new ImageDatabase();

            var result = database.Faces.Where(img => img.Id == id).FirstOrDefault();

            if (result != null)
            {
                _logger.LogInformation($"For image with id = {id} " +
                    $"find embedding: {result.Embedding[..10]}...");

                return Ok(result);
            }

            _logger.LogError($"Image with id = {id} does not exist in database");

            return NotFound();
        }

        /// <summary>
        /// Deletes all the images from database.
        /// </summary>
        [HttpDelete("images")]
        [ProducesResponseType(200)]
        public ActionResult CleanDatabase()
        {
            _logger.LogInformation("User deletes all images from database.");

            using var database = new ImageDatabase();

            foreach (var image in database.Faces)
            {
                database.Faces.Remove(image);
            }

            database.SaveChanges();

            return Ok();
        }

        #region Private methods and variables

        private ImageInDb? FindImageInDatabase(byte[] imageBytes)
        {
            using var database = new ImageDatabase();

            // get image hash for faster finding
            var hash = GetByteArrayHashCode(imageBytes);

            return database.Faces.Where(item => item.Hash == hash)
                                 .Where(item => Enumerable.SequenceEqual(item.Image, imageBytes))
                                 .SingleOrDefault();
        }

        private string GetByteArrayHashCode(byte[] array)
        {
            // compute the hash
            byte[] data = SHA256.Create().ComputeHash(array);

            // create a new Stringbuilder to collect the bytes
            // and create a string.
            var sBuilder = new StringBuilder();

            // loop through each byte of the hashed data
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // return the hexadecimal string.
            return sBuilder.ToString();
        }

        private async Task<float[]> CountImageEmbedding(byte[] imageBytes)
        {
            using var image = Image.Load<Rgb24>(imageBytes);

            image.Mutate(x => x.Resize(112, 112));

            var embeddings = await arcFaceComponent.GetEmbeddings(new Image<Rgb24>[] { image });

            return embeddings[0];
        }

        private int SaveImageToDatabase(byte[] imageBytes, string title, float[] embedding)
        {
            using var database = new ImageDatabase();

            ImageInDb image = new()
            {
                Hash = GetByteArrayHashCode(imageBytes),
                Image = imageBytes,
                Title = title,
                Embedding = GetStringFromEmbedding(embedding)
            };

            database.Add(image);
            database.SaveChanges();

            return image.Id;
        }

        private static string GetStringFromEmbedding(float[] emb)
        {
            return string.Join(' ', emb);
        }
        #endregion

    }
}