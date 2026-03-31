// =============================================================
// DTOs/Dtos.cs
// DTOs = Data Transfer Objects.
// These are the "shapes" of data going IN and OUT of your API.
//
// Why not just use the Model classes directly?
// - Models have database fields callers shouldn't set (like Id, CreatedAt)
// - DTOs let you control exactly what comes in and what goes out
// - Keeps your database structure decoupled from your API contract
// =============================================================

using System.ComponentModel.DataAnnotations;

namespace MasterDataApi.DTOs;

// ─────────────────────────────────────────
// COMPANY DTOs
// ─────────────────────────────────────────

/// <summary>What the API returns when you ask about a company</summary>
public class CompanyResponse
{
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; set; }
    /// <example>ABC Ltd</example>
    public string Name { get; set; } = string.Empty;
    /// <example>ABC001</example>
    public string Code { get; set; } = string.Empty;
    /// <example>2024-01-15T09:00:00Z</example>
    public DateTime CreatedAt { get; set; }
    /// <example>2024-03-20T14:30:00Z</example>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>What the caller sends when CREATING a new company</summary>
public class CreateCompanyRequest
{
    /// <summary>Full company name</summary>
    /// <example>ABC Ltd</example>
    [Required(ErrorMessage = "Company name is required")]
    [StringLength(255, ErrorMessage = "Name cannot exceed 255 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Short unique code — alphanumeric, no spaces, max 50 chars</summary>
    /// <example>ABC001</example>
    [Required(ErrorMessage = "Company code is required")]
    [StringLength(50, ErrorMessage = "Code cannot exceed 50 characters")]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Code must be alphanumeric with no spaces")]
    public string Code { get; set; } = string.Empty;
}

/// <summary>What the caller sends when UPDATING an existing company</summary>
public class UpdateCompanyRequest
{
    /// <example>ABC Holdings Ltd</example>
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <example>ABC001</example>
    [Required]
    [StringLength(50)]
    [RegularExpression(@"^[a-zA-Z0-9]+$")]
    public string Code { get; set; } = string.Empty;
}

// ─────────────────────────────────────────
// DEPARTMENT DTOs
// ─────────────────────────────────────────

/// <summary>What the API returns for a department</summary>
public class DepartmentResponse
{
    /// <example>d290f1ee-6c54-4b01-90e6-d701748f0851</example>
    public Guid Id { get; set; }
    /// <example>Human Resources</example>
    public string Name { get; set; } = string.Empty;
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid? CompanyId { get; set; }
    /// <example>false</example>
    public bool IsGlobal { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>What the caller sends when CREATING a department</summary>
public class CreateDepartmentRequest
{
    /// <summary>Department name</summary>
    /// <example>Human Resources</example>
    [Required(ErrorMessage = "Department name is required")]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional: scope this department to one company.
    /// Leave null to make it a global department available to all companies.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid? CompanyId { get; set; }
}

/// <summary>What the caller sends when UPDATING a department</summary>
public class UpdateDepartmentRequest
{
    /// <example>HR &amp; Talent</example>
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid? CompanyId { get; set; }
}

// ─────────────────────────────────────────
// ROLE DTOs
// ─────────────────────────────────────────

/// <summary>What the API returns for a role</summary>
public class RoleResponse
{
    /// <example>a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11</example>
    public Guid Id { get; set; }
    /// <example>Manager</example>
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>What the caller sends when CREATING a role</summary>
public class CreateRoleRequest
{
    /// <summary>Role name — must be unique across the system</summary>
    /// <example>Manager</example>
    [Required(ErrorMessage = "Role name is required")]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;
}

/// <summary>What the caller sends when UPDATING a role</summary>
public class UpdateRoleRequest
{
    /// <example>Senior Manager</example>
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;
}

// ─────────────────────────────────────────
// USER DTOs
// ─────────────────────────────────────────

/// <summary>What the API returns for a user</summary>
public class UserResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>What the caller sends when creating a user</summary>
public class CreateUserRequest
{
    [Required(ErrorMessage = "FullName is required")]
    [StringLength(255)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email must be a valid email address")]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;
}

/// <summary>What the caller sends when updating a user</summary>
public class UpdateUserRequest
{
    [Required]
    [StringLength(255)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;
}

// ─────────────────────────────────────────
// USER ASSIGNMENT DTOs
// ─────────────────────────────────────────

/// <summary>What the API returns for a user assignment — expands all references</summary>
public class AssignmentResponse
{
    /// <example>e3d7c9a1-1b2e-4c5d-9f0a-7b8c6d5e4f3a</example>
    public Guid UserId { get; set; }
    public CompanyRef Company { get; set; } = new();
    public DepartmentRef Department { get; set; } = new();
    public RoleRef Role { get; set; } = new();
    public DateTime AssignedAt { get; set; }
}

// These small "Ref" classes expand the related entity inline in the response
// so callers don't need to make extra requests to look up names
public class CompanyRef
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
public class DepartmentRef
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
public class RoleRef
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>What the caller sends when CREATING or UPDATING a user assignment</summary>
public class AssignUserRequest
{
    /// <summary>GUID of the user to assign</summary>
    /// <example>e3d7c9a1-1b2e-4c5d-9f0a-7b8c6d5e4f3a</example>
    [Required(ErrorMessage = "UserId is required")]
    public Guid UserId { get; set; }

    /// <summary>The company's unique code (not its ID)</summary>
    /// <example>ABC001</example>
    [Required(ErrorMessage = "CompanyCode is required")]
    public string CompanyCode { get; set; } = string.Empty;

    /// <summary>GUID of the target department</summary>
    /// <example>d290f1ee-6c54-4b01-90e6-d701748f0851</example>
    [Required(ErrorMessage = "DepartmentId is required")]
    public Guid DepartmentId { get; set; }

    /// <summary>GUID of the target role</summary>
    /// <example>a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11</example>
    [Required(ErrorMessage = "RoleId is required")]
    public Guid RoleId { get; set; }
}

// ─────────────────────────────────────────
// ERROR DTO — consistent error shape
// ─────────────────────────────────────────

/// <summary>Standard error response returned by all endpoints on failure</summary>
public class ErrorResponse
{
    /// <summary>Human-readable error message</summary>
    /// <example>Company not found</example>
    public string Error { get; set; } = string.Empty;

    /// <summary>Machine-readable error code for programmatic handling</summary>
    /// <example>COMPANY_NOT_FOUND</example>
    public string Code { get; set; } = string.Empty;
}

// ─────────────────────────────────────────
// BULK UPLOAD DTO
// ─────────────────────────────────────────

/// <summary>Response returned after a bulk Excel upload</summary>
public class BulkUploadResponse
{
    /// <summary>Number of rows successfully processed</summary>
    /// <example>10</example>
    public int SuccessCount { get; set; }

    /// <summary>Number of rows that failed validation and were skipped</summary>
    /// <example>2</example>
    public int FailedCount { get; set; }

    /// <summary>Details of each failed row</summary>
    public List<BulkUploadError> Errors { get; set; } = new();
}

public class BulkUploadError
{
    /// <summary>1-based Excel row number that failed</summary>
    /// <example>3</example>
    public int Row { get; set; }

    /// <example>a1b2c3d4-0000-0000-0000-000000000000</example>
    public string? UserId { get; set; }

    /// <example>Invalid Company Code: 'ZZZ999' does not exist</example>
    public string Message { get; set; } = string.Empty;

    /// <example>INVALID_COMPANY_CODE</example>
    public string Code { get; set; } = string.Empty;
}
