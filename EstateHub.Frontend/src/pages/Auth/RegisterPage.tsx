import { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../../shared/context/AuthContext';
import { Button, Input } from '../../shared/ui';
import { UserFriendlyError } from '../../shared/lib/errorParser';
import './AuthPages.css';

export const RegisterPage = () => {
  const navigate = useNavigate();
  const { register, isAuthenticated, isLoading: authLoading } = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState('');
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  const [isLoading, setIsLoading] = useState(false);

  // Redirect if already authenticated (use useEffect to avoid conditional hook calls)
  // Only redirect after auth has finished loading
  useEffect(() => {
    if (!authLoading && isAuthenticated) {
      navigate('/listings', { replace: true });
    }
  }, [isAuthenticated, authLoading, navigate]);

  if (isAuthenticated) {
    return null;
  }

  const validatePassword = (pwd: string): string | null => {
    if (pwd.length < 6) {
      return 'Password must be at least 6 characters long';
    }
    return null;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setFieldErrors({});

    // Validation
    if (password !== confirmPassword) {
      setError('Passwords do not match');
      return;
    }

    const passwordError = validatePassword(password);
    if (passwordError) {
      setError(passwordError);
      return;
    }

    setIsLoading(true);

    try {
      const result = await register(email, password, confirmPassword);
      // If user is auto-logged in (result is not null), redirect to listings
      if (result) {
        navigate('/listings');
        return;
      }
      // Otherwise, redirect to check email page
      navigate('/check-email', {
        state: { email },
      });
    } catch (err) {
      if (err instanceof UserFriendlyError) {
        setError(err.userMessage);
        // Map field errors to form fields
        const mappedFieldErrors: Record<string, string> = {};
        if (err.fieldErrors) {
          Object.keys(err.fieldErrors).forEach((field) => {
            // Map backend field names to form field names
            const formField = field.toLowerCase();
            if (formField.includes('email')) {
              mappedFieldErrors.email = err.getFieldError(field) || '';
            } else if (formField.includes('password')) {
              mappedFieldErrors.password = err.getFieldError(field) || '';
            } else if (formField.includes('confirmpassword')) {
              mappedFieldErrors.confirmPassword = err.getFieldError(field) || '';
            }
          });
        }
        setFieldErrors(mappedFieldErrors);
      } else {
        setError(err instanceof Error ? err.message : 'Registration failed. Please try again.');
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="auth-page">
      <div className="auth-page__container">
        <div className="auth-page__content">
          <h1 className="auth-page__title">Create Account</h1>
          <p className="auth-page__subtitle">Sign up to get started</p>

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
              autoComplete="new-password"
              helperText="Must be at least 12 characters and contain uppercase, lowercase, number, and special character"
              error={fieldErrors.password}
            />

            <Input
              type="password"
              label="Confirm Password"
              value={confirmPassword}
              onChange={(e) => {
                setConfirmPassword(e.target.value);
                if (fieldErrors.confirmPassword) {
                  setFieldErrors((prev) => {
                    const next = { ...prev };
                    delete next.confirmPassword;
                    return next;
                  });
                }
              }}
              required
              fullWidth
              placeholder="Confirm your password"
              autoComplete="new-password"
              error={fieldErrors.confirmPassword}
            />

            <Button
              type="submit"
              variant="primary"
              fullWidth
              isLoading={isLoading}
              disabled={!email || !password || !confirmPassword}
            >
              Sign Up
            </Button>
          </form>

          <div className="auth-page__footer">
            <p>
              Already have an account?{' '}
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


