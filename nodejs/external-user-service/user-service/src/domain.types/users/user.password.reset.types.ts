import { uuid } from "../miscellaneous/system.types";

///////////////////////////////////////////////////////////////////////////////////////

//Please note that - there are 3 ways supported to reset the password
//1. By using the token
//2. By using the email-otp
//3. By using the phone-otp

///////////////////////////////////////////////////////////////////////////////////////

export interface UserPasswordResetSendPhoneOtpModel {
    PhoneCode  ?: string;  // Optional country code for phone
    PhoneNumber?: string;  // Optional phone number
    TenantId   ?: uuid;
}

export interface UserPasswordResetSendEmailOtpModel {
    Email?: string;  // Optional user email address
    TenantId?: uuid;
}

export interface UserPasswordResetSendEmailLinkModel {
    Email?: string;  // Optional user email address
    TenantId?: uuid;
}

// For storing the token in the database
export interface UserPasswordResetCreateTokenModel {
    TenantId ?: uuid;     // Optional tenant Id
    UserId   ?: string;   // Optional ID of the user the token is for
    Email    ?: string;   // Optional email of the user
    UserName ?: string;   // Optional username
    Token    ?: string;   // Optional token string
    ValidTill : Date;     // Expiry date of the token
}

export interface UserPasswordResetByTokenModel {
    UserId      : uuid;
    TenantId   ?: uuid;
    Token       : string;
    NewPassword : string;
}

export interface UserPasswordResetByOtpModel {
    UserId      : uuid;
    TenantId   ?: uuid;
    NewPassword : string;
    Otp         : string;
}
