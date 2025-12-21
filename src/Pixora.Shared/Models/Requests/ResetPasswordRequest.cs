namespace Pixora.Shared.Models.Requests;

public record class ResetPasswordRequest(string Secret, string Token, string NewPassword, string ConfirmPassword);