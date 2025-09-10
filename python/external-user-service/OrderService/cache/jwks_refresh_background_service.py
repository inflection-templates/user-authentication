"""
JWKS Refresh Background Service
Background task for periodically refreshing JWKS keys similar to .NET implementation
"""

import asyncio
import logging
from datetime import datetime, timedelta
from typing import Optional

from .jwt_authentication_service import JwtAuthenticationService

logger = logging.getLogger(__name__)


class JwksRefreshBackgroundService:
    """Background service for refreshing JWKS keys periodically"""
    
    def __init__(self, 
                 jwt_service: JwtAuthenticationService,
                 refresh_interval_minutes: int = 5):
        """
        Initialize background service
        
        Args:
            jwt_service: JWT authentication service to refresh keys for
            refresh_interval_minutes: How often to refresh keys
        """
        self._jwt_service = jwt_service
        self._refresh_interval = timedelta(minutes=refresh_interval_minutes)
        self._task: Optional[asyncio.Task] = None
        self._stop_event = asyncio.Event()
    
    async def start_async(self):
        """Start the background refresh task"""
        if self._task and not self._task.done():
            logger.warning("Background refresh service is already running")
            return
        
        logger.info("Starting JWKS refresh background service")
        self._stop_event.clear()
        self._task = asyncio.create_task(self._refresh_loop())
    
    async def stop_async(self):
        """Stop the background refresh task"""
        if not self._task or self._task.done():
            logger.warning("Background refresh service is not running")
            return
        
        logger.info("Stopping JWKS refresh background service")
        self._stop_event.set()
        
        try:
            await self._task
        except asyncio.CancelledError:
            pass
        
        logger.info("JWKS refresh background service stopped")
    
    async def _refresh_loop(self):
        """Main refresh loop"""
        try:
            # Initial refresh
            await self._jwt_service.refresh_keys_async()
            
            while not self._stop_event.is_set():
                try:
                    # Wait for the refresh interval or stop event
                    await asyncio.wait_for(
                        self._stop_event.wait(), 
                        timeout=self._refresh_interval.total_seconds()
                    )
                    # If we reach here, stop event was set
                    break
                except asyncio.TimeoutError:
                    # Timeout is expected, time to refresh
                    pass
                
                if not self._stop_event.is_set():
                    logger.debug("Performing periodic JWKS refresh")
                    await self._jwt_service.refresh_keys_async()
                    
        except asyncio.CancelledError:
            logger.info("JWKS refresh background service was cancelled")
            raise
        except Exception as ex:
            logger.error(f"Error in JWKS refresh background service: {ex}")
        finally:
            logger.info("JWKS refresh background service loop ended")
