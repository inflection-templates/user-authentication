import { IUserSessionRepo } from '../../database/repository.interfaces/users/user.session.repo.interface';
import { inject, injectable } from 'tsyringe';
import { ApiError } from '../../common/api.error';
import { Helper } from '../../common/helper';
import { logger } from '../../logger/logger';
import { TimeHelper } from '../../common/time.helper';
import { ConfigurationManager } from '../../config/configuration.manager';
import { IUserOtpRepo } from '../../database/repository.interfaces/users/user.otp.repo.interface';
import { IUserRepo } from '../../database/repository.interfaces/users/user.repo.interface';
import { CurrentUser } from '../../domain.types/miscellaneous/current.user';
import { OtpModel } from '../../domain.types/users/user.otp.types';
import {
    UserLoginPasswordUsernameParams,
    UserLoginPasswordEmailParams,
    UserLoginPasswordPhoneParams,
    UserLoginOtpPhoneParams,
    UserLoginOtpEmailParams,
    UserLoginResult,
    UserSessionCreateModel,
} from '../../domain.types/users/user.session.types';
import { EmailOtpCreateModel, PhoneOtpCreateModel } from '../../domain.types/users/user.otp.types';
import {
    UserPasswordResetSendEmailLinkModel,
    UserPasswordResetSendPhoneOtpModel,
    UserPasswordResetSendEmailOtpModel,
    UserPasswordResetByTokenModel,
    UserPasswordResetByOtpModel,
} from '../../domain.types/users/user.password.reset.types';
import { UserDto } from '../../domain.types/users/user.types';
import { DurationType } from '../../domain.types/miscellaneous/time.types';
import { uuid } from '../../domain.types/miscellaneous/system.types';
import { ITenantRepo } from '../../database/repository.interfaces/tenant/tenant.repo.interface';
import { TenantDto } from "../../domain.types/tenant/tenant.types";
import { Loader } from '../../startup/loader';
import { UserAuthHandler } from '../../auth/user.auth/user.auth.handler';
import { EmailService } from '../../modules/communication/email/email.service';
import { EmailDetails } from '../../modules/communication/email/email.details';
import { IUserMetadataRepo } from '../../database/repository.interfaces/users/user.metadata.repo.interface';
import { IUserPasswordRepo } from '../../database/repository.interfaces/users/user.password.repo.interface';
import { OTPScope, UserLoginMethod, OTPChannel } from '../../domain.types/users/user.enums';
import { UserAccountActionResult } from '../../domain.types/users/user.types';

////////////////////////////////////////////////////////////////////////////////////////////////////////

@injectable()
export class UserAuthService {

    constructor(
        @inject('IUserRepo') private _userRepo: IUserRepo,
        @inject('IUserOtpRepo') private _otpRepo: IUserOtpRepo,
        @inject('IUserPasswordRepo') private _userPasswordRepo: IUserPasswordRepo,
        @inject('IUserSessionRepo') private _userSessionRepo: IUserSessionRepo,
        @inject('IUserMetadataRepo') private _userMetadataRepo: IUserMetadataRepo,
        @inject('ITenantRepo') private _tenantRepo: ITenantRepo,
    ) { }


    public setPassword = async (userId: uuid, password: string) => {
        const hashedPassword = Helper.hash(password);
        await this._userPasswordRepo.updateCurrentUserHashedPassword(userId, hashedPassword);
    };

    public loginWithEmailPassword = async (model: UserLoginPasswordEmailParams): Promise<UserLoginResult> => {
        let tenant;
        let tenantId = model.TenantId;
        
        // If no tenant provided, use default tenant (like Python service)
        if (!tenantId) {
            tenant = await this._tenantRepo.getTenantWithCode('default');
            
            // If no tenant with code 'default', use the first available tenant
            if (!tenant) {
                const allTenants = await this._tenantRepo.search({ 
                    PageIndex: 0, 
                    ItemsPerPage: 1 
                });
                if (allTenants && allTenants.Items && allTenants.Items.length > 0) {
                    tenant = allTenants.Items[0];
                }
            }
            
            if (!tenant) {
                throw new ApiError(404, 'No tenant found for authentication.');
            }
            tenantId = tenant.id;
        } else {
            tenant = await this._tenantRepo.getById(tenantId);
            if (!tenant) {
                throw new ApiError(404, 'Tenant not found.');
            }
        }
        
        const user = await this._userRepo.getByEmail(tenantId, model.Email);
        if (!user) {
            throw new ApiError(404, 'User not found.');
        }
        return await this.passwordLogin(user, tenant, model.Password);
    };

