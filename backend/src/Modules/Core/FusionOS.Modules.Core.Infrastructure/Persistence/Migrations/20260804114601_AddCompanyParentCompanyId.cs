using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FusionOS.Modules.Core.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyParentCompanyId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentCompanyId",
                schema: "core",
                table: "companies",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "sod_conflict_rules",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConflictingPermissionA = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ConflictingPermissionB = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sod_conflict_rules", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_companies_ParentCompanyId",
                schema: "core",
                table: "companies",
                column: "ParentCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_sod_conflict_rules_CompanyId_ConflictingPermissionA_Conflic~",
                schema: "core",
                table: "sod_conflict_rules",
                columns: new[] { "CompanyId", "ConflictingPermissionA", "ConflictingPermissionB" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_companies_companies_ParentCompanyId",
                schema: "core",
                table: "companies",
                column: "ParentCompanyId",
                principalSchema: "core",
                principalTable: "companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_companies_companies_ParentCompanyId",
                schema: "core",
                table: "companies");

            migrationBuilder.DropTable(
                name: "sod_conflict_rules",
                schema: "core");

            migrationBuilder.DropIndex(
                name: "IX_companies_ParentCompanyId",
                schema: "core",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "ParentCompanyId",
                schema: "core",
                table: "companies");
        }
    }
}
