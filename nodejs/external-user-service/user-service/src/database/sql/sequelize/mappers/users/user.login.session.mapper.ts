import { UserSessionDto } from '../../../../../domain.types/users/user.session.types';
import UserSession from '../../models/users/user.session.model';

///////////////////////////////////////////////////////////////////////////////////

export class UserSessionMapper {

    static toDto = (
        userSession: UserSession): UserSessionDto => {
        if (userSession == null) {
            return null;
        }
        const dto: UserSessionDto = {
            id        : userSession.id,
            UserId    : userSession.UserId,
            IsActive  : userSession.IsActive,
            StartedAt : userSession.StartedAt,
            ValidTill : userSession.ValidTill,
            TenantId  : userSession.TenantId,
        };
        return dto;
    };

}
