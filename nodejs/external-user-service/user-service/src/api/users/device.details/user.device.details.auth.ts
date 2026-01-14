import {
    UserAuthOptions,
    RequestType,
    ResourceOwnership,
    ActionScope,
    DefaultUserAuthOptions
} from '../../../auth/user.auth/auth.types';

///////////////////////////////////////////////////////////////////////////////////////

export class UserDeviceDetailsAuth {

    static readonly _baseContext = 'User.DeviceDetails';

    static readonly create: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.Create`,
        Ownership   : ResourceOwnership.Owner,
        ActionScope : ActionScope.Public,
        RequestType : RequestType.CreateOne,
    };

    static readonly sendTestNotification: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.SendTestNotification`,
        Ownership   : ResourceOwnership.System,
        ActionScope : ActionScope.Public,
        RequestType : RequestType.UpdateOne,
    };

    static readonly sendNotificationWithTopic: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.SendNotificationWithTopic`,
        Ownership   : ResourceOwnership.System,
        ActionScope : ActionScope.Public,
        RequestType : RequestType.UpdateOne,
    };

    static readonly search: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.Search`,
        Ownership   : ResourceOwnership.System,
        ActionScope : ActionScope.Public,
        RequestType : RequestType.Search,
    };

    static readonly getByUserId: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context        : `${this._baseContext}.GetByUserId`,
        Ownership      : ResourceOwnership.Owner,
        ActionScope    : ActionScope.Tenant,
        RequestType    : RequestType.GetOne,
        ResourceIdName : 'userId'
    };

    static readonly getById: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context        : `${this._baseContext}.GetById`,
        Ownership      : ResourceOwnership.Owner,
        ActionScope    : ActionScope.Tenant,
        RequestType    : RequestType.GetOne,
        ResourceIdName : 'id'
    };

    static readonly update: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.Update`,
        Ownership   : ResourceOwnership.Owner,
        ActionScope : ActionScope.Owner,
        RequestType : RequestType.UpdateOne,
    };

    static readonly delete: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.Delete`,
        Ownership   : ResourceOwnership.Owner,
        ActionScope : ActionScope.Owner,
        RequestType : RequestType.DeleteOne,
    };

}
