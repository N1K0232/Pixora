namespace Pixora.Shared.Models;

public record class Post(Guid Id, string UserName, string Title, string Content, DateTime CreatedAt, DateTime? LastModifiedAt, bool IsEdited);