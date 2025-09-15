import {
    UserAuthOptions,
    RequestType,
    ResourceOwnership,
    ActionScope,
    DefaultUserAuthOptions } from '../../auth/user.auth/auth.types';

export class ClientAppAuth {

    static readonly _baseContext = `ClientApp`;

    static readonly create: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.Create`,
        RequestType : RequestType.CreateOne,
        ActionScope : ActionScope.System,
        Ownership   : ResourceOwnership.System,
    };

    static readonly update: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.Update`,
        ActionScope : ActionScope.System,
        Ownership   : ResourceOwnership.System,
        RequestType : RequestType.UpdateOne,
    };

    static readonly delete: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.Delete`,
        ActionScope : ActionScope.System,
        Ownership   : ResourceOwnership.System,
        RequestType : RequestType.DeleteOne,
    };

    static readonly search: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.Search`,
        ActionScope : ActionScope.System,
        Ownership   : ResourceOwnership.System,
        RequestType : RequestType.Search,
    };

    static readonly getById: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.GetById`,
        ActionScope : ActionScope.System,
        Ownership   : ResourceOwnership.System,
        RequestType : RequestType.GetOne,
    };

    static readonly getCurrentApiKey: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context             : `${this._baseContext}.GetCurrentApiKey`,
        ActionScope         : ActionScope.System,
        Ownership           : ResourceOwnership.System,
        RequestType         : RequestType.Custom,
        CustomAuthorization : false,
        AlternateAuth       : true,
    };

    static readonly renewApiKey: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context             : `${this._baseContext}.RenewApiKey`,
        ActionScope         : ActionScope.System,
        Ownership           : ResourceOwnership.System,
        RequestType         : RequestType.Custom,
        CustomAuthorization : false,
        AlternateAuth       : true,
    };

}
