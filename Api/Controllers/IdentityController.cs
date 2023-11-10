using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System;
using IdentityApp.Data.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Api.Controllers
{
    public class IdentityController : ControllerBase
    {
        protected readonly SignInManager<User> _signInManager;
        protected readonly UserManager<User> _userManager;
        protected string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier); //id of current user
        protected string EmailAddress => User.FindFirstValue(ClaimTypes.Email); //Email address of current user
        protected string FullName => User.FindFirstValue(ClaimTypes.GivenName) + " " + User.FindFirstValue(ClaimTypes.Surname);
        protected bool IsAdministrator => User.IsInRole("Administrators") || User.IsInRole("SystemAdministrators");

        public IdentityController(SignInManager<User> signInManager,UserManager<User> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }
        protected async Task<User> CurrentUser() 
        {
            return await _userManager.FindByIdAsync(UserId) is not null ? await _userManager.FindByIdAsync(UserId) : default;
        }
    }
}
