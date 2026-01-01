using System.Globalization;

namespace Pixora.BusinessLayer.Settings;

public class AppSettings
{
    public string ApplicationName { get; init; } = "Pixora";

    public string ApplicationDescription { get; init; } = "Upload photos, videos and posts. Interact with other users";

    public int CommandTimeout { get; init; } = 120;

    public bool ExecuteStartup { get; init; } = true;

    public int MaxRetryCount { get; init; } = 10;

    public TimeSpan MaxRetryDelay { get; init; } = TimeSpan.FromSeconds(2);

    public long MaxUploadSize { get; init; } = 20971520;

    public string? SenderEmail { get; init; }

    public string? SenderName { get; init; }

    public string StorageFolder { get; init; } = string.Empty;

    public string[] SupportedCultures { get; init; } = [];
}