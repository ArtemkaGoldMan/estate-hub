import { ApolloLink, Observable } from '@apollo/client';
import type { Operation, FetchResult } from '@apollo/client';
import { print } from 'graphql';
import { authApi } from '../auth';

/**
 * Custom Apollo Link for handling file uploads with HotChocolate GraphQL
 * HotChocolate uses multipart/form-data for file uploads
 */
export class UploadLink extends ApolloLink {
  private uri: string;

  constructor(uri: string) {
    super();
    this.uri = uri;
  }

  request(operation: Operation): Observable<FetchResult> {
    return new Observable((observer) => {
      const { variables } = operation;
      
      // Check if operation contains a file
      const hasFile = this.hasFile(variables);
      
      if (hasFile) {
        // Use multipart/form-data for file uploads
        this.handleFileUpload(operation, observer);
      } else {
        // Use standard JSON request
        this.handleJsonRequest(operation, observer);
      }
    });
  }

  private hasFile(variables: Record<string, unknown> | undefined): boolean {
    if (!variables) return false;
    
    for (const value of Object.values(variables)) {
      if (value instanceof File || value instanceof FileList) {
        return true;
      }
      if (Array.isArray(value)) {
        if (value.some(item => item instanceof File)) {
          return true;
        }
      }
    }
    return false;
  }

  private async handleFileUpload(
    operation: Operation,
    observer: { next: (value: FetchResult) => void; error: (error: Error) => void; complete: () => void }
  ): Promise<void> {
    try {
      const { query, variables, operationName } = operation;
      const formData = new FormData();
      
      // Extract file variables and create variable map
      const fileMap: Record<string, File> = {};
      const variableMap: Record<string, string[]> = {};
      let fileIndex = 0;
      const processedVariables: Record<string, unknown> = {};
      
      if (variables) {
        for (const [key, value] of Object.entries(variables)) {
          if (value instanceof File) {
            const fileKey = `${fileIndex}`;
            variableMap[fileKey] = [`variables.${key}`];
            fileMap[fileKey] = value;
            processedVariables[key] = null; // Placeholder, will be replaced by map
            fileIndex++;
          } else {
            processedVariables[key] = value;
          }
        }
      }
      
      // Add operations according to GraphQL multipart request spec
      const operations = {
        query: print(query),
        variables: processedVariables,
        operationName,
      };
      formData.append('operations', JSON.stringify(operations));
      
      // Add map
      if (Object.keys(variableMap).length > 0) {
        formData.append('map', JSON.stringify(variableMap));
      }
      
      // Append files
      for (const [fileKey, file] of Object.entries(fileMap)) {
        formData.append(fileKey, file, file.name);
      }
      
      // Get auth token
      const token = localStorage.getItem('estatehub_access_token');
      const headers: HeadersInit = {};
      if (token) {
        headers['Authorization'] = `Bearer ${token}`;
      }
      
      const response = await fetch(this.uri, {
        method: 'POST',
        headers,
        credentials: 'include',
        body: formData,
      });
      
      if (!response.ok) {
        let errorText = '';
        try {
          errorText = await response.text();
        } catch {
          errorText = `HTTP ${response.status}: ${response.statusText}`;
        }
        const error = new Error(`Upload failed: ${errorText}`);
        observer.error(error);
        return;
      }
      
      let result;
      try {
        const text = await response.text();
        if (!text) {
          throw new Error('Empty response');
        }
        result = JSON.parse(text);
      } catch (parseError) {
        const error = new Error(`Failed to parse response: ${parseError instanceof Error ? parseError.message : 'Unknown error'}`);
        observer.error(error);
        return;
      }
      
      if (result.errors && result.errors.length > 0) {
        // Check for authorization errors
        const authError = result.errors.find(
          (err: { message?: string; extensions?: { code?: string } }) =>
            err.message?.includes('not authorized') ||
            err.message?.includes('authorized') ||
            err.message?.includes('authentication') ||
            err.extensions?.code === 'UNAUTHENTICATED' ||
            err.extensions?.code === 'FORBIDDEN'
        );

        if (authError) {
          // Try to refresh token
          try {
            const refreshed = await authApi.refreshToken();
            localStorage.setItem('estatehub_access_token', refreshed.accessToken);
            localStorage.setItem('estatehub_user', JSON.stringify(refreshed));
            
            // For file uploads, we can't easily retry, so just clear and redirect
            this.clearAuthAndRedirect();
            observer.error(new Error('Session expired. Please log in again.'));
            return;
          } catch (refreshError) {
            // Refresh failed, clear auth and redirect
            this.clearAuthAndRedirect();
            observer.error(new Error('Session expired. Please log in again.'));
            return;
          }
        }

        const errorMessages = result.errors.map((err: { message?: string; extensions?: { message?: string } }) => 
          err.message || err.extensions?.message || JSON.stringify(err)
        ).join(', ');
        const error = new Error(errorMessages);
        observer.error(error);
      } else {
        observer.next(result);
        observer.complete();
      }
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown error occurred';
      observer.error(new Error(errorMessage));
    }
  }

