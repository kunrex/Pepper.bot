using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscordBotDataBase.Dal.Migrations.Migrations
{
    public partial class AddedStartTimeToMute : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StartTime",
                table: "ModMutes",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "ModMutes");
        }
    }
}
