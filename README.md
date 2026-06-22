# Identity Service

A backend service that handles user authentication and authorization built with ASP.NET Core and Clean Architecture.

## Features

- User registration and login
- Email verification
- Password reset flow
- JWT access tokens with refresh token rotation
- Logout invalidates the token immediately
- Role-based access control (User / Admin)
- Rate limiting on all auth endpoints

## Setup

Make sure Docker is running, then:

```bash
docker compose up --build
```

This starts the API, SQL Server, and Redis. Migrations run automatically.

API docs available at `http://localhost:5000/scalar`

## Auth endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register a new user |
| GET | `/api/auth/verify-email` | Verify email with token |
| POST | `/api/auth/resend-verification` | Resend verification email |
| POST | `/api/auth/login` | Login, returns access and refresh tokens |
| POST | `/api/auth/refresh-token` | Get new tokens using refresh token |
| POST | `/api/auth/logout` | Logout and invalidate tokens |
| POST | `/api/auth/forgot-password` | Request password reset email |
| POST | `/api/auth/reset-password` | Reset password using token |

## Running tests

Docker must be running — tests spin up a real SQL Server container.

```bash
dotnet test
```
