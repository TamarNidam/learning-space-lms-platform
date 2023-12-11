using System.ComponentModel.DataAnnotations;

namespace Learning_Space.DTO
{
    public class UserSignUpDTO
    {
        public int UserId { get; set; }
        [RegularExpression(@"^[a-zA-Z0-9]+$",
            ErrorMessage = "Invalid characters in the Username field.")]
        public string FirstName { get; set; }
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$",
          ErrorMessage = "Invalid password. Password must be at least 8 characters long and contain at least one letter and one digit.")]
        public string Password { get; set; }
    }
}
