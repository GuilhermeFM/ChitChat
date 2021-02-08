using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class SignUpModel
    {
        [Required(ErrorMessage = "Fullname is required")]
        public string Fullname { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        [Required(ErrorMessage = "RedirectUrl is required")]
        public string RedirectUrl { get; set; }
    }
}
