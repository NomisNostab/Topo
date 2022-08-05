using NLog;
using NLog.Web;
using Topo.Services;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Info($"Version {Topo.Constants.Version}");

try
{
    // Create AppData folder
    string path = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\Topo");
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
    builder.Services.AddScoped<IMilestoneService, MilestoneService>();
    builder.Services.AddScoped<ILogbookService, LogbookService>();
    builder.Services.AddScoped<IWallchartService, WallchartService>();
    builder.Services.AddScoped<IAdditionalAwardService, AdditionalAwardService>();
    builder.Services.AddScoped<IReportService, ReportService>();
    builder.Services.AddScoped<IApprovalsService, ApprovalsService>();
    Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBMAY9C3t2VVhiQlFadVdJXGFWfVJpTGpQdk5xdV9DaVZUTWY/P1ZhSXxRdkJiUX9YdHZRRGheVkQ=");

    // NLog: Setup NLog for Dependency injection
    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
    builder.Host.UseNLog();

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
}
catch (Exception exception)
{
    // NLog: catch setup errors
    logger.Error(exception, "Stopped program because of exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    NLog.LogManager.Shutdown();
}