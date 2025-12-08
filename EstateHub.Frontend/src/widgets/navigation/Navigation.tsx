import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../../shared/context/AuthContext';
import { getUserRoles } from '../../shared/lib/permissions';
import { Button } from '../../shared/ui';
import logoImage from '../../assets/Logo_Icon.svg';
import './Navigation.css';

export const Navigation = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const { isAuthenticated, user, logout } = useAuth();
  const userRoles = getUserRoles();
  const isAdmin = userRoles.includes('Admin');

  const navItems = [
    { path: '/listings', label: 'Listings' },
    ...(isAuthenticated ? [{ path: '/dashboard', label: 'Dashboard' }] : []),
  ];

  const handleLogout = async () => {
    try {
      await logout();
      navigate('/listings', { replace: true });
    } catch {
      // Logout errors are handled silently - user is redirected anyway
    }
  };

  const handleLoginClick = (e: React.MouseEvent<HTMLButtonElement>) => {
    e.preventDefault();
    navigate('/login', { replace: false });
  };

  const handleSignUpClick = (e: React.MouseEvent<HTMLButtonElement>) => {
    e.preventDefault();
    navigate('/register', { replace: false });
  };

  const isActive = (path: string) => {
    if (path === '/listings') {
      return location.pathname === '/' || location.pathname === '/listings' || location.pathname.startsWith('/listings/');
    }
    return location.pathname === path || location.pathname.startsWith(`${path}/`);
  };

  return (
    <nav className="navigation">
      <div className="navigation__container">
        <Link 
          to="/" 
          className="navigation__logo"
        >
          <img 
            src={logoImage} 
            alt="EstateHub" 
            className="navigation__logo-image"
          />
          <span className="navigation__logo-text">EstateHub</span>
        </Link>
        <ul className="navigation__menu">
          {navItems.map((item) => (
            <li key={item.path}>
              <Link
                to={item.path}
                className={`navigation__link ${
                  isActive(item.path) ? 'navigation__link--active' : ''
                }`}
              >
                {item.label}
              </Link>
            </li>
          ))}
        </ul>
        <div className="navigation__actions">
          {isAuthenticated ? (
            <>
              {user && (
                <div className="navigation__user-section">
                  <Link
                    to="/profile"
                    className={`navigation__link ${
                      isActive('/profile') ? 'navigation__link--active' : ''
                    }`}
                  >
                    {user.displayName || user.email}
                  </Link>
                  {isAdmin && (
                    <span className="navigation__admin-badge" title="Administrator">
                      Admin
                    </span>
                  )}
                </div>
              )}
              <Button variant="ghost" size="sm" onClick={handleLogout}>
                Logout
              </Button>
            </>
          ) : (
            <>
              <Button variant="ghost" size="sm" onClick={handleLoginClick}>
                Login
              </Button>
              <Button variant="primary" size="sm" onClick={handleSignUpClick}>
                Sign Up
              </Button>
            </>
          )}
        </div>
      </div>
    </nav>
  );
};

