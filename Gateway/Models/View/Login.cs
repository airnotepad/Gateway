using System.ComponentModel.DataAnnotations;

namespace Gateway.Models.View
{
    public class LoginModel
    {
        public string Username { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public class RegisterModel
    {
        public string Username { get; set; }

        public string Invite { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Incorrect password confirm")]
        public string ConfirmPassword { get; set; }
    }
}
