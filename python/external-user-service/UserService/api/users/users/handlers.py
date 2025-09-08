"""
User management handlers
Business logic for user management operations
"""

import logging
from fastapi import HTTPException, status
from sqlalchemy.ext.asyncio import AsyncSession

from domain.types.user_types import (
    UserRegistrationModel, LoginResponseModel, UserRegistrationResponseModel
)
from services.user_auth_service import UserAuthService
from services.jwt_token_service import JwtTokenService

logger = logging.getLogger(__name__)

class UserHandler:
    def __init__(self, auth_service: UserAuthService, jwt_service: JwtTokenService):
        self.auth_service = auth_service
        self.jwt_service = jwt_service

    async def register_user(self, registration_data: UserRegistrationModel) -> LoginResponseModel:
        """
        Register a new user (alternative endpoint for compatibility)
        """
        try:
            result = await self.auth_service.register_user(registration_data)
            
            if not result:
                raise HTTPException(
                    status_code=status.HTTP_400_BAD_REQUEST,
                    detail="User registration failed"
                )
            
            user, session = result
            
            # Generate tokens
            access_token = self.jwt_service.generate_token(user, session.session_id)
            refresh_token = self.jwt_service.generate_refresh_token(user, session.session_id)
            
            logger.info(f"User {user.email} registered successfully via /users endpoint")
            
            return LoginResponseModel.create(
                access_token=access_token,
                refresh_token=refresh_token,
                expires_in=self.jwt_service.access_token_validity_days * 24 * 60 * 60,
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

    async def register_user_only(self, registration_data: UserRegistrationModel) -> UserRegistrationResponseModel:
        """
        Register a new user without creating session or tokens
        """
        try:
            # Use the auth service to register user but without creating session
            user = await self.auth_service.register_user_without_session(registration_data)
            
            if not user:
                raise HTTPException(
                    status_code=status.HTTP_400_BAD_REQUEST,
                    detail="User registration failed"
                )
            
            logger.info(f"User {user.email} registered successfully (no session created)")
            
            return UserRegistrationResponseModel(
                success=True,
                message="User registered successfully. Please login to get access tokens.",
                user=user
            )
            
        except HTTPException:
            raise
        except Exception as e:
            logger.error(f"Registration error: {e}")
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail="Internal server error during registration"
            )

    async def get_current_user(self, user_id: str):
        """Get current user profile (requires authentication)"""
        try:
            user = await self.auth_service.get_user_by_id(user_id)
            if not user:
                raise HTTPException(
                    status_code=status.HTTP_404_NOT_FOUND,
                    detail="User not found"
                )
            return user
        except HTTPException:
            raise
        except Exception as e:
            logger.error(f"Get current user error: {e}")
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail="Internal server error while fetching user"
            )

    async def update_current_user(self, user_id: str, update_data: dict):
        """Update current user profile (requires authentication)"""
        try:
            # TODO: Implement user update logic
            return {"message": "Update current user - implementation needed"}
        except Exception as e:
            logger.error(f"Update current user error: {e}")
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail="Internal server error while updating user"
            )
