using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReferralBot.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddSelectedCourseIdToUserState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SelectedCourseId",
                table: "TelegramUserStates",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelectedCourseId",
                table: "TelegramUserStates");
        }
    }
}
