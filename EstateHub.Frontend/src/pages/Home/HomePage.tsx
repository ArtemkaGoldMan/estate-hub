import { Link } from 'react-router-dom';
import { useListingsQuery } from '../../entities/listing';
import { ListingCard } from '../../entities/listing/ui';
import { Button, LoadingSpinner } from '../../shared/ui';
import './HomePage.css';

export const HomePage = () => {
  // Get featured listings (first 6 published listings)
  const { data, loading, error } = useListingsQuery({
    filter: {},
    page: 1,
    pageSize: 6,
  });

  const featuredListings = data?.items || [];

  return (
    <div className="home-page">
      {/* Hero Section */}
      <section className="home-page__hero">
        <div className="home-page__hero-content">
          <h1 className="home-page__title">Find Your Perfect Property</h1>
          <p className="home-page__subtitle">
            Discover thousands of properties across Poland. Buy, rent, or list your property with
            EstateHub.
          </p>
          <div className="home-page__hero-actions">
            <Link to="/listings">
              <Button variant="primary" size="lg">
                Browse Listings
              </Button>
            </Link>
            <Link to="/map">
              <Button variant="outline" size="lg">
                Explore Map
              </Button>
            </Link>
          </div>
        </div>
      </section>

      {/* Features Section */}
      <section className="home-page__features">
        <div className="home-page__container">
          <h2 className="home-page__section-title">Why Choose EstateHub?</h2>
          <div className="home-page__features-grid">
            <div className="home-page__feature">
              <div className="home-page__feature-icon">🔍</div>
              <h3>Advanced Search</h3>
              <p>Filter by price, location, size, and amenities to find exactly what you're looking for.</p>
            </div>
            <div className="home-page__feature">
              <div className="home-page__feature-icon">🗺️</div>
              <h3>Interactive Maps</h3>
              <p>Explore properties on an interactive map to see locations and neighborhoods.</p>
            </div>
            <div className="home-page__feature">
              <div className="home-page__feature-icon">📸</div>
              <h3>Rich Media</h3>
              <p>View high-quality photos and detailed information for every property.</p>
            </div>
            <div className="home-page__feature">
              <div className="home-page__feature-icon">🔒</div>
              <h3>Secure Platform</h3>
              <p>Your data and transactions are protected with industry-standard security.</p>
            </div>
          </div>
        </div>
      </section>

      {/* Featured Listings Section */}
      {!error && (
        <section className="home-page__listings">
          <div className="home-page__container">
            <div className="home-page__listings-header">
              <h2 className="home-page__section-title">Featured Properties</h2>
              <Link to="/listings">
                <Button variant="ghost">View All →</Button>
              </Link>
            </div>

            {loading && (
              <div className="home-page__loading">
                <LoadingSpinner />
                <p>Loading featured properties...</p>
              </div>
            )}

            {!loading && featuredListings.length > 0 && (
              <div className="home-page__listings-grid">
                {featuredListings.map((listing) => (
                  <ListingCard key={listing.id} listing={listing} />
                ))}
              </div>
            )}

            {!loading && featuredListings.length === 0 && (
              <div className="home-page__empty">
                <p>No properties available at the moment.</p>
                <Link to="/listings/new">
                  <Button variant="primary">List Your Property</Button>
                </Link>
              </div>
            )}
          </div>
        </section>
      )}

      {/* CTA Section */}
      <section className="home-page__cta">
        <div className="home-page__container">
          <h2 className="home-page__cta-title">Ready to Get Started?</h2>
          <p className="home-page__cta-text">
            Join thousands of users finding and listing properties on EstateHub.
          </p>
          <div className="home-page__cta-actions">
            <Link to="/register">
              <Button variant="primary" size="lg">
                Create Account
              </Button>
            </Link>
            <Link to="/listings">
              <Button variant="outline" size="lg">
                Browse Properties
              </Button>
            </Link>
          </div>
        </div>
      </section>
    </div>
  );
};

