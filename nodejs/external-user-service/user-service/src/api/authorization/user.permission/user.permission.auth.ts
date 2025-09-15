import {
    UserAuthOptions,
    RequestType,
    ResourceOwnership,
    ActionScope,
    DefaultUserAuthOptions
} from '../../../auth/user.auth/auth.types';

///////////////////////////////////////////////////////////////////////////////////////

export class UserPermissionAuth {

    static readonly _baseContext = 'Authorization.UserPermission';

    static readonly add: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.Addon.Add`,
        Ownership   : ResourceOwnership.System,
        ActionScope : ActionScope.System,
        RequestType : RequestType.UpdateOne,
    };

    static readonly remove: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.Addon.Remove`,
        Ownership   : ResourceOwnership.System,
        ActionScope : ActionScope.Public,
        RequestType : RequestType.UpdateOne,
    };

    static readonly grant: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.Addon.Grant`,
        Ownership   : ResourceOwnership.System,
        ActionScope : ActionScope.System,
        RequestType : RequestType.UpdateOne,
    };

    static readonly revoke: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.Addon.Revoke`,
        Ownership   : ResourceOwnership.System,
        ActionScope : ActionScope.System,
        RequestType : RequestType.UpdateOne,
    };

    static readonly updateScope: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.Addon.UpdateScope`,
        Ownership   : ResourceOwnership.System,
        ActionScope : ActionScope.System,
        RequestType : RequestType.UpdateOne,
    };

    static readonly getGranted: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.Addon.GetGranted`,
        Ownership   : ResourceOwnership.System,
        ActionScope : ActionScope.Public,
        RequestType : RequestType.GetMany,
    };

    static readonly getRevoked: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.Addon.GetRevoked`,
        Ownership   : ResourceOwnership.System,
        ActionScope : ActionScope.Public,
        RequestType : RequestType.GetMany,
    };

    static readonly getAddon: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.Addon.Get`,
        Ownership   : ResourceOwnership.System,
        ActionScope : ActionScope.Public,
        RequestType : RequestType.GetMany,
    };

    static readonly getRoleBased: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.RoleBased.Get`,
        Ownership   : ResourceOwnership.System,
        ActionScope : ActionScope.Public,
        RequestType : RequestType.GetMany,
    };

    static readonly getAll: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.All.Get`,
        Ownership   : ResourceOwnership.System,
        ActionScope : ActionScope.Public,
        RequestType : RequestType.GetMany,
    };

}
