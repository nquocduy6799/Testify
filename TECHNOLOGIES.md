# Tech Stack & Architecture Overview

This document outlines the technologies, frameworks, and libraries used in the **Testify** project.

## 🏗️ Core Architecture
- **Solution Type:** Blazor WebAssembly Hosting Model (ASP.NET Core Hosted)
- **Platform:** .NET 10.0
- **Language:** C# 13 / 14 (implied by .NET 10)

## 🔙 Backend (Testify.Server)
- **Framework:** ASP.NET Core Web API
- **Database Interface:** Entity Framework Core 10.0.2
- **Database Engine:** Microsoft SQL Server
- **Authentication:** 
  - ASP.NET Core Identity (Individual Accounts)
  - Cookie-based Authentication
- **Real-time Communication:** SignalR (Hubs for Chat, Notifications, Calls)
- **API Documentation:** OpenAPI / Swagger (implied by standard templates, though explicitly mapped endpoints are Controllers)

## 🎨 Frontend (Testify.Client)
- **Framework:** Blazor WebAssembly
- **CSS Framework:** Tailwind CSS v4.1.18 (configured via npm & npx)
- **Component Model:** Razor Components (.razor)
- **State Management:** In-memory Services (Scoped)
- **Real-time Client:** `Microsoft.AspNetCore.SignalR.Client`
- **Cloud/Media:** `CloudinaryDotNet` (v1.28.0)
- **P2P Communication:** WebRTC (Custom implementation for video calls)

## 📦 Shared Libraries (Testify.Shared)
- **Purpose:** DTOs, Enums, Shared Models, and Interfaces.
- **Dependencies:** Minimal (mainly `Microsoft.AspNetCore.Identity.EntityFrameworkCore` to share Identity User models if needed).

## 🛠️ Dev Tools & Configuration
- **Build System:** MSBuild (SDK-style projects)
- **Frontend Build:** `npm` scripts for Tailwind CSS processing
- **Solution File:** `.slnx` (New XML-based Solution format)

## 📂 Project Structure
- **Global:** Centralized `Program.cs` logic for both Client and Server.
- **Features (Client):** Feature-folder organization (Chat, Kanban, Projects, Milestones).
- **Services:** Dependency Injection used extensively for Repositories and Logic Services.
