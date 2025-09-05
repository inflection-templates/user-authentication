"""
User authentication handlers
Business logic for user authentication operations
"""

import logging
from datetime import datetime
from uuid import uuid4
from fastapi import HTTPException, Depends, status
from fastapi.security import HTTPBearer, HTTPAuthorizationCredentials
from sqlalchemy.ext.asyncio import AsyncSession

from database.db_context import get_db_session
from domain.types.user_types import (
    UserPasswordLoginModel, UserOtpLoginModel, UserSendOtpModel,
    UserResetPasswordSendLinkModel, UserResetPasswordModel,
    UserChangePasswordModel, UserRefreshTokenModel, UserRegistrationModel,
    LoginResponseModel, TokenResponseModel, ApiResponse
)
from services.user_auth_service import UserAuthService
from services.jwt_token_service import JwtTokenService

logger = logging.getLogger(__name__)

class AuthHandler:
    def __init__(self, auth_service: UserAuthService, jwt_service: JwtTokenService):
        self.auth_service = auth_service
        self.jwt_service = jwt_service

    async def login_with_password(self, login_data: UserPasswordLoginModel) -> LoginResponseModel:
        """
        Authenticate user with email and password
        """
        try:
            result = await self.auth_service.authenticate_with_password(
                login_data.email,
                login_data.password,
                login_data.remember_me
            )
            
            if not result:
                raise HTTPException(
                    status_code=status.HTTP_401_UNAUTHORIZED,
                    detail="Invalid email or password"
                )
            
            user, session = result
            
            # Generate tokens
            access_token = self.jwt_service.generate_token(user, session.session_id)
            refresh_token = self.jwt_service.generate_refresh_token(user, session.session_id)
            
            logger.info(f"User {user.email} logged in successfully")
            
            return LoginResponseModel(
                access_token=access_token,
                refresh_token=refresh_token,
                expires_in=self.jwt_service.access_token_validity_days * 24 * 60 * 60,
                user=user,
                session_id=str(session.session_id)
            )
            
        except HTTPException:
            raise
        except Exception as e:
            logger.error(f"Login error: {e}")
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail="Internal server error during login"
            )

    async def login_with_otp(self, login_data: UserOtpLoginModel) -> LoginResponseModel:
        """
        Authenticate user with email and OTP
        """
        try:
            result = await self.auth_service.authenticate_with_otp(
                login_data.email,
                login_data.otp,
                login_data.session_id
            )
            
            if not result:
                raise HTTPException(
                    status_code=status.HTTP_401_UNAUTHORIZED,
                    detail="Invalid email or OTP"
                )
            
            user, session = result
            
            # Generate tokens
            access_token = self.jwt_service.generate_token(user, session.session_id)
            refresh_token = self.jwt_service.generate_refresh_token(user, session.session_id)
            
            logger.info(f"User {user.email} logged in with OTP successfully")
            
            return LoginResponseModel(
                access_token=access_token,
                refresh_token=refresh_token,
                expires_in=self.jwt_service.access_token_validity_days * 24 * 60 * 60,
                user=user,
                session_id=str(session.session_id)
            )
            
        except HTTPException:
            raise
        except Exception as e:
            logger.error(f"OTP login error: {e}")
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail="Internal server error during OTP login"
            )

    async def send_otp(self, otp_request: UserSendOtpModel) -> ApiResponse:
        """
        Send OTP to user's email
        """
        try:
            result = await self.auth_service.send_otp(otp_request.email, otp_request.purpose)
            
            if result:
                return ApiResponse(
                    success=True,
                    message="OTP sent successfully",
                    data={"email": otp_request.email}
                )
            else:
                raise HTTPException(
                    status_code=status.HTTP_400_BAD_REQUEST,
                    detail="Failed to send OTP"
                )
            
        except HTTPException:
            raise
        except Exception as e:
            logger.error(f"Send OTP error: {e}")
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail="Internal server error while sending OTP"
            )

    async def register_user(self, registration_data: UserRegistrationModel) -> LoginResponseModel:
        """
        Register a new user
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
            
            logger.info(f"User {user.email} registered successfully")
            
            return LoginResponseModel(
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

    async def refresh_token(self, refresh_data: UserRefreshTokenModel) -> TokenResponseModel:
        """
        Refresh access token using refresh token
        """
        try:
            # Validate refresh token
            claims = self.jwt_service.validate_token(refresh_data.refresh_token)
            
            if claims.get("token_type") != "refresh":
                raise HTTPException(
                    status_code=status.HTTP_401_UNAUTHORIZED,
                    detail="Invalid refresh token"
                )
            
            user_id = claims.get("sub")
            session_id = claims.get("session_id")
            
            # Get user and validate session
            user = await self.auth_service.get_user_by_id(user_id)
            if not user:
                raise HTTPException(
                    status_code=status.HTTP_401_UNAUTHORIZED,
                    detail="User not found"
                )
            
            session = await self.auth_service.validate_session(session_id)
            if not session:
                raise HTTPException(
                    status_code=status.HTTP_401_UNAUTHORIZED,
                    detail="Invalid session"
                )
            
            # Generate new access token
            access_token = self.jwt_service.generate_token(user, session.session_id)
            
            logger.info(f"Token refreshed for user {user.email}")
            
            return TokenResponseModel(
                access_token=access_token,
                expires_in=self.jwt_service.access_token_validity_days * 24 * 60 * 60
            )
            
        except HTTPException:
            raise
        except Exception as e:
            logger.error(f"Token refresh error: {e}")
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail="Internal server error during token refresh"
            )

    async def logout(self, credentials: HTTPAuthorizationCredentials) -> ApiResponse:
        """
        Logout user and invalidate session
        """
        try:
            # Validate token
            claims = self.jwt_service.validate_token(credentials.credentials)
            session_id = claims.get("session_id")
            
            # End session
            await self.auth_service.end_session(session_id)
            
            logger.info(f"User logged out, session {session_id} ended")
            
            return ApiResponse(
                success=True,
                message="Logged out successfully"
            )
            
        except HTTPException:
            raise
        except Exception as e:
            logger.error(f"Logout error: {e}")
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail="Internal server error during logout"
            )

    async def send_password_reset_link(self, reset_request: UserResetPasswordSendLinkModel) -> ApiResponse:
        """
        Send password reset link to user's email
        """
        try:
            result = await self.auth_service.send_password_reset_link(reset_request.email)
            
            # Always return success for security reasons (don't reveal if email exists)
            return ApiResponse(
                success=True,
                message="If the email exists, a password reset link has been sent"
            )
            
        except Exception as e:
            logger.error(f"Password reset link error: {e}")
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail="Internal server error while sending password reset link"
            )

    async def reset_password(self, reset_data: UserResetPasswordModel) -> ApiResponse:
        """
        Reset user password using reset token
        """
        try:
            result = await self.auth_service.reset_password(
                reset_data.email,
                reset_data.reset_token,
                reset_data.new_password
            )
            
            if result:
                return ApiResponse(
                    success=True,
                    message="Password reset successfully"
                )
            else:
                raise HTTPException(
                    status_code=status.HTTP_400_BAD_REQUEST,
                    detail="Invalid or expired reset token"
                )
            
        except HTTPException:
            raise
        except Exception as e:
            logger.error(f"Password reset error: {e}")
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail="Internal server error during password reset"
            )

    async def change_password(self, password_data: UserChangePasswordModel, credentials: HTTPAuthorizationCredentials) -> ApiResponse:
        """
        Change user password (requires authentication)
        """
        try:
            # Validate token
            claims = self.jwt_service.validate_token(credentials.credentials)
            user_id = claims.get("sub")
            
            result = await self.auth_service.change_password(
                user_id,
                password_data.current_password,
                password_data.new_password
            )
            
            if result:
                return ApiResponse(
                    success=True,
                    message="Password changed successfully"
                )
            else:
                raise HTTPException(
                    status_code=status.HTTP_400_BAD_REQUEST,
                    detail="Current password is incorrect"
                )
            
        except HTTPException:
            raise
        except Exception as e:
            logger.error(f"Password change error: {e}")
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail="Internal server error during password change"
            )
