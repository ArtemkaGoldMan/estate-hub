using EstateHub.ListingService.Core.DTO;
using EstateHub.ListingService.Domain.Enums;
using FluentValidation;

namespace EstateHub.ListingService.Core.Validators;

public class CreateListingInputValidator : AbstractValidator<CreateListingInput>
{
    public CreateListingInputValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

        RuleFor(x => x.AddressLine)
            .NotEmpty().WithMessage("Address line is required")
            .MaximumLength(200).WithMessage("Address line cannot exceed 200 characters");

        RuleFor(x => x.District)
            .NotEmpty().WithMessage("District is required")
            .MaximumLength(100).WithMessage("District cannot exceed 100 characters");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required")
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters");

        RuleFor(x => x.PostalCode)
            .NotEmpty().WithMessage("Postal code is required")
            .MaximumLength(10).WithMessage("Postal code cannot exceed 10 characters");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180");

        RuleFor(x => x.SquareMeters)
            .GreaterThan(0).WithMessage("Square meters must be greater than 0");

        RuleFor(x => x.Rooms)
            .GreaterThanOrEqualTo(1).WithMessage("Rooms must be at least 1");

        RuleFor(x => x.Floor)
            .GreaterThanOrEqualTo(0).WithMessage("Floor must be non-negative")
            .When(x => x.Floor.HasValue);

        RuleFor(x => x.FloorCount)
            .GreaterThanOrEqualTo(1).WithMessage("Floor count must be at least 1")
            .When(x => x.FloorCount.HasValue);

        RuleFor(x => x)
            .Must(x => !x.Floor.HasValue || !x.FloorCount.HasValue || x.Floor <= x.FloorCount)
            .WithMessage("Floor cannot be greater than floor count");

        RuleFor(x => x.BuildYear)
            .InclusiveBetween(1800, DateTime.Now.Year).WithMessage("Build year must be between 1800 and current year")
            .When(x => x.BuildYear.HasValue);

        RuleFor(x => x.PricePln)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be non-negative")
            .When(x => x.PricePln.HasValue);

        RuleFor(x => x.MonthlyRentPln)
            .GreaterThanOrEqualTo(0).WithMessage("Monthly rent must be non-negative")
            .When(x => x.MonthlyRentPln.HasValue);

        // Category-dependent validation
        RuleFor(x => x.PricePln)
            .NotNull().WithMessage("Price is required for sale listings")
            .GreaterThanOrEqualTo(0).WithMessage("Price must be non-negative")
            .When(x => x.Category == ListingCategory.Sale);

        RuleFor(x => x.MonthlyRentPln)
            .NotNull().WithMessage("Monthly rent is required for rent listings")
            .GreaterThanOrEqualTo(0).WithMessage("Monthly rent must be non-negative")
            .When(x => x.Category == ListingCategory.Rent);
    }
}
