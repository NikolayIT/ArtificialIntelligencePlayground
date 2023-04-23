namespace DnesBgCommentsBinaryClassification.Data
{
    using Microsoft.EntityFrameworkCore;

    public class DnesBgCommentsContext : DbContext
    {
        private readonly string connectionString;

        public DnesBgCommentsContext(string connectionString)
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

        public DbSet<DnesBgComment> DnesBgComments { get; set; }
    }
}
