# Project Explanation

## Overview

This project is a full-stack Smart Building Helpdesk system built around an AI chatbot workflow.

The core idea is:

1. A user logs in.
2. The user types a building-related issue in natural language.
3. The chatbot analyzes the message.
4. The chatbot responds with one of several behaviors:
   - greeting/general response
   - troubleshooting guidance
   - ticket creation suggestion
   - ticket status guidance
   - urgent human handoff recommendation
5. If ticket creation is needed and the user confirms, the system creates a ticket.
6. Users can see their own tickets.
7. Admins can see all tickets and update status, priority, and assignment.

This makes the app more than a normal CRUD helpdesk. It combines:

- authentication
- ticket management
- AI-assisted triage
- admin workflow

## High-Level Architecture

The project has two major parts:

### 1. Frontend

Folder: `frontend/`

Built with Angular standalone components.

Responsibilities:

- login/register UI
- chatbot UI
- ticket confirmation UI
- user ticket list
- admin dashboard
- API calls to backend

### 2. Backend

Folder: `backend/`

Built with ASP.NET Core Web API.

Responsibilities:

- authentication and JWT generation
- database access using Entity Framework Core
- ticket APIs
- role-based authorization
- OpenRouter integration
- chatbot orchestration and fallback logic

### 3. Database

The backend stores data in SQL Server / LocalDB.

Main tables:

- `Users`
- `Tickets`

## Main User Flows

## 1. Authentication Flow

The app supports register and login.

### Register

Frontend sends request to:

- `POST /api/auth/register`

Backend:

- validates if email already exists
- hashes password using ASP.NET password hasher
- stores user in DB
- creates JWT token
- returns session info

### Login

Frontend sends request to:

- `POST /api/auth/login`

Backend:

- looks up user by email
- verifies password hash
- creates JWT token
- returns token and user details

The Angular app stores the token in local storage and sends it automatically on future requests using an HTTP interceptor.

## 2. Chatbot Flow

The chatbot flow is the heart of the system.

### Frontend behavior

In `frontend/src/app/pages/helpdesk-page.component.ts`:

- user types message
- message is shown in chat UI
- frontend calls backend `POST /api/chat`
- bot response is displayed
- if backend says `shouldOfferTicket = true`, a confirmation panel appears

### Backend behavior

In `backend/Services/OpenRouterService.cs`:

- backend sends a structured prompt to OpenRouter
- prompt explains the role of the model: smart building helpdesk specialist
- prompt includes a small knowledge base
- prompt asks the model to return JSON only

The LLM is expected to return:

- `issue`
- `category`
- `location`
- `priority`
- `solution`
- `intent`
- `confidence`
- `shouldOfferTicket`
- `requiresHumanHandoff`
- `handoffReason`
- `botMessage`

The backend then:

- parses the JSON
- normalizes missing or invalid values
- keeps minimal safety guardrails
- returns the final structured response to the frontend

### Chat intents currently supported

- `greeting_general_chat`
- `troubleshooting_request`
- `ticket_creation_request`
- `ticket_status_query`
- `unrelated_query`

This means the system can react differently depending on user intent instead of treating every message as a maintenance fault.

## 3. Ticket Creation Flow

If the chatbot decides a ticket may be needed, it sets:

- `shouldOfferTicket = true`

Then the frontend shows a confirmation prompt.

If the user confirms:

- frontend calls `POST /api/tickets`

Backend:

- reads the authenticated user id from JWT
- creates a ticket with issue, category, location, priority
- sets default status to `Open`
- stores it in DB

## 4. My Tickets Flow

On the helpdesk page, the frontend calls:

- `GET /api/tickets`

Backend behavior:

- if user is normal `User`, return only their tickets
- if user is `Admin`, return all tickets

This is a simple but important authorization rule.

## 5. Admin Dashboard Flow

Admin dashboard shows:

- all tickets
- summary counts for Open, In Progress, Resolved

Admin can update:

- status
- priority
- assigned person

Frontend sends:

- `PUT /api/tickets/{id}`

Backend only allows this for admin role.

## Folder-by-Folder Explanation

## Root

- `README.md`
  - setup and run instructions
- `PROJECT_EXPLANATION.md`
  - this document
- `CONCEPTS_USED.md`
  - study/learning document
- `.gitignore`
  - ignores build artifacts and dependencies

## Backend

### `Program.cs`

This is the startup and dependency injection entry point.

It configures:

- controllers
- Swagger
- EF Core database context
- password hasher
- JWT authentication
- CORS
- OpenRouter HTTP client
- DB initialization and demo user seeding

### `Controllers/`

- `AuthController.cs`
  - login and register
- `ChatController.cs`
  - chatbot API endpoint
- `TicketsController.cs`
  - create, list, update tickets

### `Data/`

- `AppDbContext.cs`
  - EF Core database context
