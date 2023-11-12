using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleClaimController : ControllerBase
    {
        [HttpGet("public")]
        public IActionResult Public()
        {
            return Ok("public");
        }
        #region Roles

        [HttpGet("super-admin-role")]
        [Authorize(Roles = SD.SuperAdminRole)]
        public IActionResult SuperAdminRole()
        {
            return Ok("super-admin-role");
        }

        [HttpGet("admin-role")]
        [Authorize(Roles = SD.AdminRole)]
        public IActionResult AdminRole()
        {
            return Ok("admin-role");
        }
        
        [HttpGet("manager-role")]
        [Authorize(Roles = SD.ManagerRole)]
        public IActionResult ManagerRole()
        {
            return Ok("manager-role");
        }

        [HttpGet("player-role")]
        [Authorize(Roles = SD.PlayerRole)]
        public IActionResult PlayerRole()
        {
            return Ok("player-role");
        }

        [HttpGet("admin-or-manager-role")]
        [Authorize(Roles = $"{SD.AdminRole},{SD.ManagerRole}")]
        public IActionResult AdminOrManagerRole()
        {
            return Ok("admin-or-manager-role");
        }

        [HttpGet("admin-or-player-role")]
        [Authorize(Roles = $"{SD.AdminRole},{SD.PlayerRole}")]
        public IActionResult AdminOrPlayerRole()
        {
            return Ok("admin-or-player-role");

        }
        #endregion

        #region Policy
        [HttpGet("super-admin-policy")]
        [Authorize(Policy = SD.SuperAdminPolicy)]
        public IActionResult SuperAdminPolicy()
        {
            return Ok("super-admin-policy");
        }
        [HttpGet("admin-policy")]
        [Authorize(Policy = SD.AdminPolicy)]  
        public IActionResult AdminPolicy()
        {
            return Ok("admin-policy");
        }
        [HttpGet("manager-policy")]
        [Authorize(Policy = SD.ManagerPolicy)]
        public IActionResult ManagerPolicy()
        {
            return Ok("manager-policy");
        }
        [HttpGet("player-policy")]
        [Authorize(Policy = SD.PlayerPolicy)]
        public IActionResult PlayerPolicy()
        {
            return Ok("player-policy");
        }
        [HttpGet("admin-or-manager-policy")]
        [Authorize(Policy = SD.AdminOrManagerPolicy)]
        public IActionResult AdminOrManagerPolicy()
        {
            return Ok("admin-or-manager-policy");
        }
        [HttpGet("admin-and-manager-policy")]
        [Authorize(Policy = SD.AdminAndManagerPolicy)]
        public IActionResult AdminAndManagerPolicy()
        {
            return Ok("admin-and-manager-policy");
        }

        [HttpGet("all-roles-policy")]
        [Authorize(Policy = SD.AllRolesPolicy)]
        public IActionResult AllRolesPolicy()
        {
            return Ok("all-roles-policy");
        }

        #endregion

        #region Claim Policy

        [HttpGet("admin-email-policy")]
        [Authorize(Policy = SD.AdminEmailPolicy)]
        public IActionResult AdminEmailPolicy()
        {
            return Ok("admin-email-policy");
        }
        [HttpGet("paulson-surname-policy")]
        [Authorize(Policy = SD.PaulsonSurnamePolicy)]
        public IActionResult PaulsonSurnamePolicy()
        {
            return Ok("paulson-surname-policy");
        }
        [HttpGet("manager-email-and-paulson-surname-policy")]
        [Authorize(Policy = SD.ManagerEmailAndPaulsonSurnamePolicy)]
        public IActionResult ManagerEmailAdnPaulsonSurnamePolicy()
        {
            return Ok("manager-email-and-paulson-surname-policy");
        }
        [HttpGet("vip-policy")]
        [Authorize(Policy = SD.VipPolicy)]
        public IActionResult VipPolicy()
        {
            return Ok("vip-policy");
        }
        #endregion
    }
}
