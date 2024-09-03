using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlugIn.Dal.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAppProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserLogin",
                table: "AppProject",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserLogin",
                table: "AppProject");
        }
    }
}
