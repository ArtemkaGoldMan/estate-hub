import { useLocation, Link } from 'react-router-dom';
import './AuthPages.css';

export const CheckEmailPage = () => {
  const location = useLocation();
  const email = location.state?.email || 'your email';

  return (
    <div className="auth-page">
      <div className="auth-page__container">
        <div className="auth-page__content">
          <div className="auth-page__success">
            <h1 className="auth-page__title">Check Your Email</h1>
            <p className="auth-page__subtitle">
              We've sent a confirmation link to <strong>{email}</strong>
            </p>
            <p>
              Please check your email and click the confirmation link to activate your account.
            </p>
            <p style={{ marginTop: '2rem', fontSize: '0.9rem', color: '#666' }}>
              Didn't receive the email? Check your spam folder or try registering again.
            </p>
          </div>

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




