"""
Database connector for initializing and managing database connections.
Similar to the Node.js database.connector.ts implementation.
"""
import logging
from sqlalchemy import create_engine, text
from sqlalchemy.orm import sessionmaker, Session
from sqlalchemy.exc import SQLAlchemyError
from config.config import Config
from database.models.base import Base
# Import all models to ensure they're registered with Base
from database.models.user import User
from database.models.tenant import Tenant
from database.models.role import Role
from database.models.client_app import ClientApp

# Suppress SQLAlchemy INFO logs
logging.getLogger('sqlalchemy.engine').setLevel(logging.WARNING)
logging.getLogger('sqlalchemy.pool').setLevel(logging.WARNING)


class DatabaseConnector:
    """Database connector class."""
    
    _engine = None
    _session_factory = None
    
    @classmethod
    def get_engine(cls):
        """
        Get or create database engine.
        
        Returns:
            SQLAlchemy engine
        """
        if cls._engine is None:
            database_url = cls._get_database_url()
            cls._engine = create_engine(
                database_url,
                pool_size=20,
                max_overflow=0,
                pool_pre_ping=True,
                echo=False  # Disable SQL query logging
            )
        return cls._engine
    
    @classmethod
    def _get_database_url(cls) -> str:
        """
        Get database URL based on dialect.
        
        Returns:
            Database URL string
        """
        if Config.DB_DIALECT == 'postgres':
            return f"postgresql://{Config.DB_USER_NAME}:{Config.DB_USER_PASSWORD}@{Config.DB_HOST}:{Config.DB_PORT}/{Config.DB_NAME}"
        elif Config.DB_DIALECT == 'mysql':
            return f"mysql+pymysql://{Config.DB_USER_NAME}:{Config.DB_USER_PASSWORD}@{Config.DB_HOST}:{Config.DB_PORT}/{Config.DB_NAME}?charset=utf8mb4"
        else:
            raise ValueError(f"Unsupported database dialect: {Config.DB_DIALECT}")
    
    @classmethod
    def create_database(cls) -> bool:
        """
        Create database if it doesn't exist.
        
        Returns:
            True if successful
        """
        try:
            # Connect to postgres/mysql to create database
            if Config.DB_DIALECT == 'postgres':
                admin_url = f"postgresql://{Config.DB_USER_NAME}:{Config.DB_USER_PASSWORD}@{Config.DB_HOST}:{Config.DB_PORT}/postgres"
            elif Config.DB_DIALECT == 'mysql':
                admin_url = f"mysql+pymysql://{Config.DB_USER_NAME}:{Config.DB_USER_PASSWORD}@{Config.DB_HOST}:{Config.DB_PORT}/"
            else:
                print(f"[WARN] Auto database creation not supported for {Config.DB_DIALECT}")
                return True
            
            admin_engine = create_engine(admin_url)
            
            # Check if database exists
            with admin_engine.connect() as conn:
                if Config.DB_DIALECT == 'postgres':
                    result = conn.execute(text(
                        f"SELECT 1 FROM pg_database WHERE datname = '{Config.DB_NAME}'"
                    ))
                    exists = result.fetchone() is not None
                elif Config.DB_DIALECT == 'mysql':
                    result = conn.execute(text(
                        f"SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{Config.DB_NAME}'"
                    ))
                    exists = result.fetchone() is not None
                
                if not exists:
                    print(f"[INFO] Creating database: {Config.DB_NAME}")
                    if Config.DB_DIALECT == 'postgres':
                        conn.execute(text("COMMIT"))
                        conn.execute(text(f"CREATE DATABASE {Config.DB_NAME}"))
                    elif Config.DB_DIALECT == 'mysql':
                        conn.execute(text(f"CREATE DATABASE {Config.DB_NAME}"))
                    conn.commit()
                    print(f"[OK] Database {Config.DB_NAME} created successfully")
                else:
                    print(f"[INFO] Database {Config.DB_NAME} already exists")
            
            admin_engine.dispose()
            return True
        except Exception as e:
            print(f"[WARN] Could not create database (may already exist): {str(e)}")
            return True  # Continue even if creation fails (might already exist)
    
    @classmethod
    def initialize(cls) -> bool:
        """
        Initialize database connection and create tables.
        
        Returns:
            True if successful
        """
        try:
            print(f"[INFO] Environment: {Config.NODE_ENV}")
            print(f"[INFO] Database Name: {Config.DB_NAME}")
            print(f"[INFO] Database Username: {Config.DB_USER_NAME}")
            print(f"[INFO] Database Host: {Config.DB_HOST}")
            
            # Create database if it doesn't exist
            cls.create_database()
            
            # Create engine
            engine = cls.get_engine()
            
            # Create all tables
            print("[INFO] Creating database tables...")
            Base.metadata.create_all(engine)
            print("[OK] Database tables created successfully")
            
            # Create session factory
            cls._session_factory = sessionmaker(
                bind=engine,
                autocommit=False,
                autoflush=False
            )
            
            print("🔄 Database connection has been established successfully.")
            return True
        except Exception as e:
            print(f"❌ Unable to connect to the database: {str(e)}")
            return False
    
    @classmethod
    def setup(cls) -> bool:
        """
        Setup database (create database and initialize).
        
        Returns:
            True if successful
        """
        print("🛢️ Setting up the database...")
        return cls.initialize()
    
    @classmethod
    def get_session(cls) -> Session:
        """
        Get a new database session.
        
        Returns:
            SQLAlchemy session
        """
        if cls._session_factory is None:
            raise RuntimeError("Database not initialized. Call setup() first.")
        return cls._session_factory()
    
    @classmethod
    def close(cls):
        """Close database connections."""
        if cls._engine:
            cls._engine.dispose()
            print("🔄 Database connection has been closed successfully.")


# Export session for use in other modules
def get_db():
    """Dependency function for FastAPI to get database session."""
    db = DatabaseConnector.get_session()
    try:
        yield db
    finally:
        db.close()

