using HangfireTest.Api.Services;
using HangfireTest.Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace HangfireTest.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IEnumerable<User>> GetUsers()
        {
            return await _userService.GetUsersAsync();
        }
    }
}