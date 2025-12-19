using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.VisualStudio.TestPlatform.CoreUtilities.Helpers;
using RenessansAPI.DataAccess.AppDBContexts;
using RenessansAPI.Extensions;
using RenessansAPI.Middlewares;
using RenessansAPI.Service.Helpers;
using RenessansAPI.Service.Hubs;
using RenessansAPI.Service.IService;
using RenessansAPI.Service.Seeders;
using Serilog;
using System;
using System.Text.Json.Serialization;
using EnvironmentHelper = RenessansAPI.Service.Helpers.EnvironmentHelper;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;

// Services
builder.Services.AddServices();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();

// Serilog
var confLogger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(confLogger);
builder.Logging.AddConsole();

//DB
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.WriteIndented = true;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddJwtService(builder.Configuration);
builder.Services.AddSwaggerService();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
EnvironmentHelper.WebRootPath = builder.Environment.WebRootPath;
builder.Services.AddHttpContextAccessor();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// ----------------- Apply Migrations & Seeding ----------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<AppDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        var userService = services.GetRequiredService<IUserService>();
        var permissionService = services.GetRequiredService<IPermissionService>();
        var roleService = services.GetRequiredService<IRoleService>();

        logger.LogInformation("?? Starting database migration and seeding...");

        db.Database.Migrate();
        await DbSeeder.SeedAsync(db, userService, permissionService, roleService, logger);

        logger.LogInformation("? Databasze migration and seeding completed.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();

        logger.LogError(ex, "? Database migration and seeding failed.");
    }
}

if (app.Services.GetService<IHttpContextAccessor>() != null)
    HttpContextHelper.Accessor = app.Services.GetRequiredService<IHttpContextAccessor>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHub<AdminHub>("/hubs/admin");

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
EnvironmentHelper.WebRootPath = app.Services.GetRequiredService<IWebHostEnvironment>().WebRootPath;
EnvironmentHelper.WebRootPath =
    Environment.GetEnvironmentVariable("WEB_ROOT_PATH")
    ?? (env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot"));
Directory.CreateDirectory(EnvironmentHelper.WebRootPath);

app.UseMiddleware<ExceptionHandlerMiddleWare>();
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    RequestPath = "/images",
    FileProvider = new PhysicalFileProvider(
        Path.Combine(app.Environment.WebRootPath, "images"))
});
app.UseMiddleware<LanguageMiddleware>();

app.UseMiddleware<TokenValidationMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("AllowAll");

app.MapControllers();

app.Run();