  private async handleJsonRequest(
    operation: Operation,
    observer: { next: (value: FetchResult) => void; error: (error: Error) => void; complete: () => void }
  ): Promise<void> {
    try {
      const { query, variables, operationName } = operation;
      
      const token = localStorage.getItem('estatehub_access_token');
      const headers: HeadersInit = {
        'Content-Type': 'application/json',
      };
      if (token) {
        headers['Authorization'] = `Bearer ${token}`;
      }
      
      const response = await fetch(this.uri, {
        method: 'POST',
        headers,
        credentials: 'include',
        body: JSON.stringify({
          query: print(query),
          variables,
          operationName,
        }),
      });
      
      if (!response.ok) {
        let errorText = '';
        try {
          errorText = await response.text();
        } catch {
          errorText = `HTTP ${response.status}: ${response.statusText}`;
        }
        const error = new Error(`Request failed: ${errorText}`);
        observer.error(error);
        return;
      }
      
      let result;
      try {
        const text = await response.text();
        if (!text) {
          throw new Error('Empty response');
        }
        result = JSON.parse(text);
      } catch (parseError) {
        const error = new Error(`Failed to parse response: ${parseError instanceof Error ? parseError.message : 'Unknown error'}`);
        observer.error(error);
        return;
      }
      
      if (result.errors && result.errors.length > 0) {
        // Check for authorization errors
        const authError = result.errors.find(
          (err: { message?: string; extensions?: { code?: string } }) =>
            err.message?.includes('not authorized') ||
            err.message?.includes('authorized') ||
            err.message?.includes('authentication') ||
            err.extensions?.code === 'UNAUTHENTICATED' ||
            err.extensions?.code === 'FORBIDDEN'
        );

        if (authError) {
          // Try to refresh token
          try {
            const refreshed = await authApi.refreshToken();
            localStorage.setItem('estatehub_access_token', refreshed.accessToken);
            localStorage.setItem('estatehub_user', JSON.stringify(refreshed));
            
            // Retry the request with new token
            const token = refreshed.accessToken;
            const retryHeaders: HeadersInit = {
              'Content-Type': 'application/json',
            };
            if (token) {
              retryHeaders['Authorization'] = `Bearer ${token}`;
            }
            
            const retryResponse = await fetch(this.uri, {
              method: 'POST',
              headers: retryHeaders,
              credentials: 'include',
              body: JSON.stringify({
                query: print(operation.query),
                variables: operation.variables,
                operationName: operation.operationName,
              }),
            });
            
            const retryResult = await retryResponse.json();
            if (retryResult.errors) {
              // Still errors after refresh, clear auth
              this.clearAuthAndRedirect();
              observer.error(new Error('Session expired. Please log in again.'));
            } else {
              observer.next(retryResult);
              observer.complete();
            }
            return;
          } catch (refreshError) {
            // Refresh failed, clear auth and redirect
            this.clearAuthAndRedirect();
            observer.error(new Error('Session expired. Please log in again.'));
            return;
          }
        }

        const errorMessages = result.errors.map((err: { message?: string; extensions?: { message?: string } }) => 
          err.message || err.extensions?.message || JSON.stringify(err)
        ).join(', ');
        const error = new Error(errorMessages);
        observer.error(error);
      } else {
        observer.next(result);
        observer.complete();
      }
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown error occurred';
      observer.error(new Error(errorMessage));
    }
  }

  private clearAuthAndRedirect(): void {
    // Clear auth data
    localStorage.removeItem('estatehub_access_token');
    localStorage.removeItem('estatehub_user');
    
    // Redirect to login after a short delay to allow error to be displayed
    setTimeout(() => {
      window.location.href = '/login';
    }, 2000);
  }
}

