using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BitacoraAlfipac.Migrations
{
    /// <inheritdoc />
    public partial class datamodstart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DatosIngresosViajes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Viaje = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecintoOrigen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacionViaje = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Declarante = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Transportista = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mercancia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaRegistroSistema = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatosIngresosViajes", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DatosIngresosViajes");
        }
    }
}
