using TaskFlow.API.DTOs;

namespace TaskFlow.API.Services
{
    public interface ITaskService
    {
        Task<TaskDto> CreateTaskAsync(TaskCreateDto dto);
        Task<List<TaskDto>> GetTasksByUserIdAsync(int userId, bool? isCompleted = null, DateTime? dueDate = null);
        Task<TaskDto> UpdateTaskAsync(int id, TaskCreateDto dto, int userId);
        Task<bool> DeleteTaskAsync(int id);
    }
}