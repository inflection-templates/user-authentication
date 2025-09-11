"""
Startup configurations
Equivalent to the .NET shala.api.startup.configurations
"""

import os
import logging
from fastapi import FastAPI

logger = logging.getLogger(":")


async def configure_services(app: FastAPI):
    """
    Configure application services
    Equivalent to the .NET builder.AddConfigs()
    """
    
    # Load configuration
    app.state.config = {
        "database": {
            "connection_string": os.getenv("DATABASE_URL", "sqlite+aiosqlite:///./user_service.db")
        },
        "jwt": {
            "issuer": os.getenv("JWT_ISSUER", "shala"),
            "audience": os.getenv("JWT_AUDIENCE", "shala"),
            "access_token_validity_days": int(os.getenv("JWT_ACCESS_TOKEN_VALIDITY_DAYS", "5")),
            "refresh_token_validity_days": int(os.getenv("JWT_REFRESH_TOKEN_VALIDITY_DAYS", "365"))
        },
        "email": {
            "from_address": os.getenv("EMAIL_FROM", "support@example.com"),
            "smtp_host": os.getenv("SMTP_HOST", "localhost"),
            "smtp_port": int(os.getenv("SMTP_PORT", "587")),
            "smtp_username": os.getenv("SMTP_USERNAME", ""),
            "smtp_password": os.getenv("SMTP_PASSWORD", "")
        },
        "oauth": {
            "google": {
                "enabled": os.getenv("OAUTH_GOOGLE_ENABLED", "false").lower() == "true",
                "client_id": os.getenv("OAUTH_GOOGLE_CLIENT_ID", ""),
                "client_secret": os.getenv("OAUTH_GOOGLE_CLIENT_SECRET", "")
            },
            "facebook": {
                "enabled": os.getenv("OAUTH_FACEBOOK_ENABLED", "false").lower() == "true",
                "client_id": os.getenv("OAUTH_FACEBOOK_CLIENT_ID", ""),
                "client_secret": os.getenv("OAUTH_FACEBOOK_CLIENT_SECRET", "")
            },
            "microsoft": {
                "enabled": os.getenv("OAUTH_MICROSOFT_ENABLED", "false").lower() == "true",
                "client_id": os.getenv("OAUTH_MICROSOFT_CLIENT_ID", ""),
                "client_secret": os.getenv("OAUTH_MICROSOFT_CLIENT_SECRET", "")
            },
            "github": {
                "enabled": os.getenv("OAUTH_GITHUB_ENABLED", "false").lower() == "true",
                "client_id": os.getenv("OAUTH_GITHUB_CLIENT_ID", ""),
                "client_secret": os.getenv("OAUTH_GITHUB_CLIENT_SECRET", "")
            }
        },
        "cache": {
            "enabled": os.getenv("CACHE_ENABLED", "true").lower() == "true",
            "provider": os.getenv("CACHE_PROVIDER", "memory"),
            "redis_connection_string": os.getenv("REDIS_CONNECTION_STRING", "")
        },
        "telemetry": {
            "enabled": os.getenv("TELEMETRY_ENABLED", "true").lower() == "true",
            "tracing_enabled": os.getenv("TRACING_ENABLED", "true").lower() == "true",
            "metrics_enabled": os.getenv("METRICS_ENABLED", "true").lower() == "true"
        }
    }
    
    logger.info("Application configuration loaded")
    logger.info(f"   • Database: {app.state.config['database']['connection_string']}")
    logger.info(f"   • JWT Issuer: {app.state.config['jwt']['issuer']}")
    logger.info(f"   • Cache Provider: {app.state.config['cache']['provider']}")
    logger.info(f"   • Telemetry Enabled: {app.state.config['telemetry']['enabled']}")
    
    # Configure OAuth providers
    oauth_providers = []
    for provider, config in app.state.config["oauth"].items():
        if config["enabled"]:
            oauth_providers.append(provider)
    
    if oauth_providers:
        logger.info(f"   • OAuth Providers: {', '.join(oauth_providers)}")
    else:
        logger.info("   • OAuth Providers: None enabled")
    
    return app.state.config
