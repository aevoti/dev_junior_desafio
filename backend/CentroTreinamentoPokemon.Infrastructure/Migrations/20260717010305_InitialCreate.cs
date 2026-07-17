using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentroTreinamentoPokemon.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlanoTreinamento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: false),
                    ValorMensal = table.Column<decimal>(type: "DECIMAL(10,2)", nullable: false),
                    Descricao = table.Column<string>(type: "VARCHAR(300)", maxLength: 300, nullable: false),
                    NivelPlano = table.Column<int>(type: "INT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanoTreinamento", x => x.Id);
                    table.CheckConstraint("CK_PlanoTreinamento_NivelPlano", "[NivelPlano] BETWEEN 1 AND 3");
                    table.CheckConstraint("CK_PlanoTreinamento_ValorMensal", "[ValorMensal] > 0");
                });

            migrationBuilder.CreateTable(
                name: "Treinador",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "VARCHAR(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "VARCHAR(254)", maxLength: 254, nullable: false),
                    CidadeOrigem = table.Column<string>(type: "VARCHAR(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Treinador", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pokemon",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: false),
                    Tipo = table.Column<int>(type: "INT", nullable: false),
                    Nivel = table.Column<int>(type: "INT", nullable: false),
                    TreinadorId = table.Column<int>(type: "INT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pokemon", x => x.Id);
                    table.CheckConstraint("CK_Pokemon_Nivel", "[Nivel] BETWEEN 1 AND 100");
                    table.ForeignKey(
                        name: "FK_Pokemon_Treinador_TreinadorId",
                        column: x => x.TreinadorId,
                        principalTable: "Treinador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Matricula",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PokemonId = table.Column<int>(type: "INT", nullable: false),
                    PlanoTreinamentoId = table.Column<int>(type: "INT", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "DATETIME2", nullable: false),
                    DataEncerramento = table.Column<DateTime>(type: "DATETIME2", nullable: true),
                    Status = table.Column<int>(type: "INT", nullable: false),
                    ValorMensal = table.Column<decimal>(type: "DECIMAL(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matricula", x => x.Id);
                    table.CheckConstraint("CK_Matricula_Status", "[Status] IN (1, 2, 3)");
                    table.CheckConstraint("CK_Matricula_ValorMensal", "[ValorMensal] > 0");
                    table.ForeignKey(
                        name: "FK_Matricula_PlanoTreinamento_PlanoTreinamentoId",
                        column: x => x.PlanoTreinamentoId,
                        principalTable: "PlanoTreinamento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Matricula_Pokemon_PokemonId",
                        column: x => x.PokemonId,
                        principalTable: "Pokemon",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Matricula_PlanoTreinamentoId",
                table: "Matricula",
                column: "PlanoTreinamentoId");

            migrationBuilder.CreateIndex(
                name: "UX_Matricula_Pokemon_Ativa",
                table: "Matricula",
                column: "PokemonId",
                unique: true,
                filter: "[Status] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_PlanoTreinamento_NivelPlano",
                table: "PlanoTreinamento",
                column: "NivelPlano",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanoTreinamento_Nome",
                table: "PlanoTreinamento",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pokemon_TreinadorId",
                table: "Pokemon",
                column: "TreinadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Treinador_Email",
                table: "Treinador",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Matricula");

            migrationBuilder.DropTable(
                name: "PlanoTreinamento");

            migrationBuilder.DropTable(
                name: "Pokemon");

            migrationBuilder.DropTable(
                name: "Treinador");
        }
    }
}
