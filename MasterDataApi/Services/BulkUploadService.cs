// =============================================================
// Services/BulkUploadService.cs
//
// Handles the Excel (.xlsx) bulk upload feature.
// Uses the EPPlus library to read and write Excel files.
//
// Flow:
//  1. Caller uploads .xlsx file via multipart/form-data
//  2. We read each row, validate it, and attempt to create assignments
//  3. Bad rows are logged and skipped — good rows are saved
//  4. We return a summary: how many succeeded vs failed
// =============================================================

using Microsoft.EntityFrameworkCore;
using MasterDataApi.Data;
using MasterDataApi.DTOs;
using OfficeOpenXml;          // The EPPlus namespace for Excel
using OfficeOpenXml.Style;    // For styling cells (colors, alignment, etc.)
using System.Drawing;
using System.Text.RegularExpressions;

namespace MasterDataApi.Services;

public class BulkUploadService : IBulkUploadService
{
    private readonly AppDbContext _db;
    private readonly IUserAssignmentService _assignmentService;

    public BulkUploadService(AppDbContext db, IUserAssignmentService assignmentService)
    {
        _db = db;
        _assignmentService = assignmentService;

        // EPPlus 5+ requires you to declare your license context.
        // For non-commercial use, set NonCommercial. For production,
        // set Commercial and provide a license key.
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    /// <summary>
    /// Reads the uploaded Excel file row-by-row, validates each row,
    /// and creates or updates user assignments for valid rows.
    /// </summary>
    public async Task<BulkUploadResponse> ProcessExcelAsync(Stream fileStream)
    {
        var response = new BulkUploadResponse();

        using var package = new ExcelPackage(fileStream);

        // Grab the first sheet in the workbook
        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
        if (worksheet == null)
        {
            response.Errors.Add(new BulkUploadError
            {
                Row     = 0,
                Message = "Excel file contains no worksheets",
                Code    = "MISSING_REQUIRED_FIELD",
            });
            response.FailedCount = 1;
            return response;
        }

        // worksheet.Dimension.End.Row gives us the last row with data
        int totalRows = worksheet.Dimension?.End.Row ?? 1;

        // Row 1 = header row, so data starts at row 2
        for (int row = 2; row <= totalRows; row++)
        {
            // Read each cell. GetValue<string>() returns null if empty.
            string? userIdStr     = worksheet.Cells[row, 1].GetValue<string>()?.Trim();
            string? companyCode   = worksheet.Cells[row, 2].GetValue<string>()?.Trim();
            string? deptName      = worksheet.Cells[row, 3].GetValue<string>()?.Trim();
            string? roleName      = worksheet.Cells[row, 4].GetValue<string>()?.Trim();

            // ── Validate required fields ────────────────────
            if (string.IsNullOrWhiteSpace(userIdStr) ||
                string.IsNullOrWhiteSpace(companyCode) ||
                string.IsNullOrWhiteSpace(deptName) ||
                string.IsNullOrWhiteSpace(roleName))
            {
                response.Errors.Add(new BulkUploadError
                {
                    Row     = row,
                    UserId  = userIdStr,
                    Message = "Missing required field in one or more columns",
                    Code    = "MISSING_REQUIRED_FIELD",
                });
                response.FailedCount++;
                continue; // Skip to the next row
            }

            // ── Parse UserId as GUID ────────────────────────
            var parsedUserIdStr = TryExtractGuid(userIdStr);
            if (parsedUserIdStr == null || !Guid.TryParse(parsedUserIdStr, out Guid userId))
            {
                response.Errors.Add(new BulkUploadError
                {
                    Row     = row,
                    UserId  = userIdStr,
                    Message = $"Invalid user value: '{userIdStr}'. Provide a GUID or a value containing a GUID (e.g. 'Jane Doe (GUID)')",
                    Code    = "INVALID_USER_ID",
                });
                response.FailedCount++;
                continue;
            }

            var userExists = await _db.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                response.Errors.Add(new BulkUploadError
                {
                    Row = row,
                    UserId = userIdStr,
                    Message = $"User not found: '{userId}' does not exist",
                    Code = "USER_NOT_FOUND",
                });
                response.FailedCount++;
                continue;
            }

            // ── Validate Company Code ───────────────────────
            var company = await _db.Companies
                .FirstOrDefaultAsync(c => c.Code == companyCode);
            if (company == null)
            {
                response.Errors.Add(new BulkUploadError
                {
                    Row     = row,
                    UserId  = userIdStr,
                    Message = $"Invalid Company Code: '{companyCode}' does not exist",
                    Code    = "INVALID_COMPANY_CODE",
                });
                response.FailedCount++;
                continue;
            }

            // ── Validate Department Name ────────────────────
            var department = await _db.Departments
                .FirstOrDefaultAsync(d => d.Name.ToLower() == deptName.ToLower());
            if (department == null)
            {
                response.Errors.Add(new BulkUploadError
                {
                    Row     = row,
                    UserId  = userIdStr,
                    Message = $"Invalid Department Name: '{deptName}' does not exist",
                    Code    = "INVALID_DEPARTMENT_NAME",
                });
                response.FailedCount++;
                continue;
            }

            // ── Validate Role Name ──────────────────────────
            var role = await _db.Roles
                .FirstOrDefaultAsync(r => r.Name.ToLower() == roleName.ToLower());
            if (role == null)
            {
                response.Errors.Add(new BulkUploadError
                {
                    Row     = row,
                    UserId  = userIdStr,
                    Message = $"Invalid Role Name: '{roleName}' does not exist",
                    Code    = "INVALID_ROLE_NAME",
                });
                response.FailedCount++;
                continue;
            }

            // ── All valid — create the assignment ───────────
            var request = new DTOs.AssignUserRequest
            {
                UserId       = userId,
                CompanyCode  = companyCode,
                DepartmentId = department.Id,
                RoleId       = role.Id,
            };

            var (result, errorCode) = await _assignmentService.AssignAsync(request);
            if (errorCode != null)
            {
                response.Errors.Add(new BulkUploadError
                {
                    Row     = row,
                    UserId  = userIdStr,
                    Message = $"Assignment failed: {errorCode}",
                    Code    = errorCode,
                });
                response.FailedCount++;
            }
            else
            {
                response.SuccessCount++;
            }
        }

        return response;
    }

