namespace Pixora.BusinessLayer.Settings;

public class AppSettings
{
    public string ApplicationName { get; init; } = "Pixora";

    public string ApplicationDescription { get; init; } = "Upload photos, videos and posts. Interact with other users";

    public bool ExecuteStartup { get; init; } = true;

    public long MaxUploadSize { get; set; } = 20971520;

    public string? SenderEmail { get; init; }

    public string? SenderName { get; init; }

    public string StorageFolder { get; init; } = string.Empty;

    public string[] SupportedCultures { get; init; } = [];
}