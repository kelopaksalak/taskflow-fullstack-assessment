using System.ComponentModel.DataAnnotations;
using TaskFlow.Api.Models;

namespace TaskFlow.Api.DTOs;

public class SaveTaskRequest
{
    [Required]
    [MaxLength(200, ErrorMessage = "Title must not exceed 200 characters.")]
    public string Title {get; set;} = string.Empty;

    [MaxLength(200, ErrorMessage = "Description must not exceed 200 characters.")]
    public string? Description {get; set;}

    [EnumDataType(typeof(TaskPriority), ErrorMessage = "Priority must be Low, Medium or High")]
    public TaskPriority Priority {get; set;} = TaskPriority.Medium;

    public DateOnly? DueDate {get; set;}
}