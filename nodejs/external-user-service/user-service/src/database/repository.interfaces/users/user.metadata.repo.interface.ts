import { UserMetadataModel } from '../../../domain.types/users/user.types';
import { uuid } from '../../../domain.types/miscellaneous/system.types';
import { SupportedLanguage } from '../../../domain.types/users/user.enums';

////////////////////////////////////////////////////////////////////////////////////

export interface IUserMetadataRepo
{
    create(model: UserMetadataModel): Promise<boolean>;

    createDefault(userId: uuid): Promise<boolean>;

    getUserMetadata(userId: uuid): Promise<UserMetadataModel>;

    getCurrentTimeZone(userId: uuid): Promise<string>;

    getDefaultTimeZone(userId: uuid): Promise<string>;

    getPreferredLanguage(userId: uuid): Promise<SupportedLanguage>;

    getUserSettings(userId: uuid): Promise<any>;

    getDisplayName(userId: uuid): Promise<string>;

    getLastLogin(userId: uuid): Promise<Date>;

    isTestUser(userId: uuid): Promise<boolean>;

    updateLastLogin(userId: uuid, lastLogin: Date): Promise<void>;

    updateCurrentTimeZone(userId: uuid, currentTimeZone: string): Promise<void>;

    updateDefaultTimeZone(userId: uuid, defaultTimeZone: string): Promise<void>;

    updateDisplayName(userId: uuid, displayName: string): Promise<void>;

    updatePreferredLanguage(userId: uuid, preferredLanguage: SupportedLanguage): Promise<void>;

    updateUserSettings(userId: uuid, userSettings: any): Promise<void>;

    update(userId: uuid, model: UserMetadataModel): Promise<boolean>;

    delete(userId: uuid): Promise<void>;
}
