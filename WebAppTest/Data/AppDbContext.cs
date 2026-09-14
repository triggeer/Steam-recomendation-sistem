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
			modelBuilder.Entity<GameTag>().HasKey(gt => new { gt.GameId, gt.TagId });
		// делаем названия в таблицах жанров и тегов уникальными
			modelBuilder.Entity<Tag>().HasIndex(t => t.Name).IsUnique();
			modelBuilder.Entity<UserProfile>().HasKey(i => i.UserId);
			modelBuilder.Entity<UserProfile>().Property(t => t.TagStrength).HasColumnType("jsonb");
			modelBuilder.Entity<RecList>().HasIndex(i => i.Id).IsUnique();
			modelBuilder.Entity<ListGame>().HasKey(lg => new { lg.ListId, lg.GameId });
			modelBuilder.Entity<ListGame>().HasIndex(lp => new { lp.ListId, lp.GamePosition }).IsUnique();
			modelBuilder.Entity<UserList>().HasKey(ul => new { ul.UserId, ul.ListId});
			modelBuilder.Entity<UserGame>().HasKey(ug => new { ug.UserId, ug.GameId });



			base.OnModelCreating(modelBuilder);
		}

		public DbSet<Game> Games { get; set; }
		public DbSet<Tag> Tags { get; set; }
		public DbSet<GameTag> GameTags { get; set; }
		public DbSet<UserProfile> UserProfiles { get; set; }
		public DbSet<RecList> RecLists { get; set; }
		public DbSet<ListGame> ListGames { get; set; }
		public DbSet<UserList> UserLists { get; set; }
		public DbSet<UserGame> UserGames { get; set; }

	}
}
