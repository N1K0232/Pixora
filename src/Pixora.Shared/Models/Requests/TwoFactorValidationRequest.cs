namespace Pixora.Shared.Models.Requests;

public record class TwoFactorValidationRequest(string Token, string Code);