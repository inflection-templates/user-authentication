"""
Google OAuth Routes - Simple implementation like C# version
"""

import logging
from fastapi import APIRouter, Depends, Request, Query, HTTPException
from fastapi.responses import RedirectResponse
from sqlalchemy.ext.asyncio import AsyncSession

from database.db_context import get_db_session
from api.users.social_auth.google_handler import GoogleOAuthHandler

logger = logging.getLogger(__name__)

router = APIRouter(prefix="/oauth", tags=["Google OAuth"])


async def get_google_handler(
    db_session: AsyncSession = Depends(get_db_session),
    request: Request = None
) -> GoogleOAuthHandler:
    """Get Google OAuth handler with dependencies"""
    return GoogleOAuthHandler(db_session, request.app.state.config)


@router.get("/google")
async def google_redirect(
    code: str = Query(..., description="Authorization code from Google"),
    state: str = Query(..., description="State parameter"),
    request: Request = None,
    handler: GoogleOAuthHandler = Depends(get_google_handler)
):
    """
    Google OAuth callback - equivalent to C# GoogleRedirect method
    
    This endpoint handles the Google OAuth callback and processes the authorization code
    to authenticate the user and generate JWT tokens.
    
    Args:
        code: Authorization code from Google
        state: State parameter for OAuth security
    
    Returns:
        Redirects to frontend with authentication result
    """
    try:
        # Process Google OAuth callback
        result = await handler.google_redirect(code, state)
        
        # Get frontend redirect URL
        google_config = request.app.state.config.get("oauth", {}).get("google", {})
        frontend_url = google_config.get("frontend_redirect_url", "http://localhost:5000")
        
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
        error_params = f"success=false&message=Google OAuth authentication failed: {str(e)}"
        frontend_url = "http://localhost:5000"  # Fallback URL
        redirect_url = f"{frontend_url}?{error_params}"
        
        return RedirectResponse(url=redirect_url)


@router.get("/google/login")
async def google_login(request: Request = None):
    """
    Initiate Google OAuth login
    
    Returns the Google authorization URL that the user should visit
    """
    try:
        # Get Google configuration
        google_config = request.app.state.config.get("oauth", {}).get("google", {})
        client_id = google_config.get("client_id")
        callback_url = google_config.get("callback_url")
        
        # Debug logging
        logger.info(f"Google config: {google_config}")
        logger.info(f"Client ID: {client_id}")
        logger.info(f"Callback URL: {callback_url}")
        
        if not client_id:
            raise HTTPException(
                status_code=500,
                detail="Google OAuth not configured"
            )
        
        if not callback_url:
            raise HTTPException(
                status_code=500,
                detail="Google callback URL not configured in environment"
            )
        
        # Generate Google OAuth URL
        auth_url = (
            f"https://accounts.google.com/o/oauth2/v2/auth"
            f"?client_id={client_id}"
            f"&redirect_uri={callback_url}"
            f"&scope=openid email profile"
            f"&response_type=code"
            f"&access_type=offline"
            f"&prompt=select_account"
            f"&state=google_oauth_state"
        )
        
        return {
            "success": True,
            "message": "Google OAuth login initiated",
            "data": {
                "authorization_url": auth_url,
                "provider": "Google"
            }
        }
        
    except Exception as e:
        logger.error(f"Error initiating Google OAuth: {e}")
        raise HTTPException(
            status_code=500,
            detail=f"Failed to initiate Google OAuth: {str(e)}"
        )
