using System.Diagnostics;

namespace DocumentProcessor.ImageExtraction.Services;

public class PdfImageRendererService
{
    public async Task<string[]> RenderPagesToBase64Png(string pdfPath, CancellationToken cancellationToken)
    {
        if (!File.Exists(pdfPath))
        {
            throw new FileNotFoundException($"No file exists at: {pdfPath}", pdfPath);
        }

        await using var stream = File.OpenRead(pdfPath);
        return await RenderPagesToBase64Png(stream, cancellationToken);
    }

    public async Task<string[]> RenderPagesToBase64Png(Stream pdfStream, CancellationToken cancellationToken)
    {
        var tempDirectory = Directory.CreateTempSubdirectory("document-processor-image-extraction-");
        var pdfPath = Path.Combine(tempDirectory.FullName, "input.pdf");
        var pngPath = Path.Combine(tempDirectory.FullName, "page.png");

        try
        {
            await using (var fileStream = File.Create(pdfPath))
            {
                await pdfStream.CopyToAsync(fileStream, cancellationToken);
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = "sips",
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            startInfo.ArgumentList.Add("-s");
            startInfo.ArgumentList.Add("format");
            startInfo.ArgumentList.Add("png");
            startInfo.ArgumentList.Add("-Z");
            startInfo.ArgumentList.Add("2400");
            startInfo.ArgumentList.Add(pdfPath);
            startInfo.ArgumentList.Add("--out");
            startInfo.ArgumentList.Add(pngPath);

            using var process = Process.Start(startInfo) ??
                                throw new InvalidOperationException("Failed to start sips for PDF image rendering.");
            var standardOutput = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            var standardError = await process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0 || !File.Exists(pngPath))
            {
                throw new InvalidOperationException(
                    $"Failed to render PDF to image. sips exit code: {process.ExitCode}. Output: {standardOutput}. Error: {standardError}");
            }

            var imageBytes = await File.ReadAllBytesAsync(pngPath, cancellationToken);
            return [Convert.ToBase64String(imageBytes)];
        }
        finally
        {
            tempDirectory.Delete(recursive: true);
        }
    }
}
