const getEnv = (key: string, fallback?: string) => {
  const value = import.meta.env[`VITE_${key}`];
  if (value) {
    return value;
  }

  if (fallback !== undefined) {
    return fallback;
  }

  // Only warn in development
  if (import.meta.env.DEV) {
    console.warn(`Environment variable VITE_${key} is not defined`);
  }
  return '';
};

export const API_CONFIG = {
  graphqlUrl: getEnv('LISTING_SERVICE_GRAPHQL_URL', '/graphql'),
  assetsBaseUrl: getEnv('LISTING_SERVICE_ASSETS_URL', '/api/photo'),
  authorizationApiUrl: getEnv('AUTHORIZATION_SERVICE_URL', '/'),
};


