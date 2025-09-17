import {
    UserAuthOptions,
    RequestType,
    ResourceOwnership,
    ActionScope,
    DefaultUserAuthOptions
} from '../../../auth/user.auth/auth.types';

///////////////////////////////////////////////////////////////////////////////////////

export class AuthAuth {

    static readonly _baseContext = 'User.Auth';

    static readonly loginWithEmailPassword: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context        : `${this._baseContext}.LoginWithEmailPassword`,
        Ownership      : ResourceOwnership.System,
        ActionScope    : ActionScope.Public,
        RequestType    : RequestType.UpdateOne,
        SignupOrSignin : true
    };

    static readonly loginWithPhonePassword: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context        : `${this._baseContext}.LoginWithPhonePassword`,
        Ownership      : ResourceOwnership.System,
        ActionScope    : ActionScope.Public,
        RequestType    : RequestType.UpdateOne,
        SignupOrSignin : true,
    };

    static readonly loginWithUsernamePassword: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context        : `${this._baseContext}.LoginWithUsernamePassword`,
        Ownership      : ResourceOwnership.System,
        ActionScope    : ActionScope.Public,
        RequestType    : RequestType.UpdateOne,
        SignupOrSignin : true,
    };

    static readonly loginWithEmailOtp: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context        : `${this._baseContext}.LoginWithEmailOtp`,
        Ownership      : ResourceOwnership.System,
        ActionScope    : ActionScope.Public,
        RequestType    : RequestType.UpdateOne,
        SignupOrSignin : true,
    };

    static readonly loginWithPhoneOtp: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context        : `${this._baseContext}.LoginWithPhoneOtp`,
        Ownership      : ResourceOwnership.System,
        ActionScope    : ActionScope.Public,
        RequestType    : RequestType.UpdateOne,
        SignupOrSignin : true,
    };

    static readonly loginWithOtp: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context        : `${this._baseContext}.LoginWithOtp`,
        Ownership      : ResourceOwnership.System,
        ActionScope    : ActionScope.Public,
        RequestType    : RequestType.UpdateOne,
        SignupOrSignin : true,
    };

    static readonly generateEmailOtp: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context        : `${this._baseContext}.GenerateEmailOtp`,
        Ownership      : ResourceOwnership.System,
        ActionScope    : ActionScope.Public,
        RequestType    : RequestType.UpdateOne,
        SignupOrSignin : true,
    };

    static readonly generatePhoneOtp: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context        : `${this._baseContext}.GeneratePhoneOtp`,
        Ownership      : ResourceOwnership.System,
        ActionScope    : ActionScope.Public,
        RequestType    : RequestType.UpdateOne,
        SignupOrSignin : true,
    };

    static readonly sendResetPasswordByEmailOtp: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context        : `${this._baseContext}.SendResetPasswordByEmailOtp`,
        Ownership      : ResourceOwnership.System,
        ActionScope    : ActionScope.Public,
        RequestType    : RequestType.UpdateOne,
        SignupOrSignin : true,
    };

    static readonly sendResetPasswordByPhoneOtp: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context        : `${this._baseContext}.SendResetPasswordByPhoneOtp`,
        Ownership      : ResourceOwnership.System,
        ActionScope    : ActionScope.Public,
        RequestType    : RequestType.UpdateOne,
        SignupOrSignin : true,
    };

    static readonly sendResetPasswordByEmailLink: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context        : `${this._baseContext}.SendResetPasswordByEmailLink`,
        Ownership      : ResourceOwnership.System,
        ActionScope    : ActionScope.Public,
        RequestType    : RequestType.UpdateOne,
        SignupOrSignin : true,
    };

    static readonly resetPasswordByToken: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context        : `${this._baseContext}.ResetPasswordByToken`,
        Ownership      : ResourceOwnership.System,
        ActionScope    : ActionScope.Public,
        RequestType    : RequestType.UpdateOne,
        SignupOrSignin : true,
    };

    static readonly resetPasswordByOtp: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context        : `${this._baseContext}.ResetPasswordByOtp`,
        Ownership      : ResourceOwnership.System,
        ActionScope    : ActionScope.Public,
        RequestType    : RequestType.UpdateOne,
        SignupOrSignin : true,
    };

    static readonly rotateUserAccessToken: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.RotateUserAccessToken`,
        Ownership   : ResourceOwnership.System,
        ActionScope : ActionScope.Public,
        RequestType : RequestType.UpdateOne,
    };

    static readonly logout: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.Logout`,
        Ownership   : ResourceOwnership.Owner,
        ActionScope : ActionScope.Owner,
        RequestType : RequestType.UpdateOne,
    };

    static readonly changePassword: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.ChangePassword`,
        Ownership   : ResourceOwnership.Owner,
        ActionScope : ActionScope.Owner,
        RequestType : RequestType.UpdateOne,
    };

    static readonly githubOAuthLogin: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context        : `${this._baseContext}.GithubOAuthLogin`,
        Ownership      : ResourceOwnership.System,
        ActionScope    : ActionScope.Public,
        RequestType    : RequestType.GetOne,
        SignupOrSignin : true,
    };

    static readonly githubOAuthCallback: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context        : `${this._baseContext}.GithubOAuthCallback`,
        Ownership      : ResourceOwnership.System,
        ActionScope    : ActionScope.Public,
        RequestType    : RequestType.UpdateOne,
        SignupOrSignin : true,
    };

    static readonly googleOAuthLogin: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context        : `${this._baseContext}.GoogleOAuthLogin`,
        Ownership      : ResourceOwnership.System,
        ActionScope    : ActionScope.Public,
        RequestType    : RequestType.GetOne,
        SignupOrSignin : true,
    };

    static readonly googleOAuthCallback: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context        : `${this._baseContext}.GoogleOAuthCallback`,
        Ownership      : ResourceOwnership.System,
        ActionScope    : ActionScope.Public,
        RequestType    : RequestType.UpdateOne,
        SignupOrSignin : true,
    };

}
