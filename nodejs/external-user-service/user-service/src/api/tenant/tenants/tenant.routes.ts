import express from 'express';
import { TenantController } from './tenant.controller';
import { auth } from '../../../auth/user.auth/user.auth.handler';
import { TenantAuth } from './tenant.auth';

///////////////////////////////////////////////////////////////////////////////////////

export const register = (app: express.Application): void => {

    const router = express.Router();
    const controller = new TenantController();

    router.post('/', auth(TenantAuth.create), controller.create);
    router.get('/search', auth(TenantAuth.search), controller.search);
    router.put('/:id', auth(TenantAuth.update), controller.update);
    router.delete('/:id', auth(TenantAuth.delete), controller.delete);


    router.get('/by-code/:code', auth(TenantAuth.getByCode), controller.getByCode);
    router.get('/:id', auth(TenantAuth.getById), controller.getById);

    app.use('/api/v1/tenants', router);
};
