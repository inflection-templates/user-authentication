import ClientAppAuthMiddleware from './client.app.auth.middleware';
import { Middleware } from '../../domain.types/miscellaneous/middleware.types';

////////////////////////////////////////////////////////////////////////////////////////////////

export interface ClientAppAuthOptions {
    ClientAppAuth : boolean
}
////////////////////////////////////////////////////////////////////////////////////////////////

export class clientAppAuthAuthHandler {

    public static handle = (options: ClientAppAuthOptions): Middleware[] => {

        var middlewares: Middleware[] = [];

        // Client app authentication could be turned off for certain endpoints. e.g. public file downloads, etc.
        if (options.ClientAppAuth === true) {
            middlewares.push(ClientAppAuthMiddleware.authenticateClient);
        }

        return middlewares;
    };

}
