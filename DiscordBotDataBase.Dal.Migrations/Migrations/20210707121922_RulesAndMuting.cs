using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscordBotDataBase.Dal.Migrations.Migrations
{
    public partial class RulesAndMuting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServerProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GuildId = table.Column<long>(type: "INTEGER", nullable: false),
                    MutedRoleId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RuleContent = table.Column<string>(type: "TEXT", nullable: true),
                    ServerProfileId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServerRules_ServerProfiles_ServerProfileId",
                        column: x => x.ServerProfileId,
                        principalTable: "ServerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServerRules_ServerProfileId",
                table: "ServerRules",
                column: "ServerProfileId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServerRules");

            migrationBuilder.DropTable(
                name: "ServerProfiles");
        }
    }
}
