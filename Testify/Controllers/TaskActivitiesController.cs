using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Testify.Entities;
using Testify.Interfaces.Testify.Interfaces;
using Testify.Shared.DTOs.TaskActivity;

namespace Testify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TaskActivitiesController : ControllerBase
    {
        private readonly ITaskActivityRepository _taskActivityRepository;

        public TaskActivitiesController(ITaskActivityRepository taskActivityRepository)
        {
            _taskActivityRepository = taskActivityRepository;
        }

        // GET: api/TaskActivities/task/5
        [HttpGet("task/{taskId}")]
        public async Task<ActionResult<IEnumerable<TaskActivityResponse>>> GetActivitiesByTaskId(int taskId)
        {
            var activities = await _taskActivityRepository.GetActivitiesByTaskIdAsync(taskId);
            return Ok(activities);
        }

        // POST: api/TaskActivities
        //[HttpPost]
        //public async Task<ActionResult<TaskActivity>> CreateActivity(TaskActivity activity)
        //{
        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    var userName = User.Identity?.Name ?? "System";

        //    if (string.IsNullOrEmpty(userId))
        //    {
        //        return Unauthorized();
        //    }

        //    activity.UserId = userId;
        //    activity.CreatedBy = userName;

        //    var created = await _taskActivityRepository.CreateActivityAsync(activity);

        //    return CreatedAtAction(nameof(GetActivitiesByTaskId), new { taskId = created.TaskId }, created);
        //}
    }
}