namespace Pixora.BusinessLayer.Settings;

public class AppSettings
{
    public string ApplicationName { get; init; } = "Pixora";

    public string ApplicationDescription { get; init; } = "Upload photos, videos and posts. Interact with other users";

    public string[] SupportedCultures { get; init; } = [];
}