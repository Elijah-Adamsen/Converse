using Microsoft.AspNetCore.Mvc;
using Converse.Services.User;
using Converse.Services.Group;
using Converse.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;

namespace Converse.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class GroupController : ControllerBase
    {
        private readonly GroupManagementService _groupManagementService;

        public GroupController(GroupManagementService groupManagementService)
        {
            _groupManagementService = groupManagementService;
        }

        [HttpPost]
        public IActionResult CreateGroup([FromBody] GroupData group)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (_groupManagementService.CreateGroup(group))
                return CreatedAtAction(nameof(GetGroup), new { groupId = group.GroupId }, group);

            return Conflict(new { message = $"Group '{group.GroupId}' already exists." });
        }

        [HttpGet("{groupId}")]
        public IActionResult GetGroup(string groupId)
        {
            var group = _groupManagementService.GetGroupById(groupId);
            return group != null ? Ok(group) : NotFound(new { message = $"Group '{groupId}' not found." });
        }

        [HttpDelete("{groupId}")]
        public IActionResult DeleteGroup(string groupId)
        {
            if (_groupManagementService.DeleteGroup(groupId))
                return Ok(new { message = $"Group '{groupId}' removed." });

            return NotFound(new { message = $"No such group '{groupId}' exists." });
        }

        [HttpPost("members")]
        public IActionResult AddMember([FromBody] GroupMembers member)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (_groupManagementService.AddMemberToGroup(member.GroupId, member.SenderPhone))
                return Ok(new { message = $"Member '{member.SenderPhone}' added to group '{member.GroupId}'." });

            return BadRequest(new { message = $"Failed to add member '{member.SenderPhone}'." });
        }

        [HttpDelete("members")]
        public IActionResult RemoveMember([FromBody] GroupMembers member)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (_groupManagementService.RemoveMemberFromGroup(member.GroupId, member.SenderPhone))
                return Ok(new { message = $"Member '{member.SenderPhone}' removed from group '{member.GroupId}'." });

            return NotFound(new { message = $"Member '{member.SenderPhone}' not found in group '{member.GroupId}'." });
        }

        [HttpGet("{groupId}/members")]
        public IActionResult GetGroupMembers(string groupId)
        {
            var members = _groupManagementService.GetGroupMembers(groupId);
            return members.Count != 0 ? Ok(members) : NotFound(new { message = $"No members found for group '{groupId}'." });
        }
    }

    public class GroupMembers
    {
        [Required]
        public string GroupId { get; set; }

        [Required]
        public string SenderPhone { get; set; }
    }
}