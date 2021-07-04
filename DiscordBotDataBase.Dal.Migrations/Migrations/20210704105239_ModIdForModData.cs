using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscordBotDataBase.Dal.Migrations.Migrations
{
    public partial class ModIdForModData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ModeratorID",
                table: "ModKicks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "ModeratorID",
                table: "ModInfractions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "ModeratorID",
                table: "ModEndorsements",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "ModeratorID",
                table: "ModBans",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModeratorID",
                table: "ModKicks");

            migrationBuilder.DropColumn(
                name: "ModeratorID",
                table: "ModInfractions");

            migrationBuilder.DropColumn(
                name: "ModeratorID",
                table: "ModEndorsements");

            migrationBuilder.DropColumn(
                name: "ModeratorID",
                table: "ModBans");
        }
    }
}
