using System.Text.RegularExpressions;
using FluentValidation;
using Pixora.Shared.Models.Requests;

namespace Pixora.BusinessLayer.Validation;

public partial class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(p => p.Secret).NotEmpty();
        RuleFor(p => p.Token).NotEmpty();
        RuleFor(p => p.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .Matches(IsCapital()).WithMessage("Password must contain at least one capital letter")
            .Matches(IsLowercase()).WithMessage("Password must contain at least one lower case letter")
            .Matches(IsNumber()).WithMessage("Password must contain at least one number")
            .Matches(IsSpecialChar()).WithMessage("Password must contain at least one special character");

        RuleFor(p => p.ConfirmPassword).NotEmpty().Equal(p => p.NewPassword);
    }

    [GeneratedRegex("[A-Z]")]
    private partial Regex IsCapital();

    [GeneratedRegex("[a-z]")]
    private partial Regex IsLowercase();

    [GeneratedRegex("[0-9]")]
    private partial Regex IsNumber();

    [GeneratedRegex("[^a-zA-Z0-9]")]
    private partial Regex IsSpecialChar();
}