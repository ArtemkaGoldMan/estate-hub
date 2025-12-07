import { Outlet, useLocation } from 'react-router-dom';
import { useEffect, useState } from 'react';
import { Sidebar, Footer } from '../../widgets/navigation';
import './MainLayout.css';

export const MainLayout = () => {
  const location = useLocation();
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

  // Scroll to top on route change
  useEffect(() => {
    window.scrollTo(0, 0);
  }, [location.pathname]);

  // Close mobile menu on route change
  useEffect(() => {
    setIsMobileMenuOpen(false);
  }, [location.pathname]);

  // Hide footer on map search page
  const showFooter = location.pathname !== '/map';

  return (
    <div className="main-layout">
      <Sidebar
        isMobileOpen={isMobileMenuOpen}
        onMobileClose={() => setIsMobileMenuOpen(false)}
      />
      <div className="main-layout__content">
        {/* Mobile menu button */}
        <button
          className="main-layout__mobile-toggle"
          onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
          aria-label="Toggle menu"
          aria-expanded={isMobileMenuOpen}
          aria-controls="main-navigation"
        >
          <span aria-hidden="true">☰</span>
        </button>
        <div className="main-layout__page">
          <Outlet />
        </div>
        {showFooter && <Footer />}
      </div>
      {/* Mobile overlay */}
      {isMobileMenuOpen && (
        <div
          className="main-layout__overlay"
          onClick={() => setIsMobileMenuOpen(false)}
          aria-hidden="true"
        />
      )}
    </div>
  );
};


