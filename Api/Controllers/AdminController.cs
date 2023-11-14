using Api.Attributes;
using Api.DTOs.Admin;
using IdentityApp.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers
{
    //Bu controller'a sadece adminlerin ulaşabileceğini belirttik.
    [Authorize(Roles = SD.AdminRole)]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : IdentityController
    {
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public AdminController(UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }
        [HttpGet("get-members")]
        public async Task<ActionResult<IEnumerable<MemberViewDto>>> GetMembers()
        {
            var members = await userManager.Users.ToListAsync();
                //this is a protection
            var result = members.Where( member => !IsUserAnAdmin(member))
                .Select( member => new MemberViewDto
                {
                    Id = member.Id,
                    UserName = member.UserName,
                    DateCreated = member.DateCreated,
                    FirstName = member.FirstName,
                    LastName = member.LastName,
                    IsLocked = userManager.IsLockedOutAsync(member).GetAwaiter().GetResult(),
                    Roles = userManager.GetRolesAsync(member).GetAwaiter().GetResult()
                    
                }).ToList();

            return Ok(result);
        }
        [HttpPut("lock-member/{id}")]
        public async Task<IActionResult> LockMember(string id)
        {
            var member = await userManager.FindByIdAsync(id);
            if (member == null) return NotFound("User not found!");

            //Kullanıcı admin ise değişikliğe izin verilmeyecek.
            if (IsUserAnSuperAdmin(member)) return BadRequest(SD.SuperAdminChangeNotAllowed);
                
            //Kullanıcıyı 5 günlüğüne kilitledik.
            await userManager.SetLockoutEndDateAsync(member, DateTime.UtcNow.AddDays(5));
            return NoContent();
        }
        [HttpPut("unlock-member/{id}")]
        public async Task<IActionResult> UnlockMember(string id)
        {
            var member = await userManager.FindByIdAsync(id);
            if (member == null) return NotFound("User not found!");

            //Kullanıcı admin ise değişikliğe izin verilmeyecek.
            if (IsUserAnSuperAdmin(member)) return BadRequest(SD.SuperAdminChangeNotAllowed);

            if(await userManager.GetAccessFailedCountAsync(member) > 0) 
                member.AccessFailedCount = 0;
            
            await userManager.SetLockoutEndDateAsync(member, null);
            await userManager.UpdateAsync(member);
            return NoContent();
        }

        [HttpDelete("delete-member/{id}")]
        public async Task<IActionResult> DeleteMember(string id)
        {
            var member = await userManager.FindByIdAsync(id);
            if (member == null) return NotFound("User not found!");

            //Kullanıcı admin ise değişikliğe izin verilmeyecek.
            if (IsUserAnSuperAdmin(member)) return BadRequest(SD.SuperAdminChangeNotAllowed);

            await userManager.DeleteAsync(member);
            return NoContent();
        }
        [HttpGet("application-roles")]
        public async Task<ActionResult<IEnumerable<AppRoleDto>>> GetApplicationRoles()
        {
            if (IsSuperAdmin)
            {
                var appRoles = await roleManager.Roles.Select(x => new AppRoleDto { Id = x.Id, Name = x.Name }).ToListAsync();
                return Ok(appRoles);
            }
            return BadRequest(SD.OnlySuperAdminCanSeeApplicationRoles);
        }

        [HttpGet("get-member/{id}")]
        public async Task<ActionResult<MemberAddEditDto>> GetMember(string id)
        {
            //kullanıcı türü admin değil ve ilgili id'ye eşit olan member'ı getirmeliyiz.

            var members = await userManager.Users.ToListAsync();

            var member = members.Where(member => !IsUserAnAdmin(member) && member.Id == id)
                .Select(member => new MemberAddEditDto
                {
                    Id = member.Id,
                    FirstName = member.FirstName,
                    LastName = member.LastName,
                    UserName = member.UserName,
                    Roles = string.Join(",",userManager.GetRolesAsync(member).GetAwaiter().GetResult())
                }).SingleOrDefault();

            if (member == null) return NotFound("User not found!");

            return Ok(member);
        }

        [HttpPost("add-edit-member")]
        [ServiceFilter(typeof(ValidationErrorResponseAttribute))]
        public async Task<IActionResult> AddEditMember(MemberAddEditDto model)
        {
            User user;

            if(string.IsNullOrEmpty(model.Id))
            {
                //adding a new member
                if(string.IsNullOrEmpty(model.Password) || model.Password.Length < 6)
                {
                    ModelState.AddModelError("errors", "Password must be at least 6 characters");
                    return BadRequest(ModelState);
                }
                //check if email address already used by other members
                if(await userManager.Users.AnyAsync(x => x.Email == model.UserName.ToLower()))
                 {
                    return BadRequest($"An existing account is using {model.UserName}, email address. Please try with another email address");
                }
                user = new User
                {
                    FirstName = model.FirstName.ToLower(),
                    LastName = model.LastName.ToLower(),
                    UserName = model.UserName.ToLower(),
                    Email = model.UserName.ToLower(),
                    EmailConfirmed = true
                };
               var result =  await userManager.CreateAsync(user, model.Password);

                if (!result.Succeeded) return BadRequest(result.Errors);
            }
            else
            {
                //editing an existing member

                if (!string.IsNullOrEmpty(model.Password))
                {
                    if (model.Password.Length < 6)
                    {
                        ModelState.AddModelError("errors", "Password must be at least 6 characters");
                        return BadRequest(ModelState);
                    }
                }

                user = await userManager.FindByIdAsync(model.Id);
                if (user == null) return NotFound("User not found!");

                if (IsUserAnSuperAdmin(user))
                {
                    return BadRequest(SD.SuperAdminChangeNotAllowed);
                }

                user.Email = model.UserName.ToLower();
                user.UserName = model.UserName.ToLower();
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;

                if (!string.IsNullOrEmpty(model.Password))
                {
                    await userManager.RemovePasswordAsync(user);
                    await userManager.AddPasswordAsync(user, model.Password);
                }
            
                await userManager.UpdateAsync(user);
            }
            // remove existing roles from user
            var userRoles = await userManager.GetRolesAsync(user);
            await userManager.RemoveFromRolesAsync(user, userRoles);

            //add role to user
            foreach (var role in model.Roles.Split(",").ToArray())
            {
                var roleToAdd = await roleManager.Roles.FirstOrDefaultAsync(r => r.Name == role.ToLower());
                if (roleToAdd == null) return NotFound($"Invalid role name => {role}");

                await userManager.AddToRoleAsync(user, role.ToLower());
                
            }
            if(string.IsNullOrEmpty(model.Id))
            {
                return Ok(new JsonResult(new { title = "Member Created", message = $"{model.UserName} has been created" }));
            }
            else
            {
                return Ok(new JsonResult(new { title = "Member Edited", message = $"{model.UserName} has been edited" }));

            }

        }

        private bool IsUserAnSuperAdmin(User user) => userManager.GetRolesAsync(user).GetAwaiter().GetResult().ToArray().Any(x => x == SD.SuperAdminRole);

        private bool IsUserAnAdmin(User user) => userManager.GetRolesAsync(user).GetAwaiter().GetResult().ToArray().Any(x => x == SD.AdminRole);

    }
}
