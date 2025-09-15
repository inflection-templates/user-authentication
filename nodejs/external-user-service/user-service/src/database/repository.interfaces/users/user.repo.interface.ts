import { uuid } from '../../../domain.types/miscellaneous/system.types';
import {
    UserCreateModel,
    UserUpdateModel,
    UserDto,
    UserSearchFilters,
    UserSearchResults,
} from '../../../domain.types/users/user.types';

////////////////////////////////////////////////////////////////////////////////////

export interface IUserRepo {

    getByUserName(userName: string): Promise<UserDto>;

    getByEmail(tenantId: uuid, email: string): Promise<UserDto>;

    getByPhone(tenantId: uuid, phoneCode: string, phoneNumber: string): Promise<UserDto>;

    create(model: UserCreateModel): Promise<UserDto>;

    getById(id: uuid): Promise<UserDto>;

    search(filters: UserSearchFilters): Promise<UserSearchResults>;

    delete(id: uuid): Promise<boolean>;

    update(id: uuid, model: UserUpdateModel): Promise<UserDto>;

    deleteProfileImageUrl(id: string): Promise<UserDto>;

}