    public loginWithPhonePassword = async (model: UserLoginPasswordPhoneParams): Promise<UserLoginResult> => {
        let tenant;
        let tenantId = model.TenantId;
        
        // If no tenant provided, use default tenant (like Python service)
        if (!tenantId) {
            tenant = await this._tenantRepo.getTenantWithCode('default');
            
            // If no tenant with code 'default', use the first available tenant
            if (!tenant) {
                const allTenants = await this._tenantRepo.search({ 
                    PageIndex: 0, 
                    ItemsPerPage: 1 
                });
                if (allTenants && allTenants.Items && allTenants.Items.length > 0) {
                    tenant = allTenants.Items[0];
                }
            }
            
            if (!tenant) {
                throw new ApiError(404, 'No tenant found for authentication.');
            }
            tenantId = tenant.id;
        } else {
            tenant = await this._tenantRepo.getById(tenantId);
            if (!tenant) {
                throw new ApiError(404, 'Tenant not found.');
            }
        }
        
        const user = await this._userRepo.getByPhone(tenantId, model.PhoneCode, model.PhoneNumber);
        if (!user) {
            throw new ApiError(404, 'User not found.');
        }
        return await this.passwordLogin(user, tenant, model.Password);
    };

    public loginWithUsernamePassword = async (model: UserLoginPasswordUsernameParams): Promise<UserLoginResult> => {
        let tenant;
        let tenantId = model.TenantId;
        
        // If no tenant provided, use default tenant (like Python service)
        if (!tenantId) {
            tenant = await this._tenantRepo.getTenantWithCode('default');
            
            // If no tenant with code 'default', use the first available tenant
            if (!tenant) {
                const allTenants = await this._tenantRepo.search({ 
                    PageIndex: 0, 
                    ItemsPerPage: 1 
                });
                if (allTenants && allTenants.Items && allTenants.Items.length > 0) {
                    tenant = allTenants.Items[0];
                }
            }
            
            if (!tenant) {
                throw new ApiError(404, 'No tenant found for authentication.');
            }
            tenantId = tenant.id;
        } else {
            tenant = await this._tenantRepo.getById(tenantId);
            if (!tenant) {
                throw new ApiError(404, 'Tenant not found.');
            }
        }
        
        const user = await this._userRepo.getByUserName(model.UserName);
        if (!user) {
            throw new ApiError(404, 'User not found.');
        }
        return await this.passwordLogin(user, tenant, model.Password);
    };

    public loginWithPhoneOtp = async (model: UserLoginOtpPhoneParams): Promise<UserLoginResult> => {
        let tenant;
        let tenantId = model.TenantId;
        
        // If no tenant provided, use default tenant (like Python service)
        if (!tenantId) {
            tenant = await this._tenantRepo.getTenantWithCode('default');
            
            // If no tenant with code 'default', use the first available tenant
            if (!tenant) {
                const allTenants = await this._tenantRepo.search({ 
                    PageIndex: 0, 
                    ItemsPerPage: 1 
                });
                if (allTenants && allTenants.Items && allTenants.Items.length > 0) {
                    tenant = allTenants.Items[0];
                }
            }
            
            if (!tenant) {
                throw new ApiError(404, 'No tenant found for authentication.');
            }
            tenantId = tenant.id;
        } else {
            tenant = await this._tenantRepo.getById(tenantId);
            if (!tenant) {
                throw new ApiError(404, 'Tenant not found.');
            }
        }
        
        const user = await this._userRepo.getByPhone(tenantId, model.PhoneCode, model.PhoneNumber);
        if (!user) {
            throw new ApiError(404, 'User not found.');
        }
        return await this.otpLogin(user, tenant, model.Otp);
    };

