using System.ComponentModel.DataAnnotations;

namespace apiv4.Dto
{
    public class UserDto:LoginUserDto
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
    }
}
