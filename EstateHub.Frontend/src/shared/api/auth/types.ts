export interface UserRegistrationRequest {
  email: string;
  password: string;
  confirmPassword: string;
  callbackUrl: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface ConfirmEmailRequest {
  token: string;
  userId: string;
}

export interface AuthenticationResponse {
  id: string;
  email: string;
  displayName: string;
  role: string;
  accessToken: string;
  avatar?: string;
}


