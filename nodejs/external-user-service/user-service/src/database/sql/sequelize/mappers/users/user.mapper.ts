import User from '../../models/users/user.model';
import { UserDto } from '../../../../../domain.types/users/user.types';
import Tenant from '../../models/tenant/tenant.model';
import Role from '../../models/authorization/role.model';
import { Gender } from '../../../../../domain.types/miscellaneous/system.types';
import UserMetadata from '../../models/users/user.metadata.model';
import { SupportedLanguage } from '../../../../../domain.types/users/user.enums';

///////////////////////////////////////////////////////////////////////////////////

export class UserMapper {

    static toDto = (
        user: User,
        metadata: UserMetadata | null = null,
        tenant: Tenant | null = null,
        roles: Role[] | null = null
    ): UserDto => {

        if (user == null){
            return null;
        }

        // Safely cast to Gender if valid, or use default Unknown
        const gender = Object.values(Gender).includes(user.Gender as Gender)
            ? user.Gender as Gender
            : Gender.Unknown;

        const dto: UserDto = {
            id       : user.id,
            UserName : user.UserName,
            Tenant   : {
                id         : tenant ? tenant.id : user.TenantId,
                TenantCode : tenant ? tenant.TenantCode : null,
                TenantName : tenant ? tenant.Name : null,
            },
            Prefix          : user.Prefix,
            FirstName       : user.FirstName,
            MiddleName      : user.MiddleName,
            LastName        : user.LastName,
            PhoneCode       : user.PhoneCode,
            PhoneNumber     : user.PhoneNumber,
            Email           : user.Email,
            Gender          : gender,
            DateOfBirth     : user.DateOfBirth,
            ProfileImageUrl : user.ProfileImageUrl,
            Metadata        : metadata ? {
                DisplayName       : metadata.DisplayName,
                DefaultTimeZone   : metadata.DefaultTimeZone,
                CurrentTimeZone   : metadata.CurrentTimeZone,
                IsTestUser        : metadata.IsTestUser,
                PreferredLanguage : metadata.PreferredLanguage as SupportedLanguage,
                UserSettings      : metadata.UserSettings,
            } : null,
            Roles : roles ? roles.map(role => ({
                id   : role.id,
                Name : role.Name,
            })) : null,
        };
        return dto;
    };

}
