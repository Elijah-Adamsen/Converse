using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Converse.Services.User;

namespace Converse.Services.User
{
    public class AuthenticationService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManagementService _userManagementService;

        public AuthenticationService(IConfiguration configuration, UserManagementService userManagementService)
        {
            _configuration = configuration;
            _userManagementService = userManagementService;
        }

        // Generates JWT token for authentication
        public string GenerateJwtToken(string phoneNumber)
        {
            var user = _userManagementService.GetUser(phoneNumber);
            if (user == null)
            {
                return null; // User does not exist, prevent token generation
            }

            var secretKey = _configuration["Jwt:SecretKey"];
            if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 32)
            {
                throw new InvalidOperationException("JWT Secret Key must be at least 32 characters long.");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, phoneNumber)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}