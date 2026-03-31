// =============================================================
// Controllers/BulkUploadController.cs
//
// Handles the two Excel-related endpoints:
//   POST /api/users/upload    — accepts an .xlsx file, processes it
//   GET  /api/users/template  — returns a blank .xlsx template to fill in
//
// File upload uses multipart/form-data instead of JSON.
// That's why this controller has a different [Consumes] attribute.
// =============================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MasterDataApi.DTOs;
using MasterDataApi.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace MasterDataApi.Controllers;

/// <summary>
/// Excel-based bulk operations for user assignments.
/// Ideal for onboarding many users at once without manual API calls.
/// </summary>
[ApiController]
[Authorize]
[Tags("Bulk Upload")]
public class BulkUploadController : ControllerBase
{
    private readonly IBulkUploadService _bulkService;

    public BulkUploadController(IBulkUploadService bulkService)
    {
        _bulkService = bulkService;
    }

    // ──────────────────────────────────────────────────────────
    // POST /api/users/upload
    // ──────────────────────────────────────────────────────────

    /// <summary>Bulk upload user assignments via Excel file</summary>
    /// <remarks>
    /// Upload a `.xlsx` file with the following columns:
    ///
    /// | Column | Required | Description |
    /// |--------|----------|-------------|
    /// | UserId | ✅ Yes | GUID of the user (or a value containing a GUID, e.g. `Jane Doe (GUID)`) |
    /// | CompanyCode | ✅ Yes | Must match an existing company |
    /// | DepartmentName | ✅ Yes | Must match an existing department |
    /// | RoleName | ✅ Yes | Must match an existing role |
    ///
    /// Rows that fail validation are **skipped** (not rejected entirely).
    /// The response reports exactly which rows succeeded and which failed.
    ///
    /// 💡 **Tip:** Download the template first using `GET /api/users/template`.
    /// </remarks>
    [HttpPost("api/users/upload")]
    [Consumes("multipart/form-data")]
    [Produces("application/json")]
    [SwaggerOperation(OperationId = "BulkUploadAssignments")]
    [SwaggerResponse(200, "Upload processed — check errors array for any row failures", typeof(BulkUploadResponse))]
    [SwaggerResponse(400, "No file provided or file is not a valid .xlsx", typeof(ErrorResponse))]
    public async Task<ActionResult<BulkUploadResponse>> Upload(IFormFile file)
    {
        // Validate the file was actually provided
        if (file == null || file.Length == 0)
            return BadRequest(new ErrorResponse { Error = "No file uploaded", Code = "MISSING_REQUIRED_FIELD" });

        // Only accept Excel files
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".xlsx")
            return BadRequest(new ErrorResponse
            {
                Error = "Only .xlsx files are accepted",
                Code  = "INVALID_FILE_TYPE"
            });

        // OpenReadStream() gives us a readable Stream of the uploaded file
        using var stream = file.OpenReadStream();
        var result = await _bulkService.ProcessExcelAsync(stream);

        return Ok(result);
    }

    // ──────────────────────────────────────────────────────────
    // GET /api/users/template
    // ──────────────────────────────────────────────────────────

    /// <summary>Download the Excel upload template</summary>
    /// <remarks>
    /// Returns a pre-formatted `.xlsx` file containing:
    /// - Column headers with descriptions
    /// - Two sample rows showing the expected format
    /// - Inline column comments explaining each field's constraints
    ///
    /// ⚠️ **Always download a fresh template before uploading.**
    /// The dropdowns are populated with current valid values.
    /// </remarks>
    [HttpGet("api/users/template")]
    [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    [SwaggerOperation(OperationId = "DownloadUploadTemplate")]
    [SwaggerResponse(200, "Excel template file returned")]
    public IActionResult DownloadTemplate()
    {
        var bytes = _bulkService.GenerateTemplate();

        // Return the bytes as a downloadable Excel file
        // The content-type tells the browser "this is an Excel file"
        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "user_assignment_template.xlsx"
        );
    }
}
