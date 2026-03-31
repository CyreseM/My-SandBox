# Master Data & User Assignment API
## Beginner Developer Tutorial

---

## What Is This Project?

This is a **REST API** built with **ASP.NET Core 8** (.NET 8). It manages an organization's structure:
- **Companies** (e.g. "ABC Ltd")
- **Departments** within companies (e.g. "Human Resources")
- **Roles** (e.g. "Manager", "Analyst")
- **User Assignments** — linking a user to a company + department + role

The API comes with:
- 📄 **Swagger UI** — an interactive web page to test all endpoints in your browser
- 🔐 **JWT Authentication** — every request needs a token
- 📊 **Excel Bulk Upload** — add many users at once via `.xlsx` file
- 🗄️ **In-Memory Database** — no database install needed to get started

---

## Project Structure

```
MasterDataApi/
├── Controllers/          ← Handle HTTP requests (the "front door")
│   ├── CompaniesController.cs
│   ├── DepartmentsController.cs
│   ├── RolesAndAssignmentsController.cs
│   └── BulkUploadController.cs
│
├── Services/             ← Business logic (the "brain")
│   ├── IServices.cs            ← Interfaces (contracts)
│   ├── CompanyService.cs
│   ├── DepartmentRoleAssignmentServices.cs
│   └── BulkUploadService.cs
│
├── Models/               ← Database entities (table shapes)
│   └── Entities.cs
│
├── DTOs/                 ← API request/response shapes
│   └── Dtos.cs
│
├── Data/                 ← Database configuration
│   └── AppDbContext.cs
│
├── Program.cs            ← App entry point & DI configuration
├── appsettings.json      ← Configuration (JWT key, DB connection, etc.)
└── MasterDataApi.csproj  ← Project file (NuGet packages, build settings)
```

---

## Prerequisites

| Tool | Version | Download |
|------|---------|----------|
| .NET SDK | 8.0+ | https://dotnet.microsoft.com/download |
| A code editor | Any | VS Code, Visual Studio, Rider |

Check your .NET version:
```bash
dotnet --version
# Should output: 8.0.x
```

---

## How to Run

### 1. Restore packages
```bash
cd MasterDataApi
dotnet restore
```

### 2. Run the application
```bash
dotnet run
```

