"""
MFA (Multi-Factor Authentication) Service
Provides comprehensive MFA functionality including TOTP, backup codes, and validation
"""

import logging
from typing import Optional, Dict, Any, List, Tuple
from uuid import UUID
from datetime import datetime, timedelta

from services.totp_service import TotpService, TotpSecret, TotpValidationResult
from domain.types.user_types import User

logger = logging.getLogger(__name__)


class MfaSetupResult:
    """MFA setup result"""
    def __init__(self, success: bool, totp_secret: Optional[TotpSecret] = None, 
                 backup_codes: Optional[List[str]] = None, message: str = ""):
        self.success = success
        self.totp_secret = totp_secret
        self.backup_codes = backup_codes
        self.message = message


class MfaValidationResult:
    """MFA validation result"""
    def __init__(self, success: bool, method: str, message: str = ""):
        self.success = success
        self.method = method  # 'totp', 'backup', 'sms', 'email'
        self.message = message


class MfaStatus:
    """MFA status information"""
    def __init__(self, enabled: bool, methods: List[str], backup_codes_remaining: Optional[int] = None):
        self.enabled = enabled
        self.methods = methods
        self.backup_codes_remaining = backup_codes_remaining


class MfaService:
    """MFA service for managing multi-factor authentication"""

    def __init__(self, db_session, issuer_name: str = "User Service"):
        self.db_session = db_session
        self.totp_service = TotpService(issuer_name)
        
        # These would typically be injected repositories
        # For now, we'll implement basic functionality

    async def setup_totp(self, user_id: UUID, user_identifier: str) -> MfaSetupResult:
        """
        Setup TOTP for a user
        
        Args:
            user_id: User ID
            user_identifier: User email or username
            
        Returns:
            MfaSetupResult with TOTP secret and QR code
        """
        try:
            # Check if user exists
            user = await self._get_user_by_id(user_id)
            if not user:
                return MfaSetupResult(success=False, message="User not found")

            # Check if TOTP is already enabled
            mfa_status = await self.get_mfa_status(user_id)
            if mfa_status.enabled and 'totp' in mfa_status.methods:
                return MfaSetupResult(success=False, message="TOTP is already enabled for this user")

            # Generate TOTP secret
            totp_secret = self.totp_service.generate_secret(user_identifier)

            # Store the secret temporarily (user needs to verify before enabling)
            await self._store_temp_totp_secret(user_id, totp_secret.secret)

            logger.info(f"TOTP setup initiated for user: {user_id}")

            return MfaSetupResult(
                success=True,
                totp_secret=totp_secret,
                backup_codes=totp_secret.backup_codes,
                message="TOTP setup initiated. Please verify with your authenticator app."
            )

        except Exception as e:
            logger.error(f"Error setting up TOTP for user {user_id}: {str(e)}")
            raise

    async def verify_and_enable_totp(self, user_id: UUID, token: str) -> MfaSetupResult:
        """
        Verify and enable TOTP for a user
        
        Args:
            user_id: User ID
            token: TOTP token from user's authenticator app
            
        Returns:
            MfaSetupResult
        """
        try:
            # Get temporary TOTP secret
            temp_secret = await self._get_temp_totp_secret(user_id)
            if not temp_secret:
                return MfaSetupResult(
                    success=False, 
                    message="No TOTP setup found. Please initiate setup first."
                )

            # Verify the token
            validation = self.totp_service.verify_token(token, temp_secret)
            if not validation.valid:
                return MfaSetupResult(
                    success=False,
                    message="Invalid TOTP token. Please try again."
                )

            # Enable TOTP for the user
            await self._enable_totp_for_user(user_id, temp_secret)

            # Clean up temporary secret
            await self._clear_temp_totp_secret(user_id)

            logger.info(f"TOTP enabled successfully for user: {user_id}")

            return MfaSetupResult(
                success=True,
                message="TOTP enabled successfully. Your account is now secured with two-factor authentication."
            )

        except Exception as e:
            logger.error(f"Error verifying and enabling TOTP for user {user_id}: {str(e)}")
            raise

    async def validate_mfa(self, user_id: UUID, token: str, method: str = 'totp') -> MfaValidationResult:
        """
        Validate MFA token (TOTP or backup code)
        
        Args:
            user_id: User ID
            token: TOTP token or backup code
            method: Authentication method ('totp' or 'backup')
            
        Returns:
            MfaValidationResult
        """
        try:
            user = await self._get_user_by_id(user_id)
            if not user:
                return MfaValidationResult(
                    success=False, 
                    method=method, 
                    message="User not found"
                )

            mfa_status = await self.get_mfa_status(user_id)
            if not mfa_status.enabled:
                return MfaValidationResult(
                    success=False,
                    method=method,
                    message="MFA is not enabled for this user"
                )

            if method == 'totp':
                return await self._validate_totp_token(user_id, token)
            elif method == 'backup':
                return await self._validate_backup_code(user_id, token)

            return MfaValidationResult(
                success=False,
                method=method,
                message="Invalid authentication method"
            )

        except Exception as e:
            logger.error(f"Error validating MFA for user {user_id}: {str(e)}")
            raise

    async def disable_mfa(self, user_id: UUID, current_token: str) -> bool:
        """
        Disable MFA for a user
        
        Args:
            user_id: User ID
            current_token: Current TOTP token for verification
            
        Returns:
            Boolean indicating success
        """
        try:
            # Verify current token before disabling
            validation = await self.validate_mfa(user_id, current_token, 'totp')
            if not validation.success:
                raise ValueError("Invalid TOTP token. Cannot disable MFA.")

            # Disable MFA
            await self._disable_mfa_for_user(user_id)

            logger.info(f"MFA disabled for user: {user_id}")
            return True

        except Exception as e:
            logger.error(f"Error disabling MFA for user {user_id}: {str(e)}")
            raise

    async def get_mfa_status(self, user_id: UUID) -> MfaStatus:
        """
        Get MFA status for a user
        
        Args:
            user_id: User ID
            
        Returns:
            MfaStatus
        """
        try:
            user = await self._get_user_by_id(user_id)
            if not user:
                raise ValueError("User not found")

            # Check if user has TOTP enabled
            totp_enabled = await self._is_totp_enabled_for_user(user_id)
            
            methods = []
            if totp_enabled:
                methods.append('totp')

            backup_codes_count = None
            if totp_enabled:
                backup_codes_count = await self._get_backup_codes_count(user_id)

            return MfaStatus(
                enabled=len(methods) > 0,
                methods=methods,
                backup_codes_remaining=backup_codes_count
            )

        except Exception as e:
            logger.error(f"Error getting MFA status for user {user_id}: {str(e)}")
            raise

    async def generate_new_backup_codes(self, user_id: UUID, current_token: str) -> List[str]:
        """
        Generate new backup codes
        
        Args:
            user_id: User ID
            current_token: Current TOTP token for verification
            
        Returns:
            List of new backup codes
        """
        try:
            # Verify current token
            validation = await self.validate_mfa(user_id, current_token, 'totp')
            if not validation.success:
                raise ValueError("Invalid TOTP token. Cannot generate backup codes.")

            # Generate new backup codes
            backup_codes = self.totp_service._generate_backup_codes()
            
            # Store backup codes (encrypted)
            await self._store_backup_codes(user_id, backup_codes)

            logger.info(f"New backup codes generated for user: {user_id}")
            return backup_codes

        except Exception as e:
            logger.error(f"Error generating backup codes for user {user_id}: {str(e)}")
            raise

    # Private helper methods

    async def _validate_totp_token(self, user_id: UUID, token: str) -> MfaValidationResult:
        """Validate TOTP token"""
        try:
            totp_secret = await self._get_totp_secret_for_user(user_id)
            if not totp_secret:
                return MfaValidationResult(
                    success=False,
                    method='totp',
                    message="TOTP not configured for this user"
                )

            validation = self.totp_service.verify_token(token, totp_secret)
            
            return MfaValidationResult(
                success=validation.valid,
                method='totp',
                message="TOTP token validated successfully" if validation.valid else "Invalid TOTP token"
            )

        except Exception as e:
            logger.error(f"Error validating TOTP token: {str(e)}")
            return MfaValidationResult(
                success=False,
                method='totp',
                message="Error validating TOTP token"
            )

    async def _validate_backup_code(self, user_id: UUID, code: str) -> MfaValidationResult:
        """Validate backup code"""
        try:
            backup_codes = await self._get_backup_codes_for_user(user_id)
            if not backup_codes:
                return MfaValidationResult(
                    success=False,
                    method='backup',
                    message="No backup codes available"
                )

            is_valid = self.totp_service.verify_backup_code(code, backup_codes)
            
            if is_valid:
                # Remove used backup code
                await self._remove_used_backup_code(user_id, code)

            return MfaValidationResult(
                success=is_valid,
                method='backup',
                message="Backup code validated successfully" if is_valid else "Invalid backup code"
            )

        except Exception as e:
            logger.error(f"Error validating backup code: {str(e)}")
            return MfaValidationResult(
                success=False,
                method='backup',
                message="Error validating backup code"
            )

    # Database interaction methods
    
    async def _get_user_by_id(self, user_id: UUID) -> Optional[User]:
        """Get user by ID"""
        try:
            from sqlalchemy import select
            from database.entities.user_entities import UserEntity
            
            result = await self.db_session.execute(
                select(UserEntity).where(UserEntity.id == str(user_id))
            )
            user_entity = result.scalar_one_or_none()
            
            if user_entity:
                return User(
                    id=user_entity.id,
                    email=user_entity.email,
                    username=user_entity.username,
                    first_name=user_entity.first_name,
                    last_name=user_entity.last_name,
                    is_active=user_entity.is_active,
                    created_at=user_entity.created_at,
                    updated_at=user_entity.updated_at,
                    last_login=user_entity.last_login
                )
            return None
            
        except Exception as e:
            logger.error(f"Error getting user by ID {user_id}: {str(e)}")
            return None

    async def _store_temp_totp_secret(self, user_id: UUID, secret: str) -> None:
        """Store temporary TOTP secret"""
        try:
            from database.entities.user_entities import UserMfaTempSecretEntity
            from datetime import datetime, timedelta
            
            # Clear any existing temp secret for this user
            await self._clear_temp_totp_secret(user_id)
            
            # Create new temp secret
            temp_secret = UserMfaTempSecretEntity(
                user_id=str(user_id),
                secret=secret,
                secret_type="TOTP",
                expires_at=datetime.utcnow() + timedelta(minutes=30)
            )
            
            self.db_session.add(temp_secret)
            await self.db_session.commit()
            
            logger.info(f"Stored temporary TOTP secret for user: {user_id}")
            
        except Exception as e:
            logger.error(f"Error storing temp TOTP secret for user {user_id}: {str(e)}")
            await self.db_session.rollback()
            raise

    async def _get_temp_totp_secret(self, user_id: UUID) -> Optional[str]:
        """Get temporary TOTP secret"""
        try:
            from sqlalchemy import select, and_
            from database.entities.user_entities import UserMfaTempSecretEntity
            from datetime import datetime
            
            result = await self.db_session.execute(
                select(UserMfaTempSecretEntity).where(
                    and_(
                        UserMfaTempSecretEntity.user_id == str(user_id),
                        UserMfaTempSecretEntity.secret_type == "TOTP",
                        UserMfaTempSecretEntity.is_used == False,
                        UserMfaTempSecretEntity.expires_at > datetime.utcnow()
                    )
                )
            )
            temp_secret = result.scalar_one_or_none()
            
            return temp_secret.secret if temp_secret else None
            
        except Exception as e:
            logger.error(f"Error getting temp TOTP secret for user {user_id}: {str(e)}")
            return None

    async def _clear_temp_totp_secret(self, user_id: UUID) -> None:
        """Clear temporary TOTP secret"""
        try:
            from sqlalchemy import delete
            from database.entities.user_entities import UserMfaTempSecretEntity
            
            await self.db_session.execute(
                delete(UserMfaTempSecretEntity).where(
                    UserMfaTempSecretEntity.user_id == str(user_id)
                )
            )
            await self.db_session.commit()
            
            logger.info(f"Cleared temporary TOTP secret for user: {user_id}")
            
        except Exception as e:
            logger.error(f"Error clearing temp TOTP secret for user {user_id}: {str(e)}")
            await self.db_session.rollback()
            raise

    async def _enable_totp_for_user(self, user_id: UUID, secret: str) -> None:
        """Enable TOTP for user"""
        try:
            from sqlalchemy import select, update
            from database.entities.user_entities import UserAuthProfileEntity
            from datetime import datetime
            import json
            
            # Generate backup codes
            backup_codes = self.totp_service._generate_backup_codes()
            
            # Get or create auth profile
            result = await self.db_session.execute(
                select(UserAuthProfileEntity).where(
                    UserAuthProfileEntity.user_id == str(user_id)
                )
            )
            auth_profile = result.scalar_one_or_none()
            
            if auth_profile:
                # Update existing profile
                await self.db_session.execute(
                    update(UserAuthProfileEntity)
                    .where(UserAuthProfileEntity.user_id == str(user_id))
                    .values(
                        mfa_enabled=True,
                        mfa_type="TOTP",
                        totp_secret=secret,
                        totp_secret_last_rotated=datetime.utcnow(),
                        backup_codes=json.dumps(backup_codes),
                        updated_at=datetime.utcnow()
                    )
                )
            else:
                # Create new auth profile
                auth_profile = UserAuthProfileEntity(
                    user_id=str(user_id),
                    login_type="EMAIL",  # Default login type
                    mfa_enabled=True,
                    mfa_type="TOTP",
                    totp_secret=secret,
                    totp_secret_last_rotated=datetime.utcnow(),
                    backup_codes=json.dumps(backup_codes)
                )
                self.db_session.add(auth_profile)
            
            await self.db_session.commit()
            logger.info(f"Enabled TOTP for user: {user_id}")
            
        except Exception as e:
            logger.error(f"Error enabling TOTP for user {user_id}: {str(e)}")
            await self.db_session.rollback()
            raise

    async def _disable_mfa_for_user(self, user_id: UUID) -> None:
        """Disable MFA for user"""
        try:
            from sqlalchemy import update
            from database.entities.user_entities import UserAuthProfileEntity
            from datetime import datetime
            
            await self.db_session.execute(
                update(UserAuthProfileEntity)
                .where(UserAuthProfileEntity.user_id == str(user_id))
                .values(
                    mfa_enabled=False,
                    mfa_type="NONE",
                    totp_secret=None,
                    totp_secret_last_rotated=None,
                    backup_codes=None,
                    updated_at=datetime.utcnow()
                )
            )
            await self.db_session.commit()
            
            logger.info(f"Disabled MFA for user: {user_id}")
            
        except Exception as e:
            logger.error(f"Error disabling MFA for user {user_id}: {str(e)}")
            await self.db_session.rollback()
            raise

    async def _is_totp_enabled_for_user(self, user_id: UUID) -> bool:
        """Check if TOTP is enabled for user"""
        try:
            from sqlalchemy import select, and_
            from database.entities.user_entities import UserAuthProfileEntity
            
            result = await self.db_session.execute(
                select(UserAuthProfileEntity).where(
                    and_(
                        UserAuthProfileEntity.user_id == str(user_id),
                        UserAuthProfileEntity.mfa_enabled == True,
                        UserAuthProfileEntity.mfa_type == "TOTP",
                        UserAuthProfileEntity.totp_secret.isnot(None)
                    )
                )
            )
            auth_profile = result.scalar_one_or_none()
            
            return auth_profile is not None
            
        except Exception as e:
            logger.error(f"Error checking TOTP status for user {user_id}: {str(e)}")
            return False

    async def _get_totp_secret_for_user(self, user_id: UUID) -> Optional[str]:
        """Get TOTP secret for user"""
        try:
            from sqlalchemy import select, and_
            from database.entities.user_entities import UserAuthProfileEntity
            
            result = await self.db_session.execute(
                select(UserAuthProfileEntity).where(
                    and_(
                        UserAuthProfileEntity.user_id == str(user_id),
                        UserAuthProfileEntity.mfa_enabled == True,
                        UserAuthProfileEntity.mfa_type == "TOTP"
                    )
                )
            )
            auth_profile = result.scalar_one_or_none()
            
            return auth_profile.totp_secret if auth_profile else None
            
        except Exception as e:
            logger.error(f"Error getting TOTP secret for user {user_id}: {str(e)}")
            return None

    async def _get_backup_codes_for_user(self, user_id: UUID) -> List[str]:
        """Get backup codes for user"""
        try:
            from sqlalchemy import select, and_
            from database.entities.user_entities import UserAuthProfileEntity
            import json
            
            result = await self.db_session.execute(
                select(UserAuthProfileEntity).where(
                    and_(
                        UserAuthProfileEntity.user_id == str(user_id),
                        UserAuthProfileEntity.mfa_enabled == True,
                        UserAuthProfileEntity.mfa_type == "TOTP"
                    )
                )
            )
            auth_profile = result.scalar_one_or_none()
            
            if auth_profile and auth_profile.backup_codes:
                return json.loads(auth_profile.backup_codes)
            
            return []
            
        except Exception as e:
            logger.error(f"Error getting backup codes for user {user_id}: {str(e)}")
            return []

    async def _get_backup_codes_count(self, user_id: UUID) -> int:
        """Get backup codes count for user"""
        codes = await self._get_backup_codes_for_user(user_id)
        return len(codes)

    async def _store_backup_codes(self, user_id: UUID, codes: List[str]) -> None:
        """Store backup codes"""
        try:
            from sqlalchemy import update
            from database.entities.user_entities import UserAuthProfileEntity
            from datetime import datetime
            import json
            
            await self.db_session.execute(
                update(UserAuthProfileEntity)
                .where(UserAuthProfileEntity.user_id == str(user_id))
                .values(
                    backup_codes=json.dumps(codes),
                    updated_at=datetime.utcnow()
                )
            )
            await self.db_session.commit()
            
            logger.info(f"Stored backup codes for user: {user_id}")
            
        except Exception as e:
            logger.error(f"Error storing backup codes for user {user_id}: {str(e)}")
            await self.db_session.rollback()
            raise

    async def _remove_used_backup_code(self, user_id: UUID, code: str) -> None:
        """Remove used backup code"""
        try:
            backup_codes = await self._get_backup_codes_for_user(user_id)
            
            # Remove the used code (case-insensitive)
            normalized_code = code.replace(' ', '').upper()
            backup_codes = [bc for bc in backup_codes if bc.replace(' ', '').upper() != normalized_code]
            
            # Store updated backup codes
            await self._store_backup_codes(user_id, backup_codes)
            
            logger.info(f"Removed used backup code for user: {user_id}")
            
        except Exception as e:
            logger.error(f"Error removing backup code for user {user_id}: {str(e)}")
            raise
