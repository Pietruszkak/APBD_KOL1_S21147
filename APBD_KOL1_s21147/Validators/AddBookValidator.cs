using APBD_KOL1_s21147.DTOs;
using FluentValidation;

namespace APBD_KOL1_s21147.Validators;

public class AddBookValidator : AbstractValidator<AddBooksDTO>
{
    public AddBookValidator()
    {
        RuleFor(e => e.Title).MaximumLength(100).NotNull();
    }
}