using Microsoft.AspNetCore.Http.Features;
using TaskFlow.Api.DTOs;
using TaskFlow.Api.Models;
using TaskFlow.Api.Repositories;

namespace TaskFlow.Api.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _repositories;

    public TaskService(ITaskRepository repositories)
    {
        _repositories = repositories;
    }

    public async Task<List<TaskItem>> GetAllAsync()
    {
        return await _repositories.GetAllAsync();
    } 

    public async Task<TaskItem?> GetByIdAsync(int id)
    {
        return await _repositories.GetByIdAsync(id);
    }

    public async Task<TaskItem> CreateAsync (SaveTaskRequest req)
    {
        var newTask = new TaskItem
        {
            Title = req.Title,
            Description = req.Description,
            Priority = req.Priority,
            DueDate = req.DueDate,
            Status = TaskItemStatus.Todo
        };

        await _repositories.AddAsync(newTask);
        await _repositories.SaveChangeAsync();

        return newTask;
    }

    public async Task<TaskItem?> UpdateAsync(SaveTaskRequest req, int id)
    {
        var task = await _repositories.GetByIdAsync(id);

        if (task == null)
        {
            return null;
        }

        task.Title = req.Title;
        task.Description = req.Description;
        task.Priority = req.Priority;
        task.DueDate = req.DueDate; 

        await _repositories.SaveChangeAsync();

        return task;
    }

    public async Task<TaskItem?> ToggleStatusAsync(int id)
    {
        var task = await _repositories.GetByIdAsync(id);

        if (task is null)
        {
            return null;
        }

        task.Status = task.Status == TaskItemStatus.Todo ? TaskItemStatus.Done : TaskItemStatus.Todo;

        await _repositories.SaveChangeAsync();

        return task; 
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var task = await _repositories.GetByIdAsync(id);

        if (task is null)
        {
            return false;
        }

        _repositories.Remove(task);
        await _repositories.SaveChangeAsync();

        return true;
    }
}