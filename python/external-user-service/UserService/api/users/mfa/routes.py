"""
MFA API routes
FastAPI routes for MFA functionality
"""

import logging
from typing import Dict, Any

from fastapi import APIRouter, Depends, Response, HTTPException, status
from fastapi.responses import StreamingResponse
from sqlalchemy.ext.asyncio import AsyncSession

from api.users.mfa.handlers import (
    MfaHandler,
    MfaSetupRequest,
    MfaVerifySetupRequest,
    MfaValidateRequest,
    MfaDisableRequest,
    GenerateBackupCodesRequest,
    MfaSetupResponse,
    MfaValidationResponse,
    MfaStatusResponse,
    BackupCodesResponse
)
from services.mfa_service import MfaService
from database.database import get_database_session
from domain.types.user_types import User
from startup.dependencies import get_current_user
import io

logger = logging.getLogger(__name__)

# Create router
mfa_router = APIRouter(prefix="/api/v1/users/mfa", tags=["MFA"])


def get_mfa_service(db_session: AsyncSession = Depends(get_database_session)) -> MfaService:
    """Dependency to get MFA service"""
    return MfaService(db_session)


def get_mfa_handler(mfa_service: MfaService = Depends(get_mfa_service)) -> MfaHandler:
    """Dependency to get MFA handler"""
    return MfaHandler(mfa_service)


# Routes

@mfa_router.post("/setup-totp", response_model=MfaSetupResponse)
async def setup_totp(
    request_data: MfaSetupRequest,
    current_user: User = Depends(get_current_user),
    mfa_handler: MfaHandler = Depends(get_mfa_handler)
) -> MfaSetupResponse:
    """
    Setup TOTP for the current user
    
    Returns QR code and backup codes for TOTP setup
    """
    return await mfa_handler.setup_totp(current_user.id, request_data, current_user)


@mfa_router.post("/verify-totp-setup")
async def verify_totp_setup(
    request_data: MfaVerifySetupRequest,
    current_user: User = Depends(get_current_user),
    mfa_handler: MfaHandler = Depends(get_mfa_handler)
) -> Dict[str, Any]:
    """
    Verify and enable TOTP for the current user
    
    Requires a valid TOTP token from the user's authenticator app
    """
    return await mfa_handler.verify_totp_setup(current_user.id, request_data)


@mfa_router.post("/validate", response_model=MfaValidationResponse)
async def validate_mfa(
    request_data: MfaValidateRequest,
    current_user: User = Depends(get_current_user),
    mfa_handler: MfaHandler = Depends(get_mfa_handler)
) -> MfaValidationResponse:
    """
    Validate MFA token (TOTP or backup code)
    
    Supports both TOTP tokens and backup codes
    """
    return await mfa_handler.validate_mfa(current_user.id, request_data)


@mfa_router.get("/status", response_model=MfaStatusResponse)
async def get_mfa_status(
    current_user: User = Depends(get_current_user),
    mfa_handler: MfaHandler = Depends(get_mfa_handler)
) -> MfaStatusResponse:
    """
    Get MFA status for the current user
    
    Returns whether MFA is enabled and which methods are available
    """
    return await mfa_handler.get_mfa_status(current_user.id)


@mfa_router.post("/disable")
async def disable_mfa(
    request_data: MfaDisableRequest,
    current_user: User = Depends(get_current_user),
    mfa_handler: MfaHandler = Depends(get_mfa_handler)
) -> Dict[str, Any]:
    """
    Disable MFA for the current user
    
    Requires current TOTP token for verification
    """
    return await mfa_handler.disable_mfa(current_user.id, request_data)


@mfa_router.post("/generate-backup-codes", response_model=BackupCodesResponse)
async def generate_backup_codes(
    request_data: GenerateBackupCodesRequest,
    current_user: User = Depends(get_current_user),
    mfa_handler: MfaHandler = Depends(get_mfa_handler)
) -> BackupCodesResponse:
    """
    Generate new backup codes
    
    Requires current TOTP token for verification.
    Old backup codes will be invalidated.
    """
    return await mfa_handler.generate_backup_codes(current_user.id, request_data)


@mfa_router.get("/qr-code")
async def get_qr_code(
    current_user: User = Depends(get_current_user),
    mfa_handler: MfaHandler = Depends(get_mfa_handler)
):
    """
    Get QR code for TOTP setup as PNG image
    
    Returns a PNG image that can be scanned by authenticator apps
    """
    try:
        qr_code_bytes = await mfa_handler.get_qr_code_image(current_user.id, current_user)
        
        return StreamingResponse(
            io.BytesIO(qr_code_bytes),
            media_type="image/png",
            headers={"Content-Disposition": "inline; filename=totp-qr-code.png"}
        )
    
    except Exception as e:
        logger.error(f"Error generating QR code: {str(e)}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Error generating QR code"
        )


# Health check for MFA service
@mfa_router.get("/health")
async def mfa_health_check():
    """
    Health check endpoint for MFA service
    """
    return {
        "status": "healthy",
        "service": "MFA Service",
        "features": ["TOTP", "Backup Codes", "QR Code Generation"]
    }
