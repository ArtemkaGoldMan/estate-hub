import { useState, useEffect } from 'react';
import { useNavigate, useSearchParams, Link } from 'react-router-dom';
import { useAuth } from '../../shared/context/AuthContext';
import { Button } from '../../shared/ui';
import './AuthPages.css';

export const EmailConfirmationPage = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { confirmEmail, isAuthenticated } = useAuth();
  const [token, setToken] = useState('');
  const [userId, setUserId] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [isConfirmed, setIsConfirmed] = useState(false);
  
  // Get token and userId from URL params (from email link)
  // Backend sends ?token=xxx&id=yyy in the email link (token is URL-encoded)
  // useSearchParams().get() automatically decodes URL-encoded values
  useEffect(() => {
    const urlToken = searchParams.get('token');
    const urlUserId = searchParams.get('id'); // Backend uses 'id' not 'userId'
    if (urlToken && urlUserId) {
      setToken(urlToken);
      setUserId(urlUserId);
    }
  }, [searchParams]);

  // Redirect if already authenticated and confirmed (use useEffect to avoid conditional hook calls)
  useEffect(() => {
    if (isAuthenticated && isConfirmed) {
      navigate('/listings', { replace: true });
    }
  }, [isAuthenticated, isConfirmed, navigate]);

  if (isAuthenticated && isConfirmed) {
    return null;
  }

  const handleConfirm = async () => {
    if (!token || !userId) {
      setError('Token and User ID are required');
      return;
    }

    setError('');
    setIsLoading(true);

    try {
      await confirmEmail(token, userId);
      setIsConfirmed(true);
      // Redirect to main page after successful confirmation
      setTimeout(() => {
        navigate('/listings');
      }, 2000);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Email confirmation failed. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    await handleConfirm();
  };

  if (isConfirmed) {
    return (
      <div className="auth-page">
        <div className="auth-page__container">
          <div className="auth-page__content">
            <div className="auth-page__success">
              <h1 className="auth-page__title">Email Confirmed!</h1>
              <p className="auth-page__subtitle">Your email has been successfully confirmed.</p>
              <p>Redirecting you to the listings page...</p>
            </div>
          </div>
        </div>
      </div>
    );
  }

  // If token and userId are in URL, show confirmation button
  // Otherwise show message to check email
  if (!token || !userId) {
    return (
      <div className="auth-page">
        <div className="auth-page__container">
          <div className="auth-page__content">
            <h1 className="auth-page__title">Confirm Your Email</h1>
            <p className="auth-page__subtitle">
              Please check your email and click the confirmation link.
            </p>
            <div className="auth-page__footer">
              <p>
                Already confirmed?{' '}
                <Link to="/login" className="auth-page__link">
                  Sign in
                </Link>
              </p>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="auth-page">
      <div className="auth-page__container">
        <div className="auth-page__content">
          <h1 className="auth-page__title">Confirm Your Email</h1>
          <p className="auth-page__subtitle">
            Click the button below to confirm your email address.
          </p>

          <form onSubmit={handleSubmit} className="auth-page__form">
            {error && <div className="auth-page__error">{error}</div>}

            <Button
              type="submit"
              variant="primary"
              fullWidth
              isLoading={isLoading}
              disabled={!token || !userId}
            >
              Confirm Email
            </Button>
          </form>

          <div className="auth-page__footer">
            <p>
              Already confirmed?{' '}
              <Link to="/login" className="auth-page__link">
                Sign in
              </Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

