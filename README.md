# TaskFlow — Mini Task Manager

A full-stack task manager with JWT authentication, built using Angular,
ASP.NET Core, PostgreSQL, Docker, and Kubernetes.

Users can log in, create and edit tasks, mark tasks Done or Todo, and
delete tasks.

## Technology stack

- Angular 21 and TypeScript
- Bootstrap 5
- ASP.NET Core 10 Web API
- Entity Framework Core with Npgsql
- PostgreSQL 17
- JWT authentication
- Docker Compose
- nginx
- Kubernetes YAML manifests

## Prerequisites

For the complete Docker application:

- Git
- Docker Desktop running with Linux containers
- Docker Compose
- Available local ports: 8080, 5000, and 5432

Node.js and the .NET SDK are not required on the host when running the
complete application through Docker.

For development outside Docker:

- Node.js 22.12 or newer within the Node.js 22 release line
- npm
- .NET 10 SDK for backend development

For Kubernetes:

- A running Kubernetes cluster
- kubectl configured for that cluster
- A default StorageClass for persistent storage
- Application images accessible to the cluster's nodes

## Quick start

### 1. Clone the repository

```powershell
git clone https://github.com/kelopaksalak/taskflow-fullstack-assessment.git
cd taskflow-fullstack-assessment
```

For a private repository, authenticate with a GitHub account that has
access.

### 2. Start Docker Desktop

Ensure Docker Desktop is running with Linux containers.

Verify Docker and Compose:

```powershell
docker info
docker compose version
```

### 3. Configure environment variables

For a new clone, copy the example file:

```powershell
Copy-Item .env.example .env
```

Open `.env` and replace its placeholders.

The file requires these variables:

```dotenv
POSTGRES_USER=taskflow
POSTGRES_DB=taskflow
POSTGRES_PASSWORD=replace_with_a_random_database_password

JWT_ISSUER=TaskFlow.Api
JWT_AUDIENCE=TaskFlow.Client
JWT_SIGNING_KEY=replace_with_a_random_signing_key
JWT_EXPIRY_MINUTES=60

AUTH_EMAIL=admin@taskflow.com
AUTH_PASSWORD=Admin@123
```

Use a long, random alphanumeric database password to avoid
connection-string escaping issues.

Generate a JWT signing key in PowerShell:

```powershell
$b = New-Object byte[] 32; [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($b); [Convert]::ToBase64String($b)
```

Copy the generated value into `JWT_SIGNING_KEY`.

Do not commit `.env`. Do not overwrite an existing configured `.env`
when restarting the application.

### 4. Build and run

From the repository root:

```powershell
docker compose up --build
```

Alternatively, run in the background:

```powershell
docker compose up --build -d
```

Docker starts PostgreSQL, the backend, and the frontend.

The backend waits for database readiness and applies pending EF Core
migrations before accepting requests. Wait for backend startup to finish
before logging in.

### 5. Open the application

| Component | Address |
|---|---|
| Frontend | http://localhost:8080 |
| Backend Swagger | http://localhost:5000/swagger |
| Backend health | http://localhost:5000/api/health |
| API through nginx | http://localhost:8080/api/health |
| PostgreSQL | localhost:5432 |

Login:

```text
Email: admin@taskflow.com
Password: Admin@123
```

These credentials must match `AUTH_EMAIL` and `AUTH_PASSWORD` in `.env`.

### 6. Stop and restart

Stop the application from another terminal in the repository root:

```powershell
docker compose down
```

Start it again:

```powershell
docker compose up --build -d
```

Database data is stored in the named volume `taskflow-pgdata` and survives
normal shutdown and container recreation.

Adding `-v` to `docker compose down` removes Compose-managed volumes and
can delete database data.

Changing `POSTGRES_PASSWORD` in `.env` does not change the password of an
already initialized PostgreSQL database.

## Features

- JWT login
- Protected task API endpoints and frontend routes
- Create, list, retrieve, update, and delete tasks
- Mark tasks Done or return them to Todo
- Reactive forms with validation
- Loading indicators and user-friendly errors
- Consistent API errors using Problem Details
- Swagger UI with Bearer token support
- Persistent database storage
- Automatic database migrations during backend startup
- Docker Compose and Kubernetes deployment configuration

## Project structure

```text
taskflow-fullstack-assessment/
├── backend/
│   ├── Configuration/
│   ├── Controllers/
│   ├── Data/
│   ├── DTOs/
│   ├── Middleware/
│   ├── Migrations/
│   ├── Models/
│   ├── Repositories/
│   ├── Services/
│   ├── Dockerfile
│   └── Program.cs
├── frontend/
│   ├── src/app/
│   │   ├── core/
│   │   │   ├── guards/
│   │   │   ├── interceptors/
│   │   │   ├── models/
│   │   │   └── services/
│   │   └── features/
│   │       ├── login/
│   │       └── tasks/
│   ├── Dockerfile
│   ├── nginx.conf
│   └── proxy.conf.json
├── k8s/
│   ├── namespace.yaml
│   ├── configmap.yaml
│   ├── secret.yaml.example
│   ├── database.yaml
│   ├── backend.yaml
│   └── frontend.yaml
├── .env.example
├── .gitignore
├── docker-compose.yml
├── README.md
└── DECISIONS.md
```

