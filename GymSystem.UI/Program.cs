using GymSystem.Infrastructure.Data;
using GymSystem.Infrastructure.Interceptor;
using GymSystem.Infrastructure.Seeders;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// applying interceptors
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<AuditInterceptor>();

builder.Services.AddDbContext<GymAppDbContext>((serviceProvider, options) =>
{
    var auditInterceptor = serviceProvider.GetRequiredService<AuditInterceptor>();
    options
        .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
        .AddInterceptors(auditInterceptor);
});

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
