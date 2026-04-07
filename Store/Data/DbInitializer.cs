using Microsoft.EntityFrameworkCore;
using Store.Models;
using System;
using System.Linq;

namespace Store.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.Migrate();

            // ======= Roles =======
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role { Name = "Admin" },
                    new Role { Name = "Customer" },
                    new Role { Name = "Supplier" }
                );
                context.SaveChanges();
            }

            // ======= Supplier User =======
            var supplierUser = context.Users.FirstOrDefault(u => u.UserName == "supplier");
            var supplierRole = context.Roles.FirstOrDefault(r => r.Name == "Supplier");

            if (supplierUser == null)
            {
                supplierUser = new User
                {
                    UserName = "supplier",
                    Email = "supplier@mail.com",
                    RoleId = supplierRole?.Id
                };

                // ❗ Используем встроенный PasswordHasher
                var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
                supplierUser.PasswordHash = hasher.HashPassword(supplierUser, "1"); // пароль "1"

                context.Users.Add(supplierUser);
                context.SaveChanges();
            }
            else
            {
                // Если пользователь существует, но роли нет — привяжем
                if (supplierUser.RoleId != supplierRole?.Id)
                {
                    supplierUser.RoleId = supplierRole?.Id;
                    context.Update(supplierUser);
                    context.SaveChanges();
                }
            }

            // ======= Suppliers =======
            if (!context.Suppliers.Any(s => s.Name == "ООО Поставщик 1"))
            {
                var sup = new Supplier { Name = "ООО Поставщик 1", ContactInfo = "+370600001" };
                context.Suppliers.Add(sup);
                context.SaveChanges();
            }

            // ======= Остальные инициализации (Categories, Products и т.д.) =======
            // Можно оставить как есть из твоей текущей версии
        }
    }
}