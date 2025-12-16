using FluentValidation;
using Pixora.Shared.Models.Requests;

namespace Pixora.BusinessLayer.Validation;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(r => r.AccessToken).NotEmpty();
        RuleFor(r => r.RefreshToken).NotEmpty();
    }
}