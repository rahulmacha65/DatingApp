using DatingApp.DTOs;
using DatingApp.Entities;
using DatingApp.HelperClasses;

namespace DatingApp.Interfaces
{
    public interface IUserRepository
    {
        void Update(AppUser user);
        //Task<bool> SaveAllAsync();
        Task<IEnumerable<AppUser>> GetUsersAsync();
        Task<AppUser> GetUserByIdAsync(int id);
        Task<AppUser> GetUserByUsernameAsync(string username);
        Task<MemberDto> GetMemberAsync(string userName);
        Task<PagedList<MemberDto>> GetAllMembersAsync(UserParams userParams);
        Task<string> GetGender(string userName);
    }
}
