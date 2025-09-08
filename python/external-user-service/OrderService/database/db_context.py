"""
Database context and session management
Equivalent to the .NET OrderService.Data.OrderDbContext
"""

import os
from datetime import datetime
from decimal import Decimal
from sqlalchemy import create_engine, text
from sqlalchemy.ext.asyncio import AsyncSession, create_async_engine
from sqlalchemy.orm import sessionmaker
from sqlalchemy.pool import StaticPool

from models.entities import Base, Customer, Product, Order, OrderItem

# Database configuration
DATABASE_URL = os.getenv("DATABASE_URL", "mysql+aiomysql://root:password@localhost:3306/order_service_db")

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
    from urllib.parse import urlparse
    import logging
    
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
        result = await session.execute(text("SELECT COUNT(*) FROM customers"))
        customer_count = result.scalar()
        
        if customer_count > 0:
            return  # Data already seeded
        
        # Seed customers
        customers = [
            Customer(
                first_name="John",
                last_name="Doe",
                email="john.doe@example.com",
                phone="+1234567890",
                address="123 Main St, City, State 12345",
                created_at=datetime.utcnow(),
                updated_at=datetime.utcnow()
            ),
            Customer(
                first_name="Jane",
                last_name="Smith",
                email="jane.smith@example.com",
                phone="+1234567891",
                address="456 Oak Ave, City, State 12346",
                created_at=datetime.utcnow(),
                updated_at=datetime.utcnow()
            )
        ]
        
        for customer in customers:
            session.add(customer)
        
        # Seed products
        products = [
            Product(
                id=1,
                name="Laptop Computer",
                description="High-performance laptop for business and gaming",
                price=Decimal("1299.99"),
                sku="LAPTOP-001",
                stock_quantity=50,
                category="Electronics",
                is_active=True,
                created_at=datetime.utcnow(),
                updated_at=datetime.utcnow()
            ),
            Product(
                id=2,
                name="Wireless Mouse",
                description="Ergonomic wireless mouse with USB receiver",
                price=Decimal("29.99"),
                sku="MOUSE-001",
                stock_quantity=200,
                category="Electronics",
                is_active=True,
                created_at=datetime.utcnow(),
                updated_at=datetime.utcnow()
            ),
            Product(
                id=3,
                name="Office Chair",
                description="Comfortable ergonomic office chair",
                price=Decimal("199.99"),
                sku="CHAIR-001",
                stock_quantity=75,
                category="Furniture",
                is_active=True,
                created_at=datetime.utcnow(),
                updated_at=datetime.utcnow()
            )
        ]
        
        for product in products:
            session.add(product)
        
        await session.commit()
        print("✅ Database seeded with initial data")
