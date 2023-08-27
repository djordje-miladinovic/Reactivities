using Microsoft.EntityFrameworkCore;
using Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
ConfigureServices(builder);

var app = builder.Build();

await ApplyDatabaseMigrations(app);

// Configure Middleware and Request Pipeline
ConfigureMiddleware(app);

app.Run();

void ConfigureServices(WebApplicationBuilder webAppBuilder)
{
    webAppBuilder.Services.AddControllers();
    webAppBuilder.Services.AddEndpointsApiExplorer();
    webAppBuilder.Services.AddSwaggerGen();
    webAppBuilder.Services.AddDbContext<DataContext>(options =>
        options.UseSqlite(webAppBuilder.Configuration.GetConnectionString("DefaultConnection")));
}

async Task ApplyDatabaseMigrations(WebApplication host)
{
    using var scope = host.Services.CreateScope();
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<DataContext>();
        await context.Database.MigrateAsync();
        await Seed.SeedData(context);
    }
    catch (Exception e)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(e, "Error during migration!");
        throw;
    }
}

void ConfigureMiddleware(WebApplication host)
{
    // Configure the HTTP request pipeline.
    if (host.Environment.IsDevelopment())
    {
        host.UseSwagger();
        host.UseSwaggerUI();
    }

    // ? host.UseRouting();
    host.UseHttpsRedirection();

    host.UseAuthorization();

    // ? host.UseEndpoints(endpoints => endpoints.MapControllers());
    host.MapControllers();
}