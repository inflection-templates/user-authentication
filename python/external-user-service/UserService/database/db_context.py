"""
Database context and session management
Equivalent to the .NET shala.api.database.relational.efcore
"""

import os
import json
from datetime import datetime
from uuid import uuid4
from sqlalchemy import create_engine, text
from sqlalchemy.ext.asyncio import AsyncSession, create_async_engine
from sqlalchemy.orm import sessionmaker
from sqlalchemy.pool import StaticPool

from database.entities.user_entities import (
    Base, UserEntity, UserAuthProfileEntity, RoleEntity, UserRoleEntity,
    TenantEntity, ClientAppEntity
)
from domain.types.user_types import UserLoginType, UserStatus, GenderType

# Database configuration
DATABASE_URL = os.getenv("DATABASE_URL", "mysql+aiomysql://root:password@localhost:3306/user_service_db")

# Create async engine with MySQL-specific configuration
engine = create_async_engine(
    DATABASE_URL,
    echo=os.getenv("SQL_DEBUG", "false").lower() == "true",  # Control SQL logging via environment
    pool_pre_ping=True,  # Verify connections before use
    pool_recycle=3600,   # Recycle connections every hour
    connect_args={
        "charset": "utf8mb4",
        "autocommit": False
    }
)

# Create session factory
AsyncSessionLocal = sessionmaker(
    engine, class_=AsyncSession, expire_on_commit=False
)


async def get_db_session():
    """Get database session"""
    async with AsyncSessionLocal() as session:
        try:
            yield session
        finally:
            await session.close()


async def create_database_if_not_exists():
    """Create database if it doesn't exist"""
    import pymysql
    import logging
    from urllib.parse import urlparse
    
    logger = logging.getLogger(__name__)
    
    # Parse the database URL
    parsed_url = urlparse(DATABASE_URL)
    
    # Extract connection details
    host = parsed_url.hostname or 'localhost'
    port = parsed_url.port or 3306
    username = parsed_url.username or 'root'
    password = parsed_url.password or ''
    database_name = parsed_url.path.lstrip('/')
    
    # Connect to MySQL without specifying database
    try:
        connection = pymysql.connect(
            host=host,
            port=port,
            user=username,
            password=password,
            charset='utf8mb4'
        )
        
        with connection.cursor() as cursor:
            # Create database if it doesn't exist
            cursor.execute(f"CREATE DATABASE IF NOT EXISTS `{database_name}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci")
            logger.info(f"✅ Database '{database_name}' created or already exists")
        
        connection.commit()
        connection.close()
        
    except Exception as e:
        logger.error(f"❌ Failed to create database '{database_name}': {e}")
        raise


async def init_db():
    """Initialize database and seed data"""
    # First create the database if it doesn't exist
    await create_database_if_not_exists()
    
    # Then create tables and seed data
    async with engine.begin() as conn:
        # Create all tables
        await conn.run_sync(Base.metadata.create_all)
    
    # Seed initial data
    await seed_data()


