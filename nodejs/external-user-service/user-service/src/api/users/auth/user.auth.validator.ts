import express from 'express';
import Joi from 'joi';
import {
    UserLoginPasswordEmailParams,
    UserLoginPasswordPhoneParams,
    UserLoginPasswordUsernameParams,
    UserLoginOtpEmailParams,
    UserLoginOtpPhoneParams,
} from '../../../domain.types/users/user.session.types';
import { PhoneOtpCreateModel, EmailOtpCreateModel } from '../../../domain.types/users/user.otp.types';
import { UserPasswordChangeModel } from '../../../domain.types/users/user.password.change.types';
import {
    UserPasswordResetByTokenModel,
    UserPasswordResetSendEmailLinkModel,
    UserPasswordResetByOtpModel,
    UserPasswordResetSendPhoneOtpModel
} from '../../../domain.types/users/user.password.reset.types';
import { GitHubOAuthCallbackParams } from '../../../domain.types/users/github.oauth.types';
import { GoogleOAuthCallbackParams } from '../../../domain.types/users/google.oauth.types';
import { FacebookOAuthCallbackParams } from '../../../domain.types/users/facebook.oauth.types';
import { TwitterOAuthCallbackParams } from '../../../domain.types/users/twitter.oauth.types';
import { OTPScope } from '../../../domain.types/users/user.enums';
import { ErrorHandler } from '../../../common/handlers/error.handler';

///////////////////////////////////////////////////////////////////////////////////////

export class UserAuthValidator {

    static loginWithEmailPassword = async (request: express.Request)
        : Promise<UserLoginPasswordEmailParams> => {
        try {
            const schema = Joi.object({
                Email    : Joi.string().required().trim(),
                Password : Joi.string().required().trim(),
                TenantId : Joi.string().optional()
            });

            await schema.validateAsync(request.body);

            const loginDetails: UserLoginPasswordEmailParams = {
                Email    : request.body.Email,
                Password : request.body.Password,
                TenantId : request.body.TenantId ?? null
            };

            return loginDetails;
        } catch (error) {
            ErrorHandler.handleValidationError(error);
        }
    };

    static loginWithPhonePassword = async (request: express.Request)
        : Promise<UserLoginPasswordPhoneParams> => {
        try {
            const schema = Joi.object({
                PhoneCode   : Joi.string().required().trim(),
                PhoneNumber : Joi.string().required().trim(),
                Password    : Joi.string().required().trim(),
                TenantId    : Joi.string().optional()
            });

            await schema.validateAsync(request.body);

            const loginDetails: UserLoginPasswordPhoneParams = {
                PhoneCode   : request.body.PhoneCode,
                PhoneNumber : request.body.PhoneNumber,
                Password    : request.body.Password,
                TenantId    : request.body.TenantId ?? null
            };

            return loginDetails;
        } catch (error) {
            ErrorHandler.handleValidationError(error);
        }
    };

    static loginWithUsernamePassword = async (request: express.Request)
        : Promise<UserLoginPasswordUsernameParams> => {
        try {
            const schema = Joi.object({
                UserName : Joi.string().required().trim(),
                Password : Joi.string().required().trim(),
                TenantId : Joi.string().optional()
            });

            await schema.validateAsync(request.body);

            const loginDetails: UserLoginPasswordUsernameParams = {
                UserName : request.body.UserName,
                Password : request.body.Password,
                TenantId : request.body.TenantId ?? null
            };

            return loginDetails;
        } catch (error) {
            ErrorHandler.handleValidationError(error);
        }
    };

    static changePassword = async (request: express.Request)
        : Promise<UserPasswordChangeModel> => {
        try {
            const schema = Joi.object({
                OldPassword : Joi.string().required().trim(),
                NewPassword : Joi.string().required().trim(),
            });

            await schema.validateAsync(request.body);

            const obj: UserPasswordChangeModel = {
                OldPassword : request.body.OldPassword,
                NewPassword : request.body.NewPassword,
            };

            return obj;
        } catch (error) {
            ErrorHandler.handleValidationError(error);
        }
    };

