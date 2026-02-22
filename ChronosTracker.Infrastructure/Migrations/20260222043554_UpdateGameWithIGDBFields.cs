using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChronosTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGameWithIGDBFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoverUrl",
                table: "Games",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "Games",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IGDBId",
                table: "Games",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Games",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverUrl",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "IGDBId",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Games");
        }
    }
}
