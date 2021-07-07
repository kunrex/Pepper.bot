using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscordBotDataBase.Dal.Migrations.Migrations
{
    public partial class StoresMutesinDB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModMutes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Reason = table.Column<string>(type: "TEXT", nullable: true),
                    Time = table.Column<string>(type: "TEXT", nullable: true),
                    ModeratorID = table.Column<long>(type: "INTEGER", nullable: false),
                    ModerationProfileId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModMutes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModMutes_ModerationProfiles_ModerationProfileId",
                        column: x => x.ModerationProfileId,
                        principalTable: "ModerationProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModMutes_ModerationProfileId",
                table: "ModMutes",
                column: "ModerationProfileId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModMutes");
        }
    }
}
