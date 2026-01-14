import 'reflect-metadata';
import { DependencyContainer } from 'tsyringe';
import { CommunicationInjector } from './communication/communication.injector';
import { FileStorageInjector } from './storage/file.storage.injector';

////////////////////////////////////////////////////////////////////////////////

export class ModuleInjector {

    static registerInjections(container: DependencyContainer) {
        CommunicationInjector.registerInjections(container);
        FileStorageInjector.registerInjections(container);
    }

}
