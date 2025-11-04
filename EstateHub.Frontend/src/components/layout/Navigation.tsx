import { Link, useLocation } from 'react-router-dom';
import './Navigation.css';

export const Navigation = () => {
  const location = useLocation();

  const navItems = [
    { path: '/', label: 'Home' },
    { path: '/components', label: 'Components' },
  ];

  return (
    <nav className="navigation">
      <div className="navigation__container">
        <Link to="/" className="navigation__logo">
          EstateHub
        </Link>
        <ul className="navigation__menu">
          {navItems.map((item) => (
            <li key={item.path}>
              <Link
                to={item.path}
                className={`navigation__link ${
                  location.pathname === item.path ? 'navigation__link--active' : ''
                }`}
              >
                {item.label}
              </Link>
            </li>
          ))}
        </ul>
      </div>
    </nav>
  );
};

