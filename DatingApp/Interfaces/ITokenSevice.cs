using DatingApp.Entities;

namespace DatingApp.Interfaces
{
    public interface ITokenSevice
    {
        Task<string> CreateToken(AppUser user);
    }
}
