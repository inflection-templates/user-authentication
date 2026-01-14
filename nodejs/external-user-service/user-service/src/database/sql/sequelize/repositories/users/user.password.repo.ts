import { ApiError } from "../../../../../common/api.error";
import { logger } from "../../../../../logger/logger";
import { uuid } from '../../../../../domain.types/miscellaneous/system.types';
import UserPassword from "../../models/users/user.password.model";
import { IUserPasswordRepo } from "../../../../repository.interfaces/users/user.password.repo.interface";
import { Op } from "sequelize";
import { UserPasswordDto } from "../../../../../domain.types/users/user.password.change.types";
import { ConfigurationManager } from "../../../../../config/configuration.manager";
import { Helper } from '../../../../../common/helper';

///////////////////////////////////////////////////////////////////////////////////

export class UserPasswordRepo implements IUserPasswordRepo {

    isCurrentPasswordExpired = async (userId: uuid): Promise<boolean> => {
        try {
            const currentPassword  = await this.getCurrentPassword(userId);
            if (currentPassword == null) {
                // No password found for the user.
                // All passwords are expired.
                return true;
            }
            const currentDate = new Date();
            return currentPassword.ValidTill < currentDate;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    getUserCurrentHashedPassword = async (userId: uuid): Promise<UserPasswordDto> => {
        const current = await this.getCurrentPassword(userId);
        if (current == null) {
            return null;
        }
        const dto : UserPasswordDto = {
            id           : current.id,
            UserId       : current.UserId,
            PasswordHash : current.PasswordHash,
            ValidFrom    : current.ValidFrom,
            ValidTill    : current.ValidTill,
            Retired      : current.Retired
        };
        return dto;
    };

    updateCurrentUserHashedPassword = async (userId: uuid, newHashedPassword: string): Promise<void> => {
        try {
            const userPasswords = await UserPassword.findAll({
                where : {
                    UserId  : userId,
                    Retired : false,
                },
                order : [
                    ['createdAt', 'DESC']
                ]
            });
            for await (const userPassword of userPasswords) {
                userPassword.Retired = true;
                await userPassword.save();
            }
            const expirationDurationDays = ConfigurationManager.PasswordExpirationDurationDays;
            const durationInMilliseconds = 1000 * 60 * 60 * 24 * expirationDurationDays;
            const newUserPassword = new UserPassword({
                UserId       : userId,
                PasswordHash : newHashedPassword,
                Retired      : false,
                ValidFrom    : new Date(),
                ValidTill    : new Date(new Date().getTime() + durationInMilliseconds)
            });
            await newUserPassword.save();
        }
        catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    checkPastPasswordForSimilarity = async (
        userId: uuid,
        newPassword: string,
        numberOfPastPasswords: number
    ): Promise<boolean> => {
        try {
            //Take last numberOfPastPasswords passwords
            //Check if the new password is similar to any of the past passwords
            //If it is, return true
            //If it is not, return false
            const userPasswords = await UserPassword.findAll({
                where : {
                    UserId : userId,
                },
                order : [
                    ['createdAt', 'DESC']
                ],
                limit : numberOfPastPasswords
            });
            const pastPasswords = userPasswords.slice(0, numberOfPastPasswords);
            for (const pastPassword of pastPasswords) {
                const passwordHash = pastPassword.PasswordHash;
                const isSimilar = Helper.compare(newPassword, passwordHash);
                if (isSimilar) {
                    return true;
                }
            }
            return false;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    private getCurrentPassword = async (userId: uuid): Promise<UserPassword> => {
        try {
            const userPasswords = await UserPassword.findAll({
                where : {
                    UserId    : userId,
                    Retired   : false,
                    ValidFrom : {
                        [Op.lte] : new Date()
                    },
                    ValidTill : {
                        [Op.gte] : new Date()
                    }
                },
                order : [
                    ['createdAt', 'DESC']
                ]
            });
            if (userPasswords.length === 0) {
                // No password found for the user.
                // All passwords are expired.
                return null;
            }
            if (userPasswords.length > 1) {
                // More than one password found for the user.
                // This should not happen.
                // Retire all passwords except the latest one.
                for (let i = 0; i < userPasswords.length; i++) {
                    if (i !== 0) {
                        userPasswords[i].Retired = true;
                        await userPasswords[i].save();
                    }
                }
            }
            const currentPassword = userPasswords[0];
            return currentPassword;
        }
        catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

}
