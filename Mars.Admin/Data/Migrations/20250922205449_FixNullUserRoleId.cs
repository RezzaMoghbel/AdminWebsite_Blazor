using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mars.Admin.Migrations
{
    /// <inheritdoc />
    public partial class FixNullUserRoleId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix NULL UserRoleId values by setting them to the SuperAdmin role (ID = 1)
            migrationBuilder.Sql(@"
                UPDATE AspNetUsers 
                SET UserRoleId = 1 
                WHERE UserRoleId IS NULL
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
