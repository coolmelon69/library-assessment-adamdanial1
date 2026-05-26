# Library System Assessment Kanban

## Phase 0: Project Cleanup And Baseline

Goal: make the starter project assessment-ready before feature work.

Tasks:

- [x] Add standard Visual Studio/.NET `.gitignore`.
- [x] Confirm `bin/` and `obj/` are not committed when git is initialized.
- [x] Confirm the starter project builds cleanly.
- [x] Add initial README skeleton with selected stack and setup decisions.

Acceptance Check:

- [x] `dotnet build` succeeds.
- [x] README states one-tier app, .NET 8, LocalDB, EF Core, and Google SSO.
- [x] No application feature code has been added before baseline cleanup is stable.

## Phase 1: Domain Model And LocalDB Setup

Goal: establish the database foundation.

Tasks:

- [x] Add EF Core SQL Server packages.
- [x] Add `ApplicationDbContext`.
- [x] Add `Book` entity.
- [x] Add `Member` entity.
- [x] Add `Loan` entity.
- [x] Configure unique ISBN.
- [x] Configure unique member SSO subject.
- [x] Configure required fields.
- [x] Configure non-negative total copies.
- [x] Add LocalDB connection string.
- [x] Register DbContext in `Program.cs`.
- [x] Create initial EF migration.

Acceptance Check:

- [x] `dotnet build` succeeds.
- [x] EF migration creates Books, Members, and Loans schema.
- [x] App can connect to SQL Server Express LocalDB.

## Phase 2: Book Read Features

Goal: implement public book listing and detail retrieval.

Tasks:

- [x] Add book query service.
- [x] Add public `GET /books`.
- [x] Add optional title filter.
- [x] Add optional author filter.
- [x] Add public `GET /books/{id}`.
- [x] Include available copy count in book detail.
- [x] Return `404 Not Found` for missing book.

Acceptance Check:

- [x] `GET /books` works without authentication.
- [x] Title filtering works as a case-insensitive partial match.
- [x] Author filtering works as a case-insensitive partial match.
- [x] `GET /books/{id}` includes available copies.
- [x] Missing book returns `404`.

## Phase 3: Book Create Feature

Goal: add protected book creation.

Tasks:

- [x] Configure Google bearer token authentication.
- [x] Add `Authentication:Google:ClientId` placeholder.
- [x] Add authenticated `POST /books`.
- [x] Validate title.
- [x] Validate author.
- [x] Validate ISBN.
- [x] Validate published year.
- [x] Validate total copies.
- [x] Return `201 Created` on success.
- [x] Return validation errors for invalid payloads.

Acceptance Check:

- [x] Missing token returns `401 Unauthorized`.
- [x] Invalid token returns `401 Unauthorized`.
- [ ] Valid token can create a book.
- [x] Invalid payload returns `400 Bad Request`.

## Phase 4: Member Provisioning And `/me`

Goal: satisfy Google-token-based member provisioning.

Tasks:

- [x] Add member provisioning service.
- [x] Read `sub` claim from current token.
- [x] Read `name` claim from current token.
- [x] Read `email` claim from current token.
- [x] Create member on first authenticated request.
- [x] Reuse existing member on later requests with the same `sub`.
- [x] Add protected `GET /me`.
- [x] Ensure member identity is never accepted from request body or URL.

Acceptance Check:

- [x] Missing token returns `401 Unauthorized`.
- [ ] First valid token provisions a member.
- [ ] Same Google `sub` returns the existing member.
- [ ] `/me` returns current member profile.

## Phase 5: Borrow Book

Goal: implement borrowing and its business rules.

Tasks:

- [x] Add loan service.
- [x] Add protected `POST /books/{bookId}/borrow`.
- [x] Resolve current member from token claims.
- [x] Reject missing book.
- [x] Reject borrowing when no copies are available.
- [x] Reject borrowing when member has 3 active loans.
- [x] Create active loan with borrowed date.

Acceptance Check:

- [x] Missing token returns `401 Unauthorized`.
- [ ] Unknown book returns `404 Not Found`.
- [ ] No available copies returns a meaningful error.
- [ ] Three active loans blocks borrowing.
- [ ] Successful borrow creates an active loan.

## Phase 6: List Current Member Loans

Goal: expose active loans for the authenticated member.

Tasks:

- [x] Add protected `GET /me/loans`.
- [x] Resolve current member from token claims.
- [x] Return only active loans.
- [x] Include useful book details in each loan item.
- [x] Exclude other members' loans.

Acceptance Check:

- [x] Missing token returns `401 Unauthorized`.
- [x] Returned loans are excluded.
- [x] Other members' loans are excluded.
- [x] Active loans for the current member are returned.

## Phase 7: Return Loan

Goal: implement return flow and ownership protection.

Tasks:

- [x] Add protected `POST /loans/{loanId}/return`.
- [x] Resolve current member from token claims.
- [x] Return `404 Not Found` if loan does not exist.
- [x] Return `403 Forbidden` if loan belongs to another member.
- [x] Handle already returned loans clearly.
- [x] Set `ReturnedDate` for successful returns.

Acceptance Check:

- [ ] Missing token returns `401 Unauthorized`.
- [ ] Missing loan returns `404 Not Found`.
- [ ] Another member's loan returns `403 Forbidden`.
- [ ] Own active loan can be returned.
- [ ] Returned loan is no longer active.

## Phase 8: MVC Reviewer UI

Goal: make the one-tier app easier to inspect manually.

Tasks:

- [x] Add book list/filter Razor page or MVC view.
- [x] Add book detail Razor page or MVC view.
- [x] Keep create book as API-only because the chosen auth flow uses bearer tokens.
- [x] Keep all UI inside the same ASP.NET Core project.
- [x] Keep API endpoints as the assessment source of truth.

Acceptance Check:

- [x] App opens cleanly in Visual Studio.
- [x] Public book pages work.
- [x] No separate frontend project exists.
- [x] Auth-required actions remain protected.

## Phase 9: SQL Deliverables

Goal: complete Part B SQL deliverables.

Tasks:

- [ ] Add `/sql/schema.sql`.
- [ ] Add `/sql/seed.sql`.
- [ ] Add `/sql/queries.sql`.
- [ ] Include primary keys.
- [ ] Include foreign keys.
- [ ] Include indexes.
- [ ] Include constraints.
- [ ] Add SQL comments explaining index choices.
- [ ] Add at least 10 books.
- [ ] Add at least 5 members.
- [ ] Add at least 10 loans.
- [ ] Include active and returned loans.
- [ ] Add all 5 required assessment queries.

Acceptance Check:

- [ ] `schema.sql` runs on SQL Server 2019 or later.
- [ ] `seed.sql` inserts required sample data.
- [ ] `queries.sql` contains all required queries.
- [ ] Queries correctly distinguish active and returned loans.

## Phase 10: Unit Tests

Goal: satisfy the assessment testing requirement with meaningful tests.

Tasks:

- [ ] Add xUnit test project.
- [ ] Add tests for no-copy borrow rejection.
- [ ] Add tests for 3-active-loan borrow rejection.
- [ ] Add tests for first-time member provisioning.
- [ ] Add tests for existing member reuse by Google `sub`.
- [ ] Add tests for rejecting another member's loan return.

Acceptance Check:

- [ ] `dotnet test` succeeds.
- [ ] At least 3 non-trivial business-rule tests exist.
- [ ] Tests cover borrowing, returning, and provisioning behavior.

## Phase 11: Error Handling, Logging, And Polish

Goal: make API behavior clear and reviewer-friendly.

Tasks:

- [ ] Review `400 Bad Request` responses.
- [ ] Review `401 Unauthorized` responses.
- [ ] Review `403 Forbidden` responses.
- [ ] Review `404 Not Found` responses.
- [ ] Add `ILogger` logging for member provisioning.
- [ ] Add `ILogger` logging for book borrowing.
- [ ] Add `ILogger` logging for loan returns.
- [ ] Add `ILogger` logging for rejected business-rule actions.
- [ ] Remove dead or unused code.

Acceptance Check:

- [ ] API failures return understandable messages.
- [ ] Normal failures do not expose stack traces.
- [ ] Logs cover key flows.
- [ ] `dotnet build` succeeds.
- [ ] `dotnet test` succeeds.

## Phase 12: README And Final Submission Prep

Goal: prepare the GitHub submission.

Tasks:

- [ ] Complete README project description.
- [ ] Document prerequisites.
- [ ] Document Visual Studio 2022 requirement.
- [ ] Document .NET 8 SDK requirement.
- [ ] Document SQL Server Express LocalDB requirement.
- [ ] Document Google OAuth Client ID setup.
- [ ] Document connection string format.
- [ ] Document how to run the app.
- [ ] Document how to apply database setup.
- [ ] Document how to run tests.
- [ ] Document SQL script usage.
- [ ] Justify EF Core.
- [ ] Document authorization choices.
- [ ] Document unfinished work, if any.
- [ ] Confirm `.gitignore` excludes build artifacts.
- [ ] Initialize git when ready.
- [ ] Commit final state.
- [ ] Capture final commit hash.

Acceptance Check:

- [ ] Fresh reviewer instructions are clear.
- [ ] `dotnet build` succeeds.
- [ ] `dotnet test` succeeds.
- [ ] No secrets are committed.
- [ ] `bin/` and `obj/` are not committed.
- [ ] Final commit hash is available for submission.

## Stop Rules

- [ ] Stop after each phase and verify before starting the next phase.
- [ ] Fix build or test failures within the phase that introduced them.
- [ ] Do not move to the next feature until the current phase is stable.
- [ ] Keep changes scoped to the active phase.
