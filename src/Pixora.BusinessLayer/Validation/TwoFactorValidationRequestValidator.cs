using FluentValidation;
using Pixora.Shared.Models.Requests;

namespace Pixora.BusinessLayer.Validation;

public class TwoFactorValidationRequestValidator : AbstractValidator<TwoFactorValidationRequest>
{
    public TwoFactorValidationRequestValidator()
    {
        RuleFor(t => t.Token).NotEmpty();
        RuleFor(t => t.Code).NotEmpty();
    }
}