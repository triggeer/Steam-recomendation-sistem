using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebAppTest.Models;

namespace WebAppTest.Data
{
	public class AppDbContext : DbContext
	{
		protected readonly IConfiguration Configuration;

		//public AppDbContext(IConfiguration configuration)
		//{
		//	Configuration = configuration;
		//}

		public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
		//protected override void OnConfiguring(DbContextOptionsBuilder options)
		//{
		//	// connect to postgres with connection string from app settings
		//	options.UseNpgsql(Configuration.GetConnectionString("DBConnection"));
		//}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Game>().HasIndex(g => g.SteamAppId).IsUnique();
		// задаём ключи для таблиц связей с game
			modelBuilder.Entity<GameGenre>().HasKey(gg => new { gg.GameId, gg.GenreId });
			modelBuilder.Entity<GameTag>().HasKey(gt => new { gt.GameId, gt.TagId });
		// делаем названия в таблицах жанров и тегов уникальными
			modelBuilder.Entity<Genre>().HasIndex(g => g.Name).IsUnique();
			modelBuilder.Entity<Tag>().HasIndex(t => t.Name).IsUnique();
			modelBuilder.Entity<UserProfile>().HasIndex(i => i.Id).IsUnique();
			modelBuilder.Entity<UserProfile>().Property(t => t.TagStrength).HasColumnType("jsonb");

			base.OnModelCreating(modelBuilder);
		}

		public DbSet<Game> Games { get; set; }
		public DbSet<Tag> Tags { get; set; }
		public DbSet<GameTag> GameTags { get; set; }
		public DbSet<Genre> Genres { get; set; }
		public DbSet<GameGenre> GameGenres { get; set; }
		public DbSet<UserProfile> UserProfiles { get; set; }
	}
}
