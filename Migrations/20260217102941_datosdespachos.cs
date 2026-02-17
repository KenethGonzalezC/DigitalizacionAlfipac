using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BitacoraAlfipac.Migrations
{
    /// <inheritdoc />
    public partial class datosdespachos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EstadoCarga",
                table: "DatosDespachosViajes",
                newName: "ViajeDua");

            migrationBuilder.AddColumn<string>(
                name: "Chofer",
                table: "DatosDespachosViajes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlacaCabezal",
                table: "DatosDespachosViajes",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Chofer",
                table: "DatosDespachosViajes");

            migrationBuilder.DropColumn(
                name: "PlacaCabezal",
                table: "DatosDespachosViajes");

            migrationBuilder.RenameColumn(
                name: "ViajeDua",
                table: "DatosDespachosViajes",
                newName: "EstadoCarga");
        }
    }
}
