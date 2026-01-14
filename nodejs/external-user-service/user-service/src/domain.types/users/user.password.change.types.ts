import { uuid } from "../miscellaneous/system.types";

///////////////////////////////////////////////////////////////////////////////////////

export interface UserPasswordChangeModel {
    OldPassword : string;
    NewPassword : string;
}

export interface UserPasswordDto {
    id          : uuid;
    UserId      : uuid;
    PasswordHash: string;
    ValidFrom   : Date;
    ValidTill   : Date;
    Retired     : boolean;
}
