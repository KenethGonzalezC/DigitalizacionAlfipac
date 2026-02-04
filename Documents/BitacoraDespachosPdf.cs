using BitacoraAlfipac.Models.Entidades;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BitacoraAlfipac.Documents;

public class BitacoraDespachosPdf : IDocument
{
    private readonly DateTime _fecha;
    private readonly List<BitacoraDespacho> _despachos;

    public BitacoraDespachosPdf(DateTime fecha, List<BitacoraDespacho> despachos)
    {
        _fecha = fecha;
        _despachos = despachos;
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
            foreach (var d in _despachos)
            {
                BodyCell(table, d.FechaHoraDespacho.ToString("HH:mm"));
                BodyCell(table, d.Contenedor);
                BodyCell(table, d.Marchamos);
                BodyCell(table, d.Transportista);
                BodyCell(table, d.Informacion);
                BodyCell(table, d.Chofer);
                BodyCell(table, d.PlacaCabezal);
                BodyCell(table, d.Chasis);
                BodyCell(table, d.ViajeDua);                
                BodyCell(table, d.ContenedorReferencia);
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
