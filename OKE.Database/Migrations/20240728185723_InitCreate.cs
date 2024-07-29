using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OKE.Database.Migrations
{
    public partial class InitCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Actors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Actors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Movies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ActorMovie",
                columns: table => new
                {
                    CastId = table.Column<int>(type: "integer", nullable: false),
                    MoviesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActorMovie", x => new { x.CastId, x.MoviesId });
                    table.ForeignKey(
                        name: "FK_ActorMovie_Actors_CastId",
                        column: x => x.CastId,
                        principalTable: "Actors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActorMovie_Movies_MoviesId",
                        column: x => x.MoviesId,
                        principalTable: "Movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Actors",
                columns: new[] { "Id", "FullName" },
                values: new object[,]
                {
                    { -2, "Laurence Fishburne" },
                    { -1, "Keanu Reeves" }
                });

            migrationBuilder.InsertData(
                table: "Movies",
                columns: new[] { "Id", "Description", "Title" },
                values: new object[,]
                {
                    { -3, "Three childhood friends, Darrin, Tre and Ricky, who struggle to cope with the distractions and dangers of growing up in a Los Angeles ghetto.", "Boyz n the Hood" },
                    { -2, "John Wick, a retired hitman, is forced to return to his old ways after a group of Russian gangsters steal his car and kill a puppy gifted to him by his late wife.", "John Wick" },
                    { -1, "The Matrix is a 1999 science fiction action film written and directed by the Wachowskis.", "Matrix" }
                });

            migrationBuilder.InsertData(
                table: "ActorMovie",
                columns: new[] { "CastId", "MoviesId" },
                values: new object[,]
                {
                    { -1, -1 },
                    { -1, -2 },
                    { -2, -1 },
                    { -2, -2 },
                    { -2, -3 },
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActorMovie_MoviesId",
                table: "ActorMovie",
                column: "MoviesId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActorMovie");

            migrationBuilder.DropTable(
                name: "Actors");

            migrationBuilder.DropTable(
                name: "Movies");
        }
    }
}
