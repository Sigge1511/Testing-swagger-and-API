// DataSeeder.cs
using apiv4.Data;
using apiv4.Models;
using apiv4.Constants; // LÄGG TILL DENNA USING
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace apiv4.SeedData
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            // Skapar ett scope för att hantera livslängden på de injicerade tjänsterna
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApiContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApiUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // ====================================================================
            // 1. Roll-Seeding
            // Använder konstanten för User-rollen
            string userRole = ApiRole.User;
            string adminRole = ApiRole.Admin;
            string superUserRole = ApiRole.SuperUser;
            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRole));
            }
            if (!await roleManager.RoleExistsAsync(userRole))
            {
                await roleManager.CreateAsync(new IdentityRole(userRole));
            }
            if (!await roleManager.RoleExistsAsync(superUserRole))
            {
                await roleManager.CreateAsync(new IdentityRole(superUserRole));
            }

            // ====================================================================
            // 2. Användar-Seeding
            string defaultUserName = "Sigge";
            string defaultPassword = "VougeDiva.91";
            var user = await userManager.FindByNameAsync(defaultUserName);
            // KONTROLL: Se till att användaren INTE redan finns
            if (user == null)
            {
                // ... (resten av användarobjektet)
                var defaultUser = new ApiUser
                {
                    UserName = "tinydancer@test.com",
                    Email = "tinydancer@test.com",
                    FirstName = "Sigge",
                    LastName = "Diva",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(defaultUser, defaultPassword);
                if (result.Succeeded)
                {
                    // Lägg till användaren i 'User'-rollen, använder ApiRole.User
                    await userManager.AddToRoleAsync(defaultUser, ApiRole.User);
                }
            }

            //********************************************
            // 2. Användar-Seeding
            string defaultUserName2 = "Märta";
            string defaultPassword2 = "MjukaÖron.19";
            var user2 = await userManager.FindByNameAsync(defaultUserName);
            // KONTROLL: Se till att användaren INTE redan finns
            if (user2 == null)
            {
                // ... (resten av användarobjektet)
                var defaultUser2 = new ApiUser
                {
                    UserName = "marta@test.com",
                    Email = "marta@test.com",
                    FirstName = "Märta",
                    LastName = "Söt",
                    EmailConfirmed = true
                };
                var result2 = await userManager.CreateAsync(defaultUser2, defaultPassword2);
                if (result2.Succeeded)
                {
                    await userManager.AddToRoleAsync(defaultUser2, ApiRole.SuperUser);
                }
            }


            // ====================================================================
            // 3. Bok-Seeding 
            if (!await context.BookSet.AnyAsync())
            {
                var books = new List<Book>
                {
                    new Book { Title = "It", Author = "Stephen King", YearPublished = 1986 },
                    new Book { Title = "The Shining", Author = "Stephen King", YearPublished = 1977 },
                    new Book { Title = "The Stand", Author = "Stephen King", YearPublished = 1978 },
                    new Book { Title = "Carrie", Author = "Stephen King", YearPublished = 1974 },
                    new Book { Title = "11/22/63", Author = "Stephen King", YearPublished = 2011 }
                };

                await context.BookSet.AddRangeAsync(books);
                await context.SaveChangesAsync();
            }
        }
    }
}