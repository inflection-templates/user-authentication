"""
User management routes
Equivalent to the .NET shala.api.api.users.users.UserRoutes
"""

import logging
from fastapi import APIRouter, HTTPException, Depends, status
from sqlalchemy.ext.asyncio import AsyncSession

from database.db_context import get_db_session
from domain.types.user_types import (
    UserRegistrationModel, LoginResponseModel
)
from services.user_auth_service import UserAuthService
from services.jwt_token_service import JwtTokenService

logger = logging.getLogger(__name__)

users_router = APIRouter()

# Dependencies
def get_jwt_service() -> JwtTokenService:
    return JwtTokenService()

def get_auth_service(db_session: AsyncSession = Depends(get_db_session)) -> UserAuthService:
    return UserAuthService(db_session)

@users_router.post("", response_model=LoginResponseModel)
async def register_user(
    registration_data: UserRegistrationModel,
    auth_service: UserAuthService = Depends(get_auth_service),
    jwt_service: JwtTokenService = Depends(get_jwt_service)
):
    """
    Register a new user (alternative endpoint for compatibility)
    """
    try:
        result = await auth_service.register_user(registration_data)
        
        if not result:
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail="User registration failed"
            )
        
        user, session = result
        
        # Generate tokens
        access_token = jwt_service.generate_token(user, session.session_id)
        refresh_token = jwt_service.generate_refresh_token(user, session.session_id)
        
        logger.info(f"User {user.email} registered successfully via /users endpoint")
        
        return LoginResponseModel(
            access_token=access_token,
            refresh_token=refresh_token,
            expires_in=jwt_service.access_token_validity_days * 24 * 60 * 60,
            user=user,
            session_id=str(session.session_id)
        )
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Registration error: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Internal server error during registration"
        )

@users_router.get("/me")
async def get_current_user():
    """Get current user profile (requires authentication)"""
    return {"message": "Get current user - implementation needed"}

@users_router.put("/me") 
async def update_current_user():
    """Update current user profile (requires authentication)"""
    return {"message": "Update current user - implementation needed"}
