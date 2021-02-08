using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class ResetPasswordLinkModel
    {
        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "RedirectUrl is required")]
        public string RedirectUrl { get; set; }
    }
}
