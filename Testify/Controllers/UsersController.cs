using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Testify.Client.Interfaces;
using Testify.Shared.DTOs.Users;

namespace Testify.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserListItemResponse>>> GetUsers()
    {
        var users = await _userService.GetUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponse>> GetUser(string id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            return Ok(user);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<UserResponse>> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            var user = await _userService.CreateUserAsync(request);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UserResponse>> UpdateUser(string id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            var user = await _userService.UpdateUserAsync(id, request);
            return Ok(user);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(string id)
    {
        var result = await _userService.DeleteUserAsync(id);
        return result ? NoContent() : NotFound();
    }

    [HttpGet("roles")]
    public async Task<ActionResult<List<string>>> GetRoles()
    {
        var roles = await _userService.GetAvailableRolesAsync();
        return Ok(roles);
    }

    [HttpGet("{id}/roles")]
    public async Task<ActionResult<List<string>>> GetUserRoles(string id)
    {
        var roles = await _userService.GetUserRolesAsync(id);
        return Ok(roles);
    }

    [HttpPost("assign-roles")]
    public async Task<ActionResult> AssignRoles([FromBody] AssignRoleRequest request)
    {
        var result = await _userService.AssignRolesAsync(request);
        return result ? NoContent() : BadRequest();
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<UserListItemResponse>>> Search([FromQuery] string term)
    {
        var users = await _userService.SearchUsersAsync(term);
        return Ok(users);
    }

    [HttpGet("by-role/{role}")]
    public async Task<ActionResult<List<UserListItemResponse>>> GetByRole(string role)
    {
        var users = await _userService.GetUsersByRoleAsync(role);
        return Ok(users);
    }
}
