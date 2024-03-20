using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace PerfumesStore.Models
{
    public class RegistrationViewModel
    {
        [Display(Name = "Username")]
        [Required(ErrorMessage = "Username is required")]
        [StringLength(16, MinimumLength = 4, ErrorMessage = "Username must be 4-12 chars in length")]
        [RegularExpression("[0-9a-zA-Z]*", ErrorMessage = "Username must be only latin chars and digits")]
        public string Username { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Incorect email address format")]
        public string Email { get; set; }

		[Display(Name = "Address")]
		//[Required(ErrorMessage = "Address is required")]
		public string Address { get; set; }

		[Display(Name = "Password")]
        [Required(ErrorMessage = "Password is required")]
        [StringLength(12, MinimumLength = 4, ErrorMessage = "Password must be 4-12 chars in length")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])[a-zA-Z\d]+$", ErrorMessage = "Password must contain at least one digit uppercase and lowercase symbols")]
        public string Password { get; set; }
    }
}
