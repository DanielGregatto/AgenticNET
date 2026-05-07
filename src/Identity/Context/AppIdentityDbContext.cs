using Identity.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
namespace Identity.Context
{
    public class AppIdentityDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly string _schema;
        private readonly string _defaultSchema;
        private readonly IConfiguration _configuration;

        public AppIdentityDbContext(
            DbContextOptions<AppIdentityDbContext> options,
            IConfiguration configuration) : base(options)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _schema = _configuration["Schema"];
            _defaultSchema = _configuration["DefaultSchema"];
        }

        public DbSet<UserRefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema(_defaultSchema);

            modelBuilder.Entity<ApplicationUser>().ToTable($"{this._schema}AspNetUsers");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable($"{this._schema}AspNetUserRoles");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable($"{this._schema}AspNetUserLogins");
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable($"{this._schema}AspNetUserClaims");
            modelBuilder.Entity<IdentityRole>().ToTable($"{this._schema}AspNetRoles");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable($"{this._schema}AspNetRoleClaims");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable($"{this._schema}AspNetUserTokens");
            modelBuilder.Entity<UserRefreshToken>().ToTable($"{this._schema}UserRefreshToken");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connection = _configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrEmpty(connection))
                    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

                optionsBuilder.UseSqlServer(connection);
            }
        }
    }
}