Local `.env` and `k8s/secret.local.yaml` files are excluded from Git.

## Architecture

```text
Browser
   ↓
nginx / Angular
   ↓ /api/
ASP.NET Core Controller
   ↓
Service
   ↓
Repository
   ↓
EF Core / PostgreSQL
```

nginx serves the Angular production build and forwards `/api/` requests
to the backend.

Controllers handle HTTP requests and responses, services implement
application behavior, and repositories handle database access.

See [DECISIONS.md](DECISIONS.md) for design choices and limitations.

## Environment variables

| Variable | Purpose |
|---|---|
| POSTGRES_USER | Database username |
| POSTGRES_DB | Database name |
| POSTGRES_PASSWORD | Database password |
| JWT_ISSUER | Expected JWT issuer |
| JWT_AUDIENCE | Expected JWT audience |
| JWT_SIGNING_KEY | Secret used to sign and validate JWTs |
| JWT_EXPIRY_MINUTES | Token lifetime in minutes |
| AUTH_EMAIL | Assessment user's login email |
| AUTH_PASSWORD | Assessment user's login password |

Docker Compose reads `.env` and passes configuration into containers.
ASP.NET Core reads the container environment through its configuration
system.

Double underscores represent nested configuration keys. For example,
`Jwt__Issuer` becomes `Jwt:Issuer`.

Apply environment changes with:

```powershell
docker compose up -d
```

Existing shell environment variables can override matching `.env` values.
Use a terminal without conflicting variables.

## Database migrations

Migration files are included under `backend/Migrations`.

The backend calls `Database.MigrateAsync()` during startup to apply
pending migrations automatically. No manual migration command is required
for the Docker Compose setup.

Do not use `EnsureCreated()` alongside this migration workflow.

Migration failures stop backend startup and appear in the backend logs.

## API endpoints

| Method | Endpoint | Authentication | Purpose |
|---|---|---|---|
| POST | /api/auth/login | Public | Obtain JWT |
| GET | /api/health | Public | Check API process |
| GET | /api/tasks | JWT | List tasks |
| GET | /api/tasks/{id} | JWT | Retrieve one task |
| POST | /api/tasks | JWT | Create task |
| PUT | /api/tasks/{id} | JWT | Update task details |
| PATCH | /api/tasks/{id}/status | JWT | Toggle Todo/Done |
| DELETE | /api/tasks/{id} | JWT | Delete task |

### Login request

```json
{
  "email": "admin@taskflow.com",
  "password": "Admin@123"
}
```

The response contains:

```json
{
  "token": "<JWT>",
  "expiresAtUtc": "<UTC expiry timestamp>"
}
```

Send the token with protected requests:

```http
Authorization: Bearer <token>
```

### Create or update request

```json
{
  "title": "Prepare for technical interview",
  "description": "Review application architecture",
  "priority": "High",
  "dueDate": null
}
```

For a due date, use a calendar date string in `YYYY-MM-DD` format.

Task rules:

- Title is required and limited to 200 characters.
- Description is optional and limited to 200 characters.
- Priority is Low, Medium, or High.
- Due date is optional.
- New tasks start with Todo status.
- Editing task details preserves the current status.
- The status endpoint toggles between Todo and Done.

The Angular form prevents dates before the user's current local date.
This additional date restriction is currently enforced only by the
frontend.

### Response codes

| Status | Meaning |
|---|---|
| 200 | Successful login, retrieval, update, or status change |
| 201 | Task created |
| 204 | Task deleted |
| 400 | Invalid input |
| 401 | Invalid credentials or missing/invalid/expired token |
| 404 | Task or route not found |
| 500 | Unexpected server error |

Errors use JSON Problem Details. Validation errors include an `errors`
object, and error responses include a trace ID for troubleshooting.

## Swagger

Open http://localhost:5000/swagger.

1. Execute `POST /api/auth/login`.
2. Copy the returned token.
3. Click Authorize.
4. Paste only the token, without quotes or the word Bearer.
5. Execute a task endpoint.

## Development

### Frontend

From the repository root, start the backend and database:

```powershell
docker compose up --build -d backend
```

Then start Angular:

```powershell
cd frontend
npm install
npm start
```

Open http://localhost:4200.

The Angular development proxy forwards `/api/` requests to
http://localhost:5000. The Docker frontend uses nginx for API forwarding.

Login storage is separate for ports 4200, 8080, and 8081 because they are
different browser origins.

### Backend build

With the .NET 10 SDK installed:

```powershell
cd backend
dotnet build
```

Running the backend outside Docker requires supplying its database, JWT,
and authentication configuration separately. ASP.NET Core does not
automatically read the Compose `.env` file.

### Frontend build and tests

From `frontend`:

```powershell
npm run build
npm test -- --watch=false
```

The current automated frontend test checks application creation.
Functional behavior should also be verified using the checklist below.

## Docker troubleshooting

Check services:

```powershell
docker compose ps
```

Follow backend logs:

