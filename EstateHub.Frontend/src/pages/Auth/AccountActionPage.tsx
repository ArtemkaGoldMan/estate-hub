import { useState, useEffect } from 'react';
import { useNavigate, useSearchParams, Link } from 'react-router-dom';
import { authApi } from '../../shared/api/auth/authApi';
import { Button } from '../../shared/ui';
import './AuthPages.css';

export const AccountActionPage = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [userId, setUserId] = useState('');
  const [token, setToken] = useState('');
  const [actionType, setActionType] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);

  useEffect(() => {
    const userIdParam = searchParams.get('userId');
    const tokenParam = searchParams.get('token');
    const actionParam = searchParams.get('actionType');

    if (userIdParam && tokenParam && actionParam) {
      setUserId(userIdParam);
      setToken(tokenParam);
      setActionType(actionParam);
    } else {
      setError('Invalid confirmation link. Please check your email for the correct link.');
    }
  }, [searchParams]);

  const handleConfirm = async () => {
    setError('');
    setIsLoading(true);

    try {
      await authApi.confirmAccountAction(userId, token, actionType);
      setIsSuccess(true);
      setTimeout(() => {
        navigate('/login');
      }, 3000);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to confirm account action. The link may have expired.');
    } finally {
      setIsLoading(false);
    }
  };

  const getActionLabel = (action: string) => {
    switch (action) {
      case 'Recover':
        return 'Account Recovery';
      case 'HardDelete':
        return 'Account Deletion';
      default:
        return 'Account Action';
    }
  };

  const getActionDescription = (action: string) => {
    switch (action) {
      case 'Recover':
        return 'Your account will be recovered and you will be able to log in again.';
      case 'HardDelete':
        return 'Your account will be permanently deleted. This action cannot be undone.';
      default:
        return 'This action will be processed.';
    }
  };

  if (isSuccess) {
    return (
      <div className="auth-page">
        <div className="auth-page__container">
          <div className="auth-page__content">
            <h1 className="auth-page__title">Action Confirmed</h1>
            <p className="auth-page__subtitle">
              Your {getActionLabel(actionType).toLowerCase()} has been confirmed successfully.
            </p>
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

  if (!userId || !token || !actionType) {
    return (
      <div className="auth-page">
        <div className="auth-page__container">
          <div className="auth-page__content">
            <h1 className="auth-page__title">Invalid Confirmation Link</h1>
            <p className="auth-page__subtitle">This confirmation link is invalid or has expired.</p>
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
          <h1 className="auth-page__title">{getActionLabel(actionType)}</h1>
          <p className="auth-page__subtitle">Confirm your account action</p>
          <p className="auth-page__text">{getActionDescription(actionType)}</p>

          {error && <div className="auth-page__error">{error}</div>}

          <div className="auth-page__actions">
            <Button
              variant={actionType === 'HardDelete' ? 'danger' : 'primary'}
              fullWidth
              onClick={handleConfirm}
              isLoading={isLoading}
              disabled={isLoading}
            >
              Confirm {getActionLabel(actionType)}
            </Button>
            <Link to="/login" className="auth-page__link">
              Cancel
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
};

