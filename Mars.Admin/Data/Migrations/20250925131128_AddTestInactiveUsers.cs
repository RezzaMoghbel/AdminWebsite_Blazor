using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mars.Admin.Migrations
{
    /// <inheritdoc />
    public partial class AddTestInactiveUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add test user with 32 days of inactivity
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount", "IsActive", "IsDeleted", "UserRoleId", "LastLoginAt", "IsNewUser", "NeedsAttention", "AttentionCreatedAt", "AttentionIgnoredAt", "AttentionIgnoredBy" },
                values: new object[] { "test-user-32-days", "testuser32@example.com", "TESTUSER32@EXAMPLE.COM", "testuser32@example.com", "TESTUSER32@EXAMPLE.COM", true, "AQAAAAIAAYagAAAAEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=", "TEST32", Guid.NewGuid().ToString(), null, false, false, null, true, 0, true, false, null, DateTime.UtcNow.AddDays(-32), false, false, null, null, null });

            // Add test user with 91 days of inactivity
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount", "IsActive", "IsDeleted", "UserRoleId", "LastLoginAt", "IsNewUser", "NeedsAttention", "AttentionCreatedAt", "AttentionIgnoredAt", "AttentionIgnoredBy" },
                values: new object[] { "test-user-91-days", "testuser91@example.com", "TESTUSER91@EXAMPLE.COM", "testuser91@example.com", "TESTUSER91@EXAMPLE.COM", true, "AQAAAAIAAYagAAAAEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=", "TEST91", Guid.NewGuid().ToString(), null, false, false, null, true, 0, true, false, null, DateTime.UtcNow.AddDays(-91), false, false, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove test users
            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "test-user-32-days");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "test-user-91-days");
        }
    }
}
