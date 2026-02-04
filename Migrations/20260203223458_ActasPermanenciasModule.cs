using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BitacoraAlfipac.Migrations
{
    /// <inheritdoc />
    public partial class ActasPermanenciasModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActasPermanencias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Contenedor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Viaje = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cliente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Detalles = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActasPermanencias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ActasPermanenciaAplicaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActaPermanenciaId = table.Column<int>(type: "int", nullable: false),
                    FechaHoraIngresoContenedor = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AplicadoCorrectamente = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActasPermanenciaAplicaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActasPermanenciaAplicaciones_ActasPermanencias_ActaPermanenciaId",
                        column: x => x.ActaPermanenciaId,
                        principalTable: "ActasPermanencias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActasPermanenciaAplicaciones_ActaPermanenciaId",
                table: "ActasPermanenciaAplicaciones",
                column: "ActaPermanenciaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActasPermanenciaAplicaciones");

            migrationBuilder.DropTable(
                name: "ActasPermanencias");
        }
    }
}
