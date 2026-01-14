import express from 'express';
import { UserMetadataController } from './user.metadata.controller';
import { auth } from '../../../auth/user.auth/user.auth.handler';
import { UserMetadataAuth } from './metadata.auth';

////////////////////////////////////////////////////////////////////////////////

export const register = (app: express.Application): void => {

    const controller = new UserMetadataController();

    const router = express.Router();

    router.get('/:userId', auth(UserMetadataAuth.getUserMetadata), controller.getUserMetadata);
    router.get('/:userId/current-time-zone', auth(UserMetadataAuth.getCurrentTimeZone), controller.getCurrentTimeZone);
    router.get('/:userId/default-time-zone', auth(UserMetadataAuth.getDefaultTimeZone), controller.getDefaultTimeZone);
    router.get('/:userId/preferred-language', auth(UserMetadataAuth.getPreferredLanguage), controller.getPreferredLanguage);
    router.get('/:userId/user-settings', auth(UserMetadataAuth.getUserSettings), controller.getUserSettings);
    router.get('/:userId/display-name', auth(UserMetadataAuth.getDisplayName), controller.getDisplayName);
    router.get('/:userId/last-login', auth(UserMetadataAuth.getLastLogin), controller.getLastLogin);
    router.get('/:userId/is-test-user', auth(UserMetadataAuth.isTestUser), controller.isTestUser);

    router.put('/:userId/current-time-zone', auth(UserMetadataAuth.updateCurrentTimeZone), controller.updateCurrentTimeZone);
    router.put('/:userId/default-time-zone', auth(UserMetadataAuth.updateDefaultTimeZone), controller.updateDefaultTimeZone);
    router.put('/:userId/preferred-language', auth(UserMetadataAuth.updatePreferredLanguage), controller.updatePreferredLanguage);
    router.put('/:userId/user-settings', auth(UserMetadataAuth.updateUserSettings), controller.updateUserSettings);

    app.use('/api/v1/auth', router);

};
