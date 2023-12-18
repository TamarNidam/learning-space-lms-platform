using System.ComponentModel.DataAnnotations;

namespace Learning_Space.DTO
{
    public class UserDTO
    {
        public int UserId { get; set; }

        [RegularExpression(@"^[a-zA-Z0-9]+$",
            ErrorMessage = "Invalid characters in the Username field.")]
        [Required]
        public string FirstName { get; set; }

        [RegularExpression(@"^[a-zA-Z]+$",
        ErrorMessage = "Invalid characters in the name field.")]
        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string Phone { get; set; }

        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$",
           ErrorMessage = "Invalid password. Password must be at least 8 characters long and contain at least one letter and one digit.")]
        public string Password { get; set; }
        public string? Role { get; set; }

        //public List<UserTaskDTO> UserTask { get; set; }
    }
}
