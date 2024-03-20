using System.ComponentModel.DataAnnotations;

namespace PerfumesStore.Models
{
    public class LoginViewModel
    {
        [Display(Name = "Username")]
        [Required(ErrorMessage = "Username is required")]
        [StringLength(16, MinimumLength = 4, ErrorMessage = "Username must be 4-12 chars in length")]
        [RegularExpression("[0-9a-zA-Z]*", ErrorMessage = "Username must not contain special chars")]
        public string Username { get; set; }

        [Display(Name = "Password")]
        [Required(ErrorMessage = "Password is required")]
        [StringLength(12, MinimumLength = 4, ErrorMessage = "Password must be 4-12 chars in length")]
        [RegularExpression("[0-9a-zA-Z]*", ErrorMessage = "Username must be only latin chars and digits")]
        public string Password { get; set; }

    }
}
