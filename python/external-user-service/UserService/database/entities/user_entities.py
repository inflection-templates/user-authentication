"""
User database entities
Equivalent to the .NET shala.api.database.relational.efcore entities
"""

from datetime import datetime, date
from uuid import uuid4
from sqlalchemy import Column, String, DateTime, Boolean, Text, Date, Integer, ForeignKey, Enum as SQLEnum
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy.orm import relationship
import enum

from domain.types.user_types import GenderType, UserLoginType, OAuthProvider, UserStatus

Base = declarative_base()


class UserEntity(Base):
    """User entity"""
    __tablename__ = "users"
    
    id = Column(String(36), primary_key=True, default=lambda: str(uuid4()))
    tenant_id = Column(String(36), nullable=True)
    username = Column(String(100), unique=True, nullable=True, index=True)
    email = Column(String(255), unique=True, nullable=False, index=True)
    phone = Column(String(20), nullable=True)
    first_name = Column(String(100), nullable=True)
    last_name = Column(String(100), nullable=True)
    gender = Column(SQLEnum(GenderType), nullable=True)
    birth_date = Column(Date, nullable=True)
    profile_image_url = Column(String(500), nullable=True)
    default_timezone = Column(String(50), nullable=False, default="UTC")
    current_timezone = Column(String(50), nullable=True)
    is_active = Column(Boolean, nullable=False, default=True)
    status = Column(SQLEnum(UserStatus), nullable=False, default=UserStatus.ACTIVE)
    created_at = Column(DateTime, nullable=False, default=datetime.utcnow)
    updated_at = Column(DateTime, nullable=False, default=datetime.utcnow, onupdate=datetime.utcnow)
    last_login = Column(DateTime, nullable=True)
    
    # Relationships
    auth_profiles = relationship("UserAuthProfileEntity", back_populates="user", cascade="all, delete-orphan")
    oauth_profiles = relationship("UserOAuthProfileEntity", back_populates="user", cascade="all, delete-orphan")
    login_sessions = relationship("UserLoginSessionEntity", back_populates="user", cascade="all, delete-orphan")
    user_roles = relationship("UserRoleEntity", back_populates="user", cascade="all, delete-orphan")


class UserAuthProfileEntity(Base):
    """User authentication profile entity"""
    __tablename__ = "user_auth_profiles"
    
    id = Column(String(36), primary_key=True, default=lambda: str(uuid4()))
    user_id = Column(String(36), ForeignKey("users.id"), nullable=False)
    login_type = Column(SQLEnum(UserLoginType), nullable=False)
    password_hash = Column(String(255), nullable=True)
    password_salt = Column(String(255), nullable=True)
    last_password_change = Column(DateTime, nullable=True)
    failed_login_attempts = Column(Integer, nullable=False, default=0)
    locked_until = Column(DateTime, nullable=True)
    is_locked = Column(Boolean, nullable=False, default=False)
    created_at = Column(DateTime, nullable=False, default=datetime.utcnow)
    updated_at = Column(DateTime, nullable=False, default=datetime.utcnow, onupdate=datetime.utcnow)
    
    # Relationships
    user = relationship("UserEntity", back_populates="auth_profiles")


class UserOAuthProfileEntity(Base):
    """User OAuth profile entity"""
    __tablename__ = "user_oauth_profiles"
    
    id = Column(String(36), primary_key=True, default=lambda: str(uuid4()))
    user_id = Column(String(36), ForeignKey("users.id"), nullable=False)
    provider = Column(SQLEnum(OAuthProvider), nullable=False)
    provider_user_id = Column(String(255), nullable=False)
    provider_username = Column(String(255), nullable=True)
    access_token = Column(Text, nullable=True)
    refresh_token = Column(Text, nullable=True)
    token_expires_at = Column(DateTime, nullable=True)
    created_at = Column(DateTime, nullable=False, default=datetime.utcnow)
    updated_at = Column(DateTime, nullable=False, default=datetime.utcnow, onupdate=datetime.utcnow)
    
    # Relationships
    user = relationship("UserEntity", back_populates="oauth_profiles")


