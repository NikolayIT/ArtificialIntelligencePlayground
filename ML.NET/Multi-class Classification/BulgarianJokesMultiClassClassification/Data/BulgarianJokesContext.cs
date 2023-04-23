namespace BulgarianJokesMultiClassClassification.Data
{
    using Microsoft.EntityFrameworkCore;

    public class BulgarianJokesContext : DbContext
    {
        private readonly string connectionString;

        public BulgarianJokesContext(string connectionString)
        {
            this.connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        public DbSet<Joke> BulgarianJokes { get; set; }
    }
}
