import { IUserSessionRepo } from '../../database/repository.interfaces/users/user.session.repo.interface';
import { inject, injectable } from 'tsyringe';
import { IUserOtpRepo } from '../../database/repository.interfaces/users/user.otp.repo.interface';
import { IRoleRepo } from '../../database/repository.interfaces/authorization/role.repo.interface';
import { IUserRepo } from '../../database/repository.interfaces/users/user.repo.interface';
import { UserMetadataModel } from '../../domain.types/users/user.types';
import { uuid } from '../../domain.types/miscellaneous/system.types';
import { ITenantRepo } from '../../database/repository.interfaces/tenant/tenant.repo.interface';
import { IUserMetadataRepo } from '../../database/repository.interfaces/users/user.metadata.repo.interface';
import { IUserRoleRepo } from '../../database/repository.interfaces/authorization/user.role.repo.interface';
import { IUserPasswordRepo } from '../../database/repository.interfaces/users/user.password.repo.interface';
import { SupportedLanguage } from '../../domain.types/users/user.enums';

////////////////////////////////////////////////////////////////////////////////////////////////////////

@injectable()
export class UserMetadataService {

    constructor(
        @inject('IUserRepo') private _userRepo: IUserRepo,
        @inject('IUserRoleRepo') private _userRoleRepo: IUserRoleRepo,
        @inject('IRoleRepo') private _roleRepo: IRoleRepo,
        @inject('IUserOtpRepo') private _otpRepo: IUserOtpRepo,
        @inject('IUserPasswordRepo') private _userPasswordRepo: IUserPasswordRepo,
        @inject('IUserSessionRepo') private _userSessionRepo: IUserSessionRepo,
        @inject('IUserMetadataRepo') private _userMetadataRepo: IUserMetadataRepo,
        @inject('ITenantRepo') private _tenantRepo: ITenantRepo,
    ) { }

    public create = async (model: UserMetadataModel): Promise<boolean> => {
        return await this._userMetadataRepo.create(model);
    };

    public createDefault = async (userId: uuid): Promise<boolean> => {
        return await this._userMetadataRepo.createDefault(userId);
    };

    public getUserMetadata = async (userId: uuid): Promise<UserMetadataModel> => {
        const user = await this._userMetadataRepo.getUserMetadata(userId);
        return user;
    };

    public getCurrentTimeZone = async (userId: uuid): Promise<string> => {
        const user = await this._userMetadataRepo.getCurrentTimeZone(userId);
        return user;
    };

    public getDefaultTimeZone = async (userId: uuid): Promise<string> => {
        const user = await this._userMetadataRepo.getDefaultTimeZone(userId);
        return user;
    };

    public getPreferredLanguage = async (userId: uuid): Promise<SupportedLanguage> => {
        const user = await this._userMetadataRepo.getPreferredLanguage(userId);
        return user;
    };

    public getUserSettings = async (userId: uuid): Promise<any> => {
        const user = await this._userMetadataRepo.getUserSettings(userId);
        return user;
    };

    public getDisplayName = async (userId: uuid): Promise<string> => {
        const user = await this._userMetadataRepo.getDisplayName(userId);
        return user;
    };

    public getLastLogin = async (userId: uuid): Promise<Date> => {
        const user = await this._userMetadataRepo.getLastLogin(userId);
        return user;
    };

    public isTestUser = async (userId: uuid): Promise<boolean> => {
        const user = await this._userMetadataRepo.isTestUser(userId);
        return user;
    };

    public updateCurrentTimeZone = async (userId: uuid, currentTimeZone: string): Promise<void> => {
        await this._userMetadataRepo.updateCurrentTimeZone(userId, currentTimeZone);
    };

    public updateDefaultTimeZone = async (userId: uuid, defaultTimeZone: string): Promise<void> => {
        await this._userMetadataRepo.updateDefaultTimeZone(userId, defaultTimeZone);
    };

    public updateDisplayName = async (userId: uuid, displayName: string): Promise<void> => {
        await this._userMetadataRepo.updateDisplayName(userId, displayName);
    };

    public updatePreferredLanguage = async (userId: uuid, preferredLanguage: SupportedLanguage): Promise<void> => {
        await this._userMetadataRepo.updatePreferredLanguage(userId, preferredLanguage);
    };

    public updateUserSettings = async (userId: uuid, userSettings: any): Promise<void> => {
        await this._userMetadataRepo.updateUserSettings(userId, userSettings);
    };

    public update = async (userId: uuid, model: UserMetadataModel): Promise<boolean> => {
        return await this._userMetadataRepo.update(userId, model);
    };

    public delete = async (userId: uuid): Promise<void> => {
        await this._userMetadataRepo.delete(userId);
    };

}
