# syntax=docker/dockerfile:1.4
# To rebuild without cache, use: docker-compose build --no-cache frontend
FROM node:18-alpine AS build

ARG VITE_ENVIRONMENT
ARG VITE_ENABLE_DEBUG
ARG VITE_LISTING_SERVICE_GRAPHQL_URL
ARG VITE_LISTING_SERVICE_ASSETS_URL
ARG VITE_AUTHORIZATION_SERVICE_URL

ENV VITE_ENVIRONMENT=${VITE_ENVIRONMENT}
ENV VITE_ENABLE_DEBUG=${VITE_ENABLE_DEBUG}
ENV VITE_LISTING_SERVICE_GRAPHQL_URL=${VITE_LISTING_SERVICE_GRAPHQL_URL}
ENV VITE_LISTING_SERVICE_ASSETS_URL=${VITE_LISTING_SERVICE_ASSETS_URL}
ENV VITE_AUTHORIZATION_SERVICE_URL=${VITE_AUTHORIZATION_SERVICE_URL}

WORKDIR /workspace

# Copy package files first for better layer caching
COPY EstateHub.Frontend/package*.json ./EstateHub.Frontend/

WORKDIR /workspace/EstateHub.Frontend

# Install dependencies with cache mount for faster builds
RUN --mount=type=cache,id=npm,target=/root/.npm \
    npm ci --prefer-offline --no-audit

WORKDIR /workspace

# Copy source code
COPY EstateHub.Frontend/ ./EstateHub.Frontend/

WORKDIR /workspace/EstateHub.Frontend

# Build the application
RUN npm run build

FROM nginx:alpine

COPY --from=build /workspace/EstateHub.Frontend/dist /usr/share/nginx/html
COPY EstateHub.Frontend/nginx.conf /etc/nginx/nginx.conf

EXPOSE 3001

CMD ["nginx", "-g", "daemon off;"]
