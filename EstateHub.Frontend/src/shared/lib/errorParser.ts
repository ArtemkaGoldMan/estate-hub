/**
 * Error parser utility to extract user-friendly messages from backend ProblemDetails responses
 */

export interface ParsedError {
  userMessage: string;
  fieldErrors?: Record<string, string[]>;
  errorCode?: string;
  technicalMessage?: string;
  statusCode?: number;
}

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  extensions?: {
    userMessage?: string;
    fieldErrors?: Record<string, string[]>;
    errorCode?: string;
    error?: {
      Code?: string;
      Description?: string;
    };
    [key: string]: unknown;
  };
  [key: string]: unknown;
}

/**
 * Parses a ProblemDetails response from the backend to extract user-friendly error information
 */
export async function parseErrorResponse(response: Response): Promise<ParsedError> {
  let problemDetails: ProblemDetails | null = null;

  try {
    const contentType = response.headers.get('content-type');
    if (contentType && contentType.includes('application/json')) {
      problemDetails = await response.json();
    }
  } catch {
    // If parsing fails, fall back to default error
  }

  // Extract user message from extensions
  const userMessage =
    problemDetails?.extensions?.userMessage ||
    problemDetails?.detail ||
    `Request failed with status ${response.status}`;

  // Extract field errors from extensions
  const fieldErrors = problemDetails?.extensions?.fieldErrors as
    | Record<string, string[]>
    | undefined;

  // Extract error code
  const errorCode =
    problemDetails?.extensions?.errorCode ||
    problemDetails?.extensions?.error?.Code;

  // Extract technical message
  const technicalMessage = problemDetails?.detail || problemDetails?.title;

  return {
    userMessage,
    fieldErrors,
    errorCode,
    technicalMessage,
    statusCode: problemDetails?.status || response.status,
  };
}

/**
 * Creates a user-friendly error from a parsed error response
 */
export class UserFriendlyError extends Error {
  public readonly userMessage: string;
  public readonly fieldErrors?: Record<string, string[]>;
  public readonly errorCode?: string;
  public readonly statusCode?: number;

  constructor(parsedError: ParsedError) {
    super(parsedError.userMessage);
    this.name = 'UserFriendlyError';
    this.userMessage = parsedError.userMessage;
    this.fieldErrors = parsedError.fieldErrors;
    this.errorCode = parsedError.errorCode;
    this.statusCode = parsedError.statusCode;
  }

  /**
   * Gets the error message for a specific field
   */
  getFieldError(fieldName: string): string | undefined {
    if (!this.fieldErrors || !this.fieldErrors[fieldName]) {
      return undefined;
    }
    return this.fieldErrors[fieldName][0]; // Return first error for the field
  }

  /**
   * Gets all error messages for a specific field
   */
  getFieldErrors(fieldName: string): string[] {
    return this.fieldErrors?.[fieldName] || [];
  }

  /**
   * Checks if there are any field errors
   */
  hasFieldErrors(): boolean {
    return !!this.fieldErrors && Object.keys(this.fieldErrors).length > 0;
  }
}

/**
 * Helper function to parse and throw a user-friendly error from a fetch response
 */
export async function throwUserFriendlyError(response: Response): Promise<never> {
  const parsedError = await parseErrorResponse(response);
  throw new UserFriendlyError(parsedError);
}

