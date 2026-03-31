// =============================================================
// Controllers/RolesController.cs
// =============================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MasterDataApi.DTOs;
using MasterDataApi.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace MasterDataApi.Controllers;

/// <summary>
/// Manage roles — system-wide definitions of a user's function.
/// Roles are global (not company-specific). "Manager" means the same
/// thing regardless of which company a user belongs to.
/// </summary>
[ApiController]
[Route("api/roles")]
[Authorize]
[Produces("application/json")]
[Tags("Roles")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _service;
    public RolesController(IRoleService service) { _service = service; }

    /// <summary>Get all roles</summary>
    [HttpGet]
    [SwaggerOperation(OperationId = "GetAllRoles")]
    [SwaggerResponse(200, "List of roles", typeof(IEnumerable<RoleResponse>))]
    public async Task<ActionResult<IEnumerable<RoleResponse>>> GetAll()
        => Ok(await _service.GetAllAsync());

    /// <summary>Get a single role by ID</summary>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(OperationId = "GetRoleById")]
    [SwaggerResponse(200, "Role found", typeof(RoleResponse))]
    [SwaggerResponse(404, "Role not found", typeof(ErrorResponse))]
    public async Task<ActionResult<RoleResponse>> GetById(Guid id)
    {
        var role = await _service.GetByIdAsync(id);
        if (role == null)
            return NotFound(new ErrorResponse { Error = "Role not found", Code = "ROLE_NOT_FOUND" });
        return Ok(role);
    }

    /// <summary>Create a new role</summary>
    /// <remarks>
    /// Role names must be unique across the entire system.
    ///
    /// **Sample:**
    /// ```json
    /// { "name": "Manager" }
    /// ```
    /// </remarks>
    [HttpPost]
    [SwaggerOperation(OperationId = "CreateRole")]
    [SwaggerResponse(201, "Role created", typeof(RoleResponse))]
    [SwaggerResponse(409, "Role name already exists", typeof(ErrorResponse))]
    public async Task<ActionResult<RoleResponse>> Create([FromBody] CreateRoleRequest request)
    {
        var (result, errorCode) = await _service.CreateAsync(request);
        if (errorCode != null)
            return Conflict(new ErrorResponse { Error = "Role name already exists", Code = errorCode });

        return CreatedAtAction(nameof(GetById), new { id = result!.Id }, result);
    }

    /// <summary>Update a role name</summary>
    [HttpPut("{id:guid}")]
    [SwaggerOperation(OperationId = "UpdateRole")]
    [SwaggerResponse(200, "Role updated", typeof(RoleResponse))]
    [SwaggerResponse(404, "Role not found", typeof(ErrorResponse))]
    public async Task<ActionResult<RoleResponse>> Update(Guid id, [FromBody] UpdateRoleRequest request)
    {
        var role = await _service.UpdateAsync(id, request);
        if (role == null)
            return NotFound(new ErrorResponse { Error = "Role not found", Code = "ROLE_NOT_FOUND" });
        return Ok(role);
    }

    /// <summary>Delete a role</summary>
    /// <remarks>
    /// ⚠️ **Warning:** Deleting a role assigned to users will leave those users without a role.
    /// Validate or reassign before deleting.
    /// </remarks>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(OperationId = "DeleteRole")]
    [SwaggerResponse(204, "Role deleted")]
    [SwaggerResponse(404, "Role not found", typeof(ErrorResponse))]
    public async Task<IActionResult> Delete(Guid id)
    {
        bool deleted = await _service.DeleteAsync(id);
        if (!deleted)
            return NotFound(new ErrorResponse { Error = "Role not found", Code = "ROLE_NOT_FOUND" });
        return NoContent();
    }
}

// =============================================================
// Controllers/UserAssignmentsController.cs
// =============================================================


/// <summary>
/// Link users to a company, department, and role.
/// A user can only have ONE active assignment at a time.
/// The organizational hierarchy is: User → Company → Department → Role
/// </summary>
[ApiController]
[Authorize]
[Produces("application/json")]
[Tags("User Assignments")]
public class UserAssignmentsController : ControllerBase
{
    private readonly IUserAssignmentService _service;
    public UserAssignmentsController(IUserAssignmentService service) { _service = service; }

