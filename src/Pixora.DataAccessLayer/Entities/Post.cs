using Pixora.Authentication.Entities;
using Pixora.DataAccessLayer.Entities.Common;
using Pixora.Shared.Enums;

namespace Pixora.DataAccessLayer.Entities;

public class Post : BaseEntity
{
    public Guid UserId { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public PostVisibility Visibility { get; set; }

    public bool IsPublished { get; set; }

    public bool IsEdited { get; set; }

    public virtual ApplicationUser User { get; set; } = null!;
}