using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mars.Admin.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserEmailsToSIDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update existing user emails to SI.co.uk domain
            migrationBuilder.Sql("UPDATE AspNetUsers SET UserName = 'superadmin@SI.co.uk', Email = 'superadmin@SI.co.uk' WHERE UserName = 'superadmin@safelyinsured.co.uk'");
            migrationBuilder.Sql("UPDATE AspNetUsers SET UserName = 'developer@SI.co.uk', Email = 'developer@SI.co.uk' WHERE UserName = 'developer@safelyinsured.co.uk'");
            migrationBuilder.Sql("UPDATE AspNetUsers SET UserName = 'manager@SI.co.uk', Email = 'manager@SI.co.uk' WHERE UserName = 'manager@safelyinsured.co.uk'");
            migrationBuilder.Sql("UPDATE AspNetUsers SET UserName = 'support@SI.co.uk', Email = 'support@SI.co.uk' WHERE UserName = 'support@safelyinsured.co.uk'");
            migrationBuilder.Sql("UPDATE AspNetUsers SET UserName = 'account@SI.co.uk', Email = 'account@SI.co.uk' WHERE UserName = 'account@safelyinsured.co.uk'");
            migrationBuilder.Sql("UPDATE AspNetUsers SET UserName = 'viewer@SI.co.uk', Email = 'viewer@SI.co.uk' WHERE UserName = 'viewer@safelyinsured.co.uk'");
            migrationBuilder.Sql("UPDATE AspNetUsers SET UserName = 'editor@SI.co.uk', Email = 'editor@SI.co.uk' WHERE UserName = 'editor@safelyinsured.co.uk'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert user emails back to safelyinsured.co.uk domain
            migrationBuilder.Sql("UPDATE AspNetUsers SET UserName = 'superadmin@safelyinsured.co.uk', Email = 'superadmin@safelyinsured.co.uk' WHERE UserName = 'superadmin@SI.co.uk'");
            migrationBuilder.Sql("UPDATE AspNetUsers SET UserName = 'developer@safelyinsured.co.uk', Email = 'developer@safelyinsured.co.uk' WHERE UserName = 'developer@SI.co.uk'");
            migrationBuilder.Sql("UPDATE AspNetUsers SET UserName = 'manager@safelyinsured.co.uk', Email = 'manager@safelyinsured.co.uk' WHERE UserName = 'manager@SI.co.uk'");
            migrationBuilder.Sql("UPDATE AspNetUsers SET UserName = 'support@safelyinsured.co.uk', Email = 'support@safelyinsured.co.uk' WHERE UserName = 'support@SI.co.uk'");
            migrationBuilder.Sql("UPDATE AspNetUsers SET UserName = 'account@safelyinsured.co.uk', Email = 'account@safelyinsured.co.uk' WHERE UserName = 'account@SI.co.uk'");
            migrationBuilder.Sql("UPDATE AspNetUsers SET UserName = 'viewer@safelyinsured.co.uk', Email = 'viewer@safelyinsured.co.uk' WHERE UserName = 'viewer@SI.co.uk'");
            migrationBuilder.Sql("UPDATE AspNetUsers SET UserName = 'editor@safelyinsured.co.uk', Email = 'editor@safelyinsured.co.uk' WHERE UserName = 'editor@SI.co.uk'");
        }
    }
}
