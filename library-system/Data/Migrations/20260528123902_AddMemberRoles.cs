using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace library_system.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Members",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "User");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Members_Role_Allowed",
                table: "Members",
                sql: "[Role] IN ('User', 'Admin')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Members_Role_Allowed",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Members");
        }
    }
}