async def seed_data():
    """Seed initial data"""
    async with AsyncSessionLocal() as session:
        # Check if data already exists
        result = await session.execute(text("SELECT COUNT(*) FROM tenants"))
        tenant_count = result.scalar()
        
        if tenant_count > 0:
            return  # Data already seeded
        
        # Seed default tenant
        default_tenant = TenantEntity(
            id=str(uuid4()),
            name="Default Tenant",
            code="default",
            description="Default tenant for the system",
            is_active=True,
            created_at=datetime.utcnow(),
            updated_at=datetime.utcnow()
        )
        session.add(default_tenant)
        await session.flush()
        
        # Seed default client app
        default_client_app = ClientAppEntity(
            id=str(uuid4()),
            tenant_id=default_tenant.id,
            name="Default Client",
            client_code="default_client",
            description="Default client application",
            is_active=True,
            created_at=datetime.utcnow(),
            updated_at=datetime.utcnow()
        )
        session.add(default_client_app)
        await session.flush()
        
        # Seed system roles
        admin_role = RoleEntity(
            id=str(uuid4()),
            tenant_id=default_tenant.id,
            name="SystemAdmin",
            description="System Administrator with full access",
            is_system_role=True,
            is_active=True,
            created_at=datetime.utcnow(),
            updated_at=datetime.utcnow()
        )
        
        user_role = RoleEntity(
            id=str(uuid4()),
            tenant_id=default_tenant.id,
            name="User",
            description="Standard user role",
            is_system_role=True,
            is_active=True,
            created_at=datetime.utcnow(),
            updated_at=datetime.utcnow()
        )
        
        session.add(admin_role)
        session.add(user_role)
        await session.flush()
        
        # Seed system admin user (from config if available)
        try:
            # Try to load system admin config
            if os.path.exists("static.content/seed.data/system.admin.seed.sample.json"):
                with open("static.content/seed.data/system.admin.seed.sample.json", "r") as f:
                    admin_config = json.load(f)
                
                admin_user = UserEntity(
                    id=str(uuid4()),
                    tenant_id=default_tenant.id,
                    username=admin_config.get("username", "admin"),
                    email=admin_config.get("email", "admin@example.com"),
                    first_name=admin_config.get("firstName", "System"),
                    last_name=admin_config.get("lastName", "Administrator"),
                    gender=GenderType.PREFER_NOT_TO_SAY,
                    default_timezone="UTC",
                    is_active=True,
                    status=UserStatus.ACTIVE,
                    created_at=datetime.utcnow(),
                    updated_at=datetime.utcnow()
                )
            else:
                # Default system admin
                admin_user = UserEntity(
                    id=str(uuid4()),
                    tenant_id=default_tenant.id,
                    username="admin",
                    email="admin@example.com",
                    first_name="System",
                    last_name="Administrator",
                    gender=GenderType.PREFER_NOT_TO_SAY,
                    default_timezone="UTC",
                    is_active=True,
                    status=UserStatus.ACTIVE,
                    created_at=datetime.utcnow(),
                    updated_at=datetime.utcnow()
                )
            
            session.add(admin_user)
            await session.flush()
            
            # Create auth profile for admin (password: Admin@123)
            import bcrypt
            password = "Admin@123"
            salt = bcrypt.gensalt()
            password_hash = bcrypt.hashpw(password.encode('utf-8'), salt).decode('utf-8')
            
            admin_auth_profile = UserAuthProfileEntity(
                id=str(uuid4()),
                user_id=admin_user.id,
                login_type=UserLoginType.PASSWORD,
                password_hash=password_hash,
                password_salt=salt.decode('utf-8'),
                last_password_change=datetime.utcnow(),
                failed_login_attempts=0,
                is_locked=False,
                created_at=datetime.utcnow(),
                updated_at=datetime.utcnow()
            )
            session.add(admin_auth_profile)
            
            # Assign admin role
            admin_user_role = UserRoleEntity(
                id=str(uuid4()),
                user_id=admin_user.id,
                role_id=admin_role.id,
                assigned_at=datetime.utcnow(),
                is_active=True
            )
            session.add(admin_user_role)
            
        except Exception as e:
            print(f"Warning: Could not create system admin user: {e}")
        
        # Seed sample regular user
        sample_user = UserEntity(
            id=str(uuid4()),
            tenant_id=default_tenant.id,
            username="john.doe",
            email="john.doe@example.com",
            first_name="John",
            last_name="Doe",
            gender=GenderType.MALE,
            default_timezone="UTC",
            is_active=True,
            status=UserStatus.ACTIVE,
            created_at=datetime.utcnow(),
            updated_at=datetime.utcnow()
        )
        session.add(sample_user)
        await session.flush()
        
        # Create auth profile for sample user (password: User@123)
        import bcrypt
        password = "User@123"
        salt = bcrypt.gensalt()
        password_hash = bcrypt.hashpw(password.encode('utf-8'), salt).decode('utf-8')
        
        user_auth_profile = UserAuthProfileEntity(
            id=str(uuid4()),
            user_id=sample_user.id,
            login_type=UserLoginType.PASSWORD,
            password_hash=password_hash,
            password_salt=salt.decode('utf-8'),
            last_password_change=datetime.utcnow(),
            failed_login_attempts=0,
            is_locked=False,
            created_at=datetime.utcnow(),
            updated_at=datetime.utcnow()
        )
        session.add(user_auth_profile)
        
        # Assign user role
        sample_user_role = UserRoleEntity(
            id=str(uuid4()),
            user_id=sample_user.id,
            role_id=user_role.id,
            assigned_at=datetime.utcnow(),
            is_active=True
        )
        session.add(sample_user_role)
        
        await session.commit()
        print("✅ Database seeded with initial data")
        print("   • Default tenant created")
        print("   • System roles created (SystemAdmin, User)")
        print("   • System admin user: admin@example.com (password: Admin@123)")
        print("   • Sample user: john.doe@example.com (password: User@123)")
