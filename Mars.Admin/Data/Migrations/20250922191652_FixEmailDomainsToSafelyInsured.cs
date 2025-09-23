using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mars.Admin.Migrations
{
    /// <inheritdoc />
    public partial class FixEmailDomainsToSafelyInsured : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix email domains from @SI.co.uk back to @safelyinsured.co.uk
            migrationBuilder.Sql(@"
                UPDATE AspNetUsers 
                SET UserName = REPLACE(UserName, '@SI.co.uk', '@safelyinsured.co.uk'),
                    Email = REPLACE(Email, '@SI.co.uk', '@safelyinsured.co.uk')
                WHERE UserName LIKE '%@SI.co.uk' OR Email LIKE '%@SI.co.uk'
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert email domains back to @SI.co.uk (if needed)
            migrationBuilder.Sql(@"
                UPDATE AspNetUsers 
                SET UserName = REPLACE(UserName, '@safelyinsured.co.uk', '@SI.co.uk'),
                    Email = REPLACE(Email, '@safelyinsured.co.uk', '@SI.co.uk')
                WHERE UserName LIKE '%@safelyinsured.co.uk' OR Email LIKE '%@safelyinsured.co.uk'
            ");
        }
    }
}
