"""
Google OAuth Handler - Direct implementation similar to C# version
"""

import logging
import httpx
import json
from datetime import datetime
from typing import Optional, Dict, Any
from uuid import uuid4
from fastapi import HTTPException
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select

from database.entities.user_entities import UserEntity
from services.user_auth_service import UserAuthService
from services.jwt_token_service import JwtTokenService
from domain.types.user_types import UserRegistrationModel, GenderType

logger = logging.getLogger(__name__)


class GoogleOAuthHandler:
    """Google OAuth handler - simple implementation like C# version"""
    
    def __init__(self, db_session: AsyncSession, config: dict):
        self.db_session = db_session
        self.config = config
        self.user_auth_service = UserAuthService(db_session)
        self.jwt_service = JwtTokenService()
    
    async def google_redirect(self, code: str, state: str) -> dict:
        """Handle Google OAuth redirect - equivalent to C# GoogleRedirect method"""
        try:
            # Get configuration
            google_config = self.config.get("oauth", {}).get("google", {})
            client_id = google_config.get("client_id")
            client_secret = google_config.get("client_secret")
            redirect_uri = google_config.get("callback_url")
            
            if not client_id or not client_secret:
                raise HTTPException(
                    status_code=500,
                    detail="Google OAuth not properly configured"
                )
            
            # Exchange code for access token
            token_url = "https://oauth2.googleapis.com/token"
            
            token_data = {
                "client_id": client_id,
                "client_secret": client_secret,
                "code": code,
                "grant_type": "authorization_code",
                "redirect_uri": redirect_uri
            }
            
            async with httpx.AsyncClient() as client:
                response = await client.post(token_url, data=token_data)
                
                if not response.is_success:
                    raise HTTPException(
                        status_code=500,
                        detail="Unable to retrieve Google access token."
                    )
                
                response_text = response.text
                if not response_text:
                    raise HTTPException(
                        status_code=500,
                        detail="Unable to parse Google authorization response."
                    )
                
                try:
                    response_model = json.loads(response_text)
                except json.JSONDecodeError:
                    raise HTTPException(
                        status_code=500,
                        detail="Unable to deserialize Google response."
                    )
                
                google_access_token = response_model.get("access_token")
                if not google_access_token:
                    raise HTTPException(
                        status_code=500,
                        detail="No access token in Google response."
                    )
            
            # Get Google user info
            google_user = await self._get_google_user(google_access_token)
            if not google_user:
                raise HTTPException(
                    status_code=500,
                    detail="Unable to retrieve Google user's information."
                )
            
            # Get user email
            user_email = google_user.get("email")
            if not user_email:
                raise HTTPException(
                    status_code=500,
                    detail="No email found in Google user profile."
                )
            
            # Check if user already exists
            existing_user = await self._get_user_by_email(user_email)
            
            if existing_user:
                # User exists - generate login session
                session = await self.user_auth_service._create_login_session(existing_user.id, False)
                await self.db_session.commit()
                
                # Generate JWT token
                access_token = self.jwt_service.generate_token(existing_user, session.session_id)
                
                return {
                    "success": True,
                    "message": f"User {existing_user.username} logged in successfully!",
                    "data": {
                        "token": access_token,
                        "user_id": existing_user.id,
                        "username": existing_user.username,
                        "session_id": session.id,
                        "valid_till": session.started_at
                    }
                }
            else:
                # Create new user
                name = google_user.get("name", "")
                name_tokens = name.split() if name else []
                first_name = google_user.get("given_name") or (name_tokens[0] if name_tokens else "")
                last_name = google_user.get("family_name") or (name_tokens[1] if len(name_tokens) > 1 else "")
                
                # Generate username from email if not provided
                username = user_email.split("@")[0] if user_email else ""
                
                registration_data = UserRegistrationModel(
                    email=user_email,
                    password=self._generate_random_string(12),  # Random password for OAuth users
                    first_name=first_name,
                    last_name=last_name,
                    username=username,
                )
                
                # Register new user
                result = await self.user_auth_service.register_user(registration_data)
                if not result:
                    raise HTTPException(
                        status_code=500,
                        detail="Unable to create a new user"
                    )
                
                created_user, session = result
                
                # Generate JWT token
                access_token = self.jwt_service.generate_token(created_user, session.session_id)
                
                return {
                    "success": True,
                    "message": f"User {created_user.username} logged in successfully!",
                    "data": {
                        "token": access_token,
                        "user_id": created_user.id,
                        "username": created_user.username,
                        "session_id": session.id,
                        "valid_till": session.started_at
                    }
                }
                
        except HTTPException:
            raise
        except Exception as e:
            logger.error(f"Google OAuth error: {e}")
            raise HTTPException(
                status_code=500,
                detail=f"Internal server error: {str(e)}"
            )
    
    async def _get_google_user(self, token: str) -> Optional[Dict[str, Any]]:
        """Get Google user info - equivalent to C# GetGoogleUser method"""
        try:
            api_url = "https://www.googleapis.com/oauth2/v2/userinfo"
            
            async with httpx.AsyncClient() as client:
                headers = {"Authorization": f"Bearer {token}"}
                response = await client.get(api_url, headers=headers)
                
                if response.is_success:
                    logger.info(f"Google user response: {response.text}")
                    return response.json()
                else:
                    logger.error(f"Google user API error: {response.text}")
                    return None
                    
        except Exception as e:
            logger.error(f"Error getting Google user: {e}")
            return None
    
    async def _get_user_by_email(self, email: str) -> Optional[UserEntity]:
        """Get user by email"""
        try:
            result = await self.db_session.execute(
                select(UserEntity).where(UserEntity.email == email)
            )
            return result.scalar_one_or_none()
        except Exception as e:
            logger.error(f"Error getting user by email: {e}")
            return None
    
    def _generate_random_string(self, length: int) -> str:
        """Generate random string for OAuth user passwords"""
        import secrets
        import string
        alphabet = string.ascii_letters + string.digits
        return ''.join(secrets.choice(alphabet) for _ in range(length))
