using api.DTOs;
using api.Entities;
using api.Helpers;

namespace api.Interfaces
{
    public interface IUserRepository
    {
        void Update(AppUser user);
        Task<bool> SaveAllAsync();
        Task<IEnumerable<AppUser>> GetUsersAsync();
        Task<AppUser> GetUserByIdAsync(int id);
        Task<AppUser> GetUserByUsernameAsync(string username);

        // we're not going to  return an enumerable of MemberDto
        //Task<IEnumerable<MemberDto>> GetMembersAsync();
        
        // but we're going to return a PagedList of MemberDto
        Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams);
        Task<MemberDto> GetMemberAsync(string username);

    }
}