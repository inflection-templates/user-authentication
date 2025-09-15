import express from 'express';
import { TenantService } from '../../../services/tenant/tenant.service';
import { ResponseHandler } from '../../../common/handlers/response.handler';
import { Injector } from '../../../startup/injector';
import { TenantValidator } from './tenant.validator';
import { ApiError } from '../../../common/api.error';
import { uuid } from '../../../domain.types/miscellaneous/system.types';
import { RoleService } from '../../../services/authorization/role.service';
import { UserService } from '../../../services/users/user.service';
import { UserRoleService } from '../../../services/authorization/user.role.service';
import { DefaultRoleTypes } from '../../../domain.types/authorization/enums';
import { UserCreateModel } from '../../../domain.types/users/user.types';
import { logger } from '../../../logger/logger';
import { EmailService } from "../../../modules/communication/email/email.service";
import { EmailDetails } from "../../../modules/communication/email/email.details";
import { TenantDto, TenantCreateModel, TenantUpdateModel } from "../../../domain.types/tenant/tenant.types";
import { Helper } from '../../../common/helper';
import { BaseController } from '../../../api/base.controller';

///////////////////////////////////////////////////////////////////////////////////////

export class TenantController extends BaseController {

    //#region member variables and constructors

    _service: TenantService = Injector.Container.resolve(TenantService);

    _roleService: RoleService = Injector.Container.resolve(RoleService);

    _userService: UserService = Injector.Container.resolve(UserService);

    _userRoleService: UserRoleService = Injector.Container.resolve(UserRoleService);

    _validator: TenantValidator = new TenantValidator();

    //#endregion

    create = async (request: express.Request, response: express.Response): Promise<void> => {
        let tenant: TenantDto = null;
        try {
            const model: TenantCreateModel = await this._validator.create(request);
            if (model.TenantCode === 'default') {
                throw new ApiError(400, 'Cannot create tenant with code "default"!');
            }

            await this.authorizeOne(request);

            tenant = await this._service.create(model);
            if (tenant == null) {
                throw new ApiError(400, 'Unable to create tenant.');
            }

            const tenantCode = tenant.TenantCode;
            const adminUserName = model.UserName?.toLowerCase() ?? (tenantCode + '-admin').toLowerCase();
            let existingUser = await this._userService.getByUserName(adminUserName);
            if (existingUser) {
                throw new ApiError(400, 'Username already exists');
            }

            const adminPassword = model.Password ?? Helper.generatePassword();
            const role = await this._roleService.getByName(DefaultRoleTypes.TenantAdmin);
            const userModel: UserCreateModel = {
                PhoneCode   : tenant.PhoneCode,
                PhoneNumber : tenant.PhoneNumber,
                Email       : tenant.Email,
                FirstName   : 'Admin',
                LastName    : tenant.Name,
                TenantId    : tenant.id,
                UserName    : adminUserName,
            };

            existingUser = await this._userService.getExistingUser({
                PhoneCode   : userModel.PhoneCode,
                PhoneNumber : userModel.PhoneNumber,
                Email       : userModel.Email,
                UserName    : userModel.UserName,
                TenantId    : userModel.TenantId,
            });

            if (existingUser) {
                throw new ApiError(400, 'User already exists!');
            }

            const user = await this._userService.create(userModel);
            if (user == null) {
                throw new ApiError(400, 'Unable to create tenant admin user.');
            }
            logger.info(`Tenant admin user created successfully. UserName: ${adminUserName}`);

            const userRole = await this._userRoleService.addUserRole(user.id, role.id, tenant.id);
            if (userRole == null) {
                throw new ApiError(400, 'Unable to create tenant admin user role.');
            }
            //Send email to the admin user with username and password
            await this.sendWelcomeEmail(tenant, adminUserName, adminPassword);

            ResponseHandler.success(request, response, 'Tenant added successfully!', 201, {
                Tenant    : tenant,
                AdminUser : {
                    UserName : adminUserName
                }
            });
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }

    };

