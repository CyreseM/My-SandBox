// =============================================================
// Controllers/CompaniesController.cs
//
// A Controller is the entry point for HTTP requests.
// It reads the request, calls the service, and returns a response.
//
// [ApiController]      → enables automatic model validation & JSON binding
// [Route("api/...")] → sets the URL prefix for all actions in this class
// =============================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MasterDataApi.DTOs;
using MasterDataApi.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace MasterDataApi.Controllers;

/// <summary>
/// Manage companies — the top-level organizational units.
/// Every department and user assignment ultimately traces back to a company.
/// </summary>
[ApiController]
[Route("api/companies")]
[Authorize]   // All endpoints require a valid JWT Bearer token
[Produces("application/json")]
[Tags("Companies")]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _service;

    // ASP.NET Core's DI container automatically provides ICompanyService here
    public CompaniesController(ICompanyService service) { _service = service; }

    // ──────────────────────────────────────────────────────────
    // GET /api/companies
    // ──────────────────────────────────────────────────────────

    /// <summary>Get all companies</summary>
    /// <remarks>Returns a list of all companies registered in the system, ordered by name.</remarks>
    [HttpGet]
    [SwaggerOperation(OperationId = "GetAllCompanies")]
    [SwaggerResponse(200, "List of companies returned successfully", typeof(IEnumerable<CompanyResponse>))]
    [SwaggerResponse(401, "Authentication required — provide a Bearer token", typeof(ErrorResponse))]
    public async Task<ActionResult<IEnumerable<CompanyResponse>>> GetAll()
    {
        var companies = await _service.GetAllAsync();
        return Ok(companies); // 200 OK
    }

    // ──────────────────────────────────────────────────────────
    // GET /api/companies/{id}
    // ──────────────────────────────────────────────────────────

    /// <summary>Get a single company by ID</summary>
    /// <param name="id">The GUID of the company</param>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(OperationId = "GetCompanyById")]
    [SwaggerResponse(200, "Company found", typeof(CompanyResponse))]
    [SwaggerResponse(404, "Company not found", typeof(ErrorResponse))]
    public async Task<ActionResult<CompanyResponse>> GetById(Guid id)
    {
        var company = await _service.GetByIdAsync(id);

        // If the service returned null, the company doesn't exist → 404
        if (company == null)
            return NotFound(new ErrorResponse { Error = "Company not found", Code = "COMPANY_NOT_FOUND" });

        return Ok(company);
    }

    // ──────────────────────────────────────────────────────────
    // POST /api/companies
    // ──────────────────────────────────────────────────────────

    /// <summary>Create a new company</summary>
    /// <remarks>
    /// The `code` field must be unique across all companies.
    /// Once set, the `id` is system-generated and immutable.
    ///
    /// **Sample request:**
    /// ```json
    /// { "name": "ABC Ltd", "code": "ABC001" }
    /// ```
    /// </remarks>
    [HttpPost]
    [SwaggerOperation(OperationId = "CreateCompany")]
    [SwaggerResponse(201, "Company created successfully", typeof(CompanyResponse))]
    [SwaggerResponse(400, "Invalid request body", typeof(ErrorResponse))]
    [SwaggerResponse(409, "Company code already exists", typeof(ErrorResponse))]
    public async Task<ActionResult<CompanyResponse>> Create([FromBody] CreateCompanyRequest request)
    {
        // [ApiController] validates the request against DataAnnotations BEFORE this runs.
        // If Name or Code is missing, it returns 400 automatically.

        var (result, errorCode) = await _service.CreateAsync(request);

        if (errorCode == "DUPLICATE_COMPANY_CODE")
            return Conflict(new ErrorResponse { Error = "Company code already exists", Code = errorCode });

        // 201 Created — Location header points to the new resource
        return CreatedAtAction(nameof(GetById), new { id = result!.Id }, result);
    }

    // ──────────────────────────────────────────────────────────
    // PUT /api/companies/{id}
    // ──────────────────────────────────────────────────────────

    /// <summary>Update an existing company</summary>
    /// <param name="id">The GUID of the company to update</param>
    [HttpPut("{id:guid}")]
    [SwaggerOperation(OperationId = "UpdateCompany")]
    [SwaggerResponse(200, "Company updated", typeof(CompanyResponse))]
    [SwaggerResponse(404, "Company not found", typeof(ErrorResponse))]
    [SwaggerResponse(409, "New code conflicts with another company", typeof(ErrorResponse))]
    public async Task<ActionResult<CompanyResponse>> Update(Guid id, [FromBody] UpdateCompanyRequest request)
    {
        var (result, errorCode) = await _service.UpdateAsync(id, request);

        return errorCode switch
        {
            "COMPANY_NOT_FOUND"      => NotFound(new ErrorResponse { Error = "Company not found", Code = errorCode }),
            "DUPLICATE_COMPANY_CODE" => Conflict(new ErrorResponse  { Error = "Company code already in use", Code = errorCode }),
            _                        => Ok(result)
        };
    }

    // ──────────────────────────────────────────────────────────
    // DELETE /api/companies/{id}
    // ──────────────────────────────────────────────────────────

    /// <summary>Delete a company</summary>
    /// <remarks>
    /// ⚠️ **Warning:** Deleting a company may affect associated departments and user assignments.
    /// </remarks>
    /// <param name="id">The GUID of the company to delete</param>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(OperationId = "DeleteCompany")]
    [SwaggerResponse(204, "Company deleted successfully — no body returned")]
    [SwaggerResponse(404, "Company not found", typeof(ErrorResponse))]
    public async Task<IActionResult> Delete(Guid id)
    {
        bool deleted = await _service.DeleteAsync(id);

        if (!deleted)
            return NotFound(new ErrorResponse { Error = "Company not found", Code = "COMPANY_NOT_FOUND" });

        return NoContent(); // 204 — success but nothing to return
    }
}
