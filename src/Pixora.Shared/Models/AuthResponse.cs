namespace Pixora.Shared.Models;

public class AuthResponse
{
    public string? AccessToken { get; }

    public string? RefreshToken { get; }

    public string? TwoFactorToken { get; }

    public AuthResponse(string twoFactorToken)
    {
        TwoFactorToken = twoFactorToken;
    }

    public AuthResponse(string accessToken, string refreshToken)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
    }
}