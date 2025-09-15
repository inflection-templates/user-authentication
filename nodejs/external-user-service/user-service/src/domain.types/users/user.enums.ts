
export enum SupportedLanguage {
    English = 'en-US',
    Spanish = 'es-ES'
}

export enum OAuthProvider {
    Google   = 'google',
    Github   = 'github',
    Gitlab   = 'gitlab',
    LinkedIn = 'linkedin',
    Facebook = 'facebook',
    X        = 'twitter'
}

export const OAuthProviders = Object.values(OAuthProvider);

export enum OTPScope {
    Login          = 'Login',
    ChangePassword = 'Change Password',
    ResetPassword  = 'Reset Password',
    VerifyEmail    = 'Verify Email',
    VerifyPhone    = 'Verify Phone',
}

export const OTPScopes = Object.values(OTPScope);

export enum OTPChannel {
    SMS   = 'SMS',
    EMAIL = 'Email'
}

export const OTPChannels = Object.values(OTPChannel);

export enum UserLoginMethod {
    UsernamePassword = 'Username Password',
    EmailPassword    = 'Email Password',
    PhonePassword    = 'Phone Password',
    EmailOtp         = 'Email OTP',
    PhoneOtp         = 'Phone OTP',
    Oauth            = 'Oauth'
}

export const UserLoginMethods = Object.values(UserLoginMethod);

///////////////////////////////////////////////////////////////////////////////////////

export const SEVEN_DAYS = 7;
export const FIFTEEN_DAYS = 15;
export const THIRTY_DAYS = 30;
export const SIXTY_DAYS = 60;
export const NINETY_DAYS = 90;
export const ONE_YEAR = 365;

///////////////////////////////////////////////////////////////////////////////////////
