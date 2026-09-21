using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using WebAppTest.Data;
using WebAppTest.Interfaces;
using WebAppTest.Services;
using WebAppTest.Services.Recommendation;
using WebAppTest.Services.User;


var builder = WebApplication.CreateBuilder(args);

//"ConnectionStrings": {
//	"DBConnection": "Host=localhost; Port=5432; Database=test; Username=postgres; Password=123"
//	}

var connectionString = builder.Configuration.GetValue<string>("ConnectionStrings:DBConnection");

var dataSourceBuilder =
	new NpgsqlDataSourceBuilder(connectionString);

dataSourceBuilder.EnableDynamicJson();

var dataSource = dataSourceBuilder.Build();
//
//builder.Services.AddScoped<IgameService, gameService>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IImportService, ImportService>();
builder.Services.AddScoped<IUserListService, UserListService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddHttpClient<IDataGainService, DataGainService>();

//builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddDbContext<AppDbContext>(options =>	options.UseNpgsql(dataSource));

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(30);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
	var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>(); // Укажите ваш DbContext
	dbContext.Database.Migrate(); // Эта строка сама создаст БД и применит все новые миграции
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}")
	.WithStaticAssets();


app.Run();

