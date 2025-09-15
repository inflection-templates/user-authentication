import { OtpDto } from "../../../../../domain.types/users/user.otp.types";
import Otp from "../../models/users/user.otp.model";

///////////////////////////////////////////////////////////////////////////////////

export class OtpMapper {

    static toDto = (otp: Otp): OtpDto => {
        if (otp == null) {
            return null;
        }
        const dto: OtpDto = {
            id        : otp.id,
            UserId    : otp.UserId,
            Scope     : otp.Scope,
            Channel   : otp.Channel,
            Otp       : otp.Otp,
            ValidFrom : otp.ValidFrom,
            ValidTill : otp.ValidTill,
            Validated : otp.Validated,
            CreatedAt : otp.CreatedAt,
        };
        return dto;
    };

}
