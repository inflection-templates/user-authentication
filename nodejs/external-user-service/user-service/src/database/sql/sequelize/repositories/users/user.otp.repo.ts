import { logger } from "../../../../../logger/logger";
import { ApiError } from "../../../../../common/api.error";
import { IUserOtpRepo } from "../../../../repository.interfaces/users/user.otp.repo.interface";
import {
    OtpDto,
    OtpModel,
} from '../../../../../domain.types/users/user.otp.types';
import { uuid } from '../../../../../domain.types/miscellaneous/system.types';
import { OtpMapper } from "../../mappers/users/otp.mapper";
import UserOtp from "../../../sequelize/models/users/user.otp.model";
import { OTPScope } from "../../../../../domain.types/users/user.enums";

///////////////////////////////////////////////////////////////////////

export class UserOtpRepo implements IUserOtpRepo {

    create = async (entity: OtpModel): Promise<OtpDto> => {
        try {
            const otp = await UserOtp.create(entity as any);
            const dto = await OtpMapper.toDto(otp);
            return dto;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    isActive = async (userId: uuid, otp: string, scope?: OTPScope): Promise<boolean> => {
        try {
            if (scope) {
                const otps = await UserOtp.findAll({
                    where : {
                        UserId : userId,
                        Otp    : otp,
                        Scope  : scope as string,
                    },
                    order : [
                        [ 'ValidTill', 'DESC' ],
                    ],
                });
                if (otps.length > 0) {
                    const otp = otps[0];
                    return !otp.Validated && otp.ValidTill > new Date();
                }
            }
            else {
                const otps = await UserOtp.findAll({
                    where : {
                        UserId : userId,
                        Otp    : otp,
                    },
                    order : [
                        [ 'ValidTill', 'DESC' ],
                    ],
                });
                if (otps.length > 0) {
                    const otp = otps[0];
                    return !otp.Validated && otp.ValidTill > new Date();
                }
            }
            return false;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    getActiveOtp = async (userId: uuid, otp: string): Promise<OtpDto> => {
        try {
            const record = await UserOtp.findOne({
                where : {
                    UserId    : userId,
                    Otp       : otp,
                    Validated : false,
                },
            });
            if (record) {
                const dto = await OtpMapper.toDto(record);
                return dto;
            }
            return null;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    markAsValidated = async (id: uuid): Promise<OtpDto> => {
        try {
            const otp = await UserOtp.findByPk(id);
            otp.Validated = true;
            await otp.save();
            const dto = await OtpMapper.toDto(otp);
            return dto;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

}
