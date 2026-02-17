using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BitacoraAlfipac.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
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
                    Detalle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Viaje = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cliente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaHoraIngresoContenedor = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AplicadoCorrectamente = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActasPermanencias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Anden2000",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Contenedor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Marchamos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tamano = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Transportista = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cliente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Chasis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstadoCarga = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ubicacion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anden2000", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BitacoraDespachos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Contenedor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContenedorReferencia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EsSalidaEnFurgon = table.Column<bool>(type: "bit", nullable: false),
                    Marchamos = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaHoraDespacho = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Transportista = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Informacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Chofer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlacaCabezal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Chasis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ViajeDua = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BitacoraDespachos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BitacoraIngresos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Contenedor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Marchamos = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaHoraIngreso = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Transportista = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cliente = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tamaño = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Chofer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlacaCabezal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Chasis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ViajeDua = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BitacoraIngresos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContenedoresBackupDespacho",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Contenedor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PatioOrigen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Marchamos = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tamaño = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Transportista = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cliente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Chasis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaRespaldo = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContenedoresBackupDespacho", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContenedoresRefrigerados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Contenedor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaHoraIngreso = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaHoraConexion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SetPoint = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FechaHoraDespacho = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaHoraDesconexion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContenedoresRefrigerados", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContenedoresSinAsignarPatio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Contenedor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Marchamos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tamano = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Transportista = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cliente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Chasis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstadoCarga = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ubicacion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContenedoresSinAsignarPatio", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DatosIngresosViajes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Viaje = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Contenedor = table.Column<string>(type: "nvarchar(450)", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "HistorialContenedores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Contenedor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaHoraIngreso = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaHoraSalida = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialContenedores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PabMercanciasSusceptibles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PabMercanciasSusceptibles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Patio1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Contenedor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Marchamos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tamano = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Transportista = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cliente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Chasis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstadoCarga = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ubicacion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patio1", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Patio2",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Contenedor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Marchamos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tamano = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Transportista = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cliente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Chasis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstadoCarga = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ubicacion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patio2", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PatioQuimicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Contenedor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Marchamos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tamano = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Transportista = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cliente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Chasis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstadoCarga = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ubicacion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatioQuimicos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransportistasAutorizados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CedulaJuridica = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransportistasAutorizados", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreUsuario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegistrosTemperatura",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Temperatura = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContenedorRefrigeradoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosTemperatura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrosTemperatura_ContenedoresRefrigerados_ContenedorRefrigeradoId",
                        column: x => x.ContenedorRefrigeradoId,
                        principalTable: "ContenedoresRefrigerados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DatosIngresosViajes_Contenedor",
                table: "DatosIngresosViajes",
                column: "Contenedor",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosTemperatura_ContenedorRefrigeradoId",
                table: "RegistrosTemperatura",
                column: "ContenedorRefrigeradoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActasPermanencias");

            migrationBuilder.DropTable(
                name: "Anden2000");

            migrationBuilder.DropTable(
                name: "BitacoraDespachos");

            migrationBuilder.DropTable(
                name: "BitacoraIngresos");

            migrationBuilder.DropTable(
                name: "ContenedoresBackupDespacho");

            migrationBuilder.DropTable(
                name: "ContenedoresSinAsignarPatio");

            migrationBuilder.DropTable(
                name: "DatosIngresosViajes");

            migrationBuilder.DropTable(
                name: "HistorialContenedores");

            migrationBuilder.DropTable(
                name: "PabMercanciasSusceptibles");

            migrationBuilder.DropTable(
                name: "Patio1");

            migrationBuilder.DropTable(
                name: "Patio2");

            migrationBuilder.DropTable(
                name: "PatioQuimicos");

            migrationBuilder.DropTable(
                name: "RegistrosTemperatura");

            migrationBuilder.DropTable(
                name: "TransportistasAutorizados");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "ContenedoresRefrigerados");
        }
    }
}
