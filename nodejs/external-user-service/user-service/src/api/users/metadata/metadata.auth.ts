import {
    UserAuthOptions,
    RequestType,
    ResourceOwnership,
    ActionScope,
    DefaultUserAuthOptions
} from '../../../auth/user.auth/auth.types';

///////////////////////////////////////////////////////////////////////////////////////

export class UserMetadataAuth {

    static readonly _baseContext = 'User.Metadata';

    static readonly getUserMetadata: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context        : `${this._baseContext}.GetUserMetadata`,
        Ownership      : ResourceOwnership.System,
        ActionScope    : ActionScope.Tenant,
        RequestType    : RequestType.GetOne,
        SignupOrSignin : true
    };

    static readonly getCurrentTimeZone: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context        : `${this._baseContext}.GetCurrentTimeZone`,
        Ownership      : ResourceOwnership.System,
        ActionScope    : ActionScope.Tenant,
        RequestType    : RequestType.GetOne,
        SignupOrSignin : true,
    };

    static readonly getDefaultTimeZone: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context        : `${this._baseContext}.GetDefaultTimeZone`,
        Ownership      : ResourceOwnership.System,
        ActionScope    : ActionScope.Tenant,
        RequestType    : RequestType.GetOne,
        SignupOrSignin : true,
    };

    static readonly getPreferredLanguage: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context        : `${this._baseContext}.GetPreferredLanguage`,
        Ownership      : ResourceOwnership.System,
        ActionScope    : ActionScope.Tenant,
        RequestType    : RequestType.GetOne,
        SignupOrSignin : true,
    };

    static readonly getUserSettings: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context        : `${this._baseContext}.GetUserSettings`,
        Ownership      : ResourceOwnership.System,
        ActionScope    : ActionScope.Tenant,
        RequestType    : RequestType.GetOne,
        SignupOrSignin : true,
    };

    static readonly getDisplayName: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context        : `${this._baseContext}.GetDisplayName`,
        Ownership      : ResourceOwnership.System,
        ActionScope    : ActionScope.Tenant,
        RequestType    : RequestType.GetOne,
        SignupOrSignin : true,
    };

    static readonly getLastLogin: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context        : `${this._baseContext}.GetLastLogin`,
        Ownership      : ResourceOwnership.System,
        ActionScope    : ActionScope.Tenant,
        RequestType    : RequestType.GetOne,
        SignupOrSignin : true,
    };

    static readonly isTestUser: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context        : `${this._baseContext}.IsTestUser`,
        Ownership      : ResourceOwnership.System,
        ActionScope    : ActionScope.Tenant,
        RequestType    : RequestType.GetOne,
        SignupOrSignin : true,
    };

    static readonly updateCurrentTimeZone: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context        : `${this._baseContext}.UpdateCurrentTimeZone`,
        Ownership      : ResourceOwnership.System,
        ActionScope    : ActionScope.Owner,
        RequestType    : RequestType.UpdateOne,
        SignupOrSignin : true,
    };

    static readonly updateDefaultTimeZone: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context        : `${this._baseContext}.UpdateDefaultTimeZone`,
        Ownership      : ResourceOwnership.System,
        ActionScope    : ActionScope.Owner,
        RequestType    : RequestType.UpdateOne,
        SignupOrSignin : true,
    };

    static readonly updatePreferredLanguage: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context        : `${this._baseContext}.UpdatePreferredLanguage`,
        Ownership      : ResourceOwnership.System,
        ActionScope    : ActionScope.Owner,
        RequestType    : RequestType.UpdateOne,
        SignupOrSignin : true,
    };

    static readonly updateUserSettings: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context        : `${this._baseContext}.UpdateUserSettings`,
        Ownership      : ResourceOwnership.System,
        ActionScope    : ActionScope.Owner,
        RequestType    : RequestType.UpdateOne,
        SignupOrSignin : true,
    };

    static readonly update: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context        : `${this._baseContext}.Update`,
        Ownership      : ResourceOwnership.System,
        ActionScope    : ActionScope.Owner,
        RequestType    : RequestType.UpdateOne,
        SignupOrSignin : true,
    };

    static readonly delete: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.Delete`,
        Ownership   : ResourceOwnership.System,
        ActionScope : ActionScope.Owner,
        RequestType : RequestType.UpdateOne,
    };

}
