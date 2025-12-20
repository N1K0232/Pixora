namespace Pixora.Authentication.Entities;

public class IpAddressBan
{
    public Guid Id { get; set; }

    public string Value { get; set; } = null!;

    public string? Reason { get; set; }

    public DateTimeOffset BannedAt { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }
}