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

        protected string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier); //id of current user

        protected bool IsAdministrator => User.IsInRole(SD.AdminRole);


    }
}
