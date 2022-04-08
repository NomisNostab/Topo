using Microsoft.EntityFrameworkCore;
using Topo.Services;

// Create AppData folder
string path = @"%LOCALAPPDATA%\Topo";
path = Environment.ExpandEnvironmentVariables(path);
if (!Directory.Exists(path))
    Directory.CreateDirectory(path);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register DI services
builder.Services.AddSingleton<StorageService>();

builder.Services.AddScoped<ITerrainAPIService, TerrainAPIService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IMemberListService, MemberListService>();
builder.Services.AddScoped<IOASService, OASService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<ISIAService, SIAService>();
builder.Services.AddSqlite<Topo.Data.TopoDBContext>($@"Data Source={path}\Topo.db");
builder.Services.AddHttpClient();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
