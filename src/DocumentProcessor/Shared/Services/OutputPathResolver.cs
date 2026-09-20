namespace DocumentProcessor.Shared.Services;

public class OutputPathResolver
{
    public string Resolve(string pdfPath, string? outputPath, params string[] defaultDirectoryParts)
    {
        if (!string.IsNullOrWhiteSpace(outputPath))
        {
            return Path.GetFullPath(outputPath);
        }

        var timestamp = DateTimeOffset.Now.ToString("yyyyMMdd_HHmmss");
        var fileName = $"{Path.GetFileNameWithoutExtension(pdfPath)}_{timestamp}.json";
        return Path.GetFullPath(Path.Combine(defaultDirectoryParts.Append(fileName).ToArray()));
    }
}
