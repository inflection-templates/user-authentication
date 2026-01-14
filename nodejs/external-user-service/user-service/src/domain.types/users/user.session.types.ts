import { uuid } from '../miscellaneous/system.types';
import { UserLoginMethod } from './user.enums';
import { UserDto } from './user.types';

///////////////////////////////////////////////////////////////////////////////////////

export interface UserSessionCreateModel {
    id       ?: uuid;
    UserId    : uuid;
    LoginMethod: UserLoginMethod;
    StartedAt?: Date;
    ValidTill?: Date;
    TenantId  : uuid;
}

export interface UserSessionDto {
    id        : uuid;
    UserId    : uuid;
    IsActive  : boolean;
    StartedAt?: Date;
    ValidTill?: Date;
    TenantId  : uuid;
}

export interface UserLoginPasswordUsernameParams {
    UserName: string;
    Password: string;
    TenantId: uuid;
    //LoginMethod: UserLoginMethod.UsernamePassword;
}

export interface UserLoginPasswordEmailParams {
    Email   : string;
    Password: string;
    TenantId: uuid;
    //LoginMethod: UserLoginMethod.EmailPassword;
}

export interface UserLoginPasswordPhoneParams {
    PhoneCode  : string;
    PhoneNumber: string;
    Password   : string;
    TenantId   : uuid;
    //LoginMethod: UserLoginMethod.PhonePassword;
}

export interface UserLoginOtpPhoneParams {
    PhoneCode  : string;
    PhoneNumber: string;
    Otp        : string;
    TenantId   : uuid;
    //LoginMethod: UserLoginMethod.PhoneOtp;
}

export interface UserLoginOtpEmailParams {
    Email   : string;
    Otp     : string;
    TenantId: uuid;
    //LoginMethod: UserLoginMethod.EmailOtp;
}

export interface UserLoginResult {
    SessionId     : uuid;
    UserId        : uuid;
    AccessToken   : string;
    RefreshToken  : string;
    ExpiresAt     : Date;
    LoginTimestamp: Date;
    LoginMethod   : UserLoginMethod;
    TenantId      : uuid;
    IsTestUser    : boolean;
    User         ?: UserDto;
}

export interface UserSessionLogoutParams {
    SessionId: uuid;
    UserId  ?: uuid;
}

export interface UserSessionRefreshParams {
    SessionId   : uuid;
    RefreshToken: string;
}

