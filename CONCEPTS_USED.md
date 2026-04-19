# Concepts Used

This document lists the important concepts used in the project so you can prepare for interviews, demos, or future implementation work.

## 1. Full-Stack Architecture

Concepts:

- frontend and backend separation
- client-server communication
- REST API architecture
- role-based application design

Why it matters:

- helps you understand how UI, backend, database, and AI work together

## 2. Angular Concepts

### Angular standalone components

Used instead of NgModule-heavy structure.

Examples:

- `auth-page.component.ts`
- `helpdesk-page.component.ts`
- `admin-dashboard.component.ts`

Learn:

- what standalone components are
- how `imports` works inside components
- benefits over module-based architecture

### Angular routing

Used in:

- `frontend/src/app/app.routes.ts`

Learn:

- route definitions
- route guards
- redirects
- protected routes

### Angular guards

Used in:

- `auth.guard.ts`
- `admin.guard.ts`

Learn:

- route protection
- auth-based navigation
- role-based route access

### Angular services

Used in:

- `auth.service.ts`
- `chat.service.ts`
- `ticket.service.ts`
- `toast.service.ts`

Learn:

- dependency injection in Angular
- separating UI from data logic
- reusable business logic on frontend

### Angular HttpClient

Used for API calls.

Learn:

- `get`, `post`, `put`
- request/response typing
- async API handling

### HTTP interceptor

Used in:

- `auth.interceptor.ts`

Learn:

- attaching JWT tokens automatically
- request cloning
- centralized request behavior

### Angular forms

Used in:

- login/register page
- chatbot input
- admin editable fields

Learn:

- reactive forms
- form validation
- `ngModel`
- handling user input

### Angular signals

Used in multiple components/services.

Learn:

- `signal()`
- `computed()`
- state management with signals
- how signals differ from RxJS-based local state

### Component communication through shared services

Example:

- toast state managed globally by a service and displayed in a shared component

## 3. ASP.NET Core Concepts

### ASP.NET Core Web API

Concepts:

- API controllers
- routing
- dependency injection
- middleware pipeline

Learn:

- `Program.cs`
- `[ApiController]`
- `[Route]`
- `[HttpGet]`, `[HttpPost]`, `[HttpPut]`

### Dependency Injection (DI)

Configured in:

- `backend/Program.cs`

Learn:

- service registration
- `AddScoped`
- service lifetime basics
- constructor injection

### Middleware

Used in request pipeline:

- CORS
- authentication
- authorization
- Swagger

Learn:

- middleware order matters
- request pipeline in ASP.NET Core

### Configuration system

Used in:

- `appsettings.json`
- environment variables

Learn:

- reading config values
- environment-based secrets
- connection strings

## 4. Authentication and Authorization

### JWT authentication

Used to authenticate users after login.

Learn:

- what JWT is
- claims
- issuer/audience/signing key
- Bearer token flow

### Role-based authorization

Used for admin vs user access.

Learn:

- `[Authorize]`
- `[Authorize(Roles = ...)]`
- role claim usage

### Password hashing

Used with:

- `PasswordHasher<User>`

Learn:

- why passwords are never stored plain text
- hashing vs encryption
- password verification flow

## 5. Entity Framework Core Concepts

### DbContext

Used in:

- `AppDbContext.cs`

Learn:

- database context
- `DbSet<T>`
- how EF maps classes to tables

### Entity models

Used in:

- `User.cs`
- `Ticket.cs`

Learn:

- entity classes
- primary keys
- relationships
- enum mapping

### LINQ queries

Used in controllers.

Learn:

- `Where`
- `Select`
- `OrderByDescending`
- `FirstOrDefaultAsync`
- `Include`

### Database initialization and seeding

Used in:

- `SeedData.cs`

Learn:

- dev-time seed data
- why demo users are helpful
- limitations of `EnsureCreated()`

### SQL Server connection handling

Learn:

- connection strings
- LocalDB vs SQL Server Express vs full SQL Server

## 6. API Design Concepts

### REST endpoints

Current endpoints:

- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/chat`
- `POST /api/tickets`
- `GET /api/tickets`
- `PUT /api/tickets/{id}`

Learn:

- resource-based routing
- HTTP verbs
- request/response design

### DTOs

Used in:

- `AuthDtos.cs`
- `ChatDtos.cs`
- `TicketDtos.cs`

Learn:

- why DTOs are separate from DB models
- request/response shaping
- API contract design

## 7. AI / LLM Integration Concepts

### External AI API integration

Used with OpenRouter.

Learn:

- HTTP client integration with LLM APIs
- authorization headers
- request body structure
- model selection

### Prompt engineering

Used in:

- `OpenRouterService.cs`

Learn:

- system prompt design
- asking for structured JSON
- grounding the model with domain context
- behavior instruction design

### Structured LLM output

The model is asked to return JSON only.

Learn:

- why structured output is useful in applications
- mapping AI output to application logic
- failure handling when output is invalid

### Intent classification

Used for chatbot routing.

Learn:

- user intent detection
- conversational AI workflow branching

Current intents:

- greeting/general chat
- troubleshooting request
- ticket creation request
- ticket status query
- unrelated query

### Confidence scoring

Used in chat response model.

Learn:

- why AI confidence matters
- how confidence can influence handoff or automation

### Human handoff logic

Used for risky or unclear situations.

Learn:

- escalation flow
- safety-first design
- not over-trusting AI

### Retrieval / knowledge grounding

The project currently passes a small knowledge base in the prompt.

Learn:

- grounding model responses with domain context
- difference between static prompt knowledge and full RAG systems

### Fallback logic

If OpenRouter is unavailable, backend still returns a usable result.

Learn:

- resilience in AI apps
- graceful degradation

## 8. Chatbot Product Design Concepts

### Chat-first support workflow

Concept:

- user asks naturally
- bot helps first
- ticket is created only when needed

Learn:

- conversational triage
- reducing unnecessary tickets
- support automation design

### Confirmation flow

Before creating a ticket, the user confirms.

Learn:

- user confirmation in product flows
- safe automation

### Safety escalation

Examples:

- smoke
- electrical smell
- major leak

Learn:

- high-risk incident handling
- where AI must not act as the final authority

## 9. Frontend UX Concepts

### Chat UI patterns

Used in helpdesk page.

Learn:

- chat bubbles
- typing indicator
- async message flow
- confirmation prompts in chat

### Admin dashboard UX

Used in admin page.

Learn:

- editable tables
- summary cards
- operational dashboards

### Toast notifications

Learn:

- transient feedback to users
- success/error/info notifications

### Responsive design basics

CSS includes mobile-aware layouts.

Learn:

- media queries
- adaptive layouts

## 10. TypeScript and C# Language Concepts

### TypeScript concepts

Learn:

- interfaces
- async/await
- dependency injection patterns in Angular
- template binding
- event binding
- two-way binding with `ngModel`

### C# concepts

Learn:

- records
- classes
- enums
- async/await
- nullable reference types
- constructor injection
- LINQ

## 11. Dev and Tooling Concepts

### Swagger / OpenAPI

Enabled in backend development mode.

Learn:

- API documentation
- testing API endpoints manually

### Proxy configuration

Used in:

- `frontend/proxy.conf.json`

Learn:

- avoiding CORS issues during frontend development
- forwarding `/api` requests to backend

### CORS

Configured in backend.

Learn:

- why browsers block cross-origin requests
- allowed origins setup

### Git basics

Used to version the project.

Learn:

- `git init`
- `git add`
- `git commit`
- `git remote add`
- `git push`

## 12. Topics to Study Next

If you want to go deeper, study these next:

1. EF Core migrations
2. Refresh token authentication
3. Advanced Angular state management
4. Logging with Serilog
5. Unit and integration testing
6. Docker for full-stack apps
7. RAG with vector databases
8. Secure secret management
9. Production deployment for Angular + ASP.NET Core
10. Real-time updates with SignalR

## 13. Best Study Order

Recommended order for preparation:

1. HTTP and REST APIs
2. Angular basics
3. ASP.NET Core Web API basics
4. Entity Framework Core basics
5. JWT auth and role-based authorization
6. Dependency injection
7. SQL Server basics
8. LLM API integration
9. Prompt engineering and structured outputs
10. Chatbot workflow design

## 14. Interview-Style Questions You Should Be Ready For

Examples:

- How does JWT authentication work in your project?
- Why did you use DTOs instead of returning EF entities directly?
- How do users and admins have different access?
- How does the chatbot decide when to create a ticket?
- Why is fallback logic important in AI-powered applications?
- Why is structured JSON output better than plain text for this project?
- What are the risks of relying only on the LLM?
- How would you make the project production-ready?
- Why would EF migrations be better than `EnsureCreated()`?
- How would you store and use chat history in a future version?
