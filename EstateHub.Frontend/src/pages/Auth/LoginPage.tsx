import { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../../shared/context/AuthContext';
import { Button, Input } from '../../shared/ui';
import { UserFriendlyError } from '../../shared/lib/errorParser';
import './AuthPages.css';

export const LoginPage = () => {
  const navigate = useNavigate();
  const { login, isAuthenticated } = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  const [isLoading, setIsLoading] = useState(false);

  // Redirect if already authenticated (use useEffect to avoid conditional hook calls)
  useEffect(() => {
    if (isAuthenticated) {
      navigate('/listings', { replace: true });
    }
  }, [isAuthenticated, navigate]);

  if (isAuthenticated) {
    return null;
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setFieldErrors({});
    setIsLoading(true);

    try {
      await login(email, password);
      navigate('/listings');
    } catch (err) {
      if (err instanceof UserFriendlyError) {
        setError(err.userMessage);
        // Map field errors to form fields
        const mappedFieldErrors: Record<string, string> = {};
        if (err.fieldErrors) {
          Object.keys(err.fieldErrors).forEach((field) => {
            const formField = field.toLowerCase();
            if (formField.includes('email')) {
              mappedFieldErrors.email = err.getFieldError(field) || '';
            } else if (formField.includes('password')) {
              mappedFieldErrors.password = err.getFieldError(field) || '';
            }
          });
        }
        setFieldErrors(mappedFieldErrors);
      } else {
        setError(err instanceof Error ? err.message : 'Login failed. Please try again.');
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="auth-page">
      <div className="auth-page__container">
        <div className="auth-page__content">
          <h1 className="auth-page__title">Welcome Back</h1>
          <p className="auth-page__subtitle">Sign in to your account</p>

          <form onSubmit={handleSubmit} className="auth-page__form">
            {error && <div className="auth-page__error">{error}</div>}

            <Input
              type="email"
              label="Email"
              value={email}
              onChange={(e) => {
                setEmail(e.target.value);
                if (fieldErrors.email) {
                  setFieldErrors((prev) => {
                    const next = { ...prev };
                    delete next.email;
                    return next;
                  });
                }
              }}
              required
              fullWidth
              placeholder="Enter your email"
              autoComplete="email"
              error={fieldErrors.email}
            />

            <Input
              type="password"
              label="Password"
              value={password}
              onChange={(e) => {
                setPassword(e.target.value);
                if (fieldErrors.password) {
                  setFieldErrors((prev) => {
                    const next = { ...prev };
                    delete next.password;
                    return next;
                  });
                }
              }}
              required
              fullWidth
              placeholder="Enter your password"
              autoComplete="current-password"
              error={fieldErrors.password}
            />

            <Button
              type="submit"
              variant="primary"
              fullWidth
              isLoading={isLoading}
              disabled={!email || !password}
            >
              Sign In
            </Button>
          </form>

          <div className="auth-page__footer">
            <p>
              Don't have an account?{' '}
              <Link to="/register" className="auth-page__link">
                Sign up
              </Link>
            </p>
            <Link to="/auth/forgot" className="auth-page__link">
              Forgot password?
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
};


