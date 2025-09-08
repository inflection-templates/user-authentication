"""
User domain types and models
Equivalent to the .NET shala.api.domain.types.users
"""

from datetime import datetime, date
from decimal import Decimal
from typing import List, Optional, Dict, Any
from enum import Enum
from pydantic import BaseModel, EmailStr, Field, validator
from uuid import UUID, uuid4


class GenderType(str, Enum):
    """Gender enumeration"""
    MALE = "Male"
    FEMALE = "Female"
    OTHER = "Other"
    PREFER_NOT_TO_SAY = "PreferNotToSay"


class UserLoginType(str, Enum):
    """User login type enumeration"""
    PASSWORD = "Password"
    OTP = "Otp"
    OAUTH = "OAuth"


class OAuthProvider(str, Enum):
    """OAuth provider enumeration"""
    GOOGLE = "Google"
    FACEBOOK = "Facebook"
    MICROSOFT = "Microsoft"
    TWITTER = "Twitter"
    GITHUB = "GitHub"
    LINKEDIN = "LinkedIn"
    GITLAB = "GitLab"


class UserStatus(str, Enum):
    """User status enumeration"""
    ACTIVE = "Active"
    INACTIVE = "Inactive"
    SUSPENDED = "Suspended"
    DELETED = "Deleted"


# User Models
class User(BaseModel):
    """User domain model"""
    id: UUID = Field(default_factory=uuid4)
    tenant_id: Optional[UUID] = None
    username: Optional[str] = None
    email: EmailStr
    country_code: Optional[str] = Field(None, alias="countryCode")
    phone_number: Optional[str] = Field(None, alias="phoneNumber")
    first_name: Optional[str] = Field(None, alias="firstName")
    last_name: Optional[str] = Field(None, alias="lastName")
    gender: Optional[GenderType] = None
    birth_date: Optional[date] = None
    profile_image_url: Optional[str] = None
    default_timezone: str = "UTC"
    current_timezone: Optional[str] = None
    is_active: bool = True
    status: UserStatus = UserStatus.ACTIVE
    created_at: datetime = Field(default_factory=datetime.utcnow)
    updated_at: datetime = Field(default_factory=datetime.utcnow)
    last_login: Optional[datetime] = None
    
    class Config:
        from_attributes = True
        allow_population_by_field_name = True
    
    def parse_phone_number(self, combined_phone: str):
        """Parse the combined phone number into country code and phone number"""
        if combined_phone and combined_phone.startswith('+'):
            # Extract country code (typically 2-4 digits after +)
            for i in range(2, 5):  # Check +XX, +XXX, +XXXX patterns
                if i < len(combined_phone):
                    potential_code = combined_phone[:i+1]
                    remaining = combined_phone[i+1:]
                    if len(remaining) >= 10:  # Minimum phone number length
                        self.country_code = potential_code
                        self.phone_number = remaining
                        break


class UserLoginSession(BaseModel):
    """User login session model"""
    id: UUID = Field(default_factory=uuid4)
    user_id: UUID
    session_id: UUID
    is_active: bool = True
    started_at: datetime = Field(default_factory=datetime.utcnow)
    last_activity: datetime = Field(default_factory=datetime.utcnow)
    ended_at: Optional[datetime] = None
    ip_address: Optional[str] = None
    user_agent: Optional[str] = None
    
    class Config:
        from_attributes = True


class UserAuthProfile(BaseModel):
    """User authentication profile model"""
    id: UUID = Field(default_factory=uuid4)
    user_id: UUID
    login_type: UserLoginType
    password_hash: Optional[str] = None
    password_salt: Optional[str] = None
    last_password_change: Optional[datetime] = None
    failed_login_attempts: int = 0
    locked_until: Optional[datetime] = None
    is_locked: bool = False
    created_at: datetime = Field(default_factory=datetime.utcnow)
    updated_at: datetime = Field(default_factory=datetime.utcnow)
    
    class Config:
        from_attributes = True


class UserOAuthProfile(BaseModel):
    """User OAuth profile model"""
    id: UUID = Field(default_factory=uuid4)
    user_id: UUID
    provider: OAuthProvider
    provider_user_id: str
    provider_username: Optional[str] = None
    access_token: Optional[str] = None
    refresh_token: Optional[str] = None
    token_expires_at: Optional[datetime] = None
    created_at: datetime = Field(default_factory=datetime.utcnow)
    updated_at: datetime = Field(default_factory=datetime.utcnow)
    
    class Config:
        from_attributes = True


# Request/Response Models
class UserPasswordLoginModel(BaseModel):
    """User password login request model"""
    email: EmailStr
    password: str = Field(..., min_length=1)
    remember_me: bool = False


