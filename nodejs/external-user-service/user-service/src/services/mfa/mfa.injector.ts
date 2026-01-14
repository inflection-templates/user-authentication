import { DependencyContainer } from 'tsyringe';
import { TotpService } from './totp.service';
import { MfaService } from './mfa.service';

////////////////////////////////////////////////////////////////////////

export class MfaInjector {

    public static registerInjections(container: DependencyContainer) {
        
        // Register TOTP Service as singleton
        container.registerSingleton('TotpService', TotpService);
        
        // Register MFA Service as singleton
        container.registerSingleton('MfaService', MfaService);
    }

}
