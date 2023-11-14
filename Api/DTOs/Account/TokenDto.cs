using IdentityApp.Data.Models;
using System;

namespace Api.DTOs.Account
{
    public class TokenDto
    {
        public string AccessToken { get; set; }
        public DateTime ExpirationTime { get; set; }
        public RefreshTokenDto RefreshToken { get; set; }
    }
}
