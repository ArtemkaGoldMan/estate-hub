# Domain Models

This folder contains pure domain models for the EstateHub Listing Service, following DDD principles and implemented as **records** for better immutability and value semantics.

## Structure

- **Listing.cs** - Main domain aggregate with business logic and validation
- **ListingPhoto.cs** - Photo domain model with validation
- **LikedListing.cs** - User-like relationship domain model

## Implementation Notes

- **Records** for immutability by default and value semantics
- Pure domain objects with no EF Core dependencies
- Business logic encapsulated within models
- Comprehensive validation rules in constructors
- **Immutable updates** using `with` expressions for state changes
- Rich domain behavior through methods that return new instances
- Ready for mapping to EF Core entities in DataAccess layer

## Benefits of Records

1. **Immutability** - Properties are `init`-only by default
2. **Value Semantics** - Built-in equality comparison
3. **Cleaner Syntax** - `with` expressions for updates
4. **Thread Safety** - Immutable objects are inherently thread-safe
5. **Functional Programming** - Encourages functional patterns
