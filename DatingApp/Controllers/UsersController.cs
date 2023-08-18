using AutoMapper;
using DatingApp.DTOs;
using DatingApp.Entities;
using DatingApp.Extensions;
using DatingApp.HelperClasses;
using DatingApp.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Security.Claims;

namespace DatingApp.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UsersController :BaseApiController
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        public UsersController(IUnitOfWork uow,IMapper mapper,IPhotoService photoService) 
        {
            _uow = uow;
            _mapper = mapper; 
            _photoService = photoService;
        }
        
        [HttpGet]
        public async Task<ActionResult<PagedList<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        {
            string userName = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            userParams.CurrentUserName = userName;

            var gender = await _uow.UserRepository.GetGender(userName);

            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = gender == "male" ? "female" : "male";
            }

            var users = await _uow.UserRepository.GetAllMembersAsync(userParams);

            Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage,
                users.PageSize,users.TotalCount,users.TotalPages));
            return Ok(users);
        }
        
        [HttpGet("{userName}")]
        public async Task<ActionResult<MemberDto>> GetUser(string userName)
        {
            var user =  await _uow.UserRepository.GetMemberAsync(userName);
            return Ok(user);
        }
        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var userName = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var user  = await _uow.UserRepository.GetUserByUsernameAsync(userName);

            if (user == null) { return NotFound(); }
            //passing memeberUpdateDto values to AppUser properties
            _mapper.Map(memberUpdateDto, user);

            if (await _uow.Complete()) return NoContent();

            return BadRequest("Failed to update. Please check your changes");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            string userName = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _uow.UserRepository.GetUserByUsernameAsync(userName);

            if (user == null) { return NotFound(); }

            //Adding user photo to cloudinary
            var result = await _photoService.AddPhotoAsync(file);

            if (result.Error != null) { return BadRequest(result.Error.Message); }

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId,
            };

            if (user.Photos.Count == 0) { photo.IsMain = true; }

            user.Photos.Add(photo);

            if (await _uow.Complete())
            {
                return  _mapper.Map<PhotoDto>(photo);
            }

            return BadRequest("Problem adding photo. Please try after some time.");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> setMainPhoto(int photoId)
        {
            string userName = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _uow.UserRepository.GetUserByUsernameAsync(userName);
            if (user == null) { return NotFound(); };

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            if (photo == null) { return NotFound(); }

            if (photo.IsMain) return BadRequest("Already Main photo");

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if (currentMain != null) currentMain.IsMain = false;

            photo.IsMain = true;

            if (await _uow.Complete()) return NoContent();

            return BadRequest("Problem setting profile photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            string userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _uow.UserRepository.GetUserByUsernameAsync(userName);

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if(photo==null) { return NotFound(); }

            if (photo.IsMain) return BadRequest("You cannot delete main photo");

            if (photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error);
            }

            user.Photos.Remove(photo);

            if (await _uow.Complete()) return NoContent();

            return BadRequest("Problem while deleting photo");
        }
    }
}
