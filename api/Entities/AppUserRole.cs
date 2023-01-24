// this entity represents a join table between AppUser and AppRole entities
// because we want a many to many relationship between our app user and our app role

using Microsoft.AspNetCore.Identity;

namespace api.Entities
{
    // we extends AppUserRole class with IdentityUserRole because we want to use Identity
    // we add <int> because we want our Id property to be an int instead of string
    public class AppUserRole : IdentityUserRole<int>
    {
        public AppUser User { get; set; }
        public AppRole Role { get; set; }
    }
}