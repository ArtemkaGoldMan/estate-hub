import { useState, useEffect } from 'react';
import { useNavigate, useSearchParams, Link } from 'react-router-dom';
import { authApi } from '../../shared/api/auth/authApi';
import { Button, Input } from '../../shared/ui';
import { UserFriendlyError } from '../../shared/lib/errorParser';
import './AuthPages.css';

export const ResetPasswordPage = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [userId, setUserId] = useState('');
  const [token, setToken] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);

  useEffect(() => {
    const userIdParam = searchParams.get('userId');
    const tokenParam = searchParams.get('token');

    if (userIdParam && tokenParam) {
      setUserId(userIdParam);
      setToken(tokenParam);
    } else {
      setError('Invalid reset link. Please request a new password reset.');
    }
  }, [searchParams]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (password !== confirmPassword) {
      setError('Passwords do not match');
      return;
    }

    if (password.length < 8) {
      setError('Password must be at least 8 characters long');
      return;
    }

    setIsLoading(true);

    try {
      await authApi.resetPassword(userId, token, password, confirmPassword);
      setIsSuccess(true);
      setTimeout(() => {
        navigate('/login');
      }, 3000);
    } catch (err) {
      if (err instanceof UserFriendlyError) {
        setError(err.userMessage);
      } else {
        setError(err instanceof Error ? err.message : 'Failed to reset password. The link may have expired.');
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
            <h1 className="auth-page__title">Password Reset Successful</h1>
            <p className="auth-page__subtitle">Your password has been reset successfully.</p>
            <p className="auth-page__text">Redirecting to login page...</p>
            <div className="auth-page__actions">
              <Link to="/login" className="auth-page__link">
                Go to Login
              </Link>
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (!userId || !token) {
    return (
      <div className="auth-page">
        <div className="auth-page__container">
          <div className="auth-page__content">
            <h1 className="auth-page__title">Invalid Reset Link</h1>
            <p className="auth-page__subtitle">This password reset link is invalid or has expired.</p>
            <div className="auth-page__actions">
              <Link to="/auth/forgot" className="auth-page__link">
                Request New Reset Link
              </Link>
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
          <h1 className="auth-page__title">Reset Password</h1>
          <p className="auth-page__subtitle">Enter your new password</p>

          <form onSubmit={handleSubmit} className="auth-page__form">
            {error && <div className="auth-page__error">{error}</div>}

            <Input
              type="password"
              label="New Password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              disabled={isLoading}
              placeholder="At least 8 characters"
              fullWidth
              helperText="Password must be at least 8 characters long"
            />

            <Input
              type="password"
              label="Confirm Password"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              required
              disabled={isLoading}
              placeholder="Confirm your password"
              fullWidth
            />

            <Button
              type="submit"
              variant="primary"
              fullWidth
              isLoading={isLoading}
              disabled={!password || !confirmPassword || isLoading}
            >
              Reset Password
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

