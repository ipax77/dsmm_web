using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace sc2dsstats_mm.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MMdbPlayers",
                columns: table => new
                {
                    MMdbPlayerId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    AuthName = table.Column<string>(nullable: true),
                    Mode = table.Column<string>(nullable: true),
                    Server = table.Column<string>(nullable: true),
                    Mode2 = table.Column<string>(nullable: true),
                    Ladder = table.Column<bool>(nullable: false),
                    Credential = table.Column<bool>(nullable: false),
                    Deleted = table.Column<bool>(nullable: false),
                    MMDeleted = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MMdbPlayers", x => x.MMdbPlayerId);
                });

            migrationBuilder.CreateTable(
                name: "MMdbRaces",
                columns: table => new
                {
                    MMdbRaceId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    AuthName = table.Column<string>(nullable: true),
                    Mode = table.Column<string>(nullable: true),
                    Server = table.Column<string>(nullable: true),
                    Mode2 = table.Column<string>(nullable: true),
                    Ladder = table.Column<bool>(nullable: false),
                    Credential = table.Column<bool>(nullable: false),
                    Deleted = table.Column<bool>(nullable: false),
                    MMDeleted = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MMdbRaces", x => x.MMdbRaceId);
                });

            migrationBuilder.CreateTable(
                name: "MMdbRatings",
                columns: table => new
                {
                    MMdbRatingId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Lobby = table.Column<string>(nullable: true),
                    EXP = table.Column<double>(nullable: false),
                    MU = table.Column<double>(nullable: false),
                    SIGMA = table.Column<double>(nullable: false),
                    Games = table.Column<int>(nullable: false),
                    Time = table.Column<DateTime>(nullable: false),
                    MMdbPlayerId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MMdbRatings", x => x.MMdbRatingId);
                    table.ForeignKey(
                        name: "FK_MMdbRatings_MMdbPlayers_MMdbPlayerId",
                        column: x => x.MMdbPlayerId,
                        principalTable: "MMdbPlayers",
                        principalColumn: "MMdbPlayerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MMdbRaceRatings",
                columns: table => new
                {
                    MMdbRaceRatingId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Lobby = table.Column<string>(nullable: true),
                    EXP = table.Column<double>(nullable: false),
                    MU = table.Column<double>(nullable: false),
                    SIGMA = table.Column<double>(nullable: false),
                    Games = table.Column<int>(nullable: false),
                    Time = table.Column<DateTime>(nullable: false),
                    MMdbRaceId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MMdbRaceRatings", x => x.MMdbRaceRatingId);
                    table.ForeignKey(
                        name: "FK_MMdbRaceRatings_MMdbRaces_MMdbRaceId",
                        column: x => x.MMdbRaceId,
                        principalTable: "MMdbRaces",
                        principalColumn: "MMdbRaceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MMdbRaceRatings_MMdbRaceId",
                table: "MMdbRaceRatings",
                column: "MMdbRaceId");

            migrationBuilder.CreateIndex(
                name: "IX_MMdbRatings_MMdbPlayerId",
                table: "MMdbRatings",
                column: "MMdbPlayerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MMdbRaceRatings");

            migrationBuilder.DropTable(
                name: "MMdbRatings");

            migrationBuilder.DropTable(
                name: "MMdbRaces");

            migrationBuilder.DropTable(
                name: "MMdbPlayers");
        }
    }
}
