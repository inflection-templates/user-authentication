"""
Configuration manager for loading and accessing environment variables.
Similar to the Node.js ConfigurationManager implementation.
"""
import os
from typing import Optional
from dotenv import load_dotenv
from pathlib import Path

# Load environment variables from .env file
# Try python_service/.env first, then parent directory
env_path_local = Path(__file__).parent.parent / '.env'  # python_service/.env
env_path_parent = Path(__file__).parent.parent.parent / '.env'  # service-skeleton/.env
if env_path_local.exists():
    load_dotenv(dotenv_path=env_path_local)
elif env_path_parent.exists():
    load_dotenv(dotenv_path=env_path_parent)
else:
    # Try current directory
    load_dotenv()  # This will look for .env in current working directory


class Config:
    """Configuration manager class."""
    
    # Service Configuration
    SERVICE_NAME = os.getenv('SERVICE_NAME', 'service-skeleton')
    NODE_ENV = os.getenv('NODE_ENV', 'development')
    PORT = int(os.getenv('PORT', 3000))
    BASE_URL = os.getenv('BASE_URL', 'http://localhost:3000')
    SERVICE_VERSION = os.getenv('SERVICE_VERSION', '1.0.0')
    
    # Database Configuration
    DB_DIALECT = os.getenv('DB_DIALECT', 'postgres')
    DB_HOST = os.getenv('DB_HOST', 'localhost')
    DB_PORT = int(os.getenv('DB_PORT', 5432))
    DB_NAME = os.getenv('DB_NAME', 'service_skeleton')
    DB_USER_NAME = os.getenv('DB_USER_NAME', 'postgres')
    DB_USER_PASSWORD = os.getenv('DB_USER_PASSWORD', 'password')
    BUSINESS_MODE = os.getenv('BUSINESS_MODE', 'b2c')
    
    # JWT Configuration
    JWT_ACCESS_SECRET = os.getenv('JWT_ACCESS_SECRET', 'your-super-secret-access-key-here')
    JWT_REFRESH_SECRET = os.getenv('JWT_REFRESH_SECRET', 'your-super-secret-refresh-key-here')
    JWT_ACCESS_EXPIRES_IN = os.getenv('JWT_ACCESS_EXPIRES_IN', '15m')
    JWT_REFRESH_EXPIRES_IN = os.getenv('JWT_REFRESH_EXPIRES_IN', '7d')
    
    # Internal API Configuration
    INTERNAL_API_KEY = os.getenv('INTERNAL_API_KEY', 'your-internal-api-key')
    USER_SERVICE_BASE_URL = os.getenv('USER_SERVICE_BASE_URL', 'http://user-service:3000')
    
    # OAuth Configuration
    GITHUB_CLIENT_ID = os.getenv('GITHUB_CLIENT_ID', None)
    GITHUB_CLIENT_SECRET = os.getenv('GITHUB_CLIENT_SECRET', None)
    GOOGLE_CLIENT_ID = os.getenv('GOOGLE_CLIENT_ID', None)
    GOOGLE_CLIENT_SECRET = os.getenv('GOOGLE_CLIENT_SECRET', None)
    FACEBOOK_CLIENT_ID = os.getenv('FACEBOOK_CLIENT_ID', None)
    FACEBOOK_CLIENT_SECRET = os.getenv('FACEBOOK_CLIENT_SECRET', None)
    TWITTER_CLIENT_ID = os.getenv('TWITTER_CLIENT_ID', None)
    TWITTER_CLIENT_SECRET = os.getenv('TWITTER_CLIENT_SECRET', None)
    
    # Email Configuration
    SMTP_HOST = os.getenv('SMTP_HOST', 'smtp.gmail.com')
    SMTP_PORT = int(os.getenv('SMTP_PORT', 587))
    SMTP_USER = os.getenv('SMTP_USER', None)
    SMTP_PASSWORD = os.getenv('SMTP_PASSWORD', None)
    
    # SMS Configuration
    TWILIO_ACCOUNT_SID = os.getenv('TWILIO_ACCOUNT_SID', None)
    TWILIO_AUTH_TOKEN = os.getenv('TWILIO_AUTH_TOKEN', None)
    TWILIO_PHONE_NUMBER = os.getenv('TWILIO_PHONE_NUMBER', None)
    
    # AWS S3 Configuration
    AWS_ACCESS_KEY_ID = os.getenv('AWS_ACCESS_KEY_ID', None)
    AWS_SECRET_ACCESS_KEY = os.getenv('AWS_SECRET_ACCESS_KEY', None)
    AWS_REGION = os.getenv('AWS_REGION', 'us-west-2')
    AWS_S3_BUCKET = os.getenv('AWS_S3_BUCKET', None)
    
    # Logging & Monitoring
    HTTP_LOGGING = os.getenv('HTTP_LOGGING', 'true').lower() == 'true'
    ENABLE_TELEMETRY = os.getenv('ENABLE_TELEMETRY', 'false').lower() == 'true'
    OTEL_EXPORTER_OTLP_TRACES_ENDPOINT = os.getenv('OTEL_EXPORTER_OTLP_TRACES_ENDPOINT', None)
    
    # Seeding Configuration (defaults to True - seeds automatically unless explicitly set to false)
    AUTO_SEED_ON_STARTUP = os.getenv('AUTO_SEED_ON_STARTUP', 'true').lower() == 'true'
    SEED_USER_COUNT = int(os.getenv('SEED_USER_COUNT', 2))
    
    # External Services
    S3_CONFIG_BUCKET = os.getenv('S3_CONFIG_BUCKET', None)
    S3_CONFIG_PATH = os.getenv('S3_CONFIG_PATH', None)
    
    # System Identifier (can be loaded from service.config.json if needed)
    SYSTEM_IDENTIFIER = os.getenv('SYSTEM_IDENTIFIER', 'Kleo')
    
    @classmethod
    def get_database_url(cls) -> str:
        """
        Get database connection URL.
        
        Returns:
            Database URL string
        """
        return f"{cls.DB_DIALECT}://{cls.DB_USER_NAME}:{cls.DB_USER_PASSWORD}@{cls.DB_HOST}:{cls.DB_PORT}/{cls.DB_NAME}"
    
    @classmethod
    def is_development(cls) -> bool:
        """Check if running in development mode."""
        return cls.NODE_ENV == 'development'
    
    @classmethod
    def is_production(cls) -> bool:
        """Check if running in production mode."""
        return cls.NODE_ENV == 'production'
    
    @classmethod
    def is_test(cls) -> bool:
        """Check if running in test mode."""
        return cls.NODE_ENV == 'test'

