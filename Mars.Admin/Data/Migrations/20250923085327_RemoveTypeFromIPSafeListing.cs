using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mars.Admin.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTypeFromIPSafeListing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_IPSafeListing_Type_UserId",
                table: "IPSafeListings");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "IPSafeListings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "IPSafeListings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddCheckConstraint(
                name: "CK_IPSafeListing_Type_UserId",
                table: "IPSafeListings",
                sql: "(Type = 'Office' AND UserId IS NULL) OR (Type = 'Individual' AND UserId IS NOT NULL)");
        }
    }
}
