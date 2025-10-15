using Microsoft.AspNetCore.Identity;

namespace apiv4.Models
{
    public class ApiUser:IdentityUser
    {
        public string FirstName = "";
        public string LastName = "";
    }
}
