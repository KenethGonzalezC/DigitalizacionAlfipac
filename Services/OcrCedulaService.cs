using BitacoraAlfipac.Models.ViewModels;
using System.Text.RegularExpressions;
using Tesseract;

namespace BitacoraAlfipac.Services;

public class OcrCedulaService
{
    public OcrCedulaResult LeerCedula(string rutaImagen)
    {
        try
        {
            string tessDataPath =
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "tessdata"
                );

            using var engine =
                new TesseractEngine(
                    tessDataPath,
                    "spa",
                    EngineMode.Default
                );

            using var image =
                Pix.LoadFromFile(
                    rutaImagen
                );

            using var page =
                engine.Process(
                    image
                );

            string texto =
                page.GetText() ?? string.Empty;

            texto = texto
                .Replace("\r", " ")
                .Replace("\n", " ");

            texto = Regex.Replace(
                texto,
                @"\s+",
                " "
            );

            string? cedula =
                ExtraerCedula(texto);

            string? nombre =
                ExtraerNombre(texto);

            return new OcrCedulaResult
            {
                Success = true,
                Cedula = cedula,
                NombreCompleto = nombre,
                TextoDetectado = texto,
                Mensaje = "OCR ejecutado correctamente"
            };
        }
        catch (Exception ex)
        {
            return new OcrCedulaResult
            {
                Success = false,
                Mensaje = ex.Message
            };
        }
    }

    private string? ExtraerCedula(string texto)
    {
        var match =
            Regex.Match(
                texto,
                @"\b\d\s\d{4}\s\d{4}\b"
            );

        if (match.Success)
        {
            return match.Value
                .Replace(" ", "-");
        }

        return null;
    }

    private string? ExtraerNombre(
        string texto)
    {
        var lineas =
            texto.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries
            );

        var ignorar =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase)
                {
                    "REPUBLICA",
                    "COSTA",
                    "RICA",
                    "TRIBUNAL",
                    "SUPREMO",
                    "ELECCIONES",
                    "CEDULA",
                    "IDENTIDAD",
                    "DOCUMENTO",
                    "NACIONAL",
                    "SEXO",
                    "FECHA",
                    "NACIMIENTO"
                };

        var palabras =
            lineas
            .Where(x =>
                x.Length > 2 &&
                x.All(char.IsLetter) &&
                !ignorar.Contains(x))
            .Take(8)
            .ToList();

        if (!palabras.Any())
        {
            return null;
        }

        return string.Join(
            " ",
            palabras
        );
    }
}