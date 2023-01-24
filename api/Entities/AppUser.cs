using Microsoft.AspNetCore.Identity;

namespace api.Entities
{
    // we extends AppUser class with IdentityUser because we want to use Identity
    // we add <int> because we want our Id property to be an int instead of string
    public class AppUser : IdentityUser<int>
    {
        // we don't need to use these four properties any more, 
        // because Identity is taking care of this for us 
        // public int Id { get; set; }
        // public string UserName { get; set; }
        // public byte[] PasswordHash { get; set; }
        // public byte[] PasswordSalt { get; set; }

        public DateOnly DateOfBirth { get; set; }
        public string KnownAs { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime LastActive { get; set; } = DateTime.UtcNow;
        public string Gender { get; set; }
        public string Introduction { get; set; }
        public string LookingFor { get; set; }
        public string Interests { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public List<Photo> Photos { get; set; }

        // these are the users that like AppUser
        public List<UserLike> LikedByUsers { get; set; }


        // these are the users that the AppUser likes
        public List<UserLike> LikedUsers { get; set; }

        public List<Message> MessagesSent { get; set; }
        public List<Message> MessagesReceived { get; set; }

        // this property is the same to the join table (the AppUserRole entity)
        public ICollection<AppUserRole> UserRoles { get; set; }


        // if add this method, automapper will need full AppUser entity
        // public int GetAge()
        // {
        //     return DateOfBirth.CalculateAge();
        // }
    }
}