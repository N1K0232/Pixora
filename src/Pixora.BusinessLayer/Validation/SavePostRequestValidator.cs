using FluentValidation;
using Pixora.Shared.Models.Requests;

namespace Pixora.BusinessLayer.Validation;

public class SavePostRequestValidator : AbstractValidator<SavePostRequest>
{
    public SavePostRequestValidator()
    {
        RuleFor(p => p.Title).NotEmpty().MaximumLength(255);
        RuleFor(p => p.Content).NotEmpty();
    }
}