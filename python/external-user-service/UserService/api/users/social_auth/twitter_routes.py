"""
Twitter OAuth Routes - Simple implementation like C# version
"""

import logging
import secrets
import base64
import hashlib
from fastapi import APIRouter, Depends, Request, Query, HTTPException
from fastapi.responses import RedirectResponse
from sqlalchemy.ext.asyncio import AsyncSession

from database.db_context import get_db_session
from api.users.social_auth.twitter_handler import TwitterOAuthHandler

logger = logging.getLogger(__name__)

router = APIRouter(prefix="/oauth", tags=["Twitter OAuth"])


async def get_twitter_handler(
    db_session: AsyncSession = Depends(get_db_session),
    request: Request = None
) -> TwitterOAuthHandler:
    """Get Twitter OAuth handler with dependencies"""
    return TwitterOAuthHandler(db_session, request.app.state.config)


@router.get("/twitter")
async def twitter_redirect(
    code: str = Query(..., description="Authorization code from Twitter"),
    state: str = Query(..., description="State parameter"),
    request: Request = None,
    handler: TwitterOAuthHandler = Depends(get_twitter_handler)
):
    """
    Twitter OAuth callback - equivalent to C# TwitterRedirect method
    
    This endpoint handles the Twitter OAuth callback and processes the authorization code
    to authenticate the user and generate JWT tokens.
    
    Args:
        code: Authorization code from Twitter
        state: State parameter for OAuth security
    
    Returns:
        Redirects to frontend with authentication result
    """
    try:
        # Process Twitter OAuth callback
        result = await handler.twitter_redirect(code, state)
        
        # Get frontend redirect URL
        twitter_config = request.app.state.config.get("oauth", {}).get("twitter", {})
        frontend_url = twitter_config.get("frontend_redirect_url", "http://localhost:5000")
        
        if result.get("success"):
            # Redirect to frontend with success parameters
            success_params = (
                f"success=true"
                f"&message={result['message']}"
                f"&token={result['data']['token']}"
                f"&user_id={result['data']['user_id']}"
                f"&username={result['data']['username']}"
                f"&session_id={result['data']['session_id']}"
            )
            redirect_url = f"{frontend_url}?{success_params}"
            
            return RedirectResponse(url=redirect_url)
        else:
            # Redirect to frontend with error parameters
            error_params = f"success=false&message={result.get('message', 'OAuth failed')}"
            redirect_url = f"{frontend_url}?{error_params}"
            
            return RedirectResponse(url=redirect_url)
            
    except Exception as e:
        # Redirect to frontend with error
        error_params = f"success=false&message=OAuth authentication failed: {str(e)}"
        frontend_url = "http://localhost:5000"  # Fallback URL
        redirect_url = f"{frontend_url}?{error_params}"
        
        return RedirectResponse(url=redirect_url)


@router.get("/twitter/login")
async def twitter_login(request: Request = None):
    """
    Initiate Twitter OAuth login
    
    Returns the Twitter authorization URL that the user should visit
    """
    try:
        # Get Twitter configuration
        twitter_config = request.app.state.config.get("oauth", {}).get("twitter", {})
        client_id = twitter_config.get("client_id")
        callback_url = twitter_config.get("callback_url")
        
        # Debug logging
        logger.info(f"Twitter config: {twitter_config}")
        logger.info(f"Client ID: {client_id}")
        logger.info(f"Callback URL: {callback_url}")
        
        if not client_id:
            raise HTTPException(
                status_code=500,
                detail="Twitter OAuth not configured"
            )
        
        if not callback_url:
            raise HTTPException(
                status_code=500,
                detail="Twitter callback URL not configured in environment"
            )
        
        # Generate PKCE challenge for Twitter OAuth 2.0
        code_verifier = base64.urlsafe_b64encode(secrets.token_bytes(32)).decode('utf-8').rstrip('=')
        code_challenge = base64.urlsafe_b64encode(
            hashlib.sha256(code_verifier.encode('utf-8')).digest()
        ).decode('utf-8').rstrip('=')
        
        # Generate state parameter for security
        state = secrets.token_urlsafe(32)
        
        # Generate Twitter OAuth URL using OAuth 2.0 with PKCE
        auth_url = (
            f"https://twitter.com/i/oauth2/authorize"
            f"?response_type=code"
            f"&client_id={client_id}"
            f"&redirect_uri={callback_url}"
            f"&scope=tweet.read users.read offline.access"
            f"&state={state}"
            f"&code_challenge={code_challenge}"
            f"&code_challenge_method=S256"
        )
        
        # In production, you should store code_verifier and state in session/cache
        # For now, we'll return them for the client to handle
        return {
            "success": True,
            "message": "Twitter OAuth login initiated",
            "data": {
                "authorization_url": auth_url,
                "provider": "Twitter",
                "code_verifier": code_verifier,  # Client should store this securely
                "state": state
            }
        }
        
    except Exception as e:
        logger.error(f"Error initiating Twitter OAuth: {e}")
        raise HTTPException(
            status_code=500,
            detail=f"Failed to initiate Twitter OAuth: {str(e)}"
        )