    /// <summary>Assign a user to a company, department, and role</summary>
    /// <remarks>
    /// If the user already has an assignment, it will be replaced.
    /// All referenced IDs (companyCode, departmentId, roleId) must exist.
    ///
    /// **Sample:**
    /// ```json
    /// {
    ///   "userId": "e3d7c9a1-1b2e-4c5d-9f0a-7b8c6d5e4f3a",
    ///   "companyCode": "ABC001",
    ///   "departmentId": "d290f1ee-6c54-4b01-90e6-d701748f0851",
    ///   "roleId": "a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11"
    /// }
    /// ```
    /// </remarks>
    [HttpPost("api/users/assign")]
    [SwaggerOperation(OperationId = "AssignUser")]
    [SwaggerResponse(201, "User assigned successfully", typeof(AssignmentResponse))]
    [SwaggerResponse(400, "Invalid request body", typeof(ErrorResponse))]
    [SwaggerResponse(404, "Referenced company, department, or role not found", typeof(ErrorResponse))]
    public async Task<ActionResult<AssignmentResponse>> Assign([FromBody] AssignUserRequest request)
    {
        var (result, errorCode) = await _service.AssignAsync(request);

        if (errorCode != null)
        {
            var msg = errorCode switch
            {
                "USER_NOT_FOUND"       => "User not found",
                "INVALID_COMPANY_CODE" => "Company code not found",
                "DEPARTMENT_NOT_FOUND" => "Department not found",
                "ROLE_NOT_FOUND"       => "Role not found",
                _                      => "Assignment failed"
            };
            return NotFound(new ErrorResponse { Error = msg, Code = errorCode });
        }

        return StatusCode(201, result);
    }

    /// <summary>Get a user's current assignment</summary>
    /// <param name="id">The user's GUID</param>
    [HttpGet("api/users/{id:guid}/assignment")]
    [SwaggerOperation(OperationId = "GetUserAssignment")]
    [SwaggerResponse(200, "Assignment found", typeof(AssignmentResponse))]
    [SwaggerResponse(404, "No assignment found for this user", typeof(ErrorResponse))]
    public async Task<ActionResult<AssignmentResponse>> GetByUserId(Guid id)
    {
        var assignment = await _service.GetByUserIdAsync(id);
        if (assignment == null)
            return NotFound(new ErrorResponse { Error = "No assignment found for this user", Code = "ASSIGNMENT_NOT_FOUND" });
        return Ok(assignment);
    }

    /// <summary>Update a user's assignment</summary>
    /// <param name="id">The user's GUID</param>
    [HttpPut("api/users/{id:guid}/assignment")]
    [SwaggerOperation(OperationId = "UpdateUserAssignment")]
    [SwaggerResponse(200, "Assignment updated", typeof(AssignmentResponse))]
    [SwaggerResponse(404, "User or referenced entity not found", typeof(ErrorResponse))]
    public async Task<ActionResult<AssignmentResponse>> Update(Guid id, [FromBody] AssignUserRequest request)
    {
        var (result, errorCode) = await _service.UpdateAsync(id, request);

        if (errorCode != null)
            return NotFound(new ErrorResponse { Error = $"Error: {errorCode}", Code = errorCode });

        return Ok(result);
    }

    /// <summary>Remove a user's assignment</summary>
    /// <param name="id">The user's GUID</param>
    [HttpDelete("api/users/{id:guid}/assignment")]
    [SwaggerOperation(OperationId = "DeleteUserAssignment")]
    [SwaggerResponse(204, "Assignment removed")]
    [SwaggerResponse(404, "No assignment found for this user", typeof(ErrorResponse))]
    public async Task<IActionResult> Delete(Guid id)
    {
        bool deleted = await _service.DeleteAsync(id);
        if (!deleted)
            return NotFound(new ErrorResponse { Error = "No assignment found for this user", Code = "ASSIGNMENT_NOT_FOUND" });
        return NoContent();
    }
}
