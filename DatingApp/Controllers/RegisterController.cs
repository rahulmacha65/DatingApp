using AutoMapper;
using DatingApp.Data;
using DatingApp.DTOs;
using DatingApp.Entities;
using DatingApp.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace DatingApp.Controllers
{
    public class RegisterController:BaseApiController
    {
        private readonly DataContext _context;

        private readonly ITokenSevice _tokenSevice;
        private readonly IMapper _mapper;
        public RegisterController(DataContext context, ITokenSevice tokenSevice,IMapper mapper)
        {
             _context = context;
            _tokenSevice = tokenSevice;
            _mapper = mapper;
        }
        [HttpPost] // POST: api/register
        public async Task<ActionResult<UserDto>> Register(RegisterDTO registerDTO)
        {
            if(await UserExists(registerDTO.UserName)) return BadRequest("Username already used.");

            var user = _mapper.Map<AppUser>(registerDTO);

            using var hmac = new HMACSHA512();

            user.UserName = registerDTO.UserName.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password));
            user.PasswordSalt = hmac.Key;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDto
            {
                UserName = registerDTO.UserName,
                Token = _tokenSevice.CreateToken(user),
                KnownAs = user.KnownAs,
            };
        }
        private async Task<bool> UserExists(string userName)
        {
            return await _context.Users.AnyAsync(x => x.UserName == userName.ToLower());
        }

    }
}
