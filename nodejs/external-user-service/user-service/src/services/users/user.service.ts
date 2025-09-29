import { generate } from 'generate-password';
import { inject, injectable } from 'tsyringe';
import { ApiError } from '../../common/api.error';
import { Helper } from '../../common/helper';
import { logger } from '../../logger/logger';
import { TimeHelper } from '../../common/time.helper';
import { IUserRepo } from '../../database/repository.interfaces/users/user.repo.interface';
import { UserCreateModel, UserDto, UserUniqueIdentifiers, UserUpdateModel } from '../../domain.types/users/user.types';
import { uuid } from '../../domain.types/miscellaneous/system.types';
import { ITenantRepo } from '../../database/repository.interfaces/tenant/tenant.repo.interface';
import { UserSearchFilters, UserSearchResults } from '../../domain.types/users/user.types';
import { IUserMetadataRepo } from '../../database/repository.interfaces/users/user.metadata.repo.interface';
import { IUserPasswordRepo } from '../../database/repository.interfaces/users/user.password.repo.interface';

////////////////////////////////////////////////////////////////////////////////////////////////////////

@injectable()
export class UserService {

    constructor(
        @inject('IUserRepo') private _userRepo: IUserRepo,
        @inject('IUserMetadataRepo') private _userMetadataRepo: IUserMetadataRepo,
        @inject('IUserPasswordRepo') private _userPasswordRepo: IUserPasswordRepo,
        @inject('ITenantRepo') private _tenantRepo: ITenantRepo,
    ) {}

    public create = async (model: UserCreateModel) => {

        var dto = await this._userRepo.create(model);
        if (dto == null) {
            return null;
        }

        dto = await this._userRepo.getById(dto.id);
        return dto;
    };

    public getById = async (id: string): Promise<UserDto> => {
        var dto = await this._userRepo.getById(id);
        logger.info(`DTO from user repo: ${JSON.stringify(dto)}`);
        return dto;
    };

    public getByPhone = async (tenantId: uuid, phoneCode: string, phoneNumber: string): Promise<UserDto> => {
        var dto = await this._userRepo.getByPhone(tenantId, phoneCode, phoneNumber);
        return dto;
    };

    public getByEmail = async (tenantId: uuid, email: string): Promise<UserDto> => {
        var dto = await this._userRepo.getByEmail(tenantId, email);
        return dto;
    };

    public getByUserName = async (userName: string): Promise<UserDto> => {
        var dto = await this._userRepo.getByUserName(userName);
        return dto;
    };

    public update = async (id: string, model: UserUpdateModel): Promise<UserDto> => {
        var dto = await this._userRepo.update(id, model);
        return dto;
    };

    public search = async (filters: UserSearchFilters): Promise<UserSearchResults> => {
        return await this._userRepo.search(filters);
    };

    public delete = async (id: string): Promise<boolean> => {
        return await this._userRepo.delete(id);
    };

    public generateUserName = async (
        firstName: string | null | undefined,
        lastName: string | null | undefined): Promise<string> => {
        if (!firstName) {
            firstName = generate({ length: 4, numbers: false, lowercase: true, uppercase: false, symbols: false });
        }
        if (!lastName) {
            lastName = generate({ length: 4, numbers: false, lowercase: true, uppercase: false, symbols: false });
        }
        let userName = this.constructUserName(firstName, lastName);
        let exists = await this._userRepo.getByUserName(userName);
        while (exists) {
            userName = this.constructUserName(firstName, lastName);
            exists = await this._userRepo.getByUserName(userName);
        }
        return userName;
    };

    public getDateInUserTimeZone = async (userId: uuid, dateStr: string, useCurrent = true) => {
        var user = await this.getById(userId);
        if (user === null) {
            throw new ApiError(422, 'Invalid user id.');
        }
        var timezoneOffset = '+05:30';
        if (user.Metadata.CurrentTimeZone !== null && useCurrent) {
            timezoneOffset = user.Metadata.CurrentTimeZone;
        } else if (user.Metadata.DefaultTimeZone !== null) {
            timezoneOffset = user.Metadata.DefaultTimeZone;
        }
        return TimeHelper.getDateWithTimezone(dateStr, timezoneOffset);
    };

    public seedSystemAdmin = async () => {
        try {
            const sysAdmin = Helper.loadJSONSeedFile('system.admin.seed.json');
            const tenant = await this._tenantRepo.getTenantWithCode('default');
            if (tenant == null) {
                throw new ApiError(404, 'Default tenant not found.');
            }
            const existingUser = await this._userRepo.getByUserName(sysAdmin.UserName);
            if (existingUser) {
                return;
            }

            const model: UserCreateModel = {
                TenantId    : tenant.id,
                PhoneCode   : sysAdmin.PhoneCode,
                PhoneNumber : sysAdmin.PhoneNumber,
                Email       : sysAdmin.Email,
                FirstName   : sysAdmin.FirstName,
                UserName    : sysAdmin.UserName,
            };

            const user = await this._userRepo.create(model);
            await this._userMetadataRepo.createDefault(user.id);
            await this._userMetadataRepo.updateDisplayName(user.id, 'System Admin');
            if (sysAdmin.Password) {
                const hashedPassword = Helper.hash(sysAdmin.Password);
                await this._userPasswordRepo.updateCurrentUserHashedPassword(user.id, hashedPassword);
            }

            logger.info('Seeded admin user successfully!');
        } catch (error) {
            logger.info(error);
        }
    };

    public getExistingUser = async (model: UserUniqueIdentifiers): Promise<UserDto> => {

        let user: UserDto = null;

        if (model.PhoneCode && model.PhoneNumber) {
            user = await this._userRepo.getByPhone(model.TenantId, model.PhoneCode, model.PhoneNumber);
            if (user == null) {
                const message = 'User does not exist with phone(' + model.PhoneNumber + ')';
                logger.info(message);
            }
        }
        if (model.Email && !user) {
            user = await this._userRepo.getByEmail(model.TenantId, model.Email);
            if (user == null) {
                const message = 'User does not exist with email(' + model.Email + ')';
                logger.info(message);
            }
        }
        if (model.UserName && !user) {
            user = await this._userRepo.getByUserName(model.UserName);
            if (user == null) {
                const message = 'User does not exist with username (' + model.UserName + ')';
                logger.info(message);
            }
        }
        if (user == null) {
            return null;
        }

        return user;
    };

    private constructUserName(firstName: string, lastName: string) {
        const rand = Math.random().toString(10)
            .substring(2, 4);
        let userName = firstName.substring(0, 3) + lastName.substring(0, 3) + rand;
        userName = userName.toLowerCase();
        return userName;
    }

    deleteProfileImage = async (id: string): Promise<UserDto> => {
        var dto = await this._userRepo.deleteProfileImageUrl(id);
        return dto;
    };

    private sanitizeTimezone = (inputString) => {
        const parts = inputString.split(':');

        if (parts.length < 3) {
            return inputString;
        }

        const extractedString = parts.slice(0, 2).join(':');
        return extractedString;
    };

}
