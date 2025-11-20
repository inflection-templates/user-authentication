"""
MFA API handlers
Handles MFA-related HTTP requests including TOTP setup, validation, and management
"""

import logging
from typing import Optional, Dict, Any
from uuid import UUID

from fastapi import HTTPException, status, Response
from pydantic import BaseModel, Field

from services.mfa_service import MfaService, MfaSetupResult, MfaValidationResult, MfaStatus
from domain.types.user_types import User

logger = logging.getLogger(__name__)


# Request/Response Models

class MfaSetupRequest(BaseModel):
    user_identifier: Optional[str] = Field(None, description="User identifier for QR code label")


class MfaVerifySetupRequest(BaseModel):
    token: str = Field(..., min_length=6, max_length=6, description="6-digit TOTP token")


class MfaValidateRequest(BaseModel):
    token: str = Field(..., description="TOTP token or backup code")
    method: Optional[str] = Field("totp", description="Authentication method: 'totp' or 'backup'")


class MfaDisableRequest(BaseModel):
    token: str = Field(..., min_length=6, max_length=6, description="Current TOTP token")


class GenerateBackupCodesRequest(BaseModel):
    token: str = Field(..., min_length=6, max_length=6, description="Current TOTP token")


class MfaSetupResponse(BaseModel):
    qr_code_data_url: Optional[str] = None
    qr_code_url: Optional[str] = None
    backup_codes: Optional[list] = None
    message: str


class MfaValidationResponse(BaseModel):
    valid: bool
    method: str
    message: str


class MfaStatusResponse(BaseModel):
    enabled: bool
    methods: list
    backup_codes_remaining: Optional[int] = None


class BackupCodesResponse(BaseModel):
    backup_codes: list
    message: str


class MfaHandler:
    """MFA request handler"""

    def __init__(self, mfa_service: MfaService):
        self.mfa_service = mfa_service

    async def setup_totp(self, user_id: UUID, request_data: MfaSetupRequest, user: User) -> MfaSetupResponse:
        """
        Setup TOTP for the current user
        """
        try:
            user_identifier = request_data.user_identifier or user.email or user.username or "User"
            
            result: MfaSetupResult = await self.mfa_service.setup_totp(user_id, user_identifier)

            if result.success:
                return MfaSetupResponse(
                    qr_code_data_url=result.totp_secret.qr_code_data_url if result.totp_secret else None,
                    qr_code_url=result.totp_secret.qr_code_url if result.totp_secret else None,
                    backup_codes=result.backup_codes,
                    message=result.message
                )
            else:
                raise HTTPException(
                    status_code=status.HTTP_400_BAD_REQUEST,
                    detail=result.message
                )

        except HTTPException:
            raise
        except Exception as e:
            logger.error(f"Error in setup_totp: {str(e)}")
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail="Internal server error during TOTP setup"
            )

    async def verify_totp_setup(self, user_id: UUID, request_data: MfaVerifySetupRequest) -> Dict[str, Any]:
        """
        Verify and enable TOTP for the current user
        """
        try:
            if not request_data.token or len(request_data.token) != 6:
                raise HTTPException(
                    status_code=status.HTTP_400_BAD_REQUEST,
                    detail="Invalid TOTP token format"
                )

            result: MfaSetupResult = await self.mfa_service.verify_and_enable_totp(
                user_id, 
                request_data.token
            )

            if result.success:
                return {
                    "message": result.message,
                    "enabled": True
                }
            else:
                raise HTTPException(
                    status_code=status.HTTP_400_BAD_REQUEST,
                    detail=result.message
                )

        except HTTPException:
            raise
        except Exception as e:
            logger.error(f"Error in verify_totp_setup: {str(e)}")
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail="Internal server error during TOTP verification"
            )

    async def validate_mfa(self, user_id: UUID, request_data: MfaValidateRequest) -> MfaValidationResponse:
        """
        Validate MFA token (TOTP or backup code)
        """
        try:
            if not request_data.token:
                raise HTTPException(
                    status_code=status.HTTP_400_BAD_REQUEST,
                    detail="Token is required"
                )

            method = request_data.method or "totp"
            result: MfaValidationResult = await self.mfa_service.validate_mfa(
                user_id,
                request_data.token,
                method
            )

            if result.success:
                return MfaValidationResponse(
                    valid=True,
                    method=result.method,
                    message=result.message
                )
            else:
                raise HTTPException(
                    status_code=status.HTTP_400_BAD_REQUEST,
                    detail=result.message
                )

        except HTTPException:
            raise
        except Exception as e:
            logger.error(f"Error in validate_mfa: {str(e)}")
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail="Internal server error during MFA validation"
            )

    async def get_mfa_status(self, user_id: UUID) -> MfaStatusResponse:
        """
        Get MFA status for the current user
        """
        try:
            status_info: MfaStatus = await self.mfa_service.get_mfa_status(user_id)

            return MfaStatusResponse(
                enabled=status_info.enabled,
                methods=status_info.methods,
                backup_codes_remaining=status_info.backup_codes_remaining
            )

        except Exception as e:
            logger.error(f"Error in get_mfa_status: {str(e)}")
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail="Internal server error while retrieving MFA status"
            )

    async def disable_mfa(self, user_id: UUID, request_data: MfaDisableRequest) -> Dict[str, Any]:
        """
        Disable MFA for the current user
        """
        try:
            if not request_data.token:
                raise HTTPException(
                    status_code=status.HTTP_400_BAD_REQUEST,
                    detail="TOTP token is required to disable MFA"
                )

            success = await self.mfa_service.disable_mfa(user_id, request_data.token)

            if success:
                return {
                    "enabled": False,
                    "message": "Multi-factor authentication has been disabled for your account"
                }
            else:
                raise HTTPException(
                    status_code=status.HTTP_400_BAD_REQUEST,
                    detail="Failed to disable MFA"
                )

        except HTTPException:
            raise
        except Exception as e:
            logger.error(f"Error in disable_mfa: {str(e)}")
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail="Internal server error while disabling MFA"
            )

    async def generate_backup_codes(self, user_id: UUID, request_data: GenerateBackupCodesRequest) -> BackupCodesResponse:
        """
        Generate new backup codes
        """
        try:
            if not request_data.token:
                raise HTTPException(
                    status_code=status.HTTP_400_BAD_REQUEST,
                    detail="TOTP token is required to generate backup codes"
                )

            backup_codes = await self.mfa_service.generate_new_backup_codes(user_id, request_data.token)

            return BackupCodesResponse(
                backup_codes=backup_codes,
                message="Please save these backup codes in a secure location. "
                       "They can be used to access your account if you lose your authenticator device."
            )

        except HTTPException:
            raise
        except Exception as e:
            logger.error(f"Error in generate_backup_codes: {str(e)}")
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail="Internal server error while generating backup codes"
            )

    async def get_qr_code_image(self, user_id: UUID, user: User) -> bytes:
        """
        Get QR code for TOTP setup as PNG image
        """
        try:
            user_identifier = user.email or user.username or "User"
            result: MfaSetupResult = await self.mfa_service.setup_totp(user_id, user_identifier)

            if result.success and result.totp_secret:
                # Get QR code as image bytes
                from services.totp_service import TotpService
                totp_service = TotpService()
                return totp_service.get_qr_code_image(result.totp_secret.qr_code_url)
            else:
                raise HTTPException(
                    status_code=status.HTTP_400_BAD_REQUEST,
                    detail=result.message
                )

        except HTTPException:
            raise
        except Exception as e:
            logger.error(f"Error in get_qr_code_image: {str(e)}")
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail="Internal server error while generating QR code"
            )
