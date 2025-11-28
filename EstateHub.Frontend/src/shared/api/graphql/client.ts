import { ApolloClient, InMemoryCache } from '@apollo/client';
import { API_CONFIG } from '../../config';
import { UploadLink } from './uploadLink';

const uploadLink = new UploadLink(API_CONFIG.graphqlUrl);

// Use upload link for file uploads, http link for regular queries
// The UploadLink will check for files and route accordingly
export const graphqlClient = new ApolloClient({
  link: uploadLink, // UploadLink handles both file uploads and regular queries
  cache: new InMemoryCache(),
  devtools: {
    enabled: import.meta.env.DEV,
  },
});


