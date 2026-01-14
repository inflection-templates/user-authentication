import express from 'express';
import { ApiError } from '../../../common/api.error';
import { ResponseHandler } from '../../../common/handlers/response.handler';
import { UserService } from '../../../services/users/user.service';
import { UserValidator } from './user.validator';
import { Injector } from '../../../startup/injector';
import { BaseController } from '../../../api/base.controller';
import { UserEvents } from '../user.events';
import { UserDeviceDetailsService } from '../../../services/users/user.device.details.service';
import {
    UserCreateModel,
    UserUpdateModel,
    UserDto,
    UserUniqueIdentifiers,
    UserMetadataModel
} from '../../../domain.types/users/user.types';
import { UserMetadataService } from '../../../services/users/user.metadata.service';
import { SupportedLanguage } from '../../../domain.types/users/user.enums';
import { UserAuthService } from '../../../services/users/user.auth.service';
import { ConfigurationManager } from '../../../config/configuration.manager';
import { TenantService } from '../../../services/tenant/tenant.service';

///////////////////////////////////////////////////////////////////////////////////////

export class UserController extends BaseController {

    //#region member variables and constructors

    _service: UserService = Injector.Container.resolve(UserService);

    _userDeviceDetailsService: UserDeviceDetailsService = Injector.Container.resolve(UserDeviceDetailsService);

    _userMetadataService: UserMetadataService = Injector.Container.resolve(UserMetadataService);

    _userAuthService: UserAuthService = Injector.Container.resolve(UserAuthService);

    _tenantService: TenantService = Injector.Container.resolve(TenantService);

    constructor() {
        super();
    }

    //#endregion

    create = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const model: UserCreateModel = await UserValidator.create(request);

            // If no tenant provided, use default tenant (like Python service)
            if (!model.TenantId) {
                let defaultTenant = await this._tenantService.getTenantWithCode('default');

                // If no tenant with code 'default', use the first available tenant
                if (!defaultTenant) {
                    const allTenants = await this._tenantService.search({
                        PageIndex: 0,
                        ItemsPerPage: 1
                    });
                    if (allTenants && allTenants.Items && allTenants.Items.length > 0) {
                        defaultTenant = allTenants.Items[0];
                    }
                }

                if (!defaultTenant) {
                    throw new ApiError(404, 'No tenant found. Please create a tenant first.');
                }

                model.TenantId = defaultTenant.id;
            }

            let user: UserDto = null;

            const identifiers: UserUniqueIdentifiers = {
                PhoneCode   : model.PhoneCode,
                PhoneNumber : model.PhoneNumber,
                Email       : model.Email,
                UserName    : model.UserName,
                TenantId    : model.TenantId,
            };

            if (identifiers.PhoneNumber && identifiers.PhoneCode) {
                const existingUser = await this._service.getByPhone(
                    identifiers.TenantId,
                    identifiers.PhoneCode,
                    identifiers.PhoneNumber);
                if (existingUser) {
                    throw new ApiError(409, `User already exists with the same phone.`);
                }
            }
            if (identifiers.Email) {
                const existingUser = await this._service.getByEmail(
                    identifiers.TenantId,
                    identifiers.Email);
                if (existingUser) {
                    throw new ApiError(409, `User already exists with the same email.`);
                }
            }
            if (identifiers.UserName) {
                const existingUser = await this._service.getByUserName(identifiers.UserName);
                if (existingUser) {
                    throw new ApiError(409, `User already exists with the same username.`);
                }
            }

            if (!model.UserName) {
                model.UserName = await this._service.generateUserName(
                    model.FirstName,
                    model.LastName);
            }

            user = await this._service.create(model);
            if (user == null) {
                throw new ApiError(400, 'Cannot create user!');
            }
            const defaultTimeZone = process.env.DEFAULT_TIME_ZONE || '+05:30';
            const displayName = this.generateDisplayName(user);
            const userMetadata: UserMetadataModel = {
                id                : user.id,
                CurrentTimeZone   : model.CurrentTimeZone || defaultTimeZone,
                DefaultTimeZone   : model.DefaultTimeZone || defaultTimeZone,
                IsTestUser        : model.IsTestUser || false,
                PreferredLanguage : model.PreferredLanguage || SupportedLanguage.English,
                UserSettings      : model.UserSettings || '{}',
                DisplayName       : displayName,
                LastLogin         : null,
            };
            await this._userMetadataService.create(userMetadata);

            if (model.Password) {
                await this._userAuthService.setPassword(user.id, model.Password);
            }
            else {
                //Check if email is set, if yes send email or sms to user with password
                if (user.Email && ConfigurationManager.SendPasswordByEmail) {
                    await this._userAuthService.generateAndSendPasswordByEmail(user.id);
                }
                else if (user.PhoneCode && user.PhoneNumber && ConfigurationManager.SendPasswordBySms) {
                    await this._userAuthService.generateAndSendPasswordBySms(user.id);
                }
                //ELSE - The user could use reset password flow to setup the password
            }

