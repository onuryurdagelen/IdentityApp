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
            var members = userManager.Users
                .AsEnumerable()
                //this is a protection
                .Where( member => !IsUserAnAdmin(member))
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

            return Ok(members);
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

            await userManager.SetLockoutEndDateAsync(member, null);
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
            var member =  userManager.Users
                .Where(member =>member.Id == id)
                .AsEnumerable()
                .Where(member => !IsUserAnAdmin(member))
                .Select(member => new MemberAddEditDto
                {
                    Id = member.Id,
                    FirstName = member.FirstName,
                    LastName = member.LastName,
                    UserName = member.UserName,
                    Roles = userManager.GetRolesAsync(member).GetAwaiter().GetResult().ToList(),
                }).FirstOrDefault();

            return Ok(member);
        }

        [HttpPost("add-edit-member")]
        public async Task<IActionResult> AddEditMember(MemberAddEditDto model)
        {
            if(string.IsNullOrEmpty(model.Id))
            {
                //adding a new member
                if(string.IsNullOrEmpty(model.Password) || model.Password.Length < 6)
                {
                    ModelState.AddModelError("errors", "Password must be at least 6 characters");
                    return BadRequest(ModelState);
                }
            }
            else
            {
                //editing an existing member
            }
            return Ok();
        }

        private bool IsUserAnSuperAdmin(User user) => userManager.GetRolesAsync(user).GetAwaiter().GetResult().ToArray().Any(x => x == SD.SuperAdminRole);

        private bool IsUserAnAdmin(User user) => userManager.GetRolesAsync(user).GetAwaiter().GetResult().ToArray().Any(x => x == SD.AdminRole);

    }
}
