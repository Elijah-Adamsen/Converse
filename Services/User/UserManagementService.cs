using Converse.Models;
using Converse.Data;

namespace Converse.Services.User
{
    public class UserManagementService
    {
        private readonly UserDb _userDb;

        public UserManagementService(UserDb userDb)
        {
            _userDb = userDb;
        }

        // Retrieve a user by phone number
        public UserData GetUser(string phoneNumber)
        {
            var user = _userDb.GetUser(phoneNumber);
            if (user != null) return user;
            return null;  // User not found
        }

        public bool UpdateUser(string phoneNumber, string newName, string newPhone)
        {
            if (_userDb.UpdateUser(phoneNumber, newName, newPhone)) return true;
            return false;
        }

        public bool RemoveUser(string phoneNumber)
        {
            if (_userDb.RemoveUser(phoneNumber)) return true;
            return false;
        }

    }
}