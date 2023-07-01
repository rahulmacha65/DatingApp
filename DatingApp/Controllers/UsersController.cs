using AutoMapper;
using DatingApp.DTOs;
using DatingApp.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace DatingApp.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UsersController :BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        public UsersController(IUserRepository userRepository,IMapper mapper) 
        { 
            _userRepository = userRepository;
            _mapper = mapper;   
        }
        [HttpGet]
        public async Task<ActionResult<List<MemberDto>>> GetUsers()
        {
            var users = await _userRepository.GetAllMembersAsync();

            var usersToReturn = _mapper.Map<List<MemberDto>>(users);
            return Ok(usersToReturn);
        }

        [HttpGet("{userName}")]
        public async Task<ActionResult<MemberDto>> GetUser(string userName)
        {
            var user =  await _userRepository.GetMemberAsync(userName);
            return Ok(_mapper.Map<MemberDto>(user));
        }
    }
}
