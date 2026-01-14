"""
Base model class with common fields.
"""
from sqlalchemy import Column, DateTime
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy.sql import func
from database.models.types import GUID
import uuid

Base = declarative_base()


class BaseModel(Base):
    """Abstract base model with common fields."""
    __abstract__ = True
    
    id = Column(GUID(), primary_key=True, default=uuid.uuid4)
    created_at = Column(DateTime, name='createdAt', server_default=func.now())
    updated_at = Column(DateTime, name='updatedAt', server_default=func.now(), onupdate=func.now())

