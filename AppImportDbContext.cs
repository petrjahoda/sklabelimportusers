using Microsoft.EntityFrameworkCore;

namespace sklabelimportusers {
    public class AppImportDbContext : DbContext {
        private static readonly string SqlConnectionString =
            $"Server={Program._importDatabaseIpAddress};Database={Program._importDatabaseName};User Id={Program._importDatabaseUsername};Password={Program._importDatabasePassword}";

        private static readonly string MysqlConnectionString =
            $"server={Program._importDatabaseIpAddress};port={Program._importDatabasePort};database={Program._importDatabaseName};user={Program._importDatabaseUsername};password={Program._importDatabasePassword};";

        private static readonly string PostgreSqlConnectionString =
            $"Host={Program._importDatabaseIpAddress};Database={Program._importDatabasePort};Username={Program._importDatabaseName};Password={Program._importDatabasePassword};";

        private static readonly string AzureConnectionString =
            $"Server=tcp:{Program._importDatabaseIpAddress},{Program._importDatabasePort};Initial Catalog={Program._importDatabaseName};Persist Security Info=False;User ID={Program._importDatabaseUsername};Password={Program._importDatabasePassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;;";


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            if (Program._importDatabaseType.Contains("sqlserver")) {
                optionsBuilder.UseSqlServer(SqlConnectionString);
            } else if (Program._importDatabaseType.Contains("mariadb")) {
                optionsBuilder.UseMySQL(MysqlConnectionString);
            } else if (Program._importDatabaseType.Contains("postgres")) {
                optionsBuilder.UseNpgsql(PostgreSqlConnectionString);
            } else if (Program._importDatabaseType.Contains("azure")) {
                optionsBuilder.UseSqlServer(AzureConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);
            UpdateDatabaseStructure(builder);
        }

        private void UpdateDatabaseStructure(ModelBuilder builder) {
            MapNecessaryColumns(builder);
//            MapOptionalColumns(builder);
        }



        private static void MapNecessaryColumns(ModelBuilder builder) {
            builder.Entity<UserImport>()
                .ToTable(Program._importDatabaseTable);
            builder.Entity<UserImport>()
                .Property(user => user.OID)
                .HasColumnName(Program._importDatabaseUserIdColumn);
            builder.Entity<UserImport>()
                .Property(user => user.FirstName)
                .HasColumnName(Program._importDatabaseUserFirstNameColumn);
            builder.Entity<UserImport>()
                .Property(user => user.Name)
                .HasColumnName(Program._importDatabaseUserSurnameColumn);
            builder.Entity<UserImport>()
                .Property(user => user.Rfid)
                .HasColumnName(Program._importDatabaseUserRfidColumn);
        }
        
        //        private static void MapOptionalColumns(ModelBuilder builder) {
//            if (Program._importDatabaseUserTypeColumn.Length > 0) {
//                builder.Entity<User>()
//                    .Property(user => user.Type)
//                    .HasColumnName(Program._importDatabaseUserTypeColumn);
//            } else {
//                builder.Entity<User>()
//                    .Ignore(user => user.Type);
//            }
//            if (Program._importDatabaseUserEmailColumn.Length > 0) {
//                builder.Entity<User>()
//                    .Property(user => user.Email)
//                    .HasColumnName(Program._importDatabaseUserEmailColumn);
//            } else {
//                builder.Entity<User>()
//                    .Ignore(user => user.Email);
//            }
//            if (Program._importDatabaseUserPhoneColumn.Length > 0) {
//                builder.Entity<User>()
//                    .Property(user => user.Phone)
//                    .HasColumnName(Program._importDatabaseUserPhoneColumn);
//            } else {
//                builder.Entity<User>()
//                    .Ignore(user => user.Phone);
//            }
//        }


        public DbSet<UserImport> UserImport { get; set; }
    }
}