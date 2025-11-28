/**
 * Form validation utilities
 */

export interface ValidationErrors {
  [key: string]: string;
}

export interface ValidationRule {
  required?: boolean;
  minLength?: number;
  maxLength?: number;
  min?: number;
  max?: number;
  pattern?: RegExp;
  custom?: (value: any) => string | null;
}

export interface ValidationSchema {
  [field: string]: ValidationRule;
}

/**
 * Validate a single field value against a rule
 */
export const validateField = (
  value: any,
  rule: ValidationRule,
  fieldName: string
): string | null => {
  // Required check
  if (rule.required) {
    if (value === null || value === undefined || value === '') {
      return `${fieldName} is required`;
    }
  }

  // Skip other validations if value is empty and not required
  if (value === null || value === undefined || value === '') {
    return null;
  }

  // String validations
  if (typeof value === 'string') {
    if (rule.minLength && value.length < rule.minLength) {
      return `${fieldName} must be at least ${rule.minLength} characters`;
    }
    if (rule.maxLength && value.length > rule.maxLength) {
      return `${fieldName} must be no more than ${rule.maxLength} characters`;
    }
    if (rule.pattern && !rule.pattern.test(value)) {
      return `${fieldName} format is invalid`;
    }
  }

  // Number validations
  if (typeof value === 'number') {
    if (rule.min !== undefined && value < rule.min) {
      return `${fieldName} must be at least ${rule.min}`;
    }
    if (rule.max !== undefined && value > rule.max) {
      return `${fieldName} must be no more than ${rule.max}`;
    }
  }

  // Custom validation
  if (rule.custom) {
    const customError = rule.custom(value);
    if (customError) {
      return customError;
    }
  }

  return null;
};

/**
 * Validate an object against a schema
 */
export const validate = (
  data: Record<string, any>,
  schema: ValidationSchema
): ValidationErrors => {
  const errors: ValidationErrors = {};

  for (const [field, rule] of Object.entries(schema)) {
    const value = data[field];
    const error = validateField(value, rule, field);
    if (error) {
      errors[field] = error;
    }
  }

  return errors;
};

/**
 * Common validation rules
 */
export const commonRules = {
  required: { required: true },
  email: {
    required: true,
    pattern: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
  },
  password: {
    required: true,
    minLength: 8,
  },
  positiveNumber: {
    required: true,
    min: 1,
  },
  nonNegativeNumber: {
    required: true,
    min: 0,
  },
};

