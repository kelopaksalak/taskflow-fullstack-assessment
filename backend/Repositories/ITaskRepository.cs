using TaskFlow.Api.Models;

namespace TaskFlow.Api.Repositories;

public interface ITaskRepository
{
    Task<List<TaskItem>> GetAllAsync();
    Task<TaskItem?> GetByIdAsync(int id);
    Task AddAsync(TaskItem task);
    void Remove(TaskItem task);
    Task SaveChangeAsync();
}