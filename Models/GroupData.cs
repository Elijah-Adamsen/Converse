using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Converse.Models
{
    public class GroupData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } // MongoDB will use this as the unique identifier
        public string GroupId { get; set; } // A unique ID for the group (e.g., a UUID)
        public string GroupName { get; set; }
        public List<string> Members { get; set; } = new List<string>(); // List of user phone numbers

        public GroupData()
        {
            Members = new List<string>();
        }
    }
}