using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MasterDataApi.DTOs;
using MasterDataApi.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace MasterDataApi.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
[Produces("application/json")]
[Tags("Users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;

    public UsersController(IUserService service)
    {
        _service = service;
    }

    [HttpGet]
    [SwaggerOperation(OperationId = "GetAllUsers")]
    [SwaggerResponse(200, "List of users", typeof(IEnumerable<UserResponse>))]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id:guid}")]
    [SwaggerOperation(OperationId = "GetUserById")]
    [SwaggerResponse(200, "User found", typeof(UserResponse))]
    [SwaggerResponse(404, "User not found", typeof(ErrorResponse))]
    public async Task<ActionResult<UserResponse>> GetById(Guid id)
    {
        var user = await _service.GetByIdAsync(id);
        if (user == null)
            return NotFound(new ErrorResponse { Error = "User not found", Code = "USER_NOT_FOUND" });
        return Ok(user);
    }

    [HttpPost]
    [SwaggerOperation(OperationId = "CreateUser")]
    [SwaggerResponse(201, "User created", typeof(UserResponse))]
    [SwaggerResponse(409, "User email already exists", typeof(ErrorResponse))]
    public async Task<ActionResult<UserResponse>> Create([FromBody] CreateUserRequest request)
    {
        var (result, errorCode) = await _service.CreateAsync(request);
        if (errorCode == "DUPLICATE_USER_EMAIL")
            return Conflict(new ErrorResponse { Error = "User email already exists", Code = errorCode });

        return CreatedAtAction(nameof(GetById), new { id = result!.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(OperationId = "UpdateUser")]
    [SwaggerResponse(200, "User updated", typeof(UserResponse))]
    [SwaggerResponse(404, "User not found", typeof(ErrorResponse))]
    [SwaggerResponse(409, "User email already exists", typeof(ErrorResponse))]
    public async Task<ActionResult<UserResponse>> Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        var (result, errorCode) = await _service.UpdateAsync(id, request);

        return errorCode switch
        {
            "USER_NOT_FOUND" => NotFound(new ErrorResponse { Error = "User not found", Code = errorCode }),
            "DUPLICATE_USER_EMAIL" => Conflict(new ErrorResponse { Error = "User email already exists", Code = errorCode }),
            _ => Ok(result)
        };
    }

    [HttpDelete("{id:guid}")]
    [SwaggerOperation(OperationId = "DeleteUser")]
    [SwaggerResponse(204, "User deleted")]
    [SwaggerResponse(404, "User not found", typeof(ErrorResponse))]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted)
            return NotFound(new ErrorResponse { Error = "User not found", Code = "USER_NOT_FOUND" });
        return NoContent();
    }
}
