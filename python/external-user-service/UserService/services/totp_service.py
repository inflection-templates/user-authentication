"""
TOTP (Time-based One-Time Password) Service
Provides TOTP generation, validation, and QR code creation functionality
"""

import base64
import io
import logging
import secrets
import time
from typing import Optional, Dict, Any, List
import qrcode
import pyotp
from PIL import Image

logger = logging.getLogger(__name__)


class TotpSecret:
    """TOTP secret configuration"""
    def __init__(self, secret: str, qr_code_url: str, qr_code_data_url: str, backup_codes: Optional[List[str]] = None):
        self.secret = secret
        self.qr_code_url = qr_code_url
        self.qr_code_data_url = qr_code_data_url
        self.backup_codes = backup_codes or []


class TotpValidationResult:
    """TOTP validation result"""
    def __init__(self, valid: bool, token: Optional[str] = None, window: Optional[int] = None):
        self.valid = valid
        self.token = token
        self.window = window


class TotpService:
    """TOTP service for generating and validating time-based OTP tokens"""

    def __init__(self, issuer_name: str = "User Service"):
        self.issuer_name = issuer_name

    def generate_secret(self, user_identifier: str, issuer: Optional[str] = None) -> TotpSecret:
        """
        Generate a new TOTP secret for a user
        
        Args:
            user_identifier: User email or username for QR code label
            issuer: Service name (optional, uses default if not provided)
            
        Returns:
            TotpSecret object with secret, QR code URL and data URL
        """
        try:
            service_name = issuer or self.issuer_name
            
            # Generate secret (32 bytes = 256 bits for good entropy)
            secret_key = pyotp.random_base32(length=32)
            
            # Create TOTP object
            totp = pyotp.TOTP(secret_key)
            
            # Generate provisioning URI
            provisioning_uri = totp.provisioning_uri(
                name=user_identifier,
                issuer_name=service_name
            )
            
            # Generate QR code
            qr_code_data_url = self._generate_qr_code(provisioning_uri)
            
            # Generate backup codes
            backup_codes = self._generate_backup_codes()
            
            logger.info(f"TOTP secret generated for user: {user_identifier}")
            
            return TotpSecret(
                secret=secret_key,
                qr_code_url=provisioning_uri,
                qr_code_data_url=qr_code_data_url,
                backup_codes=backup_codes
            )
            
        except Exception as e:
            logger.error(f"Error generating TOTP secret for {user_identifier}: {str(e)}")
            raise

    def verify_token(self, token: str, secret: str, window: int = 2) -> TotpValidationResult:
        """
        Verify TOTP token
        
        Args:
            token: 6-digit TOTP token from user
            secret: User's TOTP secret (base32 encoded)
            window: Time window for validation (default: 2 = ±60 seconds)
            
        Returns:
            TotpValidationResult
        """
        try:
            totp = pyotp.TOTP(secret)
            
            # Verify token with time window
            is_valid = totp.verify(token, valid_window=window)
            
            return TotpValidationResult(
                valid=is_valid,
                token=token if is_valid else None,
                window=window
            )
            
        except Exception as e:
            logger.error(f"Error verifying TOTP token: {str(e)}")
            return TotpValidationResult(valid=False, window=window)

    def generate_current_token(self, secret: str) -> str:
        """
        Generate current TOTP token for testing purposes
        
        Args:
            secret: TOTP secret
            
        Returns:
            Current 6-digit token
        """
        try:
            totp = pyotp.TOTP(secret)
            return totp.now()
        except Exception as e:
            logger.error(f"Error generating current TOTP token: {str(e)}")
            raise

    def verify_backup_code(self, code: str, backup_codes: List[str]) -> bool:
        """
        Verify backup code
        
        Args:
            code: Backup code provided by user
            backup_codes: List of valid backup codes
            
        Returns:
            Boolean indicating if code is valid
        """
        try:
            normalized_code = code.replace(' ', '').upper()
            normalized_backup_codes = [bc.replace(' ', '').upper() for bc in backup_codes]
            return normalized_code in normalized_backup_codes
        except Exception as e:
            logger.error(f"Error verifying backup code: {str(e)}")
            return False

    def get_remaining_time(self) -> int:
        """
        Get remaining time until next TOTP token
        
        Returns:
            Remaining seconds until next token
        """
        time_step = 30  # TOTP time step is 30 seconds
        current_time = int(time.time())
        return time_step - (current_time % time_step)

    def validate_setup(self, token: str, secret: str) -> bool:
        """
        Validate TOTP setup by requiring user to enter current token
        
        Args:
            token: Token entered by user
            secret: Generated secret
            
        Returns:
            Boolean indicating successful setup
        """
        result = self.verify_token(token, secret, window=1)  # Smaller window for setup
        return result.valid

    def _generate_qr_code(self, provisioning_uri: str) -> str:
        """
        Generate QR code as base64 data URL
        
        Args:
            provisioning_uri: TOTP provisioning URI
            
        Returns:
            Base64 encoded data URL for QR code image
        """
        try:
            # Create QR code
            qr = qrcode.QRCode(
                version=1,
                error_correction=qrcode.constants.ERROR_CORRECT_L,
                box_size=10,
                border=4,
            )
            qr.add_data(provisioning_uri)
            qr.make(fit=True)

            # Create image
            img = qr.make_image(fill_color="black", back_color="white")
            
            # Convert to base64 data URL
            buffer = io.BytesIO()
            img.save(buffer, format='PNG')
            buffer.seek(0)
            
            img_base64 = base64.b64encode(buffer.getvalue()).decode()
            return f"data:image/png;base64,{img_base64}"
            
        except Exception as e:
            logger.error(f"Error generating QR code: {str(e)}")
            raise

    def _generate_backup_codes(self, count: int = 10) -> List[str]:
        """
        Generate backup codes for account recovery
        
        Args:
            count: Number of backup codes to generate
            
        Returns:
            List of backup codes
        """
        codes = []
        
        for _ in range(count):
            # Generate 8-character alphanumeric code
            code = secrets.token_hex(4).upper()
            # Format as XXXX-XXXX for readability
            formatted_code = f"{code[:4]}-{code[4:]}"
            codes.append(formatted_code)
        
        return codes

    def get_qr_code_image(self, provisioning_uri: str) -> bytes:
        """
        Generate QR code as PNG image bytes
        
        Args:
            provisioning_uri: TOTP provisioning URI
            
        Returns:
            PNG image bytes
        """
        try:
            # Create QR code
            qr = qrcode.QRCode(
                version=1,
                error_correction=qrcode.constants.ERROR_CORRECT_L,
                box_size=10,
                border=4,
            )
            qr.add_data(provisioning_uri)
            qr.make(fit=True)

            # Create image
            img = qr.make_image(fill_color="black", back_color="white")
            
            # Convert to bytes
            buffer = io.BytesIO()
            img.save(buffer, format='PNG')
            buffer.seek(0)
            
            return buffer.getvalue()
            
        except Exception as e:
            logger.error(f"Error generating QR code image: {str(e)}")
            raise
