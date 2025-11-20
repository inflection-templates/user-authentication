import 'reflect-metadata';
import { DependencyContainer } from 'tsyringe';
import { SimpleUserAuthorizer } from './custom/simple.user.authorizer';
import { CustomUserAuthenticator } from './custom/custom.user.authenticator';

////////////////////////////////////////////////////////////////////////////////

export class UserAuthInjector {

    static registerInjections(container: DependencyContainer) {
        container.register('IUserAuthenticator', CustomUserAuthenticator);
        container.register('IUserAuthorizer', SimpleUserAuthorizer);
    }

}
