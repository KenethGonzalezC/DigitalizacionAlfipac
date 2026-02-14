using BitacoraAlfipac.Models.Entidades;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BitacoraAlfipac.Documents;

public class BitacoraIngresosPdf : IDocument
{
    private readonly DateTime _fecha;
    private readonly List<BitacoraIngreso> _ingresos;

    public BitacoraIngresosPdf(DateTime fecha, List<BitacoraIngreso> ingresos)
    {
        _fecha = fecha;
        _ingresos = ingresos;
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
                columns.ConstantColumn(80);   // Contenedor
                columns.RelativeColumn(1.0f);    // Marchamos     ← quieres achicar esta
                columns.RelativeColumn(0.7f); // Transportista
                columns.RelativeColumn(1.2f); // Cliente       ← quieres agrandar esta un poco
                columns.ConstantColumn(45);   // Tamaño
                columns.RelativeColumn();     // Chofer
                columns.ConstantColumn(80);   // Placa
                columns.ConstantColumn(70);   // Chasis
                columns.RelativeColumn();     // Viaje / DUA
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

            // BODY
            foreach (var i in _ingresos)
            {
                BodyCell(table, i.FechaHoraIngreso.ToString("HH:mm"));
                BodyCell(table, i.Contenedor);
                BodyCell(table, i.Marchamos);
                BodyCell(table, i.Transportista);
                BodyCell(table, i.Cliente);
                BodyCell(table, i.Tamaño);
                BodyCell(table, i.Chofer);
                BodyCell(table, i.PlacaCabezal);
                BodyCell(table, i.Chasis);
                BodyCell(table, i.ViajeDua);
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

    static void BodyCell(TableDescriptor table, string text)
    {
        table.Cell()
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten3)
            .PaddingVertical(4)
            .PaddingHorizontal(4)
            .Text(text ?? "");
    }
}
