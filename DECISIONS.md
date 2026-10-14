# Architecture and Design Decisions

## 1. Scope

The application focuses on the assessment's required task-management
features: authentication, CRUD operations, status changes, validation,
containers, and Kubernetes manifests.

Registration, multiple users, refresh tokens, and advanced filtering are
outside the current scope.

## 2. Backend layering

The backend uses:

```text
Controller → Service → Repository → EF Core → PostgreSQL
```

Controllers handle HTTP requests and response codes.

Services implement application behavior, including initial Todo status
and status toggling.

Repositories handle database access.

This separation makes responsibilities clear and keeps database queries
out of controllers. The extra interfaces and classes add some complexity,
but provide explicit boundaries for the assessment.

## 3. Request DTOs

Request DTOs define the fields accepted from clients.

Task create/update requests exclude ID and status. The database generates
IDs, and a separate endpoint handles status changes.

Data annotations validate required fields, maximum lengths, and priority.

Task entities are returned directly because the current model is small
and contains no navigation properties or sensitive fields. Separate
response DTOs would provide a stronger API boundary as the model grows.

## 4. Database and task fields

PostgreSQL is accessed through EF Core and the Npgsql provider.

EF Core migration files are included in source control.

Priority and status use C# enums stored as strings for readability.

Due dates use nullable DateOnly values mapped to PostgreSQL date columns.
A deadline represents a calendar date rather than a timezone-dependent
timestamp.

Descriptions are limited to 200 characters in both API validation and
database configuration.

## 5. Migration startup

The backend applies pending migrations before accepting HTTP requests.

This supports the assessment's requirement for a runnable system from a
fresh database.

Database readiness is checked before backend startup in Compose and
Kubernetes.

Startup migration failures stop the backend and appear in container logs.
They are not handled by request exception middleware.

For larger deployments, migrations would be a separate controlled
deployment step rather than part of every application startup.

## 6. Authentication

A single configured assessment user is used instead of a users table.
The assessment explicitly permits this approach.

Credentials and the JWT signing key are supplied through configuration.

Successful login creates a signed JWT containing the user identifier,
email, token ID, issuer, audience, and expiry.

The API validates the signature, issuer, audience, and lifetime.
All task controller actions require authorization.

The token lifetime defaults to the configured 60 minutes, with a
30-second validation clock tolerance.

For multiple users, authentication would use persisted users and
password hashing, typically through an established identity system.

## 7. Frontend authentication

Angular stores the token and its expiry in localStorage, as permitted by
the assessment.

A route guard controls navigation to task pages.

An HTTP interceptor attaches Bearer tokens to relative `/api/` requests,
excluding login. Protected API responses of 401 clear the saved login and
redirect to the login page.

Frontend authentication checks improve navigation behavior. The backend
remains responsible for enforcing access.

Logout clears the browser's stored token. It does not revoke an existing
token elsewhere. Refresh tokens and token revocation are not implemented.

localStorage persists across browser sessions but is accessible to
JavaScript, so preventing cross-site scripting remains important.

## 8. Frontend structure and forms

The Angular application uses standalone components.

Core services, models, guards, and interceptors are separated from feature
pages.

Reactive Forms handle login and task input validation.

Bootstrap CSS provides layout and styling. Angular handles behavior,
without Bootstrap JavaScript.

A shared task form supports creation and editing. Signals represent
loading, saving, error, and task-list state.

## 9. Due-date restriction

The Angular form rejects dates before the user's current local date.

The native date input has a minimum date, and an Angular validator also
checks the value.

This is a frontend rule only. The backend currently accepts past due
dates.

Editing an existing overdue task requires clearing its due date or
choosing today or a later date.

## 10. API errors

The API uses Problem Details for consistent JSON errors.

Invalid requests return 400, failed authentication returns 401, and
missing tasks return 404.

Unexpected request exceptions are logged and return a generic 500
response with a trace ID.

Internal exception details are not included in the client response.

## 11. Containers and API routing

Both application Dockerfiles use multi-stage builds.

The backend runtime image contains published ASP.NET Core output.

The frontend runtime image contains nginx and the Angular production
build.

nginx serves static files and proxies `/api/` to the backend. Its
index.html fallback supports direct Angular route navigation and refresh.

Relative API URLs let the same frontend code work with the Angular
development proxy and nginx.

The browser communicates with one frontend origin in this arrangement,
so the application does not require cross-origin API requests.

## 12. Configuration and secrets

Docker Compose reads the local .env file and passes configuration into
containers.

.env.example documents required variables without real secrets.

Kubernetes uses a ConfigMap for non-sensitive values and a Secret for
passwords and the JWT signing key.

The committed Secret template contains placeholders. The populated local
Secret manifest is excluded from Git.

Base64 representation in Kubernetes Secrets is not encryption.
Production deployments would require appropriate cluster access controls
and secret-management practices.

## 13. Kubernetes

The application uses a dedicated taskflow namespace.

Each component has a Deployment and ClusterIP Service.

PostgreSQL uses one replica, a PersistentVolumeClaim, and a Recreate
strategy to avoid overlapping database Pods during replacement.

The backend has an init container that waits for PostgreSQL and
startup, readiness, and liveness probes.

The current API health endpoint checks process responsiveness. It does
not verify database connectivity after startup.

The frontend uses nginx to proxy API requests through the backend
Service. Local access uses port-forwarding.

Ingress and a highly available database are outside the current scope.

## 14. Known deployment limitations

- HTTP is used for local demonstration; external hosting requires TLS.
- Swagger is enabled in the assessment's container configuration.
- Local Kubernetes image availability depends on its container runtime.
- Kubernetes secret or environment changes require container restarts
  to take effect.