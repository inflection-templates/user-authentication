"""
Twitter OAuth Handler - Direct implementation similar to C# version
"""

import logging
import httpx
import json
import base64
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


class TwitterOAuthHandler:
    """Twitter OAuth handler - simple implementation like C# version"""
    
    def __init__(self, db_session: AsyncSession, config: dict):
        self.db_session = db_session
        self.config = config
        self.user_auth_service = UserAuthService(db_session)
        self.jwt_service = JwtTokenService()
    
    async def twitter_redirect(self, code: str, state: str) -> dict:
        """Handle Twitter OAuth redirect - equivalent to C# TwitterRedirect method"""
        try:
            # Get configuration
            twitter_config = self.config.get("oauth", {}).get("twitter", {})
            client_id = twitter_config.get("client_id")
            client_secret = twitter_config.get("client_secret")
            redirect_uri = twitter_config.get("callback_url")
            
            if not client_id or not client_secret:
                raise HTTPException(
                    status_code=500,
                    detail="Twitter OAuth not properly configured"
                )
            
            # Exchange code for access token using OAuth 2.0 PKCE flow
            token_url = "https://api.twitter.com/2/oauth2/token"
            
            # Create Basic Auth header for client credentials
            credentials = f"{client_id}:{client_secret}"
            encoded_credentials = base64.b64encode(credentials.encode()).decode()
            
            token_data = {
                "code": code,
                "grant_type": "authorization_code",
                "client_id": client_id,
                "redirect_uri": redirect_uri,
                "code_verifier": "challenge"  # In production, this should be stored and retrieved
            }
            
            headers = {
                "Authorization": f"Basic {encoded_credentials}",
                "Content-Type": "application/x-www-form-urlencoded"
            }
            
            async with httpx.AsyncClient() as client:
                response = await client.post(token_url, data=token_data, headers=headers)
                
                if not response.is_success:
                    logger.error(f"Twitter token exchange failed: {response.text}")
                    raise HTTPException(
                        status_code=500,
                        detail="Unable to retrieve Twitter access token."
                    )
                
                response_text = response.text
                if not response_text:
                    raise HTTPException(
                        status_code=500,
                        detail="Unable to parse Twitter authorization response."
                    )
                
                try:
                    response_model = json.loads(response_text)
                except json.JSONDecodeError:
                    raise HTTPException(
                        status_code=500,
                        detail="Unable to deserialize Twitter response."
                    )
                
                twitter_access_token = response_model.get("access_token")
                if not twitter_access_token:
                    raise HTTPException(
                        status_code=500,
                        detail="No access token in Twitter response."
                    )
            
            # Get Twitter user info
            twitter_user = await self._get_twitter_user(twitter_access_token)
            if not twitter_user:
                raise HTTPException(
                    status_code=500,
                    detail="Unable to retrieve Twitter user's information."
                )
            
            # Extract user information
            user_data = twitter_user.get("data", {})
            username = user_data.get("username", "")
            name = user_data.get("name", "")
            twitter_id = user_data.get("id", "")
            
            # Generate email since Twitter API v2 doesn't provide email by default
            # In production, you might want to ask user for email or use a different approach
            user_email = f"{username}@twitter.oauth.user" if username else f"user_{twitter_id}@twitter.oauth.user"
            
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
                name_tokens = name.split() if name else []
                first_name = name_tokens[0] if name_tokens else ""
                last_name = name_tokens[1] if len(name_tokens) > 1 else ""
                
                registration_data = UserRegistrationModel(
                    email=user_email,
                    password=self._generate_random_string(12),  # Random password for OAuth users
                    first_name=first_name,
                    last_name=last_name,
                    username=username or f"twitter_user_{twitter_id}",
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
            logger.error(f"Twitter OAuth error: {e}")
            raise HTTPException(
                status_code=500,
                detail=f"Internal server error: {str(e)}"
            )
    
    async def _get_twitter_user(self, token: str) -> Optional[Dict[str, Any]]:
        """Get Twitter user info - equivalent to C# GetTwitterUser method"""
        try:
            # Twitter API v2 endpoint for user information
            api_url = "https://api.twitter.com/2/users/me?user.fields=id,name,username,profile_image_url,public_metrics"
            
            async with httpx.AsyncClient() as client:
                headers = {"Authorization": f"Bearer {token}"}
                response = await client.get(api_url, headers=headers)
                
                if response.is_success:
                    logger.info(f"Twitter user response: {response.text}")
                    return response.json()
                else:
                    logger.error(f"Twitter user API error: {response.text}")
                    return None
                    
        except Exception as e:
            logger.error(f"Error getting Twitter user: {e}")
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
