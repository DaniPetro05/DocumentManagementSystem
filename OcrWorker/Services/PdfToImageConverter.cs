using System.Diagnostics;

namespace OcrWorker.Services;

public class PdfToImageConverter
{
    /// <summary>
    /// Converts each page of a PDF to a PNG file using Ghostscript.
    /// Returns the list of generated PNG file paths.
    /// </summary>
    public List<string> ConvertPdfToImages(string pdfPath, string outputDir, int dpi = 300)
    {
        var files = new List<string>();
        Directory.CreateDirectory(outputDir);

        var psi = new ProcessStartInfo
        {
            FileName = "gs",
            Arguments = $"-dNOPAUSE -dBATCH -sDEVICE=png16m -r{dpi} -sOutputFile={outputDir}/page-%d.png \"{pdfPath}\"",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        using var process = Process.Start(psi)!;
        process.WaitForExit();

        //Ghostscript benennt Dateien mit z.B. page-1.png, page-2.png, etc.
        files = Directory.GetFiles(outputDir, "page-*.png")
            .OrderBy(f => int.Parse(Path.GetFileNameWithoutExtension(f).Split('-')[1]))
            .ToList();

        return files;
    }
}