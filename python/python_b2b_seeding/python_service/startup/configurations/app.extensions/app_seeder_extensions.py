"""
Application seeder extensions.
Similar to the dotnet AppSeederExtensions.cs implementation.
This provides a helper function to use the seeder, matching the dotnet extension pattern.
"""
from typing import TYPE_CHECKING

if TYPE_CHECKING:
    from fastapi import FastAPI


def use_seeder(app: 'FastAPI'):
    """
    Extension method to use seeder on application startup.
    Similar to dotnet UseSeeder extension method.
    
    Args:
        app: FastAPI application instance
        
    Returns:
        The app instance (for method chaining)
    """
    from config.config import Config
    from startup.seeder import Seeder
    
    # Get environment info
    is_dev_environment = Config.is_development()
    is_test_environment = Config.is_test()
    
    # Run seeding (synchronously as dotnet does)
    if Config.AUTO_SEED_ON_STARTUP:
        Seeder.seed_async(is_dev_environment, is_test_environment)
    
    return app

