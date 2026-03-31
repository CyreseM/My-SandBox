// =============================================================
// Services/DepartmentService.cs
// =============================================================
using Microsoft.EntityFrameworkCore;
using MasterDataApi.Data;
using MasterDataApi.DTOs;
using MasterDataApi.Models;

namespace MasterDataApi.Services;

public class DepartmentService : IDepartmentService
{
    private readonly AppDbContext _db;
    public DepartmentService(AppDbContext db) { _db = db; }

    public async Task<IEnumerable<DepartmentResponse>> GetAllAsync(Guid? companyId, bool? globalOnly)
    {
        // Start with all departments — IQueryable lets EF build the SQL lazily
        var query = _db.Departments.AsQueryable();

        // Apply optional filters
        if (companyId.HasValue)
            query = query.Where(d => d.CompanyId == companyId.Value);

        if (globalOnly == true)
            query = query.Where(d => d.CompanyId == null);

        return await query
            .OrderBy(d => d.Name)
            .Select(d => MapToResponse(d))
            .ToListAsync();
    }

    public async Task<DepartmentResponse?> GetByIdAsync(Guid id)
    {
        var dept = await _db.Departments.FindAsync(id);
        return dept == null ? null : MapToResponse(dept);
    }

    public async Task<DepartmentResponse> CreateAsync(CreateDepartmentRequest request)
    {
        var dept = new Department
        {
            Name      = request.Name.Trim(),
            CompanyId = request.CompanyId,
        };
        _db.Departments.Add(dept);
        await _db.SaveChangesAsync();
        return MapToResponse(dept);
    }

    public async Task<DepartmentResponse?> UpdateAsync(Guid id, UpdateDepartmentRequest request)
    {
        var dept = await _db.Departments.FindAsync(id);
        if (dept == null) return null;

        dept.Name      = request.Name.Trim();
        dept.CompanyId = request.CompanyId;
        dept.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapToResponse(dept);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var dept = await _db.Departments.FindAsync(id);
        if (dept == null) return false;

        _db.Departments.Remove(dept);
        await _db.SaveChangesAsync();
        return true;
    }

    private static DepartmentResponse MapToResponse(Department d) => new()
    {
        Id        = d.Id,
        Name      = d.Name,
        CompanyId = d.CompanyId,
        IsGlobal  = d.IsGlobal,
        CreatedAt = d.CreatedAt,
        UpdatedAt = d.UpdatedAt,
    };
}

// =============================================================
// Services/RoleService.cs
// =============================================================
public class RoleService : IRoleService
{
    private readonly AppDbContext _db;
    public RoleService(AppDbContext db) { _db = db; }

    public async Task<IEnumerable<RoleResponse>> GetAllAsync()
    {
        return await _db.Roles
            .OrderBy(r => r.Name)
            .Select(r => MapToResponse(r))
            .ToListAsync();
    }

    public async Task<RoleResponse?> GetByIdAsync(Guid id)
    {
        var role = await _db.Roles.FindAsync(id);
        return role == null ? null : MapToResponse(role);
    }

    public async Task<(RoleResponse? result, string? errorCode)> CreateAsync(CreateRoleRequest request)
    {
        bool nameExists = await _db.Roles.AnyAsync(r => r.Name.ToLower() == request.Name.ToLower());
        if (nameExists) return (null, "DUPLICATE_ROLE_NAME");

        var role = new Role { Name = request.Name.Trim() };
        _db.Roles.Add(role);
        await _db.SaveChangesAsync();
        return (MapToResponse(role), null);
    }

    public async Task<RoleResponse?> UpdateAsync(Guid id, UpdateRoleRequest request)
    {
        var role = await _db.Roles.FindAsync(id);
        if (role == null) return null;

        role.Name      = request.Name.Trim();
        role.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapToResponse(role);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var role = await _db.Roles.FindAsync(id);
        if (role == null) return false;

        _db.Roles.Remove(role);
        await _db.SaveChangesAsync();
        return true;
    }

    private static RoleResponse MapToResponse(Role r) => new()
    {
        Id        = r.Id,
        Name      = r.Name,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt,
    };
}

// =============================================================
// Services/UserAssignmentService.cs
// =============================================================
public class UserAssignmentService : IUserAssignmentService
{
    private readonly AppDbContext _db;
    public UserAssignmentService(AppDbContext db) { _db = db; }

    public async Task<(AssignmentResponse? result, string? errorCode)> AssignAsync(AssignUserRequest request)
    {
        // Validate all referenced entities exist before writing anything
        var user = await _db.Users.FindAsync(request.UserId);
        if (user == null) return (null, "USER_NOT_FOUND");

        var company = await _db.Companies.FirstOrDefaultAsync(c => c.Code == request.CompanyCode);
        if (company == null) return (null, "INVALID_COMPANY_CODE");

        var department = await _db.Departments.FindAsync(request.DepartmentId);
        if (department == null) return (null, "DEPARTMENT_NOT_FOUND");

        var role = await _db.Roles.FindAsync(request.RoleId);
        if (role == null) return (null, "ROLE_NOT_FOUND");

        // If user already has an assignment, replace it (upsert behavior)
        var existing = await _db.UserAssignments.FindAsync(request.UserId);
        if (existing != null)
            _db.UserAssignments.Remove(existing);

        var assignment = new UserAssignment
        {
            UserId       = request.UserId,
            CompanyCode  = request.CompanyCode,
            DepartmentId = request.DepartmentId,
            RoleId       = request.RoleId,
            AssignedAt   = DateTime.UtcNow,
        };

        _db.UserAssignments.Add(assignment);
        await _db.SaveChangesAsync();

        return (BuildResponse(assignment, company, department, role), null);
    }

    public async Task<AssignmentResponse?> GetByUserIdAsync(Guid userId)
    {
        // Include() tells EF Core to JOIN and load the related Department and Role rows
        var assignment = await _db.UserAssignments
            .Include(ua => ua.Department)
            .Include(ua => ua.Role)
            .FirstOrDefaultAsync(ua => ua.UserId == userId);

        if (assignment == null) return null;

        var company = await _db.Companies
            .FirstOrDefaultAsync(c => c.Code == assignment.CompanyCode);

        if (company == null || assignment.Department == null || assignment.Role == null)
            return null;

        return BuildResponse(assignment, company, assignment.Department, assignment.Role);
    }

    public async Task<(AssignmentResponse? result, string? errorCode)> UpdateAsync(Guid userId, AssignUserRequest request)
    {
        var existing = await _db.UserAssignments.FindAsync(userId);
        if (existing == null) return (null, "ASSIGNMENT_NOT_FOUND");

        // Re-use the assign logic (it handles upsert)
        request.UserId = userId;
        return await AssignAsync(request);
    }

    public async Task<bool> DeleteAsync(Guid userId)
    {
        var assignment = await _db.UserAssignments.FindAsync(userId);
        if (assignment == null) return false;

        _db.UserAssignments.Remove(assignment);
        await _db.SaveChangesAsync();
        return true;
    }

    // Build the expanded response DTO from the entities we already fetched
    private static AssignmentResponse BuildResponse(
        UserAssignment ua, Company company, Department dept, Role role) => new()
    {
        UserId     = ua.UserId,
        AssignedAt = ua.AssignedAt,
        Company    = new CompanyRef    { Id = company.Id, Name = company.Name, Code = company.Code },
        Department = new DepartmentRef { Id = dept.Id,    Name = dept.Name },
        Role       = new RoleRef       { Id = role.Id,    Name = role.Name },
    };
}
