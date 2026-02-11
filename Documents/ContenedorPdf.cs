using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using BitacoraAlfipac.Models.Entidades;

namespace BitacoraAlfipac.Documents;
public class ContenedorTemperaturasPdf : IDocument
{
    private readonly ContenedorRefrigerado _c;

    public ContenedorTemperaturasPdf(ContenedorRefrigerado c)
    {
        _c = c;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(30);

            page.Header().Column(col =>
            {
                col.Item().Text("BITÁCORA DE TEMPERATURAS")
                    .FontSize(18).Bold();

                col.Item().Text($"Contenedor: {_c.Contenedor}")
                    .FontSize(12);
            });

            page.Content().Column(col =>
            {
                col.Spacing(10);

                col.Item().Table(t =>
                {
                t.ColumnsDefinition(c =>
                {
                    c.RelativeColumn();
                    c.RelativeColumn();
                });

                void Row(string label, string value)
                {
                    t.Cell().Text(label).Bold();
                    t.Cell().Text(value);
                }

                    string tiempoFormateado = "-";

                    if (_c.FechaHoraConexion != null && _c.FechaHoraDesconexion != null)
                    {
                        var ts = _c.FechaHoraDesconexion.Value - _c.FechaHoraConexion.Value;
                        tiempoFormateado = $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}";
                    }

                    Row("Fecha / Hora Ingreso", _c.FechaHoraIngreso?.ToString("dd/MM/yyyy HH:mm") ?? "-");
                Row("Fecha / Hora Conexión", _c.FechaHoraConexion?.ToString("dd/MM/yyyy HH:mm") ?? "-");
                Row("Set Point (°C)", _c.SetPoint?.ToString() ?? "-");
                Row("Fecha / Hora Despacho", _c.FechaHoraDespacho?.ToString("dd/MM/yyyy HH:mm") ?? "-");
                Row("Fecha / Hora Desconexión", _c.FechaHoraDesconexion?.ToString("dd/MM/yyyy HH:mm") ?? "-");
                Row("Horas totales", tiempoFormateado);
                });

                col.Item().PaddingTop(15).Text("REGISTROS DE TEMPERATURA")
                    .Bold();

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3);
                        c.RelativeColumn(2);
                        c.RelativeColumn(4);
                    });

                    table.Header(h =>
                    {
                        h.Cell().Text("Fecha / Hora").Bold();
                        h.Cell().Text("Temp (°C)").Bold();
                        h.Cell().Text("Observación").Bold();
                    });

                    foreach (var r in _c.RegistrosTemperatura.OrderBy(x => x.FechaHora))
                    {
                        table.Cell().Text(r.FechaHora.ToString("dd/MM/yyyy HH:mm"));
                        table.Cell().Text(r.Temperatura.ToString());
                        table.Cell().Text(r.Observacion);
                    }
                });
            });

            page.Footer()
                .AlignCenter()
                .Text(x =>
                {
                    x.Span("Generado el ");
                    x.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                        .Bold();
                });
        });
    }
}
