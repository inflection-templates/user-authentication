import express from 'express';
import { PermissionController } from './permission.controller';
import { auth } from '../../../auth/user.auth/user.auth.handler';
import { PermissionAuth } from './permission.auth';

///////////////////////////////////////////////////////////////////////////////////////

export const register = (app: express.Application): void => {

    const router = express.Router();
    const controller = new PermissionController();

    router.post('/', auth(PermissionAuth.create), controller.create);
    router.get('/search', auth(PermissionAuth.search), controller.search);
    router.put('/:id', auth(PermissionAuth.update), controller.update);
    router.delete('/:id', auth(PermissionAuth.delete), controller.delete);
    router.get('/:id', auth(PermissionAuth.getById), controller.getById);

    app.use('/api/v1/permissions', router);
};
