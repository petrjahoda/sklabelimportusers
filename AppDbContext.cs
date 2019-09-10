using Microsoft.EntityFrameworkCore;

namespace sklabelimportusers {
    public class AppDbContext : DbContext {
        private static readonly string SqlConnectionString =
            $"Server={Program._databaseIpAddress},{Program._databasePort};Database={Program._databaseName};User Id={Program._databaseUsername};Password={Program._databasePassword}";

        private static readonly string MysqlConnectionString =
            $"server={Program._databaseIpAddress};port={Program._databasePort};database={Program._databaseName};user={Program._databaseUsername};password={Program._databasePassword};";

        private static readonly string PostgreSqlConnectionString =
            $"Host={Program._databaseIpAddress};Database={Program._databaseName};Username={Program._databaseUsername};Password={Program._databasePassword};";

        private static readonly string AzureConnectionString =
            $"Server=tcp:{Program._databaseIpAddress},{Program._databasePort};Initial Catalog={Program._databaseName};Persist Security Info=False;User ID={Program._databaseUsername};Password={Program._databasePassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;;";


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            if (Program._databaseType.Contains("sqlserver")) {
                optionsBuilder.UseSqlServer(SqlConnectionString);
            } else if (Program._databaseType.Contains("mariadb")) {
                optionsBuilder.UseMySQL(MysqlConnectionString);
            } else if (Program._databaseType.Contains("postgres")) {
                optionsBuilder.UseNpgsql(PostgreSqlConnectionString);
            } else if (Program._databaseType.Contains("azure")) {
                optionsBuilder.UseSqlServer(AzureConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);
        }


        public DbSet<user> user { get; set; }
    }
}