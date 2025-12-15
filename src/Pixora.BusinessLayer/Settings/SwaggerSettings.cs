namespace Pixora.BusinessLayer.Settings;

public class SwaggerSettings
{
    public bool IsEnabled { get; init; } = true;

    public string? UserName { get; init; }

    public string? Password { get; init; }
}