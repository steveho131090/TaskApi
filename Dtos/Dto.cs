namespace TaskApi.Dtos; 

public record TaskDto(int Id, string Title, string? Description, DateTime? DueDate, string Status);
public record CreateTaskDto(string Title, string? Description, DateTime? DueDate);

public record GetTaskByIdDto(int Id); 

public record UpdateTaskStatusWithIdDto(int Id, string Status);

public record DeleteTaskDto(int Id);