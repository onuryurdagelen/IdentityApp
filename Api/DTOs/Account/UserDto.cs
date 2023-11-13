namespace Api.DTOs.Account
{
    public class UserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public TokenDto Token { get; set; }
    }
}
