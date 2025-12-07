import { useEffect } from 'react';
import clsx from 'clsx';
import { FaCheck, FaTimes, FaExclamationTriangle, FaInfoCircle } from 'react-icons/fa';
import './Toast.css';

export type ToastType = 'success' | 'error' | 'info' | 'warning';

export interface Toast {
  id: string;
  message: string;
  type: ToastType;
  duration?: number;
}

interface ToastProps {
  toast: Toast;
  onClose: (id: string) => void;
}

export const ToastItem = ({ toast, onClose }: ToastProps) => {
  const duration = toast.duration ?? 5000; // Default 5 seconds

  useEffect(() => {
    const timer = setTimeout(() => {
      onClose(toast.id);
    }, duration);

    return () => clearTimeout(timer);
  }, [toast.id, duration, onClose]);

  const handleClose = () => {
    onClose(toast.id);
  };

  const getIcon = () => {
    switch (toast.type) {
      case 'success':
        return <FaCheck />;
      case 'error':
        return <FaTimes />;
      case 'warning':
        return <FaExclamationTriangle />;
      case 'info':
        return <FaInfoCircle />;
      default:
        return null;
    }
  };

  return (
    <div
      className={clsx('toast', `toast--${toast.type}`)}
      role="alert"
      aria-live="polite"
    >
      <div className="toast__content">
        <span className="toast__icon">{getIcon()}</span>
        <span className="toast__message">{toast.message}</span>
      </div>
      <button
        className="toast__close"
        onClick={handleClose}
        aria-label="Close notification"
      >
        <FaTimes />
      </button>
    </div>
  );
};

