import { Link } from 'react-router-dom';
import './Footer.css';

export const Footer = () => {
  const currentYear = new Date().getFullYear();

  return (
    <footer className="footer">
      <div className="footer__container">
        <div className="footer__content">
          <div className="footer__section">
            <h3 className="footer__title">EstateHub</h3>
            <p className="footer__description">
              Your trusted platform for finding and listing real estate properties in Poland.
            </p>
          </div>

          <div className="footer__section">
            <h4 className="footer__section-title">Quick Links</h4>
            <ul className="footer__links">
              <li>
                <Link to="/listings">Browse Listings</Link>
              </li>
              <li>
                <Link to="/map">Map Search</Link>
              </li>
              <li>
                <Link to="/dashboard">Dashboard</Link>
              </li>
            </ul>
          </div>

          <div className="footer__section">
            <h4 className="footer__section-title">Information</h4>
            <ul className="footer__links">
              <li>
                <Link to="/about">About Us</Link>
              </li>
              <li>
                <Link to="/terms">Terms of Service</Link>
              </li>
              <li>
                <Link to="/privacy">Privacy Policy</Link>
              </li>
            </ul>
          </div>

          <div className="footer__section">
            <h4 className="footer__section-title">Support</h4>
            <ul className="footer__links">
              <li>
                <Link to="/reports">Report Center</Link>
              </li>
              <li>
                <a href="mailto:support@estatehub.com">Contact Support</a>
              </li>
            </ul>
          </div>
        </div>

        <div className="footer__bottom">
          <p className="footer__copyright">
            © {currentYear} EstateHub. All rights reserved.
          </p>
        </div>
      </div>
    </footer>
  );
};

