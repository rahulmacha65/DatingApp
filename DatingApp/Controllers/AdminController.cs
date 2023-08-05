using DatingApp.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Controllers
{
    [Authorize(AuthenticationSchemes =JwtBearerDefaults.AuthenticationScheme)]
    public class AdminController:BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        public AdminController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }
        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUserWithRoles()
        {
            var users = await _userManager.Users
                        .OrderBy(u => u.UserName)
                        .Select(u => new
                        {
                            u.Id,
                            UserName = u.UserName,
                            Roles = u.UserRoles.Select(u => u.Role.Name).ToList()
                        }).ToListAsync();

            return Ok(users);
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("edit-roles/{userName}")]
        public async Task<ActionResult> EditRoles(string userName, [FromQuery]string roles)
        {
            if (string.IsNullOrEmpty(roles)) return BadRequest("You must select at least one role");
            var selectedRoles = roles.Split(",").ToArray();
            var user = await _userManager.FindByNameAsync(userName);

            if (user == null) return NotFound("User not found");

            var userRoles = await _userManager.GetRolesAsync(user);

            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

            if (!result.Succeeded) return BadRequest("Failed to add to roles");

            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

            if (!result.Succeeded) return BadRequest("Failed to remove from roles");

            return Ok(await _userManager.GetRolesAsync(user));
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public ActionResult GetPhotosForModerators()
        {
            return Ok("Admins or moderators can see this");
        }
    }
}
