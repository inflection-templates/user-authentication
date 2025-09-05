"""
User authentication routes
Equivalent to the .NET shala.api.api.users.auth.UserAuthRoutes
"""

import logging
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
from .handlers import AuthHandler

logger = logging.getLogger(__name__)

auth_router = APIRouter()
security = HTTPBearer()

# Dependencies
def get_jwt_service() -> JwtTokenService:
    return JwtTokenService()

def get_auth_service(db_session: AsyncSession = Depends(get_db_session)) -> UserAuthService:
    return UserAuthService(db_session)

def get_auth_handler(
    auth_service: UserAuthService = Depends(get_auth_service),
    jwt_service: JwtTokenService = Depends(get_jwt_service)
) -> AuthHandler:
    return AuthHandler(auth_service, jwt_service)


@auth_router.post("/login", response_model=LoginResponseModel)
async def login_with_password(
    login_data: UserPasswordLoginModel,
    handler: AuthHandler = Depends(get_auth_handler)
):
    """
    Authenticate user with email and password
    """
    return await handler.login_with_password(login_data)


@auth_router.post("/login/otp", response_model=LoginResponseModel)
async def login_with_otp(
    login_data: UserOtpLoginModel,
    handler: AuthHandler = Depends(get_auth_handler)
):
    """
    Authenticate user with email and OTP
    """
    return await handler.login_with_otp(login_data)


@auth_router.post("/send-otp", response_model=ApiResponse)
async def send_otp(
    otp_request: UserSendOtpModel,
    handler: AuthHandler = Depends(get_auth_handler)
):
    """
    Send OTP to user's email
    """
    return await handler.send_otp(otp_request)


@auth_router.post("/register", response_model=LoginResponseModel)
async def register_user(
    registration_data: UserRegistrationModel,
    handler: AuthHandler = Depends(get_auth_handler)
):
    """
    Register a new user
    """
    return await handler.register_user(registration_data)


@auth_router.post("/refresh", response_model=TokenResponseModel)
async def refresh_token(
    refresh_data: UserRefreshTokenModel,
    handler: AuthHandler = Depends(get_auth_handler)
):
    """
    Refresh access token using refresh token
    """
    return await handler.refresh_token(refresh_data)


@auth_router.post("/logout", response_model=ApiResponse)
async def logout(
    credentials: HTTPAuthorizationCredentials = Depends(security),
    handler: AuthHandler = Depends(get_auth_handler)
):
    """
    Logout user and invalidate session
    """
    return await handler.logout(credentials)


@auth_router.post("/password/reset/send-link", response_model=ApiResponse)
async def send_password_reset_link(
    reset_request: UserResetPasswordSendLinkModel,
    handler: AuthHandler = Depends(get_auth_handler)
):
    """
    Send password reset link to user's email
    """
    return await handler.send_password_reset_link(reset_request)


@auth_router.post("/password/reset", response_model=ApiResponse)
async def reset_password(
    reset_data: UserResetPasswordModel,
    handler: AuthHandler = Depends(get_auth_handler)
):
    """
    Reset user password using reset token
    """
    return await handler.reset_password(reset_data)


@auth_router.post("/password/change", response_model=ApiResponse)
async def change_password(
    password_data: UserChangePasswordModel,
    credentials: HTTPAuthorizationCredentials = Depends(security),
    handler: AuthHandler = Depends(get_auth_handler)
):
    """
    Change user password (requires authentication)
    """
    return await handler.change_password(password_data, credentials)
