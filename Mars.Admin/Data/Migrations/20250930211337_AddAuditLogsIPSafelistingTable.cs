using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mars.Admin.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogsIPSafelistingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogsIPSafelistings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IPAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RequestPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Referer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AccessAttempts = table.Column<int>(type: "int", nullable: false),
                    FirstAttemptAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastAttemptAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogsIPSafelistings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogsIPSafelistings_IPAddress",
                table: "AuditLogsIPSafelistings",
                column: "IPAddress");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogsIPSafelistings");
        }
    }
}
