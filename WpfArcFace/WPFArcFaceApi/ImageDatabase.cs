using Microsoft.EntityFrameworkCore;
using WPFArcFaceApi.DTO;

namespace WPFArcFaceApi
{
    public class ImageDatabase: DbContext
    {
        // property which holds set of Face
        public DbSet<ImageInDb> Faces => Set<ImageInDb>();
        // when creating context, we also create database if it does not exist
        public ImageDatabase() => Database.EnsureCreated();

        // configure connection string to DB
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source = images.db");
        }
    }
}
