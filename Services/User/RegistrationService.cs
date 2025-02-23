using Converse.Models;
using Converse.Data;

namespace Converse.Services.User
{
    public class RegistrationService
    {
        private readonly UserDb _userDb;

        public RegistrationService(UserDb userDb)
        {
            _userDb = userDb;
        }

        // Register a user with phone number
        public async Task<bool> RegisterUserAsync(string phoneNumber, string name)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            // Check if the user already exists based on the phone number
            var existingUser = _userDb.GetUser(phoneNumber);
            if (existingUser != null)
                return false;  // User already exists

            // Create a new UserData object
            var user = new UserData
            {
                SenderPhone = phoneNumber,
                Name = name
            };

            // Add the new user to the UserDb
            _userDb.AddUser(user);

            return true;
        }
    }
}