using System.ComponentModel.DataAnnotations;

namespace Learning_Space.DTO
{
    public class UserSignUpDTO
    {
        public int UserId { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
         public string Password { get; set; }
    }
}
