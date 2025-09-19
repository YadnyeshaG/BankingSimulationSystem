namespace BankingSimulationSystemMVC.Models
{
    using System.ComponentModel.DataAnnotations;

    public class RegisterRequest
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
    

    public class LoginRequest
    {
        public string UserName{ get; set; }

        public string Password{ get; set; }

    }

    public class LoginResponse
    {
        public string Token { get; set; }

        public string UserName { get; set; }        

        public DateTime Expiration { get; set; }

    }

}