```powershell
docker compose logs -f backend
```

View recent frontend and database logs:

```powershell
docker compose logs --tail=100 frontend database
```

Common issues:

- Port conflict: stop the conflicting process or adjust the published port.
- Missing configuration: replace placeholders in `.env`.
- Database authentication failure: confirm credentials match the existing
  database.
- Initial proxy error: wait for backend startup and migrations to finish.
- Source changes not appearing: rebuild with `docker compose up --build -d`.

## Kubernetes deployment

The manifests use the `taskflow` namespace.

Kubernetes has its own PostgreSQL storage, separate from Docker Compose,
so its task list may initially be empty.

### 1. Check the cluster

```powershell
kubectl config current-context
kubectl get nodes
```

Confirm the context points to the intended cluster and the nodes are Ready.

For Docker Desktop, enable Kubernetes and select its context if needed:

```powershell
kubectl config use-context docker-desktop
```

### 2. Build images

From the repository root:

```powershell
docker build -t taskflow-backend:local ./backend
docker build -t taskflow-frontend:local ./frontend
```

The manifests use `imagePullPolicy: IfNotPresent`.

Ensure these images are available to the cluster's nodes. Depending on the
local cluster, this may require loading the images into its runtime.
For a remote cluster, push images to a registry and update the image
references in the manifests.

### 3. Configure secrets

On first setup:

```powershell
Copy-Item k8s/secret.yaml.example k8s/secret.local.yaml
```

Replace the placeholders using your configured values:

| Kubernetes Secret key | Corresponding `.env` variable |
|---|---|
| POSTGRES_PASSWORD | POSTGRES_PASSWORD |
| Jwt__SigningKey | JWT_SIGNING_KEY |
| Auth__Password | AUTH_PASSWORD |

Keep `secret.local.yaml` excluded from Git.

Review `k8s/configmap.yaml` for database names, login email, and JWT
configuration. Kubernetes does not automatically read the Compose `.env`.

### 4. Deploy

```powershell
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/secret.local.yaml
```

Deploy PostgreSQL and wait:

```powershell
kubectl apply -f k8s/database.yaml
kubectl -n taskflow rollout status deployment/database --timeout=180s
```

Deploy the backend and wait:

```powershell
kubectl apply -f k8s/backend.yaml
kubectl -n taskflow rollout status deployment/backend --timeout=300s
```

Deploy the frontend and wait:

```powershell
kubectl apply -f k8s/frontend.yaml
kubectl -n taskflow rollout status deployment/frontend --timeout=180s
```

Resolve any rollout failure before continuing to the next component.

### 5. Access

```powershell
kubectl -n taskflow port-forward service/frontend 8081:80
```

Keep the terminal open and visit http://localhost:8081.

### 6. Check resources and logs

```powershell
kubectl -n taskflow get pods,services,pvc
kubectl -n taskflow logs deployment/backend
kubectl -n taskflow describe pods
```

Expected state:

- All application Pods are Running and Ready.
- The PostgreSQL PVC is Bound.

Troubleshooting:

- ImagePullBackOff: check image availability and image references.
- Pending PVC: check the cluster's StorageClass and storage provisioner.
- Backend startup failure: check configuration and migration logs.

### 7. Stop and restart

To stop browser access, press Ctrl+C in the terminal running
`kubectl port-forward`. This does not stop the application.

To stop TaskFlow's containers while keeping database storage:

```powershell
kubectl -n taskflow scale deployment frontend backend database --replicas=0
```

To restart, start PostgreSQL first:

```powershell
kubectl -n taskflow scale deployment database --replicas=1
kubectl -n taskflow rollout status deployment/database --timeout=180s
```

Then start the backend and frontend:

```powershell
kubectl -n taskflow scale deployment backend frontend --replicas=1
kubectl -n taskflow rollout status deployment/backend --timeout=300s
kubectl -n taskflow rollout status deployment/frontend --timeout=180s
```

Restore browser access:

```powershell
kubectl -n taskflow port-forward service/frontend 8081:80
```

Scaling to zero keeps the database's PersistentVolumeClaim.
Do not delete the namespace or PVC if you want to retain database data.

Applying the Deployment manifests again restores their configured
replica counts.

## Verification checklist

- Valid login returns a token.
- Invalid login returns 401.
- Protected endpoints reject missing, invalid, and expired tokens.
- Create, retrieve, update, toggle, and delete work.
- Invalid task input returns 400.
- Missing task IDs return 404.
- Loading states and friendly errors appear in the frontend.
- Data survives a normal restart.
- Direct frontend routes work after browser refresh.
- A fresh database receives migrations during startup.
- Docker Compose starts all three services.
- Kubernetes Pods become Ready and database storage is Bound.

## Scope and limitations

- One configured assessment user; no registration or users table.
- JWTs are stored in localStorage.
- No refresh tokens or server-side token revocation.
- Local demonstration uses HTTP.
- Health probes check API responsiveness, not ongoing database connectivity.
- Kubernetes PostgreSQL uses a single replica.
- Ingress, database high availability, and backups are not implemented.

See [DECISIONS.md](DECISIONS.md) for further explanation.