    public loginWithEmailOtp = async (model: UserLoginOtpEmailParams): Promise<UserLoginResult> => {
        let tenant;
        let tenantId = model.TenantId;
        
        // If no tenant provided, use default tenant (like Python service)
        if (!tenantId) {
            tenant = await this._tenantRepo.getTenantWithCode('default');
            
            // If no tenant with code 'default', use the first available tenant
            if (!tenant) {
                const allTenants = await this._tenantRepo.search({ 
                    PageIndex: 0, 
                    ItemsPerPage: 1 
                });
                if (allTenants && allTenants.Items && allTenants.Items.length > 0) {
                    tenant = allTenants.Items[0];
                }
            }
            
            if (!tenant) {
                throw new ApiError(404, 'No tenant found for authentication.');
            }
            tenantId = tenant.id;
        } else {
            tenant = await this._tenantRepo.getById(tenantId);
            if (!tenant) {
                throw new ApiError(404, 'Tenant not found.');
            }
        }
        
        const user = await this._userRepo.getByEmail(tenantId, model.Email);
        if (!user) {
            throw new ApiError(404, 'User not found.');
        }
        return await this.otpLogin(user, tenant, model.Otp);
    };

    public loginWithOAuth = async (userId: uuid): Promise<UserLoginResult> => {
        const user = await this._userRepo.getById(userId);
        if (!user) {
            throw new ApiError(404, 'User not found.');
        }

        let tenant;
        let tenantId = user.Tenant?.id;
        
        // If no tenant provided, use default tenant
        if (!tenantId) {
            tenant = await this._tenantRepo.getTenantWithCode('default');
            
            // If no tenant with code 'default', use the first available tenant
            if (!tenant) {
                const allTenants = await this._tenantRepo.search({ 
                    PageIndex: 0, 
                    ItemsPerPage: 1 
                });
                if (allTenants && allTenants.Items && allTenants.Items.length > 0) {
                    tenant = allTenants.Items[0];
                }
            }
            
            if (!tenant) {
                throw new ApiError(404, 'No tenant found for authentication.');
            }
            tenantId = tenant.id;
        } else {
            tenant = await this._tenantRepo.getById(tenantId);
            if (!tenant) {
                throw new ApiError(404, 'Tenant not found.');
            }
        }

        return await this.oauthLogin(user, tenant);
    };

    public generateEmailOtp = async (model: EmailOtpCreateModel): Promise<UserAccountActionResult> => {
        let tenant;
        let tenantId = model.TenantId;
        
        // If no tenant provided, use default tenant (like Python service)
        if (!tenantId) {
            tenant = await this._tenantRepo.getTenantWithCode('default');
            
            // If no tenant with code 'default', use the first available tenant
            if (!tenant) {
                const allTenants = await this._tenantRepo.search({ 
                    PageIndex: 0, 
                    ItemsPerPage: 1 
                });
                if (allTenants && allTenants.Items && allTenants.Items.length > 0) {
                    tenant = allTenants.Items[0];
                }
            }
            
            if (!tenant) {
                throw new ApiError(404, 'No tenant found for authentication.');
            }
            tenantId = tenant.id;
        } else {
            tenant = await this._tenantRepo.getById(tenantId);
            if (!tenant) {
                throw new ApiError(404, 'Tenant not found.');
            }
        }
        
        const user = await this._userRepo.getByEmail(tenantId, model.Email);
        if (!user) {
            throw new ApiError(404, 'User not found.');
        }
        const otp = await this.createOtp(user.id, OTPScope.Login, OTPChannel.EMAIL);

        var userFirstName = 'user';
        if (user && user.FirstName) {
            userFirstName = user.FirstName;
        }

        const sentEmail = await this.sendLoginOtpEmail(user.Email, otp, userFirstName);
        if (sentEmail) {
            logger.info(`Login OTP sent successfully to user ${user.Email}.`);
        }

        return { Success: sentEmail, User: user };
    };

