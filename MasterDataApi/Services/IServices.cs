// =============================================================
// Services/IServices.cs
//
// Interfaces define WHAT a service can do, without saying HOW.
// Controllers depend on the interface, not the concrete class.
// This lets you swap implementations (e.g. for tests) easily.
// =============================================================

using MasterDataApi.DTOs;

namespace MasterDataApi.Services;

// ── Company Service ──────────────────────────────────────────
public interface ICompanyService
{
    Task<IEnumerable<CompanyResponse>> GetAllAsync();
    Task<CompanyResponse?> GetByIdAsync(Guid id);
    Task<(CompanyResponse? result, string? errorCode)> CreateAsync(CreateCompanyRequest request);
    Task<(CompanyResponse? result, string? errorCode)> UpdateAsync(Guid id, UpdateCompanyRequest request);
    Task<bool> DeleteAsync(Guid id);
}

// ── Department Service ───────────────────────────────────────
public interface IDepartmentService
{
    Task<IEnumerable<DepartmentResponse>> GetAllAsync(Guid? companyId, bool? globalOnly);
    Task<DepartmentResponse?> GetByIdAsync(Guid id);
    Task<DepartmentResponse> CreateAsync(CreateDepartmentRequest request);
    Task<DepartmentResponse?> UpdateAsync(Guid id, UpdateDepartmentRequest request);
    Task<bool> DeleteAsync(Guid id);
}

// ── Role Service ─────────────────────────────────────────────
public interface IRoleService
{
    Task<IEnumerable<RoleResponse>> GetAllAsync();
    Task<RoleResponse?> GetByIdAsync(Guid id);
    Task<(RoleResponse? result, string? errorCode)> CreateAsync(CreateRoleRequest request);
    Task<RoleResponse?> UpdateAsync(Guid id, UpdateRoleRequest request);
    Task<bool> DeleteAsync(Guid id);
}

// ── User Service ─────────────────────────────────────────────
public interface IUserService
{
    Task<IEnumerable<UserResponse>> GetAllAsync();
    Task<UserResponse?> GetByIdAsync(Guid id);
    Task<(UserResponse? result, string? errorCode)> CreateAsync(CreateUserRequest request);
    Task<(UserResponse? result, string? errorCode)> UpdateAsync(Guid id, UpdateUserRequest request);
    Task<bool> DeleteAsync(Guid id);
}

// ── User Assignment Service ──────────────────────────────────
public interface IUserAssignmentService
{
    Task<(AssignmentResponse? result, string? errorCode)> AssignAsync(AssignUserRequest request);
    Task<AssignmentResponse?> GetByUserIdAsync(Guid userId);
    Task<(AssignmentResponse? result, string? errorCode)> UpdateAsync(Guid userId, AssignUserRequest request);
    Task<bool> DeleteAsync(Guid userId);
}

// ── Bulk Upload Service ──────────────────────────────────────
public interface IBulkUploadService
{
    Task<BulkUploadResponse> ProcessExcelAsync(Stream fileStream);
    byte[] GenerateTemplate();
}
