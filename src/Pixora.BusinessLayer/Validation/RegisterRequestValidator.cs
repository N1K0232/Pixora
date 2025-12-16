using FluentValidation;
using Pixora.Shared.Models.Requests;

namespace Pixora.BusinessLayer.Validation;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(r => r.FirstName).NotEmpty();
        RuleFor(r => r.LastName).NotEmpty();
        RuleFor(r => r.Email).NotEmpty().EmailAddress();
        RuleFor(r => r.UserName).NotEmpty();
        RuleFor(r => r.Password).NotEmpty();
    }
}