import 'reflect-metadata';
import { DependencyContainer } from 'tsyringe';
import { ConfigurationManager } from '../../config/configuration.manager';
import { IApiKeyVerifier } from './interfaces/api.key.verifier.interface';
import { InternalApiKeyVerifier } from './verifiers/internal.api.key.verifier';
import { ExternalApiKeyVerifier } from './verifiers/external.api.key.verifier';

////////////////////////////////////////////////////////////////////////////////

export class ClientAppAuthInjector {

    static registerInjections(container: DependencyContainer) {

        const internalClientAppManagement = ConfigurationManager.InternalClientAppManagement;

        if (internalClientAppManagement) {
            // Use internal API key verification
            container.register<IApiKeyVerifier>('IApiKeyVerifier', InternalApiKeyVerifier);
        } else {
            // Use external API key verification
            container.register<IApiKeyVerifier>('IApiKeyVerifier', ExternalApiKeyVerifier);
        }
    }

}
