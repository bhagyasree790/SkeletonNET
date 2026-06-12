# SkeletonNET

A "from-scratch" implementation of a .NET-like web framework to understand the internals of Dependency Injection (DI) lifetimes, middleware pipelines, and HTTP request handling.

## 🚀 Overview

This project implements a simplified version of the ASP.NET Core architecture using only basic C# primitives and TCP listeners. It was built to demonstrate:
- **Custom DI Container**: Supporting Singleton, Scoped, and Transient lifetimes.
- **Middleware Pipeline**: A recursive "Russian Doll" pattern for request processing.
- **Attribute-Based Routing**: Auto-discovery of controllers and methods using Reflection.
- **Custom Web Server**: A manual TCP implementation ("Mini-Kestral") that parses HTTP requests and serves responses.

## 🛠️ Key Components

### 1. Dependency Injection (`/Services`)
- **Transient**: New instance every time it's requested.
- **Scoped**: Shared instance within a single HTTP request (via `CreateScope()`).
- **Singleton**: One instance for the entire application lifetime.

### 2. Middleware (`GlobalErrorHandlingMiddleware.cs`)
Demonstrates how to wrap the entire request pipeline in a `try-catch` block to return standardized JSON error responses.

### 3. WebApplication & Router
- `WebApplicationBuilder`: Configures services and builds the app.
- `Router`: Uses Regex to match URL patterns and extract route parameters (e.g., `/users/{id}`).
- `MapControllers()`: Scans the assembly for classes ending in `Controller` and registers their methods as endpoints.

### 4. Custom Server (`Kestral.cs`)
Uses `TcpListener` to handle raw socket connections, parse HTTP headers/bodies, and dispatch them to the middleware pipeline.

## 🚦 Getting Started

### Prerequisites
- .NET 10.0 SDK (or compatible)

### Running the App
```bash
dotnet run
```
The server will start on `http://localhost:5005`.

## 🧪 Testing the Features

### 1. DI Lifetimes (The Difference)
Navigate to `GET /lifetimes` to see a live comparison of service IDs.
- **Transient**: Different IDs between the Controller and Internal Service.
- **Scoped**: Identical IDs within one request, but changes on refresh.
- **Singleton**: Same ID forever.

### 2. CRUD Operations
- **GET** `/users`: List all users.
- **POST** `/users`: Create a user (Body: `{"Name": "NewUser"}`).
- **DELETE** `/users/{id}`: Delete a user by ID.

### 3. Middleware & Error Handling
- **GET** `/error`: Triggers a manual exception. The `GlobalErrorHandlingMiddleware` will catch it and return a clean JSON error instead of crashing the server.

## 📝 Project Structure
- `Program.cs`: Entry point and service configuration.
- `WebApplication.cs`: The core engine managing the pipeline and routing.
- `Kestral.cs`: The TCP server and HTTP context definitions.
- `Services/`: The custom DI implementation.
- `UserController.cs`: Example of CRUD and JSON handling.
- `LifetimeController.cs`: Demonstration of DI scopes.