    public generatePhoneOtp = async (model: PhoneOtpCreateModel): Promise<UserAccountActionResult> => {
        let tenant;
        let tenantId = model.TenantId;
        
        // If no tenant provided, use default tenant (like Python service)
        if (!tenantId) {
            tenant = await this._tenantRepo.getTenantWithCode('default');
            
            // If no tenant with code 'default', use the first available tenant
            if (!tenant) {
                const allTenants = await this._tenantRepo.search({ 
                    PageIndex: 0, 
                    ItemsPerPage: 1 
                });
                if (allTenants && allTenants.Items && allTenants.Items.length > 0) {
                    tenant = allTenants.Items[0];
                }
            }
            
            if (!tenant) {
                throw new ApiError(404, 'No tenant found for authentication.');
            }
            tenantId = tenant.id;
        } else {
            tenant = await this._tenantRepo.getById(tenantId);
            if (!tenant) {
                throw new ApiError(404, 'Tenant not found.');
            }
        }
        
        const user = await this._userRepo.getByPhone(tenantId, model.PhoneCode, model.PhoneNumber);
        if (!user) {
            throw new ApiError(404, 'User not found.');
        }
        const otp = await this.createOtp(user.id, OTPScope.Login, OTPChannel.SMS);

        var userFirstName = 'user';
        if (user && user.FirstName) {
            userFirstName = user.FirstName;
        }

        const systemIdentifier = ConfigurationManager.SystemIdentifier;
        const otpValidityInMinutes = ConfigurationManager.OtpValidityInMinutes;

        const message = `Dear ${userFirstName}, ${otp} is OTP for your ${systemIdentifier} account and will expire in ${otpValidityInMinutes} minutes.`;
        const sentSms = await Loader.messagingService.sendSMS(user.PhoneNumber, message);
        if (sentSms) {
            logger.info(`Otp sent successfully. OTP: ${otp}\n `);
        }

        return { Success: sentSms, User: user };
    };

    public changePassword = async (
        userId: uuid,
        oldPassword: string,
        newPassword: string)
        : Promise<UserAccountActionResult> => {

        const user = await this._userRepo.getById(userId);
        if (user == null) {
            throw new ApiError(404, 'User not found.');
        }

        const hashedPassword = await this._userPasswordRepo.getUserCurrentHashedPassword(user.id);
        const isPasswordValid = Helper.compare(oldPassword, hashedPassword.PasswordHash);
        if (!isPasswordValid) {
            const message = 'Invalid old password! Please provide correct old password. If you have forgotten your password, please use the "Forgot Password" feature.';
            throw new ApiError(401, message);
        }

        const newPasswordHash = Helper.hash(newPassword);
        await this._userPasswordRepo.updateCurrentUserHashedPassword(user.id, newPasswordHash);

        return { Success: true, User: user };
    };

    // This generates a reset token and sends a link to the user's email
    public sendResetPasswordByEmailLink = async (
        model: UserPasswordResetSendEmailLinkModel): Promise<UserAccountActionResult> => {

        const user = await this._userRepo.getByEmail(model.TenantId, model.Email);
        if (!user) {
            throw new ApiError(404, 'User not found.');
        }

        const frontendUrl = process.env.FRONTEND_URL;
        if (!frontendUrl) {
            throw new ApiError(500, 'Frontend URL not found.');
        }

        const resetToken = await UserAuthHandler.generatePasswordResetToken(user.id, user.Email, model.TenantId);
        const resetLink = `${frontendUrl}/reset-password?token=${resetToken}`;

        const sentEmail = await this.sendPasswordResetLinkEmail(user.Email, resetLink, user.FirstName);
        if (sentEmail) {
            logger.info(`Password reset link sent successfully to user ${user.Email}.`);
        }

        return { Success: sentEmail, User: user };
    };

    public sendResetPasswordByEmailOtp = async (
        model: UserPasswordResetSendEmailOtpModel): Promise<UserAccountActionResult> => {

        const user = await this._userRepo.getByEmail(model.TenantId, model.Email);
        if (!user) {
            throw new ApiError(404, 'User not found.');
        }

        const otp = await this.createOtp(user.id, OTPScope.ResetPassword, OTPChannel.EMAIL);
        const sentEmail = await this.sendPasswordResetOtpEmail(user.Email, otp, user.FirstName);
        if (sentEmail) {
            logger.info(`Password reset code sent successfully to user ${user.Email}.`);
        }

        return { Success: sentEmail, User: user };
    };

