using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MasterDataApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersAndAssignmentUserFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    CompanyId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departments_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UserAssignments",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CompanyCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DepartmentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAssignments", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserAssignments_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserAssignments_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserAssignments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "Id", "Code", "CreatedAt", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("3fa85f64-5717-4562-b3fc-2c963f66afa6"), "ABC001", new DateTime(2024, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), "ABC Ltd", new DateTime(2024, 3, 20, 14, 30, 0, 0, DateTimeKind.Utc) },
                    { new Guid("7c9e6679-7425-40de-944b-e07fc1f90ae7"), "XYZ002", new DateTime(2024, 2, 1, 8, 0, 0, 0, DateTimeKind.Utc), "XYZ Corp", new DateTime(2024, 2, 1, 8, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "Id", "CompanyId", "CreatedAt", "Name", "UpdatedAt" },
                values: new object[] { new Guid("f47ac10b-58cc-4372-a567-0e02b2c3d479"), null, new DateTime(2024, 1, 10, 8, 0, 0, 0, DateTimeKind.Utc), "IT Support", new DateTime(2024, 1, 10, 8, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedAt", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11"), new DateTime(2024, 1, 10, 8, 0, 0, 0, DateTimeKind.Utc), "Manager", new DateTime(2024, 1, 10, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b1ccde00-ad1c-5f09-cc7e-7cc0ce491b22"), new DateTime(2024, 1, 10, 8, 0, 0, 0, DateTimeKind.Utc), "Analyst", new DateTime(2024, 1, 10, 8, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("e3d7c9a1-1b2e-4c5d-9f0a-7b8c6d5e4f3a"), new DateTime(2024, 1, 10, 8, 0, 0, 0, DateTimeKind.Utc), "john.carter@example.com", "John Carter", new DateTime(2024, 1, 10, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("f4e8d0b2-0000-0000-0000-000000000002"), new DateTime(2024, 1, 10, 8, 0, 0, 0, DateTimeKind.Utc), "priya.nair@example.com", "Priya Nair", new DateTime(2024, 1, 10, 8, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "Id", "CompanyId", "CreatedAt", "Name", "UpdatedAt" },
                values: new object[] { new Guid("d290f1ee-6c54-4b01-90e6-d701748f0851"), new Guid("3fa85f64-5717-4562-b3fc-2c963f66afa6"), new DateTime(2024, 1, 16, 10, 0, 0, 0, DateTimeKind.Utc), "Human Resources", new DateTime(2024, 1, 16, 10, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Code",
                table: "Companies",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_CompanyId",
                table: "Departments",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAssignments_DepartmentId",
                table: "UserAssignments",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAssignments_RoleId",
                table: "UserAssignments",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserAssignments");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Companies");
        }
    }
}
