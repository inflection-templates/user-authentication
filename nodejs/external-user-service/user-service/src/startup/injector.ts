import 'reflect-metadata';
import { ModuleInjector } from '../modules/module.injector';
import { DependencyContainer, container } from 'tsyringe';
import { UserAuthInjector } from '../auth/user.auth/user.auth.injector';
import { DatabaseInjector } from '../database/database.injector';
import { ClientAppAuthInjector } from '../auth/client.app.auth/client.app.auth.injector';
import { MfaInjector } from '../services/mfa/mfa.injector';

//////////////////////////////////////////////////////////////////////////////////////////////////

export class Injector {

    private static _container: DependencyContainer = container;

    public static get Container() {
        return Injector._container;
    }

    static registerInjections() {
        ClientAppAuthInjector.registerInjections(Injector.Container);
        UserAuthInjector.registerInjections(Injector.Container);
        DatabaseInjector.registerInjections(Injector.Container);
        MfaInjector.registerInjections(Injector.Container);
        ModuleInjector.registerInjections(Injector.Container);
    }

}
