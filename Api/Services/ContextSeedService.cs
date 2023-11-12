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
                await roleManager.CreateAsync(new IdentityRole { Name = SD.AdminRole });
                await roleManager.CreateAsync(new IdentityRole { Name = SD.ManagerRole });
                await roleManager.CreateAsync(new IdentityRole { Name = SD.PlayerRole });
            }
            if(!await userManager.Users.AnyAsync())
            {
                //admin kullanıcısı
                var admin = new User
                {
                    FirstName = "Admin",
                    LastName = "Jackson",
                    UserName = "admin@example.com",
                    EmailConfirmed = true,
                    Email = "admin@example.com"
                };
                await userManager.CreateAsync(admin, "Pa$$w0rd");
                await userManager.AddToRolesAsync(admin, new[] { SD.AdminRole,SD.ManagerRole,SD.PlayerRole });
                await userManager.AddClaimsAsync(admin, new Claim[]
                {
                    new Claim(ClaimTypes.Email,admin.Email),
                    new Claim(ClaimTypes.GivenName,admin.FirstName),
                    new Claim(ClaimTypes.Surname,admin.LastName),
                });

                //manager kullanıcısı
                var manager = new User
                {
                    FirstName = "Manager",
                    LastName = "Paulson",
                    UserName = "manager@example.com",
                    EmailConfirmed = true,
                    Email = "manager@example.com"
                };
                await userManager.CreateAsync(manager, "Pa$$w0rd");
                await userManager.AddToRolesAsync(manager, new[] { SD.ManagerRole });
                await userManager.AddClaimsAsync(manager, new Claim[]
                {
                    new Claim(ClaimTypes.Email,manager.Email),
                    new Claim(ClaimTypes.GivenName,manager.FirstName),
                    new Claim(ClaimTypes.Surname,manager.LastName),
                });

                //vip player kullanıcısı
                var vip_player = new User
                {
                    FirstName = "vip_player",
                    LastName = "tomsson",
                    UserName = "vip_player@example.com",
                    Email = "vip_player@example.com",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(vip_player, "Pa$$w0rd");
                await userManager.AddToRolesAsync(vip_player, new[] { SD.PlayerRole });
                await userManager.AddClaimsAsync(vip_player, new Claim[]
                {
                    new Claim(ClaimTypes.Email,manager.Email),
                    new Claim(ClaimTypes.GivenName,manager.FirstName),
                    new Claim(ClaimTypes.Surname,manager.LastName),
                });

                //player kullanıcısı
                var player = new User
                {
                    FirstName = "Player",
                    LastName = "Player",
                    UserName = "player@example.com",
                    EmailConfirmed = true,
                    Email = "player@example.com"
                };
                await userManager.CreateAsync(player, "Pa$$w0rd");
                await userManager.AddToRolesAsync(player, new[] { SD.PlayerRole });
                await userManager.AddClaimsAsync(player, new Claim[]
                {
                    new Claim(ClaimTypes.Email,player.Email),
                    new Claim(ClaimTypes.GivenName,player.FirstName),
                    new Claim(ClaimTypes.Surname,player.LastName),
                });
            }
        }
    }
}
