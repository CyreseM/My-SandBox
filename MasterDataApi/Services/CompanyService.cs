// =============================================================
// Services/CompanyService.cs
//
// This is the BRAIN of company operations.
// Controllers receive HTTP requests, then delegate the real work
// to services. Services talk to the database via DbContext.
//
// Why the separation? Controllers should only handle HTTP stuff
// (parsing requests, returning responses). Business logic lives here.
// =============================================================

using Microsoft.EntityFrameworkCore;
using MasterDataApi.Data;
using MasterDataApi.DTOs;
using MasterDataApi.Models;

namespace MasterDataApi.Services;

public class CompanyService : ICompanyService
{
    // _db is our gateway to the database
    // It's injected by ASP.NET Core's DI container
    private readonly AppDbContext _db;

    public CompanyService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Fetch ALL companies from the database.
    /// .Select() maps each Company entity → CompanyResponse DTO
    /// so we never accidentally expose internal fields.
    /// </summary>
    public async Task<IEnumerable<CompanyResponse>> GetAllAsync()
    {
        return await _db.Companies
            .OrderBy(c => c.Name)
            .Select(c => MapToResponse(c))
            .ToListAsync();
    }

    /// <summary>
    /// Fetch a single company by ID.
    /// Returns null if not found — the controller turns null into 404.
    /// </summary>
    public async Task<CompanyResponse?> GetByIdAsync(Guid id)
    {
        var company = await _db.Companies.FindAsync(id);
        return company == null ? null : MapToResponse(company);
    }

    /// <summary>
    /// Create a new company.
    /// Returns (result: the created company, errorCode: null) on success.
    /// Returns (result: null, errorCode: "DUPLICATE_COMPANY_CODE") if code conflicts.
    /// The tuple lets us return both a result and an error without exceptions.
    /// </summary>
    public async Task<(CompanyResponse? result, string? errorCode)> CreateAsync(CreateCompanyRequest request)
    {
        // Check for duplicate code BEFORE inserting
        bool codeExists = await _db.Companies
            .AnyAsync(c => c.Code.ToLower() == request.Code.ToLower());

        if (codeExists)
            return (null, "DUPLICATE_COMPANY_CODE");

        var company = new Company
        {
            Name = request.Name.Trim(),
            Code = request.Code.Trim().ToUpper(),
        };

        _db.Companies.Add(company);
        await _db.SaveChangesAsync(); // Actually write to the database

        return (MapToResponse(company), null);
    }

    /// <summary>
    /// Update a company. Finds it, changes its fields, saves.
    /// </summary>
    public async Task<(CompanyResponse? result, string? errorCode)> UpdateAsync(Guid id, UpdateCompanyRequest request)
    {
        var company = await _db.Companies.FindAsync(id);
        if (company == null) return (null, "COMPANY_NOT_FOUND");

        // Check if the new code conflicts with ANOTHER company's code
        bool codeTaken = await _db.Companies
            .AnyAsync(c => c.Code.ToLower() == request.Code.ToLower() && c.Id != id);

        if (codeTaken) return (null, "DUPLICATE_COMPANY_CODE");

        // Update fields
        company.Name = request.Name.Trim();
        company.Code = request.Code.Trim().ToUpper();
        company.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return (MapToResponse(company), null);
    }

    /// <summary>
    /// Delete a company. Returns false if it wasn't found.
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id)
    {
        var company = await _db.Companies.FindAsync(id);
        if (company == null) return false;

        _db.Companies.Remove(company);
        await _db.SaveChangesAsync();
        return true;
    }

    // ─── Private helper ──────────────────────────────────────
    // Maps the database entity → the DTO shape we return to callers.
    // Static because it doesn't need any instance state.
    private static CompanyResponse MapToResponse(Company c) => new()
    {
        Id        = c.Id,
        Name      = c.Name,
        Code      = c.Code,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt,
    };
}
