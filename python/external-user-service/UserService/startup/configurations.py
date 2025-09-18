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
                "client_id": os.getenv("OAUTH_GOOGLE_CLIENT_ID", "your_google_client_id_here"),
                "client_secret": os.getenv("OAUTH_GOOGLE_CLIENT_SECRET", "your_google_client_secret_here"),
                "callback_url": os.getenv("OAUTH_GOOGLE_CALLBACK_URL", "http://localhost:5000/api/v1/users/oauth/google"),
                "frontend_redirect_url": os.getenv("OAUTH_GOOGLE_FRONTEND_REDIRECT_URL", "http://localhost:5000"),
                "auth_url": "https://accounts.google.com/o/oauth2/v2/auth",
                "token_url": "https://oauth2.googleapis.com/token",
                "user_info_url": "https://www.googleapis.com/oauth2/v2/userinfo",
                "scope": "openid email profile"
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
                "enabled": os.getenv("OAUTH_GITHUB_ENABLED", "true").lower() == "true",
                "client_id": os.getenv("OAUTH_GITHUB_CLIENT_ID", "your_github_client_id_here"),
                "client_secret": os.getenv("OAUTH_GITHUB_CLIENT_SECRET", "your_github_client_secret_here"),
                "callback_url": os.getenv("OAUTH_GITHUB_CALLBACK_URL", "http://localhost:5000/api/v1/users/oauth/github"),
                "frontend_redirect_url": os.getenv("OAUTH_GITHUB_FRONTEND_REDIRECT_URL", "http://localhost:5000"),
                "auth_url": "https://github.com/login/oauth/authorize",
                "token_url": "https://github.com/login/oauth/access_token",
                "user_info_url": "https://api.github.com/user",
                "user_emails_url": "https://api.github.com/user/emails",
                "scope": "user:email"
            },
            "twitter": {
                "enabled": os.getenv("OAUTH_TWITTER_ENABLED", "false").lower() == "true",
                "client_id": os.getenv("OAUTH_TWITTER_CLIENT_ID", "your_twitter_client_id_here"),
                "client_secret": os.getenv("OAUTH_TWITTER_CLIENT_SECRET", "your_twitter_client_secret_here"),
                "callback_url": os.getenv("OAUTH_TWITTER_CALLBACK_URL", "http://localhost:5000/api/v1/users/oauth/twitter"),
                "frontend_redirect_url": os.getenv("OAUTH_TWITTER_FRONTEND_REDIRECT_URL", "http://localhost:5000"),
                "auth_url": "https://twitter.com/i/oauth2/authorize",
                "token_url": "https://api.twitter.com/2/oauth2/token",
                "user_info_url": "https://api.twitter.com/2/users/me",
                "scope": "tweet.read users.read offline.access"
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