    static resetPasswordByToken = async (request: express.Request)
        : Promise<UserPasswordResetByTokenModel> => {
        try {
            const schema = Joi.object({
                UserId      : Joi.string().optional(),
                Token       : Joi.string().required().trim(),
                NewPassword : Joi.string().required().trim(),
                TenantId    : Joi.string().optional()
            });

            await schema.validateAsync(request.body);

            const obj: UserPasswordResetByTokenModel = {
                UserId      : request.body.UserId ?? null,
                Token       : request.body.Token,
                NewPassword : request.body.NewPassword,
                TenantId    : request.body.TenantId ?? null
            };

            return obj;
        } catch (error) {
            ErrorHandler.handleValidationError(error);
        }
    };

    static resetPasswordByOtp = async (request: express.Request)
        : Promise<UserPasswordResetByOtpModel> => {
        try {
            const schema = Joi.object({
                UserId      : Joi.string().required(),
                Otp         : Joi.string().required().trim(),
                NewPassword : Joi.string().required().trim(),
                TenantId    : Joi.string().optional()
            });

            await schema.validateAsync(request.body);

            const obj: UserPasswordResetByOtpModel = {
                UserId      : request.body.UserId,
                Otp         : request.body.Otp,
                NewPassword : request.body.NewPassword,
                TenantId    : request.body.TenantId ?? null
            };

            return obj;
        } catch (error) {
            ErrorHandler.handleValidationError(error);
        }
    };

    static sendResetPasswordByEmailLink = async (request: express.Request)
        : Promise<UserPasswordResetSendEmailLinkModel> => {
        try {
            const schema = Joi.object({
                Email    : Joi.string().required().trim(),
                TenantId : Joi.string().optional()
            });

            await schema.validateAsync(request.body);

            const obj: UserPasswordResetSendEmailLinkModel = {
                Email    : request.body.Email,
                TenantId : request.body.TenantId ?? null
            };

            return obj;
        } catch (error) {
            ErrorHandler.handleValidationError(error);
        }
    };

    static sendResetPasswordByPhoneOtp = async (request: express.Request)
        : Promise<UserPasswordResetSendPhoneOtpModel> => {
        try {
            const schema = Joi.object({
                PhoneCode   : Joi.string().required().trim(),
                PhoneNumber : Joi.string().required().trim(),
                TenantId    : Joi.string().optional()
            });

            await schema.validateAsync(request.body);

            const obj: UserPasswordResetSendPhoneOtpModel = {
                PhoneCode   : request.body.PhoneCode,
                PhoneNumber : request.body.PhoneNumber,
                TenantId    : request.body.TenantId ?? null
            };

            return obj;
        } catch (error) {
            ErrorHandler.handleValidationError(error);
        }
    };

    static generateEmailOtp = async (request: express.Request)
        : Promise<EmailOtpCreateModel> => {
        try {
            const schema = Joi.object({
                Email    : Joi.string().optional().trim(),
                Scope    : Joi.string().optional().trim().valid(...Object.values(OTPScope)),
                TenantId : Joi.string().optional()
            });

            await schema.validateAsync(request.body);

            const obj: EmailOtpCreateModel = {
                Email    : request.body.Email ?? null,
                Scope    : request.body.Scope ?? OTPScope.Login,
                TenantId : request.body.TenantId ?? null
            };

            return obj;
        } catch (error) {
            ErrorHandler.handleValidationError(error);
        }
    };

    static generatePhoneOtp = async (request: express.Request)
        : Promise<PhoneOtpCreateModel> => {
        try {
            const schema = Joi.object({
                PhoneCode   : Joi.string().optional().trim(),
                PhoneNumber : Joi.string().optional().trim(),
                Scope       : Joi.string().optional().trim().valid(...Object.values(OTPScope)),
                TenantId    : Joi.string().optional()
            });

            await schema.validateAsync(request.body);

            const obj: PhoneOtpCreateModel = {
                PhoneCode   : request.body.PhoneCode ?? null,
                PhoneNumber : request.body.PhoneNumber ?? null,
                Scope       : request.body.Scope ?? OTPScope.Login,
                TenantId    : request.body.TenantId ?? null
            };

            return obj;
        } catch (error) {
            ErrorHandler.handleValidationError(error);
        }
    };

