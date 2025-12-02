import { Link } from 'react-router-dom';
import { Button } from '../../shared/ui';
import { useAuth } from '../../shared/context/AuthContext';
import logoImage from '../../assets/Logo-optimized.png';
import './HomePage.css';

export const HomePage = () => {
  const { isAuthenticated } = useAuth();

  return (
    <div className="home-page">
      {/* Hero Section */}
      <section className="home-page__hero">
        <div className="home-page__hero-content">
          <div className="home-page__logo">
            <img 
              src={logoImage} 
              alt="EstateHub" 
              className="home-page__logo-image"
            />
          </div>
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

      {/* CTA Section */}
      <section className="home-page__cta">
        <div className="home-page__container">
          {isAuthenticated ? (
            <>
              <h2 className="home-page__cta-title">Continue Exploring</h2>
              <p className="home-page__cta-text">
                Discover new properties, manage your listings, or explore the map to find your next home.
              </p>
              <div className="home-page__cta-actions">
                <Link to="/dashboard">
                  <Button variant="primary" size="lg">
                    Go to Dashboard
                  </Button>
                </Link>
                <Link to="/listings/new">
                  <Button variant="outline" size="lg">
                    Create Listing
                  </Button>
                </Link>
              </div>
            </>
          ) : (
            <>
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
            </>
          )}
        </div>
      </section>
    </div>
  );
};

