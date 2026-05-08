using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BitacoraAlfipac.Migrations
{
    /// <inheritdoc />
    public partial class AddClienteToReefer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cliente",
                table: "ContenedoresRefrigerados",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cliente",
                table: "ContenedoresRefrigerados");
        }
    }
}
