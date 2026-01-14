"""
User management routes
Equivalent to the .NET shala.api.api.users.users.UserRoutes
"""

import logging
from fastapi import APIRouter, HTTPException, Depends, status
from fastapi.security import HTTPBearer, HTTPAuthorizationCredentials
from sqlalchemy.ext.asyncio import AsyncSession

from database.db_context import get_db_session
from domain.types.user_types import (
    UserRegistrationModel, LoginResponseModel, UserRegistrationResponseModel
)
from services.user_auth_service import UserAuthService
from services.jwt_token_service import JwtTokenService
from .handlers import UserHandler

logger = logging.getLogger(__name__)

users_router = APIRouter()
security = HTTPBearer()

# Dependencies
def get_jwt_service() -> JwtTokenService:
    return JwtTokenService()

def get_auth_service(db_session: AsyncSession = Depends(get_db_session)) -> UserAuthService:
    return UserAuthService(db_session)

def get_user_handler(
    auth_service: UserAuthService = Depends(get_auth_service),
    jwt_service: JwtTokenService = Depends(get_jwt_service)
) -> UserHandler:
    return UserHandler(auth_service, jwt_service)

def get_current_user_id(
    credentials: HTTPAuthorizationCredentials = Depends(security),
    jwt_service: JwtTokenService = Depends(get_jwt_service)
) -> str:
    """Extract user ID from JWT token"""
    claims = jwt_service.validate_token(credentials.credentials)
    return claims.get("sub")

@users_router.post("", response_model=UserRegistrationResponseModel)
async def register_user(
    registration_data: UserRegistrationModel,
    handler: UserHandler = Depends(get_user_handler)
):
    """
    Register a new user (without creating session or tokens)
    """
    return await handler.register_user_only(registration_data)

@users_router.get("/me")
async def get_current_user(
    user_id: str = Depends(get_current_user_id),
    handler: UserHandler = Depends(get_user_handler)
):
    """Get current user profile (requires authentication)"""
    return await handler.get_current_user(user_id)

@users_router.put("/me") 
async def update_current_user(
    update_data: dict,
    user_id: str = Depends(get_current_user_id),
    handler: UserHandler = Depends(get_user_handler)
):
    """Update current user profile (requires authentication)"""
    return await handler.update_current_user(user_id, update_data)
