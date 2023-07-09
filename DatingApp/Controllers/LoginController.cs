using DatingApp.Data;
using DatingApp.DTOs;
using DatingApp.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace DatingApp.Controllers
{
    public class LoginController: BaseApiController
    {
        private readonly DataContext _context;

        private readonly ITokenSevice _tokenSevice;
        public LoginController(DataContext context,ITokenSevice services)
        {
            _context = context; 
            _tokenSevice = services;
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _context.Users.
                Include(p=>p.Photos).
                SingleOrDefaultAsync(x => x.UserName == loginDto.UserName.ToLower());

            if (user == null) return Unauthorized("Username or Password invalid");

            using var hmac = new HMACSHA512(user.PasswordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Username or Password invalid");
            }

            return new UserDto
            {
                UserName = loginDto.UserName,
                Token = _tokenSevice.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x=>x.IsMain)?.Url,
                KnownAs = user.KnownAs
            };

        }
    }
}
