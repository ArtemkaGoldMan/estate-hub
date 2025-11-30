import './StaticPages.css';

export const AboutPage = () => {
  return (
    <div className="static-page">
      <div className="static-page__container">
        <h1 className="static-page__title">About EstateHub</h1>
        <div className="static-page__content">
          <section>
            <h2>Our Mission</h2>
            <p>
              EstateHub is dedicated to making property discovery and management simple, transparent, and accessible
              for everyone. We believe that finding the perfect property should be an exciting journey, not a
              frustrating experience.
            </p>
          </section>

          <section>
            <h2>What We Do</h2>
            <p>
              EstateHub is a comprehensive real estate platform that connects property seekers with their ideal
              homes and investment opportunities. Our platform offers:
            </p>
            <ul>
              <li>Extensive property listings with detailed information and high-quality photos</li>
              <li>Interactive map-based search to explore properties by location</li>
              <li>Advanced filtering and search capabilities</li>
              <li>User-friendly tools for property owners to manage their listings</li>
              <li>Secure and transparent transaction processes</li>
            </ul>
          </section>

          <section>
            <h2>Our Values</h2>
            <ul>
              <li>
                <strong>Transparency:</strong> We believe in honest, clear communication and full disclosure of
                property information.
              </li>
              <li>
                <strong>User-Centric:</strong> Our platform is designed with users in mind, prioritizing ease of
                use and functionality.
              </li>
              <li>
                <strong>Innovation:</strong> We continuously improve our platform with the latest technology to
                enhance user experience.
              </li>
              <li>
                <strong>Trust:</strong> We maintain high standards for listings and user interactions to build a
                trusted community.
              </li>
            </ul>
          </section>

          <section>
            <h2>Contact Us</h2>
            <p>
              If you have any questions, suggestions, or need support, please don't hesitate to reach out to our
              team. We're here to help!
            </p>
            <p>
              <strong>Email:</strong> support@estatehub.com
            </p>
          </section>
        </div>
      </div>
    </div>
  );
};

