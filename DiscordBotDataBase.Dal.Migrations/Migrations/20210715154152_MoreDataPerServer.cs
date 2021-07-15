using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscordBotDataBase.Dal.Migrations.Migrations
{
    public partial class MoreDataPerServer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "LogChannel",
                table: "ServerProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "LogErrors",
                table: "ServerProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LogNewMembers",
                table: "ServerProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "ModeratorRoleId",
                table: "ServerProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "RulesChannelId",
                table: "ServerProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogChannel",
                table: "ServerProfiles");

            migrationBuilder.DropColumn(
                name: "LogErrors",
                table: "ServerProfiles");

            migrationBuilder.DropColumn(
                name: "LogNewMembers",
                table: "ServerProfiles");

            migrationBuilder.DropColumn(
                name: "ModeratorRoleId",
                table: "ServerProfiles");

            migrationBuilder.DropColumn(
                name: "RulesChannelId",
                table: "ServerProfiles");
        }
    }
}
