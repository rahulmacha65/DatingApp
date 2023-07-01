using DatingApp.DTOs;
using DatingApp.Entities;

namespace DatingApp.Interfaces
{
    public interface IUserRepository
    {
        void Update(AppUser user);
        Task<bool> SaveAllAsync();
        Task<IEnumerable<AppUser>> GetUsersAsync();
        Task<AppUser> GetUserByIdAsync(int id);
        Task<AppUser> GetUserByUsernameAsync(string username);
        Task<MemberDto> GetMemberAsync(string userName);
        Task<IEnumerable<MemberDto>> GetAllMembersAsync();
    }
}
