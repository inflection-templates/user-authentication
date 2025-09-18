import {
    UserAuthOptions,
    RequestType,
    ResourceOwnership,
    ActionScope,
    DefaultUserAuthOptions
} from '../../auth/user.auth/auth.types';

///////////////////////////////////////////////////////////////////////////////////////

export class OAuthAuth {

    static readonly _baseContext = 'OAuth';

    //#region GitHub OAuth

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

    //#endregion

    //#region Google OAuth

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

    //#endregion

    //#region Facebook OAuth

    static readonly facebookOAuthLogin: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context        : `${this._baseContext}.FacebookOAuthLogin`,
        Ownership      : ResourceOwnership.System,
        ActionScope    : ActionScope.Public,
        RequestType    : RequestType.GetOne,
        SignupOrSignin : true,
    };

    static readonly facebookOAuthCallback: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context        : `${this._baseContext}.FacebookOAuthCallback`,
        Ownership      : ResourceOwnership.System,
        ActionScope    : ActionScope.Public,
        RequestType    : RequestType.UpdateOne,
        SignupOrSignin : true,
    };

    //#endregion

    //#region Twitter OAuth

    static readonly twitterOAuthLogin: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context        : `${this._baseContext}.TwitterOAuthLogin`,
        Ownership      : ResourceOwnership.System,
        ActionScope    : ActionScope.Public,
        RequestType    : RequestType.GetOne,
        SignupOrSignin : true,
    };

    static readonly twitterOAuthCallback: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context        : `${this._baseContext}.TwitterOAuthCallback`,
        Ownership      : ResourceOwnership.System,
        ActionScope    : ActionScope.Public,
        RequestType    : RequestType.UpdateOne,
        SignupOrSignin : true,
    };

    //#endregion
}
