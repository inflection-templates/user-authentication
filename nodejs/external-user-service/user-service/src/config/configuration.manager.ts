import path from 'path';
import * as defaultConfiguration from '../../config.json';
import * as localConfiguration from '../../config.local.json';
import {
    AuthenticationType,
    AuthorizationType,
    Configurations,
    DatabaseORM,
    DatabaseType,
    EmailServiceProvider,
    FileStorageProvider,
    InAppNotificationServiceProvider,
    LoggerProvider,
    SMSServiceProvider
} from './configuration.types';

////////////////////////////////////////////////////////////////////////////////////////////////////////

export class ConfigurationManager {

    static _config: Configurations = null;

    public static loadConfigurations = (): void => {

        const configuration = process.env.NODE_ENV === 'local'
            || process.env.NODE_ENV === 'test'
            ? localConfiguration : defaultConfiguration;

        ConfigurationManager._config = {
            SystemIdentifier : configuration.SystemIdentifier,
            BaseUrl          : process.env.BASE_URL,
            Logger           : process.env.LOGGER_PROVIDER as LoggerProvider,
            Auth             : {
                Authentication                 : configuration.Auth.Authentication as AuthenticationType,
                Authorization                  : configuration.Auth.Authorization as AuthorizationType,
                UseRefreshToken                : configuration.Auth.UseRefreshToken,
                AccessTokenExpiresInSeconds    : configuration.Auth.AccessTokenExpiresInSeconds,
                RefreshTokenExpiresInSeconds   : configuration.Auth.RefreshTokenExpiresInSeconds,
                PasswordExpirationDurationDays : configuration.Auth.PasswordExpirationDurationDays,
                OtpValidityInMinutes           : configuration.Auth.OtpValidityInMinutes,
                SendPasswordBySms              : configuration.Auth.SendPasswordBySms,
                SendPasswordByEmail            : configuration.Auth.SendPasswordByEmail,
            },
            Analytics : {
                Enabled  : configuration.Analytics.Enabled,
                Endpoint : configuration.Analytics.Endpoint,
            },
            Database : {
                Type : configuration.Database.Type as DatabaseType,
                ORM  : configuration.Database.ORM as DatabaseORM,
            },
            FileStorage : {
                Provider : configuration?.FileStorage?.Provider as FileStorageProvider ?? 'Custom',
            },
            Communication : {
                SMSProvider               : configuration.Communication.SMS.Provider as SMSServiceProvider,
                EmailProvider             : configuration.Communication.Email.Provider as EmailServiceProvider,
                // eslint-disable-next-line max-len
                InAppNotificationProvider : configuration.Communication.InAppNotifications.Provider as InAppNotificationServiceProvider,
            },
            TemporaryFolders : {
                Upload                     : configuration.TemporaryFolders.Upload as string,
                Download                   : configuration.TemporaryFolders.Download as string,
                CleanupFolderBeforeMinutes : configuration.TemporaryFolders.CleanupFolderBeforeMinutes as number,
            },
            MaxUploadFileSize           : configuration.MaxUploadFileSize,
            InternalClientAppManagement : configuration.InternalClientAppManagement,
            ClientAppAuth               : configuration.ClientAppAuth,
        };

        ConfigurationManager.checkConfigSanity();
    };

    public static get BaseUrl(): string {
        return ConfigurationManager._config.BaseUrl;
    }

    public static get SystemIdentifier(): string {
        return ConfigurationManager._config.SystemIdentifier;
    }

    public static get Logger(): LoggerProvider {
        return ConfigurationManager._config.Logger;
    }

    public static get Authentication(): AuthenticationType {
        return ConfigurationManager._config.Auth.Authentication;
    }

    public static get Authorization(): AuthorizationType {
        return ConfigurationManager._config.Auth.Authorization;
    }

    public static get UseRefreshToken(): boolean {
        return ConfigurationManager._config.Auth.UseRefreshToken;
    }

    public static get AccessTokenExpiresInSeconds(): number {
        return ConfigurationManager._config.Auth.AccessTokenExpiresInSeconds;
    }

    public static get RefreshTokenExpiresInSeconds(): number {
        return ConfigurationManager._config.Auth.RefreshTokenExpiresInSeconds;
    }

    public static get PasswordExpirationDurationDays(): number {
        return ConfigurationManager._config.Auth.PasswordExpirationDurationDays;
    }

    public static get OtpValidityInMinutes(): number {
        return ConfigurationManager._config.Auth.OtpValidityInMinutes;
    }

    public static get SendPasswordBySms(): boolean {
        return ConfigurationManager._config.Auth.SendPasswordBySms;
    }

    public static get SendPasswordByEmail(): boolean {
        return ConfigurationManager._config.Auth.SendPasswordByEmail;
    }

    public static get AnalyticsEnabled(): boolean {
        return ConfigurationManager._config.Analytics.Enabled;
    }

    public static get AnalyticsEndpoint(): string {
        return ConfigurationManager._config.Analytics.Endpoint;
    }

    public static get DatabaseType(): DatabaseType {
        return ConfigurationManager._config.Database.Type;
    }

    public static get DatabaseORM(): DatabaseORM {
        return ConfigurationManager._config.Database.ORM;
    }

    public static get MaxUploadFileSize(): number {
        return ConfigurationManager._config.MaxUploadFileSize;
    }

    public static get FileStorageProvider(): FileStorageProvider {
        return ConfigurationManager._config.FileStorage.Provider;
    }

    public static get SMSServiceProvider(): SMSServiceProvider {
        return ConfigurationManager._config.Communication.SMSProvider;
    }

    public static get EmailServiceProvider(): EmailServiceProvider {
        return ConfigurationManager._config.Communication.EmailProvider;
    }

    public static get UploadTemporaryFolder(): string {
        var location = ConfigurationManager._config.TemporaryFolders.Upload;
        return path.join(process.cwd(), location);
    }

    public static get DownloadTemporaryFolder(): string {
        var location = ConfigurationManager._config.TemporaryFolders.Download;
        return path.join(process.cwd(), location);
    }

    public static get TemporaryFolderCleanupBefore(): number {
        return ConfigurationManager._config.TemporaryFolders.CleanupFolderBeforeMinutes;
    }

    public static get InAppNotificationServiceProvider(): InAppNotificationServiceProvider {
        return ConfigurationManager._config.Communication.InAppNotificationProvider;
    }

    public static get InternalClientAppManagement(): boolean {
        return ConfigurationManager._config.InternalClientAppManagement;
    }

    public static get ClientAppAuth(): boolean {
        return ConfigurationManager._config.ClientAppAuth;
    }

    private static checkConfigSanity() {

        //Check database configurations

        if (ConfigurationManager._config.Database.Type === 'SQL') {
            var orm = ConfigurationManager._config.Database.ORM;
            if (orm !== 'Sequelize' && orm !== 'TypeORM') {
                throw new Error('Database configuration error! - Unspported/non-matching ORM');
            }
        }
        if (ConfigurationManager._config.Database.Type === 'NoSQL') {
            var orm = ConfigurationManager._config.Database.ORM;
            const dialect = process.env.DB_DIALECT;
            if (dialect === 'MongoDB') {
                if (orm !== 'Mongoose') {
                    throw new Error('Database configuration error! - Unspported/non-matching ORM');
                }
            }
        }
    }

}

ConfigurationManager.loadConfigurations();
