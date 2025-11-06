# Python Service Skeleton

A Python FastAPI-based service skeleton that mirrors the Node.js service structure.

## Features

- ✅ FastAPI web framework
- ✅ Environment variable configuration (.env support)
- ✅ Health check endpoints
- ✅ User API endpoints (controllers and routers)
- ✅ Database seeding functionality
- ✅ Standardized API responses
- ✅ CORS middleware support

## Project Structure

```
python_service/
├── .env.example              # Environment variables template
├── app.py                    # Main FastAPI application
├── main.py                   # Application entry point
├── requirements.txt          # Python dependencies
├── config/                   # Configuration module
│   ├── __init__.py
│   └── config.py             # Configuration manager
├── api/                      # API modules
│   ├── common/               # Common utilities
│   │   └── response_handler.py
│   ├── health/               # Health check endpoints
│   │   ├── health_controller.py
│   │   └── health_routes.py
│   └── user/                 # User endpoints
│       ├── user_controller.py
│       └── user_routes.py
└── database/                 # Database modules
    ├── seeds/                # Seeding functionality
    │   ├── base_seeder.py
    │   ├── user_seeder.py
    │   ├── tenant_seeder.py
    │   ├── role_seeder.py
    │   ├── client_app_seeder.py
    │   └── database_seeder.py
    └── __init__.py
```

## Installation

1. **Create virtual environment** (recommended):
```bash
python -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate
```

2. **Install dependencies**:
```bash
pip install -r requirements.txt
```

3. **Set up environment variables**:
```bash
cp .env.example .env
# Edit .env with your configuration
```

## Configuration

Copy `.env.example` to `.env` and configure:

```bash
# Service Configuration
SERVICE_NAME=service-skeleton
NODE_ENV=development
PORT=3000
BASE_URL=http://localhost:3000

# Database Configuration
DB_DIALECT=postgres
DB_HOST=localhost
DB_PORT=5432
DB_NAME=service_skeleton
DB_USER_NAME=postgres
DB_USER_PASSWORD=password

# JWT Configuration
JWT_ACCESS_SECRET=your-secret-key
JWT_REFRESH_SECRET=your-refresh-secret-key
```

## Running the Service

### Development Mode

```bash
python main.py
```

Or using uvicorn directly:

```bash
uvicorn app:app --reload --port 3000
```

### Production Mode

```bash
uvicorn app:app --host 0.0.0.0 --port 3000 --workers 4
```

## API Endpoints

### Health Check

- `GET /health-check` - Health check (alternative route)
- `GET /api/v1/health` - Health check endpoint

### User Endpoints

- `POST /api/v1/users` - Create user
- `GET /api/v1/users/profile` - Get profile
- `PUT /api/v1/users/profile` - Update profile
- `GET /api/v1/users/search` - Search users
- `GET /api/v1/users/{id}` - Get user by ID
- `PUT /api/v1/users/{id}` - Update user by ID
- `DELETE /api/v1/users/{id}` - Delete user by ID

## Database Seeding

See `database/seeds/README.md` for details on seeding the database.

```bash
# After implementing database connection
python database/seeds/seed_database.py
```

## Development

### Adding New Endpoints

1. Create controller in `api/{module}/{module}_controller.py`
2. Create routes in `api/{module}/{module}_routes.py`
3. Include router in `app.py`:
```python
from api.{module} import {module}_routes
app.include_router({module}_routes.router, prefix="/api/v1")
```

### Environment Variables

Access configuration via `config.config.Config`:

```python
from config.config import Config

port = Config.PORT
db_url = Config.get_database_url()
```

## Next Steps

1. Implement database models and services
2. Add authentication middleware
3. Implement actual business logic in controllers
4. Add input validation (Pydantic models)
5. Add database connection setup
6. Add logging configuration
7. Add unit tests

## Notes

- Controllers currently have placeholder implementations
- Database services need to be implemented based on your ORM
- Authentication middleware needs to be added
- Update routes to include proper middleware for authentication and permissions

