using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mars.Admin.Migrations
{
    /// <inheritdoc />
    public partial class FixCorruptedUserWebsiteAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Clean up corrupted UserWebsiteAccess records with NULL or invalid WebsiteId
            migrationBuilder.Sql(@"
                DELETE FROM UserWebsiteAccesses 
                WHERE WebsiteId IS NULL 
                   OR WebsiteId = 0 
                   OR WebsiteId NOT IN (SELECT Id FROM Websites)
            ");

            // Clean up any UserWebsiteAccess records with NULL UserId
            migrationBuilder.Sql(@"
                DELETE FROM UserWebsiteAccesses 
                WHERE UserId IS NULL 
                   OR UserId NOT IN (SELECT Id FROM AspNetUsers)
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No rollback needed for data cleanup
        }
    }
}
