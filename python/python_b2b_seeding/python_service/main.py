"""
Application entry point.
Similar to the Node.js index.ts implementation.
"""
import os
import sys
from pathlib import Path

# Add project root to path
project_root = Path(__file__).parent
sys.path.insert(0, str(project_root))

from app import start
from config.config import Config

if __name__ == "__main__":
    # Print startup information
    print("=" * 50)
    print(f"Service: {Config.SERVICE_NAME}")
    print(f"Environment: {Config.NODE_ENV}")
    print(f"Version: {Config.SERVICE_VERSION}")
    print(f"Port: {Config.PORT}")
    print(f"Database: {Config.DB_DIALECT}://{Config.DB_HOST}:{Config.DB_PORT}/{Config.DB_NAME}")
    print("=" * 50)
    
    # Start the application
    start()

