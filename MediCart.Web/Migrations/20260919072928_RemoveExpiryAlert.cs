using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MediCart.Web.Migrations
{
    /// <inheritdoc />
    public partial class RemoveExpiryAlert : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExpiryAlerts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExpiryAlerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MedicineId = table.Column<int>(type: "integer", nullable: false),
                    StockId = table.Column<int>(type: "integer", nullable: false),
                    AlertDate = table.Column<DateOnly>(type: "date", nullable: false),
                    AlertLevel = table.Column<string>(type: "text", nullable: false),
                    IsResolved = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpiryAlerts", x => x.Id);
                    table.CheckConstraint("CK_ExpiryAlert_AlertLevel", "\"AlertLevel\" IN ('warning','critical')");
                    table.ForeignKey(
                        name: "FK_ExpiryAlerts_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExpiryAlerts_Stocks_StockId",
                        column: x => x.StockId,
                        principalTable: "Stocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExpiryAlerts_MedicineId",
                table: "ExpiryAlerts",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpiryAlerts_StockId",
                table: "ExpiryAlerts",
                column: "StockId");
        }
    }
}
