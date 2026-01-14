"""
Database-agnostic column types.
"""
from sqlalchemy import TypeDecorator, String
from sqlalchemy.dialects.postgresql import UUID as PostgresUUID
from sqlalchemy.dialects.mysql import CHAR
import uuid


class GUID(TypeDecorator):
    """
    Platform-independent GUID type.
    Uses PostgreSQL's UUID type, or MySQL's CHAR(36) type.
    """
    impl = String(36)
    cache_ok = True
    
    def load_dialect_impl(self, dialect):
        if dialect.name == 'postgresql':
            return dialect.type_descriptor(PostgresUUID(as_uuid=True))
        elif dialect.name == 'mysql':
            return dialect.type_descriptor(CHAR(36, charset='utf8mb4'))
        else:
            return dialect.type_descriptor(String(36))
    
    def process_bind_param(self, value, dialect):
        if value is None:
            return value
        elif dialect.name == 'postgresql':
            if isinstance(value, str):
                return uuid.UUID(value)
            return value
        else:
            # MySQL: store as string
            if isinstance(value, uuid.UUID):
                return str(value)
            elif isinstance(value, str):
                # Validate it's a valid UUID string
                try:
                    uuid.UUID(value)
                    return value
                except (ValueError, AttributeError):
                    return value
            return str(value) if value else None
    
    def process_result_value(self, value, dialect):
        if value is None:
            return value
        elif dialect.name == 'postgresql':
            # PostgreSQL returns UUID objects
            return value
        else:
            # MySQL: convert string to UUID
            if isinstance(value, uuid.UUID):
                return value
            elif isinstance(value, str):
                try:
                    return uuid.UUID(value)
                except (ValueError, AttributeError):
                    return None
            return value

