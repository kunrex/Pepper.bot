using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscordBotDataBase.Dal.Migrations.Migrations
{
    public partial class AddedModerationData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModerationProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DiscordId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModBans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Reason = table.Column<string>(type: "TEXT", nullable: true),
                    Time = table.Column<string>(type: "TEXT", nullable: true),
                    ModerationProfileId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModBans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModBans_ModerationProfiles_ModerationProfileId",
                        column: x => x.ModerationProfileId,
                        principalTable: "ModerationProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ModEndorsements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Reason = table.Column<string>(type: "TEXT", nullable: true),
                    ModerationProfileId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModEndorsements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModEndorsements_ModerationProfiles_ModerationProfileId",
                        column: x => x.ModerationProfileId,
                        principalTable: "ModerationProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ModInfractions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Reason = table.Column<string>(type: "TEXT", nullable: true),
                    ModerationProfileId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModInfractions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModInfractions_ModerationProfiles_ModerationProfileId",
                        column: x => x.ModerationProfileId,
                        principalTable: "ModerationProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ModKicks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Reason = table.Column<string>(type: "TEXT", nullable: true),
                    ModerationProfileId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModKicks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModKicks_ModerationProfiles_ModerationProfileId",
                        column: x => x.ModerationProfileId,
                        principalTable: "ModerationProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModBans_ModerationProfileId",
                table: "ModBans",
                column: "ModerationProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ModEndorsements_ModerationProfileId",
                table: "ModEndorsements",
                column: "ModerationProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ModInfractions_ModerationProfileId",
                table: "ModInfractions",
                column: "ModerationProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ModKicks_ModerationProfileId",
                table: "ModKicks",
                column: "ModerationProfileId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModBans");

            migrationBuilder.DropTable(
                name: "ModEndorsements");

            migrationBuilder.DropTable(
                name: "ModInfractions");

            migrationBuilder.DropTable(
                name: "ModKicks");

            migrationBuilder.DropTable(
                name: "ModerationProfiles");
        }
    }
}
