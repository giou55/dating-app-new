namespace api.Entities
{
    // this class is going to act as a join table between AppUsers
    public class UserLike
    {
        public AppUser SourceUser { get; set; }
        public int SourceUserId { get; set; }

        // target user is being liked by the source user
        public AppUser TargetUser { get; set; }
        public int TargetUserId { get; set; }
    }
}