    public sendResetPasswordByPhoneOtp = async (model: UserPasswordResetSendPhoneOtpModel)
        : Promise<UserAccountActionResult> => {
        const user = await this._userRepo.getByPhone(model.TenantId, model.PhoneCode, model.PhoneNumber);
        if (!user) {
            throw new ApiError(404, 'User not found.');
        }

        const otp = await this.createOtp(user.id, OTPScope.ResetPassword, OTPChannel.SMS);
        var userFirstName = 'user';
        if (user && user.FirstName) {
            userFirstName = user.FirstName;
        }

        const systemIdentifier = ConfigurationManager.SystemIdentifier;
        const otpValidityInMinutes = ConfigurationManager.OtpValidityInMinutes;

        const phone = user.PhoneCode + user.PhoneNumber;

        const message = `Dear ${userFirstName}, ${otp} is OTP for ${systemIdentifier} account for password reset and will expire in ${otpValidityInMinutes} minutes.`;
        const sentSms = await Loader.messagingService.sendSMS(phone, message);
        if (sentSms) {
            logger.info(`Otp sent successfully. OTP: ${otp}\n `);
        }

        return { Success: sentSms, User: user };
    };

    public resetPasswordByToken = async (model: UserPasswordResetByTokenModel): Promise<UserAccountActionResult> => {
        const user = await this._userRepo.getById(model.UserId);
        if (!user) {
            throw new ApiError(404, 'User not found.');
        }
        // Verify the token
        const result = await UserAuthHandler.verifyPasswordResetToken(model.Token);
        if (!result) {
            throw new ApiError(401, 'Invalid token.');
        }
        if (result.UserId !== user.id) {
            throw new ApiError(401, 'Token is not valid for this user.');
        }

        // Update the password
        const newPasswordHash = Helper.hash(model.NewPassword);
        await this._userPasswordRepo.updateCurrentUserHashedPassword(user.id, newPasswordHash);

        return { Success: true, User: user };
    };

    public resetPasswordByOtp = async (model: UserPasswordResetByOtpModel): Promise<UserAccountActionResult> => {
        const user = await this._userRepo.getById(model.UserId);
        if (!user) {
            throw new ApiError(404, 'User not found.');
        }

        const otp = await this._otpRepo.getActiveOtp(user.id, model.Otp, OTPScope.ResetPassword);
        if (!otp) {
            throw new ApiError(404, 'Active OTP record not found!');
        }

        if (otp.ValidTill <= new Date()) {
            throw new ApiError(400, 'Reset password OTP has expired. Please regenerate OTP again!');
        }

        const newPasswordHash = Helper.hash(model.NewPassword);
        await this._userPasswordRepo.updateCurrentUserHashedPassword(user.id, newPasswordHash);

        return { Success: true, User: user };
    };

    public invalidateSession = async (sesssionId: uuid): Promise<boolean> => {
        var invalidated = await this._userSessionRepo.invalidate(sesssionId);
        return invalidated;
    };

    public invalidateAllSessions = async (userId: uuid): Promise<boolean> => {
        var invalidated = await this._userSessionRepo.invalidateAll(userId);
        return invalidated;
    };

    public rotateUserAccessToken = async (refreshToken: string): Promise<string> => {
        return await UserAuthHandler.rotateUserSessionToken(refreshToken);
    };

    public isValidUserLoginSession = async (sessionId: uuid): Promise<boolean> => {
        const isValid = await this._userSessionRepo.isValid(sessionId);
        return isValid;
    };

    public logout = async (sessionId: uuid): Promise<boolean> => {
        return await this._userSessionRepo.invalidate(sessionId);
    };

    public generateAndSendPasswordByEmail = async (userId: uuid) => {
        const user = await this._userRepo.getById(userId);
        if (!user) {
            throw new ApiError(404, 'User not found.');
        }

        const password = Helper.generatePassword();
        await this.setPassword(userId, password);

        const emailService = new EmailService();
        var body = await emailService.getTemplate('password.send.template.html');

        const name = user.FirstName || 'user';

        body = body.replace(/{{PLATFORM_NAME}}/g, process.env.PLATFORM_NAME);
        body = body.replace('{{USER_FIRST_NAME}}', name);
        body = body.replace('{{PASSWORD}}', password);
        const emailDetails: EmailDetails = {
            EmailTo : user.Email,
            Subject : `Password for your account`,
            Body    : body,
        };
        logger.info(`Email details: ${JSON.stringify(emailDetails)}`);

        const sent = await emailService.sendEmail(emailDetails, false);
        if (!sent) {
            logger.info(`Unable to send email to ${user.Email}`);
            return false;
        }
        return true;
    };

