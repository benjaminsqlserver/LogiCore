using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogiCore.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Shipments",
                columns: table => new
                {
                    ShipmentId            = table.Column<int>(type: "int", nullable: false)
                                              .Annotation("SqlServer:Identity", "1, 1"),
                    TrackingNumber        = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),

                    // Sender
                    SenderName            = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SenderEmail           = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, defaultValue: ""),
                    SenderPhone           = table.Column<string>(type: "nvarchar(50)",  maxLength: 50,  nullable: false, defaultValue: ""),

                    // Recipient
                    RecipientName         = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    RecipientEmail        = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, defaultValue: ""),
                    RecipientPhone        = table.Column<string>(type: "nvarchar(50)",  maxLength: 50,  nullable: false, defaultValue: ""),

                    // Origin
                    OriginAddress         = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    OriginCity            = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginState           = table.Column<string>(type: "nvarchar(50)",  maxLength: 50,  nullable: false, defaultValue: ""),
                    OriginPostalCode      = table.Column<string>(type: "nvarchar(20)",  maxLength: 20,  nullable: false),
                    OriginCountry         = table.Column<string>(type: "nvarchar(3)",   maxLength: 3,   nullable: false, defaultValue: "US"),

                    // Destination
                    DestinationAddress    = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    DestinationCity       = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DestinationState      = table.Column<string>(type: "nvarchar(50)",  maxLength: 50,  nullable: false, defaultValue: ""),
                    DestinationPostalCode = table.Column<string>(type: "nvarchar(20)",  maxLength: 20,  nullable: false),
                    DestinationCountry    = table.Column<string>(type: "nvarchar(3)",   maxLength: 3,   nullable: false, defaultValue: "US"),

                    // Enums (stored as string)
                    Status                = table.Column<string>(type: "nvarchar(30)",  maxLength: 30,  nullable: false, defaultValue: "Pending"),
                    ServiceType           = table.Column<string>(type: "nvarchar(30)",  maxLength: 30,  nullable: false, defaultValue: "Ground"),

                    // Measurements
                    TotalWeight           = table.Column<decimal>(type: "decimal(10,3)", nullable: false, defaultValue: 0m),
                    DeclaredValue         = table.Column<decimal>(type: "decimal(12,2)", nullable: false, defaultValue: 0m),
                    PackageCount          = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Length                = table.Column<decimal>(type: "decimal(8,2)", nullable: false, defaultValue: 0m),
                    Width                 = table.Column<decimal>(type: "decimal(8,2)", nullable: false, defaultValue: 0m),
                    Height                = table.Column<decimal>(type: "decimal(8,2)", nullable: false, defaultValue: 0m),

                    // Dates
                    PickupDate            = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstimatedDelivery     = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualDelivery        = table.Column<DateTime>(type: "datetime2", nullable: true),

                    // Delivery info
                    SignedBy              = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ProofOfDeliveryUrl    = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes                 = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),

                    // Billing
                    ShippingCost          = table.Column<decimal>(type: "decimal(12,2)", nullable: false, defaultValue: 0m),
                    IsBilled              = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),

                    // Audit
                    CreatedAt             = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt             = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsDeleted             = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shipments", x => x.ShipmentId);
                });

            migrationBuilder.CreateTable(
                name: "Packages",
                columns: table => new
                {
                    PackageId    = table.Column<int>(type: "int", nullable: false)
                                     .Annotation("SqlServer:Identity", "1, 1"),
                    ShipmentId   = table.Column<int>(type: "int", nullable: false),
                    Weight       = table.Column<decimal>(type: "decimal(10,3)", nullable: false, defaultValue: 0m),
                    Length       = table.Column<decimal>(type: "decimal(8,2)",  nullable: false, defaultValue: 0m),
                    Width        = table.Column<decimal>(type: "decimal(8,2)",  nullable: false, defaultValue: 0m),
                    Height       = table.Column<decimal>(type: "decimal(8,2)",  nullable: false, defaultValue: 0m),
                    Description  = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false, defaultValue: ""),
                    DeclaredValue = table.Column<decimal>(type: "decimal(12,2)", nullable: false, defaultValue: 0m)
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
                    EventId        = table.Column<int>(type: "int", nullable: false)
                                       .Annotation("SqlServer:Identity", "1, 1"),
                    ShipmentId     = table.Column<int>(type: "int", nullable: false),
                    TrackingNumber = table.Column<string>(type: "nvarchar(30)",  maxLength: 30,   nullable: false),
                    EventType      = table.Column<string>(type: "nvarchar(40)",  maxLength: 40,   nullable: false),
                    Location       = table.Column<string>(type: "nvarchar(200)", maxLength: 200,  nullable: false, defaultValue: ""),
                    Notes          = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false, defaultValue: ""),
                    OperatorName   = table.Column<string>(type: "nvarchar(150)", maxLength: 150,  nullable: false, defaultValue: ""),
                    Timestamp      = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
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

            // ---- Indexes ----

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_TrackingNumber",
                table: "Shipments",
                column: "TrackingNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_Status",
                table: "Shipments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_ServiceType",
                table: "Shipments",
                column: "ServiceType");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_PickupDate",
                table: "Shipments",
                column: "PickupDate");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_CreatedAt",
                table: "Shipments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_SenderName_RecipientName",
                table: "Shipments",
                columns: new[] { "SenderName", "RecipientName" });

            migrationBuilder.CreateIndex(
                name: "IX_Packages_ShipmentId",
                table: "Packages",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentEvents_ShipmentId",
                table: "ShipmentEvents",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentEvents_TrackingNumber",
                table: "ShipmentEvents",
                column: "TrackingNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentEvents_Timestamp",
                table: "ShipmentEvents",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ShipmentEvents");
            migrationBuilder.DropTable(name: "Packages");
            migrationBuilder.DropTable(name: "Shipments");
        }
    }
}
