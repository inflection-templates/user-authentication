"""
User authentication routes
Equivalent to the .NET shala.api.api.users.auth.UserAuthRoutes
"""

import logging
from datetime import datetime
from uuid import uuid4
from fastapi import APIRouter, HTTPException, Depends, status
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

auth_router = APIRouter()
security = HTTPBearer()

# Dependencies
def get_jwt_service() -> JwtTokenService:
    return JwtTokenService()

def get_auth_service(db_session: AsyncSession = Depends(get_db_session)) -> UserAuthService:
    return UserAuthService(db_session)


@auth_router.post("/login", response_model=LoginResponseModel)
async def login_with_password(
    login_data: UserPasswordLoginModel,
    auth_service: UserAuthService = Depends(get_auth_service),
    jwt_service: JwtTokenService = Depends(get_jwt_service)
):
    """
    Authenticate user with email and password
    """
    try:
        result = await auth_service.authenticate_with_password(
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
        access_token = jwt_service.generate_token(user, session.session_id)
        refresh_token = jwt_service.generate_refresh_token(user, session.session_id)
        
        logger.info(f"User {user.email} logged in successfully")
        
        return LoginResponseModel(
            access_token=access_token,
            refresh_token=refresh_token,
            expires_in=jwt_service.access_token_validity_days * 24 * 60 * 60,  # Convert to seconds
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


@auth_router.post("/login/otp", response_model=LoginResponseModel)
async def login_with_otp(
    login_data: UserOtpLoginModel,
    auth_service: UserAuthService = Depends(get_auth_service),
    jwt_service: JwtTokenService = Depends(get_jwt_service)
):
    """
    Authenticate user with email and OTP
    """
    try:
        result = await auth_service.authenticate_with_otp(
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
        access_token = jwt_service.generate_token(user, session.session_id)
        refresh_token = jwt_service.generate_refresh_token(user, session.session_id)
        
        logger.info(f"User {user.email} logged in with OTP successfully")
        
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
        logger.error(f"OTP login error: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Internal server error during OTP login"
        )


@auth_router.post("/send-otp", response_model=ApiResponse)
async def send_otp(
    otp_request: UserSendOtpModel,
    auth_service: UserAuthService = Depends(get_auth_service)
):
    """
    Send OTP to user's email
    """
    try:
        result = await auth_service.send_otp(otp_request.email, otp_request.purpose)
        
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


@auth_router.post("/register", response_model=LoginResponseModel)
async def register_user(
    registration_data: UserRegistrationModel,
    auth_service: UserAuthService = Depends(get_auth_service),
    jwt_service: JwtTokenService = Depends(get_jwt_service)
):
    """
    Register a new user
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
        
        logger.info(f"User {user.email} registered successfully")
        
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


@auth_router.post("/refresh", response_model=TokenResponseModel)
async def refresh_token(
    refresh_data: UserRefreshTokenModel,
    auth_service: UserAuthService = Depends(get_auth_service),
    jwt_service: JwtTokenService = Depends(get_jwt_service)
):
    """
    Refresh access token using refresh token
    """
    try:
        # Validate refresh token
        claims = jwt_service.validate_token(refresh_data.refresh_token)
        
        if claims.get("token_type") != "refresh":
            raise HTTPException(
                status_code=status.HTTP_401_UNAUTHORIZED,
                detail="Invalid refresh token"
            )
        
        user_id = claims.get("sub")
        session_id = claims.get("session_id")
        
        # Get user and validate session
        user = await auth_service.get_user_by_id(user_id)
        if not user:
            raise HTTPException(
                status_code=status.HTTP_401_UNAUTHORIZED,
                detail="User not found"
            )
        
        session = await auth_service.validate_session(session_id)
        if not session:
            raise HTTPException(
                status_code=status.HTTP_401_UNAUTHORIZED,
                detail="Invalid session"
            )
        
        # Generate new access token
        access_token = jwt_service.generate_token(user, session.session_id)
        
        logger.info(f"Token refreshed for user {user.email}")
        
        return TokenResponseModel(
            access_token=access_token,
            expires_in=jwt_service.access_token_validity_days * 24 * 60 * 60
        )
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Token refresh error: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Internal server error during token refresh"
        )


@auth_router.post("/logout", response_model=ApiResponse)
async def logout(
    credentials: HTTPAuthorizationCredentials = Depends(security),
    auth_service: UserAuthService = Depends(get_auth_service),
    jwt_service: JwtTokenService = Depends(get_jwt_service)
):
    """
    Logout user and invalidate session
    """
    try:
        # Validate token
        claims = jwt_service.validate_token(credentials.credentials)
        session_id = claims.get("session_id")
        
        # End session
        await auth_service.end_session(session_id)
        
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


@auth_router.post("/password/reset/send-link", response_model=ApiResponse)
async def send_password_reset_link(
    reset_request: UserResetPasswordSendLinkModel,
    auth_service: UserAuthService = Depends(get_auth_service)
):
    """
    Send password reset link to user's email
    """
    try:
        result = await auth_service.send_password_reset_link(reset_request.email)
        
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


@auth_router.post("/password/reset", response_model=ApiResponse)
async def reset_password(
    reset_data: UserResetPasswordModel,
    auth_service: UserAuthService = Depends(get_auth_service)
):
    """
    Reset user password using reset token
    """
    try:
        result = await auth_service.reset_password(
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


@auth_router.post("/password/change", response_model=ApiResponse)
async def change_password(
    password_data: UserChangePasswordModel,
    credentials: HTTPAuthorizationCredentials = Depends(security),
    auth_service: UserAuthService = Depends(get_auth_service),
    jwt_service: JwtTokenService = Depends(get_jwt_service)
):
    """
    Change user password (requires authentication)
    """
    try:
        # Validate token
        claims = jwt_service.validate_token(credentials.credentials)
        user_id = claims.get("sub")
        
        result = await auth_service.change_password(
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
