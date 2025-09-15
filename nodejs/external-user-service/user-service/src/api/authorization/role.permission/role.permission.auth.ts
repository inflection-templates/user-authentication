import {
    UserAuthOptions,
    RequestType,
    ResourceOwnership,
    ActionScope,
    DefaultUserAuthOptions
} from '../../../auth/user.auth/auth.types';

///////////////////////////////////////////////////////////////////////////////////////

export class RolePermissionAuth {

    static readonly _baseContext = 'Authorization.RolePermission';

    static readonly add: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.Add`,
        Ownership   : ResourceOwnership.System,
        ActionScope : ActionScope.System,
        RequestType : RequestType.UpdateOne,
    };

    static readonly get: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.Get`,
        Ownership   : ResourceOwnership.System,
        ActionScope : ActionScope.Public,
        RequestType : RequestType.GetMany,
    };

    static readonly getForTenant: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.GetForTenant`,
        Ownership   : ResourceOwnership.System,
        ActionScope : ActionScope.Public,
        RequestType : RequestType.GetMany,
    };

    static readonly getSystemRolePermissions: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.GetSystemRolePermissions`,
        Ownership   : ResourceOwnership.System,
        ActionScope : ActionScope.Public,
        RequestType : RequestType.GetMany,
    };

    static readonly remove: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.Remove`,
        Ownership   : ResourceOwnership.System,
        ActionScope : ActionScope.Public,
        RequestType : RequestType.UpdateOne,
    };

    static readonly enable: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.Enable`,
        Ownership   : ResourceOwnership.System,
        ActionScope : ActionScope.System,
        RequestType : RequestType.UpdateOne,
    };

    static readonly disable: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.Disable`,
        Ownership   : ResourceOwnership.System,
        ActionScope : ActionScope.System,
        RequestType : RequestType.UpdateOne,
    };

    static readonly updateScope: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.UpdateScope`,
        Ownership   : ResourceOwnership.System,
        ActionScope : ActionScope.System,
        RequestType : RequestType.UpdateOne,
    };

}
