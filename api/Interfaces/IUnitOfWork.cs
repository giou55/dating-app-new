namespace api.Interfaces
{
    public interface IUnitOfWork
    {
        IUserRepository UserRepository {get;}
        IMessageRepository MessageRepository {get;}
        ILikesRepository LikesRepository {get;}

        // if the Complete doesn't work, then everything rolls back to what it was before
        Task<bool> Complete();

        // this will tell us if Entity framework is tracking anything 
        // that's been changed inside its transaction 
        bool HasChanges();
    }
}