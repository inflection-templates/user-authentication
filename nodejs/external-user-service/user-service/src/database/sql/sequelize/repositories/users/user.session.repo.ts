import { IUserSessionRepo } from '../../../../repository.interfaces/users/user.session.repo.interface';
import { uuid } from '../../../../../domain.types/miscellaneous/system.types';
import UserSession from '../../models/users/user.session.model';
import { logger } from '../../../../../logger/logger';
import { ApiError } from '../../../../../common/api.error';
import { UserSessionMapper } from '../../mappers/users/user.login.session.mapper';
import { UserSessionCreateModel, UserSessionDto } from '../../../../../domain.types/users/user.session.types';

///////////////////////////////////////////////////////////////////////////////////

export class UserSessionRepo implements IUserSessionRepo {

    create = async (model: UserSessionCreateModel):
    Promise<UserSessionDto> => {
        try {
            const entity = {
                UserId      : model.UserId,
                IsActive    : true,
                StartedAt   : model.StartedAt,
                ValidTill   : model.ValidTill,
                LoginMethod : model.LoginMethod,
                TenantId    : model.TenantId,
            };
            const session = await UserSession.create(entity);
            return await UserSessionMapper.toDto(session);
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    isValid = async (sessionId: uuid): Promise<boolean> => {
        try {
            const session = await UserSession.findByPk(sessionId);
            if (!session) {
                return false;
            }
            var now = new Date();
            if (now > session.ValidTill) {
                session.IsActive = false;
                await session.save();
                return false;
            }
            return session.IsActive;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    invalidate = async (sessionId: uuid): Promise<boolean> => {
        try {
            const session = await UserSession.findByPk(sessionId);
            session.IsActive = false;
            await session.save();
            return true;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    invalidateAll = async (userId: uuid): Promise<boolean> => {
        try {
            const sessions = await UserSession.findAll({
                where : {
                    UserId : userId
                }
            });
            if (sessions.length > 0) {
                for await (var s of sessions) {
                    s.IsActive = false;
                    await s.save();
                }
            }
            return true;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

}