    getById = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const id: uuid = await this._validator.getParamUuid(request, 'id');
            const tenant = await this._service.getById(id);
            if (tenant == null) {
                throw new ApiError(404, 'Tenant not found.');
            }
            ResponseHandler.success(request, response, 'Tenant retrieved successfully!', 200, {
                Tenant : tenant,
            });
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    getByCode = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const code: string = await this._validator.getParamStr(request, 'code');
            const tenant = await this._service.getByCode(code);
            if (tenant == null) {
                throw new ApiError(404, 'Tenant not found.');
            }
            ResponseHandler.success(request, response, 'Tenant retrieved successfully!', 200, {
                Tenant : tenant,
            });
        }
        catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    search = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const currentUserRoles = request.currentUser.Roles;
            const roleNames = currentUserRoles.map(r => r.Name);
            if (!roleNames.includes(DefaultRoleTypes.SystemAdmin) &&
                !roleNames.includes(DefaultRoleTypes.SystemUser)
            ) {
                throw new ApiError(403, 'Unauthorized action!');
            }
            const filters = await this._validator.search(request);
            const searchResults = await this._service.search(filters);
            const count = searchResults.Items.length;
            const message =
                count === 0 ? 'No records found!' : `Total ${count} tenant records retrieved successfully!`;
            ResponseHandler.success(request, response, message, 200, {
                TenantRecords : searchResults,
            });
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    update = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const id: uuid = await this._validator.getParamUuid(request, 'id');
            const tenant = await this._service.getById(id);
            if (tenant == null) {
                throw new ApiError(404, 'Tenant not found.');
            }
            await this.authorizeOne(request, null, tenant.id);
            if (tenant.TenantCode === 'default') {
                throw new ApiError(400, 'Cannot update tenant with code "default"!');
            }
            const model: TenantUpdateModel = await this._validator.update(request);
            const updatedTenant = await this._service.update(id, model);
            if (tenant == null) {
                throw new ApiError(400, 'Unable to update tenant record!');
            }
            ResponseHandler.success(request, response, 'Tenant updated successfully!', 200, {
                Tenant : updatedTenant,
            });
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    delete = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const id: uuid = await this._validator.getParamUuid(request, 'id');
            const tenant = await this._service.getById(id);
            if (tenant == null) {
                throw new ApiError(404, 'Tenant not found.');
            }
            await this.authorizeOne(request, null, tenant.id);
            if (tenant.TenantCode === 'default') {
                throw new ApiError(400, 'Cannot delete tenant with code "default"!');
            }
            const deleted = await this._service.delete(id);
            ResponseHandler.success(request, response, 'Tenant deleted successfully!', 200, {
                Deleted : deleted,
            });
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    promoteTenantUserAsAdmin = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const id: uuid = await this._validator.getParamUuid(request, 'id');
            const userId: uuid = await this._validator.getParamUuid(request, 'userId');
            const tenant = await this._service.getById(id);
            if (tenant == null) {
                throw new ApiError(404, 'Tenant not found.');
            }
            await this.authorizeOne(request, null, tenant.id);
            const user = await this._service.getById(userId);
            if (user == null) {
                throw new ApiError(404, 'User not found.');
            }
            const promoted = await this._service.promoteTenantUserAsAdmin(id, userId);
            ResponseHandler.success(request, response, 'User promoted as admin to tenant successfully!', 200, {
                Promoted : promoted,
            });
        }
        catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    demoteAdmin = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const id: uuid = await this._validator.getParamUuid(request, 'id');
            const userId: uuid = await this._validator.getParamUuid(request, 'userId');
            const tenant = await this._service.getById(id);
            if (tenant == null) {
                throw new ApiError(404, 'Tenant not found.');
            }
            await this.authorizeOne(request, null, tenant.id);
            const user = await this._service.getById(userId);
            if (user == null) {
                throw new ApiError(404, 'User not found.');
            }
            const demoted = await this._service.demoteAdmin(id, userId);
            ResponseHandler.success(request, response, 'User demoted as admin from tenant successfully!', 200, {
                Demoted : demoted,
            });
        }
        catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    getTenantStats = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const id: uuid = await this._validator.getParamUuid(request, 'id');
            const tenant = await this._service.getById(id);
            if (tenant == null) {
                throw new ApiError(404, 'Tenant not found.');
            }
            await this.authorizeOne(request, null, tenant.id);
            const stats = await this._service.getTenantStats(id);
            ResponseHandler.success(request, response, 'Tenant stats retrieved successfully!', 200, {
                Stats : stats,
            });
        }
        catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    getTenantAdmins = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const id: uuid = await this._validator.getParamUuid(request, 'id');
            const tenant = await this._service.getById(id);
            if (tenant == null) {
                throw new ApiError(404, 'Tenant not found.');
            }
            await this.authorizeOne(request, null, tenant.id);
            const admins = await this._service.getTenantAdmins(id);
            ResponseHandler.success(request, response, 'Tenant admins retrieved successfully!', 200, {
                Admins : admins,
            });
        }
        catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    getTenantRegularUsers = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const id: uuid = await this._validator.getParamUuid(request, 'id');
            const tenant = await this._service.getById(id);
            if (tenant == null) {
                throw new ApiError(404, 'Tenant not found.');
            }
            await this.authorizeOne(request, null, tenant.id);
            const moderators = await this._service.getTenantRegularUsers(id);
            ResponseHandler.success(request, response, 'Tenant moderators retrieved successfully!', 200, {
                Moderators : moderators,
            });
        }
        catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    private sendWelcomeEmail = async (tenant: TenantDto, adminUserName: string, adminPassword: string) => {
        try {
            const emailService = new EmailService();
            var body = await emailService.getTemplate('tenant.welcome.template.html');

            body = body.replace(/{{PLATFORM_NAME}}/g, process.env.PLATFORM_NAME);
            body = body.replace(/{{PLATFORM_WEBSITE}}/g, process.env.PLATFORM_WEBSITE);
            body = body.replace(/{{PLATFORM_SUPPORT_EMAIL}}/g, process.env.PLATFORM_SUPPORT_EMAIL);
            body = body.replace(/{{TENANT_NAME}}/g, tenant.Name);
            body = body.replace('{{TENANT_ADMIN_USER_NAME}}', adminUserName);
            body = body.replace('{{TENANT_ADMIN_PASSWORD}}', adminPassword);

            const emailDetails: EmailDetails = {
                EmailTo : tenant.Email,
                Subject : `Welcome`,
                Body    : body,
            };
            logger.info(`Username -> ${adminUserName}`);
            logger.info(`Password -> ${adminPassword}`);
            logger.info(`Sending welcome email to ${tenant.Email}`);
            logger.info(`Email details: ${JSON.stringify(emailDetails)}`);

            const sent = await emailService.sendEmail(emailDetails, false);
            if (!sent) {
                logger.info(`Unable to send email to ${tenant.Email}`);
            }
        }
        catch (error) {
            logger.info(`Unable to send email to ${tenant.Email}`);
        }
    };

    private rollbackCreateTenant = async (tenant: TenantDto) => {
        try {
            if (tenant) {
                await this._service.delete(tenant.id, true);
            }
        } catch (error) {
            logger.info(error);
        }
    };

}
