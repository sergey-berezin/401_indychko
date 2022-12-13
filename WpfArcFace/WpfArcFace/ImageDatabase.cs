using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WpfArcFace
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

    public class ImageDataBase: DbContext
    {
        // property which holds set of Face
        public DbSet<ImageInDb> Faces => Set<ImageInDb>();
        // when creating context, we also create database if it does not exist
        public ImageDataBase() => Database.EnsureCreated();

        // configure connection string to DB
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=arcfaceapp.db");
        }
    }
}
