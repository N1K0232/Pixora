namespace Pixora.Shared.Models;

public class Image
{
    public Guid Id { get; set; }

    public string FileName { get; set; } = null!;

    public string Path { get; set; } = null!;

    public long Length { get; set; }

    public string ContentType { get; set; } = null!;

    public string? Description { get; set; }

    public IEnumerable<string> Tags { get; set; } = [];
}