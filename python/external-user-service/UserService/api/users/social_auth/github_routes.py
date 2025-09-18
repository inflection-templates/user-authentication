"""
GitHub OAuth Routes - Simple implementation like C# version
"""

import logging
from fastapi import APIRouter, Depends, Request, Query, HTTPException
from fastapi.responses import RedirectResponse
from sqlalchemy.ext.asyncio import AsyncSession

from database.db_context import get_db_session
from api.users.social_auth.github_handler import GitHubOAuthHandler

logger = logging.getLogger(__name__)

router = APIRouter(prefix="/oauth", tags=["GitHub OAuth"])


async def get_github_handler(
    db_session: AsyncSession = Depends(get_db_session),
    request: Request = None
) -> GitHubOAuthHandler:
    """Get GitHub OAuth handler with dependencies"""
    return GitHubOAuthHandler(db_session, request.app.state.config)


@router.get("/github")
async def github_redirect(
    code: str = Query(..., description="Authorization code from GitHub"),
    state: str = Query(..., description="State parameter"),
    request: Request = None,
    handler: GitHubOAuthHandler = Depends(get_github_handler)
):
    """
    GitHub OAuth callback - equivalent to C# GithubRedirect method
    
    This endpoint handles the GitHub OAuth callback and processes the authorization code
    to authenticate the user and generate JWT tokens.
    
    Args:
        code: Authorization code from GitHub
        state: State parameter for OAuth security
    
    Returns:
        Redirects to frontend with authentication result
    """
    try:
        # Process GitHub OAuth callback
        result = await handler.github_redirect(code, state)
        
        # Get frontend redirect URL
        github_config = request.app.state.config.get("oauth", {}).get("github", {})
        frontend_url = github_config.get("frontend_redirect_url", "http://localhost:5000")
        
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


@router.get("/github/login")
async def github_login(request: Request = None):
    """
    Initiate GitHub OAuth login
    
    Returns the GitHub authorization URL that the user should visit
    """
    try:
        # Get GitHub configuration
        github_config = request.app.state.config.get("oauth", {}).get("github", {})
        client_id = github_config.get("client_id")
        callback_url = github_config.get("callback_url")
        
        # Debug logging
        logger.info(f"GitHub config: {github_config}")
        logger.info(f"Client ID: {client_id}")
        logger.info(f"Callback URL: {callback_url}")
        
        if not client_id:
            raise HTTPException(
                status_code=500,
                detail="GitHub OAuth not configured"
            )
        
        if not callback_url:
            raise HTTPException(
                status_code=500,
                detail="GitHub callback URL not configured in environment"
            )
        
        # Generate GitHub OAuth URL
        auth_url = (
            f"https://github.com/login/oauth/authorize"
            f"?client_id={client_id}"
            f"&redirect_uri={callback_url}"
            f"&scope=user:email"
            f"&state=github_oauth_state"
        )
        
        return {
            "success": True,
            "message": "GitHub OAuth login initiated",
            "data": {
                "authorization_url": auth_url,
                "provider": "GitHub"
            }
        }
        
    except Exception as e:
        logger.error(f"Error initiating GitHub OAuth: {e}")
        raise HTTPException(
            status_code=500,
            detail=f"Failed to initiate GitHub OAuth: {str(e)}"
        )
