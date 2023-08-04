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
    public class LoginController: BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;

        private readonly ITokenSevice _tokenSevice;
        public LoginController(UserManager<AppUser> userManager,ITokenSevice services)
        {
            _userManager = userManager;
            _tokenSevice = services;
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.Users.
                Include(p=>p.Photos).
                SingleOrDefaultAsync(x => x.UserName == loginDto.UserName.ToLower());

            if (user == null) return Unauthorized("Username or Password invalid");

            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (!result) return Unauthorized("Username or Password invalid");

            return new UserDto
            {
                UserName = loginDto.UserName,
                Token = await _tokenSevice.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x=>x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };

        }
    }
}
