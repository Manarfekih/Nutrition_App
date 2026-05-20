using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using Recipe_Nutrition_App.Components.Auth;
using Radzen;

using Microsoft.EntityFrameworkCore;
using Recipe_Nutrition_App.Data;
using Recipe_Nutrition_App.Services;
using Recipe_Nutrition_App.Models;

var builder = WebApplication.CreateBuilder(args);

// UI
builder.Services.AddRazorPages();
builder.Services.AddRazorComponents()
       .AddInteractiveServerComponents();

// Auth state
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuthenticationStateProvider, HttpContextAuthenticationStateProvider>();

builder.Services.AddAuthorization();

builder.Services.Configure<SeedAdminOptions>(
    builder.Configuration.GetSection(SeedAdminOptions.SectionName));
builder.Services.AddScoped<RecipeService>();

// DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Services
builder.Services.AddScoped<IngredientService>();

builder.Services.AddRadzenComponents();

var app = builder.Build();

// DB migration
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    await IdentityDataSeeder.SeedAsync(scope.ServiceProvider);
}

// Error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found");

// When running from terminal without an https profile configured, this can warn:
// "Failed to determine the https port for redirect."
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Auth pipeline 
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorPages();
app.MapStaticAssets();

app.MapRazorComponents<Recipe_Nutrition_App.App>()
    .AddInteractiveServerRenderMode();

app.Run();
