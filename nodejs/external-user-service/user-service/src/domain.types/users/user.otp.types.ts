import { uuid } from '../miscellaneous/system.types';
import { OTPChannel, OTPScope } from './user.enums';

///////////////////////////////////////////////////////////////////////////////////////

export interface PhoneOtpCreateModel {
    PhoneCode  ?: string;
    PhoneNumber?: string;
    Scope       : OTPScope;
    TenantId    : uuid;
}

export interface EmailOtpCreateModel {
    Email      ?: string;
    Scope       : OTPScope;
    TenantId    : uuid;
}

export interface OtpModel {
    UserId   : uuid;
    Scope    : OTPScope;
    Channel  : OTPChannel;
    Otp      : string;
    ValidFrom: Date;
    ValidTill: Date;
}

export interface OtpDto {
    id       : uuid;
    UserId   : uuid;
    Scope    : OTPScope;
    Channel  : OTPChannel;
    Otp      : string;
    ValidFrom: Date;
    ValidTill: Date;
    Validated: boolean;
    CreatedAt: Date;
}
