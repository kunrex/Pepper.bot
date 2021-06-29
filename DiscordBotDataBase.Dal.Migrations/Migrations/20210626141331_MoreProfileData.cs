using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscordBotDataBase.Dal.Migrations.Migrations
{
    public partial class MoreProfileData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PrevLogDate",
                table: "UserProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrevMonthlyLogDate",
                table: "UserProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrevWeeklyLogDate",
                table: "UserProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrevWorkDate",
                table: "UserProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SafeMode",
                table: "UserProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrevLogDate",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "PrevMonthlyLogDate",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "PrevWeeklyLogDate",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "PrevWorkDate",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "SafeMode",
                table: "UserProfiles");
        }
    }
}
