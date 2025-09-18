"""
Main API router
Equivalent to the .NET shala.api.api.Router
"""

from fastapi import APIRouter

# Import route modules
from api.users.auth.routes import auth_router
from api.users.users.routes import users_router
from api.users.social_auth.github_routes import router as github_oauth_router
from api.users.social_auth.google_routes import router as google_oauth_router
from api.users.social_auth.twitter_routes import router as twitter_oauth_router
from api.roles.routes import roles_router

# Create main API router
api_router = APIRouter()

# Include route modules
api_router.include_router(auth_router, prefix="/auth", tags=["Authentication"])
api_router.include_router(users_router, prefix="/users", tags=["Users"])
api_router.include_router(github_oauth_router, prefix="/users", tags=["GitHub OAuth"])
api_router.include_router(google_oauth_router, prefix="/users", tags=["Google OAuth"])
api_router.include_router(twitter_oauth_router, prefix="/users", tags=["Twitter OAuth"])
api_router.include_router(roles_router, prefix="/roles", tags=["Roles"])
