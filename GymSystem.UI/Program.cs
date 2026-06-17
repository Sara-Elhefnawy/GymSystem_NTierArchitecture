using GymSystem.Domain;
using GymSystem.Infrastructure;
using GymSystem.Infrastructure.Seeders;
using GymSystem.UI;

var builder = WebApplication.CreateBuilder(args);

// Add services using the DependencyInjection class
builder.Services
    .AddInfrastructureServices(builder.Configuration)
    .AddUIServices()
    .AddDomainServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapDefaultControllerRoute();

app.UseAuthentication();   // decrypt cookie and create the ClaimPrincipal(User)
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAllAsync();
}

app.Run();
