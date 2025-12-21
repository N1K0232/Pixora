namespace Pixora.Shared.Models.Requests;

public record class RegisterRequest(string FirstName, string LastName, string Email, string UserName, string Password, string ConfirmPassword, bool EnableNotifications);