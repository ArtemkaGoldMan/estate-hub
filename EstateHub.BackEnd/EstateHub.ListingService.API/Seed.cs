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

    // Test user IDs for seed data (3 different users)
    private static readonly Guid[] TestUserIds = new[]
    {
        Guid.Parse("00000000-0000-0000-0000-000000000001"),
        Guid.Parse("00000000-0000-0000-0000-000000000002"),
        Guid.Parse("00000000-0000-0000-0000-000000000003")
    };

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
                _logger.LogInformation($"Database already contains {existingListings} listings. Cleaning up old seed data...");
                
                // Delete all existing listings and their photos to start fresh
                var allListings = await _context.Listings
                    .Include(l => l.Photos)
                    .ToListAsync();
                
                foreach (var listing in allListings)
                {
                    // Delete photos first
                    foreach (var photo in listing.Photos.ToList())
                    {
                        _context.ListingPhotos.Remove(photo);
                    }
                    // Delete the listing
                    _context.Listings.Remove(listing);
                }
                
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Cleaned up {allListings.Count} old listings. Proceeding with fresh seed data...");
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

        for (int i = 0; i < listings.Count; i++)
        {
            var listingData = listings[i];
            // Distribute listings among 3 users
            var ownerId = TestUserIds[i % TestUserIds.Length];
            
            try
            {
                // Create listing directly using repository (bypassing service authentication)
                var listing = new Listing(
                    ownerId,
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
            // ========== USER 1 LISTINGS (7 listings) ==========
            new SeedListingData
            {
                Title = "Modern 3-Bedroom Apartment in Śródmieście",
                Description = "<p>Welcome to this <strong>stunning, recently renovated</strong> apartment in the heart of Warsaw! This beautiful property offers the perfect blend of modern luxury and urban convenience.</p><p><strong>Key Features:</strong></p><ul><li><em>Spacious living room</em> with large windows and natural light</li><li><strong>Modern kitchen</strong> fully equipped with premium appliances</li><li>Two elegant bathrooms with contemporary fixtures</li><li>Three comfortable bedrooms with built-in wardrobes</li></ul><p>Located just steps away from public transport, shopping centers, restaurants, and cultural attractions. This is an <strong>exceptional opportunity</strong> to own a piece of Warsaw's vibrant city center!</p>",
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
                Description = "<p>Perfect starter apartment in a <em>quiet, charming neighborhood</em>! This cozy studio is ideal for young professionals or students looking for their first home.</p><p><strong>What makes this special:</strong></p><ul><li>Fully furnished with <strong>modern, stylish furniture</strong></li><li>Open-plan design maximizing space</li><li>Well-equipped kitchenette</li><li>Bright and airy with excellent natural light</li></ul><p>Great location near parks, cafes, and public transport. The area is known for its <em>friendly community atmosphere</em> and excellent amenities. Don't miss this opportunity!</p>",
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
                Description = "<h2>Stunning Family Residence</h2><p>This <strong>magnificent family house</strong> represents the pinnacle of modern luxury living in one of Warsaw's most prestigious neighborhoods.</p><p><strong>Property Highlights:</strong></p><ul><li><em>Three floors</em> of elegant living space</li><li><strong>Modern architecture</strong> with high-end finishes throughout</li><li>Spacious garden perfect for children and entertaining</li><li>Private garage with space for two cars</li><li>Premium materials and craftsmanship</li></ul><p>The interior features <strong>open-plan living areas</strong>, a gourmet kitchen, multiple bathrooms, and five generously sized bedrooms. The property is perfect for families seeking both luxury and comfort in a peaceful, well-connected location.</p>",
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
                Description = "<p>Discover the <em>charm and character</em> of this unique apartment in a beautifully preserved historic building. This property offers a rare opportunity to own a piece of Warsaw's architectural heritage.</p><p><strong>Special Features:</strong></p><ul><li><strong>High ceilings</strong> creating a sense of grandeur</li><li>Original architectural features carefully preserved</li><li>Recently updated with modern amenities</li><li>Excellent location near metro and parks</li></ul><p>While the property <em>needs some renovation</em>, it presents an excellent investment opportunity. The building's historic character combined with modern updates creates a truly special living space. Close to excellent schools, cultural venues, and green spaces.</p>",
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
                Description = "<h2>Perfect Family Home</h2><p>This <strong>large family apartment</strong> offers exceptional space and comfort in one of Warsaw's most family-friendly districts.</p><p><strong>Why families love this property:</strong></p><ul><li><em>Excellent layout</em> with separate living and sleeping areas</li><li><strong>Modern kitchen</strong> perfect for family meals</li><li>Two full bathrooms for convenience</li><li>Large living area for family gatherings</li><li>Four spacious bedrooms</li></ul><p>The location is <strong>ideal for families</strong> with excellent schools, parks, playgrounds, and shopping centers nearby. The building features modern amenities including elevator, security, and parking. This is a rare find in such a desirable location!</p>",
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
                Description = "<p>Looking for an <em>affordable, comfortable</em> place to call home? This furnished room in a shared apartment is perfect for students or young professionals!</p><p><strong>What's included:</strong></p><ul><li>Furnished room with <strong>bed, desk, and wardrobe</strong></li><li>Access to shared kitchen and bathroom</li><li>Fast internet connection</li><li>Friendly, respectful roommates</li></ul><p>Great location close to university campuses and the city center. The area is <em>vibrant and full of life</em> with cafes, restaurants, and cultural venues. Public transport is excellent, making it easy to get anywhere in Warsaw. Perfect for those starting their journey in the capital!</p>",
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
                Description = "<p>This <strong>cozy apartment</strong> is perfect for couples or single professionals seeking comfort and convenience in a quiet neighborhood.</p><p><strong>Apartment Features:</strong></p><ul><li><em>Fully furnished</em> with modern, comfortable furniture</li><li><strong>Modern appliances</strong> including dishwasher and washing machine</li><li>Bright and welcoming atmosphere</li><li>Quiet street with excellent neighbors</li></ul><p>The location offers <em>excellent transport links</em> making it easy to commute to work or explore the city. The area is peaceful yet well-connected, with shops, restaurants, and services within walking distance. This is a wonderful place to call home!</p>",
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
                Description = "<h2>Premium Executive Living</h2><p>Experience <strong>luxury living</strong> at its finest in this premium apartment located in Warsaw's most prestigious district.</p><p><strong>Luxury Features:</strong></p><ul><li><em>High-end finishes</em> throughout including marble and hardwood</li><li><strong>Concierge service</strong> available 24/7</li><li>Underground parking space included</li><li>Stunning city views from upper floors</li><li>Premium building amenities</li></ul><p>Perfect for <strong>executives and professionals</strong> who demand the best. The apartment features three spacious bedrooms, modern kitchen with top-of-the-line appliances, and elegant bathrooms. Located in the heart of the business district with excellent access to restaurants, shopping, and entertainment.</p>",
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
                Description = "<p>This <strong>spacious family house</strong> offers the perfect combination of comfort, space, and tranquility in a family-friendly neighborhood.</p><p><strong>Family-Friendly Features:</strong></p><ul><li><em>Large garden</em> perfect for children to play</li><li><strong>Four bedrooms</strong> providing plenty of space</li><li>Quiet, safe neighborhood</li><li>Excellent schools nearby</li><li>Parks and playgrounds within walking distance</li></ul><p>The property is <em>ideal for families</em> looking for a peaceful environment while maintaining good connections to the city center. The house features modern amenities, a well-maintained garden, and plenty of storage space. This is a wonderful place to raise a family!</p>",
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
                Description = "<p>Well-maintained apartment in a <em>desirable residential area</em> offering excellent value and investment potential.</p><p><strong>Investment Highlights:</strong></p><ul><li><strong>Excellent condition</strong> - move-in ready</li><li>Prime location near metro and shopping</li><li>Stable, growing neighborhood</li><li>Good rental yield potential</li></ul><p>This property represents a <strong>solid investment opportunity</strong> in one of Warsaw's most established residential districts. The apartment is in excellent condition and requires no immediate work. Close to excellent public transport, shopping centers, schools, and recreational facilities. Perfect for first-time buyers or investors!</p>",
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
                Description = "<p><strong>Modern studio</strong> in a brand new building - perfect for single professionals seeking contemporary living!</p><p><strong>Modern Features:</strong></p><ul><li><em>Fully equipped kitchen</em> with modern appliances</li><li><strong>Modern bathroom</strong> with quality fixtures</li><li>Efficient use of space with smart design</li><li>New building with modern amenities</li></ul><p>This studio is <em>ideal for professionals</em> who value modern design and convenience. The building is new, well-maintained, and features security and elevator. The location offers good connections to the city center while providing a quieter living environment. Great value for money!</p>",
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
                Description = "<h2>Great Value Family Home</h2><p>This <strong>comfortable apartment</strong> offers excellent value in a family-friendly area known for its community spirit and amenities.</p><p><strong>Why families choose Bemowo:</strong></p><ul><li><em>Excellent schools</em> at all levels</li><li><strong>Beautiful parks</strong> and green spaces</li><li>Family-oriented community</li><li>Good shopping and services</li><li>Safe, well-maintained neighborhood</li></ul><p>The apartment itself is <strong>spacious and well-designed</strong> with three bedrooms, modern kitchen, and comfortable living areas. The building features modern amenities including elevator and parking. This represents excellent value for families looking to settle in a great neighborhood!</p>",
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
                Description = "<p><strong>Affordable living</strong> in an up-and-coming area of Warsaw! This apartment offers great value for young professionals.</p><p><strong>What you get:</strong></p><ul><li><em>Affordable rent</em> in a developing area</li><li><strong>Good transport connections</strong> to city center</li><li>Modern building with basic amenities</li><li>Growing neighborhood with new developments</li></ul><p>The area is <em>rapidly developing</em> with new infrastructure, shops, and services being added regularly. This makes it an excellent choice for those seeking affordability without sacrificing too much on location. The apartment is well-maintained and ready to move in. Perfect for starting your Warsaw adventure!</p>",
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
                Description = "<h2>Historic Charm Meets Modern Comfort</h2><p>This <strong>beautifully renovated apartment</strong> in a historic building combines original character with modern amenities.</p><p><strong>Renovation Highlights:</strong></p><ul><li><em>Carefully preserved</em> original architectural features</li><li><strong>Modern amenities</strong> including updated plumbing and electrical</li><li>Contemporary kitchen and bathroom</li><li>High ceilings and original details</li></ul><p>This property represents an <strong>excellent investment</strong> in one of Warsaw's most characterful districts. The renovation has been done to a high standard, preserving the building's historic charm while adding all modern conveniences. The area is known for its vibrant cultural scene, excellent restaurants, and unique atmosphere. A truly special property!</p>",
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
                Description = "<h2>Luxury Living at Its Finest</h2><p>Experience <strong>spacious luxury</strong> in this stunning apartment located in one of Warsaw's most desirable neighborhoods.</p><p><strong>Premium Features:</strong></p><ul><li><em>Stunning views</em> from the 8th floor</li><li><strong>High-end finishes</strong> throughout</li><li>Premium location in prestigious district</li><li>Concierge service for your convenience</li><li>Four spacious bedrooms</li></ul><p>This property represents the <strong>pinnacle of luxury living</strong> in Warsaw. The apartment features elegant design, premium materials, and attention to detail throughout. Saska Kępa is known for its beautiful architecture, excellent restaurants, and vibrant community. This is a rare opportunity to own a truly exceptional property!</p>",
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
            },
            
            // ========== USER 3 LISTINGS (6 listings) ==========
            new SeedListingData
            {
                Title = "Charming 2-Bedroom Apartment for Rent in Mokotów",
                Description = "<p>This <em>charming apartment</em> offers a perfect blend of comfort, style, and excellent location in one of Warsaw's most popular districts.</p><p><strong>Apartment Highlights:</strong></p><ul><li><strong>Well-designed layout</strong> maximizing space</li><li>Modern kitchen with quality appliances</li><li>Two comfortable bedrooms</li><li>Bright and welcoming atmosphere</li><li>Excellent location near parks and cafes</li></ul><p>Mokotów is known for its <em>excellent quality of life</em> with great restaurants, beautiful parks, and excellent public transport. This apartment is perfect for couples or small families looking for a comfortable home in a great neighborhood. The building is well-maintained and the area is safe and friendly!</p>",
                Category = ListingCategory.Rent,
                PropertyType = PropertyType.Apartment,
                City = "Warsaw",
                District = "Mokotów",
                AddressLine = "ul. Puławska 250",
                PostalCode = "02-640",
                Latitude = 52.1900m,
                Longitude = 21.0150m,
                SquareMeters = 55,
                Rooms = 2,
                Floor = 3,
                FloorCount = 6,
                BuildYear = 2017,
                Condition = Condition.Good,
                HasBalcony = true,
                HasElevator = true,
                HasParkingSpace = false,
                HasSecurity = true,
                HasStorageRoom = false,
                MonthlyRentPln = 4200,
                PhotoUrls = new[]
                {
                    "https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?w=800",
                    "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=800"
                }
            },
            new SeedListingData
            {
                Title = "Contemporary Studio for Sale in Wola",
                Description = "<p><strong>Modern studio</strong> in a contemporary building - perfect for first-time buyers or investors!</p><p><strong>Investment Potential:</strong></p><ul><li><em>New building</em> with modern amenities</li><li><strong>Excellent rental yield</strong> potential</li><li>Growing area with new developments</li><li>Good transport connections</li></ul><p>Wola is one of Warsaw's <em>fastest-developing districts</em> with new offices, residential buildings, and amenities being added constantly. This studio represents an excellent entry point into the property market or a solid investment opportunity. The apartment is modern, efficient, and ready to move in or rent out immediately!</p>",
                Category = ListingCategory.Sale,
                PropertyType = PropertyType.Studio,
                City = "Warsaw",
                District = "Wola",
                AddressLine = "ul. Chłodna 50",
                PostalCode = "00-892",
                Latitude = 52.2400m,
                Longitude = 20.9900m,
                SquareMeters = 30,
                Rooms = 1,
                Floor = 5,
                FloorCount = 8,
                BuildYear = 2022,
                Condition = Condition.New,
                HasBalcony = false,
                HasElevator = true,
                HasParkingSpace = false,
                HasSecurity = true,
                HasStorageRoom = false,
                PricePln = 380000,
                PhotoUrls = new[]
                {
                    "https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?w=800"
                }
            },
            new SeedListingData
            {
                Title = "Spacious 3-Bedroom Apartment for Rent in Żoliborz",
                Description = "<h2>Family-Friendly Living</h2><p>This <strong>spacious apartment</strong> is perfect for families seeking comfort and excellent location in a well-established neighborhood.</p><p><strong>Family Features:</strong></p><ul><li><em>Three bedrooms</em> providing plenty of space</li><li><strong>Large living area</strong> for family time</li><li>Modern kitchen perfect for family meals</li><li>Close to excellent schools</li><li>Parks and playgrounds nearby</li></ul><p>Żoliborz is renowned for its <strong>family-friendly atmosphere</strong> and excellent amenities. The area offers great schools, beautiful parks, and a strong sense of community. This apartment provides all the space and comfort a growing family needs, in a location that's both peaceful and well-connected to the rest of the city!</p>",
                Category = ListingCategory.Rent,
                PropertyType = PropertyType.Apartment,
                City = "Warsaw",
                District = "Żoliborz",
                AddressLine = "ul. Krasińskiego 40",
                PostalCode = "01-614",
                Latitude = 52.2700m,
                Longitude = 20.9900m,
                SquareMeters = 90,
                Rooms = 3,
                Floor = 4,
                FloorCount = 6,
                BuildYear = 2013,
                Condition = Condition.Good,
                HasBalcony = true,
                HasElevator = true,
                HasParkingSpace = true,
                HasSecurity = false,
                HasStorageRoom = true,
                MonthlyRentPln = 5500,
                PhotoUrls = new[]
                {
                    "https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?w=800",
                    "https://images.unsplash.com/photo-1484154218962-a197022b5858?w=800",
                    "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=800"
                }
            },
            new SeedListingData
            {
                Title = "Luxury Penthouse for Sale in Śródmieście",
                Description = "<h2>The Ultimate City Living Experience</h2><p>This <strong>exceptional penthouse</strong> represents the pinnacle of luxury living in Warsaw's city center.</p><p><strong>Penthouse Features:</strong></p><ul><li><em>Stunning panoramic views</em> of the city</li><li><strong>Private terrace</strong> with outdoor space</li><li>Premium finishes and materials throughout</li><li>Spacious, open-plan design</li><li>Premium building with concierge</li></ul><p>This is a <strong>truly unique property</strong> offering an unparalleled living experience. The penthouse features high ceilings, floor-to-ceiling windows, and a private terrace with breathtaking views. Located in the heart of the city with immediate access to the best restaurants, shopping, and cultural venues. This is a once-in-a-lifetime opportunity!</p>",
                Category = ListingCategory.Sale,
                PropertyType = PropertyType.Apartment,
                City = "Warsaw",
                District = "Śródmieście",
                AddressLine = "ul. Nowy Świat 50",
                PostalCode = "00-002",
                Latitude = 52.2300m,
                Longitude = 21.0150m,
                SquareMeters = 160,
                Rooms = 4,
                Floor = 20,
                FloorCount = 20,
                BuildYear = 2023,
                Condition = Condition.New,
                HasBalcony = true,
                HasElevator = true,
                HasParkingSpace = true,
                HasSecurity = true,
                HasStorageRoom = true,
                PricePln = 3200000,
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
                Title = "Cozy 1-Bedroom Apartment for Rent in Ochota",
                Description = "<p>This <em>cozy apartment</em> offers comfortable living in a quiet, well-established neighborhood perfect for professionals.</p><p><strong>Comfort Features:</strong></p><ul><li><strong>Well-maintained</strong> building with character</li><li>Comfortable living space</li><li>Quiet street location</li><li>Good transport connections</li><li>Local shops and services nearby</li></ul><p>Ochota is a <em>peaceful district</em> known for its residential character and good quality of life. This apartment is perfect for those who value a quiet living environment while maintaining easy access to the city center. The area offers good local amenities and the building is well-maintained. A comfortable home at a reasonable price!</p>",
                Category = ListingCategory.Rent,
                PropertyType = PropertyType.Apartment,
                City = "Warsaw",
                District = "Ochota",
                AddressLine = "ul. Grójecka 80",
                PostalCode = "02-302",
                Latitude = 52.2100m,
                Longitude = 20.9800m,
                SquareMeters = 42,
                Rooms = 2,
                Floor = 2,
                FloorCount = 5,
                BuildYear = 2015,
                Condition = Condition.Good,
                HasBalcony = true,
                HasElevator = false,
                HasParkingSpace = false,
                HasSecurity = false,
                HasStorageRoom = false,
                MonthlyRentPln = 3200,
                PhotoUrls = new[]
                {
                    "https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?w=800",
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
