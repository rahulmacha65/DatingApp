using DatingApp.DTOs;
using DatingApp.Extensions;
using DatingApp.HelperClasses;
using DatingApp.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DatingApp.Controllers
{
    public class LikesController:BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly ILikesRepository _likesRepository;
        public LikesController(IUserRepository userRepository,ILikesRepository likesRepository)
        {
            _userRepository = userRepository;
            _likesRepository = likesRepository;
        }

        [HttpPost("{userName}")]
        public async Task<ActionResult> AddLike(string userName)
        {
            int sourceUserId = Convert.ToInt32(User.FindFirst(ClaimTypes.Name).Value);
            var likedUser = await _userRepository.GetUserByUsernameAsync(userName);
            var sourceUser = await _likesRepository.GetUserWithLikes(sourceUserId);

            if (likedUser == null) return BadRequest();

            if(sourceUser.UserName == userName) return BadRequest("You cannot like yourself");

            var userLike = await _likesRepository.GetUserLike(sourceUserId, likedUser.Id);

            if (userLike != null) return BadRequest("You already liked this user");

            userLike = new Entities.UserLike
            {
                SourceUserId = sourceUserId,
                TargetUserId = likedUser.Id,
            };

            sourceUser.LikedUsers.Add(userLike);

            if (await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed to like user");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
        {
            likesParams.UserId = Convert.ToInt32(User.FindFirst(ClaimTypes.Name).Value);
            var users = await _likesRepository.GetUserLikes(likesParams);

            Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize,
                users.TotalCount, users.TotalPages));
            return Ok(users);
        }
    }
}