    private static string? TryExtractGuid(string input)
    {
        if (Guid.TryParse(input, out var direct))
            return direct.ToString();

        // Allow values like "Jane Doe (e3d7c9a1-1b2e-4c5d-9f0a-7b8c6d5e4f3a)"
        var match = Regex.Match(
            input,
            @"\b[0-9a-fA-F]{8}\b-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\b",
            RegexOptions.CultureInvariant
        );

        return match.Success ? match.Value : null;
    }

    /// <summary>
    /// Generates a pre-formatted .xlsx template file.
    /// Includes headers, sample rows, column notes, and dropdown validation
    /// populated with current valid values from the database.
    /// </summary>
    public byte[] GenerateTemplate()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("User Assignments");
        var lists = package.Workbook.Worksheets.Add("Lists");
        lists.Hidden = eWorkSheetHidden.VeryHidden;

        // ── Header row styling ──────────────────────────────
        var headerColor = ColorTranslator.FromHtml("#2E75B6");

        string[] headers = { "UserId", "CompanyCode", "DepartmentName", "RoleName" };
        for (int col = 1; col <= headers.Length; col++)
        {
            ws.Cells[1, col].Value = headers[col - 1];
            ws.Cells[1, col].Style.Font.Bold = true;
            ws.Cells[1, col].Style.Font.Color.SetColor(Color.White);
            ws.Cells[1, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[1, col].Style.Fill.BackgroundColor.SetColor(headerColor);
            ws.Cells[1, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }

        // ── Column comments (inline documentation) ─────────
        ws.Cells[1, 1].AddComment("GUID of the user. Example: e3d7c9a1-1b2e-4c5d-9f0a-7b8c6d5e4f3a", "API");
        ws.Cells[1, 2].AddComment("Company code — must match an existing company (e.g. ABC001)", "API");
        ws.Cells[1, 3].AddComment("Department name — must match an existing department exactly", "API");
        ws.Cells[1, 4].AddComment("Role name — must match an existing role exactly", "API");

        // ── Build dropdown lists (hidden sheet) ─────────────
        // We can populate all lists from the API database.
        var companyCodes = _db.Companies.AsNoTracking()
            .OrderBy(c => c.Code)
            .Select(c => c.Code)
            .ToList();

        var departmentNames = _db.Departments.AsNoTracking()
            .OrderBy(d => d.Name)
            .Select(d => d.Name)
            .ToList();

        var roleNames = _db.Roles.AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => r.Name)
            .ToList();

        var usersForDropdown = _db.Users.AsNoTracking()
            .OrderBy(u => u.FullName)
            .Select(u => $"{u.FullName} ({u.Id})")
            .ToList();

        // Headers on the hidden sheet
        lists.Cells[1, 1].Value = "Users";
        lists.Cells[1, 2].Value = "Companies";
        lists.Cells[1, 3].Value = "Departments";
        lists.Cells[1, 4].Value = "Roles";

        for (int i = 0; i < usersForDropdown.Count; i++)
            lists.Cells[i + 2, 1].Value = usersForDropdown[i];
        for (int i = 0; i < companyCodes.Count; i++)
            lists.Cells[i + 2, 2].Value = companyCodes[i];
        for (int i = 0; i < departmentNames.Count; i++)
            lists.Cells[i + 2, 3].Value = departmentNames[i];
        for (int i = 0; i < roleNames.Count; i++)
            lists.Cells[i + 2, 4].Value = roleNames[i];

        // Named ranges make validation formulas stable and readable.
        // Use at least one cell even if the list is empty (Excel dislikes empty validation ranges).
        ExcelRangeBase UsersRange() =>
            lists.Cells[2, 1, Math.Max(2, usersForDropdown.Count + 1), 1];
        ExcelRangeBase CompaniesRange() =>
            lists.Cells[2, 2, Math.Max(2, companyCodes.Count + 1), 2];
        ExcelRangeBase DepartmentsRange() =>
            lists.Cells[2, 3, Math.Max(2, departmentNames.Count + 1), 3];
        ExcelRangeBase RolesRange() =>
            lists.Cells[2, 4, Math.Max(2, roleNames.Count + 1), 4];

        package.Workbook.Names.Add("UsersList", UsersRange());
        package.Workbook.Names.Add("CompaniesList", CompaniesRange());
        package.Workbook.Names.Add("DepartmentsList", DepartmentsRange());
        package.Workbook.Names.Add("RolesList", RolesRange());

        // ── Apply dropdowns to a reasonable row range ───────
        // Users tend to paste/drag-fill; give them plenty of rows.
        const int firstDataRow = 2;
        const int lastDataRow = 5000;

        void AddListValidation(int col, string rangeName, string promptTitle, string promptBody)
        {
            var addr = ws.Cells[firstDataRow, col, lastDataRow, col].Address;
            var validation = ws.DataValidations.AddListValidation(addr);
            validation.Formula.ExcelFormula = rangeName;
            validation.ShowErrorMessage = true;
            validation.ErrorTitle = "Invalid value";
            validation.Error = "Please select a value from the dropdown list.";
            validation.ShowInputMessage = true;
            validation.PromptTitle = promptTitle;
            validation.Prompt = promptBody;
        }

        AddListValidation(1, "UsersList", "UserId", "Select a user (Name + UserId), or paste a GUID.");
        AddListValidation(2, "CompaniesList", "CompanyCode", "Select a valid company code.");
        AddListValidation(3, "DepartmentsList", "DepartmentName", "Select a valid department name.");
        AddListValidation(4, "RolesList", "RoleName", "Select a valid role name.");

        // ── Sample rows ─────────────────────────────────────
        ws.Cells[2, 1].Value = "e3d7c9a1-1b2e-4c5d-9f0a-7b8c6d5e4f3a";
        ws.Cells[2, 2].Value = "ABC001";
        ws.Cells[2, 3].Value = "Human Resources";
        ws.Cells[2, 4].Value = "Manager";

        ws.Cells[3, 1].Value = "f4e8d0b2-0000-0000-0000-000000000002";
        ws.Cells[3, 2].Value = "XYZ002";
        ws.Cells[3, 3].Value = "IT Support";
        ws.Cells[3, 4].Value = "Analyst";

        // ── Auto-fit column widths ───────────────────────────
        ws.Cells.AutoFitColumns(10, 50);

        return package.GetAsByteArray();
    }
}
