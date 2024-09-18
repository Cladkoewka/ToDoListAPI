using Microsoft.AspNetCore.Mvc;
using ToDoList.Application.DTOs.Task;
using ToDoList.Application.Services.Interfaces;

namespace ToDoList.API.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskGetDto>>> GetAllTasks()
    {
        var tasks = await _taskService.GetAllTasksAsynt();
        return Ok(tasks);
    }
    
    [HttpGet("by-tags")]
    public async Task<ActionResult<IEnumerable<TaskGetDto>>> GetTasksByTags([FromQuery] IEnumerable<int> tagIds)
    {
        var tasks = await _taskService.GetTasksByTagsAsync(tagIds);
        return Ok(tasks);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskGetDto>> GetTaskById(int id)
    {
        var task = await _taskService.GetTaskByIdAsync(id);
        if (task == null)
            return NotFound($"Task with ID {id} not found");

        return Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskGetDto>> AddTask([FromBody] TaskCreateDto taskDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var createdTask = await _taskService.CreateTaskAsync(taskDto);

        if (createdTask == null)
            return BadRequest("Task could not be created.");

        return CreatedAtAction(nameof(GetTaskById), new { id = createdTask.Id }, createdTask);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateTask(int id, [FromBody] TaskUpdateDto taskDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var success = await _taskService.UpdateTaskAsync(id, taskDto);
        if (!success)
            return NotFound($"Task with ID {id} not found.");

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteTask(int id)
    {
        var success = await _taskService.DeleteTaskAsync(id);
        if (!success)
            return NotFound($"Task with ID {id} not found.");

        return NoContent();
    }
}