import { useState } from 'react';
import { Link } from 'react-router-dom';
import { authApi } from '../../shared/api/auth/authApi';
import { Button, Input } from '../../shared/ui';
import { UserFriendlyError } from '../../shared/lib/errorParser';
import './AuthPages.css';

export const ForgotPasswordPage = () => {
  const [email, setEmail] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setIsLoading(true);

    try {
      await authApi.forgotPassword(email);
      setIsSuccess(true);
    } catch (err) {
      if (err instanceof UserFriendlyError) {
        setError(err.userMessage);
      } else {
        setError(err instanceof Error ? err.message : 'Failed to send password reset email. Please try again.');
      }
    } finally {
      setIsLoading(false);
    }
  };

  if (isSuccess) {
    return (
      <div className="auth-page">
        <div className="auth-page__container">
          <div className="auth-page__content">
            <h1 className="auth-page__title">Check Your Email</h1>
            <p className="auth-page__subtitle">
              We've sent a password reset link to <strong>{email}</strong>
            </p>
            <p className="auth-page__text">
              Please check your email and click the link to reset your password. The link will expire in 24 hours.
            </p>
            <div className="auth-page__actions">
              <Link to="/login" className="auth-page__link">
                Back to Login
              </Link>
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
          <h1 className="auth-page__title">Forgot Password</h1>
          <p className="auth-page__subtitle">Enter your email to receive a password reset link</p>

          <form onSubmit={handleSubmit} className="auth-page__form">
            {error && <div className="auth-page__error">{error}</div>}

            <Input
              type="email"
              label="Email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              disabled={isLoading}
              placeholder="your.email@example.com"
              fullWidth
            />

            <Button
              type="submit"
              variant="primary"
              fullWidth
              isLoading={isLoading}
              disabled={!email || isLoading}
            >
              Send Reset Link
            </Button>
          </form>

          <div className="auth-page__footer">
            <Link to="/login" className="auth-page__link">
              Back to Login
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
};

