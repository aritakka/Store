using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Store.Data;
using Store.Models;
using Store.Services;

var builder = WebApplication.CreateBuilder(args);

// ===== Add services =====
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

// Password hasher
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();

var app = builder.Build();

// ===== Initialize DB & Seed =====
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();

    try
    {
        db.Database.Migrate();

        if (!db.Roles.Any())
        {
            db.Roles.AddRange(
                new Role { Name = "Customer" },
                new Role { Name = "Admin" },
                new Role { Name = "Supplier" }
            );
            db.SaveChanges();
        }

        var hasher = services.GetRequiredService<IPasswordHasher<User>>();
        var supplierRole = db.Roles.First(r => r.Name == "Supplier");
        var customerRole = db.Roles.First(r => r.Name == "Customer");

        if (!db.Users.Any(u => u.UserName == "supplier1"))
        {
            var supplierUser = new User
            {
                UserName = "supplier1",
                Email = "supplier1@mail.com",
                RoleId = supplierRole.Id
            };
            supplierUser.PasswordHash = hasher.HashPassword(supplierUser, "123");
            db.Users.Add(supplierUser);
        }

        if (!db.Users.Any(u => u.UserName == "customer1"))
        {
            var customerUser = new User
            {
                UserName = "customer1",
                Email = "customer1@mail.com",
                RoleId = customerRole.Id
            };
            customerUser.PasswordHash = hasher.HashPassword(customerUser, "123");
            db.Users.Add(customerUser);
        }

        db.SaveChanges();

        DbInitializer.Initialize(db);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ошибка при инициализации базы данных");
    }
}

// ===== Middleware =====
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

// ===== Map routes =====
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();