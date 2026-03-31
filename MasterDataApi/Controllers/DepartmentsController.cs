// =============================================================
// Controllers/DepartmentsController.cs
// =============================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MasterDataApi.DTOs;
using MasterDataApi.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace MasterDataApi.Controllers;

/// <summary>
/// Manage departments — functional units within companies.
/// Departments can be scoped to a specific company or defined as global
/// (available to all companies in the system).
/// </summary>
[ApiController]
[Route("api/departments")]
[Authorize]
[Produces("application/json")]
[Tags("Departments")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _service;
    public DepartmentsController(IDepartmentService service) { _service = service; }

    /// <summary>Get all departments</summary>
    /// <remarks>
    /// Supports optional filtering:
    /// - Use `companyId` to get only departments belonging to a specific company
    /// - Use `global=true` to get only global (company-independent) departments
    ///
    /// **Example:** `GET /api/departments?companyId=3fa85f64-5717-4562-b3fc-2c963f66afa6`
    /// </remarks>
    /// <param name="companyId">Filter by company GUID (optional)</param>
    /// <param name="global">If true, return only global departments (optional)</param>
    [HttpGet]
    [SwaggerOperation(OperationId = "GetAllDepartments")]
    [SwaggerResponse(200, "List of departments", typeof(IEnumerable<DepartmentResponse>))]
    public async Task<ActionResult<IEnumerable<DepartmentResponse>>> GetAll(
        [FromQuery] Guid? companyId,
        [FromQuery] bool? global)
    {
        var depts = await _service.GetAllAsync(companyId, global);
        return Ok(depts);
    }

    /// <summary>Get a single department by ID</summary>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(OperationId = "GetDepartmentById")]
    [SwaggerResponse(200, "Department found", typeof(DepartmentResponse))]
    [SwaggerResponse(404, "Department not found", typeof(ErrorResponse))]
    public async Task<ActionResult<DepartmentResponse>> GetById(Guid id)
    {
        var dept = await _service.GetByIdAsync(id);
        if (dept == null)
            return NotFound(new ErrorResponse { Error = "Department not found", Code = "DEPARTMENT_NOT_FOUND" });
        return Ok(dept);
    }

    /// <summary>Create a new department</summary>
    /// <remarks>
    /// To create a **company-scoped** department, provide `companyId`.
    /// To create a **global** department (available to all companies), omit `companyId`.
    ///
    /// **Sample — company-scoped:**
    /// ```json
    /// { "name": "Human Resources", "companyId": "3fa85f64-5717-4562-b3fc-2c963f66afa6" }
    /// ```
    ///
    /// **Sample — global:**
    /// ```json
    /// { "name": "IT Support" }
    /// ```
    /// </remarks>
    [HttpPost]
    [SwaggerOperation(OperationId = "CreateDepartment")]
    [SwaggerResponse(201, "Department created", typeof(DepartmentResponse))]
    [SwaggerResponse(400, "Invalid request", typeof(ErrorResponse))]
    public async Task<ActionResult<DepartmentResponse>> Create([FromBody] CreateDepartmentRequest request)
    {
        var dept = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = dept.Id }, dept);
    }

    /// <summary>Update a department</summary>
    [HttpPut("{id:guid}")]
    [SwaggerOperation(OperationId = "UpdateDepartment")]
    [SwaggerResponse(200, "Department updated", typeof(DepartmentResponse))]
    [SwaggerResponse(404, "Department not found", typeof(ErrorResponse))]
    public async Task<ActionResult<DepartmentResponse>> Update(Guid id, [FromBody] UpdateDepartmentRequest request)
    {
        var dept = await _service.UpdateAsync(id, request);
        if (dept == null)
            return NotFound(new ErrorResponse { Error = "Department not found", Code = "DEPARTMENT_NOT_FOUND" });
        return Ok(dept);
    }

    /// <summary>Delete a department</summary>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(OperationId = "DeleteDepartment")]
    [SwaggerResponse(204, "Department deleted")]
    [SwaggerResponse(404, "Department not found", typeof(ErrorResponse))]
    public async Task<IActionResult> Delete(Guid id)
    {
        bool deleted = await _service.DeleteAsync(id);
        if (!deleted)
            return NotFound(new ErrorResponse { Error = "Department not found", Code = "DEPARTMENT_NOT_FOUND" });
        return NoContent();
    }
}
