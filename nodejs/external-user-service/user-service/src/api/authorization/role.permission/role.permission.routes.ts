import express from 'express';
import { RolePermissionController } from './role.permission.controller';
import { auth } from '../../../auth/user.auth/user.auth.handler';
import { RolePermissionAuth } from './role.permission.auth';

///////////////////////////////////////////////////////////////////////////////////////

export const register = (app: express.Application): void => {

    const router = express.Router();
    const controller = new RolePermissionController();

    router.post('/:roleId/permissions/:permissionId', auth(RolePermissionAuth.add), controller.addPermission);
    router.delete('/:roleId/permissions/:permissionId', auth(RolePermissionAuth.remove), controller.removePermission);
    router.put('/:roleId/permissions/:permissionId/scope', auth(RolePermissionAuth.updateScope), controller.updatePermissionScope);
    router.put('/:roleId/permissions/:permissionId/enable', auth(RolePermissionAuth.enable), controller.enablePermission);
    router.put('/:roleId/permissions/:permissionId/disable', auth(RolePermissionAuth.disable), controller.disablePermission);
    router.get('/tenants/:tenantId', auth(RolePermissionAuth.getForTenant), controller.getForTenant);
    router.get('/system-role-permissions', auth(RolePermissionAuth.getSystemRolePermissions), controller.getSystemRolePermissions);
    router.get('/:roleId/permissions', auth(RolePermissionAuth.get), controller.getPermissions);

    app.use('/api/v1/role-permissions', router);
};
