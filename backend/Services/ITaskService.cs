using TaskFlow.Api.DTOs;
using TaskFlow.Api.Models;

namespace TaskFlow.Api.Services;

public interface ITaskService
{
    Task<List<TaskItem>> GetAllAsync();
    Task<TaskItem?> GetByIdAsync(int id);
    Task<TaskItem> CreateAsync(SaveTaskRequest req);
    Task<TaskItem?> UpdateAsync(SaveTaskRequest req, int id);
    Task<TaskItem?> ToggleStatusAsync(int id);
    Task<bool> DeleteAsync(int id);
}