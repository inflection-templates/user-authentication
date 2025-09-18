import { OTPScope } from '../../../domain.types/users/user.enums';
import { uuid } from '../../../domain.types/miscellaneous/system.types';
import { OtpDto, OtpModel } from '../../../domain.types/users/user.otp.types';

////////////////////////////////////////////////////////////////////////////////////

export interface IUserOtpRepo {

    create(entity: OtpModel): Promise<OtpDto>;

    isActive(userId: uuid, otp: string, scope?: OTPScope): Promise<boolean>;

    getActiveOtp(userId: uuid, otp: string, scope?: OTPScope): Promise<OtpDto>;

    markAsValidated(id: uuid): Promise<OtpDto>;
}
