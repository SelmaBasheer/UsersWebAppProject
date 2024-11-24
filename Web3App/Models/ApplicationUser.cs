using Microsoft.AspNetCore.Identity;

namespace Web3App.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName {  get; set; }
        public string LastName { get; set; }
        public string Address {  get; set; }
        public DateTime CreatedDate { get; set; }

    }
}
