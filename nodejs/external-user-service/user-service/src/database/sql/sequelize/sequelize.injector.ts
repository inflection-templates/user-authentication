import 'reflect-metadata';
import { DependencyContainer } from 'tsyringe';
import { DatabaseConnector_Sequelize } from './database.connector.sequelize';
import { ClientAppRepo } from './repositories/client.apps/client.app.repo';
import { FileResourceRepo } from './repositories/general/file.resource.repo';
import { UserRoleRepo } from './repositories/authorization/user.role.repo';
import { UserPermissionRepo } from './repositories/authorization/user.permission.repo';
import { PermissionRepo } from './repositories/authorization/permission.repo';
import { RolePermissionRepo } from './repositories/authorization/role.permission.repo';
import { RoleRepo } from './repositories/authorization/role.repo';
import { UserRepo } from './repositories/users/user.repo';
import { ThirdpartyApiRepo } from './repositories/general/thirdparty.api.repo';
import { UserSessionRepo } from './repositories/users/user.session.repo';
import { TenantRepo } from './repositories/tenant/tenant.repo';
import { UserOtpRepo } from './repositories/users/user.otp.repo';
import { UserPasswordRepo } from './repositories/users/user.password.repo';
import { UserMetadataRepo } from './repositories/users/user.metadata.repo';
import { UserDeviceDetailsRepo } from './repositories/users/user.device.details.repo';

////////////////////////////////////////////////////////////////////////////////

export class SequelizeInjector {

    static registerInjections(container: DependencyContainer) {

        container.register('IPrimaryDatabaseConnector', DatabaseConnector_Sequelize);

        container.register('IUserRepo', UserRepo);
        container.register('IUserRoleRepo', UserRoleRepo);
        container.register('IUserPermissionRepo', UserPermissionRepo);
        container.register('IPermissionRepo', PermissionRepo);
        container.register('IRoleRepo', RoleRepo);
        container.register('IUserOtpRepo', UserOtpRepo);
        container.register('IUserSessionRepo', UserSessionRepo);
        container.register('IUserPasswordRepo', UserPasswordRepo);
        container.register('IUserMetadataRepo', UserMetadataRepo);
        container.register('IClientAppRepo', ClientAppRepo);
        container.register('IRolePermissionRepo', RolePermissionRepo);
        container.register('IFileResourceRepo', FileResourceRepo);
        container.register('IThirdpartyApiRepo', ThirdpartyApiRepo);
        container.register('ITenantRepo', TenantRepo);
        container.register('IUserDeviceDetailsRepo', UserDeviceDetailsRepo);
    }

}
