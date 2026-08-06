using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using WebAppTest.Data;
using WebAppTest.Interfaces;
using WebAppTest.Services;

var builder = WebApplication.CreateBuilder(args);

//"ConnectionStrings": {
//	"DBConnection": "Host=localhost; Port=5432; Database=test; Username=postgres; Password=123"
//	}

var connectionString = builder.Configuration.GetValue<string>("ConnectionStrings:DBConnection");

var dataSourceBuilder =
	new NpgsqlDataSourceBuilder(connectionString);

dataSourceBuilder.EnableDynamicJson();

var dataSource = dataSourceBuilder.Build();


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient<ISteamService, SteamService>();
builder.Services.AddHttpClient<IImportService, ImportService>();
builder.Services.AddHttpClient<IDataGainService, DataGainService>();
builder.Services.AddHttpClient<ICreateService,  CreateService>();
builder.Services.AddHttpClient<IUserService, UserService>();
builder.Services.AddHttpClient<IRecommendationService, RecommendationService>();
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
