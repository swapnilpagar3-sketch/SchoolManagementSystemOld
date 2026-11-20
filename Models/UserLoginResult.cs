namespace EntityFrameworkCore.Models
{
    public class UserLoginResult
    {
        public int UserId { get; set; }

        public string Username { get; set; }

        public string EmailAddress { get; set; }

        public string Password { get; set; }

        public string ProfileImage { get; set; }
    }
}
