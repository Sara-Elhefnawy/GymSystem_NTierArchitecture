using GymSystem.Domain.Interfaces;
using GymSystem.Infrastructure.Data;
using GymSystem.Infrastructure.Seeders;
using GymSystem.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<GymAppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register IAppDbContext
builder.Services.AddScoped<IAppDbContext, GymAppDbContext>();

// Register services
builder.Services.AddScoped<CheckInService>();

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<DatabaseSeeder>();

var app = builder.Build();

app.UseRouting();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Run Seeder
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAllAsync();
}

app.Run();
