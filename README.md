# Smart Building Helpdesk

AI-powered smart building helpdesk built with Angular, ASP.NET Core, SQL Server, and OpenRouter.

The app starts with a chatbot workflow:

1. A user describes a building issue in natural language.
2. The chatbot classifies the request and gives troubleshooting guidance.
3. If needed, the chatbot suggests ticket creation.
4. Admins assign technicians and manage priorities.
5. Technicians update progress and comment on assigned work.

## Tech Stack

- Frontend: Angular standalone components
- Backend: ASP.NET Core Web API (.NET 10)
- Database: SQL Server / LocalDB
- AI: OpenRouter chat completions
- Auth: JWT bearer authentication

## Main Features

- Login and register with `User`, `Technician`, or `Admin` role
- AI chatbot with prompt-driven intent handling
- Greeting, troubleshooting, ticket creation, ticket status, and unrelated-query handling
- Safety-aware human handoff recommendations
- Ticket creation with issue, category, location, priority, and status
- User and technician ticket comments
- Admin dashboard with:
  - ticket search
  - status/priority/category filters
  - technician assignment
  - priority management
  - knowledge-base management
- Technician workflow for assigned tickets and status updates
- Database-backed chatbot knowledge base editable by admin
- OpenRouter fallback behavior when AI is unavailable

## Roles

### User

- use chatbot
- create tickets
- view own tickets
- add comments to accessible tickets

### Technician

- use chatbot for troubleshooting guidance only
- view assigned tickets
- update ticket status on assigned tickets
- add comments to accessible tickets
- cannot create tickets

### Admin

- view all tickets
- assign technicians
- update ticket priority
- manage ticket comments
- manage the chatbot knowledge base

## Project Structure

```text
new_chatbot/
  backend/   ASP.NET Core API, EF Core, auth, chatbot, ticket APIs
  frontend/  Angular app for auth, chatbot, tickets, admin dashboard
  README.md
  PROJECT_EXPLANATION.md
  CONCEPTS_USED.md
```

## Requirements

- .NET 10 SDK
- Node.js 20 LTS recommended
- SQL Server Express, SQL Server Developer, or LocalDB

## Database Configuration

The project currently uses SQL Server through this connection string in `backend/appsettings.json`:

```json
"DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=SmartBuildingHelpdeskDb;Trusted_Connection=True;TrustServerCertificate=True;"
```

If your machine does not use LocalDB, change it to your SQL Server instance.

Example for SQL Server Express:

```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=SmartBuildingHelpdeskDb;Trusted_Connection=True;TrustServerCertificate=True;"
```

Note:

- this project uses SQL Server
- it does not currently use EF Core migrations
- schema updates are handled by startup initialization code

## OpenRouter Configuration

Set your API key before starting the backend:

```powershell
$env:OPENROUTER_API_KEY="your_openrouter_api_key"
```

To save it permanently for your Windows user:

```powershell
setx OPENROUTER_API_KEY "your_openrouter_api_key"
```

Then open a new terminal.

If the key is missing or OpenRouter fails, the backend falls back to non-AI guidance so the app still works.

## Run Locally

### 1. Start backend

From `backend/`:

```powershell
dotnet run
```

Default API URL:

```text
http://localhost:5182
```

### 2. Start frontend

From `frontend/`:

```powershell
npm install
npm start
```

Default frontend URL:

```text
http://localhost:4200
```

Angular uses `frontend/proxy.conf.json`, so `/api` requests are forwarded to `http://localhost:5182`.

## Demo Accounts

- Admin: `admin@smarthelpdesk.local` / `Admin123!`
- User: `user@smarthelpdesk.local` / `User123!`
- Technician: `tech@smarthelpdesk.local` / `Tech123!`

These demo accounts are corrected/seeded automatically on backend startup.

## Key API Endpoints

### Auth

- `POST /api/auth/register`
- `POST /api/auth/login`

### Chat

- `POST /api/chat`

### Tickets

- `POST /api/tickets`
- `GET /api/tickets`
- `PUT /api/tickets/{id}`
- `GET /api/tickets/{id}/comments`
- `POST /api/tickets/{id}/comments`

### Admin

- `GET /api/users/technicians`
- `GET /api/knowledgebase`
- `POST /api/knowledgebase`
- `PUT /api/knowledgebase/{id}`
- `DELETE /api/knowledgebase/{id}`

## Portability Checklist

To run on another laptop:

1. Copy the whole `new_chatbot` folder.
2. Install .NET 10 SDK.
3. Install Node.js LTS.
4. Make sure SQL Server or LocalDB exists.
5. Update `backend/appsettings.json` if the SQL Server instance is different.
6. Set `OPENROUTER_API_KEY`.
7. Run `dotnet run` in `backend/`.
8. Run `npm install` and `npm start` in `frontend/`.

## Useful Files

- `PROJECT_EXPLANATION.md`
  - full architecture and flow explanation
- `CONCEPTS_USED.md`
  - study guide for concepts used in the project

## Build Checks

Backend:

```powershell
dotnet build
```

Frontend:

```powershell
npm run build
```

## Current Notes

- backend timestamps are returned as UTC and displayed in local browser time
- technicians can update status, but cannot create tickets
- admins can assign technicians and update ticket priority
- the chatbot knowledge base is now database-managed instead of fully hardcoded
