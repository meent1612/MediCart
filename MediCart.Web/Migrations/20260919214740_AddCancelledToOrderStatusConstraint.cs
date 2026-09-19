using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediCart.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddCancelledToOrderStatusConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Order_Status",
                table: "Orders");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Order_Status",
                table: "Orders",
                sql: "\"Status\" IN ('Pending','Processing','Shipped','Delivered','Rejected','Cancelled')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Order_Status",
                table: "Orders");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Order_Status",
                table: "Orders",
                sql: "\"Status\" IN ('Pending','Processing','Shipped','Delivered','Rejected')");
        }
    }
}
