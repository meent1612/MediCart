using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediCart.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddActionTypeToAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActionType",
                table: "AuditLogs",
                type: "text",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE ""AuditLogs"" SET ""ActionType"" = 'MarkedDelivered' WHERE ""Action"" ILIKE '%Delivered%';
                UPDATE ""AuditLogs"" SET ""ActionType"" = 'MarkedShipped' WHERE ""Action"" ILIKE '%Shipped%';
                UPDATE ""AuditLogs"" SET ""ActionType"" = 'Approved' WHERE ""Action"" ILIKE '%Approved%';
                UPDATE ""AuditLogs"" SET ""ActionType"" = 'Rejected' WHERE ""Action"" ILIKE '%Rejected%';
                UPDATE ""AuditLogs"" SET ""ActionType"" = 'MarkedRead' WHERE ""Action"" ILIKE '%marked contact message%as read%';
                UPDATE ""AuditLogs"" SET ""ActionType"" = 'MarkedUnread' WHERE ""Action"" ILIKE '%marked contact message%as unread%';
                UPDATE ""AuditLogs"" SET ""ActionType"" = 'Add' WHERE (""Action"" ILIKE '%add%' OR ""Action"" ILIKE '%created%') AND ""ActionType"" IS NULL;
                UPDATE ""AuditLogs"" SET ""ActionType"" = 'Delete' WHERE (""Action"" ILIKE '%delete%' OR ""Action"" ILIKE '%removed%') AND ""ActionType"" IS NULL;
                UPDATE ""AuditLogs"" SET ""ActionType"" = 'Security' WHERE (""Action"" ILIKE '%login%' OR ""Action"" ILIKE '%password%' OR ""Action"" ILIKE '%role%') AND ""ActionType"" IS NULL;
                UPDATE ""AuditLogs"" SET ""ActionType"" = 'Edit' WHERE ""ActionType"" IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionType",
                table: "AuditLogs");
        }
    }
}
