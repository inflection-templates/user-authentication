import { ActionScope, ResourceOwnership, UserAuthOptions } from '../../../auth/user.auth/auth.types';

////////////////////////////////////////////////////////////////////////

export class MfaAuth {

    // MFA setup and management - requires authenticated user but allows same-service tokens
    public static readonly getMfaStatus: UserAuthOptions = {
        Context: 'Users.MFA.GetStatus',
        ActionScope: ActionScope.Owner,
        Ownership: ResourceOwnership.Owner,
        ClientAppAuth: false,
        OptionalUserAuth: false,
        AlternateAuth: false,
        SignupOrSignin: false
    };

    public static readonly setupTotp: UserAuthOptions = {
        Context: 'Users.MFA.SetupTotp',
        ActionScope: ActionScope.Owner,
        Ownership: ResourceOwnership.Owner,
        ClientAppAuth: false,
        OptionalUserAuth: false,
        AlternateAuth: false,
        SignupOrSignin: false
    };

    public static readonly verifyTotpSetup: UserAuthOptions = {
        Context: 'Users.MFA.VerifySetup',
        ActionScope: ActionScope.Owner,
        Ownership: ResourceOwnership.Owner,
        ClientAppAuth: false,
        OptionalUserAuth: false,
        AlternateAuth: false,
        SignupOrSignin: false
    };

    public static readonly validateMfa: UserAuthOptions = {
        Context: 'Users.MFA.Validate',
        ActionScope: ActionScope.Owner,
        Ownership: ResourceOwnership.Owner,
        ClientAppAuth: false,
        OptionalUserAuth: false,
        AlternateAuth: false,
        SignupOrSignin: false
    };

    public static readonly disableMfa: UserAuthOptions = {
        Context: 'Users.MFA.Disable',
        ActionScope: ActionScope.Owner,
        Ownership: ResourceOwnership.Owner,
        ClientAppAuth: false,
        OptionalUserAuth: false,
        AlternateAuth: false,
        SignupOrSignin: false
    };

    public static readonly generateBackupCodes: UserAuthOptions = {
        Context: 'Users.MFA.GenerateBackupCodes',
        ActionScope: ActionScope.Owner,
        Ownership: ResourceOwnership.Owner,
        ClientAppAuth: false,
        OptionalUserAuth: false,
        AlternateAuth: false,
        SignupOrSignin: false
    };

    public static readonly getQrCode: UserAuthOptions = {
        Context: 'Users.MFA.GetQrCode',
        ActionScope: ActionScope.Owner,
        Ownership: ResourceOwnership.Owner,
        ClientAppAuth: false,
        OptionalUserAuth: false,
        AlternateAuth: false,
        SignupOrSignin: false
    };

}