- `SeedData.cs`
  - creates demo admin and user accounts on startup if missing

### `Models/`

- `User.cs`
- `Ticket.cs`
- `UserRole.cs`

These represent the database/domain entities.

### `Dtos/`

DTO means Data Transfer Object.

Used to shape API request/response models separately from DB entities.

Files:

- `AuthDtos.cs`
- `ChatDtos.cs`
- `TicketDtos.cs`

### `Services/`

- `JwtTokenService.cs`
  - creates JWT tokens
- `OpenRouterService.cs`
  - main AI orchestration service

This is where most chatbot behavior lives.

## Frontend

### `src/app/core/`

Contains reusable logic and services.

- `auth.service.ts`
  - login/register/logout/session management
- `auth.interceptor.ts`
  - attaches JWT token to API requests
- `auth.guard.ts`
  - protects authenticated routes
- `admin.guard.ts`
  - protects admin route
- `chat.service.ts`
  - calls `/api/chat`
- `ticket.service.ts`
  - calls ticket APIs
- `toast.service.ts`
  - global toast notifications
- `models.ts`
  - TypeScript interfaces for API data

### `src/app/pages/`

- `auth-page.component.*`
  - login/register page
- `helpdesk-page.component.*`
  - main chatbot and My Tickets page
- `admin-dashboard.component.*`
  - admin table and summary dashboard

### `src/app/shared/`

- `toast.component.ts`
  - reusable toast rendering component

### `src/app/app.routes.ts`

Defines Angular routing:

- `/login`
- `/helpdesk`
- `/admin`

## Security Model

The app uses JWT authentication.

### Role support

- `User`
- `Admin`

### What users can do

- log in
- chat with bot
- create tickets
- see their own tickets

### What admins can do

- everything users can do
- view all tickets
- update status, priority, assignment
- access admin dashboard

## AI Design in This Project

The chatbot is not just a plain text generator. It is used as a structured decision engine.

### Why structured JSON matters

If the LLM returned only plain text, the UI would not know:

- whether to show ticket confirmation
- whether it is a greeting or troubleshooting issue
- whether human handoff is needed

By forcing JSON output, the backend and frontend can build predictable application behavior.

### Why there are still backend guardrails

Even though the model now controls more behavior, the backend still protects the system by:

- normalizing invalid/missing fields
- forcing safety escalation for dangerous keywords
- providing fallback behavior when OpenRouter fails

This is important because LLMs are not always fully reliable.

## Knowledge Base Usage

The backend includes a small knowledge base in the AI prompt.

This helps the model produce more useful building-specific answers instead of generic advice.

Examples included:

- WiFi troubleshooting
- printer recovery
- HVAC guidance
- plumbing leak response
- cleaning spill SOP
- access control guide
- power disruption check

Right now this knowledge base is embedded in code. In a more advanced version, it could be moved to:

- JSON files
- a database
- a vector database / retrieval system

## Current Strengths of the Project

- clean separation between frontend and backend
- working JWT auth
- role-based admin/user flows
- structured AI response handling
- fallback behavior when OpenRouter is unavailable
- modern Angular standalone setup
- practical admin dashboard

## Current Simplifications / Demo-Level Design

These are intentional simplifications:

- no EF migrations yet, uses `EnsureCreated()`
- no conversation history stored
- no refresh token flow
- no production-grade logging/auditing
- no technician role yet
- no real ticket comments/history
- ticket status query is guidance, not a true chat-based DB lookup
- knowledge base is static inside code

## How Everything Connects Together

### Login flow

Angular auth page -> `AuthService` -> `/api/auth/login` -> `AuthController` -> DB -> JWT -> frontend session

### Chat flow

Angular helpdesk page -> `ChatService` -> `/api/chat` -> `ChatController` -> `OpenRouterService` -> OpenRouter/fallback -> structured response -> frontend UI

### Ticket creation flow

Helpdesk page confirmation -> `TicketService.createTicket()` -> `/api/tickets` -> `TicketsController.Create()` -> DB

### Admin update flow

Admin dashboard -> `TicketService.updateTicket()` -> `/api/tickets/{id}` -> `TicketsController.Update()` -> DB

## How to Read the Project as a Learner

If you want to understand the project step by step, read files in this order:

1. `README.md`
2. `backend/Program.cs`
3. `backend/Controllers/AuthController.cs`
4. `backend/Controllers/ChatController.cs`
5. `backend/Controllers/TicketsController.cs`
6. `backend/Services/OpenRouterService.cs`
7. `frontend/src/app/app.routes.ts`
8. `frontend/src/app/core/auth.service.ts`
9. `frontend/src/app/pages/helpdesk-page.component.ts`
10. `frontend/src/app/pages/admin-dashboard.component.ts`

That order gives you the clearest picture of how the app is wired.
