"""
Dependencies for FastAPI
"""

import logging
from fastapi import HTTPException, Depends, status
from fastapi.security import HTTPBearer, HTTPAuthorizationCredentials
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select
from typing import Optional
from uuid import UUID

from domain.types.user_types import User
from services.jwt_token_service import JwtTokenService
from database.db_context import get_db_session
from database.entities.user_entities import UserEntity

logger = logging.getLogger(__name__)

security = HTTPBearer()

def get_jwt_service() -> JwtTokenService:
    """Dependency to get JWT service"""
    return JwtTokenService()

async def get_current_user(
    credentials: HTTPAuthorizationCredentials = Depends(security),
    jwt_service: JwtTokenService = Depends(get_jwt_service),
    db_session: AsyncSession = Depends(get_db_session)
) -> User:
    """
    Get current user from JWT token
    Validates the JWT token and fetches user data from database
    """
    try:
        if not credentials or not credentials.credentials:
            raise HTTPException(
                status_code=status.HTTP_401_UNAUTHORIZED,
                detail="Could not validate credentials"
            )
        
        # Validate JWT token
        claims = jwt_service.validate_token(credentials.credentials)
        user_id = claims.get("sub")
        
        if not user_id:
            raise HTTPException(
                status_code=status.HTTP_401_UNAUTHORIZED,
                detail="Invalid token: missing user ID"
            )
        
        # Fetch user from database
        result = await db_session.execute(
            select(UserEntity).where(UserEntity.id == user_id)
        )
        user_entity = result.scalar_one_or_none()
        
        if not user_entity:
            raise HTTPException(
                status_code=status.HTTP_401_UNAUTHORIZED,
                detail="User not found"
            )
        
        if not user_entity.is_active:
            raise HTTPException(
                status_code=status.HTTP_401_UNAUTHORIZED,
                detail="User account is inactive"
            )
        
        # Convert to domain model
        user = User(
            id=user_entity.id,
            email=user_entity.email,
            username=user_entity.username,
            first_name=user_entity.first_name,
            last_name=user_entity.last_name,
            is_active=user_entity.is_active,
            created_at=user_entity.created_at,
            updated_at=user_entity.updated_at,
            last_login=user_entity.last_login
        )
        
        logger.info(f"User authenticated: {user.email} (ID: {user.id})")
        return user
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error in get_current_user: {str(e)}")
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Could not validate credentials"
        )
