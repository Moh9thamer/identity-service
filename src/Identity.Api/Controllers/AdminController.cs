using Identity.Application.DTOs.Users;
using Identity.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;
        public AdminController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsersAsync() 
        { 
            var result = await _userService.GetAllUsersAsync();

            return Ok(result);
        }

        [HttpPatch("users/{id}/role")]
        public async Task<IActionResult> UpdateUserRole(Guid id, UpdateRoleRequest request)
        {
             await _userService.UpdateRoleAsync(id, request);

            return NoContent();
        }
    }
}
