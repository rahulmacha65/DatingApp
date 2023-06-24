using DatingApp.Entities;

namespace DatingApp.Interfaces
{
    public interface ITokenSevice
    {
        string CreateToken(AppUser user);
    }
}
