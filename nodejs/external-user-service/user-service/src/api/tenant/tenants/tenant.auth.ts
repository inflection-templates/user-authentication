import {
    UserAuthOptions,
    RequestType,
    ResourceOwnership,
    ActionScope,
    DefaultUserAuthOptions
} from '../../../auth/user.auth/auth.types';

///////////////////////////////////////////////////////////////////////////////////////

export class TenantAuth {

    static readonly _baseContext = 'Tenant.Tenants';

    static readonly create: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.Create`,
        Ownership   : ResourceOwnership.Tenant,
        ActionScope : ActionScope.System,
        RequestType : RequestType.CreateOne,
    };

    static readonly search: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.Search`,
        Ownership   : ResourceOwnership.System,
        ActionScope : ActionScope.System,
        RequestType : RequestType.GetMany,
    };

    static readonly update: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.Update`,
        Ownership   : ResourceOwnership.Tenant,
        ActionScope : ActionScope.System,
        RequestType : RequestType.UpdateOne,
    };

    static readonly delete: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.Delete`,
        Ownership   : ResourceOwnership.Tenant,
        ActionScope : ActionScope.System,
        RequestType : RequestType.DeleteOne,
    };

    static readonly promoteTenantUserAsAdmin: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.PromoteTenantUserAsAdmin`,
        Ownership   : ResourceOwnership.Tenant,
        ActionScope : ActionScope.Tenant,
        RequestType : RequestType.UpdateOne,
    };

    static readonly demoteAdmin: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.DemoteAdmin`,
        Ownership   : ResourceOwnership.Tenant,
        ActionScope : ActionScope.Tenant,
        RequestType : RequestType.UpdateOne,
    };

    static readonly getTenantStats: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.GetTenantStats`,
        Ownership   : ResourceOwnership.Tenant,
        ActionScope : ActionScope.Tenant,
        RequestType : RequestType.GetOne,
    };

    static readonly getTenantAdmins: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.GetTenantAdmins`,
        Ownership   : ResourceOwnership.Tenant,
        ActionScope : ActionScope.Tenant,
        RequestType : RequestType.GetMany,
    };

    static readonly getTenantRegularUsers: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.GetTenantRegularUsers`,
        Ownership   : ResourceOwnership.Tenant,
        ActionScope : ActionScope.Tenant,
        RequestType : RequestType.GetMany,
    };

    static readonly getById: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.GetById`,
        Ownership   : ResourceOwnership.Tenant,
        ActionScope : ActionScope.System,
        RequestType : RequestType.GetOne,
    };

    static readonly getByCode: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.GetByCode`,
        Ownership   : ResourceOwnership.System,
        ActionScope : ActionScope.Public,
        RequestType : RequestType.GetOne,
    };
}
