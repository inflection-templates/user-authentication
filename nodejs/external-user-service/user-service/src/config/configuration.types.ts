
export type DatabaseType = 'SQL' | 'NoSQL';
export type DatabaseORM = 'Sequelize' | 'TypeORM' | 'Mongoose';
export type FileStorageProvider = 'AWS-S3' | 'GCP-FileStore' | 'Custom';
export type SMSServiceProvider = 'Twilio' | 'Mock';
export type EmailServiceProvider = 'SendGrid' | 'Mock';
export type InAppNotificationServiceProvider = 'Firebase' | 'Mock';
export type AuthorizationType = 'Custom'; //TBD: Other options need to be supported
export type AuthenticationType = 'Custom'; //TBD: Other options need to be supported
export type LoggerProvider = 'Winston' | 'Bunyan' | 'Custom' | 'Pino';

///////////////////////////////////////////////////////////////////////////////////////////

export interface AuthConfig {
    Authentication                : AuthenticationType;
    Authorization                 : AuthorizationType;
    UseRefreshToken               : boolean;
    AccessTokenExpiresInSeconds   : number;
    RefreshTokenExpiresInSeconds  : number;
    PasswordExpirationDurationDays: number;
    OtpValidityInMinutes          : number;
    SendPasswordBySms             : boolean;
    SendPasswordByEmail           : boolean;
}

export interface DatabaseConfig {
    Type   : DatabaseType;
    ORM    : DatabaseORM;
}

export interface FileStorageConfig {
    Provider: FileStorageProvider;
}

export interface CommunicationConfig {
    SMSProvider              : SMSServiceProvider,
    EmailProvider            : EmailServiceProvider,
    InAppNotificationProvider: InAppNotificationServiceProvider
}

export interface TemporaryFoldersConfig {
    Upload                    : string,
    Download                  : string,
    CleanupFolderBeforeMinutes: number
}

export interface AnalyticsConfig {
    Enabled: boolean;
    Endpoint: string;
}

export interface Configurations {
    SystemIdentifier           : string;
    BaseUrl                    : string;
    Auth                       : AuthConfig;
    Logger                     : LoggerProvider;
    Analytics                  : AnalyticsConfig;
    Database                   : DatabaseConfig;
    FileStorage                : FileStorageConfig;
    Communication              : CommunicationConfig;
    TemporaryFolders           : TemporaryFoldersConfig;
    MaxUploadFileSize          : number;
    InternalClientAppManagement: boolean;
    ClientAppAuth              : boolean;
}
