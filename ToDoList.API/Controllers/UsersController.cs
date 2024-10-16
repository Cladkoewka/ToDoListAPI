using Microsoft.AspNetCore.Mvc;
using Serilog;
using ToDoList.Application.DTOs.User;
using ToDoList.Application.Services.Interfaces;
using ILogger = Serilog.ILogger;

namespace ToDoList.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger _logger;

    public UsersController(IUserService userService)
    {
        _userService = userService;
        _logger = Log.ForContext<UsersController>();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserGetDto>>> GetAllUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while getting users.");
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpGet("test")]
    public async Task<ActionResult<string>> Test()
    {
        return Ok("Hello World");
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserGetDto>> GetUserById(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound($"User with ID {id} not found.");

        return Ok(user);
    }
    
    [HttpGet("email/{email}")]
    public async Task<ActionResult<UserGetDto>> GetUserByEmail(string email)
    {
        var user = await _userService.GetUserByEmailAsync(email);
        if (user == null)
            return NotFound($"User with email {email} not found.");

        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult> AddUser([FromBody] UserCreateDto userDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.Warning("Invalid model state for user creation.");
            return BadRequest(ModelState);
        }

        var createdUser = await _userService.CreateUserAsync(userDto);

        if (createdUser == null)
            return BadRequest("User could not be created.");

        return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateUser(int id, [FromBody] UserUpdateDto userDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.Warning("Invalid model state for user update.");
            return BadRequest(ModelState);
        }

        var success = await _userService.UpdateUserAsync(id, userDto);
        if (!success)
            return NotFound($"User with ID {id} not found.");

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        var success = await _userService.DeleteUserAsync(id);
        if (!success)
            return NotFound($"User with ID {id} not found.");

        return NoContent();
    }
}