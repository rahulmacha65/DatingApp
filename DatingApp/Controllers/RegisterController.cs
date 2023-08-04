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
        private readonly UserManager<AppUser> _userManager;

        private readonly ITokenSevice _tokenSevice;
        private readonly IMapper _mapper;
        public RegisterController(UserManager<AppUser> userManager, ITokenSevice tokenSevice,IMapper mapper)
        {
             _userManager = userManager;
            _tokenSevice = tokenSevice;
            _mapper = mapper;
        }
        [HttpPost] // POST: api/register
        public async Task<ActionResult<UserDto>> Register(RegisterDTO registerDTO)
        {
            if(await UserExists(registerDTO.UserName)) return BadRequest("Username already used.");

            var user = _mapper.Map<AppUser>(registerDTO);

            user.UserName = registerDTO.UserName.ToLower();

            var result =await _userManager.CreateAsync(user,registerDTO.Password);

            if (!result.Succeeded) return BadRequest("Use Strong Password. ex:- Pa$$w0rd");

            var roleResults = await _userManager.AddToRoleAsync(user, "Member");

            if (!roleResults.Succeeded) return BadRequest("Error occurred try after some time.");
            return new UserDto
            {
                UserName = registerDTO.UserName,
                Token = await _tokenSevice.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender,
            };
        }
        private async Task<bool> UserExists(string userName)
        {
            return await _userManager.Users.AnyAsync(x => x.UserName == userName.ToLower());
        }

    }
}
