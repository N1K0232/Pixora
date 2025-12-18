using Pixora.Authentication.Entities;
using Pixora.DataAccessLayer.Entities.Common;

namespace Pixora.DataAccessLayer.Entities;

public class Image : BaseEntity
{
    public Guid UserId { get; set; }

    public string FileName { get; set; } = null!;

    public string Path { get; set; } = null!;

    public long Length { get; set; }

    public string ContentType { get; set; } = null!;

    public string? Description { get; set; }

    public IEnumerable<string> Tags { get; set; } = [];

    public bool IsPublished { get; set; }

    public virtual ApplicationUser User { get; set; } = null!;
}