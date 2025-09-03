"""
User Authentication Service implementation
Equivalent to the .NET shala.api.services.implementation.UserAuthService
"""

import logging
import secrets
import string
from datetime import datetime, timedelta
from typing import Optional, Tuple
from uuid import uuid4
import bcrypt
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select, and_

from database.entities.user_entities import (
    UserEntity, UserAuthProfileEntity, UserLoginSessionEntity, 
    UserRoleEntity, RoleEntity
)
from domain.types.user_types import (
    User, UserLoginSession, UserRegistrationModel, UserLoginType, UserStatus
)

logger = logging.getLogger(__name__)


class UserAuthService:
    """User authentication service"""
    
    def __init__(self, db_session: AsyncSession):
        self.db_session = db_session
    
    async def authenticate_with_password(
        self, 
        email: str, 
        password: str, 
        remember_me: bool = False
    ) -> Optional[Tuple[User, UserLoginSession]]:
        """Authenticate user with email and password"""
        try:
            # Get user by email
            user_result = await self.db_session.execute(
                select(UserEntity).where(UserEntity.email == email)
            )
            user_entity = user_result.scalar_one_or_none()
            
            if not user_entity or not user_entity.is_active:
                return None
            
            # Get auth profile
            auth_result = await self.db_session.execute(
                select(UserAuthProfileEntity).where(
                    and_(
                        UserAuthProfileEntity.user_id == user_entity.id,
                        UserAuthProfileEntity.login_type == UserLoginType.PASSWORD
                    )
                )
            )
            auth_profile = auth_result.scalar_one_or_none()
            
            if not auth_profile or auth_profile.is_locked:
                return None
            
            # Verify password
            if not self._verify_password(password, auth_profile.password_hash):
                # Increment failed attempts
                auth_profile.failed_login_attempts += 1
                if auth_profile.failed_login_attempts >= 5:
                    auth_profile.is_locked = True
                    auth_profile.locked_until = datetime.utcnow() + timedelta(hours=1)
                
                await self.db_session.commit()
                return None
            
            # Reset failed attempts on successful login
            auth_profile.failed_login_attempts = 0
            auth_profile.is_locked = False
            auth_profile.locked_until = None
            
            # Update last login
            user_entity.last_login = datetime.utcnow()
            user_entity.updated_at = datetime.utcnow()
            
            # Create login session
            session = await self._create_login_session(user_entity.id, remember_me)
            
            await self.db_session.commit()
            
            # Convert to domain models
            user = self._map_user_entity_to_domain(user_entity)
            session_domain = self._map_session_entity_to_domain(session)
            
            logger.info(f"User {email} authenticated successfully")
            return user, session_domain
            
        except Exception as e:
            logger.error(f"Authentication error for {email}: {e}")
            return None
    
    async def authenticate_with_otp(
        self,
        email: str,
        otp: str,
        session_id: Optional[str] = None
    ) -> Optional[Tuple[User, UserLoginSession]]:
        """Authenticate user with email and OTP"""
        # This is a simplified implementation
        # In a real system, you would store OTPs in a cache or database
        # For demo purposes, we'll accept "123456" as a valid OTP
        
        if otp != "123456":
            logger.warning(f"Invalid OTP for {email}")
            return None
        
        try:
            # Get user by email
            user_result = await self.db_session.execute(
                select(UserEntity).where(UserEntity.email == email)
            )
            user_entity = user_result.scalar_one_or_none()
            
            if not user_entity or not user_entity.is_active:
                return None
            
            # Update last login
            user_entity.last_login = datetime.utcnow()
            user_entity.updated_at = datetime.utcnow()
            
            # Create login session
            session = await self._create_login_session(user_entity.id, False)
            
            await self.db_session.commit()
            
            # Convert to domain models
            user = self._map_user_entity_to_domain(user_entity)
            session_domain = self._map_session_entity_to_domain(session)
            
            logger.info(f"User {email} authenticated with OTP successfully")
            return user, session_domain
            
        except Exception as e:
            logger.error(f"OTP authentication error for {email}: {e}")
            return None
    
    async def register_user(
        self, 
        registration_data: UserRegistrationModel
    ) -> Optional[Tuple[User, UserLoginSession]]:
        """Register a new user"""
        try:
            # Check if user already exists
            existing_result = await self.db_session.execute(
                select(UserEntity).where(UserEntity.email == registration_data.email)
            )
            if existing_result.scalar_one_or_none():
                logger.warning(f"User registration failed: {registration_data.email} already exists")
                return None
            
            # Create user entity
            user_entity = UserEntity(
                id=str(uuid4()),
                email=registration_data.email,
                username=registration_data.username,
                first_name=registration_data.first_name,
                last_name=registration_data.last_name,
                phone=registration_data.phone,
                gender=registration_data.gender,
                birth_date=registration_data.birth_date,
                default_timezone="UTC",
                is_active=True,
                status=UserStatus.ACTIVE,
                created_at=datetime.utcnow(),
                updated_at=datetime.utcnow()
            )
            
            self.db_session.add(user_entity)
            await self.db_session.flush()  # Get the user ID
            
            # Create auth profile
            password_hash = self._hash_password(registration_data.password)
            auth_profile = UserAuthProfileEntity(
                id=str(uuid4()),
                user_id=user_entity.id,
                login_type=UserLoginType.PASSWORD,
                password_hash=password_hash,
                last_password_change=datetime.utcnow(),
                failed_login_attempts=0,
                is_locked=False,
                created_at=datetime.utcnow(),
                updated_at=datetime.utcnow()
            )
            
            self.db_session.add(auth_profile)
            
            # Assign default user role
            user_role_result = await self.db_session.execute(
                select(RoleEntity).where(RoleEntity.name == "User")
            )
            user_role = user_role_result.scalar_one_or_none()
            
            if user_role:
                user_role_assignment = UserRoleEntity(
                    id=str(uuid4()),
                    user_id=user_entity.id,
                    role_id=user_role.id,
                    assigned_at=datetime.utcnow(),
                    is_active=True
                )
                self.db_session.add(user_role_assignment)
            
            # Create login session
            session = await self._create_login_session(user_entity.id, False)
            
            await self.db_session.commit()
            
            # Convert to domain models
            user = self._map_user_entity_to_domain(user_entity)
            session_domain = self._map_session_entity_to_domain(session)
            
            logger.info(f"User {registration_data.email} registered successfully")
            return user, session_domain
            
        except Exception as e:
            logger.error(f"User registration error for {registration_data.email}: {e}")
            await self.db_session.rollback()
            return None
    
    async def send_otp(self, email: str, purpose: str = "login") -> bool:
        """Send OTP to user's email"""
        try:
            # Check if user exists
            user_result = await self.db_session.execute(
                select(UserEntity).where(UserEntity.email == email)
            )
            user_entity = user_result.scalar_one_or_none()
            
            if not user_entity:
                logger.warning(f"OTP request for non-existent user: {email}")
                return False
            
            # Generate OTP (in real implementation, store this in cache/database)
            otp = "123456"  # Simplified for demo
            
            # In a real implementation, you would:
            # 1. Generate a random OTP
            # 2. Store it in cache with expiration
            # 3. Send it via email service
            
            logger.info(f"OTP sent to {email} for {purpose} (demo OTP: {otp})")
            return True
            
        except Exception as e:
            logger.error(f"Send OTP error for {email}: {e}")
            return False
    
    async def send_password_reset_link(self, email: str) -> bool:
        """Send password reset link to user's email"""
        try:
            # Check if user exists
            user_result = await self.db_session.execute(
                select(UserEntity).where(UserEntity.email == email)
            )
            user_entity = user_result.scalar_one_or_none()
            
            if not user_entity:
                # Don't reveal if email exists for security
                logger.info(f"Password reset requested for non-existent email: {email}")
                return True
            
            # Generate reset token
            reset_token = secrets.token_urlsafe(32)
            
            # In a real implementation, you would:
            # 1. Store the reset token with expiration
            # 2. Send email with reset link
            
            logger.info(f"Password reset link sent to {email} (demo token: {reset_token})")
            return True
            
        except Exception as e:
            logger.error(f"Send password reset link error for {email}: {e}")
            return False
    
    async def reset_password(self, email: str, reset_token: str, new_password: str) -> bool:
        """Reset user password using reset token"""
        # Simplified implementation for demo
        # In real implementation, validate the reset token from cache/database
        
        if reset_token != "demo_reset_token":
            logger.warning(f"Invalid reset token for {email}")
            return False
        
        try:
            # Get user and auth profile
            user_result = await self.db_session.execute(
                select(UserEntity).where(UserEntity.email == email)
            )
            user_entity = user_result.scalar_one_or_none()
            
            if not user_entity:
                return False
            
            auth_result = await self.db_session.execute(
                select(UserAuthProfileEntity).where(
                    and_(
                        UserAuthProfileEntity.user_id == user_entity.id,
                        UserAuthProfileEntity.login_type == UserLoginType.PASSWORD
                    )
                )
            )
            auth_profile = auth_result.scalar_one_or_none()
            
            if not auth_profile:
                return False
            
            # Update password
            auth_profile.password_hash = self._hash_password(new_password)
            auth_profile.last_password_change = datetime.utcnow()
            auth_profile.failed_login_attempts = 0
            auth_profile.is_locked = False
            auth_profile.locked_until = None
            auth_profile.updated_at = datetime.utcnow()
            
            await self.db_session.commit()
            
            logger.info(f"Password reset successfully for {email}")
            return True
            
        except Exception as e:
            logger.error(f"Password reset error for {email}: {e}")
            return False
    
    async def change_password(self, user_id: str, current_password: str, new_password: str) -> bool:
        """Change user password (requires current password)"""
        try:
            # Get auth profile
            auth_result = await self.db_session.execute(
                select(UserAuthProfileEntity).where(
                    and_(
                        UserAuthProfileEntity.user_id == user_id,
                        UserAuthProfileEntity.login_type == UserLoginType.PASSWORD
                    )
                )
            )
            auth_profile = auth_result.scalar_one_or_none()
            
            if not auth_profile:
                return False
            
            # Verify current password
            if not self._verify_password(current_password, auth_profile.password_hash):
                logger.warning(f"Invalid current password for user {user_id}")
                return False
            
            # Update password
            auth_profile.password_hash = self._hash_password(new_password)
            auth_profile.last_password_change = datetime.utcnow()
            auth_profile.updated_at = datetime.utcnow()
            
            await self.db_session.commit()
            
            logger.info(f"Password changed successfully for user {user_id}")
            return True
            
        except Exception as e:
            logger.error(f"Password change error for user {user_id}: {e}")
            return False
    
    async def get_user_by_id(self, user_id: str) -> Optional[User]:
        """Get user by ID"""
        try:
            result = await self.db_session.execute(
                select(UserEntity).where(UserEntity.id == user_id)
            )
            user_entity = result.scalar_one_or_none()
            
            if user_entity:
                return self._map_user_entity_to_domain(user_entity)
            return None
            
        except Exception as e:
            logger.error(f"Get user by ID error for {user_id}: {e}")
            return None
    
    async def validate_session(self, session_id: str) -> Optional[UserLoginSession]:
        """Validate login session"""
        try:
            result = await self.db_session.execute(
                select(UserLoginSessionEntity).where(
                    and_(
                        UserLoginSessionEntity.session_id == session_id,
                        UserLoginSessionEntity.is_active == True
                    )
                )
            )
            session_entity = result.scalar_one_or_none()
            
            if session_entity:
                # Update last activity
                session_entity.last_activity = datetime.utcnow()
                await self.db_session.commit()
                
                return self._map_session_entity_to_domain(session_entity)
            return None
            
        except Exception as e:
            logger.error(f"Validate session error for {session_id}: {e}")
            return None
    
    async def end_session(self, session_id: str) -> bool:
        """End login session"""
        try:
            result = await self.db_session.execute(
                select(UserLoginSessionEntity).where(
                    UserLoginSessionEntity.session_id == session_id
                )
            )
            session_entity = result.scalar_one_or_none()
            
            if session_entity:
                session_entity.is_active = False
                session_entity.ended_at = datetime.utcnow()
                await self.db_session.commit()
                
                logger.info(f"Session {session_id} ended successfully")
                return True
            return False
            
        except Exception as e:
            logger.error(f"End session error for {session_id}: {e}")
            return False
    
    async def _create_login_session(self, user_id: str, remember_me: bool) -> UserLoginSessionEntity:
        """Create a new login session"""
        session = UserLoginSessionEntity(
            id=str(uuid4()),
            user_id=user_id,
            session_id=str(uuid4()),
            is_active=True,
            started_at=datetime.utcnow(),
            last_activity=datetime.utcnow()
        )
        
        self.db_session.add(session)
        return session
    
    def _hash_password(self, password: str) -> str:
        """Hash password using bcrypt"""
        salt = bcrypt.gensalt()
        return bcrypt.hashpw(password.encode('utf-8'), salt).decode('utf-8')
    
    def _verify_password(self, password: str, hashed: str) -> bool:
        """Verify password against hash"""
        try:
            return bcrypt.checkpw(password.encode('utf-8'), hashed.encode('utf-8'))
        except Exception:
            return False
    
    def _map_user_entity_to_domain(self, entity: UserEntity) -> User:
        """Map user entity to domain model"""
        return User(
            id=entity.id,
            tenant_id=entity.tenant_id,
            username=entity.username,
            email=entity.email,
            phone=entity.phone,
            first_name=entity.first_name,
            last_name=entity.last_name,
            gender=entity.gender,
            birth_date=entity.birth_date,
            profile_image_url=entity.profile_image_url,
            default_timezone=entity.default_timezone,
            current_timezone=entity.current_timezone,
            is_active=entity.is_active,
            status=entity.status,
            created_at=entity.created_at,
            updated_at=entity.updated_at,
            last_login=entity.last_login
        )
    
    def _map_session_entity_to_domain(self, entity: UserLoginSessionEntity) -> UserLoginSession:
        """Map session entity to domain model"""
        return UserLoginSession(
            id=entity.id,
            user_id=entity.user_id,
            session_id=entity.session_id,
            is_active=entity.is_active,
            started_at=entity.started_at,
            last_activity=entity.last_activity,
            ended_at=entity.ended_at,
            ip_address=entity.ip_address,
            user_agent=entity.user_agent
        )
