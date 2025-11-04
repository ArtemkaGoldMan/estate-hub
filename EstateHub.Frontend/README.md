# EstateHub Frontend

Frontend application for EstateHub - a real estate listings platform built with React + TypeScript.

## 🚀 Tech Stack

- **React 19** - UI library
- **TypeScript** - Type safety
- **Vite** - Build tool and dev server
- **ESLint** - Code linting

## 📋 Project Overview

Based on the [High-Level Design Document](../docs/HighLevelDesign.md), the frontend should provide:

- **UI for browsing and creating listings** - Property listings with search and filtering
- **User panel** - Registration, login, user profile management
- **Message handling** - Integration with messaging service (SignalR - planned)
- **Chatbot integration** - AI-powered assistant for user questions
- **API communication** - Integration with backend microservices via API Gateway

## 🛠️ Development

### Prerequisites

- Node.js 18+ 
- npm or yarn

### Installation

```bash
npm install
```

### Development Server

```bash
npm run dev
```

The app will be available at `http://localhost:5173`

### Build for Production

```bash
npm run build
```

### Preview Production Build

```bash
npm run preview
```

### Linting

```bash
npm run lint
```

## 📁 Project Structure

```
EstateHub.Frontend/
├── src/
│   ├── assets/          # Static assets (images, fonts, etc.)
│   ├── components/      # Reusable React components
│   ├── pages/           # Page components
│   ├── services/        # API service clients
│   ├── hooks/           # Custom React hooks
│   ├── utils/           # Utility functions
│   ├── types/           # TypeScript type definitions
│   ├── App.tsx          # Main App component
│   └── main.tsx         # Application entry point
├── public/              # Static public files
├── index.html           # HTML template
├── vite.config.ts       # Vite configuration
└── package.json         # Dependencies and scripts
```

## 🔌 Backend Integration

The frontend communicates with the following backend services via API Gateway:

- **AuthService** - Authentication, user management
- **ListingService** - Property listings (GraphQL)
- **MessagingService** - User-to-user messaging (planned)
- **ChatbotService** - AI chatbot (planned)
- **AI Microservices** - Recommendations and price predictions (planned)

## 📝 Next Steps

1. Set up routing (React Router)
2. Implement authentication flow
3. Create API service clients
4. Build listing components
5. Integrate GraphQL client for ListingService
6. Add user dashboard
7. Implement messaging UI (when backend is ready)
8. Add chatbot interface (when backend is ready)

## 📚 Documentation

- [High-Level Design Document](../docs/HighLevelDesign.md)
- [Vite Documentation](https://vite.dev)
- [React Documentation](https://react.dev)
- [TypeScript Documentation](https://www.typescriptlang.org/)
