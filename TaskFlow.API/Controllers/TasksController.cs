using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskFlow.API.DTOs;
using TaskFlow.API.Services;

namespace TaskFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Wrong token");
            }
            return userId;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TaskCreateDto dto)
        {
            var userId = GetUserId();
            var taskDto = await _taskService.CreateTaskAsync(dto, userId);
            return CreatedAtAction(nameof(GetTaskById), new { id = taskDto.Id }, taskDto);
        }

        [HttpGet]
        public async Task<IActionResult> GetTasks([FromQuery] bool? isCompleted = null, [FromQuery] DateTime? dueDate = null)
        {
            var userId = GetUserId();
            var tasks = await _taskService.GetTasksAsync(userId, isCompleted, dueDate);
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            var userId = GetUserId();
            var tasks = await _taskService.GetTasksAsync(userId);
            var task = tasks.FirstOrDefault(t => t.Id == id);
            if (task == null)
            {
                return NotFound("Task not found");
            }
            return Ok(task);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskCreateDto dto)
        {
            var userId = GetUserId();
            try
            {
                var taskDto = await _taskService.UpdateTaskAsync(id, dto, userId);
                return Ok(taskDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var userId = GetUserId();
            var deleted = await _taskService.DeleteTaskAsync(id, userId);
            if (!deleted)
            {
                return NotFound("Task not found ot access denied");
            }
            return NoContent();
        }
    }
}