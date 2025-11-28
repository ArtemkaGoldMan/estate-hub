using EstateHub.ListingService.DataAccess.SqlServer.Db;
using EstateHub.ListingService.DataAccess.SqlServer.Entities;
using EstateHub.ListingService.Domain.DTO;
using EstateHub.ListingService.Domain.Enums;
using EstateHub.ListingService.Domain.Interfaces;
using EstateHub.ListingService.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EstateHub.ListingService.API;

public class Seed
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<Seed> _logger;
    private readonly IListingRepository _listingRepository;
    private readonly IPhotoRepository _photoRepository;

    // Test user ID for seed data (you can change this to a real user ID)
    private static readonly Guid TestUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public Seed(
        ApplicationDbContext context,
        ILogger<Seed> logger,
        IListingRepository listingRepository,
        IPhotoRepository photoRepository)
    {
        _context = context;
        _logger = logger;
        _listingRepository = listingRepository;
        _photoRepository = photoRepository;
    }

    public async Task SeedDataContextAsync()
    {
        try
        {
            _logger.LogInformation("Starting data initialization");

            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
            var hasPendingMigrations = pendingMigrations.Any();

            if (hasPendingMigrations)
            {
                _logger.LogInformation($"Found {pendingMigrations.Count()} pending migrations. Applying...");
                await _context.Database.MigrateAsync();
                _logger.LogInformation("All migrations applied successfully");
            }
            else
            {
                _logger.LogInformation("No pending migrations found");
            }

            // Check if we already have listings
            var existingListings = await _context.Listings.CountAsync();
            if (existingListings > 0)
            {
                _logger.LogInformation($"Database already contains {existingListings} listings. Checking if photos need to be added...");
                
                // Check if any listings are missing photos
                var listingsWithoutPhotos = await _context.Listings
                    .Where(l => !l.IsDeleted && !_context.ListingPhotos.Any(p => p.ListingId == l.Id))
                    .ToListAsync();
                
                if (listingsWithoutPhotos.Any())
                {
                    _logger.LogInformation($"Found {listingsWithoutPhotos.Count} listings without photos. Adding photos...");
                    await AddPhotosToExistingListingsAsync(listingsWithoutPhotos);
                }
                else
                {
                    _logger.LogInformation("All listings already have photos. Skipping seed data.");
                }
                return;
            }

            _logger.LogInformation("Seeding listings and photos...");
            await SeedListingsAsync();
            _logger.LogInformation("Data initialization completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during data initialization");
            throw;
        }
    }

    private async Task SeedListingsAsync()
    {
        var listings = GetSeedListings();

        foreach (var listingData in listings)
        {
            try
            {
                // Create listing directly using repository (bypassing service authentication)
                var listing = new Listing(
                    TestUserId,
                    listingData.Category,
                    listingData.PropertyType,
                    listingData.Title,
                    listingData.Description,
                    listingData.AddressLine,
                    listingData.District,
                    listingData.City,
                    listingData.PostalCode,
                    listingData.Latitude,
                    listingData.Longitude,
                    listingData.SquareMeters,
                    listingData.Rooms,
                    listingData.Condition,
                    listingData.HasBalcony,
                    listingData.HasElevator,
                    listingData.HasParkingSpace,
                    listingData.HasSecurity,
                    listingData.HasStorageRoom,
                    listingData.Floor,
                    listingData.FloorCount,
                    listingData.BuildYear,
                    listingData.PricePln,
                    listingData.MonthlyRentPln
                );

                await _listingRepository.AddAsync(listing);
                _logger.LogInformation($"Created listing: {listingData.Title} (ID: {listing.Id})");

                // Publish the listing
                await _listingRepository.UpdateStatusAsync(listing.Id, ListingStatus.Published);

                // Add photos using external URLs directly via repository (bypasses auth)
                if (listingData.PhotoUrls != null && listingData.PhotoUrls.Any())
                {
                    foreach (var (photoUrl, index) in listingData.PhotoUrls.Select((url, idx) => (url, idx)))
                    {
                        try
                        {
                            // Add photo directly via repository to bypass authentication checks
                            await _photoRepository.AddPhotoAsync(listing.Id, photoUrl);
                            _logger.LogInformation($"Added photo {index + 1} to listing {listing.Id}: {photoUrl}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, $"Failed to add photo {index + 1} to listing {listing.Id}: {ex.Message}");
                        }
                    }
                }

                await Task.Delay(100); // Small delay to avoid overwhelming the system
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to create listing: {listingData.Title}");
            }
        }
    }

    private async Task AddPhotosToExistingListingsAsync(List<ListingEntity> listings)
    {
        var seedData = GetSeedListings();
        var seedDataByTitle = seedData.ToDictionary(s => s.Title, StringComparer.OrdinalIgnoreCase);

        foreach (var listing in listings)
        {
            try
            {
                // Try to match by title to get the correct photos
                if (seedDataByTitle.TryGetValue(listing.Title, out var matchingSeedData) && 
                    matchingSeedData.PhotoUrls != null && matchingSeedData.PhotoUrls.Any())
                {
                    foreach (var (photoUrl, index) in matchingSeedData.PhotoUrls.Select((url, idx) => (url, idx)))
                    {
                        try
                        {
                            await _photoRepository.AddPhotoAsync(listing.Id, photoUrl);
                            _logger.LogInformation($"Added photo {index + 1} to existing listing {listing.Id} ({listing.Title})");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, $"Failed to add photo {index + 1} to listing {listing.Id}: {ex.Message}");
                        }
                    }
                }
                else
                {
                    // If no match, add some default photos
                    var defaultPhotos = new[]
                    {
                        "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=800",
                        "https://images.unsplash.com/photo-1484154218962-a197022b5858?w=800"
                    };
                    
                    foreach (var (photoUrl, index) in defaultPhotos.Select((url, idx) => (url, idx)))
                    {
                        try
                        {
                            await _photoRepository.AddPhotoAsync(listing.Id, photoUrl);
                            _logger.LogInformation($"Added default photo {index + 1} to existing listing {listing.Id} ({listing.Title})");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, $"Failed to add default photo {index + 1} to listing {listing.Id}: {ex.Message}");
                        }
                    }
                }

                await Task.Delay(50); // Small delay
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to add photos to listing {listing.Id}");
            }
        }
    }

    private List<SeedListingData> GetSeedListings()
    {
        return new List<SeedListingData>
        {
            // Sale Listings
            new SeedListingData
            {
                Title = "Modern 3-Bedroom Apartment in Śródmieście",
                Description = "Beautiful, recently renovated apartment in the heart of Warsaw. Features modern kitchen, spacious living room, and two bathrooms. Close to public transport and shopping centers.",
                Category = ListingCategory.Sale,
                PropertyType = PropertyType.Apartment,
                City = "Warsaw",
                District = "Śródmieście",
                AddressLine = "ul. Nowy Świat 15",
                PostalCode = "00-001",
                Latitude = 52.2297m,
                Longitude = 21.0122m,
                SquareMeters = 85,
                Rooms = 3,
                Floor = 5,
                FloorCount = 8,
                BuildYear = 2015,
                Condition = Condition.Good,
                HasBalcony = true,
                HasElevator = true,
                HasParkingSpace = true,
                HasSecurity = true,
                HasStorageRoom = true,
                PricePln = 850000,
                PhotoUrls = new[]
                {
                    "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=800",
                    "https://images.unsplash.com/photo-1484154218962-a197022b5858?w=800",
                    "https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?w=800"
                }
            },
            new SeedListingData
            {
                Title = "Cozy Studio Apartment in Mokotów",
                Description = "Perfect starter apartment in quiet neighborhood. Fully furnished, modern design. Great location near parks and cafes.",
                Category = ListingCategory.Sale,
                PropertyType = PropertyType.Studio,
                City = "Warsaw",
                District = "Mokotów",
                AddressLine = "ul. Puławska 120",
                PostalCode = "02-620",
                Latitude = 52.1956m,
                Longitude = 21.0208m,
                SquareMeters = 32,
                Rooms = 1,
                Floor = 3,
                FloorCount = 5,
                BuildYear = 2018,
                Condition = Condition.New,
                HasBalcony = true,
                HasElevator = true,
                HasParkingSpace = false,
                HasSecurity = false,
                HasStorageRoom = false,
                PricePln = 420000,
                PhotoUrls = new[]
                {
                    "https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?w=800",
                    "https://images.unsplash.com/photo-1522771739844-6a9f6d5f14af?w=800"
                }
            },
            new SeedListingData
            {
                Title = "Luxury House in Wilanów",
                Description = "Stunning family house with garden and garage. Three floors, modern architecture, high-end finishes throughout. Perfect for families.",
                Category = ListingCategory.Sale,
                PropertyType = PropertyType.House,
                City = "Warsaw",
                District = "Wilanów",
                AddressLine = "ul. Królewska 45",
                PostalCode = "02-954",
                Latitude = 52.1633m,
                Longitude = 21.0875m,
                SquareMeters = 180,
                Rooms = 5,
                Floor = null,
                FloorCount = 3,
                BuildYear = 2020,
                Condition = Condition.New,
                HasBalcony = true,
                HasElevator = false,
                HasParkingSpace = true,
                HasSecurity = true,
                HasStorageRoom = true,
                PricePln = 2500000,
                PhotoUrls = new[]
                {
                    "https://images.unsplash.com/photo-1600596542815-ffad4c1539a9?w=800",
                    "https://images.unsplash.com/photo-1600585154340-be6161a56a0c?w=800",
                    "https://images.unsplash.com/photo-1600566753190-17f0baa2a6c3?w=800",
                    "https://images.unsplash.com/photo-1600585154526-990dbe4eb5a3?w=800"
                }
            },
            new SeedListingData
            {
                Title = "2-Bedroom Apartment in Żoliborz",
                Description = "Charming apartment in historic building. High ceilings, original features, recently updated. Close to metro and parks.",
                Category = ListingCategory.Sale,
                PropertyType = PropertyType.Apartment,
                City = "Warsaw",
                District = "Żoliborz",
                AddressLine = "ul. Krasińskiego 20",
                PostalCode = "01-612",
                Latitude = 52.2689m,
                Longitude = 20.9844m,
                SquareMeters = 65,
                Rooms = 2,
                Floor = 2,
                FloorCount = 4,
                BuildYear = 1930,
                Condition = Condition.NeedsRenovation,
                HasBalcony = true,
                HasElevator = false,
                HasParkingSpace = false,
                HasSecurity = false,
                HasStorageRoom = true,
                PricePln = 680000,
                PhotoUrls = new[]
                {
                    "https://images.unsplash.com/photo-1505843513577-22bb7d21e455?w=800",
                    "https://images.unsplash.com/photo-1522771739844-6a9f6d5f14af?w=800"
                }
            },
            new SeedListingData
            {
                Title = "Spacious 4-Bedroom Apartment in Wola",
                Description = "Large family apartment with great layout. Modern kitchen, two bathrooms, large living area. Excellent location for families.",
                Category = ListingCategory.Sale,
                PropertyType = PropertyType.Apartment,
                City = "Warsaw",
                District = "Wola",
                AddressLine = "ul. Chłodna 25",
                PostalCode = "00-891",
                Latitude = 52.2333m,
                Longitude = 20.9833m,
                SquareMeters = 120,
                Rooms = 4,
                Floor = 7,
                FloorCount = 10,
                BuildYear = 2012,
                Condition = Condition.Good,
                HasBalcony = true,
                HasElevator = true,
                HasParkingSpace = true,
                HasSecurity = true,
                HasStorageRoom = true,
                PricePln = 1200000,
                PhotoUrls = new[]
                {
                    "https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?w=800",
                    "https://images.unsplash.com/photo-1484154218962-a197022b5858?w=800",
                    "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=800"
                }
            },
            new SeedListingData
            {
                Title = "Modern Room for Rent in Praga",
                Description = "Furnished room in shared apartment. Great for students or young professionals. Close to university and city center.",
                Category = ListingCategory.Rent,
                PropertyType = PropertyType.Room,
                City = "Warsaw",
                District = "Praga-Północ",
                AddressLine = "ul. Ząbkowska 30",
                PostalCode = "03-736",
                Latitude = 52.2544m,
                Longitude = 21.0433m,
                SquareMeters = 18,
                Rooms = 1,
                Floor = 2,
                FloorCount = 4,
                BuildYear = 2010,
                Condition = Condition.Good,
                HasBalcony = false,
                HasElevator = true,
                HasParkingSpace = false,
                HasSecurity = false,
                HasStorageRoom = false,
                MonthlyRentPln = 1800,
                PhotoUrls = new[]
                {
                    "https://images.unsplash.com/photo-1522771739844-6a9f6d5f14af?w=800"
                }
            },
            new SeedListingData
            {
                Title = "1-Bedroom Apartment for Rent in Ochota",
                Description = "Cozy apartment perfect for couples. Fully furnished, modern appliances. Quiet street, good transport links.",
                Category = ListingCategory.Rent,
                PropertyType = PropertyType.Apartment,
                City = "Warsaw",
                District = "Ochota",
                AddressLine = "ul. Grójecka 50",
                PostalCode = "02-301",
                Latitude = 52.2167m,
                Longitude = 20.9833m,
                SquareMeters = 45,
                Rooms = 2,
                Floor = 4,
                FloorCount = 6,
                BuildYear = 2016,
                Condition = Condition.Good,
                HasBalcony = true,
                HasElevator = true,
                HasParkingSpace = false,
                HasSecurity = true,
                HasStorageRoom = false,
                MonthlyRentPln = 3500,
                PhotoUrls = new[]
                {
                    "https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?w=800",
                    "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=800"
                }
            },
            new SeedListingData
            {
                Title = "Luxury 3-Bedroom Apartment for Rent in Centrum",
                Description = "Premium apartment in prestigious location. High-end finishes, concierge service, underground parking. Perfect for executives.",
                Category = ListingCategory.Rent,
                PropertyType = PropertyType.Apartment,
                City = "Warsaw",
                District = "Śródmieście",
                AddressLine = "ul. Marszałkowska 100",
                PostalCode = "00-001",
                Latitude = 52.2297m,
                Longitude = 21.0122m,
                SquareMeters = 95,
                Rooms = 3,
                Floor = 12,
                FloorCount = 15,
                BuildYear = 2019,
                Condition = Condition.New,
                HasBalcony = true,
                HasElevator = true,
                HasParkingSpace = true,
                HasSecurity = true,
                HasStorageRoom = true,
                MonthlyRentPln = 8500,
                PhotoUrls = new[]
                {
                    "https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?w=800",
                    "https://images.unsplash.com/photo-1484154218962-a197022b5858?w=800",
                    "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=800",
                    "https://images.unsplash.com/photo-1505843513577-22bb7d21e455?w=800"
                }
            },
            new SeedListingData
            {
                Title = "Family House for Rent in Białołęka",
                Description = "Spacious house with large garden. Perfect for families with children. Quiet neighborhood, good schools nearby.",
                Category = ListingCategory.Rent,
                PropertyType = PropertyType.House,
                City = "Warsaw",
                District = "Białołęka",
                AddressLine = "ul. Modlińska 200",
                PostalCode = "03-216",
                Latitude = 52.3167m,
                Longitude = 20.9667m,
                SquareMeters = 150,
                Rooms = 4,
                Floor = null,
                FloorCount = 2,
                BuildYear = 2015,
                Condition = Condition.Good,
                HasBalcony = true,
                HasElevator = false,
                HasParkingSpace = true,
                HasSecurity = false,
                HasStorageRoom = true,
                MonthlyRentPln = 6000,
                PhotoUrls = new[]
                {
                    "https://images.unsplash.com/photo-1600596542815-ffad4c1539a9?w=800",
                    "https://images.unsplash.com/photo-1600585154340-be6161a56a0c?w=800",
                    "https://images.unsplash.com/photo-1600566753190-17f0baa2a6c3?w=800"
                }
            },
            new SeedListingData
            {
                Title = "2-Bedroom Apartment for Sale in Ursynów",
                Description = "Well-maintained apartment in residential area. Good investment opportunity. Close to metro and shopping centers.",
                Category = ListingCategory.Sale,
                PropertyType = PropertyType.Apartment,
                City = "Warsaw",
                District = "Ursynów",
                AddressLine = "ul. Puławska 300",
                PostalCode = "02-715",
                Latitude = 52.1500m,
                Longitude = 21.0333m,
                SquareMeters = 70,
                Rooms = 2,
                Floor = 6,
                FloorCount = 9,
                BuildYear = 2008,
                Condition = Condition.Good,
                HasBalcony = true,
                HasElevator = true,
                HasParkingSpace = true,
                HasSecurity = true,
                HasStorageRoom = false,
                PricePln = 750000,
                PhotoUrls = new[]
                {
                    "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=800",
                    "https://images.unsplash.com/photo-1484154218962-a197022b5858?w=800"
                }
            },
            new SeedListingData
            {
                Title = "Studio Apartment for Rent in Włochy",
                Description = "Modern studio in new building. Fully equipped kitchen, modern bathroom. Great for single professionals.",
                Category = ListingCategory.Rent,
                PropertyType = PropertyType.Studio,
                City = "Warsaw",
                District = "Włochy",
                AddressLine = "ul. 1 Sierpnia 30",
                PostalCode = "02-134",
                Latitude = 52.2000m,
                Longitude = 20.9167m,
                SquareMeters = 28,
                Rooms = 1,
                Floor = 2,
                FloorCount = 5,
                BuildYear = 2021,
                Condition = Condition.New,
                HasBalcony = false,
                HasElevator = true,
                HasParkingSpace = false,
                HasSecurity = true,
                HasStorageRoom = false,
                MonthlyRentPln = 2200,
                PhotoUrls = new[]
                {
                    "https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?w=800"
                }
            },
            new SeedListingData
            {
                Title = "3-Bedroom Apartment for Sale in Bemowo",
                Description = "Comfortable apartment in family-friendly area. Good schools, parks, and shopping nearby. Great value for money.",
                Category = ListingCategory.Sale,
                PropertyType = PropertyType.Apartment,
                City = "Warsaw",
                District = "Bemowo",
                AddressLine = "ul. Powstańców Śląskich 100",
                PostalCode = "01-381",
                Latitude = 52.2333m,
                Longitude = 20.9000m,
                SquareMeters = 80,
                Rooms = 3,
                Floor = 3,
                FloorCount = 5,
                BuildYear = 2014,
                Condition = Condition.Good,
                HasBalcony = true,
                HasElevator = true,
                HasParkingSpace = true,
                HasSecurity = false,
                HasStorageRoom = true,
                PricePln = 920000,
                PhotoUrls = new[]
                {
                    "https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?w=800",
                    "https://images.unsplash.com/photo-1484154218962-a197022b5858?w=800",
                    "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=800"
                }
            },
            new SeedListingData
            {
                Title = "1-Bedroom Apartment for Rent in Targówek",
                Description = "Affordable apartment in developing area. Good transport connections. Perfect for young professionals.",
                Category = ListingCategory.Rent,
                PropertyType = PropertyType.Apartment,
                City = "Warsaw",
                District = "Targówek",
                AddressLine = "ul. Radzymińska 150",
                PostalCode = "03-230",
                Latitude = 52.2833m,
                Longitude = 21.0500m,
                SquareMeters = 38,
                Rooms = 2,
                Floor = 1,
                FloorCount = 4,
                BuildYear = 2011,
                Condition = Condition.Good,
                HasBalcony = true,
                HasElevator = false,
                HasParkingSpace = false,
                HasSecurity = false,
                HasStorageRoom = false,
                MonthlyRentPln = 2800,
                PhotoUrls = new[]
                {
                    "https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?w=800",
                    "https://images.unsplash.com/photo-1522771739844-6a9f6d5f14af?w=800"
                }
            },
            new SeedListingData
            {
                Title = "Renovated 2-Bedroom Apartment for Sale in Praga",
                Description = "Recently renovated apartment in historic building. Modern amenities while preserving original character. Great investment.",
                Category = ListingCategory.Sale,
                PropertyType = PropertyType.Apartment,
                City = "Warsaw",
                District = "Praga-Południe",
                AddressLine = "ul. Grochowska 200",
                PostalCode = "04-301",
                Latitude = 52.2333m,
                Longitude = 21.0667m,
                SquareMeters = 60,
                Rooms = 2,
                Floor = 4,
                FloorCount = 5,
                BuildYear = 1920,
                Condition = Condition.Good,
                HasBalcony = true,
                HasElevator = false,
                HasParkingSpace = false,
                HasSecurity = false,
                HasStorageRoom = true,
                PricePln = 580000,
                PhotoUrls = new[]
                {
                    "https://images.unsplash.com/photo-1505843513577-22bb7d21e455?w=800",
                    "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=800"
                }
            },
            new SeedListingData
            {
                Title = "Modern 4-Bedroom Apartment for Sale in Saska Kępa",
                Description = "Spacious luxury apartment with stunning views. Premium location, high-end finishes, concierge service.",
                Category = ListingCategory.Sale,
                PropertyType = PropertyType.Apartment,
                City = "Warsaw",
                District = "Praga-Południe",
                AddressLine = "ul. Francuska 30",
                PostalCode = "03-905",
                Latitude = 52.2333m,
                Longitude = 21.0500m,
                SquareMeters = 140,
                Rooms = 4,
                Floor = 8,
                FloorCount = 12,
                BuildYear = 2021,
                Condition = Condition.New,
                HasBalcony = true,
                HasElevator = true,
                HasParkingSpace = true,
                HasSecurity = true,
                HasStorageRoom = true,
                PricePln = 1800000,
                PhotoUrls = new[]
                {
                    "https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?w=800",
                    "https://images.unsplash.com/photo-1484154218962-a197022b5858?w=800",
                    "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=800",
                    "https://images.unsplash.com/photo-1505843513577-22bb7d21e455?w=800",
                    "https://images.unsplash.com/photo-1522771739844-6a9f6d5f14af?w=800"
                }
            }
        };
    }

    private class SeedListingData
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ListingCategory Category { get; set; }
        public PropertyType PropertyType { get; set; }
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal SquareMeters { get; set; }
        public int Rooms { get; set; }
        public int? Floor { get; set; }
        public int? FloorCount { get; set; }
        public int? BuildYear { get; set; }
        public Condition Condition { get; set; }
        public bool HasBalcony { get; set; }
        public bool HasElevator { get; set; }
        public bool HasParkingSpace { get; set; }
        public bool HasSecurity { get; set; }
        public bool HasStorageRoom { get; set; }
        public decimal? PricePln { get; set; }
        public decimal? MonthlyRentPln { get; set; }
        public string[]? PhotoUrls { get; set; }
    }
}
