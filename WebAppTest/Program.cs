using WebAppTest.Data;
using WebAppTest.Interfaces;
using WebAppTest.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient<ISteamService, SteamService>();
builder.Services.AddHttpClient<IImportService, ImportService>();
builder.Services.AddHttpClient<IDataGainService, DataGainService>();
builder.Services.AddHttpClient<ICreateService,  CreateService>();
builder.Services.AddHttpClient<IUserService, UserService>();
builder.Services.AddHttpClient<IRecommendationService, RecommendationService>();
builder.Services.AddDbContext<AppDbContext>();

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

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}")
	.WithStaticAssets();


app.Run();
