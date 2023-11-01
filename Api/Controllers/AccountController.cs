using Api.Attributes;
using Api.DTOs.Account;
using Api.Exceptions;
using Api.Services;
using IdentityApp.Data.Models;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using User = IdentityApp.Data.Models;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly JWTService _jwtService;
        private readonly IConfiguration _config;
        private readonly SignInManager<User.User> _signInManager;
        private readonly UserManager<User.User> _userManager;
        private readonly EmailService _emailService;

        public AccountController(
            JWTService jwtService,
            IConfiguration config,
            SignInManager<User.User> signInManager,
            UserManager<User.User> userManager,
            EmailService emailService
            )
        {
            _jwtService = jwtService;
            _config = config;
            _signInManager = signInManager;
            _userManager = userManager;
            _emailService = emailService;
        }

        [Authorize]
        [HttpGet("refresh-user-token")]
        public async Task<ActionResult<UserDto>> RefreshUserToken()
        {
            var user = await _userManager.FindByNameAsync(User.FindFirst(ClaimTypes.Email)?.Value);
            return CreateApplicationUserDto(user);
        }

        [HttpPost("login")]
        [ServiceFilter(typeof(ValidationErrorResponseAttribute))]
        public async Task<ActionResult<UserDto>> Login([FromBody] LoginDto model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            //if (user == null) return Unauthorized("Invalid username or password");
            if (user == null) throw new AuthenticationErrorException("User Not Found!");

            if (user.EmailConfirmed == false) throw new AuthenticationErrorException("Please confirm your email address.");

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded) throw new AuthenticationErrorException("Invalid username or password");

            return CreateApplicationUserDto(user);
        }

        [HttpPost("register")]
        [ServiceFilter(typeof(ValidationErrorResponseAttribute))]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (await CheckEmailExistsAsync(model.EmailAddress))
            {
                throw new AuthenticationErrorException($"An existing account is using {model.EmailAddress}, email addres. Please try with another email address");
            }

            var userToAdd = new User.User
            {
                FirstName = model.FirstName.ToLower(),
                LastName = model.LastName.ToLower(),
                UserName = model.EmailAddress.ToLower(),
                Email = model.EmailAddress.ToLower(),
            };

            // creates a user inside our AspNetUsers table inside our database
            var result = await _userManager.CreateAsync(userToAdd, model.Password);
            if (!result.Succeeded)
                return new ObjectResult(new
                {
                    Errors = result.Errors
                });

            try
            {
                if (await SendConfirmEmailAsync(userToAdd))
                {

                    return Ok(new { title = "Account Created.", message = "Your account has ben created,please confirm your email address!" });
                }
                return BadRequest("Failed To send email.Please contact admin!");
            }
            catch (Exception ex)
            {

                return BadRequest("Failed To send email.Please contact admin!");
            }

        }

        [HttpPut("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.EmailAddress);

            if (user == null) return Unauthorized("This email address has not been registered yet.");

            if (user.EmailConfirmed) return BadRequest("Your email was confirmed before.Please login to your account");

            try
            {
                var decodedTokenBytes = WebEncoders.Base64UrlDecode(model.Token);
                var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

                var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

                if (result.Succeeded)
                {
                    return Ok(new { title = "Email Confirmed", message = "Your email address is confirmed.You can login now." });
                }
                return BadRequest("Invalid token.Please try again");
            }
            catch (Exception)
            {
                return BadRequest("Invalid token.Please try again");
            }
        }
        [HttpPut("reset-password")]

        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.EmailAddress);

            if (user == null) return Unauthorized("This email address has not been registered yet.");

            if (!user.EmailConfirmed) return BadRequest("Your email was not confirmed yet.Please confirm your email address first.");

            try
            {
                var decodedTokenBytes = WebEncoders.Base64UrlDecode(model.Token);
                var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

                var result = await _userManager.ResetPasswordAsync(user, decodedToken,model.NewPassword);

                if (result.Succeeded)
                {
                    return Ok(new { title = "Reset Password", message = "Your password has been changed.You can login now." });
                }
                return BadRequest("Invalid token.Please try again");
            }
            catch (Exception)
            {
                return BadRequest("Invalid token.Please try again");
            }
        }

        [HttpPost("resend-email-confirmation-link/{emailAddress}")]
        public async Task<IActionResult> ResendConfirmationLink(string emailAddress)
        {

            if (string.IsNullOrEmpty(emailAddress)) return BadRequest("Invalid email address!");

            var user = await _userManager.FindByEmailAsync(emailAddress);

            if (user == null) return Unauthorized("This email address has not been registered yet.");

            if (user.EmailConfirmed) return BadRequest("Your email was confirmed before.Please login to your account");


            try
            {
                if (await SendConfirmEmailAsync(user))
                {

                    return Ok(new { title = "Confirmation link sent.", message = "Please confirm your email address!" });
                }
                return BadRequest("Failed To send email.Please contact admin!");
            }
            catch (Exception ex)
            {

                return BadRequest("Failed To send email.Please contact admin!");
            }
        }

        [HttpPost("forgot-username-or-password/{emailAddress}")]
        public async Task<IActionResult> ForgotUsernameOrPassword(string emailAddress)
        {
            if (string.IsNullOrEmpty(emailAddress)) return BadRequest("Invalid email address!");

            var user = await _userManager.FindByEmailAsync(emailAddress);

            if (user == null) return Unauthorized("This email address has not been registered yet.");

            if (!user.EmailConfirmed) return BadRequest("Your email was not confirmed yet.Please confirm your email address first.");

            try
            {
                if (await SendForgotUsernameOrPasswordAsync(user))
                {
                    return Ok(new { title = "Forgot username or password email sent", message = "Please check your email address!" });
                }
                return BadRequest("Failed To send forgot username or password email.Please contact admin!");
            }
            catch (Exception)
            {

                return BadRequest("Failed To send forgot username or password email.Please contact admin!");
            }
        }

        #region Private Helper Methods
        private UserDto CreateApplicationUserDto(User.User user)
        {
            return new UserDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                JWT = _jwtService.CreateJWT(user),
            };
        }

        private async Task<bool> CheckEmailExistsAsync(string email)
        {
            return await _userManager.Users.AnyAsync(x => x.Email == email.ToLower());
        }

        private async Task<bool> SendConfirmEmailAsync(User.User user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var url = $"{_config["JWT:ClientUrl"]}/{_config["Email:ConfirmEmailPath"]}?token={token}&email={user.Email}";

            var body = $"<p>Hello: {user.FirstName} {user.LastName}</p>" +
                "<p>Please confirm your email address by clicking on the following link.</p>" +
                $"<p><a href=\"{url}\">Click here</a></<p>" +
                "<p>Thank you,</p>" +
                $"<br>{_config["Email:ApplicationName"]}";

            var emailSend = new EmailSendDto(user.Email, "Confirm your email address", body);
            return await _emailService.SendEmailAsync(emailSend);
        }
        public async Task<bool> SendForgotUsernameOrPasswordAsync(User.User user)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var url = $"{_config["JWT:ClientUrl"]}/{_config["Email:ResetPasswordPath"]}?token={token}&email={user.Email}";

            var body = $"<p>Hello: {user.FirstName} {user.LastName}</p>" +
                $"<p>Username: {user.UserName}</p>"+
                "<p>In order to reset your email address,please click on the following link.</p>" +
                $"<p><a href=\"{url}\">Click here</a></<p>" +
                "<p>Thank you,</p>" +
                $"<br>{_config["Email:ApplicationName"]}";

            var emailSend = new EmailSendDto(user.Email, "Forgot username or password", body);
            return await _emailService.SendEmailAsync(emailSend);
        }
        #endregion
    }
}
