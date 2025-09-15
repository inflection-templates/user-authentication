import { uuid } from '../../../domain.types/miscellaneous/system.types';
import { UserSessionCreateModel, UserSessionDto } from '../../../domain.types/users/user.session.types';

////////////////////////////////////////////////////////////////////////////////////

export interface IUserSessionRepo {

    create(model: UserSessionCreateModel): Promise<UserSessionDto>;

    isValid(sessionId: uuid): Promise<boolean>;

    invalidate(sessionId: uuid): Promise<boolean>;

    invalidateAll(userId: uuid): Promise<boolean>;
}
