using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class ConfirmEmailModel
    {
        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; }
    }
}
