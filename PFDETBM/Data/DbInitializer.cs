using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PFDETBM.Models;

namespace PFDETBM.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var configuration = services.GetRequiredService<IConfiguration>();
            var environment = services.GetRequiredService<IHostEnvironment>();

            await context.Database.EnsureCreatedAsync();

            var roles = new[] { "Admin", "User" };
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var adminPassword = configuration["BootstrapAdmin:Password"];
            if (environment.IsDevelopment())
            {
                adminPassword ??= "Admin123$";
            }

            if (!string.IsNullOrWhiteSpace(adminPassword))
            {
                var adminEmail = configuration["BootstrapAdmin:Email"] ?? "admin@pfdetbm.local";
                var adminUserName = configuration["BootstrapAdmin:Username"] ?? adminEmail;
                var adminFullName = configuration["BootstrapAdmin:FullName"] ?? adminUserName;
                var admin = await userManager.FindByEmailAsync(adminEmail);
                if (admin == null)
                {
                    admin = new ApplicationUser
                    {
                        UserName = adminUserName,
                        Email = adminEmail,
                        EmailConfirmed = true,
                        FullName = adminFullName
                    };
                    var result = await userManager.CreateAsync(admin, adminPassword);
                    if (!result.Succeeded)
                    {
                        var errors = string.Join(", ", result.Errors.Select(error => error.Description));
                        throw new InvalidOperationException($"Unable to create the bootstrap admin account: {errors}");
                    }
                }

                if (!string.Equals(admin.UserName, adminUserName, StringComparison.Ordinal))
                {
                    var userNameResult = await userManager.SetUserNameAsync(admin, adminUserName);
                    if (!userNameResult.Succeeded)
                    {
                        var errors = string.Join(", ", userNameResult.Errors.Select(error => error.Description));
                        throw new InvalidOperationException($"Unable to set the bootstrap admin username: {errors}");
                    }
                }

                admin.FullName = adminFullName;
                await userManager.UpdateAsync(admin);

                if (!await userManager.IsInRoleAsync(admin, "Admin"))
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }

            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Salary", IsExpense = false },
                    new Category { Name = "Investments", IsExpense = false },
                    new Category { Name = "Freelance", IsExpense = false },
                    new Category { Name = "Groceries", IsExpense = true },
                    new Category { Name = "Food", IsExpense = true },
                    new Category { Name = "Rent", IsExpense = true },
                    new Category { Name = "Utilities", IsExpense = true },
                    new Category { Name = "Transportation", IsExpense = true },
                    new Category { Name = "Dining", IsExpense = true },
                    new Category { Name = "Entertainment", IsExpense = true }
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }
        }
    }
}
