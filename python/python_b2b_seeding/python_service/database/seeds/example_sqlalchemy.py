"""
Example implementation for SQLAlchemy ORM.
This file shows how to integrate the seeding functionality with SQLAlchemy.

Usage:
    1. Install dependencies: pip install sqlalchemy bcrypt
    2. Update the database connection string
    3. Import and use in your application
"""
import os
import asyncio
from sqlalchemy import create_engine
from sqlalchemy.orm import sessionmaker, Session
from sqlalchemy.ext.declarative import declarative_base

from database.seeds.database_seeder import DatabaseSeeder

# Replace with your actual model imports
# from models import User, Tenant, Role, ClientApp

# Database configuration
DATABASE_URL = os.getenv(
    'DATABASE_URL',
    'postgresql://user:password@localhost:5432/dbname'
)

# Create engine and session factory
engine = create_engine(DATABASE_URL, echo=False)
SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)
Base = declarative_base()


class RepositoryWrapper:
    """Wrapper to make SQLAlchemy queries compatible with seeder."""
    
    def __init__(self, session: Session, model_class):
        self.session = session
        self.model_class = model_class
    
    def query(self):
        """Get query object."""
        return self.session.query(self.model_class)
    
    def all(self):
        """Get all records."""
        return self.query().all()
    
    def find(self):
        """Find all records (alias for all)."""
        return self.all()
    
    def count(self):
        """Count records."""
        return self.query().count()
    
    def add(self, obj):
        """Add object to session."""
        self.session.add(obj)
        return obj
    
    def add_all(self, objects):
        """Add all objects to session."""
        self.session.add_all(objects)
    
    def bulk_insert_mappings(self, mappings):
        """Bulk insert mappings."""
        # Convert dicts to model instances
        instances = [self.model_class(**mapping) for mapping in mappings]
        self.session.add_all(instances)
    
    def merge(self, obj):
        """Merge object."""
        if isinstance(obj, dict):
            obj = self.model_class(**obj)
        return self.session.merge(obj)
    
    def commit(self):
        """Commit session."""
        self.session.commit()
    
    def rollback(self):
        """Rollback session."""
        self.session.rollback()
    
    def delete(self, filters=None):
        """Delete records."""
        if filters:
            return self.query().filter_by(**filters).delete()
        return self.query().delete()


def get_database_connection() -> Session:
    """
    Get database session.
    
    Returns:
        SQLAlchemy session
    """
    return SessionLocal()


def get_repositories(data_source: Session):
    """
    Get repositories for all models.
    
    Args:
        data_source: SQLAlchemy session
        
    Returns:
        Tuple of repositories
    """
    # Import your models here
    # from models import User, Tenant, Role, ClientApp
    
    # Replace with your actual model classes
    # For now, using placeholder class names
    # You need to import your actual models
    
    # user_repo = RepositoryWrapper(data_source, User)
    # tenant_repo = RepositoryWrapper(data_source, Tenant)
    # role_repo = RepositoryWrapper(data_source, Role)
    # client_app_repo = RepositoryWrapper(data_source, ClientApp)
    
    # return (user_repo, tenant_repo, role_repo, client_app_repo)
    
    raise NotImplementedError(
        "Replace this with your actual model imports and repository creation.\n"
        "See comments in the code for guidance."
    )


async def seed_database(
    users: int = 5,
    roles: int = 3,
    tenants: int = 1,
    client_apps: int = 2
):
    """
    Seed the database with sample data.
    
    Args:
        users: Number of users to create
        roles: Number of roles to create
        tenants: Number of tenants to create
        client_apps: Number of client apps to create
    """
    session = get_database_connection()
    
    try:
        # Get repositories
        user_repo, tenant_repo, role_repo, client_app_repo = get_repositories(session)
        
        # Create seeder
        seeder = DatabaseSeeder(
            session,
            user_repo,
            tenant_repo,
            role_repo,
            client_app_repo
        )
        
        # Check connection
        if not await seeder.check_database_connection():
            print("[ERROR] Database connection failed")
            return False
        
        # Seed all tables
        counts = {
            'users': users,
            'roles': roles,
            'tenants': tenants,
            'clientApps': client_apps
        }
        
        await seeder.seed_all(counts)
        print("[OK] Seeding completed successfully!")
        return True
        
    except Exception as e:
        print(f"[ERROR] Error during seeding: {str(e)}")
        session.rollback()
        return False
    finally:
        session.close()


if __name__ == '__main__':
    # Run seeding
    asyncio.run(seed_database())

