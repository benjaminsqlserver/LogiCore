using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogiCore.Server.Migrations
{
    /// <inheritdoc />
    public partial class Start : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Shipments",
                columns: table => new
                {
                    ShipmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrackingNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SenderName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SenderEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SenderPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RecipientName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    RecipientEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RecipientPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OriginAddress = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    OriginCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginState = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OriginPostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OriginCountry = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "US"),
                    DestinationAddress = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    DestinationCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DestinationState = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DestinationPostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DestinationCountry = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "US"),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ServiceType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TotalWeight = table.Column<decimal>(type: "decimal(10,3)", nullable: false),
                    DeclaredValue = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    PackageCount = table.Column<int>(type: "int", nullable: false),
                    Length = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    Width = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    Height = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    PickupDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstimatedDelivery = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualDelivery = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SignedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ProofOfDeliveryUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ShippingCost = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    IsBilled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shipments", x => x.ShipmentId);
                });

            migrationBuilder.CreateTable(
                name: "Packages",
                columns: table => new
                {
                    PackageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShipmentId = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(10,3)", nullable: false),
                    Length = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    Width = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    Height = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DeclaredValue = table.Column<decimal>(type: "decimal(12,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packages", x => x.PackageId);
                    table.ForeignKey(
                        name: "FK_Packages_Shipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "Shipments",
                        principalColumn: "ShipmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShipmentEvents",
                columns: table => new
                {
                    EventId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShipmentId = table.Column<int>(type: "int", nullable: false),
                    TrackingNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    OperatorName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipmentEvents", x => x.EventId);
                    table.ForeignKey(
                        name: "FK_ShipmentEvents_Shipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "Shipments",
                        principalColumn: "ShipmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Packages_ShipmentId",
                table: "Packages",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentEvents_ShipmentId",
                table: "ShipmentEvents",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentEvents_Timestamp",
                table: "ShipmentEvents",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentEvents_TrackingNumber",
                table: "ShipmentEvents",
                column: "TrackingNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_CreatedAt",
                table: "Shipments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_PickupDate",
                table: "Shipments",
                column: "PickupDate");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_SenderName_RecipientName",
                table: "Shipments",
                columns: new[] { "SenderName", "RecipientName" });

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_ServiceType",
                table: "Shipments",
                column: "ServiceType");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_Status",
                table: "Shipments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_TrackingNumber",
                table: "Shipments",
                column: "TrackingNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Packages");

            migrationBuilder.DropTable(
                name: "ShipmentEvents");

            migrationBuilder.DropTable(
                name: "Shipments");
        }
    }
}
