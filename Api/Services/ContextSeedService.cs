using IdentityApp.Data;
using IdentityApp.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Api.Services
{
    public class ContextSeedService
    {
        private readonly AppDbContext context;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public ContextSeedService(
            AppDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            this.context = context;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }
        public async Task InitializeContextAsync()
        {

            //Bekleyen bir migration var ise migration yapılır.
            if(context.Database.GetPendingMigrationsAsync().Result.Count() > 0)
            {
                await context.Database.MigrateAsync();
            }
            //herhangi bir role var mı kontrol edilir.
            if (!await roleManager.Roles.AnyAsync())
            {
                await roleManager.CreateAsync(new IdentityRole { Name = SD.SuperAdminRole });
                await roleManager.CreateAsync(new IdentityRole { Name = SD.AdminRole });
                await roleManager.CreateAsync(new IdentityRole { Name = SD.ManagerRole });
                await roleManager.CreateAsync(new IdentityRole { Name = SD.PlayerRole });
            }
            if(!await userManager.Users.AnyAsync())
            {

                //süper admin kullanıcısı => Onur Yurdagelen
                var super_admin = new User
                {
                    FirstName = "Onur",
                    LastName = "Yurdagelen",
                    UserName = "onuryurdagelen@example.com",
                    EmailConfirmed = true,
                    Email = "onuryurdagelen@example.com"
                };
                await userManager.CreateAsync(super_admin, "Pa$$w0rd");
                await userManager.AddToRolesAsync(super_admin, new[] {SD.SuperAdminRole, SD.AdminRole, SD.ManagerRole, SD.PlayerRole });
                await userManager.AddClaimsAsync(super_admin, new Claim[]
                {
                    new Claim(ClaimTypes.Email,super_admin.Email),
                    new Claim(ClaimTypes.GivenName,super_admin.FirstName),
                    new Claim(ClaimTypes.Surname,super_admin.LastName),
                });

                //süper admin kullanıcısı => Bekir Yurdagelen
                var super_admin2 = new User
                {
                    FirstName = "Bekir",
                    LastName = "Yurdagelen",
                    UserName = "bekiryurdagelen@example.com",
                    EmailConfirmed = true,
                    Email = "bekiryurdagelen@example.com"
                };
                await userManager.CreateAsync(super_admin2, "Pa$$w0rd");
                await userManager.AddToRolesAsync(super_admin2, new[] { SD.SuperAdminRole, SD.AdminRole, SD.ManagerRole, SD.PlayerRole });
                await userManager.AddClaimsAsync(super_admin2, new Claim[]
                {
                    new Claim(ClaimTypes.Email,super_admin2.Email),
                    new Claim(ClaimTypes.GivenName,super_admin2.FirstName),
                    new Claim(ClaimTypes.Surname,super_admin2.LastName),
                });

                //admin kullanıcısı => Kaan Yurdagelen
                var admin = new User
                {
                    FirstName = "Kaan",
                    LastName = "Yurdagelen",
                    UserName = "kaanyurdagelen@example.com",
                    EmailConfirmed = true,
                    Email = "kaanyurdagelen@example.com"
                };
                await userManager.CreateAsync(admin, "Pa$$w0rd");
                await userManager.AddToRolesAsync(admin, new[] { SD.AdminRole,SD.ManagerRole,SD.PlayerRole });
                await userManager.AddClaimsAsync(admin, new Claim[]
                {
                    new Claim(ClaimTypes.Email,admin.Email),
                    new Claim(ClaimTypes.GivenName,admin.FirstName),
                    new Claim(ClaimTypes.Surname,admin.LastName),
                });

                //admin kullanıcısı => Adem Yurdagelen
                var admin2 = new User
                {
                    FirstName = "Adem",
                    LastName = "Yurdagelen",
                    UserName = "ademyurdagelen@example.com",
                    EmailConfirmed = true,
                    Email = "ademyurdagelen@example.com"
                };
                await userManager.CreateAsync(admin2, "Pa$$w0rd");
                await userManager.AddToRolesAsync(admin2, new[] { SD.AdminRole, SD.ManagerRole, SD.PlayerRole });
                await userManager.AddClaimsAsync(admin2, new Claim[]
                {
                    new Claim(ClaimTypes.Email,admin2.Email),
                    new Claim(ClaimTypes.GivenName,admin2.FirstName),
                    new Claim(ClaimTypes.Surname,admin2.LastName),
                });

                //manager kullanıcısı
                var manager = new User
                {
                    FirstName = "Eslem",
                    LastName = "Yurdagelen",
                    UserName = "eslemYurdagelen@example.com",
                    EmailConfirmed = true,
                    Email = "eslemYurdagelen@example.com"
                };
                await userManager.CreateAsync(manager, "Pa$$w0rd");
                await userManager.AddToRolesAsync(manager, new[] { SD.ManagerRole, SD.PlayerRole });
                await userManager.AddClaimsAsync(manager, new Claim[]
                {
                    new Claim(ClaimTypes.Email,manager.Email),
                    new Claim(ClaimTypes.GivenName,manager.FirstName),
                    new Claim(ClaimTypes.Surname,manager.LastName),
                });

                //player kullanıcısı
                var manager2 = new User
                {
                    FirstName = "Muhammed Ali",
                    LastName = "Yurdagelen",
                    UserName = "muhammedaliyurdagelen@example.com",
                    Email = "muhammedaliyurdagelen@example.com",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(manager2, "Pa$$w0rd");
                await userManager.AddToRolesAsync(manager2, new[] { SD.ManagerRole, SD.PlayerRole });
                await userManager.AddClaimsAsync(manager2, new Claim[]
                {
                    new Claim(ClaimTypes.Email,manager2.Email),
                    new Claim(ClaimTypes.GivenName,manager2.FirstName),
                    new Claim(ClaimTypes.Surname,manager2.LastName),
                });

                //player kullanıcısı 2
                var player2 = new User
                {
                    FirstName = "Ahmet Zahit",
                    LastName = "Yurdagelen",
                    UserName = "ahmetzahityurdagelen@example.com",
                    EmailConfirmed = true,
                    Email = "ahmetzahityurdagelen@example.com"
                };
                await userManager.CreateAsync(player2, "Pa$$w0rd");
                await userManager.AddToRolesAsync(player2, new[] { SD.PlayerRole });
                await userManager.AddClaimsAsync(player2, new Claim[]
                {
                    new Claim(ClaimTypes.Email,player2.Email),
                    new Claim(ClaimTypes.GivenName,player2.FirstName),
                    new Claim(ClaimTypes.Surname,player2.LastName),
                });
            }
        }
    }
}
