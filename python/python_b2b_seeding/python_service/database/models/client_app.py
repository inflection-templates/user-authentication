"""
ClientApp model.
Similar to the Node.js client-app.model.ts implementation.
"""
from sqlalchemy import Column, String, Boolean, Text, DateTime
from database.models.base import BaseModel
from database.models.types import GUID
import uuid


class ClientApp(BaseModel):
    """ClientApp model."""
    __tablename__ = 'client_apps'
    
    # Explicitly define id first to ensure it appears first in database
    id = Column(GUID(), primary_key=True, default=uuid.uuid4)
    
    name = Column(String(100), nullable=False)
    code = Column(String(50), unique=True, nullable=False, index=True)
    api_key = Column(String(100), name='apiKey', nullable=True)
    description = Column(Text, nullable=False)
    owner_user_id = Column(GUID(), name='ownerUserId', nullable=False, index=True)
    organization_id = Column(GUID(), name='organizationId', nullable=True, index=True)
    
    redirect_uri = Column(String(500), name='redirectUri', nullable=True)
    logo_url = Column(String(500), name='logoUrl', nullable=True)
    website_url = Column(String(500), name='websiteUrl', nullable=True)
    privacy_policy_url = Column(String(500), name='privacyPolicyUrl', nullable=True)
    terms_of_service_url = Column(String(500), name='termsOfServiceUrl', nullable=True)
    
    is_verified = Column(Boolean, name='isVerified', default=False, nullable=False)
    is_active = Column(Boolean, name='isActive', default=True, nullable=False)
    status = Column(String(20), default='active', nullable=False)
    
    deleted_at = Column(DateTime, name='deletedAt', nullable=True)