    public generateAndSendPasswordBySms = async (userId: uuid) => {
        const user = await this._userRepo.getById(userId);
        if (!user) {
            throw new ApiError(404, 'User not found.');
        }

        const password = Helper.generatePassword();
        await this.setPassword(userId, password);

        const name = user.FirstName || 'user';
        const phone = user.PhoneCode + user.PhoneNumber;

        const message = `Dear ${name}, ${password} is your password for your account. Please change your password after login.`;
        const sentSms = await Loader.messagingService.sendSMS(phone, message);
        if (sentSms) {
            logger.info(`Password sent successfully to ${phone}`);
        }
        return sentSms;
    };



    private async passwordLogin(user: UserDto, tenant: TenantDto, password: string) {

        const isTestUser = await this._userMetadataRepo.isTestUser(user.id);

        if (!isTestUser) {
            const hashedPasswordRecord = await this._userPasswordRepo.getUserCurrentHashedPassword(user.id);
            if (!hashedPasswordRecord) {
                throw new ApiError(401, 'Your password has expired. Please reset your password.');
            }
            const isPasswordValid = Helper.compare(password, hashedPasswordRecord.PasswordHash);
            if (!isPasswordValid) {
                throw new ApiError(401, 'Invalid password!');
            }
        }
        return await this.processSuccessfullLogin(user, tenant, isTestUser);
    }

    private async otpLogin(user: UserDto, tenant: TenantDto, otp: string) {

        const isTestUser = await this._userMetadataRepo.isTestUser(user.id);
        if (!isTestUser) {
            const storedOtp = await this._otpRepo.getActiveOtp(user.id, otp, OTPScope.Login);
            if (!storedOtp) {
                throw new ApiError(404, 'Active OTP record not found!');
            }
            if (storedOtp.ValidTill <= new Date()) {
                throw new ApiError(400, 'Login OTP has expired. Please regenerate OTP again!');
            }
        }
        return await this.processSuccessfullLogin(user, tenant, isTestUser);
    }

    private async oauthLogin(user: UserDto, tenant: TenantDto) {
        const isTestUser = await this._userMetadataRepo.isTestUser(user.id);
        return await this.processSuccessfullLogin(user, tenant, isTestUser, UserLoginMethod.Oauth);
    }

    private async processSuccessfullLogin(user: UserDto, tenant: TenantDto, isTestUser: boolean, loginMethod?: UserLoginMethod) {

        await this._userMetadataRepo.updateLastLogin(user.id, new Date());

        //Generate login session
        const expiresIn: number = ConfigurationManager.AccessTokenExpiresInSeconds;
        var expiresAt = TimeHelper.addDuration(new Date(), expiresIn, DurationType.Second);

        var entity: UserSessionCreateModel = {
            UserId      : user.id,
            LoginMethod : loginMethod || UserLoginMethod.EmailPassword,
            StartedAt   : new Date(),
            ValidTill   : expiresAt,
            TenantId    : tenant.id,
        };

        const session = await this._userSessionRepo.create(entity);

        //The following user data is immutable. Don't include any mutable data
        user = await this._userRepo.getById(user.id);

        var currentUser: CurrentUser = {
            UserId      : user.id,
            TenantId    : tenant.id,
            TenantCode  : tenant.TenantCode,
            TenantName  : tenant.Name,
            DisplayName : user.Metadata?.DisplayName,
            PhoneCode   : user.PhoneCode,
            PhoneNumber : user.PhoneNumber,
            Email       : user.Email,
            UserName    : user.UserName,
            Roles       : [],
            SessionId   : session.id,
            IsTestUser  : isTestUser,
        };

        const sessionId = currentUser.SessionId;
        const accessToken = await UserAuthHandler.generateUserSessionToken(currentUser);
        var refreshToken = null;
        if (ConfigurationManager.UseRefreshToken) {
            refreshToken = await UserAuthHandler.generateRefreshToken(currentUser);
        }

        const result: UserLoginResult = {
            SessionId      : sessionId,
            UserId         : user.id,
            AccessToken    : accessToken,
            RefreshToken   : refreshToken ?? null,
            ExpiresAt      : expiresAt,
            LoginTimestamp : new Date(),
            LoginMethod    : UserLoginMethod.EmailPassword,
            TenantId       : tenant.id,
            IsTestUser     : isTestUser,
            User           : user,
        };
        return result;
    }

