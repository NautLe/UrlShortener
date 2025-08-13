using Microsoft.EntityFrameworkCore;
using UrlShortener.Data;
using UrlShortener.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using UrlShortener.Validators;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký MVC
builder.Services.AddControllersWithViews();

// Đăng ký FluentValidation mới
builder.Services.AddFluentValidationAutoValidation(config =>
{
    config.DisableDataAnnotationsValidation = true; // Tắt DataAnnotations
});
builder.Services.AddFluentValidationClientsideAdapters();

// Đăng ký tất cả validator từ assembly chứa ShortenRequestValidator
builder.Services.AddValidatorsFromAssemblyContaining<ShortenRequestValidator>();

// Đăng ký service khác
builder.Services.AddTransient<ShortCodeGenerator>();

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddMemoryCache();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "short",
    pattern: "{shortCode}",
    defaults: new { controller = "Redirect", action = "Go" });

app.Run();
    