"""
User model.
Similar to the Node.js user.model.ts implementation.
"""
from sqlalchemy import Column, String, Boolean, DateTime, ForeignKey
from sqlalchemy.orm import relationship
from database.models.base import BaseModel
from database.models.types import GUID
import uuid


class User(BaseModel):
    """User model."""
    __tablename__ = 'users'
    
    # Explicitly define id first to ensure it appears first in database
    id = Column(GUID(), primary_key=True, default=uuid.uuid4)
    
    username = Column(String(50), unique=True, nullable=False, index=True)
    email = Column(String(255), unique=True, nullable=False, index=True)
    password = Column(String(255), nullable=False)
    first_name = Column(String(50), name='firstName', nullable=True)
    last_name = Column(String(50), name='lastName', nullable=True)
    phone_number = Column(String(20), name='phoneNumber', nullable=True)
    country_code = Column(String(5), name='countryCode', nullable=True)
    
    is_email_verified = Column(Boolean, name='isEmailVerified', default=False, nullable=False)
    is_phone_verified = Column(Boolean, name='isPhoneVerified', default=False, nullable=False)
    is_active = Column(Boolean, name='isActive', default=True, nullable=False)
    
    last_login_at = Column(DateTime, name='lastLoginAt', nullable=True)
    
    tenant_id = Column(GUID(), ForeignKey('tenants.id'), name='tenantId', nullable=True, index=True)
    role = Column(String(50), default='Normal User', nullable=False)
    
    organization_id = Column(GUID(), name='organizationId', nullable=True, index=True)
    is_organization_admin = Column(Boolean, name='isOrganizationAdmin', default=False, nullable=False)
    
    invited_at = Column(DateTime, name='invitedAt', nullable=True)
    joined_at = Column(DateTime, name='joinedAt', nullable=True)
    invited_by = Column(String(255), name='invitedBy', nullable=True)
    
    deleted_at = Column(DateTime, name='deletedAt', nullable=True)
    
    # Relationships
    tenant = relationship("Tenant", back_populates="users", foreign_keys=[tenant_id])
    roles = relationship("Role", back_populates="user", cascade="all, delete-orphan")

