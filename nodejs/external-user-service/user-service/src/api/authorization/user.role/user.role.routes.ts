import express from 'express';
import { UserRoleController } from './user.role.controller';
import { auth } from '../../../auth/user.auth/user.auth.handler';
import { UserRoleAuth } from './user.role.auth';

///////////////////////////////////////////////////////////////////////////////////////

export const register = (app: express.Application): void => {

    const router = express.Router();
    const controller = new UserRoleController();

    router.post('/:userId/roles/:roleId', auth(UserRoleAuth.addUserRole), controller.addUserRole);
    router.get('/:userId', auth(UserRoleAuth.getUserRoles), controller.getUserRoles);
    router.delete('/:userId/roles', auth(UserRoleAuth.removeAllUserRoles), controller.removeAllUserRoles);
    router.delete('/:userId/roles/:roleId', auth(UserRoleAuth.removeUserRole), controller.removeUserRole);

    app.use('/api/v1/user-roles', router);
};
