using System;

namespace Api.DTOs.Account
{
    public class RefreshTokenDto
    {
        public string Token { get; set; }
        public DateTime ExpirationDate { get; set; }
        public DateTime DateCreated { get; set; }
        public string UserId { get; set; }
    }
}
