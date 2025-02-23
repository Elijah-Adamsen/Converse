using MongoDB.Driver;
using Converse.Models;

namespace Converse.Data
{
    public class UserDb
    {
        private readonly IMongoCollection<UserData> _users;
        public UserDb(IMongoDatabase database)
        {
            try
            {
                _users = database.GetCollection<UserData>("users");
                Console.WriteLine("Successfully connected to MongoDB.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("MongoDB connection error: ", ex.Message);
            }
        }

        public List<UserData> GetAllUsers() => _users.Find(_ => true).ToList();

        public UserData GetUser(string userId) => _users.Find(user => user.SenderPhone == userId).FirstOrDefault();

        public bool RemoveUser(string phoneNumber)
        {
            var result = _users.DeleteOne(user => user.SenderPhone == phoneNumber);
            if (result.IsAcknowledged) if (result.DeletedCount > 0) return true;
            return false;
        }

        public bool AddUser(UserData user)
        {
            try
            {
                _users.InsertOne(user);
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        public bool UpdateUser(string phoneNumber, string newName, string newPhone)
        {
            var filter = Builders<UserData>.Filter.Eq(user => user.SenderPhone, phoneNumber);

            var updateDetails = Builders<UserData>.Update
            .Set(user => user.Name, newName)
            .Set(user => user.SenderPhone, newPhone);

            var updateResult = _users.UpdateOne(filter, updateDetails);

            if (updateResult.ModifiedCount > 0) return true;
            return false;
        }
    }
}