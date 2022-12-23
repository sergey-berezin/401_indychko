using System.ComponentModel.DataAnnotations;

namespace WPFArcFaceApi
{
    public class ImageInDb
    {
        [Key]
        public int Id { get; set; }
        public string Hash { get; set; }
        public byte[] Image { get; set; }
        public string Title { get; set; }
        public string Embedding { get; set; }
    }
}
