using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace PlugIn.Contracts
{
    public class RegisterForm
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        // Allow up to 40 uppercase and lowercase 
        // characters. Use custom error.
        //[RegularExpression(@"\W{4,12}$", //(@"^(?!.([A-Za-z0-9])\1{1})(?=.?[A-Z])(?=.?[a-z])(?=.?[0-9])(?=.?[#?!@$%^&-]).{1,40}$",
        //     ErrorMessage = "Only numbers.")]
        public string Password { get; set; } = string.Empty;
    }
}