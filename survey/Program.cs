using Microsoft.EntityFrameworkCore;
using AlphaSurvey.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// Configure SQL Server database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Database=AlphaSurvey;Trusted_Connection=True;TrustServerCertificate=True;";

builder.Services.AddDbContext<SurveyDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add CORS for API access
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

// Auto-migrate database and seed data on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SurveyDbContext>();
    try
    {
        dbContext.Database.Migrate();
        app.Logger.LogInformation("Database migrated successfully");

        // Seed default use case categories and options
        await SurveyDataSeeder.SeedDefaultUseCasesAsync(dbContext);
        app.Logger.LogInformation("Database seeded successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error migrating/seeding database");
    }
}

app.Run();
