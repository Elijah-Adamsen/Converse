using Converse.Models;
using Converse.Data;

namespace Converse.Services.Group
{
    public class GroupManagementService
    {
        private readonly GroupDb _groupDb;

        public GroupManagementService(GroupDb groupDb)
        {
            _groupDb = groupDb;
        }

        public bool CreateGroup(GroupData group)
        {
            if (_groupDb.GetGroupById(group.GroupId) == null)
            {
                _groupDb.AddGroup(group);
                return true;
            }
            return false;
        }

        public GroupData GetGroupById(string groupId)
        {
            return _groupDb.GetGroupById(groupId);
        }

        public bool RemoveMemberFromGroup(string groupId, string phoneNumber)
        {
            var group = _groupDb.GetGroupById(groupId);
            if (group != null && group.Members.Contains(phoneNumber))
            {
                group.Members.Remove(phoneNumber);
                _groupDb.UpdateGroup(groupId, group);
                return true;
            }
            return false;
        }

        public bool AddMemberToGroup(string groupId, string phoneNumber)
        {
            var group = _groupDb.GetGroupById(groupId);
            if (group != null && !group.Members.Contains(phoneNumber))
            {
                group.Members.Add(phoneNumber);
                _groupDb.UpdateGroup(groupId, group);
                return true;
            }
            return false;
        }

        public List<string> GetGroupMembers(string groupId)
        {
            var group = _groupDb.GetGroupById(groupId);
            return group?.Members ?? new List<string>();
        }

        public bool DeleteGroup(string groupId)
        {
            var group = _groupDb.GetGroupById(groupId);
            if (group != null)
            {
                _groupDb.DeleteGroup(groupId);
                return true;
            }
            return false;
        }
    }
}