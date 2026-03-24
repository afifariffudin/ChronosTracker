using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChronosTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSteamCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastMetadataUpdate",
                table: "Games",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SteamPositiveReviews",
                table: "Games",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SteamTotalReviews",
                table: "Games",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastMetadataUpdate",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "SteamPositiveReviews",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "SteamTotalReviews",
                table: "Games");
        }
    }
}
