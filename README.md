# Job Tracker

[![CI](https://github.com/hardikjain2005/Job-Tracker/actions/workflows/ci.yml/badge.svg)](https://github.com/hardikjain2005/Job-Tracker/actions/workflows/ci.yml)

A full-stack app for tracking job applications. Register, log in, then add, edit, filter and delete your own applications. Each user only ever sees their own data.

**Stack:** ASP.NET Core Web API (.NET 10) · EF Core + SQLite · JWT auth (BCrypt password hashing) · React + TypeScript (Vite) · xUnit · GitHub Actions

```
React (Vite, :5173)  ──HTTP + JWT──▶  ASP.NET Core API (:5176)  ──EF Core──▶  SQLite
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (LTS or newer)

## Run it locally

**1. Backend** (from the repo root)

```bash
cd JobTracker.Api

# The API refuses to start without a signing key, and so does `dotnet ef`, so set it first.
dotnet user-secrets set "Jwt:Key" "$(openssl rand -base64 48)"

dotnet tool restore              # installs the pinned dotnet-ef tool
dotnet ef database update        # creates jobtracker.db from the migration
dotnet run --launch-profile http # API on http://localhost:5176
```

Swagger UI (development only): http://localhost:5176/swagger. Register, log in, click **Authorize** and paste only the `token` value.

**2. Frontend** (second terminal)

```bash
cd job-tracker-ui
npm install
npm run dev                      # http://localhost:5173
```

The UI calls `http://localhost:5176` by default. To change it, copy `.env.example` to `.env` and edit `VITE_API_URL`.

## Tests

```bash
dotnet test                      # from the repo root
cd job-tracker-ui && npm run lint && npm run build
```

The xUnit tests cover the application and auth services against a real SQLite in-memory database.

## API

| Method | Route | Auth | Purpose |
|---|---|---|---|
| POST | `/api/auth/register` | no | Create an account (201, or 409 if the email exists) |
| POST | `/api/auth/login` | no | Returns a JWT |
| GET | `/api/applications?status=Interview` | yes | List your applications, optionally filtered |
| GET | `/api/applications/{id}` | yes | One application |
| POST | `/api/applications` | yes | Create |
| PUT | `/api/applications/{id}` | yes | Update |
| DELETE | `/api/applications/{id}` | yes | Delete |

Status is one of `Applied`, `Interview`, `Offer`, `Rejected`.

## Project layout

```
JobTracker.Api/        Controllers (thin) → Services (logic, user scoping) → Data (EF Core) → Models
JobTracker.Tests/      xUnit tests, SQLite in-memory
job-tracker-ui/        React + TypeScript: pages, components, auth context, axios client
.github/workflows/     CI: backend build + test, frontend lint + build
```

## Configuration

| Setting | Where | Notes |
|---|---|---|
| `Jwt:Key` | user-secrets locally, `Jwt__Key` env var elsewhere | At least 32 characters. Never committed. |
| `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpiryMinutes` | `appsettings.json` | Tokens last 60 minutes by default. |
| `Cors:AllowedOrigins` | `appsettings.json` | Defaults to the Vite dev server. |
| `ConnectionStrings:Default` | `appsettings.json` | SQLite file `jobtracker.db` (gitignored). |

## Status

CI runs on every push and pull request. There is no automated deployment yet.