class UserLoginSessionEntity(Base):
    """User login session entity"""
    __tablename__ = "user_login_sessions"
    
    id = Column(String(36), primary_key=True, default=lambda: str(uuid4()))
    user_id = Column(String(36), ForeignKey("users.id"), nullable=False)
    session_id = Column(String(36), nullable=False, default=lambda: str(uuid4()))
    is_active = Column(Boolean, nullable=False, default=True)
    started_at = Column(DateTime, nullable=False, default=datetime.utcnow)
    last_activity = Column(DateTime, nullable=False, default=datetime.utcnow)
    ended_at = Column(DateTime, nullable=True)
    ip_address = Column(String(45), nullable=True)  # IPv6 support
    user_agent = Column(Text, nullable=True)
    
    # Relationships
    user = relationship("UserEntity", back_populates="login_sessions")


class RoleEntity(Base):
    """Role entity"""
    __tablename__ = "roles"
    
    id = Column(String(36), primary_key=True, default=lambda: str(uuid4()))
    tenant_id = Column(String(36), nullable=True)
    name = Column(String(100), nullable=False)
    description = Column(String(500), nullable=True)
    is_system_role = Column(Boolean, nullable=False, default=False)
    is_active = Column(Boolean, nullable=False, default=True)
    created_at = Column(DateTime, nullable=False, default=datetime.utcnow)
    updated_at = Column(DateTime, nullable=False, default=datetime.utcnow, onupdate=datetime.utcnow)
    
    # Relationships
    user_roles = relationship("UserRoleEntity", back_populates="role", cascade="all, delete-orphan")


class UserRoleEntity(Base):
    """User role entity"""
    __tablename__ = "user_roles"
    
    id = Column(String(36), primary_key=True, default=lambda: str(uuid4()))
    user_id = Column(String(36), ForeignKey("users.id"), nullable=False)
    role_id = Column(String(36), ForeignKey("roles.id"), nullable=False)
    assigned_at = Column(DateTime, nullable=False, default=datetime.utcnow)
    assigned_by = Column(String(36), nullable=True)
    is_active = Column(Boolean, nullable=False, default=True)
    
    # Relationships
    user = relationship("UserEntity", back_populates="user_roles")
    role = relationship("RoleEntity", back_populates="user_roles")


class TenantEntity(Base):
    """Tenant entity"""
    __tablename__ = "tenants"
    
    id = Column(String(36), primary_key=True, default=lambda: str(uuid4()))
    name = Column(String(200), nullable=False)
    code = Column(String(50), unique=True, nullable=False, index=True)
    description = Column(String(1000), nullable=True)
    is_active = Column(Boolean, nullable=False, default=True)
    created_at = Column(DateTime, nullable=False, default=datetime.utcnow)
    updated_at = Column(DateTime, nullable=False, default=datetime.utcnow, onupdate=datetime.utcnow)


class ClientAppEntity(Base):
    """Client application entity"""
    __tablename__ = "client_apps"
    
    id = Column(String(36), primary_key=True, default=lambda: str(uuid4()))
    tenant_id = Column(String(36), nullable=True)
    name = Column(String(200), nullable=False)
    client_code = Column(String(50), unique=True, nullable=False, index=True)
    description = Column(String(1000), nullable=True)
    is_active = Column(Boolean, nullable=False, default=True)
    created_at = Column(DateTime, nullable=False, default=datetime.utcnow)
    updated_at = Column(DateTime, nullable=False, default=datetime.utcnow, onupdate=datetime.utcnow)


class ApiKeyEntity(Base):
    """API key entity"""
    __tablename__ = "api_keys"
    
    id = Column(String(36), primary_key=True, default=lambda: str(uuid4()))
    client_app_id = Column(String(36), ForeignKey("client_apps.id"), nullable=False)
    key_hash = Column(String(255), nullable=False, unique=True, index=True)
    key_name = Column(String(100), nullable=False)
    is_active = Column(Boolean, nullable=False, default=True)
    expires_at = Column(DateTime, nullable=True)
    created_at = Column(DateTime, nullable=False, default=datetime.utcnow)
    last_used_at = Column(DateTime, nullable=True)
    
    # Relationships
    client_app = relationship("ClientAppEntity")
