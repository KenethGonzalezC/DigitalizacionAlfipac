using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BitacoraAlfipac.Migrations
{
    /// <inheritdoc />
    public partial class clieninv : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cliente",
                table: "PatioQuimicos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cliente",
                table: "Patio2",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cliente",
                table: "Patio1",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cliente",
                table: "ContenedoresSinAsignarPatio",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cliente",
                table: "ContenedoresBackupDespacho",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cliente",
                table: "Anden2000",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cliente",
                table: "PatioQuimicos");

            migrationBuilder.DropColumn(
                name: "Cliente",
                table: "Patio2");

            migrationBuilder.DropColumn(
                name: "Cliente",
                table: "Patio1");

            migrationBuilder.DropColumn(
                name: "Cliente",
                table: "ContenedoresSinAsignarPatio");

            migrationBuilder.DropColumn(
                name: "Cliente",
                table: "ContenedoresBackupDespacho");

            migrationBuilder.DropColumn(
                name: "Cliente",
                table: "Anden2000");
        }
    }
}
