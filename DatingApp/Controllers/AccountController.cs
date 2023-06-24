using DatingApp.Data;
using DatingApp.DTOs;
using DatingApp.Entities;
using DatingApp.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace DatingApp.Controllers
{
    public class AccountController:BaseApiController
    {
        private readonly DataContext _context;

        private readonly ITokenSevice _tokenSevice;
        public AccountController(DataContext context, ITokenSevice tokenSevice)
        {
             _context = context;
            _tokenSevice = tokenSevice;
        }
        [HttpPost("/register")] // POST: api/Account/register
        public async Task<ActionResult<UserDto>> Register(RegisterDTO registerDTO)
        {
            if(await UserExists(registerDTO.UserName)) return BadRequest("Username already used.");

            using var hmac = new HMACSHA512();

            var user = new AppUser
            {
                UserName = registerDTO.UserName.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password)),
                PasswordSalt = hmac.Key
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDto
            {
                UserName = registerDTO.UserName,
                Token = _tokenSevice.CreateToken(user)
            };
        }
        [HttpPost("/login")]
        public async Task <ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.UserName == loginDto.UserName);

            if (user == null) return Unauthorized("Username is invalid");

            using var hmac = new HMACSHA512(user.PasswordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for(int i=0;i<computedHash.Length;i++)
            {
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Password is invalid");
            }

            return new UserDto
            {
                UserName = loginDto.UserName,
                Token = _tokenSevice.CreateToken(user)
            };

        }
        private async Task<bool> UserExists(string userName)
        {
            return await _context.Users.AnyAsync(x => x.UserName == userName.ToLower());
        }

    }
}
