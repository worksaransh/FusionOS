using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FusionOS.Modules.Quality.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "quality");

            migrationBuilder.CreateTable(
                name: "calibration_records",
                schema: "quality",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EquipmentIdentifier = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CalibrationStandard = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastCalibratedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    NextCalibrationDueDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Result = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PerformedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PerformedByExternalVendor = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_calibration_records", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "corrective_actions",
                schema: "quality",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NonConformanceReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    RootCauseDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CorrectiveActionDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    PreventiveActionDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DueDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    VerifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RcaMethod = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Other"),
                    Why1 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Why2 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Why3 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Why4 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Why5 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_corrective_actions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "deviation_requests",
                schema: "quality",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InspectionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedOrRejectedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecisionNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_deviation_requests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "inspection_plans",
                schema: "quality",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    InspectionType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
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
                    table.PrimaryKey("PK_inspection_plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "inspections",
                schema: "quality",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
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
                    table.PrimaryKey("PK_inspections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "non_conformance_reports",
                schema: "quality",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InspectionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RaisedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RaisedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_non_conformance_reports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "quality",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "text", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    OccurredOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProcessedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessedIntegrationEvents",
                schema: "quality",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "text", nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedIntegrationEvents", x => x.EventId);
                });

            migrationBuilder.CreateTable(
                name: "quality_audits",
                schema: "quality",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuditType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ScheduledDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Auditor = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Scope = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
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
                    table.PrimaryKey("PK_quality_audits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "quality_certificates",
                schema: "quality",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InspectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CertificateNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IssuedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IssuedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_quality_certificates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "quality_holds",
                schema: "quality",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    InspectionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ReleasedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReleasedByNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_quality_holds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "inspection_plan_characteristics",
                schema: "quality",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CharacteristicName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    InspectionPlanId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inspection_plan_characteristics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_inspection_plan_characteristics_inspection_plans_Inspection~",
                        column: x => x.InspectionPlanId,
                        principalSchema: "quality",
                        principalTable: "inspection_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "inspection_items",
                schema: "quality",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Characteristic = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Passed = table.Column<bool>(type: "boolean", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    MeasuredValue = table.Column<decimal>(type: "numeric(19,4)", nullable: true),
                    LowerControlLimit = table.Column<decimal>(type: "numeric(19,4)", nullable: true),
                    UpperControlLimit = table.Column<decimal>(type: "numeric(19,4)", nullable: true),
                    InspectionId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inspection_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_inspection_items_inspections_InspectionId",
                        column: x => x.InspectionId,
                        principalSchema: "quality",
                        principalTable: "inspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "audit_findings",
                schema: "quality",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsResolved = table.Column<bool>(type: "boolean", nullable: false),
                    QualityAuditId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_findings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_audit_findings_quality_audits_QualityAuditId",
                        column: x => x.QualityAuditId,
                        principalSchema: "quality",
                        principalTable: "quality_audits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_findings_QualityAuditId",
                schema: "quality",
                table: "audit_findings",
                column: "QualityAuditId");

            migrationBuilder.CreateIndex(
                name: "IX_calibration_records_CompanyId_EquipmentReferenceId",
                schema: "quality",
                table: "calibration_records",
                columns: new[] { "CompanyId", "EquipmentReferenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_calibration_records_CompanyId_NextCalibrationDueDate",
                schema: "quality",
                table: "calibration_records",
                columns: new[] { "CompanyId", "NextCalibrationDueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_corrective_actions_CompanyId_AssignedToUserId",
                schema: "quality",
                table: "corrective_actions",
                columns: new[] { "CompanyId", "AssignedToUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_corrective_actions_CompanyId_NonConformanceReportId",
                schema: "quality",
                table: "corrective_actions",
                columns: new[] { "CompanyId", "NonConformanceReportId" });

            migrationBuilder.CreateIndex(
                name: "IX_corrective_actions_CompanyId_Status",
                schema: "quality",
                table: "corrective_actions",
                columns: new[] { "CompanyId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_deviation_requests_CompanyId_InspectionId",
                schema: "quality",
                table: "deviation_requests",
                columns: new[] { "CompanyId", "InspectionId" });

            migrationBuilder.CreateIndex(
                name: "IX_deviation_requests_CompanyId_Status",
                schema: "quality",
                table: "deviation_requests",
                columns: new[] { "CompanyId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_inspection_items_InspectionId",
                schema: "quality",
                table: "inspection_items",
                column: "InspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_inspection_plan_characteristics_InspectionPlanId",
                schema: "quality",
                table: "inspection_plan_characteristics",
                column: "InspectionPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_inspection_plans_CompanyId_InspectionType",
                schema: "quality",
                table: "inspection_plans",
                columns: new[] { "CompanyId", "InspectionType" });

            migrationBuilder.CreateIndex(
                name: "IX_inspection_plans_CompanyId_IsActive",
                schema: "quality",
                table: "inspection_plans",
                columns: new[] { "CompanyId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_inspections_CompanyId_Status",
                schema: "quality",
                table: "inspections",
                columns: new[] { "CompanyId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_inspections_ReferenceId",
                schema: "quality",
                table: "inspections",
                column: "ReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_non_conformance_reports_CompanyId_InspectionId",
                schema: "quality",
                table: "non_conformance_reports",
                columns: new[] { "CompanyId", "InspectionId" });

            migrationBuilder.CreateIndex(
                name: "IX_non_conformance_reports_CompanyId_Status",
                schema: "quality",
                table: "non_conformance_reports",
                columns: new[] { "CompanyId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_quality_audits_CompanyId_Status",
                schema: "quality",
                table: "quality_audits",
                columns: new[] { "CompanyId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_quality_certificates_CompanyId_CertificateNumber",
                schema: "quality",
                table: "quality_certificates",
                columns: new[] { "CompanyId", "CertificateNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_quality_certificates_CompanyId_InspectionId",
                schema: "quality",
                table: "quality_certificates",
                columns: new[] { "CompanyId", "InspectionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_quality_holds_CompanyId_EntityType_EntityId",
                schema: "quality",
                table: "quality_holds",
                columns: new[] { "CompanyId", "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_quality_holds_CompanyId_Status",
                schema: "quality",
                table: "quality_holds",
                columns: new[] { "CompanyId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_quality_holds_InspectionId",
                schema: "quality",
                table: "quality_holds",
                column: "InspectionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_findings",
                schema: "quality");

            migrationBuilder.DropTable(
                name: "calibration_records",
                schema: "quality");

            migrationBuilder.DropTable(
                name: "corrective_actions",
                schema: "quality");

            migrationBuilder.DropTable(
                name: "deviation_requests",
                schema: "quality");

            migrationBuilder.DropTable(
                name: "inspection_items",
                schema: "quality");

            migrationBuilder.DropTable(
                name: "inspection_plan_characteristics",
                schema: "quality");

            migrationBuilder.DropTable(
                name: "non_conformance_reports",
                schema: "quality");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "quality");

            migrationBuilder.DropTable(
                name: "ProcessedIntegrationEvents",
                schema: "quality");

            migrationBuilder.DropTable(
                name: "quality_certificates",
                schema: "quality");

            migrationBuilder.DropTable(
                name: "quality_holds",
                schema: "quality");

            migrationBuilder.DropTable(
                name: "quality_audits",
                schema: "quality");

            migrationBuilder.DropTable(
                name: "inspections",
                schema: "quality");

            migrationBuilder.DropTable(
                name: "inspection_plans",
                schema: "quality");
        }
    }
}
