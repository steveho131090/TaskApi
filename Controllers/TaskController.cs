using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskApi.Data;
using TaskApi.Dtos;
using TaskApi.Models;

namespace TaskApi.Controllers; 

[ApiController]
[Route("api/[controller]")] // Base route: /api/tasks
public class TasksController : ControllerBase
{
    private readonly TaskDbContext _dbContext;
    private readonly ILogger<TasksController> _logger;

    public TasksController(TaskDbContext dbContext, ILogger<TasksController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    // Private helper for mapping Entity to DTO
    private static TaskDto MapTaskToDto(Item task) =>
        new TaskDto(task.Id, task.Title, task.Description, task.DueDate, task.Status);

    // GET: api/tasks/get-all
    [HttpGet("get-all")] // Unique route
    [ProducesResponseType(typeof(IEnumerable<TaskDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasks()
    {
        var tasks = await _dbContext.Tasks
                                  .OrderBy(t => t.Id)
                                  .Select(t => MapTaskToDto(t)) 
                                  .ToListAsync();
        return Ok(tasks);
    }

    // POST: api/tasks/get-by-id
    [HttpPost("get-by-id")] // Unique route
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> GetTaskById([FromBody] GetTaskByIdDto getDto) // Accepts ID in body DTO
    {
        var task = await _dbContext.Tasks.FindAsync(getDto.Id);

        if (task == null)
        {
            return NotFound($"Task with ID {getDto.Id} not found.");
        }

        return Ok(MapTaskToDto(task)); // Uses corrected helper
    }

    // POST: api/tasks/create
    [HttpPost("create")] // Unique route
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskDto taskDto)
    {
         if (string.IsNullOrWhiteSpace(taskDto.Title))
         {
             ModelState.AddModelError(nameof(taskDto.Title), "Task title cannot be empty.");
             return ValidationProblem(ModelState);
         }
        var taskItem = new Item
        {
            Title = taskDto.Title,
            Description = taskDto.Description,
            DueDate = taskDto.DueDate,
            Status = "Pending" // Default status
        };

        try
        {
            await _dbContext.Tasks.AddAsync(taskItem);
            await _dbContext.SaveChangesAsync();

            var createdTaskDto = MapTaskToDto(taskItem); 
             return StatusCode(StatusCodes.Status201Created, createdTaskDto); 
        }
        catch (DbUpdateException ex)
        {
             return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while saving the task.");
        }
         catch (Exception ex)
        {
             return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    // PATCH: api/tasks/update-status
    [HttpPatch("update-status")] 
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> UpdateTaskStatus([FromBody] UpdateTaskStatusWithIdDto patchDto)
    {
         if (string.IsNullOrWhiteSpace(patchDto.Status))
         {
             ModelState.AddModelError(nameof(patchDto.Status), "Status cannot be empty.");
             return ValidationProblem(ModelState);
         }

        try
        {
            var taskItem = await _dbContext.Tasks.FindAsync(patchDto.Id); 

            if (taskItem == null)
            {
                return NotFound($"Task with ID {patchDto.Id} not found.");
            }

            taskItem.Status = patchDto.Status; 
            await _dbContext.SaveChangesAsync();

            return Ok(MapTaskToDto(taskItem)); 
        }
        catch (DbUpdateConcurrencyException ex)
        {
             return Conflict("The task was modified by another user. Please refresh and try again.");
        }
        catch (Exception ex)
        {
             return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the task status.");
        }
    }

    // DELETE: api/tasks/delete
    [HttpDelete("delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask([FromBody] DeleteTaskDto deleteDto)
    {
        try
        {
            var taskItem = await _dbContext.Tasks.FindAsync(deleteDto.Id); 

            if (taskItem == null)
            {
                return NotFound($"Task with ID {deleteDto.Id} not found.");
            }

            _dbContext.Tasks.Remove(taskItem);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
             return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the task.");
        }
    }
}