class UserPhoneLoginModel(BaseModel):
    """User phone login request model"""
    country_code: str = Field(..., pattern=r"^\+\d{1,4}$", alias="countryCode")
    phone_number: str = Field(..., min_length=10, max_length=15, alias="phoneNumber")
    password: str = Field(..., min_length=1)
    remember_me: bool = False
    
    class Config:
        allow_population_by_field_name = True


class UserOtpLoginModel(BaseModel):
    """User OTP login request model"""
    email: EmailStr
    otp: str = Field(..., min_length=4, max_length=10)
    session_id: Optional[str] = None


class UserSendOtpModel(BaseModel):
    """User send OTP request model"""
    email: EmailStr
    purpose: str = "login"  # login, password_reset, verification


class UserResetPasswordSendLinkModel(BaseModel):
    """User reset password send link request model"""
    email: EmailStr


class UserResetPasswordModel(BaseModel):
    """User reset password request model"""
    email: EmailStr
    reset_token: str
    new_password: str = Field(..., min_length=8)


class UserChangePasswordModel(BaseModel):
    """User change password request model"""
    current_password: str
    new_password: str = Field(..., min_length=8)


class UserRefreshTokenModel(BaseModel):
    """User refresh token request model"""
    refresh_token: str


class UserRegistrationModel(BaseModel):
    """User registration request model"""
    email: EmailStr
    password: str = Field(..., min_length=8)
    first_name: Optional[str] = Field(None, alias="firstName")
    last_name: Optional[str] = Field(None, alias="lastName")
    phone: Optional[str] = None
    country_code: Optional[str] = Field(None, alias="countryCode")
    phone_number: Optional[str] = Field(None, alias="phoneNumber")
    username: Optional[str] = Field(None, alias="userName")
    gender: Optional[GenderType] = None
    birth_date: Optional[date] = None
    tenant_id: Optional[str] = Field(None, alias="TenantId")
    
    class Config:
        allow_population_by_field_name = True
        
    def get_full_phone(self) -> Optional[str]:
        """Get full phone number combining country code and phone number"""
        if self.country_code and self.phone_number:
            return f"{self.country_code}{self.phone_number}"
        return self.phone


class UserUpdateModel(BaseModel):
    """User update request model"""
    first_name: Optional[str] = None
    last_name: Optional[str] = None
    phone: Optional[str] = None
    username: Optional[str] = None
    gender: Optional[GenderType] = None
    birth_date: Optional[date] = None
    profile_image_url: Optional[str] = None
    default_timezone: Optional[str] = None


class LoginResponseModel(BaseModel):
    """Login response model"""
    data: dict
    
    @classmethod
    def create(cls, access_token: str, refresh_token: str, expires_in: int, user: User, session_id: str, token_type: str = "Bearer"):
        data_obj = {
            "token": access_token,
            "refreshToken": refresh_token,
            "tokenType": token_type,
            "expiresIn": expires_in,
            "user": user,
            "sessionId": session_id
        }
        return cls(data=data_obj)


class TokenResponseModel(BaseModel):
    """Token response model"""
    access_token: str
    refresh_token: Optional[str] = None
    token_type: str = "Bearer"
    expires_in: int


class UserResponseModel(BaseModel):
    """User response model"""
    id: UUID
    email: str
    username: Optional[str] = None
    first_name: Optional[str] = Field(None, alias="firstName")
    last_name: Optional[str] = Field(None, alias="lastName")
    country_code: Optional[str] = Field(None, alias="countryCode")
    phone_number: Optional[str] = Field(None, alias="phoneNumber")
    gender: Optional[GenderType] = None
    birth_date: Optional[date] = None
    profile_image_url: Optional[str] = None
    default_timezone: str
    is_active: bool
    status: UserStatus
    created_at: datetime
    updated_at: datetime
    last_login: Optional[datetime] = None
    
    class Config:
        from_attributes = True
        allow_population_by_field_name = True


class PaginatedResponse(BaseModel):
    """Paginated response model"""
    items: List[Any]
    total_count: int
    page: int
    page_size: int
    total_pages: int
    has_next: bool
    has_previous: bool


class UserRegistrationResponseModel(BaseModel):
    """User registration response model (without tokens)"""
    success: bool = True
    message: str = "User registered successfully"
    user: User
    
    class Config:
        from_attributes = True


class ApiResponse(BaseModel):
    """Generic API response model"""
    success: bool = True
    message: Optional[str] = None
    data: Optional[Any] = None
    errors: Optional[List[str]] = None
    timestamp: datetime = Field(default_factory=datetime.utcnow)
