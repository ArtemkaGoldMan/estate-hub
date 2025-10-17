using EstateHub.ListingService.Domain.DTO;
using FluentValidation;

namespace EstateHub.ListingService.Core.Validators;

public class CreateReportInputValidator : AbstractValidator<CreateReportInput>
{
    public CreateReportInputValidator()
    {
        RuleFor(x => x.ListingId)
            .NotEmpty()
            .WithMessage("Listing ID is required");

        RuleFor(x => x.Reason)
            .IsInEnum()
            .WithMessage("Invalid report reason");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(1000)
            .WithMessage("Description cannot exceed 1000 characters")
            .MinimumLength(10)
            .WithMessage("Description must be at least 10 characters long");
    }
}
