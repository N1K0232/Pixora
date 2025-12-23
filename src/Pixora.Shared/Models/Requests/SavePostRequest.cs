using Pixora.Shared.Enums;

namespace Pixora.Shared.Models.Requests;

public record class SavePostRequest(string Title, string Content, PostVisibility Visibility = PostVisibility.Friends);