"""
GitHub OAuth Handler - Direct implementation similar to C# version
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


class GitHubOAuthHandler:
    """GitHub OAuth handler - simple implementation like C# version"""
    
    def __init__(self, db_session: AsyncSession, config: dict):
        self.db_session = db_session
        self.config = config
        self.user_auth_service = UserAuthService(db_session)
        self.jwt_service = JwtTokenService()
    
    async def github_redirect(self, code: str, state: str) -> dict:
        """Handle GitHub OAuth redirect - equivalent to C# GithubRedirect method"""
        try:
            # Get configuration
            github_config = self.config.get("oauth", {}).get("github", {})
            client_id = github_config.get("client_id")
            client_secret = github_config.get("client_secret")
            redirect_uri = github_config.get("callback_url")
            
            if not client_id or not client_secret:
                raise HTTPException(
                    status_code=500,
                    detail="GitHub OAuth not properly configured"
                )
            
            # Exchange code for access token
            token_url = f"https://github.com/login/oauth/access_token?client_id={client_id}&client_secret={client_secret}&code={code}"
            
            async with httpx.AsyncClient() as client:
                client.headers.update({"Accept": "application/json"})
                response = await client.post(token_url, content="")
                
                if not response.is_success:
                    raise HTTPException(
                        status_code=500,
                        detail="Unable to retrieve GitHub access token."
                    )
                
                response_text = response.text
                if not response_text:
                    raise HTTPException(
                        status_code=500,
                        detail="Unable to parse GitHub authorization response."
                    )
                
                try:
                    response_model = json.loads(response_text)
                except json.JSONDecodeError:
                    raise HTTPException(
                        status_code=500,
                        detail="Unable to deserialize GitHub response."
                    )
                
                github_access_token = response_model.get("access_token")
                if not github_access_token:
                    raise HTTPException(
                        status_code=500,
                        detail="No access token in GitHub response."
                    )
            
            # Get GitHub user info
            github_user = await self._get_github_user(github_access_token)
            if not github_user:
                raise HTTPException(
                    status_code=500,
                    detail="Unable to retrieve GitHub user's information."
                )
            
            # Get user email
            user_email = await self._get_github_user_email(github_access_token)
            if not user_email:
                user_email = github_user.get("email")
                if not user_email:
                    user_email = f"{github_user.get('login')}@github.oauth.user"
            
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
                name_tokens = github_user.get("name", "").split() if github_user.get("name") else []
                first_name = name_tokens[0] if name_tokens else ""
                last_name = name_tokens[1] if len(name_tokens) > 1 else ""
                
                registration_data = UserRegistrationModel(
                    email=user_email,
                    password=self._generate_random_string(12),  # Random password for OAuth users
                    first_name=first_name,
                    last_name=last_name,
                    username=github_user.get("login", ""),
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
            logger.error(f"GitHub OAuth error: {e}")
            raise HTTPException(
                status_code=500,
                detail=f"Internal server error: {str(e)}"
            )
    
    async def _get_github_user(self, token: str) -> Optional[Dict[str, Any]]:
        """Get GitHub user info - equivalent to C# GetGithubUser method"""
        try:
            api_url = "https://api.github.com/user"
            
            async with httpx.AsyncClient() as client:
                headers = {"Authorization": f"Bearer {token}"}
                response = await client.get(api_url, headers=headers)
                
                if response.is_success:
                    logger.info(f"GitHub user response: {response.text}")
                    return response.json()
                else:
                    logger.error(f"GitHub user API error: {response.text}")
                    return None
                    
        except Exception as e:
            logger.error(f"Error getting GitHub user: {e}")
            return None
    
    async def _get_github_user_email(self, token: str) -> Optional[str]:
        """Get GitHub user email - equivalent to C# GetGithubUserEmail method"""
        try:
            api_url = "https://api.github.com/user/emails"
            
            async with httpx.AsyncClient() as client:
                headers = {"Authorization": f"Bearer {token}"}
                response = await client.get(api_url, headers=headers)
                
                if response.is_success:
                    logger.info(f"GitHub emails response: {response.text}")
                    emails = response.json()
                    
                    # Get the primary email or the first verified email
                    primary_email = next((e["email"] for e in emails if e.get("primary")), None)
                    verified_email = next((e["email"] for e in emails if e.get("verified")), None)
                    first_email = emails[0]["email"] if emails else None
                    
                    return primary_email or verified_email or first_email
                else:
                    logger.error(f"GitHub emails API error: {response.text}")
                    return None
                    
        except Exception as e:
            logger.error(f"Error getting GitHub user email: {e}")
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
