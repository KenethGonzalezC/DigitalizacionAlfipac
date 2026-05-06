using BitacoraAlfipac.Models.Entidades;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BitacoraAlfipac.Documents;

public class BitacoraIngresosPdf : IDocument
{
    private readonly DateTime _fecha;
    private readonly List<BitacoraIngreso> _ingresos;
    private readonly TimeSpan? _horaInicio;
    private readonly TimeSpan? _horaFin;

    public BitacoraIngresosPdf(
    DateTime fecha,
    List<BitacoraIngreso> ingresos,
    TimeSpan? horaInicio,
    TimeSpan? horaFin)
    {
        _fecha = fecha;
        _horaInicio = horaInicio;
        _horaFin = horaFin;

        // FILTRO AQUÍ (CLAVE)
        if (horaInicio.HasValue && horaFin.HasValue)
        {
            _ingresos = ingresos
                .Where(i =>
                    i.FechaHoraIngreso.TimeOfDay >= horaInicio &&
                    i.FechaHoraIngreso.TimeOfDay <= horaFin)
                .ToList();
        }
        else
        {
            _ingresos = ingresos;
        }
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(25);
            page.DefaultTextStyle(x => x.FontSize(9));

            page.Header().Element(Encabezado);
            page.Content().Element(Contenido);

            page.Footer().AlignCenter().Text(x =>
            {
                x.Span("Generado el ");
                x.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                    .SemiBold();
            });
        });
    }

    void Encabezado(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Text("BITÁCORA DE INGRESOS")
                .FontSize(18)
                .Bold();

            col.Item().Text($"Fecha: {_fecha:dd/MM/yyyy}")
                .FontSize(12);

            // NUEVO: mostrar rango
            if (_horaInicio.HasValue && _horaFin.HasValue)
            {
                col.Item().Text($"Rango: {_horaInicio:hh\\:mm} - {_horaFin:hh\\:mm}")
                    .FontSize(11)
                    .FontColor(Colors.Grey.Darken1);
            }
            else
            {
                col.Item().Text("Rango: Todo el día")
                    .FontSize(11)
                    .FontColor(Colors.Grey.Darken1);
            }

            // EXTRA PRO: total registros
            col.Item().Text($"Total registros: {_ingresos.Count}")
                .FontSize(10)
                .FontColor(Colors.Grey.Darken1);

            col.Item().LineHorizontal(1);
        });
    }

    void Contenido(IContainer container)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(45);
                columns.ConstantColumn(80);
                columns.RelativeColumn(1.0f);
                columns.RelativeColumn(0.7f);
                columns.RelativeColumn(1.2f);
                columns.ConstantColumn(45);
                columns.RelativeColumn();
                columns.ConstantColumn(80);
                columns.ConstantColumn(70);
                columns.RelativeColumn();
            });

            // HEADER
            table.Header(header =>
            {
                HeaderCell(header, "Hora");
                HeaderCell(header, "Contenedor");
                HeaderCell(header, "Marchamos");
                HeaderCell(header, "Transportista");
                HeaderCell(header, "Cliente");
                HeaderCell(header, "Tamaño");
                HeaderCell(header, "Chofer");
                HeaderCell(header, "Placa");
                HeaderCell(header, "Chasis");
                HeaderCell(header, "Viaje / DUA");
            });

            int index = 0;

            // BODY
            foreach (var i in _ingresos)
            {
                var bg = index % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                BodyCell(table, i.FechaHoraIngreso.ToString("HH:mm"), bg);
                BodyCell(table, i.Contenedor, bg);
                BodyCell(table, i.Marchamos, bg);
                BodyCell(table, i.Transportista, bg);
                BodyCell(table, i.Cliente, bg);
                BodyCell(table, i.Tamaño, bg);
                BodyCell(table, i.Chofer, bg);
                BodyCell(table, i.PlacaCabezal, bg);
                BodyCell(table, i.Chasis, bg);
                BodyCell(table, i.ViajeDua, bg);

                index++;
            }
        });
    }

    static void HeaderCell(TableCellDescriptor cell, string text)
    {
        cell.Cell()
            .Background(Colors.Grey.Lighten2)
            .Border(1)
            .BorderColor(Colors.Grey.Lighten1)
            .PaddingVertical(6)
            .PaddingHorizontal(4)
            .AlignCenter()
            .Text(text)
            .SemiBold();
    }

    // MODIFICADO: ahora acepta color alterno
    static void BodyCell(TableDescriptor table, string text, string bg)
    {
        table.Cell()
            .ShowEntire()
            .Background(bg)
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten3)
            .PaddingVertical(4)
            .PaddingHorizontal(4)
            .Text(text ?? "");
    }
}
