import express from 'express';
import { UserPermissionController } from './user.permission.controller';
import { auth } from '../../../auth/user.auth/user.auth.handler';
import { UserPermissionAuth } from './user.permission.auth';

///////////////////////////////////////////////////////////////////////////////////////

export const register = (app: express.Application): void => {

    const router = express.Router();
    const controller = new UserPermissionController();

    router.post('/:userId/addon/:permissionId', auth(UserPermissionAuth.add), controller.add);
    router.delete('/:userId/addon/:permissionId', auth(UserPermissionAuth.remove), controller.remove);
    router.put('/:userId/addon/:permissionId/scope', auth(UserPermissionAuth.updateScope), controller.updateScope);
    router.put('/:userId/addon/:permissionId/grant', auth(UserPermissionAuth.grant), controller.grant);
    router.put('/:userId/addon/:permissionId/revoke', auth(UserPermissionAuth.revoke), controller.revoke);
    router.get('/:userId/addon/granted', auth(UserPermissionAuth.getGranted), controller.getGranted);
    router.get('/:userId/addon/revoked', auth(UserPermissionAuth.getRevoked), controller.getRevoked);
    router.get('/:userId/addon', auth(UserPermissionAuth.getAddon), controller.getAddonPermissions);
    router.get('/:userId/role-based', auth(UserPermissionAuth.getRoleBased), controller.getRoleBased);
    router.get('/:userId/all', auth(UserPermissionAuth.getAll), controller.getAll);

    app.use('/api/v1/user-permissions', router);
};
