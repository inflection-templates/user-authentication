import express from 'express';
import { UserController } from './user.controller';
import { auth } from '../../../auth/user.auth/user.auth.handler';
import { UserAuth } from './user.auth';

////////////////////////////////////////////////////////////////////////////////

export const register = (app: express.Application): void => {

    const controller = new UserController();

    const router = express.Router();

    router.get('/search', auth(UserAuth.search), controller.search);
    router.get('/:id', auth(UserAuth.getById), controller.getById);
    router.post('/', auth(UserAuth.create), controller.create);
    router.put('/:id/profile-image', auth(UserAuth.deleteProfileImage), controller.deleteProfileImage);
    router.put('/:id', auth(UserAuth.update), controller.update);
    router.delete('/:id', auth(UserAuth.delete), controller.delete);

    app.use('/api/v1/users', router);

};