            if (model.RoleIds?.length === 0) {
                const defaultRole = await this._roleService.getByName(DefaultRoleTypes.User);
                if (defaultRole) {
                    model.RoleIds.push(defaultRole.id);
                }
            }

            await this._userRoleService.addUserRoles(user.id, model.RoleIds, user.Tenant?.id);

            UserEvents.onUserCreated(request, user);

            ResponseHandler.success(request, response, 'User created successfully!', 201, {
                User : user,
            });

        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    getById = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const id: string = await UserValidator.getById(request);
            const user = await this._service.getById(id);
            if (user == null) {
                ResponseHandler.failure(request, response, 'User not found.', 404);
                return;
            }
            await this.authorizeOne(request, user.id, user.Tenant?.id);
            ResponseHandler.success(request, response, 'User retrieved successfully!', 200, {
                user : user,
            });
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    update = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const userId = request.params.id;
            const user = await this._service.getById(userId);
            if (user == null) {
                throw new ApiError(404, 'User not found.');
            }
            await this.authorizeOne(request, userId, user.Tenant?.id);

            const model: UserUpdateModel = await UserValidator.update(request);

            if (model.PhoneNumber && (user.PhoneNumber !== model.PhoneNumber)) {
                const existingUser = await this._service.getByPhone(
                    user.Tenant?.id,
                    model.PhoneCode,
                    model.PhoneNumber);
                if (existingUser && existingUser.id !== userId) {
                    throw new ApiError(409, `Person already exists with the phone ${model.PhoneNumber}`);
                }
            }

            if (model.Email && (user.Email !== model.Email)) {
                const existingUser = await this._service.getByEmail(
                    user.Tenant?.id,
                    model.Email);
                if (existingUser && existingUser.id !== userId) {
                    throw new ApiError(409, `Person already exists with the email ${model.Email}`);
                }
            }

            if (model.UserName && (user.UserName !== model.UserName)) {
                const existingUser = await this._service.getByUserName(model.UserName);
                if (existingUser && existingUser.id !== userId) {
                    throw new ApiError(409, `Person already exists with the username ${model.UserName}`);
                }
            }

            let updatedUser = await this._service.update(userId, model);
            if (updatedUser == null) {
                throw new ApiError(400, 'Cannot update user!');
            }

            updatedUser = await this._service.getById(userId);

            UserEvents.onUserUpdated(request, updatedUser);

            ResponseHandler.success(request, response, 'User updated successfully!', 200, {
                User : updatedUser,
            });
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    delete = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const userId = request.params.id;
            const user = await this._service.getById(userId);
            if (user == null) {
                throw new ApiError(404, 'User not found.');
            }
            await this.authorizeOne(request, userId, user.Tenant?.id);

            // Delete user metadata and device details
            await this._userMetadataService.delete(userId);
            await this._userDeviceDetailsService.deleteByUserId(userId);

            const userDeleted = await this._service.delete(userId);
            if (userDeleted == null) {
                throw new ApiError(400, 'Cannot delete user!');
            }

            UserEvents.onUserDeleted(request, user);

            ResponseHandler.success(request, response, 'User deleted successfully!', 200, {
                Deleted : userDeleted,
            });
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    search = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const searchParams = await UserValidator.search(request);
            const users = await this._service.search(searchParams);
            ResponseHandler.success(request, response, 'Users retrieved successfully!', 200, {
                Users : users,
            });
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    deleteProfileImage = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const userId = request.params.id;
            const user = await this._service.getById(userId);
            if (user == null) {
                throw new ApiError(404, 'User not found.');
            }
            await this.authorizeOne(request, userId, user.Tenant?.id);
            const updatedUser = await this._service.deleteProfileImage(user.id);
            if (updatedUser == null) {
                throw new ApiError(400, 'Cannot update user!');
            }
            ResponseHandler.success(request, response, 'User profile image deleted successfully!', 200, {
                User : updatedUser,
            });
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    private generateDisplayName(user: UserDto) {
        let displayName = user.FirstName && user.LastName ? `${user.FirstName} ${user.LastName}` : null;
        displayName = displayName ? displayName : user.FirstName ? user.FirstName : null;
        displayName = displayName ? displayName : user.LastName ? user.LastName : null;
        displayName = displayName ? displayName : user.Email ? user.Email.split('@')[0] : null;
        displayName = displayName ? displayName : user.UserName ? user.UserName : null;
        displayName = displayName ? displayName : null;
        if (displayName) {
            displayName = displayName.trim();
        }
        return displayName;
    }

}
