using Microsoft.AspNetCore.Mvc.ModelBinding;
using TinyHelpers.AspNetCore.DataAnnotations;

namespace Pixora.Models;

public class UploadImageRequest
{
    [BindRequired]
    [AllowedExtensions("*.jpg", "*.jpeg", "*.png")]
    public required IFormFile File { get; set; }

    public string? Description { get; set; }

    public string[]? Tags { get; set; }
}