    private async createOtp(userId: uuid, scope: OTPScope, channel: OTPChannel) {

        const otpValidityInMinutes = ConfigurationManager.OtpValidityInMinutes;
        const otp = (Math.floor(Math.random() * 900000) + 100000).toString();
        const currMillsecs = Date.now();
        const validTill = new Date(currMillsecs + (otpValidityInMinutes * 60 * 1000));

        const otpEntity: OtpModel = {
            Scope     : scope,
            UserId    : userId,
            Otp       : otp,
            Channel   : channel,
            ValidFrom : new Date(),
            ValidTill : validTill,
        };

        const otpDto = await this._otpRepo.create(otpEntity);
        logger.info(`OTP : ${JSON.stringify(otpDto, null, 2)}`);

        return otp;
    }

    private sendPasswordResetLinkEmail = async (email: string, resetLink: string, userFirstName: string) => {
        try {
            const emailService = new EmailService();
            var body = await emailService.getTemplate('password.reset.link.template.html');

            body = body.replace(/{{PLATFORM_NAME}}/g, process.env.PLATFORM_NAME);
            body = body.replace('{{USER_FIRST_NAME}}', userFirstName);
            body = body.replace('{{RESET_LINK}}', resetLink);
            const emailDetails: EmailDetails = {
                EmailTo : email,
                Subject : `Reset Password`,
                Body    : body,
            };
            logger.info(`Email details: ${JSON.stringify(emailDetails)}`);

            const sent = await emailService.sendEmail(emailDetails, false);
            if (!sent) {
                logger.info(`Unable to send email to ${email}`);
                return false;
            }
            return true;
        }
        catch (error) {
            logger.info(`Unable to send email to ${email}`);
            return false;
        }
    };

    private sendPasswordResetOtpEmail = async (email: string, otp: string, userFirstName: string) => {
        try {
            const emailService = new EmailService();
            var body = await emailService.getTemplate('password.reset.otp.template.html');

            body = body.replace(/{{PLATFORM_NAME}}/g, process.env.PLATFORM_NAME);
            body = body.replace('{{USER_FIRST_NAME}}', userFirstName);
            body = body.replace('{{OTP}}', otp);
            const otpValidityInMinutes = ConfigurationManager.OtpValidityInMinutes;
            body = body.replace('{{OTP_VALIDITY_IN_MINUTES}}', otpValidityInMinutes.toString());
            const emailDetails: EmailDetails = {
                EmailTo : email,
                Subject : `Reset Password`,
                Body    : body,
            };
            logger.info(`Email details: ${JSON.stringify(emailDetails)}`);

            const sent = await emailService.sendEmail(emailDetails, false);
            if (!sent) {
                logger.info(`Unable to send email to ${email}`);
                return false;
            }
            return true;
        }
        catch (error) {
            logger.info(`Unable to send email to ${email}`);
            return false;
        }
    };

    private sendLoginOtpEmail = async (email: string, otp: string, userFirstName: string) => {
        try {
            const emailService = new EmailService();
            var body = await emailService.getTemplate('login.otp.template.html');

            const otpValidityInMinutes = ConfigurationManager.OtpValidityInMinutes;

            body = body.replace(/{{PLATFORM_NAME}}/g, process.env.PLATFORM_NAME);
            body = body.replace('{{USER_FIRST_NAME}}', userFirstName);
            body = body.replace('{{OTP}}', otp);
            body = body.replace('{{OTP_VALIDITY_IN_MINUTES}}', otpValidityInMinutes.toString());

            const emailDetails: EmailDetails = {
                EmailTo : email,
                Subject : `Login OTP`,
                Body    : body,
            };
            logger.info(`Email details: ${JSON.stringify(emailDetails)}`);

            const sent = await emailService.sendEmail(emailDetails, false);
            if (!sent) {
                logger.info(`Unable to send email to ${email}`);
                return false;
            }
            return true;
        }
        catch (error) {
            logger.info(`Unable to send email to ${email}`);
            return false;
        }
    };


}
