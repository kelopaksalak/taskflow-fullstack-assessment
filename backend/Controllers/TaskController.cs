using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.DTOs;
using TaskFlow.Api.Models;
using TaskFlow.Api.Services;

namespace TaskFlow.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/tasks")]
public class TaskController : ControllerBase
{
    private readonly ITaskService _service;

    public TaskController(ITaskService taskService)
    {
        _service = taskService;
    }

    [HttpGet]
    public async Task<ActionResult<List<TaskItem>>> GetAll()
    {
        var tasks = await _service.GetAllAsync();

        return Ok(tasks);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskItem>> GetById(int id)
    {
        var task = await _service.GetByIdAsync(id);

        if (task is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Task not found",
                detail: $"No task exists with ID {id}."
            );
        }

        return Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskItem>> Create(SaveTaskRequest request)
    {
        var task = await _service.CreateAsync(request);

        // 201 created
        return CreatedAtAction("GetById", new {id = task.Id}, task);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TaskItem>> Update(int id, SaveTaskRequest request)
    {
        var task = await _service.UpdateAsync(request, id);

        if (task is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Task not found",
                detail: $"No task exists with ID {id}."
            );
        }

        return Ok(task);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<TaskItem>> ToggleStatus(int id)
    {
        var task = await _service.ToggleStatusAsync(id);
        if (task is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Task not found",
                detail: $"No task exists with ID {id}."
            );
        }

        return Ok(task);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Task not found",
                detail: $"No task exists with ID {id}."
            );
        }
        return NoContent();
    }
}