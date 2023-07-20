using DatingApp.DTOs;
using DatingApp.Entities;
using DatingApp.HelperClasses;

namespace DatingApp.Interfaces
{
    public interface ILikesRepository
    {
        Task<UserLike> GetUserLike(int souceUserId, int targetUserId);
        Task<AppUser> GetUserWithLikes(int UserId);
        Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams);
    }
}
