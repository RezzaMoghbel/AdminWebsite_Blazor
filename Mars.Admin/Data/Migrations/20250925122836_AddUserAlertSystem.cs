using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mars.Admin.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAlertSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AttentionCreatedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AttentionIgnoredAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttentionIgnoredBy",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsNewUser",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NeedsAttention",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttentionCreatedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AttentionIgnoredAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AttentionIgnoredBy",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsNewUser",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastLoginAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NeedsAttention",
                table: "AspNetUsers");
        }
    }
}
