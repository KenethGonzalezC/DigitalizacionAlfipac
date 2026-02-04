using BitacoraAlfipac.Models.Entidades;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BitacoraAlfipac.Documents;

public class HistorialContenedoresPdf : IDocument
{
    private readonly List<HistorialContenedor> _historial;
    private readonly string? _filtro;

    public HistorialContenedoresPdf(List<HistorialContenedor> historial, string? filtro)
    {
        _historial = historial;
        _filtro = filtro;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(25);
            page.DefaultTextStyle(x => x.FontSize(9));

            page.Header().Element(Header);
            page.Content().Element(Content);
            page.Footer().AlignCenter().Text(x =>
            {
                x.Span("Generado el ");
                x.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).SemiBold();
            });
        });
    }

    void Header(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Text("HISTORIAL DE CONTENEDORES").FontSize(18).Bold();

            if (!string.IsNullOrWhiteSpace(_filtro))
                col.Item().Text($"Filtro: {_filtro}").FontSize(12);

            col.Item().LineHorizontal(1);
        });
    }

    void Content(IContainer container)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(80);   // Contenedor
                columns.ConstantColumn(120);  // Ingreso
                columns.ConstantColumn(120);  // Salida
            });

            table.Header(header =>
            {
                HeaderCell(header, "Contenedor");
                HeaderCell(header, "Ingreso");
                HeaderCell(header, "Salida");
            });

            foreach (var h in _historial)
            {
                BodyCell(table, h.Contenedor);
                BodyCell(table, h.FechaHoraIngreso?.ToString("dd/MM/yyyy HH:mm") ?? "-");
                BodyCell(table, h.FechaHoraSalida?.ToString("dd/MM/yyyy HH:mm") ?? "-");
            }
        });
    }

    static void HeaderCell(TableCellDescriptor cell, string text)
    {
        cell.Cell()
            .Background(Colors.Grey.Lighten2)
            .Border(1)
            .Padding(5)
            .AlignCenter()
            .Text(text).SemiBold();
    }

    static void BodyCell(TableDescriptor table, string text)
    {
        table.Cell()
            .BorderBottom(1)
            .Padding(4)
            .Text(text ?? "");
    }
}