You should see output like:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started.
```

### 3. Open Swagger UI
Navigate to: **http://localhost:5000**

You'll see the interactive API documentation page.

---

## Your First Test (Step-by-Step)

### Step 1 — Get a Dev Token

In Swagger, find the **Auth** section and call:
```
GET /api/auth/dev-token
```
Copy the `token` value from the response.

### Step 2 — Authorize Swagger

Click the **🔒 Authorize** button at the top of Swagger UI.
Paste your token (just the token string, not "Bearer ").
Click **Authorize**, then **Close**.

### Step 3 — Create a Company

Call `POST /api/companies` with:
```json
{
  "name": "My Test Company",
  "code": "TEST001"
}
```
Note the `id` in the response — you'll need it.

### Step 4 — Create a Department

Call `POST /api/departments` with:
```json
{
  "name": "Engineering",
  "companyId": "<paste the company id from Step 3>"
}
```

### Step 5 — Create a Role

Call `POST /api/roles` with:
```json
{
  "name": "Developer"
}
```

### Step 6 — Assign a User

Call `POST /api/users/assign` with:
```json
{
  "userId": "00000000-0000-0000-0000-000000000001",
  "companyCode": "TEST001",
  "departmentId": "<department id from Step 4>",
  "roleId": "<role id from Step 5>"
}
```

### Step 7 — View the Assignment

Call `GET /api/users/00000000-0000-0000-0000-000000000001/assignment`

You should see the full expanded assignment with all names filled in.

---

## Pre-Loaded Test Data

When the app starts, these records are automatically created:

### Companies
| ID | Name | Code |
|----|------|------|
| `3fa85f64-5717-4562-b3fc-2c963f66afa6` | ABC Ltd | ABC001 |
| `7c9e6679-7425-40de-944b-e07fc1f90ae7` | XYZ Corp | XYZ002 |

### Departments
| ID | Name | Company | Global? |
|----|------|---------|---------|
| `d290f1ee-6c54-4b01-90e6-d701748f0851` | Human Resources | ABC Ltd | No |
| `f47ac10b-58cc-4372-a567-0e02b2c3d479` | IT Support | — | Yes |

### Roles
| ID | Name |
|----|------|
| `a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11` | Manager |
| `b1ccde00-ad1c-5f09-cc7e-7cc0ce491b22` | Analyst |

---

## API Endpoints Quick Reference

### Companies
| Method | URL | What it does |
|--------|-----|-------------|
| GET | `/api/companies` | List all companies |
| GET | `/api/companies/{id}` | Get one company |
| POST | `/api/companies` | Create a company |
| PUT | `/api/companies/{id}` | Update a company |
| DELETE | `/api/companies/{id}` | Delete a company |

### Departments
| Method | URL | What it does |
|--------|-----|-------------|
| GET | `/api/departments` | List all (supports `?companyId=` and `?global=true`) |
| GET | `/api/departments/{id}` | Get one department |
| POST | `/api/departments` | Create a department |
| PUT | `/api/departments/{id}` | Update a department |
| DELETE | `/api/departments/{id}` | Delete a department |

### Roles
| Method | URL | What it does |
|--------|-----|-------------|
| GET | `/api/roles` | List all roles |
| GET | `/api/roles/{id}` | Get one role |
| POST | `/api/roles` | Create a role |
| PUT | `/api/roles/{id}` | Update a role |
| DELETE | `/api/roles/{id}` | Delete a role |

### User Assignments
| Method | URL | What it does |
|--------|-----|-------------|
| POST | `/api/users/assign` | Create/replace an assignment |
| GET | `/api/users/{id}/assignment` | Get a user's assignment |
| PUT | `/api/users/{id}/assignment` | Update an assignment |
| DELETE | `/api/users/{id}/assignment` | Remove an assignment |

### Bulk Upload
| Method | URL | What it does |
|--------|-----|-------------|
| POST | `/api/users/upload` | Upload `.xlsx` to bulk assign |
| GET | `/api/users/template` | Download the `.xlsx` template |

---

## Bulk Excel Upload Guide

### Step 1 — Download the template
```
GET /api/users/template
```
This downloads `user_assignment_template.xlsx`

### Step 2 — Fill in the spreadsheet

| UserId | CompanyCode | DepartmentName | RoleName |
|--------|-------------|----------------|----------|
| e3d7c9a1-... | ABC001 | Human Resources | Manager |
| f4e8d0b2-... | XYZ002 | IT Support | Analyst |

**Rules:**
- `UserId` must be a valid GUID format
- `CompanyCode` must exactly match an existing company code
- `DepartmentName` must exactly match an existing department name
- `RoleName` must exactly match an existing role name

### Step 3 — Upload
```
POST /api/users/upload
```
Select your `.xlsx` file in the form.

### Step 4 — Check the response
```json
{
  "successCount": 2,
  "failedCount": 0,
  "errors": []
}
```

---

## Error Codes Reference

| HTTP Status | Code | Meaning |
|-------------|------|---------|
| 400 | `MISSING_REQUIRED_FIELD` | A required field was not provided |
| 401 | — | Token missing or expired |
| 403 | — | Valid token but insufficient permissions |
| 404 | `COMPANY_NOT_FOUND` | No company matches the ID |
| 404 | `DEPARTMENT_NOT_FOUND` | No department matches the ID |
| 404 | `ROLE_NOT_FOUND` | No role matches the ID |
| 404 | `ASSIGNMENT_NOT_FOUND` | User has no active assignment |
| 409 | `DUPLICATE_COMPANY_CODE` | Company code already in use |
| 409 | `DUPLICATE_ROLE_NAME` | Role name already in use |
| 422 | `INVALID_COMPANY_CODE` | Code doesn't match any company |
| 422 | `INVALID_DEPARTMENT_NAME` | Name doesn't match any department |
| 422 | `INVALID_ROLE_NAME` | Name doesn't match any role |
| 422 | `INVALID_USER_ID` | User GUID not found |

---

## Switching to a Real Database

The app currently uses an **in-memory database** (data is lost on restart).

To use **SQL Server**:

1. Add the NuGet package:
   ```bash
   dotnet add package Microsoft.EntityFrameworkCore.SqlServer
   ```

2. In `Program.cs`, replace:
   ```csharp
   options.UseInMemoryDatabase("MasterDataDb")
   ```
   with:
   ```csharp
   options.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
   ```

3. In `appsettings.json`, uncomment and set:
   ```json
   "ConnectionStrings": {
     "Default": "Server=localhost;Database=MasterData;Trusted_Connection=True;"
   }
   ```

4. Run migrations:
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

---

## Production Checklist

Before deploying:

- [ ] Change `Jwt:Key` in `appsettings.json` to a strong random secret
- [ ] Store the JWT key as an environment variable or secret manager, NOT in source code
- [ ] Replace InMemory database with a real one (SQL Server, PostgreSQL, etc.)
- [ ] Remove the `/api/auth/dev-token` endpoint from `Program.cs`
- [ ] Add HTTPS certificates
- [ ] Enable rate limiting
- [ ] Add proper logging (Serilog, Application Insights, etc.)
- [ ] Review the CORS policy (currently allows all origins)

---

## Key Concepts Explained

### What is a DTO?
A **Data Transfer Object** is a class that defines the exact shape of data
going into or out of an API endpoint. It prevents you from accidentally exposing
internal database fields.

### What is Dependency Injection?
DI is a design pattern where you declare what a class *needs* (via constructor
parameters) and the framework *provides* them automatically. You register
services once in `Program.cs`, and they're available everywhere.

### What is EF Core?
**Entity Framework Core** is an ORM (Object-Relational Mapper). It lets you
work with your database using C# objects instead of raw SQL. You write LINQ
queries and EF translates them to SQL.

### What is JWT?
A **JSON Web Token** is a compact, self-contained string that proves who you are.
It contains claims (like your user ID and role) and is signed with a secret key.
The server validates the signature on every request.

---

## Troubleshooting

**"Unauthorized" on all requests**
→ You forgot to authorize in Swagger. Click 🔒 Authorize and paste your token.

**Token expired**
→ Call `GET /api/auth/dev-token` again to get a fresh one (valid for 7 days).

**"Company not found" on assignment**
→ Use the company's `code` (e.g. "ABC001"), not its GUID, in the `companyCode` field.

**Data disappeared after restart**
→ Expected — the in-memory database resets on each restart. Switch to a real DB for persistence.

**Port already in use**
→ Change the port in `Properties/launchSettings.json` or set `ASPNETCORE_URLS=http://localhost:5001`.
