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

            // =========================
            // HEADER
            // =========================
            page.Header().Column(col =>
            {
                col.Item().Text("BITÁCORA DE TEMPERATURAS")
                    .FontSize(20)
                    .Bold()
                    .FontColor(Colors.Blue.Darken2);

                col.Item().Text($"Contenedor: {_c.Contenedor}")
                    .FontSize(12)
                    .SemiBold();

                col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            });

            // =========================
            // CONTENT
            // =========================
            page.Content().Column(col =>
            {
                col.Spacing(15);

                // =========================
                // RESUMEN (CAJA)
                // =========================
                col.Item().Container()
                    .Padding(10)
                    .Border(1)
                    .BorderColor(Colors.Grey.Lighten2)
                    .Column(t =>
                    {
                        t.Spacing(5);

                        string tiempoFormateado = "-";

                        if (_c.FechaHoraConexion != null && _c.FechaHoraDesconexion != null)
                        {
                            var ts = _c.FechaHoraDesconexion.Value - _c.FechaHoraConexion.Value;
                            tiempoFormateado = $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}";
                        }

                        void Row(string label, string value)
                        {
                            t.Item().Row(r =>
                            {
                                r.RelativeItem(3).Text(label).SemiBold();
                                r.RelativeItem(5).Text(value);
                            });
                        }

                        Row("Fecha / Hora Ingreso", _c.FechaHoraIngreso?.ToString("dd/MM/yyyy HH:mm") ?? "-");
                        Row("Fecha / Hora Conexión", _c.FechaHoraConexion?.ToString("dd/MM/yyyy HH:mm") ?? "-");
                        Row("Set Point (°C)", _c.SetPoint?.ToString("F1") ?? "-");
                        Row("Fecha / Hora Despacho", _c.FechaHoraDespacho?.ToString("dd/MM/yyyy HH:mm") ?? "-");
                        Row("Fecha / Hora Desconexión", _c.FechaHoraDesconexion?.ToString("dd/MM/yyyy HH:mm") ?? "-");
                        Row("Horas totales", tiempoFormateado);
                    });

                // =========================
                // TITULO TABLA
                // =========================
                col.Item().Text("REGISTROS DE TEMPERATURA")
                    .Bold()
                    .FontSize(14);

                // =========================
                // TABLA
                // =========================
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3);
                        c.RelativeColumn(2);
                        c.RelativeColumn(5);
                    });

                    // HEADER
                    table.Header(h =>
                    {
                        h.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Fecha / Hora").Bold();
                        h.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Temp (°C)").Bold();
                        h.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Observación").Bold();
                    });

                    int i = 0;

                    foreach (var r in _c.RegistrosTemperatura.OrderBy(x => x.FechaHora))
                    {
                        var bg = i % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;

                        table.Cell().ShowEntire().Background(bg).Padding(5)
                            .Text(r.FechaHora.ToString("dd/MM/yyyy HH:mm"));

                        table.Cell().ShowEntire().Background(bg).Padding(5)
                            .Text(r.Temperatura.ToString("F1"));

                        table.Cell().ShowEntire().Background(bg).Padding(5)
                            .Text(r.Observacion);

                        i++;
                    }
                });
            });

            // =========================
            // FOOTER
            // =========================
            page.Footer()
                .AlignCenter()
                .Text(x =>
                {
                    x.Span("Generado el ").FontSize(9);
                    x.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                        .SemiBold()
                        .FontSize(9);
                });
        });
    }
}