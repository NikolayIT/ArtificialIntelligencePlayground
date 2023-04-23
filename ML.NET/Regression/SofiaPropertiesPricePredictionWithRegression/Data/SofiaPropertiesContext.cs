namespace SofiaPropertiesPricePredictionWithRegression.Data
{
    using Microsoft.EntityFrameworkCore;

    public class SofiaPropertiesContext : DbContext
    {
        private readonly string connectionString;

        public SofiaPropertiesContext(string connectionString)
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

        public DbSet<Property> SofiaProperties { get; set; }
    }
}
