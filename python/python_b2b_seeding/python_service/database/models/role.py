"""
Role model.
Similar to the Node.js role.model.ts implementation.
"""
from sqlalchemy import Column, String, ForeignKey
from sqlalchemy.orm import relationship
from database.models.base import BaseModel
from database.models.types import GUID
import uuid


class Role(BaseModel):
    """Role model."""
    __tablename__ = 'roles'
    
    # Explicitly define id first to ensure it appears first in database
    id = Column(GUID(), primary_key=True, default=uuid.uuid4)
    
    user_id = Column(GUID(), ForeignKey('users.id'), name='userId', nullable=False, index=True)
    role_name = Column(String(50), name='roleName', nullable=False)
    
    # Relationships
    user = relationship("User", back_populates="roles")