    static loginWithEmailOtp = async (request: express.Request)
        : Promise<UserLoginOtpEmailParams> => {
        try {
            const schema = Joi.object({
                Email    : Joi.string().optional().trim().email(),
                Otp      : Joi.string().required().trim().length(6).pattern(/^[0-9]+$/),
                TenantId : Joi.string().optional()
            });

            await schema.validateAsync(request.body);

            const obj: UserLoginOtpEmailParams = {
                Email    : request.body.Email ?? null,
                Otp      : request.body.Otp,
                TenantId : request.body.TenantId ?? null
            };

            return obj;
        } catch (error) {
            ErrorHandler.handleValidationError(error);
        }
    };

    static loginWithPhoneOtp = async (request: express.Request)
        : Promise<UserLoginOtpPhoneParams> => {
        try {
            const schema = Joi.object({
                PhoneCode   : Joi.string().optional().trim(),
                PhoneNumber : Joi.string().optional().trim(),
                Otp         : Joi.string().required().trim().length(6).pattern(/^[0-9]+$/),
                TenantId    : Joi.string().optional()
            });

            await schema.validateAsync(request.body);

            const obj: UserLoginOtpPhoneParams = {
                PhoneCode   : request.body.PhoneCode ?? null,
                PhoneNumber : request.body.PhoneNumber ?? null,
                Otp         : request.body.Otp,
                TenantId    : request.body.TenantId ?? null
            };

            return obj;
        } catch (error) {
            ErrorHandler.handleValidationError(error);
        }
    };

    static githubOAuthCallback = async (request: express.Request)
        : Promise<GitHubOAuthCallbackParams> => {
        try {
            const schema = Joi.object({
                code  : Joi.string().required(),
                state : Joi.string().optional()
            });

            await schema.validateAsync(request.query);

            const params: GitHubOAuthCallbackParams = {
                code  : request.query.code as string,
                state : request.query.state as string
            };

            return params;
        } catch (error) {
            ErrorHandler.handleValidationError(error);
        }
    };

    static googleOAuthCallback = async (request: express.Request)
        : Promise<GoogleOAuthCallbackParams> => {
        try {
            const schema = Joi.object({
                code     : Joi.string().required(),
                state    : Joi.string().optional(),
                scope    : Joi.string().optional(),
                authuser : Joi.string().optional(),  // Google sometimes includes this
                prompt   : Joi.string().optional(),  // Google sometimes includes this
                hd       : Joi.string().optional()   // Google Workspace domain
            });

            await schema.validateAsync(request.query);

            const params: GoogleOAuthCallbackParams = {
                code  : request.query.code as string,
                state : request.query.state as string,
                scope : request.query.scope as string
            };

            return params;
        } catch (error) {
            ErrorHandler.handleValidationError(error);
        }
    };

    static facebookOAuthCallback = async (request: express.Request)
        : Promise<FacebookOAuthCallbackParams> => {
        try {
            const schema = Joi.object({
                code  : Joi.string().required(),
                state : Joi.string().optional()
            });

            await schema.validateAsync(request.query);

            const params: FacebookOAuthCallbackParams = {
                code  : request.query.code as string,
                state : request.query.state as string
            };

            return params;
        } catch (error) {
            ErrorHandler.handleValidationError(error);
        }
    };

    static twitterOAuthCallback = async (request: express.Request)
        : Promise<TwitterOAuthCallbackParams> => {
        try {
            const schema = Joi.object({
                code  : Joi.string().required(),
                state : Joi.string().optional()
            });

            await schema.validateAsync(request.query);

            const params: TwitterOAuthCallbackParams = {
                code  : request.query.code as string,
                state : request.query.state as string
            };

            return params;
        } catch (error) {
            ErrorHandler.handleValidationError(error);
        }
    };

}
