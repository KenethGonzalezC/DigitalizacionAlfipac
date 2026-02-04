using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BitacoraAlfipac.Migrations
{
    /// <inheritdoc />
    public partial class CorreciondePAByActas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActasPermanenciaAplicaciones");

            migrationBuilder.AddColumn<bool>(
                name: "Aplicada",
                table: "ActasPermanencias",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AplicadaCorrectamente",
                table: "ActasPermanencias",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaHoraIngresoContenedor",
                table: "ActasPermanencias",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Aplicada",
                table: "ActasPermanencias");

            migrationBuilder.DropColumn(
                name: "AplicadaCorrectamente",
                table: "ActasPermanencias");

            migrationBuilder.DropColumn(
                name: "FechaHoraIngresoContenedor",
                table: "ActasPermanencias");

            migrationBuilder.CreateTable(
                name: "ActasPermanenciaAplicaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActaPermanenciaId = table.Column<int>(type: "int", nullable: false),
                    AplicadoCorrectamente = table.Column<bool>(type: "bit", nullable: false),
                    FechaHoraIngresoContenedor = table.Column<DateTime>(type: "datetime2", nullable: false)
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
    }
}
