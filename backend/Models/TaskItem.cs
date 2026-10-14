namespace TaskFlow.Api.Models;

public class TaskItem
{   
    public int Id {get; set;}
    public string Title {get; set;} = string.Empty;
    public string? Description {get; set;}
    public TaskPriority Priority {get; set;} = TaskPriority.Medium;
    public DateOnly? DueDate {get; set;}
    public TaskItemStatus Status {get; set;} = TaskItemStatus.Todo;
}