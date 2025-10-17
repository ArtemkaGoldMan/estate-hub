using EstateHub.ListingService.Domain.DTO;
using EstateHub.ListingService.Domain.Enums;
using FluentValidation;

namespace EstateHub.ListingService.Core.Validators;

public class ChangeStatusInputValidator : AbstractValidator<ChangeStatusInput>
{
    public ChangeStatusInputValidator()
    {
        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Invalid listing status");
    }
}
