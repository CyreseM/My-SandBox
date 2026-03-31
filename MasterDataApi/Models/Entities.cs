// =============================================================
// Models/Entities.cs
// These are the "shape" of your data - think of them as database
// table definitions written in C#. Each class = one table.
// =============================================================

namespace MasterDataApi.Models;

/// <summary>
/// Represents an application user that can be assigned to an org position.
/// </summary>
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public UserAssignment? Assignment { get; set; }
}

/// <summary>
/// Represents a company - the top-level organizational unit.
/// All departments and user assignments trace back to a company.
/// </summary>
public class Company
{
    /// <summary>
    /// Unique identifier (GUID). The database generates this automatically.
    /// We never let callers set this - it's always system-generated.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Full company name, e.g. "Acme Corporation"</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Short, unique alphanumeric code, e.g. "ACME001".
    /// This is the human-readable identifier used in Excel uploads and assignments.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Timestamp of when this record was created (UTC)</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Timestamp of the last update (UTC)</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property: one company can have many departments
    // EF Core uses this to understand the relationship
    public ICollection<Department> Departments { get; set; } = new List<Department>();
}

/// <summary>
/// A department within a company (or global, shared across all companies).
/// Examples: "Human Resources", "Engineering", "Finance"
/// </summary>
public class Department
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional: links this department to a specific company.
    /// If null, the department is "global" and available to all companies.
    /// </summary>
    public Guid? CompanyId { get; set; }

    /// <summary>
    /// Computed flag. True when CompanyId is null (i.e. not scoped to one company).
    /// </summary>
    public bool IsGlobal => CompanyId == null;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property back to the parent Company
    public Company? Company { get; set; }
}

/// <summary>
/// A role defines what someone does in the organization.
/// Roles are GLOBAL - "Manager" means the same thing in every company.
/// Examples: "Manager", "Analyst", "Director", "Intern"
/// </summary>
public class Role
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Links a user to exactly one company + department + role at a time.
/// Think of it as: "User X works at Company Y, in Department Z, as Role W".
/// A user can only have ONE active assignment.
/// </summary>
public class UserAssignment
{
    /// <summary>The GUID of the user being assigned (from your auth/user service)</summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// We store the company CODE (not the ID) because the API contract uses
    /// the human-readable code in assignment requests.
    /// </summary>
    public string CompanyCode { get; set; } = string.Empty;

    public Guid DepartmentId { get; set; }
    public Guid RoleId { get; set; }

    /// <summary>When this assignment was created or last updated</summary>
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties - EF Core populates these from the database
    public User? User { get; set; }
    public Department? Department { get; set; }
    public Role? Role { get; set; }
}
