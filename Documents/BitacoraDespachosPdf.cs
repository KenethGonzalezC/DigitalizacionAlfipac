using BitacoraAlfipac.Models.Entidades;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BitacoraAlfipac.Documents;

public class BitacoraDespachosPdf : IDocument
{
    private readonly DateTime _fecha;
    private readonly List<BitacoraDespacho> _despachos;
    private readonly TimeSpan? _horaInicio;
    private readonly TimeSpan? _horaFin;

    public BitacoraDespachosPdf(DateTime fecha, List<BitacoraDespacho> despachos,
        TimeSpan? horaInicio = null, TimeSpan? horaFin = null)
    {
        _fecha = fecha;
        _despachos = despachos;
        _horaInicio = horaInicio;
        _horaFin = horaFin;
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
                x.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).SemiBold();
            });
        });
    }

    void Encabezado(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Text("BITÁCORA DE DESPACHOS")
                .FontSize(18)
                .Bold();

            col.Item().Text($"Fecha: {_fecha:dd/MM/yyyy}")
                .FontSize(12);

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

            col.Item().Text($"Total registros: {_despachos.Count}")
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
                columns.ConstantColumn(45);   // Hora
                columns.ConstantColumn(75);   // Contenedor
                columns.RelativeColumn(2);    // Marchamos
                columns.RelativeColumn(1.7f);    // Transportista
                columns.RelativeColumn(2);     // Información
                columns.RelativeColumn();     // Chofer
                columns.ConstantColumn(75);   // Placa
                columns.ConstantColumn(70);   // Chasis
                columns.RelativeColumn();     // Viaje / DUA                
                columns.ConstantColumn(85);   // Cont. Ref.
            });

            // HEADER
            table.Header(header =>
            {
                HeaderCell(header, "Hora");
                HeaderCell(header, "Contenedor");
                HeaderCell(header, "Marchamos");
                HeaderCell(header, "Transportista");
                HeaderCell(header, "Información");
                HeaderCell(header, "Chofer");
                HeaderCell(header, "Placa");
                HeaderCell(header, "Chasis");
                HeaderCell(header, "Viaje / DUA");            
                HeaderCell(header, "Cont. Ref.");
            });

            // BODY
            int index = 0;

            foreach (var d in _despachos)
            {
                var bg = index % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                BodyCell(table, d.FechaHoraDespacho.ToString("HH:mm"), bg);
                BodyCell(table, d.Contenedor, bg);
                BodyCell(table, d.Marchamos, bg);
                BodyCell(table, d.Transportista, bg);
                BodyCell(table, d.Informacion, bg);
                BodyCell(table, d.Chofer, bg);
                BodyCell(table, d.PlacaCabezal, bg);
                BodyCell(table, d.Chasis, bg);
                BodyCell(table, d.ViajeDua, bg);
                BodyCell(table, d.ContenedorReferencia, bg);

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
