namespace Pixora.Shared.Models.Requests;

public record class IpBanRequest(string Ip, string? Reason, TimeSpan Duration);