using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace O_Dzhumyk_MefistoTheatre.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedUserSubtypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RegisteredAt",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AspNetUsers",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "RegisteredAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }
    }
}
