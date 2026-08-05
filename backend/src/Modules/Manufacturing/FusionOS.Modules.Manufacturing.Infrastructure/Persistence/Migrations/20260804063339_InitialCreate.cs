using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FusionOS.Modules.Manufacturing.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "manufacturing");

            migrationBuilder.CreateTable(
                name: "bills_of_materials",
                schema: "manufacturing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    IsCurrentVersion = table.Column<bool>(type: "boolean", nullable: false),
                    PreviousVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_bills_of_materials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "machines",
                schema: "manufacturing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    WorkCenterId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_machines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "manufacturing",
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
                schema: "manufacturing",
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
                name: "work_centers",
                schema: "manufacturing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CapacityHoursPerDay = table.Column<decimal>(type: "numeric(9,2)", nullable: false),
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
                    table.PrimaryKey("PK_work_centers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "work_orders",
                schema: "manufacturing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BillOfMaterialsId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityToProduce = table.Column<decimal>(type: "numeric(19,4)", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SourceWorkOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    QuantityReworked = table.Column<decimal>(type: "numeric(19,4)", nullable: false),
                    QuantityGoodProduced = table.Column<decimal>(type: "numeric(19,4)", nullable: true),
                    QuantityScrapped = table.Column<decimal>(type: "numeric(19,4)", nullable: false),
                    YieldPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
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
                    table.PrimaryKey("PK_work_orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "bom_lines",
                schema: "manufacturing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ComponentProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(19,4)", nullable: false),
                    UnitCost = table.Column<decimal>(type: "numeric(19,4)", nullable: false),
                    BillOfMaterialsId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bom_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_bom_lines_bills_of_materials_BillOfMaterialsId",
                        column: x => x.BillOfMaterialsId,
                        principalSchema: "manufacturing",
                        principalTable: "bills_of_materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "routing_operations",
                schema: "manufacturing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    OperationName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    WorkCenter = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StandardMinutes = table.Column<decimal>(type: "numeric(19,4)", nullable: false),
                    BillOfMaterialsId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routing_operations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_routing_operations_bills_of_materials_BillOfMaterialsId",
                        column: x => x.BillOfMaterialsId,
                        principalSchema: "manufacturing",
                        principalTable: "bills_of_materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "work_order_components",
                schema: "manufacturing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ComponentProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityRequired = table.Column<decimal>(type: "numeric(19,4)", nullable: false),
                    QuantityIssued = table.Column<decimal>(type: "numeric(19,4)", nullable: false),
                    UnitCost = table.Column<decimal>(type: "numeric(19,4)", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_order_components", x => x.Id);
                    table.ForeignKey(
                        name: "FK_work_order_components_work_orders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalSchema: "manufacturing",
                        principalTable: "work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "work_order_operations",
                schema: "manufacturing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoutingOperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    OperationName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    WorkCenter = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StandardMinutes = table.Column<decimal>(type: "numeric(19,4)", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MachineId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScheduledStart = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ScheduledEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ActualStart = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ActualEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_order_operations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_work_order_operations_work_orders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalSchema: "manufacturing",
                        principalTable: "work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bills_of_materials_CompanyId_Code_Version",
                schema: "manufacturing",
                table: "bills_of_materials",
                columns: new[] { "CompanyId", "Code", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_bills_of_materials_ProductId",
                schema: "manufacturing",
                table: "bills_of_materials",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_bom_lines_BillOfMaterialsId",
                schema: "manufacturing",
                table: "bom_lines",
                column: "BillOfMaterialsId");

            migrationBuilder.CreateIndex(
                name: "IX_machines_CompanyId_Code",
                schema: "manufacturing",
                table: "machines",
                columns: new[] { "CompanyId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_machines_WorkCenterId",
                schema: "manufacturing",
                table: "machines",
                column: "WorkCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_routing_operations_BillOfMaterialsId",
                schema: "manufacturing",
                table: "routing_operations",
                column: "BillOfMaterialsId");

            migrationBuilder.CreateIndex(
                name: "IX_work_centers_CompanyId_Code",
                schema: "manufacturing",
                table: "work_centers",
                columns: new[] { "CompanyId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_work_order_components_WorkOrderId",
                schema: "manufacturing",
                table: "work_order_components",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_work_order_operations_MachineId",
                schema: "manufacturing",
                table: "work_order_operations",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_work_order_operations_WorkOrderId",
                schema: "manufacturing",
                table: "work_order_operations",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_BillOfMaterialsId",
                schema: "manufacturing",
                table: "work_orders",
                column: "BillOfMaterialsId");

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_CompanyId_Status",
                schema: "manufacturing",
                table: "work_orders",
                columns: new[] { "CompanyId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_SourceWorkOrderId",
                schema: "manufacturing",
                table: "work_orders",
                column: "SourceWorkOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bom_lines",
                schema: "manufacturing");

            migrationBuilder.DropTable(
                name: "machines",
                schema: "manufacturing");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "manufacturing");

            migrationBuilder.DropTable(
                name: "ProcessedIntegrationEvents",
                schema: "manufacturing");

            migrationBuilder.DropTable(
                name: "routing_operations",
                schema: "manufacturing");

            migrationBuilder.DropTable(
                name: "work_centers",
                schema: "manufacturing");

            migrationBuilder.DropTable(
                name: "work_order_components",
                schema: "manufacturing");

            migrationBuilder.DropTable(
                name: "work_order_operations",
                schema: "manufacturing");

            migrationBuilder.DropTable(
                name: "bills_of_materials",
                schema: "manufacturing");

            migrationBuilder.DropTable(
                name: "work_orders",
                schema: "manufacturing");
        }
    }
}
