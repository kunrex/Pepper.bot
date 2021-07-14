using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscordBotDataBase.Dal.Migrations.Migrations
{
    public partial class MoreDataYay : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AllowNSFW",
                table: "ServerProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "DJRoleId",
                table: "ServerProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "RestrictPermissionsToAdmin",
                table: "ServerProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UseDJRoleEnforcement",
                table: "ServerProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "GuildID",
                table: "ModMutes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "GuildID",
                table: "ModKicks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "GuildID",
                table: "ModInfractions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "GuildID",
                table: "ModEndorsements",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "GuildID",
                table: "ModBans",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowNSFW",
                table: "ServerProfiles");

            migrationBuilder.DropColumn(
                name: "DJRoleId",
                table: "ServerProfiles");

            migrationBuilder.DropColumn(
                name: "RestrictPermissionsToAdmin",
                table: "ServerProfiles");

            migrationBuilder.DropColumn(
                name: "UseDJRoleEnforcement",
                table: "ServerProfiles");

            migrationBuilder.DropColumn(
                name: "GuildID",
                table: "ModMutes");

            migrationBuilder.DropColumn(
                name: "GuildID",
                table: "ModKicks");

            migrationBuilder.DropColumn(
                name: "GuildID",
                table: "ModInfractions");

            migrationBuilder.DropColumn(
                name: "GuildID",
                table: "ModEndorsements");

            migrationBuilder.DropColumn(
                name: "GuildID",
                table: "ModBans");
        }
    }
}
