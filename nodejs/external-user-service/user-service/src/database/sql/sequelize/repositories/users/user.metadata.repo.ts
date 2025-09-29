import { ApiError } from "../../../../../common/api.error";
import { logger } from "../../../../../logger/logger";
import { uuid } from '../../../../../domain.types/miscellaneous/system.types';
import { IUserMetadataRepo } from "../../../../repository.interfaces/users/user.metadata.repo.interface";
import { SupportedLanguage } from "../../../../../domain.types/users/user.enums";
import UserMetadata from "../../models/users/user.metadata.model";
import { UserMetadataModel } from "../../../../../domain.types/users/user.types";

///////////////////////////////////////////////////////////////////////////////////

export class UserMetadataRepo implements IUserMetadataRepo {

    create = async (model: UserMetadataModel): Promise<boolean> => {
        try {
            const record = await UserMetadata.create({
                UserId            : model.id,
                DisplayName       : model.DisplayName,
                DefaultTimeZone   : model.DefaultTimeZone,
                CurrentTimeZone   : model.CurrentTimeZone,
                IsTestUser        : model.IsTestUser,
                PreferredLanguage : model.PreferredLanguage,
                LastLogin         : model.LastLogin,
                UserSettings      : model.UserSettings,
            });
            return record ? true : false;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    public createDefault = async (userId: uuid): Promise<boolean> => {
        const defaultTimeZone = process.env.DEFAULT_TIME_ZONE || '+05:30';
        const userMetadata: UserMetadataModel = {
            id                : userId,
            CurrentTimeZone   : defaultTimeZone,
            DefaultTimeZone   : defaultTimeZone,
            IsTestUser        : false,
            PreferredLanguage : SupportedLanguage.English,
            UserSettings      : '{}',
            DisplayName       : null,
            LastLogin         : null,
        };
        await this.create(userMetadata);
        return true;
    };

    getUserMetadata = async (userId: uuid): Promise<UserMetadataModel> => {
        try {
            const userMetadata = await UserMetadata.findOne({ where: { userId } });
            if (!userMetadata) {
                throw new ApiError(404, 'User metadata not found.');
            }
            const dto: UserMetadataModel = {
                id                : userMetadata.id,
                DisplayName       : userMetadata.DisplayName,
                DefaultTimeZone   : userMetadata.DefaultTimeZone,
                CurrentTimeZone   : userMetadata.CurrentTimeZone,
                IsTestUser        : userMetadata.IsTestUser,
                PreferredLanguage : userMetadata.PreferredLanguage as SupportedLanguage,
                LastLogin         : userMetadata.LastLogin,
                UserSettings      : userMetadata.UserSettings ? JSON.parse(userMetadata.UserSettings) : null,
            };
            return dto;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    getCurrentTimeZone = async (userId: uuid): Promise<string> => {
        try {
            const userMetadata = await UserMetadata.findOne({ where: { userId } });
            if (!userMetadata) {
                throw new ApiError(404, 'User metadata not found.');
            }
            return userMetadata.CurrentTimeZone;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    getDefaultTimeZone = async (userId: uuid): Promise<string> => {
        try {
            const userMetadata = await UserMetadata.findOne({ where: { userId } });
            if (!userMetadata) {
                throw new ApiError(404, 'User metadata not found.');
            }
            return userMetadata.DefaultTimeZone;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    getPreferredLanguage = async (userId: uuid): Promise<SupportedLanguage> => {
        try {
            const userMetadata = await UserMetadata.findOne({ where: { userId } });
            if (!userMetadata) {
                throw new ApiError(404, 'User metadata not found.');
            }
            return userMetadata.PreferredLanguage as SupportedLanguage;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    getUserSettings = async (userId: uuid): Promise<any> => {
        try {
            const userMetadata = await UserMetadata.findOne({ where: { userId } });
            if (!userMetadata) {
                throw new ApiError(404, 'User metadata not found.');
            }
            return userMetadata.UserSettings ? JSON.parse(userMetadata.UserSettings) : null;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    getDisplayName = async (userId: uuid): Promise<string> => {
        try {
            const userMetadata = await UserMetadata.findOne({ where: { userId } });
            if (!userMetadata) {
                throw new ApiError(404, 'User metadata not found.');
            }
            return userMetadata.DisplayName;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    getLastLogin = async (userId: uuid): Promise<Date> => {
        try {
            const userMetadata = await UserMetadata.findOne({ where: { userId } });
            if (!userMetadata) {
                throw new ApiError(404, 'User metadata not found.');
            }
            return userMetadata.LastLogin;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    isTestUser = async (userId: uuid): Promise<boolean> => {
        try {
            const userMetadata = await UserMetadata.findOne({ where: { userId } });
            if (!userMetadata) {
                throw new ApiError(404, 'User metadata not found.');
            }
            return userMetadata.IsTestUser;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    updateLastLogin = async (userId: uuid, lastLogin: Date): Promise<void> => {
        try {
            const userMetadata = await UserMetadata.findOne({ where: { userId } });
            if (!userMetadata) {
                throw new ApiError(404, 'User metadata not found.');
            }
            userMetadata.LastLogin = lastLogin;
            await userMetadata.save();
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    updateCurrentTimeZone = async (userId: uuid, currentTimeZone: string): Promise<void> => {
        try {
            const userMetadata = await UserMetadata.findOne({ where: { userId } });
            if (!userMetadata) {
                throw new ApiError(404, 'User metadata not found.');
            }
            userMetadata.CurrentTimeZone = currentTimeZone;
            await userMetadata.save();
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    updateDefaultTimeZone = async (userId: uuid, defaultTimeZone: string): Promise<void> => {
        try {
            const userMetadata = await UserMetadata.findOne({ where: { userId } });
            if (!userMetadata) {
                throw new ApiError(404, 'User metadata not found.');
            }
            userMetadata.DefaultTimeZone = defaultTimeZone;
            await userMetadata.save();
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    updateDisplayName = async (userId: uuid, displayName: string): Promise<void> => {
        try {
            const userMetadata = await UserMetadata.findOne({ where: { userId } });
            if (!userMetadata) {
                throw new ApiError(404, 'User metadata not found.');
            }
            userMetadata.DisplayName = displayName;
            await userMetadata.save();
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    updatePreferredLanguage = async (userId: uuid, preferredLanguage: SupportedLanguage): Promise<void> => {
        try {
            const userMetadata = await UserMetadata.findOne({ where: { userId } });
            if (!userMetadata) {
                throw new ApiError(404, 'User metadata not found.');
            }
            userMetadata.PreferredLanguage = preferredLanguage;
            await userMetadata.save();
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    updateUserSettings = async (userId: uuid, userSettings: any): Promise<void> => {
        try {
            const userMetadata = await UserMetadata.findOne({ where: { userId } });
            if (!userMetadata) {
                throw new ApiError(404, 'User metadata not found.');
            }
            userMetadata.UserSettings = JSON.stringify(userSettings);
            await userMetadata.save();
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    update = async (userId: uuid, updates: UserMetadataModel): Promise<boolean> => {
        try {
            const record = await UserMetadata.findOne({ where: { UserId: userId } });
            if (!record) {
                logger.info(`User metadata not found: ${userId}`);
                return false;
            }
            if (updates.DisplayName !== undefined) {
                record.DisplayName = updates.DisplayName;
            }
            if (updates.DefaultTimeZone !== undefined) {
                record.DefaultTimeZone = updates.DefaultTimeZone;
            }
            if (updates.CurrentTimeZone !== undefined) {
                record.CurrentTimeZone = updates.CurrentTimeZone;
            }
            if (updates.IsTestUser !== undefined) {
                record.IsTestUser = updates.IsTestUser;
            }
            if (updates.PreferredLanguage !== undefined) {
                record.PreferredLanguage = updates.PreferredLanguage;
            }
            if (updates.UserSettings !== undefined) {
                record.UserSettings = JSON.stringify(updates.UserSettings);
            }
            await record.save();
            return true;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    delete = async (userId: uuid): Promise<void> => {
        try {
            const userMetadata = await UserMetadata.findOne({ where: { userId } });
            await userMetadata.destroy();
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

}
