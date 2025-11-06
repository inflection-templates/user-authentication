"""
Tenant model.
Similar to the Node.js tenant.model.ts implementation.
"""
from sqlalchemy import Column, String, Boolean, Text, JSON, DateTime
from sqlalchemy.orm import relationship
from database.models.base import BaseModel
from database.models.types import GUID
import uuid


class Tenant(BaseModel):
    """Tenant model."""
    __tablename__ = 'tenants'
    
    # Explicitly define id first to ensure it appears first in database
    id = Column(GUID(), primary_key=True, default=uuid.uuid4)
    
    name = Column(String(100), nullable=False)
    code = Column(String(50), unique=True, nullable=False, index=True)
    domain = Column(String(255), nullable=True)
    description = Column(Text, nullable=True)
    industry = Column(String(100), nullable=True)
    website_url = Column(String(255), name='websiteUrl', nullable=True)
    logo_url = Column(String(255), name='logoUrl', nullable=True)
    contact_email = Column(String(100), name='contactEmail', nullable=True)
    contact_phone = Column(String(20), name='contactPhone', nullable=True)
    address = Column(String(100), nullable=True)
    city = Column(String(50), nullable=True)
    state = Column(String(50), nullable=True)
    postal_code = Column(String(20), name='postalCode', nullable=True)
    country = Column(String(50), nullable=True)
    
    status = Column(String(20), default='active', nullable=False)
    is_active = Column(Boolean, name='isActive', default=True, nullable=False)
    is_verified = Column(Boolean, name='isVerified', default=False, nullable=False)
    
    settings = Column(JSON, nullable=True)
    billing_config = Column(JSON, name='billingConfig', nullable=True)
    
    verified_at = Column(DateTime, name='verifiedAt', nullable=True)
    verified_by = Column(String(255), name='verifiedBy', nullable=True)
    
    deleted_at = Column(DateTime, name='deletedAt', nullable=True)
    
    # Relationships
    users = relationship("User", back_populates="tenant")

