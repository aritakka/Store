using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Store.Data;
using Store.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// DB
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cookie authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Users/Login";
        options.LogoutPath = "/Users/Logout";
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
    });

// Password hasher for User
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Optional: custom service you added (kept but not required)
builder.Services.AddScoped<Store.Services.IPasswordHasherService, Store.Services.PasswordHasherService>();

var app = builder.Build();

// Apply migrations and ensure required seed data
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();

        // Ensure "Customer" role exists
        if (!db.Roles.Any(r => r.Name == "Customer"))
        {
            db.Roles.Add(new Role { Name = "Customer" });
            db.SaveChanges();
            logger.LogInformation("Seeded role 'Customer'.");
        }
    }
    catch (Exception ex)
    {
        // Log and continue (in development you may want to rethrow)
        logger.LogError(ex, "Error applying migrations or seeding database.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
