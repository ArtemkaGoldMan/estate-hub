import React, { useState, useRef, useEffect } from 'react';
import './Dropdown.css';

export interface DropdownOption {
  value: string | number;
  label: string;
  disabled?: boolean;
}

export interface DropdownProps {
  options: DropdownOption[];
  value?: string | number;
  onChange?: (value: string | number) => void;
  placeholder?: string;
  disabled?: boolean;
  className?: string;
  label?: string;
  error?: string;
}

export const Dropdown: React.FC<DropdownProps> = ({
  options,
  value,
  onChange,
  placeholder = 'Select an option...',
  disabled = false,
  className = '',
  label,
  error,
}) => {
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  const selectedOption = options.find((opt) => opt.value === value);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };

    if (isOpen) {
      document.addEventListener('mousedown', handleClickOutside);
    }

    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, [isOpen]);

  const handleSelect = (option: DropdownOption) => {
    if (option.disabled) return;
    onChange?.(option.value);
    setIsOpen(false);
  };

  const classes = [
    'dropdown',
    isOpen && 'dropdown--open',
    disabled && 'dropdown--disabled',
    error && 'dropdown--error',
    className,
  ]
    .filter(Boolean)
    .join(' ');

  return (
    <div className={classes} ref={dropdownRef}>
      {label && <label className="dropdown__label">{label}</label>}
      <div
        className="dropdown__trigger"
        onClick={() => !disabled && setIsOpen(!isOpen)}
        role="button"
        tabIndex={disabled ? -1 : 0}
        onKeyDown={(e) => {
          if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault();
            if (!disabled) setIsOpen(!isOpen);
          }
        }}
      >
        <span className="dropdown__value">
          {selectedOption ? selectedOption.label : placeholder}
        </span>
        <svg
          className={`dropdown__icon ${isOpen ? 'dropdown__icon--open' : ''}`}
          width="12"
          height="12"
          viewBox="0 0 12 12"
          fill="none"
          xmlns="http://www.w3.org/2000/svg"
        >
          <path
            d="M2 4L6 8L10 4"
            stroke="currentColor"
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
          />
        </svg>
      </div>
      {isOpen && (
        <div className="dropdown__menu">
          {options.length === 0 ? (
            <div className="dropdown__empty">No options available</div>
          ) : (
            options.map((option) => (
              <div
                key={option.value}
                className={`dropdown__option ${
                  option.value === value ? 'dropdown__option--selected' : ''
                } ${option.disabled ? 'dropdown__option--disabled' : ''}`}
                onClick={() => handleSelect(option)}
                role="option"
                aria-selected={option.value === value}
              >
                {option.label}
              </div>
            ))
          )}
        </div>
      )}
      {error && <span className="dropdown__error">{error}</span>}
    </div>
  );
};

