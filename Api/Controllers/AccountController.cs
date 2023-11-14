using Api.Attributes;
using Api.DTOs.Account;
using Api.Exceptions;
using Api.Services;
using IdentityApp.Data;
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
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using User = IdentityApp.Data.Models;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : IdentityController
    {
        private readonly JWTService _jwtService;
        private readonly IConfiguration _config;
        private readonly SignInManager<User.User> _signInManager;
        private readonly UserManager<User.User> _userManager;
        private readonly EmailService _emailService;
        private readonly AppDbContext _context;
        public AccountController(
            JWTService jwtService,
            IConfiguration config,
            SignInManager<User.User> signInManager,
            UserManager<User.User> userManager,
            EmailService emailService,
            AppDbContext context
            )
        {
            _jwtService = jwtService;
            _config = config;
            _signInManager = signInManager;
            _userManager = userManager;
            _emailService = emailService;
            _context = context;
        }
        [HttpPost("refresh-token")]
        public async Task<ActionResult<UserDto>> RefreshToken(RefreshTokenToSendDto model)
        {
            if (string.IsNullOrEmpty(model.UserId) || string.IsNullOrEmpty(model.Token))
                return NotFound("Invalid token.Please try to login again.");

            var fetchedRefreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.UserId == model.UserId && x.Token == model.Token);

            if (fetchedRefreshToken == null)
                return NotFound("Invalid token.Please try to login again.");

            if (fetchedRefreshToken.IsExpired)
                return BadRequest("Your sessing has expired.Please login again.");

            var existedUser = await _userManager.FindByIdAsync(model.UserId);

            return await CreateApplicationUserDto(existedUser);

        }

        [Authorize]
        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke(RevokeTokenDto model)
        {
            var refreshToken = await _context.RefreshTokens.SingleOrDefaultAsync(p => p.Token == model.Token);
            if (refreshToken == null)
                return NotFound("User does not have this refresh token");

            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
                return NotFound(SD.UserNotFoundMessage);  

            _context.RefreshTokens.Remove(refreshToken);
            user.RefreshTokenId = null;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok();
        }
        [Authorize]
        [HttpGet("refresh-page")]
        public async Task<ActionResult<UserDto>> RefreshPage()
        {
            var user = await _userManager.FindByNameAsync(User.FindFirst(ClaimTypes.Email)?.Value);
            return await CreateApplicationUserDto(user);
        }

        [HttpPost("login")]
        [ServiceFilter(typeof(ValidationErrorResponseAttribute))]
        public async Task<ActionResult<UserDto>> Login([FromBody] LoginDto model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            //if (user == null) return Unauthorized("Invalid username or password");
            if (user == null) 
                return Unauthorized(SD.UserNotFoundMessage);

            if (user.EmailConfirmed == false) 
                return Unauthorized(SD.ConfirmEmailAddressMessage);

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            if (result.IsLockedOut)
                return Unauthorized(string.Format(SD.AccountLockedMessage, user.LockoutEnd));

            if (!result.Succeeded)
            {
                //whenever the user has put invalid password for three  times,then we lock the user for one day(exclude admin users)
                if (!IsUserAnAdmin(user))
                    await _userManager.AccessFailedAsync(user);

                if (user.AccessFailedCount >= SD.MaximumLoginAttemps)
                {
                    await _userManager.SetLockoutEndDateAsync(user, DateTime.UtcNow.AddDays(1));
                    return Unauthorized(string.Format(SD.AccountLockedMessageOwingToLoginAttemps, user.LockoutEnd));
                }
                return Unauthorized(SD.InvalidUserNameOrPasswordMessage);
            }

            //eğer 1. veya 2.denemede başarılı bir şekilde giriş yapar ise AccessFailedCount sıfırlanmalıdır.
            await _userManager.ResetAccessFailedCountAsync(user);
            await _userManager.SetLockoutEndDateAsync(user, null);

            return await CreateApplicationUserDto(user);
        }

        [HttpPost("register")]
        [ServiceFilter(typeof(ValidationErrorResponseAttribute))]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (await CheckEmailExistsAsync(model.EmailAddress))
            {
                throw new AuthenticationErrorException(string.Format(SD.EmailAddressExistMessage, model.EmailAddress));
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
            if (!result.Succeeded) return BadRequest(result.Errors);
            //when user register,we assign the player role to the user here.
            await _userManager.AddToRoleAsync(userToAdd, SD.PlayerRole);

            try
            {
                if (await SendConfirmEmailAsync(userToAdd))
                {

                    return Ok(new { title = "Account Created.", message = "Your account has ben created,please confirm your email address!" });
                }
                return BadRequest(SD.FailedToSendEmailMessage);
            }
            catch (Exception ex)
            {

                return BadRequest(SD.FailedToSendEmailMessage);
            }

        }

        [HttpPut("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.EmailAddress);

            if (user == null) return Unauthorized(new { message = SD.EmailAddressHasNotRegisteredYetMessage, confirmed = false });

            if (user.EmailConfirmed) return BadRequest(new { message = SD.EmailAddressWasConfirmedBeforeMessage,confirmed = true });

            try
            {
                var decodedTokenBytes = WebEncoders.Base64UrlDecode(model.Token);
                var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

                var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

                if (result.Succeeded)
                {
                    return Ok(new { title = "Email Confirmed", message = "Your email address is confirmed.You can login now." });
                }
                return BadRequest(new {message = SD.InvalidTokenMessage ,confirmed = false});
            }
            catch (Exception)
            {
                return BadRequest(new { message = SD.InvalidTokenMessage, confirmed = false });
            }
        }
        [HttpPut("reset-password")]

        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.EmailAddress);

            if (user == null) return Unauthorized(SD.EmailAddressHasNotRegisteredYetMessage);

            if (!user.EmailConfirmed) return BadRequest(SD.EmailAddressWasNotConfirmedYetMessage);

            try
            {
                var decodedTokenBytes = WebEncoders.Base64UrlDecode(model.Token);
                var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

                var result = await _userManager.ResetPasswordAsync(user, decodedToken,model.NewPassword);

                if (result.Succeeded)
                {
                    return Ok(new { title = "Reset Password", message = "Your password has been changed.You can login now." });
                }
                return BadRequest(SD.InvalidTokenMessage);
            }
            catch (Exception)
            {
                return BadRequest(SD.InvalidTokenMessage);
            }
        }

        [HttpPost("resend-email-confirmation-link/{emailAddress}")]
        public async Task<IActionResult> ResendConfirmationLink(string emailAddress)
        {

            if (string.IsNullOrEmpty(emailAddress)) return BadRequest(SD.InvalidEmailAddressMessage);

            var user = await _userManager.FindByEmailAsync(emailAddress);

            if (user == null) return Unauthorized(SD.EmailAddressHasNotRegisteredYetMessage);

            if (user.EmailConfirmed) return BadRequest(SD.EmailAddressWasConfirmedBeforeMessage);

            try
            {
                if (await SendConfirmEmailAsync(user))
                {

                    return Ok(new { title = "Confirmation link sent.", message = "Please confirm your email address!" });
                }
                return BadRequest(SD.FailedToSendEmailMessage);
            }
            catch (Exception ex)
            {

                return BadRequest(SD.FailedToSendEmailMessage);
            }
        }

        [HttpPost("forgot-username-or-password/{emailAddress}")]
        public async Task<IActionResult> ForgotUsernameOrPassword(string emailAddress)
        {
            if (string.IsNullOrEmpty(emailAddress)) return BadRequest(SD.InvalidEmailAddressMessage);

            var user = await _userManager.FindByEmailAsync(emailAddress);

            if (user == null) return Unauthorized(SD.EmailAddressHasNotRegisteredYetMessage);

            if (!user.EmailConfirmed) return BadRequest(SD.EmailAddressWasNotConfirmedYetMessage);

            try
            {
                if (await SendForgotUsernameOrPasswordAsync(user))
                {
                    return Ok(new { title = "Forgot username or password email sent", message = "Please check your email address!" });
                }
                return BadRequest(SD.FailedToSendForgotUserNameOrPasswordEmailMessage);
            }
            catch (Exception)
            {

                return BadRequest(SD.FailedToSendForgotUserNameOrPasswordEmailMessage);
            }
        }

        #region Private Helper Methods
        private async Task<UserDto> CreateApplicationUserDto(User.User user)
        {
           var createdRefreshToken =  await SaveRefreshTokenAsync(user);

            return new UserDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = await _jwtService.CreateJWTAsync(user, createdRefreshToken),
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
            var url = $"{_config["JWT:ClientUrl"]}/{_config["Email:ConfirmEmailPath"]}?token={token}&emailAddress={user.Email}";

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
            var url = $"{_config["JWT:ClientUrl"]}/{_config["Email:ResetPasswordPath"]}?token={token}&emailAddress={user.Email}";

            var body = $"<p>Hello: {user.FirstName} {user.LastName}</p>" +
                $"<p>Username: {user.UserName}</p>"+
                "<p>In order to reset your email address,please click on the following link.</p>" +
                $"<p><a href=\"{url}\">Click here</a></<p>" +
                "<p>Thank you,</p>" +
                $"<br>{_config["Email:ApplicationName"]}";

            var emailSend = new EmailSendDto(user.Email, "Forgot username or password", body);
            return await _emailService.SendEmailAsync(emailSend);
        }

        private bool IsUserAnSuperAdmin(User.User user) => _userManager.GetRolesAsync(user).GetAwaiter().GetResult().ToArray().Any(x => x == SD.SuperAdminRole);

        private bool IsUserAnAdmin(User.User user) => _userManager.GetRolesAsync(user).GetAwaiter().GetResult().ToArray().Any(x => x == SD.AdminRole);

        private async Task<RefreshTokenDto> SaveRefreshTokenAsync(User.User user)
        {
            var createdRefreshToken = _jwtService.CreateRefreshToken(user);
            var checkedRefreshToken = await _context.RefreshTokens.SingleOrDefaultAsync(rt => rt.UserId == user.Id);

            //database'de ilgili kullanıcıya ait refresh token var ise oluşturduğumuz yeni refresh token ile değiştiriyoruz.
            if(checkedRefreshToken != null)
            {
                checkedRefreshToken.Token = createdRefreshToken.Token;
                checkedRefreshToken.DateCreatedUtc = createdRefreshToken.DateCreated;
                checkedRefreshToken.DateExpiresUtc = createdRefreshToken.ExpirationDate;

                _context.RefreshTokens.Update(checkedRefreshToken);

            }
            //yoksa kullanıcıya ait refresh token ekliyoruz.
            else
            {
                var refreshTokenId = Guid.NewGuid();
                RefreshToken refreshToken = new RefreshToken 
                { 
                    Token = createdRefreshToken.Token,
                    DateCreatedUtc = createdRefreshToken.DateCreated, 
                    DateExpiresUtc = createdRefreshToken.ExpirationDate,
                    UserId = user.Id,
                    Id = refreshTokenId,
                };
                await _context.RefreshTokens.AddAsync(refreshToken);
                user.RefreshTokenId = refreshTokenId;
                _context.Users.Update(user);
            }
            await _context.SaveChangesAsync();

            return createdRefreshToken;

        }
        public async Task<bool> IsValidRefreshTokenAsync(string userId,string token)
        {
            if(string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token)) return false;

            var fetchedRefreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.UserId == userId && x.Token == token);

            if (fetchedRefreshToken == null) return false;

            if(fetchedRefreshToken.IsExpired) return false;

            return true;
        }
        #endregion
    }
}
