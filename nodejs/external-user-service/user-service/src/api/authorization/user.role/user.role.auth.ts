import {
    UserAuthOptions,
    RequestType,
    ResourceOwnership,
    ActionScope,
    DefaultUserAuthOptions
} from '../../../auth/user.auth/auth.types';

///////////////////////////////////////////////////////////////////////////////////////

export class UserRoleAuth {

    static readonly _baseContext = 'Authorization.UserRole';

    static readonly addUserRole: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.AddUserRole`,
        Ownership   : ResourceOwnership.System,
        ActionScope : ActionScope.System,
        RequestType : RequestType.UpdateOne,
    };

    static readonly getUserRoles: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.GetUserRoles`,
        Ownership   : ResourceOwnership.System,
        ActionScope : ActionScope.Public,
        RequestType : RequestType.GetMany,
    };

    static readonly removeUserRole: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.RemoveUserRole`,
        Ownership   : ResourceOwnership.System,
        ActionScope : ActionScope.Public,
        RequestType : RequestType.UpdateOne,
    };

    static readonly removeAllUserRoles: UserAuthOptions = {
        ...DefaultUserAuthOptions,
        Context     : `${this._baseContext}.RemoveAllUserRoles`,
        Ownership   : ResourceOwnership.System,
        ActionScope : ActionScope.Public,
        RequestType : RequestType.UpdateOne,
    };

}
