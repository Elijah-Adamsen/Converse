using Microsoft.AspNetCore.Mvc;
using Converse.Services.User;
using Converse.Models;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace Converse.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly RegistrationService _registrationService;
        private readonly UserManagementService _userManagementService;
        private readonly AuthenticationService _authenticationService;

        public UserController(RegistrationService registrationService, UserManagementService userManagementService, AuthenticationService authenticationService)
        {
            _registrationService = registrationService;
            _userManagementService = userManagementService;
            _authenticationService = authenticationService;
        }

        [HttpGet("{phoneNumber}")]
        public IActionResult GetUser(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return BadRequest("Phone number is required.");

            var user = _userManagementService.GetUser(phoneNumber);
            return user != null ? Ok(user) : NotFound("User not found");
        }

        [HttpDelete("{phoneNumber}")]
        public IActionResult RemoveUser(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return BadRequest("Phone number is required.");

            return _userManagementService.RemoveUser(phoneNumber)
                ? Ok(new { message = "User removed successfully." })
                : NotFound("User not found");
        }

        [HttpPut("update")]
        public IActionResult UpdateUser([FromBody] UpdateUserRequest user)
        {
            if (string.IsNullOrWhiteSpace(user.SenderPhone))
                return BadRequest("Phone number is required.");

            var existingUser = _userManagementService.GetUser(user.SenderPhone);
            if (existingUser == null)
                return NotFound("User not found.");

            user.NewPhone ??= user.SenderPhone;
            user.Name ??= existingUser.Name;

            bool result = _userManagementService.UpdateUser(user.SenderPhone, user.Name, user.NewPhone);
            return result ? Ok("User updated successfully.") : StatusCode(500, "Update failed.");
        }

        [AllowAnonymous] // Allow registration without authentication
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserData request)
        {
            if (string.IsNullOrWhiteSpace(request.SenderPhone) || string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Invalid input. Phone number and name are required.");

            var result = await _registrationService.RegisterUserAsync(request.SenderPhone, request.Name);
            if (!result)
                return BadRequest("User already exists or invalid input.");

            // Generate JWT token after successful registration
            var token = _authenticationService.GenerateJwtToken(request.SenderPhone);

            return Ok(new { Message = "User registered successfully.", Token = token });
        }

        [AllowAnonymous] // Allow authentication without a token
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody] AuthenticateRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.SenderPhone))
                return BadRequest("Invalid authentication request.");

            var token = _authenticationService.GenerateJwtToken(request.SenderPhone);
            return token != null ? Ok(new { Token = token }) : Unauthorized("Invalid phone number.");
        }
    }

    // DTO (Data Transfer Object) for updation request
    public class UpdateUserRequest
    {
        public string SenderPhone { get; set; }
        public string? Name { get; set; }
        public string? NewPhone { get; set; }
    }

    // DTO for login/authentication request
    public class AuthenticateRequest
    {
        public string SenderPhone { get; set; }
    }
}