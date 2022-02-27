using Microsoft.EntityFrameworkCore;
using Topo.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register DI services
builder.Services.AddSingleton<StorageService>();

builder.Services.AddScoped<ITerrainAPIService, TerrainAPIService>();
builder.Services.AddScoped<IMemberListService, MemberListService>();
builder.Services.AddScoped<IOASService, OASService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddSqlite<Topo.Data.TopoDBContext>("Data Source=Topo.db");

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
