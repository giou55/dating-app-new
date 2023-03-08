using api.DTOs;
using api.Entities;
using api.Helpers;

namespace api.Interfaces
{
    public interface IUserRepository
    {
        void Update(AppUser user);
        void Remove(AppUser user);

        Task<IEnumerable<AppUser>> GetUsersAsync();
        Task<AppUser> GetUserByIdAsync(int id);
        Task<AppUser> GetUserByUsernameAsync(string username);
        Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams);

        Task<MemberDto> GetMemberAsync(string username, bool isCurrentUser);
        Task<string> GetUserGender(string username);
        Task<AppUser> GetUserByPhotoId(int photoId);
    }
}