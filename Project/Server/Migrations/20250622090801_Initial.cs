using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "User1Name",
                table: "PrivateChats");

            migrationBuilder.DropColumn(
                name: "User2Name",
                table: "PrivateChats");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "User1Name",
                table: "PrivateChats",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "User2Name",
                table: "PrivateChats",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
