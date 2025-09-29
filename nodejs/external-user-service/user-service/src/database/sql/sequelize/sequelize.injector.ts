import 'reflect-metadata';
import { DependencyContainer } from 'tsyringe';
import { DatabaseConnector_Sequelize } from './database.connector.sequelize';
import { ClientAppRepo } from './repositories/client.apps/client.app.repo';
import { UserRepo } from './repositories/users/user.repo';
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
        container.register('IUserOtpRepo', UserOtpRepo);
        container.register('IUserSessionRepo', UserSessionRepo);
        container.register('IUserPasswordRepo', UserPasswordRepo);
        container.register('IUserMetadataRepo', UserMetadataRepo);
        container.register('IClientAppRepo', ClientAppRepo);
        container.register('ITenantRepo', TenantRepo);
        container.register('IUserDeviceDetailsRepo', UserDeviceDetailsRepo);
    }

}
