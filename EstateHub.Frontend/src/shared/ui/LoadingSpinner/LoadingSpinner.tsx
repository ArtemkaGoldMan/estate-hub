import React from 'react';
import './LoadingSpinner.css';

export interface LoadingSpinnerProps {
  size?: 'small' | 'medium' | 'large';
  className?: string;
  fullScreen?: boolean;
  text?: string;
}

export const LoadingSpinner: React.FC<LoadingSpinnerProps> = ({
  size = 'medium',
  className = '',
  fullScreen = false,
  text,
}) => {
  const classes = [
    'loading-spinner',
    `loading-spinner--${size}`,
    fullScreen && 'loading-spinner--full-screen',
    className,
  ]
    .filter(Boolean)
    .join(' ');

  const content = (
    <div className={classes}>
      <div className="loading-spinner__circle" />
      {text && <p className="loading-spinner__text">{text}</p>}
    </div>
  );

  if (fullScreen) {
    return (
      <div className="loading-spinner__overlay">
        {content}
      </div>
    );
  }

  return content;
};

