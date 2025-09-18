import {
    UserAuthOptions,
    RequestType,
    ResourceOwnership,
    ActionScope,
    DefaultUserAuthOptions
} from '../../../auth/user.auth/auth.types';

///////////////////////////////////////////////////////////////////////////////////////

export class RoleAuth {

    static readonly _baseContext = 'Authorization.Role';

    static readonly create: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.Create`,
        Ownership   : ResourceOwnership.System,
        ActionScope : ActionScope.System,
        RequestType : RequestType.CreateOne,
    };

    static readonly getById: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.GetById`,
        Ownership   : ResourceOwnership.System,
        ActionScope : ActionScope.Public,
        RequestType : RequestType.GetOne,
    };

    static readonly search: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.Search`,
        Ownership   : ResourceOwnership.System,
        ActionScope : ActionScope.Public,
        RequestType : RequestType.GetMany,
    };

    static readonly update: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.Update`,
        Ownership   : ResourceOwnership.System,
        ActionScope : ActionScope.System,
        RequestType : RequestType.UpdateOne,
    };

    static readonly delete: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.Delete`,
        Ownership   : ResourceOwnership.System,
        ActionScope : ActionScope.System,
        RequestType : RequestType.DeleteOne,
    };

}
