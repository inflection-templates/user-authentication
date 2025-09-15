import { generate } from 'generate-password';
import { inject, injectable } from 'tsyringe';
import { ApiError } from '../../common/api.error';
import { Helper } from '../../common/helper';
import { logger } from '../../logger/logger';
import { TimeHelper } from '../../common/time.helper';
import { IRoleRepo } from '../../database/repository.interfaces/authorization/role.repo.interface';
import { IUserRepo } from '../../database/repository.interfaces/users/user.repo.interface';
import { DefaultRoleTypes } from '../../domain.types/authorization/enums';
import { UserCreateModel, UserDto, UserUniqueIdentifiers, UserUpdateModel } from '../../domain.types/users/user.types';
import { uuid } from '../../domain.types/miscellaneous/system.types';
import { ITenantRepo } from '../../database/repository.interfaces/tenant/tenant.repo.interface';
import { UserSearchFilters, UserSearchResults } from '../../domain.types/users/user.types';
import { IUserMetadataRepo } from '../../database/repository.interfaces/users/user.metadata.repo.interface';
import { IUserRoleRepo } from '../../database/repository.interfaces/authorization/user.role.repo.interface';
import { IUserPasswordRepo } from '../../database/repository.interfaces/users/user.password.repo.interface';

////////////////////////////////////////////////////////////////////////////////////////////////////////

@injectable()
export class UserService {

    constructor(
        @inject('IUserRepo') private _userRepo: IUserRepo,
        @inject('IUserRoleRepo') private _userRoleRepo: IUserRoleRepo,
        @inject('IRoleRepo') private _roleRepo: IRoleRepo,
        @inject('IUserMetadataRepo') private _userMetadataRepo: IUserMetadataRepo,
        @inject('IUserPasswordRepo') private _userPasswordRepo: IUserPasswordRepo,
        @inject('ITenantRepo') private _tenantRepo: ITenantRepo,
    ) {}

    //#region Publics

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

    public getUserRoles = async (userId: uuid): Promise<{ id: uuid; Name: string }[]> => {
        var dto = await this._userRepo.getById(userId);
        if (dto == null) {
            throw new ApiError(404, 'User not found.');
        }
        return dto.Roles ? dto.Roles.map(role => ({
            id   : role.id,
            Name : role.Name,
        })) : [];
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

    // public updateCurrentTimezone = async () => {
    //     try {
    //         const users = await this._userRepo.getAllRegisteredUsers();
    //         for await (var u of users) {
    //             var extractedResult = await this.sanitizeTimezone(u.DefaultTimeZone);
    //             u.CurrentTimeZone = extractedResult;
    //             var entity: UserDomainModel = {
    //                 CurrentTimeZone : extractedResult,
    //                 DefaultTimeZone : extractedResult,
    //             };
    //             const updateUser = await this._userRepo.update(u.id, entity);
    //         }
    //     } catch (error) {
    //         logger.info(`Error updating the current timezone.`);
    //     }
    // };

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

            const role = await this._roleRepo.getByName(DefaultRoleTypes.SystemAdmin);

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
            await this._userMetadataRepo.updateDisplayName(user.id, DefaultRoleTypes.SystemAdmin);
            await this._userRoleRepo.addUserRole(user.id, role.id, tenant.id);
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

    //#endregion

    //#region Privates

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

    //#endregion

}
