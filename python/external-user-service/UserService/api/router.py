"""
Main API router
Equivalent to the .NET shala.api.api.Router
"""

from fastapi import APIRouter

# Import route modules
from api.users.auth.routes import auth_router
from api.users.users.routes import users_router
from api.roles.routes import roles_router

# Create main API router
api_router = APIRouter()

# Include route modules
api_router.include_router(auth_router, prefix="/auth", tags=["Authentication"])
api_router.include_router(users_router, prefix="/users", tags=["Users"])
api_router.include_router(roles_router, prefix="/roles", tags=["Roles"])
