import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useState } from 'react';
import { FaHome, FaBuilding, FaMap, FaChartBar, FaUser, FaClipboardList, FaUsers, FaSignOutAlt, FaSignInAlt, FaUserPlus, FaChevronLeft, FaChevronRight } from 'react-icons/fa';
import { useAuth } from '../../shared/context/AuthContext';
import { getUserRoles, hasPermission, PERMISSIONS } from '../../shared/lib/permissions';
import { Button } from '../../shared/ui';
import logoImage from '../../assets/Logo_Icon.svg';
import './Sidebar.css';

interface NavItem {
  path: string;
  label: string;
  Icon?: React.ComponentType;
  requiresAuth?: boolean;
  requiresAdmin?: boolean;
}

interface SidebarProps {
  isMobileOpen?: boolean;
  onMobileClose?: () => void;
}

export const Sidebar = ({ isMobileOpen = false, onMobileClose }: SidebarProps) => {
  const location = useLocation();
  const navigate = useNavigate();
  const { isAuthenticated, user, logout } = useAuth();
  const [isCollapsed, setIsCollapsed] = useState(false);

  const userRoles = getUserRoles();
  const canManageUsers = hasPermission(userRoles, PERMISSIONS.UserManagement);

  const navItems: NavItem[] = [
    { path: '/', label: 'Home', Icon: FaHome },
    { path: '/listings', label: 'Listings', Icon: FaBuilding },
    { path: '/map', label: 'Map Search', Icon: FaMap },
    { path: '/dashboard', label: 'Dashboard', Icon: FaChartBar, requiresAuth: true },
    { path: '/profile', label: 'Profile', Icon: FaUser, requiresAuth: true },
    { path: '/reports', label: 'Reports', Icon: FaClipboardList, requiresAuth: true },
    { path: '/admin/users', label: 'User Management', Icon: FaUsers, requiresAuth: true, requiresAdmin: true } as NavItem,
  ];

  // Filter items based on authentication and permissions
  const visibleItems = navItems.filter(
    (item) => {
      if (item.requiresAuth && !isAuthenticated) return false;
      if (item.requiresAdmin && !canManageUsers) return false;
      return true;
    }
  );

  const isActive = (path: string) => {
    if (path === '/') {
      return location.pathname === '/';
    }
    if (path === '/listings') {
      return (
        location.pathname === '/listings' ||
        location.pathname.startsWith('/listings/')
      );
    }
    return (
      location.pathname === path ||
      location.pathname.startsWith(`${path}/`)
    );
  };

  const handleLogout = async () => {
    try {
      await logout();
      navigate('/listings', { replace: true });
    } catch (error) {
      // Logout errors are handled silently - user is redirected anyway
    }
  };

  const handleLinkClick = () => {
    // Close mobile menu when a link is clicked
    if (onMobileClose) {
      onMobileClose();
    }
  };

  return (
    <aside
      className={`sidebar ${isCollapsed ? 'sidebar--collapsed' : ''} ${
        isMobileOpen ? 'sidebar--open' : ''
      }`}
    >
      <div className="sidebar__header">
        <Link to="/" className="sidebar__logo">
          <img 
            src={logoImage} 
            alt="EstateHub" 
            className="sidebar__logo-image"
          />
          {!isCollapsed && <span className="sidebar__logo-text">EstateHub</span>}
        </Link>
        <button
          className="sidebar__toggle"
          onClick={() => setIsCollapsed(!isCollapsed)}
          aria-label={isCollapsed ? 'Expand sidebar' : 'Collapse sidebar'}
        >
          {isCollapsed ? <FaChevronRight /> : <FaChevronLeft />}
        </button>
      </div>

      <nav className="sidebar__nav" aria-label="Main navigation" id="main-navigation">
        <ul className="sidebar__menu" role="list">
          {visibleItems.map((item) => (
            <li key={item.path} role="listitem">
              <Link
                to={item.path}
                className={`sidebar__link ${
                  isActive(item.path) ? 'sidebar__link--active' : ''
                }`}
                title={isCollapsed ? item.label : undefined}
                onClick={handleLinkClick}
                aria-current={isActive(item.path) ? 'page' : undefined}
              >
                {item.Icon && (
                  <span className="sidebar__icon" aria-hidden="true">
                    <item.Icon />
                  </span>
                )}
                {!isCollapsed && <span className="sidebar__label">{item.label}</span>}
              </Link>
            </li>
          ))}
        </ul>
      </nav>

      <div className="sidebar__footer">
        {isAuthenticated ? (
          <>
            {!isCollapsed && user && (
              <div className="sidebar__user">
                <span className="sidebar__user-avatar">
                  {user.displayName?.charAt(0).toUpperCase() || user.email.charAt(0).toUpperCase()}
                </span>
                <div className="sidebar__user-info">
                  <span className="sidebar__user-name">
                    {user.displayName || user.email}
                  </span>
                </div>
              </div>
            )}
            <Button
              variant="ghost"
              size="sm"
              onClick={handleLogout}
              className="sidebar__logout"
              title={isCollapsed ? 'Logout' : undefined}
            >
              {!isCollapsed && 'Logout'}
              {isCollapsed && <FaSignOutAlt />}
            </Button>
          </>
        ) : (
          <>
            <Button
              variant="ghost"
              size="sm"
              onClick={() => navigate('/login')}
              className="sidebar__login"
              title={isCollapsed ? 'Login' : undefined}
            >
              {!isCollapsed && 'Login'}
              {isCollapsed && <FaSignInAlt />}
            </Button>
            <Button
              variant="primary"
              size="sm"
              onClick={() => navigate('/register')}
              className="sidebar__signup"
              title={isCollapsed ? 'Sign Up' : undefined}
            >
              {!isCollapsed && 'Sign Up'}
              {isCollapsed && <FaUserPlus />}
            </Button>
          </>
        )}
      </div>
    </aside>
  );
};

