# Library System Assessment

One-tier ASP.NET Core library management system for the Software Developer coding assessment.

## Stack

- C# only
- ASP.NET Core on .NET 8
- MVC + API endpoints in one project
- Entity Framework Core
- SQL Server Express LocalDB
- Google bearer ID token authentication
- xUnit tests

## Current Status

This project is being implemented phase by phase. The current baseline documents are:

- `prd.md` for product and technical requirements
- `kanban.md` for phased implementation tracking

## Prerequisites

- Visual Studio 2022
- .NET 8 SDK
- SQL Server Express LocalDB

## Database

Default local database target:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=LibrarySystemDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

## Authentication

Member-facing endpoints will use Google bearer ID token authentication.

Protected endpoints will expect:

```http
Authorization: Bearer <google-id-token>
```

Google client configuration will use placeholders only. Real secrets must not be committed.

## Planned Protected Endpoints

- `GET /me`
- `GET /me/loans`
- `POST /books/{bookId}/borrow`
- `POST /loans/{loanId}/return`

## Planned Public Endpoints

- `GET /books`
- `GET /books/{id}`

## How To Run

Detailed run instructions will be completed during the final documentation phase.

For the current baseline:

```powershell
dotnet build
```

## How To Test

Automated tests will be added in a later phase.

Planned test command:

```powershell
dotnet test
```

## Notes

- This is a one-tier application: no separate frontend or backend project.
- Book list and book detail endpoints are public.
- Book creation, member profile, and loan actions require authentication.
- SQL scripts will be added under `/sql` in a later phase.
